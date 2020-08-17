using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using EasyIrc;
using System.Threading;
using System.Diagnostics;
using EasyIrc.Enums;

namespace EasyIrc.Tests
{
    [TestFixture]
    class IrcClientTest
    {
        public const string SKIP_SETUP = "SkipSetup";
        private IrcClient _ircClient;

        [SetUp]
        public void Init()
        {
            if (!CheckForSkipSetup() && _ircClient != null && _ircClient.State != IrcState.Connected)
                Assert.Inconclusive();
        }

        // This method needs to be called synchronously because it initializes the IrcClient instance that will be used in other tests.
        [Test, Order(1)]
        public void ConnectAsync_Always_RaisesNoError()
        {
            _ircClient = TestHelper.GetTestIrcClient();

            using (var manualResetEvent = new ManualResetEventSlim(false))
            {
                bool errorOccured = false;

                _ircClient.Connected += (sender, e) => manualResetEvent.Set();

                _ircClient.Error += (sender, e) =>
                {
                    errorOccured = true;
                    manualResetEvent.Set();
                };

                var t = _ircClient.ConnectAsync();

                Assert.IsTrue(manualResetEvent.Wait(30000));
                Assert.IsFalse(errorOccured);
            }
        }

        [Test, Order(2)]
        public async Task SendRawMessageAsync_ConnectionEstablished_RaisesNoError()
        {
            bool errorOccured = false;

            _ircClient.Error += (sender, e) =>
            {
                errorOccured = true;
            };

            await _ircClient.SendRawMessageAsync("NUnit Test");
            Assert.IsFalse(errorOccured);
        }

        [Test, Order(3)]
        public async Task RawMessageReceived_Always_TriggersEvent()
        {
            using (var manualResetEvent = new ManualResetEventSlim(false))
            {
                _ircClient.RawMessageReceived += (sender, e) =>
                {
                    Assert.NotNull(e.RawMessage);
                    manualResetEvent.Set();
                };

                await _ircClient.SendRawMessageAsync(string.Format("WHO {0}", _ircClient.User.NickName));
                Assert.IsTrue(manualResetEvent.Wait(30000));
            }
        }

        [Test, Order(4)]
        public async Task RawMessageSent_Always_TriggersEvent()
        {
            using (var manualResetEvent = new ManualResetEventSlim(false))
            {
                _ircClient.RawMessageSent += (sender, e) =>
                {
                    Assert.NotNull(e.RawMessage);
                    manualResetEvent.Set();
                };

                await _ircClient.SendRawMessageAsync("Nunit Test");
                Assert.IsTrue(manualResetEvent.Wait(30000));
            }
        }

        [Test, Order(5)]
        public async Task JoinChannelAsync_Connected_TriggersEvent()
        {
            using (var manualResetEvent = new ManualResetEventSlim(false))
            {
                _ircClient.ChannelJoined += (sender, e) =>
                {
                    if(e.User.IsClientUser())
                        manualResetEvent.Set();
                };

                await _ircClient.JoinChannelAsync("##C");
                Assert.IsTrue(manualResetEvent.Wait(30000));
            }
        }

        [Test, Order(6)]
        public async Task SendMessageAsync_ConnectionEstabilished_TriggersEvent()
        {
            using (var manualResetEvent = new ManualResetEventSlim(false))
            {
                _ircClient.UserMessageReceived += (sender, e) =>
                {
                    manualResetEvent.Set();
                };

                await _ircClient.SendMessageAsync(_ircClient.User, "NUnit Test");
                Assert.IsTrue(manualResetEvent.Wait(10000));
            }
        }

        [Test, Order(7)]
        public async Task SendNoticeAsync_ConnectionEstabilished_TriggersEvent()
        {
            using (var manualResetEvent = new ManualResetEventSlim(false))
            {
                _ircClient.UserNoticeReceived += (sender, e) =>
                {
                    manualResetEvent.Set();
                };

                await _ircClient.SendNoticeAsync(_ircClient.User, "NUnit Test");
                Assert.IsTrue(manualResetEvent.Wait(10000));
            }
        }

        [Test]
        public async Task Dispose_ConnectionEstablished_RaisesNoError()
        {
            using (var manualResetEvent = new ManualResetEventSlim(false))
            {
                bool errorOccured = false;

                _ircClient.Disconnected += (sender, e) => manualResetEvent.Set();

                _ircClient.Error += (sender, e) =>
                {
                    errorOccured = true;
                    manualResetEvent.Set();
                };

                await _ircClient.DisconnectAsync();

                Assert.IsTrue(manualResetEvent.Wait(30000));
                Assert.IsFalse(errorOccured);
            }

            _ircClient.Dispose();
        }

        [Test]
        [Category(SKIP_SETUP)]
        public void Dispose_NoConnectionEstablished_ThrowsNoException()
        {
            var ircClient = TestHelper.GetTestIrcClient();
            ircClient.Dispose();
        }

        private static bool CheckForSkipSetup()
        {
            var categories = TestContext.CurrentContext.Test?.Properties["Category"];

            bool skipSetup = categories != null && categories.Contains("SkipSetup");
            return skipSetup;
        }
    }
}
