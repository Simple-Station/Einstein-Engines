using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Lidgren.Network;
using NUnit.Framework;

namespace UnitTests
{
    [TestOf(typeof(NetUtility))]
    [TestFixture]
    [Parallelizable]
    public class NetUtilityTests
    {
        // ReSharper disable once StringLiteralTypo
        [Test]
        [TestCase(new byte[]{0xDE, 0xAD, 0xBE, 0xEF}, "DEADBEEF")]
        public void TestToHexString(byte[] bytes, string expected)
        {
            Assert.That(NetUtility.ToHexString(bytes), Is.EqualTo(expected));
        }

        [Test]
        [TestCase((ulong) uint.MaxValue + 1, 33)]
        [TestCase(ulong.MaxValue, 64)]
        [TestCase(0ul, 1)]
        public void TestBitsToHoldUInt64(ulong num, int expectedBits)
        {
            Assert.That(NetUtility.BitsToHoldUInt64(num), Is.EqualTo(expectedBits));
        }

        [Test]
        [TestCase((uint) ushort.MaxValue + 1, 17)]
        [TestCase(0u, 1)]
        [TestCase(uint.MaxValue, 32)]
        public void TestBitsToHoldUInt32(uint num, int bits)
        {
            Assert.That(NetUtility.BitsToHoldUInt(num), Is.EqualTo(bits));
        }

        [Test]
        public void TestResolveBasic()
        {
            var addr = NetUtility.Resolve("example.com");

            Assert.That(addr, Is.Not.Null);
        }

        [Test]
        public void TestResolveEndPointBasic()
        {
            var addr = NetUtility.Resolve("example.com", 55555);

            Assert.That(addr, Is.Not.Null);
            Assert.That(addr.Port, Is.EqualTo(55555));
        }

        [Test]
        [TestCase(AddressFamily.InterNetwork)]
        [TestCase(AddressFamily.InterNetworkV6)]
        public void TestResolveAllowed(AddressFamily family)
        {
	        IgnoreIfActions();
	        
            var addr = NetUtility.Resolve("example.com", family);

            Assert.That(addr?.AddressFamily, Is.EqualTo(family));
        }

        [Test]
        public void TestResolveNothing()
        {
            var addr = NetUtility.Resolve("thisdomaindoesnotexistandneverwill.example.com");

            Assert.That(addr, Is.Null);
        }

        [Test]
        public async Task TestResolveAsyncBasic()
        {
            var addr = await NetUtility.ResolveAsync("example.com");

            Assert.That(addr, Is.Not.Null);
        }

        [Test]
        public async Task TestResolveEndPointAsyncBasic()
        {
            var addr = await NetUtility.ResolveAsync("example.com", 55555);

            Assert.That(addr, Is.Not.Null);
            Assert.That(addr.Port, Is.EqualTo(55555));
        }

        [Test]
        [TestCase(AddressFamily.InterNetwork)]
        [TestCase(AddressFamily.InterNetworkV6)]
        public async Task TestResolveAsyncAllowed(AddressFamily family)
        {
	        IgnoreIfActions();
         
	        var addr = await NetUtility.ResolveAsync("example.com", family);

            Assert.That(addr?.AddressFamily, Is.EqualTo(family));
        }

        [Test]
        public async Task TestResolveAsyncNothing()
        {
            var addr = await NetUtility.ResolveAsync("thisdomaindoesnotexistandneverwill.example.com");

            Assert.That(addr, Is.Null);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        [Test]
        public async Task TestResolveAsyncCallback()
        {
	        var tcs = new TaskCompletionSource<IPAddress?>();
	        NetUtility.ResolveAsync("example.com", result => tcs.SetResult(result));

	        var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(30));

	        Assert.That(result, Is.Not.Null);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        private static void IgnoreIfActions()
        {
	        // https://github.com/actions/virtual-environments/issues/668
	        if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
		        Assert.Ignore("GitHub Actions Runners do not support IPv6 and as such this test is disabled.");
        }
    }
}
