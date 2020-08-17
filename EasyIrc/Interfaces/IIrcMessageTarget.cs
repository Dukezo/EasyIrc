using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Interfaces
{
    /// <summary>
    ///     An interface representing a target for <see cref="IrcTargetedMessage"/>.
    /// </summary>
    public interface IIrcMessageTarget
    {
        string TargetName { get; }

        Task SendMessageAsync(string message);
        Task SendNoticeAsync(string message);
    }
}
