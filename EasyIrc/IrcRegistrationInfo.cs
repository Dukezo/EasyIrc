using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc
{
    /// <summary>
    ///     Contains all informations used for registering the connection.
    /// </summary>
    public class IrcRegistrationInfo
    {
        /// <summary>
        ///     The nickname the connection will register with.
        /// </summary>
        public String NickName { get; private set; }

        /// <summary>
        ///     The username the connection will register with.
        /// </summary>
        public String UserName { get; private set; }

        /// <summary>
        ///     The password the connection will register with.
        /// </summary>
        public String Password { get; private set; }


        /// <summary>
        ///     Initializes a new instance of the <see cref="IrcRegistrationInfo"/> class.
        /// </summary>
        /// <param name="nickName">The nickname the connection will register with.</param>
        /// <param name="userName">The username the connection will register with.</param>
        /// <param name="password">The password the connection will register with.</param>
        public IrcRegistrationInfo(string nickName, string userName, string password)
        {
            NickName = nickName;
            UserName = userName;
            Password = password;
        }

        public IrcRegistrationInfo(string nickName, string userName) : this(nickName, userName, null) { }

        public IrcRegistrationInfo(string nickName) : this(nickName, nickName, null) { }
    }
}
