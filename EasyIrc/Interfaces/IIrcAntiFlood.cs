using EasyIrc.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Interfaces
{
    public interface IIrcAntiFlood
    {
        Task SendMessageAsync(IrcClient client, string message);
    }
}
