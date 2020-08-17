using EasyIrc.Events;
using EasyIrc.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc
{
    public class IrcChannel : IIrcMessageTarget
    {
        public string TargetName
        {
            get
            {
                return "#" + Name;
            }
        }

        /// <summary>
        ///     The client this channel is associated to.
        /// </summary>
        public IrcClient Client { get; private set; }

        /// <summary>
        ///     The name of the channel.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        ///     The operator status of the client user in this channel.
        /// </summary>
        public bool IsOperator { get; internal set; }


        /// <summary>
        ///     Initializes a new instance of <see cref="IrcChannel"/>.
        /// </summary>
        /// <param name="client">A reference to the <see cref="IrcClient"/> instance.</param>
        /// <param name="name">The name of the channel-</param>
        public IrcChannel(IrcClient client, string name)
        {
            Client = client;
            Name = name;
        }

        /// <summary>
        ///     Sends a PRIVMSG message to the channel.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(string message)
        {
            await Client.SendMessageAsync(this, message);
        }

        /// <summary>
        ///     Sends a NOTICE message to the channel.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendNoticeAsync(string message)
        {
            await Client.SendNoticeAsync(this, message);
        }

        #region Events
        /// <summary>
        ///     Occurs when the channel receives a message.
        /// </summary>
        public event EventHandler<IrcChannelMessageEventArgs> MessageReceived;

        /// <summary>
        ///     Occurs when the channel receives a notice.
        /// </summary>
        public event EventHandler<IrcChannelMessageEventArgs> NoticeReceived;

        /// <summary>
        ///     Occurs when an user joiend a channel.
        /// </summary>
        public event EventHandler<IrcUserEventArgs> UserJoined;

        /// <summary>
        ///     Occurs when an user laeves a channel.
        /// </summary>
        public event EventHandler<IrcUserEventArgs> UserLeft;

        #endregion

        #region Callbacks
        /// <summary>
        ///     Raises the <see cref="MessageReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnMessageReceived(IrcChannelMessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="NoticeReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnNoticeReceived(IrcChannelMessageEventArgs e)
        {
            NoticeReceived?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="UserJoined"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnUserJoined(IrcUserEventArgs e)
        {
            UserJoined?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="UserLeft"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnUserLeft(IrcUserEventArgs e)
        {
            UserLeft?.Invoke(this, e);
        }
        #endregion
    }
}
