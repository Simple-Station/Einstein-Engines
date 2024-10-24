#if NET8_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace Lidgren.Network;

/// <summary>
/// Implements an exporter for exporting statistics about the <see cref="NetPeer"/> into
/// <c>System.Diagnostics.Metrics</c>.
/// </summary>
public static class NetPeerMetrics
{
	// I originally had this in NetPeerConfiguration,
	// but moved it here to a standalone class so that trimming solutions avoid pulling in the
	// metrics stack if you don't use it.

	private const string MeterName = "Lidgren.Network.NetPeer";

	/// <summary>
	/// Start exporting metrics for a <see cref="NetPeer"/> to <c>System.Diagnostics.Metrics</c>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This function must be called after the <see cref="NetPeer"/> has been started.
	/// </para>
	/// <para>
	/// Exported metrics can be viewed via tools such as <see cref="MeterListener"/> or <c>dotnet-counters</c>.
	/// </para>
	/// <para>
	/// If exporting metrics from multiple peers,
	/// you should use <paramref name="tags"/> to allow them to be distinguished.
	/// </para>
	/// </remarks>
	/// <param name="netPeer">The <see cref="NetPeer"/> to start exporting metrics for.</param>
	/// <param name="flags">Flags describing what metric features to enable.</param>
	/// <param name="tags">Optional tags to add to the created <see cref="Meter"/>.</param>
	/// <param name="factory">
	/// An optional <see cref="IMeterFactory"/> to be used to create the <see cref="Meter"/>.
	/// </param>
	/// <exception cref="InvalidOperationException">Thrown if the <see cref="NetPeer"/> is not running.</exception>
	public static void AddMetrics(
		this NetPeer netPeer,
		Flags flags = Flags.Statistics,
		IEnumerable<KeyValuePair<string, object?>>? tags = null,
		IMeterFactory? factory = null)
	{
		lock (netPeer.m_initializeLock)
		{
			if (netPeer.Status != NetPeerStatus.Running)
				throw new InvalidOperationException("Peer must be running.");

			Meter meter;
			if (factory != null)
			{
				meter = factory.Create(MeterName, tags: tags);
			}
			else
			{
				meter = new Meter(MeterName, null, tags: tags);
			}

			if ((flags & Flags.Statistics) != 0)
			{
				var statistics = netPeer.Statistics;

				meter.CreateObservableCounter(
					"packets_sent",
					() => statistics.SentPackets,
					null,
					"Number of packets sent by this NetPeer.");

				meter.CreateObservableCounter(
					"packets_received",
					() => statistics.ReceivedPackets,
					null,
					"Number of packets received by this NetPeer.");

				meter.CreateObservableCounter(
					"messages_sent",
					() => statistics.SentMessages,
					null,
					"Number of messages sent by this NetPeer.");

				meter.CreateObservableCounter(
					"messages_received",
					() => statistics.ReceivedMessages,
					null,
					"Number of messages received by this NetPeer.");

				meter.CreateObservableCounter(
					"sent_bytes",
					() => statistics.SentBytes,
					"bytes",
					"Number of bytes sent by this NetPeer.");

				meter.CreateObservableCounter(
					"sent_bytes",
					() => statistics.ReceivedBytes,
					"bytes",
					"Number of bytes received by this NetPeer.");

				meter.CreateObservableCounter(
					"storage_allocated_bytes",
					() => statistics.StorageBytesAllocated,
					"bytes",
					"Number of bytes allocated (and possibly garbage collected) for message storage.");

				meter.CreateObservableUpDownCounter(
					"recycle_pool_bytes",
					() => statistics.BytesInRecyclePool,
					"bytes",
					"Number of bytes in the recycled pool.");

				meter.CreateObservableCounter(
					"messages_resent",
					() => new List<Measurement<long>>
					{
						new(statistics.ResentMessagesDueToHole,
							tags: [new KeyValuePair<string, object?>("drop_reason", "hole")]),
						new(statistics.ResentMessagesDueToDelay,
							tags: [new KeyValuePair<string, object?>("drop_reason", "delay")]),
					},
					null,
					"Number of messages that had to be resent.");

				meter.CreateObservableCounter(
					"messages_dropped",
					() => statistics.DroppedMessages,
					null,
					"Number of messages that were dropped");
			}

            netPeer.m_onShutdown += () => meter.Dispose();
		}
	}

	/// <summary>
	/// Flag enum describing what metric features to enable.
	/// </summary>
	[Flags]
	public enum Flags
	{
		/// <summary>
		/// No metrics are enabled.
		/// </summary>
		None = 0,

		/// <summary>
		/// Basic statistics from <see cref="NetPeerStatistics"/> are reported.
		/// </summary>
		Statistics = 1 << 0,
	}
}

#endif
