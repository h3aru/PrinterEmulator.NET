using System.Drawing;
using EscPosEmulator.Emulator.Abstraction;
using EscPosEmulator.Emulator.Enums;
using System.Text;
using System.Collections.Generic;

namespace EscPosEmulator.Emulator.Printables;

/// <summary>
/// 영수증 텍스트 라인 클래스
/// </summary>
public class ReceiptTextLine : IReceiptPrintable
{
    private readonly PaperConfiguration.FontConfiguration _font;
    private readonly int _printWidth;
    private readonly int _charWidth;
    private readonly int _charHeight;
    private readonly TextJustification _justification;
    private readonly bool _bold;
    private readonly bool _italic;
    private readonly UnderlineMode _underline;

    private string _text;
    private int _totalWidth;

    // 한 줄에 들어갈 수 있는 최대 바이트 수 (실제 프린터 기준)
    private const int MaxLineBytes = 42; // 필요시 조정
    // 실제 프린터와 유사하게 고정폭 폰트 사용 (D2Coding)
    private readonly string _fontName = "D2Coding"; // 한글/영문 완전 고정폭 폰트
    private readonly int _baseFontSize = 8; // 폰트 크기(실제 영수증 느낌)

    /// <summary>
    /// 텍스트가 비어있는지 확인합니다
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(_text);
    
    public ReceiptTextLine(PaperConfiguration paperConfiguration, PrintMode printMode)
    {
        _font = paperConfiguration.GetFont(printMode.Font);
        _printWidth = paperConfiguration.GetPrintWidthInPixels();
        _charWidth = Math.Max(1, _font.CharacterWidth * Math.Max(1, printMode.CharWidthScale));
        _charHeight = Math.Max(1, _font.CharacterHeight * Math.Max(1, printMode.CharHeightScale));
        _justification = printMode.Justification;
        _bold = printMode.Emphasize;
        _italic = printMode.Italic;
        _underline = printMode.Underline;

        _text = "";
        _totalWidth = 0;
    }

    /// <summary>
    /// 문자를 추가하려고 시도합니다
    /// </summary>
    /// <param name="c">추가할 문자</param>
    /// <returns>추가 성공 여부</returns>
    public bool TryWriteChar(char c)
    {
        // 한글 2칸, 영문/숫자/공백 1칸 기준으로 문자 너비 계산
        int charVisualWidth = (c >= 0xAC00 && c <= 0xD7A3) ? _charWidth * 2 : _charWidth;
        
        if ((_totalWidth + charVisualWidth) >= _printWidth)
            return false;

        _text += c;
        _totalWidth += charVisualWidth;
        return true;
    }

    /// <summary>
    /// 출력 높이를 반환합니다
    /// </summary>
    /// <returns>출력 높이</returns>
    public int GetPrintHeight()
    {
        return _charHeight;
    }
    
    /// <summary>
    /// 비트맵에 렌더링합니다
    /// </summary>
    /// <param name="bitmap">대상 비트맵</param>
    /// <param name="g">그래픽 객체</param>
    /// <param name="offsetX">X 오프셋</param>
    /// <param name="offsetY">Y 오프셋</param>
    public void Render(Bitmap bitmap, Graphics g, int offsetX, int offsetY)
    {
        var fontStyle = FontStyle.Regular;
        if (_bold) fontStyle |= FontStyle.Bold;
        if (_italic) fontStyle |= FontStyle.Italic;
        
        // 고정폭 폰트와 실제 폰트 크기 적용
        var font = new Font(_fontName, _baseFontSize * _charHeight / 12, fontStyle);
        var lines = SplitByVisualLength(_text, MaxLineBytes);
        int y = offsetY;
        foreach (var line in lines)
        {
            // 정렬 방식에 따른 시작 X 위치 계산
            int lineVisualLen = GetTextVisualLength(line);
            int startX = offsetX;
            if (_justification == TextJustification.Center)
            {
                startX = offsetX + (_printWidth / 2) - (lineVisualLen * _charWidth / 2);
            }
            else if (_justification == TextJustification.Right)
            {
                startX = offsetX + (_printWidth - (lineVisualLen * _charWidth));
            }
            var rect = new Rectangle(
                x: startX,
                y: y,
                width: _printWidth,
                height: _charHeight
            );
            g.DrawString(line, font, Brushes.Black, rect);
            // 밑줄 그리기
            if (_underline is UnderlineMode.OnOneDot or UnderlineMode.OnTwoDots)
            {
                var dotHeight = (_underline is UnderlineMode.OnTwoDots ? 2 : 1);
                g.DrawLine(new Pen(Color.Black, dotHeight), rect.Left, rect.Bottom, rect.Right, rect.Bottom);
            }
            y += _charHeight;
        }
    }

    // 한글/영문 너비 계산
    private int GetTextVisualLength(string text)
    {
        int len = 0;
        foreach (char c in text)
            len += (c >= 0xAC00 && c <= 0xD7A3) ? 2 : 1;
        return len;
    }

    private List<string> SplitByVisualLength(string text, int maxLen)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        int currLen = 0;
        foreach (char c in text)
        {
            int charLen = (c >= 0xAC00 && c <= 0xD7A3) ? 2 : 1;
            if (currLen + charLen > maxLen)
            {
                result.Add(sb.ToString());
                sb.Clear();
                currLen = 0;
            }
            sb.Append(c);
            currLen += charLen;
        }
        if (sb.Length > 0) result.Add(sb.ToString());
        return result;
    }
} 