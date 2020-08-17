using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc
{
    class IrcRateLimiter : IrcAntiFlood
    {
        private int _rate;
        private int _cyclePeriod;
        private long _messageCounter;
        private long _cycleStartTimestamp;

        public IrcRateLimiter(int rate, int cyclePeriod)
        {
            _rate = rate;
            _cyclePeriod = cyclePeriod;
        }

        protected override void HandleMessageSent()
        {
            long curentTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (curentTimestamp >= _cycleStartTimestamp + _cyclePeriod * 1000)
            {
                _messageCounter = 0;
                _cycleStartTimestamp = curentTimestamp;
            }

            _messageCounter++;
        }

        protected override long GetRemainingDelay()
        {
            if (_messageCounter < _rate)
                return 0;

            return Math.Max(0, _cycleStartTimestamp + _cyclePeriod * 1000 - DateTimeOffset.Now.ToUnixTimeMilliseconds());
        }
    }
}
