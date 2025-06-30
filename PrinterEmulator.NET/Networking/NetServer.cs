using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EscPosEmulator.Logging;

namespace EscPosEmulator.Networking;

/// <summary>
/// 네트워크 서버 클래스
/// </summary>
public class NetServer
{
    public IPEndPoint EndPoint { get; private set; }
    
    private Socket? _tcpSocket;
    private CancellationTokenSource? _cts;
    
    private List<NetClient> _clients;

    /// <summary>
    /// 서버가 실행 중인지 확인합니다
    /// </summary>
    public bool IsRunning => _tcpSocket is not null && _tcpSocket.IsBound;

    public NetServer(int port)
    {
        EndPoint = new IPEndPoint(IPAddress.Any, port);
        
        _clients = new();
    }
    
    /// <summary>
    /// 서버를 실행합니다
    /// </summary>
    public async Task Run()
    {
        Stop();
        
        Logger.Info($"Starting NetServer on TCP port {EndPoint.Port}");
        
        _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _tcpSocket.Bind(EndPoint);
        _tcpSocket.Listen(8);
        
        Logger.Info($"Server bound to {EndPoint}, starting accept/receive loop");
        
        _cts = new CancellationTokenSource();
        await AcceptLoopAsync(_cts.Token);
    }

    /// <summary>
    /// 서버를 중지합니다
    /// </summary>
    public void Stop()
    {
        if (_cts is not null)
        {
            _cts.Cancel();
            _cts = null;
        }
    }

    /// <summary>
    /// 클라이언트 연결을 수락하는 루프입니다
    /// </summary>
    /// <param name="cancellationToken">취소 토큰</param>
    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (IsRunning && !cancellationToken.IsCancellationRequested)
        {
            var clientSocket = await _tcpSocket!.AcceptAsync(cancellationToken);
            
            if (!clientSocket.Connected)
                continue;

            var client = new NetClient(this, clientSocket);
            _clients.Add(client);
            
            Logger.Info($"Accepted new connection (RemoteEndPoint={client.RemoteEndPoint})");
            
            _ = client.ReceiveLoopAsync();
        }
    }
} 