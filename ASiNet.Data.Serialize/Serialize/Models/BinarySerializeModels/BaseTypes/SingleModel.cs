﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASiNet.Data.Serialize.Interfaces;

namespace ASiNet.Data.Serialize.Models.BinarySerializeModels.BaseTypes;
public class SingleModel : BaseSerializeModel<float>
{
    public override float Deserealize(ISerializeReader reader)
    {
        if (reader.CanReadSize(sizeof(float)))
        {
            var buffer = (stackalloc byte[sizeof(float)]);
            reader.ReadBytes(buffer);
            return BitConverter.ToSingle(buffer);
        }
        throw new Exception();
    }

    public override object? Deserialize(ISerializeReader reader)
    {
        if (reader.CanReadSize(sizeof(float)))
        {
            var buffer = (stackalloc byte[sizeof(float)]);
            reader.ReadBytes(buffer);
            return BitConverter.ToSingle(buffer);
        }
        throw new Exception();
    }

    public override void Serealize(float obj, ISerializerWriter writer)
    {
        var buffer = (stackalloc byte[sizeof(float)]);
        if (obj.TryToBytes(buffer))
        {
            writer.WriteBytes(buffer);
            return;
        }
        throw new Exception();
    }

    public override void Serialize(object? obj, ISerializerWriter writer)
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
}