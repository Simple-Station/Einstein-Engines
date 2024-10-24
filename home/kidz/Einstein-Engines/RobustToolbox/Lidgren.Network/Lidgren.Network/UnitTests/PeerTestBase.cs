using System;
using System.Reflection;
using System.Text;
using Lidgren.Network;
using NUnit.Framework;

namespace UnitTests
{
	public abstract class PeerTestBase
	{
		protected NetPeerConfiguration Config;
		protected NetPeer Peer;

		[SetUp]
		public void Setup()
		{
			Config = new NetPeerConfiguration(GetType().Name)
			{
				EnableUPnP = true
			};
			Peer = new NetPeer(Config);
			Peer.LogEvent += PeerOnLogEvent;
			Peer.Start();

			TestContext.Out.WriteLine("Unique identifier is " + NetUtility.ToHexString(Peer.UniqueIdentifier));
		}

		private static void PeerOnLogEvent(NetIncomingMessageType type, string text)
		{
			TestContext.Out.WriteLine($"{type}: {text}");
		}

		[TearDown]
		public void TearDown()
		{
			Peer.Shutdown("Unit test teardown.");
		}

		protected static NetIncomingMessage CreateIncomingMessage(byte[] fromData, int bitLength)
		{
			return new NetIncomingMessage
			{
				m_data = fromData,
				m_bitLength = bitLength
			};
		}

		protected static string ToBinaryString(ulong value, int bits, bool includeSpaces)
		{
			var numSpaces = Math.Max(0, bits / 8 - 1);
			if (includeSpaces == false)
			{
				numSpaces = 0;
			}

			var bdr = new StringBuilder(bits + numSpaces);
			for (var i = 0; i < bits + numSpaces; i++)
			{
				bdr.Append(' ');
			}

			for (var i = 0; i < bits; i++)
			{
				var shifted = value >> i;
				var isSet = (shifted & 1) != 0;

				var pos = bits - 1 - i;
				if (includeSpaces)
				{
					pos += Math.Max(0, pos / 8);
				}

				bdr[pos] = isSet ? '1' : '0';
			}

			return bdr.ToString();
		}
	}
}