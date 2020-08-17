using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Interfaces
{
    /// <summary>
    ///     An interface representing a source of <see cref="IrcTargetedSource"/>.
    /// </summary>
    public interface IIrcMessageSource
    {
        string SourceName { get; }
    }
}
