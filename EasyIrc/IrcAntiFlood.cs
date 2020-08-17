using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyIrc.Events;
using EasyIrc.Interfaces;

namespace EasyIrc
{
    /// <summary>
    ///     Abstract class used for all <see cref="IIrcAntiFlood"/> implementations. This class queues up messages and sends them to the server.
    ///     The messages are delayed by the duration returned by <see cref="IrcAntiFlood.GetRemainingDelay"/> to prevent flooding the server.
    /// </summary>
    public abstract class IrcAntiFlood : IIrcAntiFlood
    {
        /// <summary>
        ///     True if the message queue is being processed.
        /// </summary>
        private bool _isProcessingMessageQueue;

        /// <summary>
        ///     Holding all queued up messages.
        /// </summary>
        private ConcurrentQueue<Tuple<IrcClient, string>> _messageQueue;

        /// <summary>
        ///     The constructor of <see cref="IrcAntiFlood"/>.
        /// </summary>
        public IrcAntiFlood()
        {
            _messageQueue = new ConcurrentQueue<Tuple<IrcClient, string>>();
        }

        /// <summary>
        ///     Sends the message to the server using the passed <see cref="IrcClient"/> instance.
        ///     If the message queue is being processed during the time this method is called, the message is queued up instead.
        ///     A new asynchronous task processing the message queue is started if the message queue is empty at the time this method is called and
        ///     <see cref="GetRemainingDelay"/> returns a value greater than zero.
        /// </summary>
        /// <param name="client">The client sending the message.</param>
        /// <param name="message">The message to be sent</param>
        /// <returns></returns>
        public async Task SendMessageAsync(IrcClient client, string message)
        {
            if(!_isProcessingMessageQueue)
            {
                if(GetRemainingDelay() == 0)
                {
                    await client.WriteLineAsync(message);
                    var e = new IrcRawMessageEventArgs(message);
                    client.OnRawMessageSent(e);

                    HandleMessageSent();
                }
                else
                {
                    _messageQueue.Enqueue(new Tuple<IrcClient, string>(client, message));
                    _isProcessingMessageQueue = true;
                    await Task.Factory.StartNew(ProcessMessageQueueAsync);
                }   
            }
            else
            {
                _messageQueue.Enqueue(new Tuple<IrcClient, string>(client, message));
            }
        }

        /// <summary>
        ///     Processes the message queue and triggers the <see cref="MessageSent"/> event every time a message is sent to the server.
        /// </summary>
        /// <returns></returns>
        private async Task ProcessMessageQueueAsync()
        {
            Tuple<IrcClient, string> tuple = null;
            long delay = 0;

            while (_messageQueue.TryDequeue(out tuple))
            {
                IrcClient client = tuple.Item1;
                string message = tuple.Item2;
                delay = GetRemainingDelay();

                if (delay > 0)
                    await Task.Delay((int) delay);

                await client.WriteLineAsync(message);
                HandleMessageSent();

                var e = new IrcRawMessageEventArgs(message);
                client.OnRawMessageSent(e);
            }

            _isProcessingMessageQueue = false;
        }

        /// <summary>
        ///     Abstract method for calculating the remaining delay until a new message can be sent.
        /// </summary>
        /// <returns></returns>
        protected abstract long GetRemainingDelay();

        /// <summary>
        ///     Abstract method called after a message was sent. Classes inheriting this abstract class can use this method to
        ///     save the timestamp and more in order to determine the remaining delay.
        /// </summary>
        protected abstract void HandleMessageSent();
    }
}
