using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Tests
{
    [TestFixture]
    class IrcHostMaskTest
    {
        [Test]
        public void Parse_ValidHostMask_ReturnsInstance()
        {
            string hostMask = "test_nick!ident@test.com";
            IrcHostMask parsedHostMask = IrcHostMask.Parse(hostMask);

            Assert.IsNotNull(parsedHostMask);
        }

        [Test]
        public void Parse_InvalidHostMask_ReturnsNull()
        {
            string hostMask = "test_nick_ident@test.com";
            IrcHostMask parsedHostMask = IrcHostMask.Parse(hostMask);

            Assert.IsNull(parsedHostMask);
        }
    }
}
