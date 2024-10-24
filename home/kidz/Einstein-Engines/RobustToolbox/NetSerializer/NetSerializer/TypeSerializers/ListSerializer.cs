using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NetSerializer.TypeSerializers
{
	// Stores lists as plain arrays, meaning that high list capacity does not waste extra space.
	// This does mean list capacity is not transferred but oh well.
	sealed class ListSerializer : IDynamicTypeSerializer
    {
        public bool Handles(Type type)
        {
	        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        public IEnumerable<Type> GetSubtypes(Type type)
        {
            return new[] {typeof(uint), GetListElementType(type)};
        }

        // Yes, this is mostly copy pasted from the array code.

		public void GenerateWriterMethod(Serializer serializer, Type type, ILGenerator il)
		{
			// arg0: Serializer, arg1: Stream, arg2: value

			var elemType = GetListElementType(type);

			var notNullLabel = il.DefineLabel();

			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Brtrue_S, notNullLabel);

			// if value == null, write 0
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldc_I4_0);
			//il.Emit(OpCodes.Tailcall);
			il.Emit(OpCodes.Call, serializer.GetDirectWriter(typeof(uint)));
			il.Emit(OpCodes.Ret);

			il.MarkLabel(notNullLabel);

			// write array len + 1
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Callvirt, ListCountGetter(type));
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Add);
			il.Emit(OpCodes.Call, serializer.GetDirectWriter(typeof(uint)));

			// declare i
			var idxLocal = il.DeclareLocal(typeof(int));

			// i = 0
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Stloc_S, idxLocal);

			var loopBodyLabel = il.DefineLabel();
			var loopCheckLabel = il.DefineLabel();

			il.Emit(OpCodes.Br_S, loopCheckLabel);

			// loop body
			il.MarkLabel(loopBodyLabel);

			var data = serializer.GetIndirectData(elemType);

			if (data.WriterNeedsInstance)
				il.Emit(OpCodes.Ldarg_0);

			// write element at index i
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Ldloc_S, idxLocal);
			il.Emit(OpCodes.Callvirt, ListIndexProp(type).GetMethod);

			il.Emit(OpCodes.Call, data.WriterMethodInfo);

			// i = i + 1
			il.Emit(OpCodes.Ldloc_S, idxLocal);
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Add);
			il.Emit(OpCodes.Stloc_S, idxLocal);

			il.MarkLabel(loopCheckLabel);

			// loop condition
			il.Emit(OpCodes.Ldloc_S, idxLocal);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Callvirt, ListCountGetter(type));
			il.Emit(OpCodes.Conv_I4);
			il.Emit(OpCodes.Blt_S, loopBodyLabel);

			il.Emit(OpCodes.Ret);
		}

		public void GenerateReaderMethod(Serializer serializer, Type type, ILGenerator il)
		{
			// arg0: Serializer, arg1: stream, arg2: out value

			var elemType = GetListElementType(type);

			var lenLocal = il.DeclareLocal(typeof(uint));

			// read array len
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldloca_S, lenLocal);
			il.Emit(OpCodes.Call, serializer.GetDirectReader(typeof(uint)));

			var notNullLabel = il.DefineLabel();

			/* if len == 0, return null */
			il.Emit(OpCodes.Ldloc_S, lenLocal);
			il.Emit(OpCodes.Brtrue_S, notNullLabel);

			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Stind_Ref);
			il.Emit(OpCodes.Ret);

			il.MarkLabel(notNullLabel);

			var listLocal = il.DeclareLocal(type);

			// -- length
			il.Emit(OpCodes.Ldloc_S, lenLocal);
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Sub);
			il.Emit(OpCodes.Stloc, lenLocal);

			// create new list with len capacity.
			il.Emit(OpCodes.Ldloc_S, lenLocal);
			il.Emit(OpCodes.Conv_I4);
			il.Emit(OpCodes.Newobj, type.GetConstructor(new []{typeof(int)}));
			il.Emit(OpCodes.Stloc_S, listLocal);

			// declare i
			var idxLocal = il.DeclareLocal(typeof(int));

			// i = 0
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Stloc_S, idxLocal);

			var loopBodyLabel = il.DefineLabel();
			var loopCheckLabel = il.DefineLabel();

			il.Emit(OpCodes.Br_S, loopCheckLabel);

			// loop body
			il.MarkLabel(loopBodyLabel);

			// read element to arr[i]

			var data = serializer.GetIndirectData(elemType);

			var tempValueLocal = il.DeclareLocal(elemType);

			if (data.ReaderNeedsInstance)
				il.Emit(OpCodes.Ldarg_0);

			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldloca_S, tempValueLocal);

			il.Emit(OpCodes.Call, data.ReaderMethodInfo);

			il.Emit(OpCodes.Ldloc_S, listLocal);
			il.Emit(OpCodes.Ldloc_S, tempValueLocal);
			il.Emit(OpCodes.Callvirt, ListAddMethod(type));

			// i = i + 1
			il.Emit(OpCodes.Ldloc_S, idxLocal);
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Add);
			il.Emit(OpCodes.Stloc_S, idxLocal);

			il.MarkLabel(loopCheckLabel);

			// loop condition
			il.Emit(OpCodes.Ldloc_S, idxLocal);
			il.Emit(OpCodes.Ldloc_S, lenLocal);
			il.Emit(OpCodes.Conv_I4);
			il.Emit(OpCodes.Blt_S, loopBodyLabel);


			// store new array to the out value
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Ldloc_S, listLocal);
			il.Emit(OpCodes.Stind_Ref);

			il.Emit(OpCodes.Ret);
		}

		private static PropertyInfo ListIndexProp(Type listType)
		{
			return listType.GetProperties().Single(p =>
			{
				var idxParams = p.GetIndexParameters();
				return idxParams.Length == 1 && idxParams[0].ParameterType == typeof(int);
			});
		}

		public static MethodInfo ListAddMethod(Type listType)
		{
			return listType.GetMethod("Add", new[] {GetListElementType(listType)});
		}

		public static MethodInfo ListCountGetter(Type listType)
		{
			return listType.GetProperty(nameof(List<int>.Count)).GetMethod;
		}

		private static Type GetListElementType(Type t)
		{
			Debug.Assert(t.GetGenericTypeDefinition() == typeof(List<>));

			return t.GetGenericArguments()[0];
		}
    }
}
