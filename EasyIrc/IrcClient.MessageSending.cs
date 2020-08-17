using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Diagnostics;
using EasyIrc.Events;
using EasyIrc.Interfaces;
using EasyIrc.Enums;

namespace EasyIrc
{
    partial class IrcClient
    {
        /// <summary>
        ///     Asynchronously sends a message to the server. If <see cref="IrcClient._antiFlood"/> returns a caculated delay greater than 0, the message gets queued up in the <see cref="IrcClient._messageQueue"/> and a new thread is started to process the queue.
        ///     If <see cref="IrcClient._messageQueue"/> is being processed, the message is queued up.
        /// </summary>
        /// <param name="rawMessage">The raw message to be sent</param>
		public async Task SendRawMessageAsync(String rawMessage)
        {
            if (State == IrcState.Offline)
                throw new InvalidOperationException("The client is not connected to the server.");

            if (_antiFlood != null)
                await _antiFlood.SendMessageAsync(this, rawMessage);
            else
            {
                await WriteLineAsync(rawMessage);

                var e = new IrcRawMessageEventArgs(rawMessage);
                OnRawMessageSent(e);
            }
                
        }

        internal async Task WriteLineAsync(string line)
        {
            await _writer.WriteLineAsync(line);
        }

        private async Task RequestCapabilitiesAsync(string[] capabilities)
        {
            foreach(string capability in capabilities)
                await SendRawMessageAsync(string.Format("CAP REQ {0}", capability));
        }

        /// <summary>
        ///     Sends the NICK command to the server.
        /// </summary>
        /// <param name="nick">The nick parameter to be sent.</param>
        /// <returns></returns>
		public async Task SendNickAsync(string nick)
        {
            await SendRawMessageAsync(string.Format("NICK {0}", nick));
        }

        /// <summary>
        ///     Sends the USER command to the server.
        /// </summary>
        /// <param name="user">The user parameter to be sent.</param>
        /// <returns></returns>
		internal async Task SendUserAsync(string user)
        {
            await SendRawMessageAsync(string.Format("USER {0} {0} {0} {0}", user));
        }

        /// <summary>
        ///     Sends the PASSWORD command to the server.
        /// </summary>
        /// <param name="password">The password parameter to be sent</param>
        /// <returns></returns>
		internal async Task SendPasswordAsync(string password)
        {
            await SendRawMessageAsync(string.Format("PASS {0}", password));
        }

        /// <summary>
        ///     Sends the QUIT command to the server.
        /// </summary>
        /// <returns></returns>
        internal async Task SendQuitAsync()
        {
            await SendRawMessageAsync("QUIT");
        }

        /// <summary>
        ///     Sends a private message to the target.
        /// </summary>
        /// <param name="target">The target of the message.</param>
        /// <param name="message">The message to be sent.</param>
        /// <returns></returns>
        public async Task SendMessageAsync(string targetName, string message)
        {
            await SendRawMessageAsync(string.Format("PRIVMSG {0} :{1}", targetName, message));
        }

        public async Task SendMessageAsync(IIrcMessageTarget target, string message)
        {
            await SendMessageAsync(target.TargetName, message);
        }

        /// <summary>
        ///     Sends a notice to the target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendNoticeAsync(IIrcMessageTarget target, string message)
        {
            await SendRawMessageAsync(string.Format("NOTICE {0} :{1}", target.TargetName, message));
        }

        public async Task SendNoticeAsync(string targetName, string message)
        {
            await SendRawMessageAsync(string.Format("NOTICE {0} :{1}", targetName, message));
        }

        /// <summary>
        ///     Joins the channel.
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public async Task JoinChannelAsync(string channelName)
        {
            if (Channels.ContainsKey(channelName))
                throw new ArgumentException("The client has already present in this channel.");

            await SendRawMessageAsync(string.Format("JOIN #{0}", channelName.ToLower().ToLower()));
        }
    }
}
