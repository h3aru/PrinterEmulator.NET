using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EscPosEmulator.Emulator.Abstraction;
using EscPosEmulator.Emulator.Printables;

namespace EscPosEmulator.Emulator;

/// <summary>
/// 영수증 클래스
/// </summary>
public class Receipt
{
    private readonly PaperConfiguration _paperConfiguration;

    public readonly string Guid;
    
    private int PaperWidth => _paperConfiguration.GetPaperWidthInPixels();
    private int PrintWidth => _paperConfiguration.GetPrintWidthInPixels();
    private int PaperMargins => (PaperWidth - PrintWidth) / 2;

    private PrintMode _printMode;
    private List<IReceiptPrintable> _renderLines;
    private ReceiptTextLine? _currentTextLine;
    private int _lineSpacing;

    /// <summary>
    /// 영수증이 비어있는지 확인합니다
    /// </summary>
    public bool IsEmpty => (_currentTextLine == null || _currentTextLine.IsEmpty) && _renderLines.Count == 0;

    public Receipt(PaperConfiguration paperConfiguration, PrintMode printMode, int lineSpacing)
    {
        Guid = System.Guid.NewGuid().ToString();
        
        _paperConfiguration = paperConfiguration;

        _printMode = printMode;
        _renderLines = new();
        _currentTextLine = null;
        _lineSpacing = lineSpacing;
    }

    /// <summary>
    /// 폰트 설정을 변경합니다
    /// </summary>
    /// <param name="printMode">새로운 인쇄 모드</param>
    public void ChangeFontConfiguration(PrintMode printMode)
    {
        FinalizeTextLine(false);

        _printMode = printMode.Clone();
    }

    /// <summary>
    /// 라인 간격을 설정합니다
    /// </summary>
    /// <param name="value">라인 간격 값</param>
    public void SetLineSpacing(int value)
    {
        _lineSpacing = value;
    }

    /// <summary>
    /// 새로운 텍스트 라인을 생성합니다
    /// </summary>
    /// <returns>새로운 텍스트 라인</returns>
    private ReceiptTextLine CreateNewTextLine() => new(_paperConfiguration, _printMode);
    
    /// <summary>
    /// 텍스트를 인쇄합니다
    /// </summary>
    /// <param name="text">인쇄할 텍스트</param>
    public void PrintText(string text)
    {
        if (_currentTextLine is null)
            _currentTextLine = CreateNewTextLine();

        for (var i = 0; i < text.Length; i++)
        {
            var canContinue = _currentTextLine.TryWriteChar(text[i]);

            if (!canContinue)
            {
                FinalizeTextLine(false);

                _currentTextLine = CreateNewTextLine();
                canContinue = _currentTextLine.TryWriteChar(text[i]);

                if (!canContinue)
                    throw new Exception("Logic error - line must be able to contain > 0 chars");
            }
        }
    }

    /// <summary>
    /// 현재 텍스트 라인을 완료합니다
    /// </summary>
    /// <param name="insertLineSpacing">라인 간격 삽입 여부</param>
    public void FinalizeTextLine(bool insertLineSpacing)
    {
        if (_currentTextLine != null)
        {
            if (!_currentTextLine.IsEmpty)
                _renderLines.Add(_currentTextLine);
            _currentTextLine = null;
        }

        if (insertLineSpacing)
        {
            _renderLines.Add(new ReceiptEmptyLine(_lineSpacing));
        }
    }

    /// <summary>
    /// 새 라인으로 이동합니다
    /// </summary>
    public void AdvanceToNewLine() => FinalizeTextLine(true);

    /// <summary>
    /// 총 인쇄 높이를 반환합니다
    /// </summary>
    /// <returns>총 인쇄 높이</returns>
    public int GetTotalPrintHeight()
        => _renderLines.Sum(line => line.GetPrintHeight());

    /// <summary>
    /// 총 용지 높이를 반환합니다
    /// </summary>
    /// <returns>총 용지 높이</returns>
    public int GetTotalPaperHeight() =>
        GetTotalPrintHeight() + (PaperMargins * 2);

    /// <summary>
    /// 영수증을 비트맵으로 렌더링합니다
    /// </summary>
    /// <param name="drawPartials">부분 그리기 여부</param>
    /// <returns>렌더링된 비트맵</returns>
    public Bitmap Render(bool drawPartials = true)
    {
        var paperWidth = PaperWidth;
        var paperHeight = GetTotalPaperHeight();
        
        var bmp = new Bitmap(paperWidth, paperHeight);
        using var g = Graphics.FromImage(bmp);
        
        // 흰색 배경 채우기
        g.FillRectangle(Brushes.White, 0, 0, paperWidth, paperHeight);
        
        // 모든 렌더링된 라인 그리기
        var offsetX = PaperMargins;
        var offsetY = PaperMargins;

        foreach (var line in _renderLines)
        {
            line.Render(bmp, g, offsetX, offsetY);
            offsetY += line.GetPrintHeight();
        }

        return bmp;
    }
} 