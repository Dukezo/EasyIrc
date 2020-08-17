using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.IO;
using System.Reflection;
using EasyIrc.Exceptions;
using EasyIrc.Interfaces;
using EasyIrc.Events;
using EasyIrc.Enums;

namespace EasyIrc
{
    /// <summary>
    ///     The IrcClient is responsible for the interaction between client and IRC server.
    /// </summary>
    public partial class IrcClient : IDisposable
    {
        /// <summary>
        ///     The current state of the client.
        /// </summary>
        public IrcState State { get; set; }

        /// <summary>
        ///     Represents the user the client is logged in as.
        /// </summary>
        public IrcUser User { get; internal set; }

        /// <summary>
        ///     The host of the IRC server.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        ///     The port of the IRC server.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        ///     Contains the requested capabilities.
        /// </summary>
        public string[] Capabilities { get; private set; }

        /// <summary>
        ///     The channels the client is present in.
        /// </summary>
        public Dictionary<string, IrcChannel> Channels { get; private set; }

        /// <summary>
        ///     The user informations used for registering the connection.
        /// </summary>
        private IrcRegistrationInfo _registrationInfo;

        /// <summary>
        ///     The TCPClient used for communicating with the server.
        /// </summary>
        private TcpClient _tcpClient;

        /// <summary>
        ///     The StreamWriter used for writing to the TCPClient's network stream.
        /// </summary>
        private StreamWriter _writer;

        /// <summary>
        ///     The StreamReader used for reading from the TCPClient's network stream.
        /// </summary>
        private StreamReader _reader;

        /// <summary>
        ///     A Queue holding messages to be sent to the server.
        /// </summary>
        private ConcurrentQueue<string> _messageQueue;

        /// <summary>
        ///     The anti flood mechanism to use.
        /// </summary>
        private IIrcAntiFlood _antiFlood;

        /// <summary>
        ///     A dictionary holding generic message handlers. Message handlers are methods tagged with the <see cref="MessageHandlerAttribute"/> attribute.
        /// </summary>
        protected Dictionary<string, MessageHandler> _messageHandlers;

        /// <summary>
        ///     The custom message handler delegate. Other than <see cref="MessageHandler"/>, this delegate also has a reference to the <see cref="IrcClient"/>.
        /// </summary>
        /// <param name="client">A reference to the client responsible for the message.</param>
        /// <param name="message">The parsed irc message to be handled.</param>
        /// <returns>The <see cref="IrcMessage"/> instance to be handled.</returns>
        public delegate Task MessageHandler(IrcClient client, IrcMessage message);

        /// <summary>
        ///     Initializes a new instance of the <see cref="IrcClient"/> class.
        /// </summary>
        /// <param name="host">The host of the IRC server.</param>
        /// <param name="port">The port of the IRC server.</param>
        /// <param name="registrationInfo"> The user informations used for registering the connection.</param>
        public IrcClient(string host, int port, IrcRegistrationInfo registrationInfo, IIrcAntiFlood antiFlood, string[] capabilities)
        {
            Host = host;
            Port = port;
            _registrationInfo = registrationInfo;
            _antiFlood = antiFlood;
            Capabilities = capabilities;

            State = IrcState.Offline;


            InitMessageHandlers();
        }

        public IrcClient(string host, int port, IrcRegistrationInfo registrationInfo, IIrcAntiFlood antiFlood) : this(host, port, registrationInfo, antiFlood, null) { }
        public IrcClient(string host, int port, IrcRegistrationInfo registrationInfo) : this(host, port, registrationInfo, null, null) { }

        /// <summary>
        ///     Gets all message handlers and stores them in the dictionary. Message handlers are methods with the <see cref="MessageHandlerAttribute"/> attribute.
        /// </summary>
        protected void InitMessageHandlers()
        {
            _messageHandlers = new Dictionary<string, MessageHandler>();

            var methods = typeof(IrcMessageHandlers).GetTypeInfo().GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(m => m.GetCustomAttributes(typeof(MessageHandlerAttribute), true).Length > 0).ToArray();
            foreach (MethodInfo methodInfo in methods)
            {
                var messageHandler = (MessageHandler)methodInfo.CreateDelegate(typeof(MessageHandler));
                var attributes = (MessageHandlerAttribute[]) methodInfo.GetCustomAttributes(typeof(MessageHandlerAttribute));
                
                foreach(var attribute in attributes)
                {
                    if(!_messageHandlers.ContainsKey(attribute.CommandName))
                        _messageHandlers.Add(attribute.CommandName, messageHandler);
                    else
                        throw new InvalidOperationException(string.Format("The command {0} is already handled by another message handler.", attribute.CommandName));
                }
            }
        }

