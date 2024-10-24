/* Copyright (c) 2010 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.

*/
//#define USE_RELEASE_STATISTICS

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

#if !__NOIPENDPOINT__
using NetEndPoint = System.Net.IPEndPoint;
#endif

namespace Lidgren.Network
{
	public partial class NetPeer
	{
		private readonly List<DelayedPacket> m_delayedPackets = new List<DelayedPacket>();
		private readonly MWCRandom m_latencyRandom = new MWCRandom();

		private sealed class DelayedPacket
		{
			public byte[] Data;
			public double DelayedUntil;
			public NetEndPoint Target;

			public DelayedPacket(byte[] data, double delayedUntil, NetEndPoint target)
			{
				Data = data;
				DelayedUntil = delayedUntil;
				Target = target;
			}
		}

		internal void SendPacket(int numBytes, NetEndPoint target, int numMessages, out bool connectionReset)
		{
			connectionReset = false;

			// simulate loss
			float loss = m_configuration.m_loss;
			if (loss > 0.0f)
			{
				if ((float)m_latencyRandom.NextDouble() < loss)
				{
					LogVerbose("Sending packet " + numBytes + " bytes - SIMULATED LOST!");
					return; // packet "lost"
				}
			}

			m_statistics.PacketSent(numBytes, numMessages);

			// simulate latency
			float m = m_configuration.m_minimumOneWayLatency;
			float r = m_configuration.m_randomOneWayLatency;
			if (m == 0.0f && r == 0.0f)
			{
				// no latency simulation
				// LogVerbose("Sending packet " + numBytes + " bytes");
				bool wasSent = ActuallySendPacket(m_sendBuffer, numBytes, target, out connectionReset);
				// TODO: handle wasSent == false?

				if (m_configuration.m_duplicates > 0.0f && m_latencyRandom.NextDouble() < m_configuration.m_duplicates)
					ActuallySendPacket(m_sendBuffer, numBytes, target, out connectionReset); // send it again!

				return;
			}

			int num = 1;
			if (m_configuration.m_duplicates > 0.0f && m_latencyRandom.NextSingle() < m_configuration.m_duplicates)
				num++;

			for (int i = 0; i < num; i++)
			{
				float delay = m + (m_latencyRandom.NextSingle() * r);

				// Enqueue delayed packet
				DelayedPacket p = new DelayedPacket(new byte[numBytes], NetTime.Now + delay, target);

				Buffer.BlockCopy(m_sendBuffer, 0, p.Data, 0, numBytes);

				m_delayedPackets.Add(p);
			}

			// LogVerbose("Sending packet " + numBytes + " bytes - delayed " + NetTime.ToReadable(delay));
		}

		private void SendDelayedPackets()
		{
			if (m_delayedPackets.Count <= 0)
				return;

			double now = NetTime.Now;


			for (var i = 0; i < m_delayedPackets.Count; i++)
			{
				var p = m_delayedPackets[i];
				if (now < p.DelayedUntil)
					continue;
				ActuallySendPacket(p.Data, p.Data.Length, p.Target, out _);

				// Swap packet with last entry in list.
				// This does not preserve order (we don't care) but is O(1).
				var replaceIdx = m_delayedPackets.Count - 1;
				var replacement = m_delayedPackets[replaceIdx];
				m_delayedPackets[i] = replacement;
				m_delayedPackets.RemoveAt(replaceIdx);

				// Make sure to decrement i so we re-process the element we just swapped in.
				i -= 1;
			}
		}

		private void FlushDelayedPackets()
		{
			try
			{
				foreach (DelayedPacket p in m_delayedPackets)
					ActuallySendPacket(p.Data, p.Data.Length, p.Target, out bool connectionReset);
				m_delayedPackets.Clear();
			}
			catch { }
		}

		//Avoids allocation on mapping to IPv6
		private readonly IPEndPoint targetCopy = new IPEndPoint(IPAddress.Any, 0);
		private readonly IPEndPoint targetCopy2 = new IPEndPoint(IPAddress.Any, 0);

