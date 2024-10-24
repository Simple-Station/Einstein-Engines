using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace Lidgren.Network
{
	internal static class NetReservedAddress
	{
		private static int IP(byte a, byte b, byte c, byte d) => (a << 24) | (b << 16) | (c << 8) | d;

		// From miniupnpc
		public static readonly (int ip, int mask)[] ReservedRanges =
		{
			// @formatter:off
			(IP(0,   0,   0,   0), 8 ), // RFC1122 "This host on this network"
			(IP(10,  0,   0,   0), 8 ), // RFC1918 Private-Use
			(IP(100, 64,  0,   0), 10), // RFC6598 Shared Address Space
			(IP(127, 0,   0,   0), 8 ), // RFC1122 Loopback
			(IP(169, 254, 0,   0), 16), // RFC3927 Link-Local
			(IP(172, 16,  0,   0), 12), // RFC1918 Private-Use
			(IP(192, 0,   0,   0), 24), // RFC6890 IETF Protocol Assignments
			(IP(192, 0,   2,   0), 24), // RFC5737 Documentation (TEST-NET-1)
			(IP(192, 31,  196, 0), 24), // RFC7535 AS112-v4
			(IP(192, 52,  193, 0), 24), // RFC7450 AMT
			(IP(192, 88,  99,  0), 24), // RFC7526 6to4 Relay Anycast
			(IP(192, 168, 0,   0), 16), // RFC1918 Private-Use
			(IP(192, 175, 48,  0), 24), // RFC7534 Direct Delegation AS112 Service
			(IP(198, 18,  0,   0), 15), // RFC2544 Benchmarking
			(IP(198, 51,  100, 0), 24), // RFC5737 Documentation (TEST-NET-2)
			(IP(203, 0,   113, 0), 24), // RFC5737 Documentation (TEST-NET-3)
			(IP(224, 0,   0,   0), 4 ), // RFC1112 Multicast
			(IP(240, 0,   0,   0), 4 ), // RFC1112 Reserved for Future Use + RFC919 Limited Broadcast
			// @formatter:on
		};

		public static bool IsAddressReserved(IPAddress address)
		{
			if (address.AddressFamily != AddressFamily.InterNetwork)
				return false;

			Span<byte> ipBitsByte = stackalloc byte[4];
			address.TryWriteBytes(ipBitsByte, out _);
			var ipBits = BinaryPrimitives.ReadInt32BigEndian(ipBitsByte);

			foreach (var (reservedIp, maskBits) in ReservedRanges)
			{
				var mask = uint.MaxValue << maskBits;
				if ((ipBits & mask) == (reservedIp & mask))
					return true;
			}

			return false;
		}
	}
}
