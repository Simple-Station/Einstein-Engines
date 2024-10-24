using Lidgren.Network;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    [Parallelizable]
    [TestOf(typeof(NetBitWriter))]
    public class NetBitWriterTests
    {
        [Test]
        public void TestReadBytesUnaligned()
        {
            var fromBuffer = new byte[] {0b00001111, 0b10101010};
            var dst = new byte[2];

            NetBitWriter.ReadBytes(fromBuffer, 1, 4, dst, 1);

            Assert.That(dst, Is.EqualTo(new byte[] {0, 0b10100000}));
        }

        [Test]
        public void TestReadBytesAligned()
        {
            var fromBuffer = new byte[] {0b00001111, 0b10101010};
            var dst = new byte[2];

            NetBitWriter.ReadBytes(fromBuffer, 1, 8, dst, 1);

            Assert.That(dst, Is.EqualTo(new byte[] {0, 0b10101010}));
        }
    }
}