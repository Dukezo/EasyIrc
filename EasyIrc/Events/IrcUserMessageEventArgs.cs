using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyIrc.Interfaces;

namespace EasyIrc.Events
{
    public class IrcUserMessageEventArgs : EventArgs
    {
        public IIrcMessageSource Source { get; private set; }
        public Dictionary<string, string> Tags { get; private set; }
        public string Message { get; private set; }

        public IrcUserMessageEventArgs(IrcTargetedMessage targetedMessage)
        {
            Source = targetedMessage.Source;
            Tags = targetedMessage.Tags;
            Message = targetedMessage.Message;
        }
    }
}
