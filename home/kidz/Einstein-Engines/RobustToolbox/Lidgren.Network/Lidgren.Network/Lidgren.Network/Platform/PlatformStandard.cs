#if !__ANDROID__ && !__CONSTRAINED__ && !WINDOWS_RUNTIME && !UNITY_STANDALONE_LINUX
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Lidgren.Network
{
	public static partial class NetUtility
	{
		private static readonly long s_timeInitialized = Stopwatch.GetTimestamp();
		private static readonly double s_dInvFreq = 1.0 / (double)Stopwatch.Frequency;
		
		internal static ulong GetPlatformSeedCore(int seedInc)
		{
			ulong seed = (ulong)System.Diagnostics.Stopwatch.GetTimestamp();
			return seed ^ ((ulong)Environment.WorkingSet + (ulong)seedInc);
		}

		private static double NowCore { get { return (double)(Stopwatch.GetTimestamp() - s_timeInitialized) * s_dInvFreq; } }

		private static NetworkInterface? GetNetworkInterface()
		{
			var defaultAddress = ProbeDefaultRouteAddress();
			
			// Forgive me father for I have LINQ'd.
			return NetworkInterface.GetAllNetworkInterfaces()
				.Where(nic => nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
				              nic.NetworkInterfaceType != NetworkInterfaceType.Unknown &&
				              nic.Supports(NetworkInterfaceComponent.IPv4))
				.OrderByDescending(nic =>
				{
					if (nic.OperationalStatus != OperationalStatus.Up)
						return 0;

					// Try to get an adapter with a proper MAC address.
					// This means it will ignore things like certain VPN tunnels.
					// Also, peer UIDs are generated based off MAC address so not getting an empty one is probably good.
					if (nic.GetPhysicalAddress().GetAddressBytes().Length == 0)
						return 1;

					foreach (var address in nic.GetIPProperties().UnicastAddresses)
					{
						// If this is the adapter for the default address, it wins hands down. 
						if (defaultAddress != null && address.Address.Equals(defaultAddress))
							return 4;
						
						// make sure this adapter has any ipv4 addresses
						if (address is { Address: { AddressFamily: AddressFamily.InterNetwork } })
							return 3;
					}

					return 2;
				})
				.FirstOrDefault();
		}

		private static IPAddress? ProbeDefaultRouteAddress()
		{
			try
			{
				// Try to infer the default network interface by "connecting" a UDP socket to a global IP address.
				// This will not send any real data (like with TCP), but it *will* cause the OS
				// to fill in the local address of the socket with the local address of the interface that would be used,
				// based on the OS routing tables.
				// This basically gets us the network interface address "that goes to the router".
				using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				socket.Connect(new IPAddress(new byte[] { 1, 1, 1, 1 }), 12345);
				return ((IPEndPoint)socket.LocalEndPoint!).Address;
			}
			catch
			{
				// I can't imagine why this would fail but if it does let's just uh...
				return null;
			}
		}

		private static byte[]? GetMacAddressBytesCore()
		{
			var ni = GetNetworkInterface();
			if (ni == null)
				return null;
			return ni.GetPhysicalAddress().GetAddressBytes();
		}

		private static IPAddress? GetBroadcastAddressCore()
		{
			var ni = GetNetworkInterface();
			if (ni == null)
				return null;

			var properties = ni.GetIPProperties();
			foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
			{
				if (unicastAddress != null && unicastAddress.Address != null && unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
				{
					var mask = unicastAddress.IPv4Mask;
					byte[] ipAdressBytes = unicastAddress.Address.GetAddressBytes();
					byte[] subnetMaskBytes = mask.GetAddressBytes();

					if (ipAdressBytes.Length != subnetMaskBytes.Length)
						throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

					byte[] broadcastAddress = new byte[ipAdressBytes.Length];
					for (int i = 0; i < broadcastAddress.Length; i++)
					{
						broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
					}
					return new IPAddress(broadcastAddress);
				}
			}
			return IPAddress.Broadcast;
		}

		private static IPAddress? GetMyAddressCore(out IPAddress? mask)
		{
			var ni = GetNetworkInterface();
			if (ni == null)
			{
				mask = null;
				return null;
			}

			IPInterfaceProperties properties = ni.GetIPProperties();
			foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
			{
				if (unicastAddress != null && unicastAddress.Address != null && unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
				{
					mask = unicastAddress.IPv4Mask;
					return unicastAddress.Address;
				}
			}

			mask = null;
			return null;
		}

		private static void SleepCore(int milliseconds)
		{
			System.Threading.Thread.Sleep(milliseconds);
		}

		private static IPAddress CreateAddressFromBytesCore(byte[] bytes)
		{
			return new IPAddress(bytes);
		}
		
		private static byte[] ComputeSHAHashCore(byte[] bytes, int offset, int count)
		{
			using var sha = SHA256.Create();
			return sha.ComputeHash(bytes, offset, count);
		}
	}

	public static partial class NetTime
	{
		private static long s_timeInitialized = Stopwatch.GetTimestamp();
		private static readonly double s_dInvFreq = 1.0 / (double)Stopwatch.Frequency;

		/// <summary>
		///		Sets <see cref="Now"/> to the current value and track it like normal.
		/// </summary>
		/// <remarks>
		///		You are basically guaranteed to break everything if you use this after a NetPeer has been initialized.
		/// </remarks>
		public static void SetNow(double value)
		{
			s_timeInitialized = -(long) (value / s_dInvFreq - Stopwatch.GetTimestamp());
		}
		
		/// <summary>
		/// Get number of seconds since the application started
		/// </summary>
		public static double Now { get { return (double)(Stopwatch.GetTimestamp() - s_timeInitialized) * s_dInvFreq; } }
	}
}
#endif