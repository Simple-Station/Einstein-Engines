using System;
using System.Threading;

#if !__NOIPENDPOINT__
using NetAddress = System.Net.IPAddress;
#endif

namespace Lidgren.Network;

//
// This file contains stubs for utilities with special implementations per-platform (Platform/)
// Note: I couldn't be bothered to update the definitions for platforms other than PlatformWin32.cs,
// so you'll have to do some maintenance to make them compile, I apologize.
//

public static partial class NetUtility
{
	/// <summary>
	/// If available, returns the bytes of the physical (MAC) address for the first usable network interface
	/// </summary>
	/// <seealso cref="GetMyAddress"/>
	public static byte[]? GetMacAddressBytes() => GetMacAddressBytesCore();

	/// <summary>
	/// Get the IPv4 address for broadcast on the local network.
	/// </summary>
	/// <remarks>
	/// This may return either the IPv4 "limited broadcast" address
	/// (<see cref="NetAddress.Broadcast"/>, i.e. <c>255.255.255.255</c>) or a directed broadcast address for the local
	/// network, e.g. <c>192.168.1.255</c>.
	/// </remarks>
	/// <returns>Null if we are unable to detect a suitable network interface.</returns>
	/// <seealso cref="GetCachedBroadcastAddress"/>
	/// <seealso cref="GetMyAddress"/>
	public static NetAddress? GetBroadcastAddress() => GetBroadcastAddressCore();

	/// <summary>
	/// Gets our local IPv4 address (not necessarily external) and subnet mask.
	/// </summary>
	/// <returns>
	/// The IP address we have on the local network.
	/// Null if we are unable to detect a suitable network interface.
	/// </returns>
	/// <seealso cref="GetBroadcastAddress"/>
	/// <seealso cref="GetMacAddressBytes"/>
	public static NetAddress? GetMyAddress(out NetAddress? mask) => GetMyAddressCore(out mask);

	/// <summary>
	/// Pause execution of the current thread for a given amount of milliseconds.
	/// Equivalent to <see cref="Thread.Sleep(int)"/>.
	/// </summary>
	/// <param name="milliseconds">
	/// The duration, in milliseconds, to sleep for.
	/// </param>
	public static void Sleep(int milliseconds) => SleepCore(milliseconds);

	/// <summary>
	/// Create a <see cref="NetAddress"/> instance from an array of bytes.
	/// </summary>
	/// <param name="bytes">
	/// An array of bytes describing the IP address.
	/// </param>
	public static NetAddress CreateAddressFromBytes(byte[] bytes) => CreateAddressFromBytesCore(bytes);

	/// <summary>
	/// Compute the SHA-256 hash of an array of data.
	/// </summary>
	/// <param name="bytes">Array containing the data to hash.</param>
	/// <param name="offset">Position in <paramref name="bytes"/> that the data to hash starts at.</param>
	/// <param name="count">Amount of bytes of data to hash from <paramref name="offset"/>.</param>
	/// <returns>The computed hash.</returns>
	public static byte[] ComputeSHAHash(byte[] bytes, int offset, int count) =>
		ComputeSHAHashCore(bytes, offset, count);

	/// <summary>
	/// Get a seed for initializing a random number generator.
	/// </summary>
	/// <remarks>
	/// This is not cryptographically secure.
	/// </remarks>
	[CLSCompliant(false)]
	[Obsolete("Use NetRandomSeed instead.")]
	public static ulong GetPlatformSeed(int seedInc) => GetPlatformSeedCore(seedInc);

	/// <summary>
	/// Return a stopwatch for the current time.
	/// </summary>
	/// <remarks>
	/// Values represent seconds since the stopwatch started timing.
	/// </remarks>
	public static double Now => NowCore;
}
