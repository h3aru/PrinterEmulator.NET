using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EscPosEmulator.Emulator.Enums;
using EscPosEmulator.Logging;

namespace EscPosEmulator.Emulator;

/// <summary>
/// 영수증 프린터 클래스
/// </summary>
public class ReceiptPrinter
{
    private readonly PaperConfiguration _paperConfiguration;

    private PrintMode _printMode;
    private int _lineSpacing;
    
    public Receipt? CurrentReceipt { get; private set; }
    public List<Receipt> ReceiptStack { get; private set; }

    public event EventHandler<EventArgs> OnActivityEvent = (sender, e) => { };

    public ReceiptPrinter(PaperConfiguration paperConfiguration)
    {
        _paperConfiguration = paperConfiguration;

        _printMode = new PrintMode();

        ReceiptStack = new();

        StartNewReceipt();
        
        PowerCycle();
    }

    #region ESC/POS 명령어 처리

    /// <summary>
    /// ESC/POS 명령어를 처리합니다
    /// </summary>
    /// <param name="ascii">ESC/POS 명령어 문자열</param>
    public void FeedEscPos(string ascii)
    {
        File.WriteAllText("last_escpos_receive.txt", ascii, Encoding.ASCII);

        try
        {
            InterpretEscPos(ascii);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "ESC/POS Interpreter Error");
        }

        OnActivityEvent?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// ESC/POS 명령어를 해석합니다
    /// </summary>
    /// <param name="ascii">ESC/POS 명령어 문자열</param>
    private void InterpretEscPos(string ascii)
    {
        for (int i = 0; i < ascii.Length; i++)
        {
            char currentChar = ascii[i];

            // 라인 피드 처리 (LF, CR 모두 처리)
            if (currentChar == '\n' || currentChar == '\r')
            {
                if (currentChar == '\n' || (currentChar == '\r' && (i + 1 >= ascii.Length || ascii[i + 1] != '\n')))
                {
                    PrintAndLineFeed(GetPrintBuffer());
                    ClearPrintBuffer();
                }
                continue;
            }

            // ESC 명령어 처리
            if (currentChar == (char)27) // ESC
            {
                if (i + 1 < ascii.Length)
                {
                    char nextChar = ascii[i + 1];
                    switch (nextChar)
                    {
                        case '@': // 프린터 초기화
                            Initialize();
                            i++;
                            break;
                        case 'E': // 볼드 모드
                            if (i + 2 < ascii.Length)
                            {
                                char boldValue = ascii[i + 2];
                                SelectEmphasizeMode(boldValue == (char)1);
                                i += 2;
                            }
                            break;
                        case '-': // 언더라인 모드
                            if (i + 2 < ascii.Length)
                            {
                                char underlineValue = ascii[i + 2];
                                SelectUnderlineMode(underlineValue == (char)1 ? UnderlineMode.OnOneDot : UnderlineMode.Off);
                                i += 2;
                            }
                            break;
                        case 'a': // 정렬
                            if (i + 2 < ascii.Length)
                            {
                                char alignValue = ascii[i + 2];
                                switch (alignValue)
                                {
                                    case (char)0:
                                        SelectJustification(TextJustification.Left);
                                        break;
                                    case (char)1:
                                        SelectJustification(TextJustification.Center);
                                        break;
                                    case (char)2:
                                        SelectJustification(TextJustification.Right);
                                        break;
                                }
                                i += 2;
                            }
                            break;
                        case 'd': // 라인 피드
                            if (i + 2 < ascii.Length)
                            {
                                char lineFeedValue = ascii[i + 2];
                                for (int j = 0; j < lineFeedValue; j++)
                                {
                                    LineFeed();
                                }
                                i += 2;
                            }
                            break;
                        case 'i': // 커트 (MPrinterHelper.Cut = ESC + "i")
                            Cut();
                            i++;
                            break;
                    }
                }
                continue;
            }

            // GS 명령어 처리
            if (currentChar == (char)29) // GS
            {
                if (i + 1 < ascii.Length)
                {
                    char nextChar = ascii[i + 1];
                    switch (nextChar)
                    {
                        case 'B': // 흑백반전
                            if (i + 2 < ascii.Length)
                            {
                                char reverseValue = ascii[i + 2];
                                // 흑백반전 기능 구현 (현재는 무시)
                                i += 2;
                            }
                            break;
                        case '!': // 문자 크기 (MPrinterHelper.ConvertFontSize와 동일한 로직)
                            if (i + 2 < ascii.Length)
                            {
                                char sizeValue = ascii[i + 2];
                                // MPrinterHelper.ConvertFontSize 로직 적용
                                int width = (sizeValue & 0xF0) >> 4;
                                int height = sizeValue & 0x0F;
                                
                                // MPrinterHelper와 동일한 변환 로직
                                int _w = 0, _h = 0;
                                
                                // 가로변환 (MPrinterHelper와 동일)
                                if (width == 1) _w = 0;
                                else if (width == 2) _w = 16;
                                else if (width == 3) _w = 32;
                                else if (width == 4) _w = 48;
                                else if (width == 5) _w = 64;
                                else if (width == 6) _w = 80;
                                else if (width == 7) _w = 96;
                                else if (width == 8) _w = 112;
                                else _w = 0;

                                // 세로변환 (MPrinterHelper와 동일)
                                if (height == 1) _h = 0;
                                else if (height == 2) _h = 1;
                                else if (height == 3) _h = 2;
                                else if (height == 4) _h = 3;
                                else if (height == 5) _h = 4;
                                else if (height == 6) _h = 5;
                                else if (height == 7) _h = 6;
                                else if (height == 8) _h = 7;
                                else _h = 0;
                                
                                SelectCharacterSize(_w == 0 ? 1 : _w / 16 + 1, _h + 1);
                                i += 2;
                            }
                            break;
                        case 'f': // 폰트 선택
                            if (i + 2 < ascii.Length)
                            {
                                char fontValue = ascii[i + 2];
                                SelectFont((PrinterFont)fontValue);
                                i += 2;
                            }
                            break;
                    }
                }
                continue;
            }

            // 일반 문자 처리
            if (currentChar != (char)0) // NUL 문자 무시
            {
                AppendToPrintBuffer(currentChar);
            }
        }
    }

