using System.Drawing;
using EscPosEmulator.Emulator.Abstraction;

namespace EscPosEmulator.Emulator.Printables;

/// <summary>
/// 영수증 빈 라인 클래스
/// </summary>
public class ReceiptEmptyLine : IReceiptPrintable
{
    private readonly int _height;
    
    public ReceiptEmptyLine(int height)
    {
        _height = height;
    }

    /// <summary>
    /// 출력 높이를 반환합니다
    /// </summary>
    /// <returns>출력 높이</returns>
    public int GetPrintHeight() => _height;
    
    /// <summary>
    /// 비트맵에 렌더링합니다 (빈 라인이므로 아무것도 그리지 않음)
    /// </summary>
    /// <param name="bitmap">대상 비트맵</param>
    /// <param name="g">그래픽 객체</param>
    /// <param name="offsetX">X 오프셋</param>
    /// <param name="offsetY">Y 오프셋</param>
    public void Render(Bitmap bitmap, Graphics g, int offsetX, int offsetY)
    {
        // 빈 라인이므로 아무것도 그리지 않음
    }
} 