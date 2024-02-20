﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASiNet.Data.Serialize.Interfaces;

namespace ASiNet.Data.Serialize.Models.BinarySerializeModels.BaseTypes;
public class UInt16Model : BaseSerializeModel<ushort>
{
    public override ushort Deserealize(ISerializeReader reader)
    {
        if (reader.CanReadSize(sizeof(ushort)))
        {
            var buffer = (stackalloc byte[sizeof(ushort)]);
            reader.ReadBytes(buffer);
            return BitConverter.ToUInt16(buffer);
        }
        throw new Exception();
    }

    public override object? Deserialize(ISerializeReader reader)
    {
        if (reader.CanReadSize(sizeof(ushort)))
        {
            var buffer = (stackalloc byte[sizeof(ushort)]);
            reader.ReadBytes(buffer);
            return BitConverter.ToUInt16(buffer);
        }
        throw new Exception();
    }

    public override void Serealize(ushort obj, ISerializerWriter writer)
    {
        var buffer = (stackalloc byte[sizeof(ushort)]);
        if (obj.TryToBytes(buffer))
        {
            writer.WriteBytes(buffer);
            return;
        }
        throw new Exception();
    }

    public override void Serialize(object? obj, ISerializerWriter writer)
    {
        if (obj is ushort value)
        {
            var buffer = (stackalloc byte[sizeof(ushort)]);
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