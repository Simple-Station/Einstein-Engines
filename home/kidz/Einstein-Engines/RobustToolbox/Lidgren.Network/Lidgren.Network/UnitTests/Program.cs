using System;
using System.Reflection;
using Lidgren.Network;
using System.Net;
using System.Net.Sockets;

namespace UnitTests
{
	class Program
	{
		/*static void Main(string[] args)
		{
			NetPeerConfiguration config = new NetPeerConfiguration("unittests");
			config.EnableUPnP = true;
			NetPeer peer = new NetPeer(config);
			peer.Start(); // needed for initialization

			Console.WriteLine("Unique identifier is " + NetUtility.ToHexString(peer.UniqueIdentifier));

			ReadWriteTests.Run(peer);

			NetQueueTests.Run();

			MiscTests.Run();

			BitVectorTests.Run();

			EncryptionTests.Run(peer);

			var om = peer.CreateMessage();
			peer.SendUnconnectedMessage(om, new IPEndPoint(IPAddress.Loopback, 14242));
			try
			{
				peer.SendUnconnectedMessage(om, new IPEndPoint(IPAddress.Loopback, 14242));
			}
			catch (NetException nex)
			{
				if (nex.Message != "This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently")
					throw;
			}

			peer.Shutdown("bye");

			// read all message
			NetIncomingMessage inc = peer.WaitMessage(5000);
			while (inc != null)
			{
				switch (inc.MessageType)
				{
					case NetIncomingMessageType.DebugMessage:
					case NetIncomingMessageType.VerboseDebugMessage:
					case NetIncomingMessageType.WarningMessage:
					case NetIncomingMessageType.ErrorMessage:
						Console.WriteLine("Peer message: " + inc.ReadString());
						break;
					case NetIncomingMessageType.Error:
						throw new Exception("Received error message!");
				}

				inc = peer.ReadMessage();
			}

			Console.WriteLine("Done");
		}*/
	}
}
