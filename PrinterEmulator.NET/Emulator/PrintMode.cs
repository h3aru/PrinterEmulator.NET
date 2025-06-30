using EscPosEmulator.Emulator.Enums;

namespace EscPosEmulator.Emulator;

/// <summary>
/// 인쇄 모드 클래스
/// </summary>
public class PrintMode
{
    public PrinterFont Font;                   // 폰트
    public int CharWidthScale;                 // 문자 너비 배율
    public int CharHeightScale;                // 문자 높이 배율
    public TextJustification Justification;    // 정렬 방식
    public bool Emphasize;                     // 강조 모드
    public bool Italic;                        // 이탤릭 모드
    public UnderlineMode Underline;            // 밑줄 모드

    public PrintMode()
    {
        Initialize();
    }

    /// <summary>
    /// 현재 인쇄 모드를 복사합니다
    /// </summary>
    /// <returns>복사된 인쇄 모드</returns>
    public PrintMode Clone()
    {
        return (PrintMode)MemberwiseClone();
    }
    
    /// <summary>
    /// 인쇄 모드를 초기화합니다
    /// </summary>
    public void Initialize()
    {
        Font = PrinterFont.FontA;
        CharWidthScale = 1;
        CharHeightScale = 1;
        Justification = TextJustification.Left;
        Emphasize = false;
        Italic = false;
        Underline = UnderlineMode.Off;
    }
} 