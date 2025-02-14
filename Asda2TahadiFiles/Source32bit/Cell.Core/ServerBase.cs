﻿using Cell.Core.Exceptions;
using Cell.Core.Localization;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace Cell.Core
{
  /// <summary>
  /// This is the base class for all server classes.
  /// <seealso cref="T:Cell.Core.ClientBase" />
  /// </summary>
  public abstract class ServerBase : IDisposable
  {
    protected static readonly Logger log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// A hashtable containing all of the clients connected to the server.
    /// <seealso cref="T:Cell.Core.ClientBase" />
    /// </summary>
    protected HashSet<IClient> _clients = new HashSet<IClient>();

    /// <summary>The maximum number of pending connections.</summary>
    protected int _maxPendingCon = 100;

    /// <summary>The buffer for incoming UDP data.</summary>
    private byte[] _udpBuffer = new byte[1024];

    /// <summary>
    /// The remote endpoint (IP address and port) of the adapter to use with TCP communiations.
    /// </summary>
    protected IPEndPoint _tcpEndpoint;

    /// <summary>
    /// The remote endpoint (IP address and port) of the adapter to use with UDP communiations.
    /// </summary>
    protected IPEndPoint _udpEndpoint;

    /// <summary>
    /// The socket the server listens on for incoming TCP connections.
    /// <seealso cref="M:Cell.Core.ServerBase.Start(System.Boolean,System.Boolean)" />
    /// <seealso cref="P:Cell.Core.ServerBase.TcpIP" />
    /// <seealso cref="P:Cell.Core.ServerBase.TcpPort" />
    /// </summary>
    protected Socket _tcpListen;

    /// <summary>
    /// The socket the server listens on for incoming UDP packets.
    /// </summary>
    protected Socket _udpListen;

    /// <summary>
    /// True if the server is currently accepting connections.
    /// </summary>
    protected bool _running;

    /// <summary>True if TCP is enabled, default is true.</summary>
    protected bool TcpEnabledEnabled;

    /// <summary>True if UDP is enabled, default is false.</summary>
    protected bool UdpEnabledEnabled;

    /// <summary>Gets the current status of the server.</summary>
    public virtual bool IsRunning
    {
      get { return _running; }
      set { _running = value; }
    }

    /// <summary>Gets/Sets the maximum number of pending connections.</summary>
    /// <value>The maximum number of pending connections.</value>
    public virtual int MaximumPendingConnections
    {
      get { return _maxPendingCon; }
      set
      {
        if(value <= 0)
          return;
        _maxPendingCon = value;
      }
    }

    /// <summary>
    /// Gets/Sets the port the server will listen on for incoming TCP connections.
    /// <seealso cref="M:Cell.Core.ServerBase.Start(System.Boolean,System.Boolean)" />
    /// <seealso cref="P:Cell.Core.ServerBase.TcpIP" />
    /// </summary>
    public virtual int TcpPort
    {
      get { return _tcpEndpoint.Port; }
      set { _tcpEndpoint.Port = value; }
    }

    /// <summary>
    /// Gets/Sets the port the server will listen on for incoming UDP connections.
    /// <seealso cref="M:Cell.Core.ServerBase.Start(System.Boolean,System.Boolean)" />
    /// <seealso cref="P:Cell.Core.ServerBase.UdpIP" />
    /// </summary>
    public virtual int UdpPort
    {
      get { return _udpEndpoint.Port; }
      set { _udpEndpoint.Port = value; }
    }

    /// <summary>
    /// The IP address of the adapter the server will use for TCP communications.
    /// <seealso cref="M:Cell.Core.ServerBase.Start(System.Boolean,System.Boolean)" />
    /// <seealso cref="P:Cell.Core.ServerBase.TcpPort" />
    /// </summary>
    public virtual IPAddress TcpIP
    {
      get { return _tcpEndpoint.Address; }
      set { _tcpEndpoint.Address = value; }
    }

    /// <summary>
    /// The IP address of the adapter the server will use for UDP communications.
    /// </summary>
    public virtual IPAddress UdpIP
    {
      get { return _udpEndpoint.Address; }
      set { _udpEndpoint.Address = value; }
    }

    /// <summary>
    /// The endpoint clients will connect to for TCP connections
    /// </summary>
    public virtual IPEndPoint TcpEndPoint
    {
      get { return _tcpEndpoint; }
      set { _tcpEndpoint = value; }
    }

    /// <summary>
    /// The endpoint clients will connect to for UDP connections
    /// </summary>
    public virtual IPEndPoint UdpEndPoint
    {
      get { return _udpEndpoint; }
      set { _udpEndpoint = value; }
    }

    /// <summary>
    /// Gets the number of clients currently connected to the server.
    /// </summary>
    public int ClientCount
    {
      get { return _clients.Count; }
    }

    /// <summary>The root path of this server assembly.</summary>
    public string RootPath
    {
      get { return Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName; }
    }

    /// <summary>Gets/Sets whether or not to use TCP communications.</summary>
    public bool TCPEnabled
    {
      get { return TcpEnabledEnabled; }
      set
      {
        if(!_running || TcpEnabledEnabled == value)
          return;
        if(value)
          StartTCP();
        else
          StopTCP();
      }
    }

    /// <summary>Gets/Sets whether or not to use UDP communications.</summary>
    public bool UDPEnabled
    {
      get { return UdpEnabledEnabled; }
      set
      {
        if(UdpEnabledEnabled && !value && _running)
        {
          _udpListen.Close(60);
        }
        else
        {
          if(UdpEnabledEnabled || !value || !_running)
            return;
          StartUDP();
        }
      }
    }

    /// <summary>Holds the sequence number for UDP packets</summary>
    public ushort UdpCounter { get; set; }

    public event ClientConnectedHandler ClientConnected;

    public event ClientDisconnectedHandler ClientDisconnected;

    /// <summary>
    /// Starts the server and begins accepting connections.
    /// <seealso cref="M:Cell.Core.ServerBase.Stop" />
    /// </summary>
    public virtual void Start(bool useTcp, bool useUdp)
    {
      try
      {
        if(_running)
          return;
        log.Info(Cell_Core.BaseStart);
        IsRunning = true;
        if(useTcp)
          StartTCP();
        if(useUdp)
          StartUDP();
        log.Info(Cell_Core.ReadyForConnections, this);
      }
      catch(InvalidEndpointException ex)
      {
        log.Fatal(Cell_Core.InvalidEndpoint, ex.Endpoint);
        Stop();
      }
      catch(NoAvailableAdaptersException ex)
      {
        log.Fatal(Cell_Core.NoNetworkAdapters);
        Stop();
      }
    }

    /// <summary>
    /// Stops the server and disconnects all clients.
    /// <seealso cref="M:Cell.Core.ServerBase.Start(System.Boolean,System.Boolean)" />
    /// <seealso cref="M:Cell.Core.ServerBase.RemoveAllClients" />
    /// </summary>
    public virtual void Stop()
    {
      log.Info(Cell_Core.BaseStop);
      if(!IsRunning)
        return;
      IsRunning = false;
      RemoveAllClients();
      if(_tcpListen != null)
        _tcpListen.Close(60);
      if(_udpListen != null)
        _udpListen.Close();
    }

    /// <summary>
    /// Creates a new client object.
    /// <seealso cref="M:Cell.Core.ServerBase.Start(System.Boolean,System.Boolean)" />
    /// </summary>
    /// <returns>A client object to wrap an incoming connection.</returns>
    protected abstract IClient CreateClient();

    /// <summary>
    /// Removes a client from the internal client list.
    /// <seealso cref="M:Cell.Core.ServerBase.RemoveAllClients" />
    /// </summary>
    /// <param name="client">The client to be removed</param>
    protected void RemoveClient(IClient client)
    {
      lock(_clients)
        _clients.Remove(client);
    }

    /// <summary>
    /// Disconnects and removes a client.
    /// <seealso cref="M:Cell.Core.ServerBase.Stop" />
    /// <seealso cref="M:Cell.Core.ServerBase.RemoveAllClients" />
    /// </summary>
    /// <param name="client">The client to be disconnected/removed</param>
    /// <param name="forced">Flag indicating if the client was disconnected already</param>
    public void DisconnectClient(IClient client, bool forced, bool closeSocketLater = false)
    {
      RemoveClient(client);
      try
      {
        OnClientDisconnected(client, forced);
        if(closeSocketLater)
        {
          Timer timer = new Timer(i =>
          {
            Thread.Sleep(5000);
            client.Dispose();
          }, null, 5000, -1);
        }
        else
          client.Dispose();
      }
      catch(ObjectDisposedException ex)
      {
      }
      catch(Exception ex)
      {
        LogManager.GetLogger("CellCore").ErrorException("Could not disconnect client", ex);
      }
    }

    /// <summary>
    /// Disconnects all clients currently connected to the server.
    /// <seealso cref="M:Cell.Core.ServerBase.Stop" />
    /// </summary>
    public void RemoveAllClients()
    {
      lock(_clients)
      {
        foreach(IClient client in _clients)
        {
          try
          {
            OnClientDisconnected(client, true);
          }
          catch(ObjectDisposedException ex)
          {
          }
          catch(Exception ex)
          {
            LogManager.GetLogger("CellCore").Error(ex.ToString());
          }
        }

        _clients.Clear();
      }
    }

    /// <summary>Called when a client has connected to the server.</summary>
    /// <param name="client">The client that has connected.</param>
    /// <returns>True if the connection is to be accepted.</returns>
    protected virtual bool OnClientConnected(IClient client)
    {
      Info(client, Cell_Core.ClientConnected);
      ClientConnectedHandler clientConnected = ClientConnected;
      if(clientConnected != null)
        clientConnected(client);
      return true;
    }

    /// <summary>
    /// Called when a client has been disconnected from the server.
    /// </summary>
    /// <param name="client">The client that has been disconnected.</param>
    /// <param name="forced">Indicates if the client disconnection was forced</param>
    protected virtual void OnClientDisconnected(IClient client, bool forced)
    {
      Info(client, Cell_Core.ClientDisconnected);
      ClientDisconnectedHandler clientDisconnected = ClientDisconnected;
      if(clientDisconnected == null)
        return;
      clientDisconnected(client, forced);
    }

    /// <summary>
    /// Verifies that an endpoint exists as an address on the local network interfaces.
    /// </summary>
    /// <param name="endPoint">the endpoint to verify</param>
    public static void VerifyEndpointAddress(IPEndPoint endPoint)
    {
      if(endPoint.Address.Equals(IPAddress.Any) || endPoint.Address.Equals(IPAddress.Loopback))
        return;
      NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
      IPAddress endpointAddr = endPoint.Address;
      if(networkInterfaces.Length > 0)
      {
        foreach(NetworkInterface networkInterface in networkInterfaces)
        {
          if(networkInterface.GetIPProperties().UnicastAddresses
            .Where(
              ipInfo =>
                ipInfo.Address.Equals(endpointAddr)).Any())
            return;
        }

        throw new InvalidEndpointException(endPoint);
      }

      throw new NoAvailableAdaptersException();
    }

    /// <summary>
    /// Get the default external IP address for the current machine. This is always the first
    /// IP listed in the host address list.
    /// </summary>
    /// <returns></returns>
    public static IPAddress GetDefaultExternalIPAddress()
    {
      return IPAddress.Loopback;
    }

    /// <summary>
    /// Begin listening for TCP connections. Should not be called directly - instead use <see cref="M:Cell.Core.ServerBase.Start(System.Boolean,System.Boolean)" />
    /// <seealso cref="P:Cell.Core.ServerBase.TCPEnabled" />
    /// </summary>
    protected void StartTCP()
    {
      if(TcpEnabledEnabled || !_running)
        return;
      VerifyEndpointAddress(TcpEndPoint);
      _tcpListen = new Socket(TcpEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      try
      {
        _tcpListen.Bind(TcpEndPoint);
      }
      catch(Exception ex)
      {
        log.Error("Could not bind to Address {0}: {1}", TcpEndPoint, ex);
        return;
      }

      _tcpListen.Listen(MaximumPendingConnections);
      SocketHelpers.SetListenSocketOptions(_tcpListen);
      StartAccept(null);
      TcpEnabledEnabled = true;
      Info(null, Cell_Core.ListeningTCPSocket, (object) TcpEndPoint);
    }

    /// <summary>
    /// Begin listening for TCP connections. Should not be called directly - instead use <see cref="M:Cell.Core.ServerBase.Start(System.Boolean,System.Boolean)" />
    /// <seealso cref="P:Cell.Core.ServerBase.TCPEnabled" />
    /// </summary>
    protected void StopTCP()
    {
      if(!TcpEnabledEnabled)
        return;
      try
      {
        _tcpListen.Close();
      }
      catch(Exception ex)
      {
        log.Warn("Exception occured while trying to close the TCP Connection",
          TcpEndPoint, ex);
      }

      _tcpListen = null;
      TcpEnabledEnabled = false;
      Info(null, Cell_Core.ListeningTCPSocketStopped, (object) TcpEndPoint);
    }

    /// <summary>
    /// Begin listening for UDP connections. Should not be called directly - instead use <see cref="M:Cell.Core.ServerBase.Start(System.Boolean,System.Boolean)" />
    /// <seealso cref="P:Cell.Core.ServerBase.TCPEnabled" />
    /// </summary>
    public void StartUDP()
    {
      if(UdpEnabledEnabled || !_running)
        return;
      IPEndPoint endPoint = new IPEndPoint(UdpIP, UdpPort);
      VerifyEndpointAddress(endPoint);
      _udpListen = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      _udpListen.Bind(endPoint);
      StartReceivingUdp(null);
      UdpEnabledEnabled = true;
      Info(null, Cell_Core.ListeningUDPSocket, (object) UdpEndPoint);
    }

    protected void StartAccept(SocketAsyncEventArgs acceptEventArg)
    {
      if(acceptEventArg == null)
      {
        acceptEventArg = new SocketAsyncEventArgs();
        acceptEventArg.Completed += AcceptEventCompleted;
      }
      else
        acceptEventArg.AcceptSocket = null;

      if(_tcpListen.AcceptAsync(acceptEventArg))
        return;
      ProcessAccept(acceptEventArg);
    }

    private void AcceptEventCompleted(object sender, SocketAsyncEventArgs e)
    {
      ProcessAccept(e);
    }

    private void ProcessAccept(SocketAsyncEventArgs args)
    {
      try
      {
        if(!_running)
        {
          LogManager.GetLogger("CellCore").Info(Cell_Core.ServerNotRunning);
        }
        else
        {
          IClient client = CreateClient();
          client.TcpSocket = args.AcceptSocket;
          client.AddrTemp = ((IPEndPoint) client.TcpSocket.RemoteEndPoint).Address.ToString();
          client.BeginReceive();
          StartAccept(args);
          if(OnClientConnected(client))
          {
            lock(_clients)
              _clients.Add(client);
          }
          else
          {
            client.TcpSocket.Shutdown(SocketShutdown.Both);
            client.TcpSocket.Close();
          }
        }
      }
      catch(ObjectDisposedException ex)
      {
      }
      catch(SocketException ex)
      {
        LogManager.GetLogger("CellCore").WarnException(Cell_Core.SocketExceptionAsyncAccept, ex);
      }
      catch(Exception ex)
      {
        LogManager.GetLogger("CellCore").FatalException(Cell_Core.FatalAsyncAccept, ex);
      }
    }

    protected void StartReceivingUdp(SocketAsyncEventArgs args)
    {
      if(args == null)
      {
        args = new SocketAsyncEventArgs();
        args.Completed += UdpRecvEventCompleted;
      }

      EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
      args.RemoteEndPoint = endPoint;
      args.SetBuffer(_udpBuffer, 0, _udpBuffer.Length);
      if(_udpListen.ReceiveAsync(args))
        return;
      ProcessUdpReceive(args);
    }

    /// <summary>Handles an incoming UDP datagram.</summary>
    /// <param name="ar">The results of the asynchronous operation.</param>
    private void UdpRecvEventCompleted(object sender, SocketAsyncEventArgs e)
    {
      ProcessUdpReceive(e);
    }

    /// <summary>Handles an incoming UDP datagram.</summary>
    /// <param name="args">The results of the asynchronous operation.</param>
    private void ProcessUdpReceive(SocketAsyncEventArgs args)
    {
      try
      {
        int bytesTransferred = args.BytesTransferred;
        EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
        EndPoint remoteEndPoint = args.RemoteEndPoint;
        OnReceiveUDP(bytesTransferred, _udpBuffer, remoteEndPoint as IPEndPoint);
        StartReceivingUdp(args);
      }
      catch(ObjectDisposedException ex)
      {
      }
      catch(SocketException ex)
      {
        LogManager.GetLogger("CellCore").WarnException(Cell_Core.SocketExceptionAsyncAccept, ex);
      }
      catch(Exception ex)
      {
        LogManager.GetLogger("CellCore").FatalException(Cell_Core.FatalAsyncAccept, ex);
      }
    }

    /// <summary>Handler for a UDP datagram.</summary>
    /// <param name="num_bytes">The number of bytes in the datagram.</param>
    /// <param name="buf">The buffer holding the datagram.</param>
    /// <param name="ip">The IP address of the sender.</param>
    protected abstract void OnReceiveUDP(int num_bytes, byte[] buf, IPEndPoint ip);

    /// <summary>Asynchronously sends a UDP datagram to the client.</summary>
    /// <param name="buf">An array of bytes containing the packet to be sent.</param>
    /// <param name="client">An IPEndPoint for the datagram to be sent to.</param>
    protected void SendUDP(byte[] buf, IPEndPoint client)
    {
      if(_udpListen == null)
        return;
      _udpListen.BeginSendTo(buf, 0, buf.Length, SocketFlags.None, client,
        SendToCallback, new UDPSendToArgs(this, client));
    }

    /// <summary>Called when a datagram has been sent.</summary>
    /// <param name="ar">The result of the asynchronous operation.</param>
    private static void SendToCallback(IAsyncResult ar)
    {
      UDPSendToArgs asyncState = ar.AsyncState as UDPSendToArgs;
      try
      {
        if(asyncState == null)
          return;
        int num_bytes = asyncState.Server._udpListen.EndSendTo(ar);
        asyncState.Server.OnSendTo(asyncState.ClientIP, num_bytes);
      }
      catch(Exception ex)
      {
        if(asyncState == null)
          return;
        asyncState.Server.Error(null, ex);
      }
    }

    /// <summary>Called when a datagram has been sent.</summary>
    /// <param name="clientIP">The IP address of the recipient.</param>
    /// <param name="num_bytes">The number of bytes sent.</param>
    protected abstract void OnSendTo(IPEndPoint clientIP, int num_bytes);

    /// <summary>
    /// Create a string for logging information about a given client given a formatted message and parameters
    /// </summary>
    /// <param name="client">Client which caused the event</param>
    /// <param name="msg">Message describing the event</param>
    /// <param name="parms">Parameters for formatting the message.</param>
    protected static string FormatLogString(IClient client, string msg, params object[] parms)
    {
      msg = parms == null ? msg : string.Format(msg, parms);
      return client == null ? msg : string.Format("({0}) -> {1}", client, msg);
    }

    /// <summary>Generates a server error.</summary>
    /// <param name="e">An exception describing the error.</param>
    /// <param name="client">The client that generated the error.</param>
    public void Error(IClient client, Exception e)
    {
      Error(client, "Exception raised: " + e);
    }

    /// <summary>Generates a server error.</summary>
    /// <param name="parms">Parameters for formatting the message.</param>
    /// <param name="msg">The message describing the error.</param>
    /// <param name="client">The client that generated the error.</param>
    public virtual void Error(IClient client, string msg, params object[] parms)
    {
      log.Error(FormatLogString(client, msg, parms));
    }

    /// <summary>Generates a server warning.</summary>
    /// <param name="e">An exception describing the warning.</param>
    /// <param name="client">The client that generated the error.</param>
    public virtual void Warning(IClient client, Exception e)
    {
      if(!log.IsWarnEnabled)
        return;
      log.Warn("{0} - {1}", client, e);
    }

    /// <summary>Generates a server warning.</summary>
    /// <param name="parms">Parameters for formatting the message.</param>
    /// <param name="msg">The message describing the warning.</param>
    /// <param name="client">The client that generated the error.</param>
    public virtual void Warning(IClient client, string msg, params object[] parms)
    {
      if(!log.IsWarnEnabled)
        return;
      log.Warn(FormatLogString(client, msg, parms));
    }

    /// <summary>Generates a server notification.</summary>
    /// <param name="msg">Text describing the notification.</param>
    /// <param name="parms">The parameters to pass to the function for formatting.</param>
    /// <param name="client">The client that generated the error.</param>
    public virtual void Info(IClient client, string msg, params object[] parms)
    {
      if(!log.IsWarnEnabled)
        return;
      log.Info(FormatLogString(client, msg, parms));
    }

    /// <summary>Generates a server debug message.</summary>
    /// <param name="msg">Text describing the notification.</param>
    /// <param name="parms">The parameters to pass to the function for formatting.</param>
    /// <param name="client">The client that generated the error.</param>
    public virtual void Debug(IClient client, string msg, params object[] parms)
    {
      if(!log.IsDebugEnabled)
        return;
      log.Debug(FormatLogString(client, msg, parms));
    }

    ~ServerBase()
    {
      Dispose(false);
    }

    /// <summary>
    /// Don't call this method outside of the Context that manages the server.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if(!_running)
        return;
      Stop();
    }
  }
}