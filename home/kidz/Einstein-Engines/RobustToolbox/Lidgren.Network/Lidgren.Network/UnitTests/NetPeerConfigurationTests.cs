using Lidgren.Network;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    [Parallelizable]
    [TestOf(typeof(NetPeerConfiguration))]
    public class NetPeerConfigurationTests
    {
        [Test]
        public void TestMessageTypes()
        {
            var config = new NetPeerConfiguration("Test");

            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            Assert.That(config.IsMessageTypeEnabled(NetIncomingMessageType.UnconnectedData), Is.True);

            config.SetMessageTypeEnabled(NetIncomingMessageType.UnconnectedData, false);
            Assert.That(config.IsMessageTypeEnabled(NetIncomingMessageType.UnconnectedData), Is.False);
        }
    }
}