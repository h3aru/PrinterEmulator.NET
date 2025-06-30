using System;
using System.Collections.Generic;
using EscPosEmulator.Emulator.Enums;

namespace EscPosEmulator.Emulator;

/// <summary>
/// 용지 설정 클래스
/// </summary>
public class PaperConfiguration
{
    private const double MmToInch = 0.0393701;

    public static PaperConfiguration Default => new();

    public double DotsPerInch = 180;           // 인치당 도트 수
    public double PaperWidthMm = 76;           // 용지 너비 (mm)
    public double PrintWidthMm = 72;           // 인쇄 너비 (mm) - 일반적인 80mm 영수증 프린터 기준
    public int DefaultLineSpacing = 10;        // 기본 라인 간격

    public Dictionary<PrinterFont, FontConfiguration> _printerFonts = new()
    {
        {PrinterFont.FontA, new FontConfiguration(PrinterFont.FontA, 12, 24, "D2Coding")},
        {PrinterFont.FontB, new FontConfiguration(PrinterFont.FontB, 12, 24, "D2Coding")}
    };

    /// <summary>
    /// 지정된 폰트 설정을 가져옵니다
    /// </summary>
    /// <param name="printerFont">프린터 폰트</param>
    /// <returns>폰트 설정</returns>
    public FontConfiguration GetFont(PrinterFont printerFont)
    {
        if (_printerFonts.ContainsKey(printerFont))
            return _printerFonts[printerFont];

        if (printerFont != PrinterFont.FontA)
            return GetFont(PrinterFont.FontA);

        throw new InvalidOperationException($"Required font is missing from paper config: {printerFont}");
    }

    /// <summary>
    /// 용지 너비를 인치 단위로 반환합니다
    /// </summary>
    public double GetPaperWidthInInches() => PaperWidthMm * MmToInch;
    
    /// <summary>
    /// 인쇄 너비를 인치 단위로 반환합니다
    /// </summary>
    public double GetPrintWidthInInches() => PrintWidthMm * MmToInch;

    /// <summary>
    /// 용지 너비를 픽셀 단위로 반환합니다
    /// </summary>
    public int GetPaperWidthInPixels() => (int)Math.Ceiling(GetPaperWidthInInches() * DotsPerInch);
    
    /// <summary>
    /// 인쇄 너비를 픽셀 단위로 반환합니다
    /// </summary>
    public int GetPrintWidthInPixels() => (int)Math.Ceiling(GetPrintWidthInInches() * DotsPerInch);

    /// <summary>
    /// 폰트 설정 클래스
    /// </summary>
    public class FontConfiguration
    {
        public PrinterFont PrinterFont;        // 프린터 폰트
        public int CharacterWidth;             // 문자 너비
        public int CharacterHeight;            // 문자 높이
        public string RenderFont;              // 렌더링 폰트

        public FontConfiguration(PrinterFont printerFont, int characterWidth, int characterHeight, string renderFont)
        {
            PrinterFont = printerFont;
            CharacterWidth = characterWidth;
            CharacterHeight = characterHeight;
            RenderFont = renderFont;
        }
    }
} 