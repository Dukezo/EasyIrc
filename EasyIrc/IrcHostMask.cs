using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc
{
    /// <summary>
    ///     Represents an IRC user hostmask.
    /// </summary>
    public class IrcHostMask
    {
        /// <summary>
        ///     The hostmask's nickname.
        /// </summary>
        public string NickName { get; private set; }

        /// <summary>
        ///     The hostmask's ident.
        /// </summary>
        public string Ident { get; private set; }

        /// <summary>
        ///     The hostmask's host.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        ///     Checks if the host mask contains any wildcards.
        /// </summary>
        public bool HasWildcards
        {
            get
            {
                return NickName.Contains('*') || NickName.Contains('*') || NickName.Contains('*');
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IrcHostMask"/> class.
        /// </summary>
        /// <param name="nickName">The nickname of the hostmask.</param>
        /// <param name="ident">The ident of the hostmask.</param>
        /// <param name="host">The host of the hostmask.</param>
        private IrcHostMask(string nickName, string ident, string host)
        {
            NickName = nickName;
            Ident = ident;
            Host = host;
        }

        /// <summary>
        ///     Parses the hostmask string and initializes a new instance of <see cref="IrcHostMask"/>. Returns null if the hostmask is not valid.
        /// </summary>
        /// <param name="hostMask"></param>
        /// <returns></returns>
        public static IrcHostMask Parse(string hostMask)
        {
            int identSeparatorIdx = hostMask.IndexOf('!');
            int hostSeparatorIdx = hostMask.IndexOf('@');

            if (identSeparatorIdx != -1 && hostSeparatorIdx != -1)
            {
                string nickName = hostMask.Substring(0, identSeparatorIdx);
                string ident = hostMask.Substring(identSeparatorIdx + 1, hostSeparatorIdx - identSeparatorIdx);
                string host = hostMask.Substring(hostSeparatorIdx + 1);

                return new IrcHostMask(nickName, ident, host);
            }

            return null;
        }

        public override string ToString()
        {
            return string.Format("{0}!{1}@{2}", NickName, Ident, Host);
        }
    }
}
