namespace EscPosEmulator.Emulator.Enums;

/// <summary>
/// 커트 기능 열거형
/// </summary>
public enum CutFunction : byte
{
    /// <summary>
    /// 용지 커트 실행
    /// </summary>
    Cut,
    /// <summary>
    /// [커팅 위치 + (n × 수직 모션 단위)]까지 용지를 공급하고 용지 커트 실행
    /// </summary>
    FeedAndCut,
    /// <summary>
    /// [커팅 위치 + (n × 수직 모션 단위)]를 용지 커팅 위치로 미리 설정하고, 인쇄 및 공급 후 자동 커터 위치에 도달하면 용지 커트 실행
    /// </summary>
    SetCutPos,
    /// <summary>
    /// [커팅 위치 + (n × 수직 모션 단위)]까지 용지를 공급하고 용지 커트 실행한 후, 역공급으로 용지를 인쇄 시작 위치로 이동
    /// </summary>
    FeedAndCutAndReverse
} 