    // FeedEscPosBytes: byte[] 기반 ESC/POS 해석 (명령어와 텍스트 구분)
    public void FeedEscPosBytes(byte[] bytes)
    {
        var textBuffer = new List<byte>();
        int i = 0;
        while (i < bytes.Length)
        {
            byte b = bytes[i];

            // 명령어 시작(ESC, GS 등)이면, 그 전까지 쌓인 텍스트를 먼저 출력
            if (b == 0x1B || b == 0x1D)
            {
                FlushTextBuffer(textBuffer, false);
                if (b == 0x1B) i = HandleEscCommand(bytes, i);
                else i = HandleGsCommand(bytes, i);
                continue;
            }
            // 줄바꿈 명령어도 명령어처럼 취급
            else if (b == 0x0A || b == 0x0D)
            {
                FlushTextBuffer(textBuffer, true); // 줄바꿈 포함
                i++;
                continue;
            }
            else if (b == 0x00)
            {
                i++;
                continue;
            }
            else
            {
                textBuffer.Add(b);
                i++;
            }
        }
        FlushTextBuffer(textBuffer, false);
        OnActivityEvent?.Invoke(this, EventArgs.Empty);
    }

    // 텍스트 버퍼를 상황에 따라 PrintText 또는 PrintAndLineFeed로 출력
    private void FlushTextBuffer(List<byte> textBuffer, bool withLineFeed)
    {
        if (textBuffer.Count > 0)
        {
            string text = Encoding.GetEncoding(949).GetString(textBuffer.ToArray());
            if (withLineFeed)
                PrintAndLineFeed(text);
            else
                PrintText(text);
            textBuffer.Clear();
        }
        else if (withLineFeed)
        {
            // 텍스트가 없더라도 명령어상 줄바꿈만 있을 때 빈 줄 추가
            LineFeed();
        }
    }

