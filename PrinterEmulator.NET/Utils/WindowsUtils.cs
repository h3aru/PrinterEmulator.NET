using System;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace EscPosEmulator.Utils;

/// <summary>
/// Windows 유틸리티 클래스
/// </summary>
public static class WindowsUtils
{
    [DllImport("user32")]
    public static extern int FlashWindow(IntPtr hwnd, bool bInvert);
    
    /// <summary>
    /// 윈도우를 깜빡입니다
    /// </summary>
    /// <param name="wnd">깜빡일 윈도우</param>
    public static void FlashWindow(Window wnd) => FlashWindow(new WindowInteropHelper(wnd).Handle, true);
    
    /// <summary>
    /// 경고음을 재생합니다
    /// </summary>
    public static void Exclaim() => SystemSounds.Exclamation.Play();
    
    /// <summary>
    /// 부드러운 경고음을 재생합니다
    /// </summary>
    public static void ExclaimSoft() => SystemSounds.Hand.Play();
} 