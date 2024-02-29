﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASiNet.Data.Serialization.Interfaces;
using ASiNet.Data.Serialization.Models;

namespace ASiNet.Data.Serialization.Generators;
internal class DictionaryModelsGenerator : IModelsGenerator
{
    public SerializeModel<T> GenerateModel<T>(SerializerContext serializeContext, in GeneratorsSettings settings)
    {
        return (SerializeModel<T>)Activator.CreateInstance(typeof(DictionaryModel<>).MakeGenericType(typeof(T)))!;
    }


    public DeserializeObjectDelegate<T> GenerateDeserializeLambda<T>(SerializerContext serializeContext, in GeneratorsSettings settings)
    {
        throw new NotImplementedException();
    }

    public SerializeObjectDelegate<T> GenerateSerializeLambda<T>(SerializerContext serializeContext, in GeneratorsSettings settings)
    {
        throw new NotImplementedException();
    }

    public GetObjectSizeDelegate<T> GenerateGetSerializedObjectSizeDelegate<T>(SerializerContext serializeContext, in GeneratorsSettings settings)
    {
        throw new NotImplementedException();
    }
}
