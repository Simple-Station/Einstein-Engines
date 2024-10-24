﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NetSerializer.TypeSerializers
{
    sealed class HashSetSerializer : IStaticTypeSerializer
    {
        public bool Handles(Type type)
        {
            if (!type.IsGenericType)
                return false;

            var genTypeDef = type.GetGenericTypeDefinition();

            return genTypeDef == typeof(HashSet<>);
        }

        public IEnumerable<Type> GetSubtypes(Type type)
        {
            return new[] {typeof(uint), type.GetGenericArguments()[0].MakeArrayType()};
        }

        public MethodInfo GetStaticWriter(Type type)
        {
            Debug.Assert(type.IsGenericType);

            if (!type.IsGenericType)
                throw new Exception();

            var genTypeDef = type.GetGenericTypeDefinition();
            
            Debug.Assert(genTypeDef == typeof(HashSet<>));

            var containerType = GetType();

            var reader = containerType.GetMethod("WritePrimitive", BindingFlags.Static | BindingFlags.Public);
            
            var genArgs = type.GetGenericArguments();

            reader = reader.MakeGenericMethod(genArgs);

            return reader;
        }

        public MethodInfo GetStaticReader(Type type)
        {
            Debug.Assert(type.IsGenericType);

            if (!type.IsGenericType)
                throw new Exception();

            var genTypeDef = type.GetGenericTypeDefinition();
            
            Debug.Assert(genTypeDef == typeof(HashSet<>));

            var containerType = GetType();

            var reader = containerType.GetMethod("ReadPrimitive", BindingFlags.Static | BindingFlags.Public);
            
            var genArgs = type.GetGenericArguments();

            reader = reader.MakeGenericMethod(genArgs);

            return reader;
        }
        
        public static void WritePrimitive<T>(Serializer serializer, Stream stream, HashSet<T> value)
        {
            if (value == null)
            {
                serializer.Serialize(stream, null);
                return;
            }
            
            var array = new T[value.Count];

            var i = 0;
            foreach (var t in value)
                array[i++] = t;

            serializer.Serialize(stream, array);
        }

        public static void ReadPrimitive<T>(Serializer serializer, Stream stream, out HashSet<T> value)
        {
            var array = (T[])serializer.Deserialize(stream);

            if (array == null)
            {
                value = null;
                return;
            }

            value = new HashSet<T>();

            foreach (var t in array)
                value.Add(t);
        }
    }
}