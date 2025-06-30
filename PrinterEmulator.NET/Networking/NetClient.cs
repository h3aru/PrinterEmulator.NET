using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EscPosEmulator.Logging;

namespace EscPosEmulator.Networking;

/// <summary>
/// 네트워크 클라이언트 클래스
/// </summary>
public class NetClient
{
    public readonly NetServer Server;
    public readonly EndPoint RemoteEndPoint;
    
    private Socket _socket;
    private CancellationTokenSource _lifetimeCts;

    /// <summary>
    /// 클라이언트가 연결되어 있는지 확인합니다
    /// </summary>
    public bool IsConnected => _socket.Connected;
    
    public NetClient(NetServer server, Socket clientSocket)
    {
        Server = server;
        RemoteEndPoint = clientSocket.RemoteEndPoint!;
        
        _socket = clientSocket;
        _lifetimeCts = new();
    }

    /// <summary>
    /// 클라이언트 연결을 닫습니다
    /// </summary>
    public void Close()
    {
        if (!_lifetimeCts.IsCancellationRequested)
            _lifetimeCts.Cancel();
        
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
        
        Logger.Info($"Closed client connection {RemoteEndPoint}");
    }

    /// <summary>
    /// 데이터 수신 루프를 실행합니다
    /// </summary>
    public async Task ReceiveLoopAsync()
    {
        try
        {
            var receiveBuffer = GC.AllocateArray<byte>(1024, true);
            var bufferMemory = receiveBuffer.AsMemory();

            while (!_lifetimeCts.Token.IsCancellationRequested)
            {
                var byteCount = await _socket.ReceiveAsync(bufferMemory, SocketFlags.None);

                if (byteCount <= 0)
                {
                    Close();
                    return;
                }

                Logger.Info($"Received TCP data (byteCount={byteCount}, RemoteEndPoint={RemoteEndPoint})");

                HandleIncomingData(bufferMemory.Span[..byteCount]);
            }
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "Receive error");
            Close();
        }
    }

    /// <summary>
    /// 수신된 데이터를 처리합니다
    /// </summary>
    /// <param name="data">수신된 데이터</param>
    private static void HandleIncomingData(ReadOnlySpan<byte> data) =>
        App.Printer?.FeedEscPos(Encoding.ASCII.GetString(data));
} 