    // ESC 명령어 해석 (bytes, index) → 다음 index 반환
    private int HandleEscCommand(byte[] bytes, int i)
    {
        if (i + 1 >= bytes.Length) return i + 1;
        byte cmd = bytes[i + 1];
        switch (cmd)
        {
            case (byte)'@': // 초기화
                Initialize();
                return i + 2;
            case (byte)'E': // 볼드
                if (i + 2 < bytes.Length)
                {
                    SelectEmphasizeMode(bytes[i + 2] == 1);
                    return i + 3;
                }
                break;
            case (byte)'-': // 언더라인
                if (i + 2 < bytes.Length)
                {
                    SelectUnderlineMode(bytes[i + 2] == 1 ? UnderlineMode.OnOneDot : UnderlineMode.Off);
                    return i + 3;
                }
                break;
            case (byte)'a': // 정렬
                if (i + 2 < bytes.Length)
                {
                    switch (bytes[i + 2])
                    {
                        case 0: SelectJustification(TextJustification.Left); break;
                        case 1: SelectJustification(TextJustification.Center); break;
                        case 2: SelectJustification(TextJustification.Right); break;
                    }
                    return i + 3;
                }
                break;
            case (byte)'d': // 라인피드
                if (i + 2 < bytes.Length)
                {
                    int count = bytes[i + 2];
                    for (int j = 0; j < count; j++) LineFeed();
                    return i + 3;
                }
                break;
            case (byte)'i': // 커트
                Cut();
                return i + 2;
        }
        return i + 1;
    }

    // GS 명령어 해석 (bytes, index) → 다음 index 반환
    private int HandleGsCommand(byte[] bytes, int i)
    {
        if (i + 1 >= bytes.Length) return i + 1;
        byte cmd = bytes[i + 1];
        switch (cmd)
        {
            case (byte)'B': // 흑백반전 (무시)
                return i + 3;
            case (byte)'!': // 문자 크기
                if (i + 2 < bytes.Length)
                {
                    byte sizeValue = bytes[i + 2];
                    int width = (sizeValue & 0xF0) >> 4;
                    int height = sizeValue & 0x0F;
                    int _w = 0, _h = 0;
                    if (width == 1) _w = 0;
                    else if (width == 2) _w = 16;
                    else if (width == 3) _w = 32;
                    else if (width == 4) _w = 48;
                    else if (width == 5) _w = 64;
                    else if (width == 6) _w = 80;
                    else if (width == 7) _w = 96;
                    else if (width == 8) _w = 112;
                    else _w = 0;
                    if (height == 1) _h = 0;
                    else if (height == 2) _h = 1;
                    else if (height == 3) _h = 2;
                    else if (height == 4) _h = 3;
                    else if (height == 5) _h = 4;
                    else if (height == 6) _h = 5;
                    else if (height == 7) _h = 6;
                    else if (height == 8) _h = 7;
                    else _h = 0;
                    SelectCharacterSize(_w == 0 ? 1 : _w / 16 + 1, _h + 1);
                    return i + 3;
                }
                break;
            case (byte)'f': // 폰트 선택
                if (i + 2 < bytes.Length)
                {
                    SelectFont((PrinterFont)bytes[i + 2]);
                    return i + 3;
                }
                break;
        }
        return i + 1;
    }

    #endregion

    #region 영수증 관리

    /// <summary>
    /// 새로운 영수증을 시작합니다
    /// </summary>
    public void StartNewReceipt()
    {
        CurrentReceipt = new(_paperConfiguration, _printMode, _lineSpacing);
        ReceiptStack.Add(CurrentReceipt);
        
        Logger.Info($"Starting new receipt (#{ReceiptStack.Count})");
    }

    #endregion

    #region 에뮬레이션

    /// <summary>
    /// 전원 사이클을 수행합니다
    /// </summary>
    public void PowerCycle()
    {
        Initialize();
    }

    #endregion

    #region 직접 API

    /// <summary>
    /// 프린터를 초기화합니다
    /// </summary>
    public void Initialize()
    {
        ClearPrintBuffer();
    
        SelectFont(PrinterFont.FontA);
        SelectJustification(TextJustification.Left);
        SelectCharacterSize(1, 1);
        SelectEmphasizeMode(false);
        SelectItalicMode(false);
        SelectUnderlineMode(UnderlineMode.Off);
        SetDefaultLineSpacing();
    }

    /// <summary>
    /// 텍스트를 인쇄합니다
    /// </summary>
    /// <param name="text">인쇄할 텍스트</param>
    public void PrintText(string text)
    {
        Logger.Info($"Print: {text}");
        
        CurrentReceipt.PrintText(text);
    }

    /// <summary>
    /// 용지를 커트합니다
    /// </summary>
    /// <param name="cutFunction">커트 기능</param>
    /// <param name="cutShape">커트 모양</param>
    /// <param name="n">추가 값</param>
    public void Cut(CutFunction cutFunction = CutFunction.Cut, CutShape cutShape = CutShape.Full, int n = 0)
    {
        Logger.Info($"Execute cut: {cutFunction}, {cutShape}, {n}");
        
        LineFeed();
        
        StartNewReceipt();
    }

