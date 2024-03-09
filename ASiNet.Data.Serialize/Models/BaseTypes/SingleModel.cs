﻿using ASiNet.Data.Serialization.Interfaces;

namespace ASiNet.Data.Serialization.Models.BinarySerializeModels.BaseTypes;
public class SingleModel : SerializeModelBase<float>
{
    public override float Deserialize(in ISerializeReader reader)
    {
        if (reader.CanReadSize(sizeof(float)))
        {
            var buffer = (stackalloc byte[sizeof(float)]);
            reader.ReadBytes(buffer);
            return BitConverter.ToSingle(buffer);
        }
        throw new Exception();
    }

    public override object? DeserializeToObject(in ISerializeReader reader)
    {
        if (reader.CanReadSize(sizeof(float)))
        {
            var buffer = (stackalloc byte[sizeof(float)]);
            reader.ReadBytes(buffer);
            return BitConverter.ToSingle(buffer);
        }
        throw new Exception();
    }

    public override void Serialize(float obj, in ISerializeWriter writer)
    {
        var buffer = (stackalloc byte[sizeof(float)]);
        if (obj.TryToBytes(buffer))
        {
            writer.WriteBytes(buffer);
            return;
        }
        throw new Exception();
    }

    public override void SerializeObject(object? obj, in ISerializeWriter writer)
    {
        if (obj is float value)
        {
            var buffer = (stackalloc byte[sizeof(float)]);
            if (value.TryToBytes(buffer))
            {
                writer.WriteBytes(buffer);
                return;
            }
            throw new Exception();
        }
        throw new Exception();
    }


    public override int ObjectSerializedSize(float obj) => sizeof(float);
}