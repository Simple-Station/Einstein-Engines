using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NetSerializer
{
	/// <summary>
	/// Represents a type that can serialize a <see cref="List{T}"/> or array, but always deserializes as array.
	/// </summary>
	/// <typeparam name="T">The type of contents stored in the list or array.</typeparam>
	[Serializable]
	public readonly struct NetListAsArray<T>
	{
		/// <summary>
		/// The collection contained by this instance. This can either be an array or a <see cref="List{T}"/>.
		/// </summary>
		public readonly IReadOnlyCollection<T> Value;

		/// <summary>
		/// The span of the contents of the collection.
		/// </summary>
		public ReadOnlySpan<T> Span => Value is List<T> l ? CollectionsMarshal.AsSpan(l) : (T[])Value;

		/// <summary>
		/// If true, <see cref="Value"/> is a non-empty collection.
		/// </summary>
		public bool HasContents => Value is { Count: > 0 };

		public NetListAsArray(T[] array)
		{
			Value = array;
		}

		public NetListAsArray(List<T> array)
		{
			Value = array;
		}

		public static implicit operator NetListAsArray<T>(T[] array) => new(array);
		public static implicit operator NetListAsArray<T>(List<T> array) => new(array);
	}
}