    /// <summary>
    /// 현재 라인 간격에 따라 한 줄을 공급합니다
    /// </summary>
    public void LineFeed()
    {
        Logger.Info($"Line feed");
        CurrentReceipt.AdvanceToNewLine();
    }

    /// <summary>
    /// 폰트를 선택합니다
    /// </summary>
    /// <param name="printerFont">프린터 폰트</param>
    public void SelectFont(PrinterFont printerFont)
    {
        Logger.Info($"Select font: {printerFont}");
        
        _printMode.Font = printerFont;
        CurrentReceipt.ChangeFontConfiguration(_printMode);
    }

    /// <summary>
    /// 정렬 방식을 선택합니다
    /// </summary>
    /// <param name="justification">정렬 방식</param>
    public void SelectJustification(TextJustification justification)
    {
        Logger.Info($"Select justification: {justification}");

        _printMode.Justification = justification;
        CurrentReceipt.ChangeFontConfiguration(_printMode);
    }

    /// <summary>
    /// 문자 크기를 선택합니다
    /// </summary>
    /// <param name="width">너비 배율</param>
    /// <param name="height">높이 배율</param>
    public void SelectCharacterSize(int width, int height)
    {
        Logger.Info($"Set character size scale: x{width} width, x{height} height");

        _printMode.CharWidthScale = width;
        _printMode.CharHeightScale = height;
        CurrentReceipt.ChangeFontConfiguration(_printMode);
    }

    /// <summary>
    /// 강조 모드를 선택합니다
    /// </summary>
    /// <param name="enable">강조 모드 활성화 여부</param>
    public void SelectEmphasizeMode(bool enable)
    {
        Logger.Info($"Set emphasize mode: {enable}");

        _printMode.Emphasize = enable;
        CurrentReceipt.ChangeFontConfiguration(_printMode);
    }

    /// <summary>
    /// 이탤릭 모드를 선택합니다
    /// </summary>
    /// <param name="enable">이탤릭 모드 활성화 여부</param>
    public void SelectItalicMode(bool enable)
    {
        Logger.Info($"Set italic mode: {enable}");

        _printMode.Italic = enable;
        CurrentReceipt.ChangeFontConfiguration(_printMode);
    }

    /// <summary>
    /// 밑줄 모드를 선택합니다
    /// </summary>
    /// <param name="mode">밑줄 모드</param>
    public void SelectUnderlineMode(UnderlineMode mode)
    {
        Logger.Info($"Set underline mode: {mode}");

        _printMode.Underline = mode;
        CurrentReceipt.ChangeFontConfiguration(_printMode);
    }

    /// <summary>
    /// 라인 간격을 설정합니다
    /// </summary>
    /// <param name="value">라인 간격 값</param>
    public void SetLineSpacing(int value)
    {
        Logger.Info($"Set line spacing: {value}");

        _lineSpacing = value;
        CurrentReceipt.SetLineSpacing(_lineSpacing);
    }

    /// <summary>
    /// 기본 라인 간격을 설정합니다
    /// </summary>
    public void SetDefaultLineSpacing() => SetLineSpacing(_paperConfiguration.DefaultLineSpacing);
    
    #endregion

    #region 명령어 API

    /// <summary>
    /// 인쇄 버퍼의 데이터를 인쇄하고 현재 라인 간격에 따라 한 줄을 공급합니다
    /// </summary>
    /// <param name="printBuffer">인쇄 버퍼</param>
    public void PrintAndLineFeed(string printBuffer)
    {
        PrintText(printBuffer);
        LineFeed();
    }

    #endregion

    #region 인쇄 버퍼 관리

    private StringBuilder _printBuffer = new StringBuilder();

    /// <summary>
    /// 인쇄 버퍼에 문자를 추가합니다
    /// </summary>
    /// <param name="c">추가할 문자</param>
    private void AppendToPrintBuffer(char c)
    {
        _printBuffer.Append(c);
    }

    /// <summary>
    /// 인쇄 버퍼의 내용을 가져옵니다
    /// </summary>
    /// <returns>인쇄 버퍼 내용</returns>
    private string GetPrintBuffer()
    {
        return _printBuffer.ToString();
    }

    /// <summary>
    /// 인쇄 버퍼를 비웁니다
    /// </summary>
    private void ClearPrintBuffer()
    {
        _printBuffer.Clear();
    }

    #endregion
} 