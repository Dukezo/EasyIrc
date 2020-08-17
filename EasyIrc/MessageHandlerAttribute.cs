using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal class MessageHandlerAttribute : Attribute
    {
        public string CommandName { get; private set; }

        public MessageHandlerAttribute(string commandName)
        {
            CommandName = commandName;
        }
    }
}
