﻿using ASiNet.Data.Serialization.Interfaces;

namespace ASiNet.Data.Serialization.Models.BinarySerializeModels.BaseTypes;
public class Int32Model : SerializeModelBase<int>
{
    public override int Deserialize(in ISerializeReader reader, ISerializerContext context)
    {
        if (reader.CanReadSize(sizeof(int)))
        {
            var buffer = (stackalloc byte[sizeof(int)]);
            reader.ReadBytes(buffer);
            return BitConverter.ToInt32(buffer);
        }
        throw new Exception();
    }

    public override object? DeserializeToObject(in ISerializeReader reader, ISerializerContext context)
    {
        if (reader.CanReadSize(sizeof(int)))
        {
            var buffer = (stackalloc byte[sizeof(int)]);
            reader.ReadBytes(buffer);
            return BitConverter.ToInt32(buffer);
        }
        throw new Exception();
    }

    public override void Serialize(int obj, in ISerializeWriter writer, ISerializerContext context)
    {
        var buffer = (stackalloc byte[sizeof(int)]);
        if (obj.TryToBytes(buffer))
        {
            writer.WriteBytes(buffer);
            return;
        }
        throw new Exception();
    }

    public override void SerializeObject(object? obj, in ISerializeWriter writer, ISerializerContext context)
    {
        if (obj is int value)
        {
            var buffer = (stackalloc byte[sizeof(int)]);
            if (value.TryToBytes(buffer))
            {
                writer.WriteBytes(buffer);
                return;
            }
            throw new Exception();
        }
        throw new Exception();
    }

    public override int ObjectSerializedSize(int obj, ISerializerContext context) => sizeof(int);

    public override int ObjectSerializedSize(object obj, ISerializerContext context) => sizeof(int);
}
