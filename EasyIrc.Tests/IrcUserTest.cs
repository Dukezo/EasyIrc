using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Tests
{
    [TestFixture]
    class IrcUserTest
    {
        [Test]
        public void Update_Always_PropertiesChanged()
        {
            var client = TestHelper.GetTestIrcClient();
            var user = new IrcUser(client, "test_nick", "test_ident", "test_host");
            var hostMask = IrcHostMask.Parse("updated_test_nick!update_test_ident@updated_test_host");
            user.Update(hostMask);

            Assert.True(user.NickName == hostMask.NickName);
            Assert.True(user.Ident == hostMask.Ident);
            Assert.True(user.Host == hostMask.Host);
        }
    }
}
