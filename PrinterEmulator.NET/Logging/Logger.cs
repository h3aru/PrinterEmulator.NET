using System;
using System.Text;

namespace EscPosEmulator.Logging;

/// <summary>
/// 로깅 클래스
/// </summary>
public static class Logger
{
    /// <summary>
    /// 정보 로그를 출력합니다
    /// </summary>
    /// <param name="values">출력할 값들</param>
    public static void Info(params object[] values)
    {
        PrintMessage("Info", values);
    }
    
    /// <summary>
    /// 예외 로그를 출력합니다
    /// </summary>
    /// <param name="ex">예외 객체</param>
    /// <param name="message">추가 메시지</param>
    public static void Exception(Exception ex, string? message = null)
    {
        PrintMessage("Exception", new object[] { message ?? string.Empty, ex });
    }

    /// <summary>
    /// 메시지를 출력합니다
    /// </summary>
    /// <param name="prefix">접두사</param>
    /// <param name="values">출력할 값들</param>
    private static void PrintMessage(string prefix, object[] values)
    {
        var combinedMessage = $"[{prefix}] {FormatValues(values)}";
        Console.WriteLine(combinedMessage);
    }

    /// <summary>
    /// 값들을 문자열로 포맷합니다
    /// </summary>
    /// <param name="values">포맷할 값들</param>
    /// <returns>포맷된 문자열</returns>
    private static string FormatValues(params object[] values)
    {
        var sb = new StringBuilder();

        foreach (var val in values)
        {
            var asString = val.ToString();
            
            if (String.IsNullOrWhiteSpace(asString))
                continue;
            
            if (sb.Length > 0)
                sb.Append(' ');
            sb.Append(asString);
        }

        return sb.ToString();
    }
} 