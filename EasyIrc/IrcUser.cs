using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyIrc.Interfaces;

namespace EasyIrc
{
    /// <summary>
    ///     Represents an user in the IRC.
    /// </summary>
    public class IrcUser : IIrcMessageSource, IIrcMessageTarget
    {
        public string SourceName
        {
            get
            {
                return ToString();
            }
        }

        public string TargetName
        {
            get
            {
                return NickName;
            }
        }

        /// <summary>
        ///     Returns the nickname of the user.
        /// </summary>
        public string NickName { get; internal set; }

        /// <summary>
        ///     Returns the ident of the user.
        /// </summary>
        public string Ident { get; private set; }

        /// <summary>
        ///     Returns the host of the user.
        /// </summary>
        public string Host { get; private set; }

        private IrcClient _client;

        /// <summary>
        ///     Initializes a new instance of <see cref="IrcUser"/>
        /// </summary>
        /// <param name="hostMask">The parsed hostmask of the user</param>
        public IrcUser(IrcClient client, IrcHostMask hostMask) : this(client, hostMask.NickName, hostMask.Ident, hostMask.Host) { }

        /// <summary>
        ///     Initializes a new instance of <see cref="IrcUser"/>
        /// </summary>
        /// <param name="nickName">The user's nickname.</param>
        /// <param name="ident">The user's ident.</param>
        /// <param name="host">The user's host.</param>
        public IrcUser(IrcClient client, string nickName, string ident, string host)
        {
            _client = client;
            NickName = nickName;
            Ident = ident;
            Host = host;
        }

        public IrcUser(IrcClient client, string nickName, string ident) : this(client, nickName, ident, null) { }
        public IrcUser(IrcClient client, string nickName) : this(client, nickName, null, null) { }

        public async Task SendMessageAsync(string message)
        {
            await _client.SendMessageAsync(this, message);
        }

        public async Task SendNoticeAsync(string message)
        {
            await _client.SendNoticeAsync(this, message);
        }

        public override string ToString()
        {
            if (NickName != null && (Ident == null || Host == null))
                return NickName;

            return string.Format("{0}!{1}@{2}", NickName, Ident, Host);
        }

        public void Update(IrcHostMask hostMask)
        {
            NickName = hostMask.NickName;
            Ident = hostMask.Ident;
            Host = hostMask.Host;
        }

        public bool Equals(IrcUser other)
        {
            if (other == null)
                return false;

            // Only compare nicknames if one of the instances doesn't have ident or host defined.
            if (NickName != null && (Ident == null || Host == null) || other.NickName != null && (other.Ident == null || other.Host == null))
                return String.Equals(NickName, other.NickName, StringComparison.OrdinalIgnoreCase);
            
            return String.Equals(other.ToString(), ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public bool IsClientUser()
        {
            return Equals(_client.User);
        }
    }
}
