﻿using ASiNet.Data.Serialization.Interfaces;
using ASiNet.Data.Serialization.IO.Arrays;

namespace ASiNet.Data.Serialization;

public class BinarySerializer<TContext>(TContext context) : IBinarySerializer where TContext : ISerializerContext
{

    public ISerializerContext Context => SerializeContext;

    private TContext SerializeContext { get; init; } = context;

    public int GetSize<T>(T obj) =>
        SerializeContext.GetOrGenerate<T>().ObjectSerializedSize(obj, Context);

    public int Serialize<T>(T obj, byte[] buffer)
        => Serialize<T>(obj, (ISerializeWriter)(ArrayWriter)buffer);

    public int Serialize<T>(T obj, in ISerializeWriter writer)
    {
        var model = SerializeContext.GetOrGenerate<T>();
        model.Serialize(obj, writer, Context);
        return writer.FilledBytes;
    }

    public T? Deserialize<T>(byte[] buffer)
        => Deserialize<T>((ISerializeReader)(ArrayReader)buffer);

    public T? Deserialize<T>(in ISerializeReader reader)
    {
        var model = SerializeContext.GetOrGenerate<T>();
        return model.Deserialize(reader, Context);
    }
}