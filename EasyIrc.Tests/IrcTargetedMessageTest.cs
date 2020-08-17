using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using EasyIrc;

namespace EasyIrc.Tests
{
    [TestFixture]
    class IrcTargetedMessageTest
    {
        [Test]
        public void Constructor__PrivateMessage_ReturnsTrue()
        {
            using (var ircClient = TestHelper.GetTestIrcClient())
            {
                var testRawMessage = ":nickname!ident@host PRIVMSG localuser :Test message";
                var ircMessage = new IrcMessage(testRawMessage);
                var targetedMessage = new IrcTargetedMessage(ircClient, ircMessage);

                Assert.True(targetedMessage.Type == IrcTargetedMessage.MessageType.Privmsg);
            }
        }

        [Test]
        public void Constructor_Notice_ReturnsTrue()
        {
            using (var ircClient = TestHelper.GetTestIrcClient())
            {
                var testRawMessage = ":nickname!ident@host NOTICE localuser :Test message";
                var ircMessage = new IrcMessage(testRawMessage);
                var targetedMessage = new IrcTargetedMessage(ircClient, ircMessage);

                Assert.True(targetedMessage.Type == IrcTargetedMessage.MessageType.Notice);
            }
        }
    }
}
