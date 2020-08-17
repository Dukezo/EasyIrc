using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc
{
    /// <summary>
    ///     A basic anti flood prevention delaying messages by a constant duration.
    /// </summary>
    public class IrcBasicAntiFlood : IrcAntiFlood
    {
        private long _delay;
        private long _lastMessageTimestamp;

        public IrcBasicAntiFlood(long delay)
        {
            _delay = delay;
        }

        protected override void HandleMessageSent()
        {
            _lastMessageTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        protected override long GetRemainingDelay()
        {
            return  Math.Max(0, _lastMessageTimestamp + _delay - DateTimeOffset.Now.ToUnixTimeMilliseconds());
        }
    }
}