		internal bool ActuallySendPacket(byte[] data, int numBytes, NetEndPoint target, out bool connectionReset)
		{
			var dualStack = m_configuration.DualStack && m_configuration.LocalAddress.AddressFamily == AddressFamily.InterNetworkV6;
			connectionReset = false;
			IPAddress? ba = default(IPAddress);

			NetException.Assert(m_socket != null);

			try
			{
				var realTarget = target;
				ba = NetUtility.GetCachedBroadcastAddress();

				// TODO: refactor this check outta here
				if (target.Address.Equals(ba))
				{
					// Some networks do not allow 
					// a global broadcast so we use the BroadcastAddress from the configuration
					// this can be resolved to a local broadcast addresss e.g 192.168.x.255                    
					targetCopy.Address = m_configuration.BroadcastAddress;
					targetCopy.Port = target.Port;
					realTarget = targetCopy;
					m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

					if (dualStack)
					{
						NetUtility.CopyEndpoint(realTarget, targetCopy2); //Maps to IPv6 for Dual Mode
						realTarget = targetCopy2;
					}

				}
				else if (dualStack)
				{
					NetUtility.CopyEndpoint(target, targetCopy); //Maps to IPv6 for Dual Mode
					realTarget = targetCopy;
				}

				int bytesSent = NetFastSocket.SendTo(m_socket, data, 0, numBytes, SocketFlags.None, realTarget);
				if (numBytes != bytesSent)
					LogWarning($"Failed to send the full {numBytes}; only {bytesSent} bytes sent in packet!");

				// LogDebug("Sent " + numBytes + " bytes");
			}
			catch (SocketException sx)
			{
				if (sx.SocketErrorCode == SocketError.WouldBlock)
				{
					// send buffer full?
					LogWarning("Socket threw exception; would block - send buffer full? Increase in NetPeerConfiguration");
					return false;
				}
				if (sx.SocketErrorCode == SocketError.ConnectionReset)
				{
					// connection reset by peer, aka connection forcibly closed aka "ICMP port unreachable" 
					connectionReset = true;
					return false;
				}
				LogError($"Failed to send packet: {sx}");
			}
			catch (Exception ex)
			{
				LogError($"Failed to send packet: {ex}");
			}
			finally
			{
				if (target.Address.Equals(ba))
					m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);
			}
			return true;
		}

		internal bool SendMTUPacket(int numBytes, NetEndPoint target)
		{
			if (!CanAutoExpandMTU)
				throw new NotSupportedException("MTU expansion not currently supported on this operating system");

			NetException.Assert(m_socket != null);

			try
			{
				// NOTE: Socket.DontFragment doesn't work on dual-stack sockets.
				// The equivalent SetSocketOption does work.
				// See: https://github.com/dotnet/runtime/issues/76410
				if (m_socket.DualMode || target.AddressFamily == AddressFamily.InterNetwork)
					m_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, true);

				int bytesSent = NetFastSocket.SendTo(m_socket, m_sendBuffer, 0, numBytes, SocketFlags.None, target);
				if (numBytes != bytesSent)
					LogWarning($"Failed to send the full {numBytes}; only {bytesSent} bytes sent in packet!");

				m_statistics.PacketSent(numBytes, 1);
			}
			catch (SocketException sx)
			{
				if (sx.SocketErrorCode == SocketError.MessageSize)
					return false;
				if (sx.SocketErrorCode == SocketError.WouldBlock)
				{
					// send buffer full?
					LogWarning("Socket threw exception; would block - send buffer full? Increase in NetPeerConfiguration");
					return true;
				}
				if (sx.SocketErrorCode == SocketError.ConnectionReset)
					return true;
				LogError($"Failed to send packet: ({sx.SocketErrorCode}) {sx}");
			}
			catch (Exception ex)
			{
				LogError($"Failed to send packet: {ex}");
			}
			finally
			{
				if (m_socket.DualMode || target.AddressFamily == AddressFamily.InterNetwork)
					m_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, false);
			}
			return true;
		}

		// CoreCLR can set DontFragment on Windows and Linux, as far as I've tested.
		// macOS doesn't work yet, probably due to recency of the relevant flags (Big Sur).
		private static bool CanAutoExpandMTU => NetNativeSocket.IsLinux || NetNativeSocket.IsWindows;
	}
}