        public void AddCustomMessageHandler(string command, MessageHandler messageHandler)
        {
            if (!_messageHandlers.ContainsKey(command))
            {
                _messageHandlers.Add(command, messageHandler);
            }
            else
                throw new ArgumentException("The command is already handled by another message handler.");
        }

        /// <summary>
        ///     Connects asynchronously to the server and registers the connection.
        /// </summary>
        public async Task ConnectAsync()
        {
            if(State == IrcState.Connected)
                throw new InvalidOperationException("The client is already connected to a server.");
            else if(State == IrcState.Connecting)
                throw new InvalidOperationException("The client is already connecting to a server.");


            State = IrcState.Connecting;
            Channels = new Dictionary<string, IrcChannel>();
            _messageQueue = new ConcurrentQueue<string>();
            _tcpClient = new TcpClient();

            try
            {
                await _tcpClient.ConnectAsync(Host, Port);

                NetworkStream stream = _tcpClient.GetStream();
                _writer = new StreamWriter(stream) { AutoFlush = true };
                _reader = new StreamReader(stream);

                User = new IrcUser(this, _registrationInfo.NickName);

                Task __ = ReceiveDataAsync();
                await RegisterConnectionAsync(_registrationInfo);

                if (Capabilities != null)
                    await RequestCapabilitiesAsync(Capabilities);

                Debug.WriteLine(string.Format("IrcClient connected to {0}:{1}.", Host, Port));
                State = IrcState.Connected;

                OnConnected();
            }
            catch(SocketException ex)
            {
                HandleSocketError(ex);
            }
            catch(Exception ex)
            {
                OnError(new IrcErrorEventArgs(ex));
            }
        }

        /// <summary>
        ///     Disconnects from the server.
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            if(State != IrcState.Connected)
                throw new InvalidOperationException("The client is not connected to the server.");

            if (State == IrcState.Disconnecting)
                throw new InvalidOperationException("The client is already disconnecting from the server.");

            State = IrcState.Disconnecting;
            await SendQuitAsync();
            HandleClientDisconnected(false);
        }

        private void HandleClientDisconnected(bool connectionClosedByServer)
        {
            State = IrcState.Offline;

            DisposeTcpClient();
            Debug.WriteLine("IrcClient disconnected from {0}:{1}", Host, Port);
            OnDisconnected(new IrcDisconnectEventArgs(false));
        }

        /// <summary>
        ///     Receives data from the IRC connection.
        /// </summary>
        /// <returns></returns>
        private async Task ReceiveDataAsync()
        {
            String rawMessage = null;

            try
            {
                while (State >= IrcState.Connecting)
                {
                    rawMessage = await _reader.ReadLineAsync();

                    if(rawMessage != null)
                        HandleRawMessage(rawMessage);
                    else
                    {
                        HandleClientDisconnected(true);
                        break;
                    }
                }
            }
            catch(ObjectDisposedException ex)
            {
                // The connection got properly closed and the stream disposed. Ignore this exception and move on.
            }
            catch(Exception ex)
            {
                OnError(new IrcErrorEventArgs(ex));
            }
        }

        /// <summary>
        ///     Handles raw messages received from the IRC connection.
        /// </summary>
        /// <param name="rawMessage">The raw message to be handled.</param>
        private void HandleRawMessage(string rawMessage)
        {
            OnRawMessageReceived(new IrcRawMessageEventArgs(rawMessage));

            IrcMessage message = new IrcMessage(rawMessage);

            if (_messageHandlers.ContainsKey(message.Command))
            {
                try
                {
                    _messageHandlers[message.Command](this, message);
                }
                catch(Exception ex)
                {
                    OnError(new IrcErrorEventArgs(ex));
                }  
            }    
        }

        /// <summary>
        ///     Registers the connection.
        /// </summary>
        /// <param name="registrationInfo"></param>
        /// <returns></returns>
        private async Task RegisterConnectionAsync(IrcRegistrationInfo registrationInfo)
        {
            if (registrationInfo.Password != null)
                await SendPasswordAsync(registrationInfo.Password);

            await SendNickAsync(registrationInfo.NickName);
            await SendUserAsync(registrationInfo.UserName);
        }

        /// <summary>
        ///     Handles socket errors thrown by the server connection.
        /// </summary>
        /// <param name="ex">The thrown exception.</param>
        private void HandleSocketError(SocketException ex)
        {
            switch(ex.SocketErrorCode)
            {
                case SocketError.ConnectionReset:
                    HandleClientDisconnected(true);
                    break;
                default:
                    OnError(new IrcErrorEventArgs(ex));
                    break;
            }
        }

