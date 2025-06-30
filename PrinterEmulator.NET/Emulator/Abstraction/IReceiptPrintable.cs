using System.Drawing;

namespace EscPosEmulator.Emulator.Abstraction;

/// <summary>
/// 영수증 출력 가능한 항목 인터페이스
/// </summary>
public interface IReceiptPrintable
{
    /// <summary>
    /// 비트맵에 렌더링합니다
    /// </summary>
    /// <param name="bitmap">대상 비트맵</param>
    /// <param name="g">그래픽 객체</param>
    /// <param name="offsetX">X 오프셋</param>
    /// <param name="offsetY">Y 오프셋</param>
    public void Render(Bitmap bitmap, Graphics g, int offsetX, int offsetY);
    
    /// <summary>
    /// 출력 높이를 반환합니다
    /// </summary>
    /// <returns>출력 높이</returns>
    public int GetPrintHeight();
} 