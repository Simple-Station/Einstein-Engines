using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;

namespace NetSerializer.TypeSerializers
{
	public sealed class NetListAsArraySerializer : IDynamicTypeSerializer
	{
		public bool Handles(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(NetListAsArray<>);
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			var elementType = GetElementType(type);

			return new[]
			{
				typeof(uint),
				elementType,
				// We use List<T> and T[] serializers in our implementation so...
				elementType.MakeArrayType(),
				typeof(List<>).MakeGenericType(elementType)
			};
		}

		public void GenerateWriterMethod(Serializer serializer, Type type, ILGenerator il)
		{
			// arg0: Serializer, arg1: Stream, arg2: value

			var elementType = GetElementType(type);

			var collectionLocal = il.DeclareLocal(typeof(IReadOnlyCollection<>).MakeGenericType(elementType));

			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Ldfld, type.GetField("Value")!);
			il.Emit(OpCodes.Stloc, collectionLocal);

			var notNullLabel = il.DefineLabel();

			il.Emit(OpCodes.Ldloc, collectionLocal);
			il.Emit(OpCodes.Brtrue_S, notNullLabel);

			// if value == null, write 0
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldc_I4_0);
			//il.Emit(OpCodes.Tailcall);
			il.Emit(OpCodes.Call, serializer.GetDirectWriter(typeof(uint)));
			il.Emit(OpCodes.Ret);

			il.MarkLabel(notNullLabel);

			// List<T> and T[] have the same on-wire format in the current serializers.
			// As such, we can just defer to the serializers for the internal values with some basic type checks.

			var notListLabel = il.DefineLabel();

			var listType = typeof(List<>).MakeGenericType(elementType);
			var listLocal = il.DeclareLocal(listType);
			il.Emit(OpCodes.Ldloc, collectionLocal);
			il.Emit(OpCodes.Isinst, listType);
			il.Emit(OpCodes.Stloc, listLocal);
			il.Emit(OpCodes.Ldloc, listLocal);
			il.Emit(OpCodes.Brfalse_S, notListLabel);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldloc, listLocal);
			il.Emit(OpCodes.Call, serializer.GetDirectWriter(listType));
			il.Emit(OpCodes.Ret);
			il.MarkLabel(notListLabel);

			var arrayType = elementType.MakeArrayType();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldloc, collectionLocal);
			il.Emit(OpCodes.Castclass, arrayType);
			il.Emit(OpCodes.Call, serializer.GetDirectWriter(arrayType));
			il.Emit(OpCodes.Ret);
		}

		public void GenerateReaderMethod(Serializer serializer, Type type, ILGenerator il)
		{
			// arg0: Serializer, arg1: stream, arg2: out value

			var elementType = GetElementType(type);

			// Always deserialize as array so this is just a basic hoop to call the T[] reader and fill in the struct.

			var arrayType = elementType.MakeArrayType();
			var arrayLocal = il.DeclareLocal(arrayType);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldloca, arrayLocal);
			il.Emit(OpCodes.Call, serializer.GetDirectReader(arrayType));
			il.Emit(OpCodes.Ldloc, arrayLocal);
			il.Emit(OpCodes.Newobj, type.GetConstructor(new []{arrayType})!);
			il.Emit(OpCodes.Stobj, type);
			il.Emit(OpCodes.Ret);
		}

		private static Type GetElementType(Type t)
		{
			Debug.Assert(t.GetGenericTypeDefinition() == typeof(NetListAsArray<>));

			return t.GetGenericArguments()[0];
		}
	}
}