        /// <summary>
        ///     Disposes the TCPClient and it's stream including the stream reader and stream writer.
        /// </summary>
        private void DisposeTcpClient()
        {
            if(_tcpClient != null) {
                _tcpClient.Close();
                _tcpClient = null;

                _reader.Dispose();
                _reader = null;

                _writer.Dispose();
                _writer = null;
            }
        }

        /// <summary>
        ///     Disposes disposable objects.
        /// </summary>
        public void Dispose()
        {
            if(State == IrcState.Connected)
            {
                HandleClientDisconnected(false);
            }
            else
            {
                DisposeTcpClient();
            }
        }

        #region Events
        /// <summary>
        ///     Occurs when the client encounters an error.
        /// </summary>
        public event EventHandler<IrcErrorEventArgs> Error;

        /// <summary>
        ///     Occurs when the socket connection receives data.
        /// </summary>
        public event EventHandler<IrcRawMessageEventArgs> RawMessageReceived;

        /// <summary>
        ///     Occurs when the socket connection sends data to the server.
        /// </summary>
        public event EventHandler<IrcRawMessageEventArgs> RawMessageSent;

        /// <summary>
        ///     Occurs when client successfully connected to the server.
        /// </summary>
        public event EventHandler<EventArgs> Connected;

        /// <summary>
        ///     Occurs when client disconnected from the server.
        /// </summary>
        public event EventHandler<EventArgs> Disconnected;

        /// <summary>
        ///     Occurs after the client joined a channel.
        /// </summary>
        public event EventHandler<IrcUserEventArgs> ChannelJoined;

        /// <summary>
        ///     Occurs when the client left a channel.
        /// </summary>
        public event EventHandler<IrcChannelEventArgs> ChannelLeft;

        /// <summary>
        ///     Occurs when the client receives a channel PRIVMSG.
        /// </summary>
        public event EventHandler<IrcChannelMessageEventArgs> ChannelMessageReceived;

        /// <summary>
        ///     Occurs when the client receives a PRIVMSG from another user or server.
        /// </summary>
        public event EventHandler<IrcUserMessageEventArgs> UserMessageReceived;

        /// <summary>
        ///     Occurs when the client receives a channel PRIVMSG.
        /// </summary>
        public event EventHandler<IrcChannelMessageEventArgs> ChannelNoticeReceived;

        /// <summary>
        ///     Occurs when the client receives a NOTICE message from another user or server.
        /// </summary>
        public event EventHandler<IrcUserMessageEventArgs> UserNoticeReceived;

        /// <summary>
        ///     Occurs when the client receives a MODE message with operator flag.
        /// </summary>
        public event EventHandler<IrcOperatorEventArgs> UserOperatorStatusUpdated;

        #endregion
        #region Callbacks
        /// <summary>
        ///     Raises the <see cref="Error"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnError(IrcErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="RawMessageReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnRawMessageReceived(IrcRawMessageEventArgs e)
        {
            RawMessageReceived?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="RawMessageSent"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnRawMessageSent(IrcRawMessageEventArgs e)
        {
            RawMessageSent?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="Connected"/> event.
        /// </summary>#
        /// 
        internal void OnConnected()
        {
            Connected?.Invoke(this, new EventArgs());
        }

        /// <summary>
        ///     Raises the <see cref="Disconnected"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnDisconnected(IrcDisconnectEventArgs e)
        {
            Disconnected?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="ChannelJoined"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnChannelJoined(IrcUserEventArgs e)
        {
            ChannelJoined?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="ChannelLeft"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnChannelLeft(IrcChannelEventArgs e)
        {
            ChannelLeft?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="ChannelMessageReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnChannelMessageReceived(IrcChannelMessageEventArgs e)
        {
            ChannelMessageReceived?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="UserMessageReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnUserMessageReceived(IrcUserMessageEventArgs e)
        {
            UserMessageReceived?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="ChannelNoticeReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnChannelNoticeReceived(IrcChannelMessageEventArgs e)
        {
            ChannelNoticeReceived?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="UserNoticeReceived"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnUserNoticeReceived(IrcUserMessageEventArgs e)
        {
            UserNoticeReceived?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="UserOperatorStatusUpdated"/> event.
        /// </summary>
        /// <param name="e"></param>
        internal void OnUserOperatorStatusUpdated(IrcOperatorEventArgs e)
        {
            UserOperatorStatusUpdated?.Invoke(this, e);
        }
        #endregion
    }
}
