#if !__NOIPENDPOINT__
using NetEndPoint = System.Net.IPEndPoint;
using NetAddress = System.Net.IPAddress;
#endif

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Lidgren.Network
{
	public static partial class NetUtility
	{
		/// <summary>
		/// Asynchronous callback raised when a remote <see cref="NetEndPoint"/> has been resolved.
		/// </summary>
		/// <remarks>
		/// This callback is not raised on any particular thread.
		/// </remarks>
		/// <param name="endPoint">
		/// Null if the resolved host name does not exist or does not contain suitable DNS records.
		/// </param>
		public delegate void ResolveEndPointCallback(NetEndPoint? endPoint);

		/// <summary>
		/// Resolve address callback
		/// </summary>
		/// <remarks>
		/// This callback is not raised on any particular thread.
		/// </remarks>
		/// <param name="adr">
		/// Null if the resolved host name does not exist or does not contain suitable DNS records.
		/// </param>
		public delegate void ResolveAddressCallback(NetAddress? adr);

		/// <summary>
		/// Resolve an IP address or hostname into a <see cref="NetEndPoint"/>. (asynchronous callback version)
		/// </summary>
		/// <remarks>
		/// <para>
		///	This function can accept host names or direct IP addresses
		/// (in standard notation, i.e. <c>xxx.xxx.xxx.xxx</c> for IPv4 or <c>xxxx:xxxx:...:xxxx</c> for IPv6).
		/// If an IP address is given, it is parsed and immediately returned.
		/// </para>
		/// <para>
		/// This is an asynchronous callback version.
		/// Instead of returning a value directly, <paramref name="callback"/> is called with the result.
		/// </para>
		/// </remarks>
		/// <param name="ipOrHost">IP address or host name string to resolve.</param>
		/// <param name="port">Port to use on the returned <see cref="NetEndPoint"/> instance.</param>
		/// <param name="callback">Callback that is ran when resolution completes.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="ipOrHost"/> is empty.</exception>
		/// <exception cref="SocketException">Thrown if a network error occurs.</exception>
		[Obsolete("This function does not handle network errors properly, prefer task-based ResolveAsync instead.")]
		public static void ResolveAsync(string ipOrHost, int port, ResolveEndPointCallback callback)
		{
			ResolveAsync(ipOrHost, port, null, callback);
		}

		/// <summary>
		/// Resolve an IP address or hostname into a <see cref="NetEndPoint"/>. (asynchronous callback version)
		/// </summary>
		/// <remarks>
		/// <para>
		///	This function can accept host names or direct IP addresses
		/// (in standard notation, i.e. <c>xxx.xxx.xxx.xxx</c> for IPv4 or <c>xxxx:xxxx:...:xxxx</c> for IPv6).
		/// If an IP address is given, it is parsed and immediately returned.
		/// </para>
		/// <para>
		/// This is an asynchronous callback version.
		/// Instead of returning a value directly, <paramref name="callback"/> is called with the result.
		/// </para>
		/// </remarks>
		/// <param name="ipOrHost">IP address or host name string to resolve.</param>
		/// <param name="port">Port to use on the returned <see cref="NetEndPoint"/> instance.</param>
		/// <param name="allowedFamily">
		/// If not <see langword="null"/>, only allow the given address family to be returned.
		/// Otherwise, both IPv4 and IPv6 addresses can be returned.
		/// </param>
		/// <param name="callback">Callback that is ran when resolution completes.</param>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="ipOrHost"/> is empty
		/// OR
		/// <paramref name="allowedFamily"/> is not null and not one of <see cref="AddressFamily.InterNetwork"/>
		/// or <see cref="AddressFamily.InterNetworkV6"/>.
		/// </exception>
		/// <exception cref="SocketException">Thrown if a network error occurs.</exception>
		[Obsolete("This function does not handle network errors properly, prefer task-based ResolveAsync instead.")]
		public static void ResolveAsync(string ipOrHost, int port, AddressFamily? allowedFamily,
			ResolveEndPointCallback callback)
		{
			ResolveAsync(ipOrHost, port, allowedFamily).ContinueWith(t => callback(t.Result));
		}

		/// <summary>
		/// Resolve an IP address or hostname into a <see cref="NetEndPoint"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		///	This function can accept host names or direct IP addresses
		/// (in standard notation, i.e. <c>xxx.xxx.xxx.xxx</c> for IPv4 or <c>xxxx:xxxx:...:xxxx</c> for IPv6).
		/// If an IP address is given, it is parsed and immediately returned.
		/// </para>
		/// <para>
		/// This function is synchronous,
		/// prefer using <see cref="M:Lidgren.Network.NetUtility.ResolveAsync(System.String,System.Int32)"/>
		/// to avoid hanging the current thread if the network has to be accessed.
		/// </para>
		/// </remarks>
		/// <param name="ipOrHost">IP address or host name string to resolve.</param>
		/// <param name="port">Port to use on the returned <see cref="NetEndPoint"/> instance.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="ipOrHost"/> is empty.</exception>
		/// <exception cref="SocketException">Thrown if a network error occurs.</exception>
		/// <returns><see langword="null"/> if the given host does not exist.</returns>
		public static NetEndPoint? Resolve(string ipOrHost, int port)
		{
			return Resolve(ipOrHost, port, null);
		}

		/// <summary>
		/// Resolve an IP address or hostname into a <see cref="NetEndPoint"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		///	This function can accept host names or direct IP addresses
		/// (in standard notation, i.e. <c>xxx.xxx.xxx.xxx</c> for IPv4 or <c>xxxx:xxxx:...:xxxx</c> for IPv6).
		/// If an IP address is given, it is parsed and immediately returned.
		/// </para>
		/// <para>
		/// This function is synchronous,
		/// prefer using <see cref="M:Lidgren.Network.NetUtility.ResolveAsync(System.String,System.Int32)"/>
		/// to avoid hanging the current thread if the network has to be accessed.
		/// </para>
		/// </remarks>
		/// <param name="ipOrHost">IP address or host name string to resolve.</param>
		/// <param name="port">Port to use on the returned <see cref="NetEndPoint"/> instance.</param>
		/// <param name="allowedFamily">
		/// If not <see langword="null"/>, only allow the given address family to be returned.
		/// Otherwise, both IPv4 and IPv6 addresses can be returned.
		/// </param>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="ipOrHost"/> is empty
		/// OR
		/// <paramref name="allowedFamily"/> is not null and not one of <see cref="AddressFamily.InterNetwork"/>
		/// or <see cref="AddressFamily.InterNetworkV6"/>.
		/// </exception>
		/// <exception cref="SocketException">Thrown if a network error occurs.</exception>
		/// <returns><see langword="null"/> if the given host does not exist.</returns>
		public static NetEndPoint? Resolve(string ipOrHost, int port, AddressFamily? allowedFamily)
		{
			var adr = Resolve(ipOrHost, allowedFamily);
			return adr == null ? null : new NetEndPoint(adr, port);
		}

		/// <summary>
		/// Resolve an IP address or hostname into a <see cref="NetAddress"/>. (asynchronous callback version)
		/// </summary>
		/// <remarks>
		/// <para>
		///	This function can accept host names or direct IP addresses
		/// (in standard notation, i.e. <c>xxx.xxx.xxx.xxx</c> for IPv4 or <c>xxxx:xxxx:...:xxxx</c> for IPv6).
		/// If an IP address is given, it is parsed and immediately returned.
		/// </para>
		/// <para>
		/// This is an asynchronous callback version.
		/// Instead of returning a value directly, <paramref name="callback"/> is called with the result.
		/// </para>
		/// </remarks>
		/// <param name="ipOrHost">IP address or host name string to resolve.</param>
		/// <param name="callback">Callback that is ran when resolution completes.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="ipOrHost"/> is empty.</exception>
		/// <exception cref="SocketException">Thrown if a network error occurs.</exception>
		[Obsolete("This function does not handle network errors properly, prefer task-based ResolveAsync instead.")]
		public static void ResolveAsync(string ipOrHost, ResolveAddressCallback callback)
		{
			ResolveAsync(ipOrHost, null, callback);
		}

		/// <summary>
		/// Resolve an IP address or hostname into a <see cref="NetAddress"/>. (asynchronous callback version)
		/// </summary>
		/// <remarks>
		/// <para>
		///	This function can accept host names or direct IP addresses
		/// (in standard notation, i.e. <c>xxx.xxx.xxx.xxx</c> for IPv4 or <c>xxxx:xxxx:...:xxxx</c> for IPv6).
		/// If an IP address is given, it is parsed and immediately returned.
		/// </para>
		/// <para>
		/// This is an asynchronous callback version.
		/// Instead of returning a value directly, <paramref name="callback"/> is called with the result.
		/// </para>
		/// </remarks>
		/// <param name="ipOrHost">IP address or host name string to resolve.</param>
		/// <param name="allowedFamily">
		/// If not <see langword="null"/>, only allow the given address family to be returned.
		/// Otherwise, both IPv4 and IPv6 addresses can be returned.
		/// </param>
		/// <param name="callback">Callback that is ran when resolution completes.</param>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="ipOrHost"/> is empty
		/// OR
		/// <paramref name="allowedFamily"/> is not null and not one of <see cref="AddressFamily.InterNetwork"/>
		/// or <see cref="AddressFamily.InterNetworkV6"/>.
		/// </exception>
		/// <exception cref="SocketException">Thrown if a network error occurs.</exception>
		[Obsolete("This function does not handle network errors properly, prefer task-based ResolveAsync instead.")]
		public static void ResolveAsync(string ipOrHost, AddressFamily? allowedFamily, ResolveAddressCallback callback)
		{
			ResolveAsync(ipOrHost, allowedFamily).ContinueWith(t => callback(t.Result));
		}

		/// <summary>
		/// Resolve an IP address or hostname into a <see cref="NetEndPoint"/>. (asynchronous task version)
		/// </summary>
		/// <remarks>
		/// <para>
		///	This function can accept host names or direct IP addresses
		/// (in standard notation, i.e. <c>xxx.xxx.xxx.xxx</c> for IPv4 or <c>xxxx:xxxx:...:xxxx</c> for IPv6).
		/// If an IP address is given, it is parsed and immediately returned.
		/// </para>
		/// </remarks>
		/// <param name="ipOrHost">IP address or host name string to resolve.</param>
		/// <param name="port">Port to use on the returned <see cref="NetEndPoint"/> instance.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="ipOrHost"/> is empty.</exception>
		/// <exception cref="SocketException">Thrown if a network error occurs.</exception>
		/// <returns><see langword="null"/> if the given host does not exist.</returns>
		public static async Task<NetEndPoint?> ResolveAsync(string ipOrHost, int port)
		{
			return await ResolveAsync(ipOrHost, port, (AddressFamily?)null);
		}

		/// <summary>
		/// Resolve an IP address or hostname into a <see cref="NetEndPoint"/>. (asynchronous task version)
		/// </summary>
		/// <remarks>
		/// <para>
		///	This function can accept host names or direct IP addresses
		/// (in standard notation, i.e. <c>xxx.xxx.xxx.xxx</c> for IPv4 or <c>xxxx:xxxx:...:xxxx</c> for IPv6).
		/// If an IP address is given, it is parsed and immediately returned.
		/// </para>
		/// </remarks>
		/// <param name="ipOrHost">IP address or host name string to resolve.</param>
		/// <param name="port">Port to use on the returned <see cref="NetEndPoint"/> instance.</param>
		/// <param name="allowedFamily">
		/// If not <see langword="null"/>, only allow the given address family to be returned.
		/// Otherwise, both IPv4 and IPv6 addresses can be returned.
		/// </param>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="ipOrHost"/> is empty
		/// OR
		/// <paramref name="allowedFamily"/> is not null and not one of <see cref="AddressFamily.InterNetwork"/>
		/// or <see cref="AddressFamily.InterNetworkV6"/>.
		/// </exception>
		/// <exception cref="SocketException">Thrown if a network error occurs.</exception>
		/// <returns><see langword="null"/> if the given host does not exist.</returns>
		public static async Task<NetEndPoint?> ResolveAsync(string ipOrHost, int port, AddressFamily? allowedFamily)
		{
			var adr = await ResolveAsync(ipOrHost, allowedFamily);
			return adr == null ? null : new NetEndPoint(adr, port);
		}

		/// <summary>
		/// Resolve an IP address or hostname into a <see cref="NetAddress"/>. (asynchronous task version)
		/// </summary>
		/// <remarks>
		/// <para>
		///	This function can accept host names or direct IP addresses
		/// (in standard notation, i.e. <c>xxx.xxx.xxx.xxx</c> for IPv4 or <c>xxxx:xxxx:...:xxxx</c> for IPv6).
		/// If an IP address is given, it is parsed and immediately returned.
		/// </para>
		/// </remarks>
		/// <param name="ipOrHost">IP address or host name string to resolve.</param>
		/// <param name="allowedFamily">
		/// If not <see langword="null"/>, only allow the given address family to be returned.
		/// Otherwise, both IPv4 and IPv6 addresses can be returned.
		/// </param>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="ipOrHost"/> is empty
		/// OR
		/// <paramref name="allowedFamily"/> is not null and not one of <see cref="AddressFamily.InterNetwork"/>
		/// or <see cref="AddressFamily.InterNetworkV6"/>.
		/// </exception>
		/// <exception cref="SocketException">Thrown if a network error occurs.</exception>
		/// <returns><see langword="null"/> if the given host does not exist.</returns>
		public static async Task<NetAddress?> ResolveAsync(string ipOrHost, AddressFamily? allowedFamily = null)
		{
			if (ResolveHead(ref ipOrHost, allowedFamily, out var resolve))
			{
				return resolve;
			}

			// ok must be a host name
			try
			{
				var addresses = await Dns.GetHostAddressesAsync(ipOrHost);

				return ResolveFilter(allowedFamily, addresses);
			}
			catch (SocketException ex)
			{
				if (ex.SocketErrorCode == SocketError.HostNotFound)
				{
					//LogWrite(string.Format(CultureInfo.InvariantCulture, "Failed to resolve host '{0}'.", ipOrHost));
					return null;
				}
				else
				{
					throw;
				}
			}

		}

		/// <summary>
		/// Resolve an IP address or hostname into a <see cref="NetAddress"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		///	This function can accept host names or direct IP addresses
		/// (in standard notation, i.e. <c>xxx.xxx.xxx.xxx</c> for IPv4 or <c>xxxx:xxxx:...:xxxx</c> for IPv6).
		/// If an IP address is given, it is parsed and immediately returned.
		/// </para>
		/// <para>
		/// This function is synchronous,
		/// prefer using <see cref="M:Lidgren.Network.NetUtility.ResolveAsync(System.String,System.Int32)"/>
		/// to avoid hanging the current thread if the network has to be accessed.
		/// </para>
		/// </remarks>
		/// <param name="ipOrHost">IP address or host name string to resolve.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="ipOrHost"/> is empty.</exception>
		/// <exception cref="SocketException">Thrown if a network error occurs.</exception>
		/// <returns><see langword="null"/> if the given host does not exist.</returns>
		public static NetAddress? Resolve(string ipOrHost)
		{
			return Resolve(ipOrHost, null);
		}

		/// <summary>
		/// Get IPv4 or IPv6 address from notation (xxx.xxx.xxx.xxx or xxxx:xxxx:...:xxxx) or hostname,
		/// taking in an allowed address family to filter resolved addresses by.
		/// </summary>
		/// <remarks>
		/// If <paramref name="allowedFamily"/> is not null, the address returned will only be of the specified family.
		/// </remarks>
		/// <param name="ipOrHost">The hostname or IP address to parse.</param>
		/// <param name="allowedFamily">If not null, the allowed address family to return.</param>
		/// <returns>
		/// A resolved address matching the specified filter if it exists,
		/// null if no such address exists or a lookup error occured.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="ipOrHost"/> is null or empty OR
		/// <paramref name="allowedFamily"/> is not one of null, <see cref="AddressFamily.InterNetwork"/>
		/// or <see cref="AddressFamily.InterNetworkV6"/>
		/// </exception>
		public static NetAddress? Resolve(string ipOrHost, AddressFamily? allowedFamily)
		{
			if (ResolveHead(ref ipOrHost, allowedFamily, out var resolve))
			{
				return resolve;
			}

			// ok must be a host name
			try
			{
				var addresses = Dns.GetHostAddresses(ipOrHost);

				return ResolveFilter(allowedFamily, addresses);
			}
			catch (SocketException ex) when (ex.SocketErrorCode == SocketError.HostNotFound)
			{
				//LogWrite(string.Format(CultureInfo.InvariantCulture, "Failed to resolve host '{0}'.", ipOrHost));
				return null;
			}
		}

		private static IPAddress? ResolveFilter(AddressFamily? allowedFamily, IPAddress[] addresses)
		{
			foreach (var address in addresses)
			{
				var family = address.AddressFamily;
				if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
					continue;
				if (allowedFamily == null || allowedFamily == family)
					return address;
			}

			return null;
		}

		private static bool ResolveHead(ref string ipOrHost, AddressFamily? allowedFamily, out IPAddress? resolve)
		{
			if (string.IsNullOrEmpty(ipOrHost))
				throw new ArgumentException("Supplied string must not be empty", "ipOrHost");

			if (allowedFamily != null && allowedFamily != AddressFamily.InterNetwork
									  && allowedFamily != AddressFamily.InterNetworkV6)
			{
				throw new ArgumentException("Address family must be either InterNetwork, InterNetworkV6 or null",
					nameof(allowedFamily));
			}

			ipOrHost = ipOrHost.Trim();

			if (NetAddress.TryParse(ipOrHost, out NetAddress? ipAddress))
			{
				if (allowedFamily != null && ipAddress.AddressFamily != allowedFamily)
				{
					resolve = null;
					return true;
				}

				if (ipAddress.AddressFamily == AddressFamily.InterNetwork ||
					ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
				{
					resolve = ipAddress;
					return true;
				}

				throw new ArgumentException("This method will not currently resolve other than IPv4 or IPv6 addresses");
			}

			resolve = null;
			return false;
		}
	}
}