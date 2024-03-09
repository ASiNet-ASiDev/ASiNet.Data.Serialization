using ASiNet.Data.Serialization.Interfaces;

namespace ASiNet.Data.Serialization.Models.BinarySerializeModels.BaseTypes;

public class GuidModel : SerializeModelBase<Guid>
{
    public const int GUID_SIZE = 16;
    
    public override Guid Deserialize(in ISerializeReader reader)
    {
        if (reader.CanReadSize(GUID_SIZE))
        {
            var buffer = (stackalloc byte[GUID_SIZE]);
            reader.ReadBytes(buffer);
            
            return new Guid(buffer);
        }
        throw new Exception();
    }

    public override object? DeserializeToObject(in ISerializeReader reader)
    {
        if (reader.CanReadSize(GUID_SIZE))
        {
            var buffer = (stackalloc byte[GUID_SIZE]);
            reader.ReadBytes(buffer);
            return new Guid(buffer);
        }
        throw new Exception();
    }

    public override void Serialize(Guid obj, in ISerializeWriter writer)
    {
        var buffer = (stackalloc byte[GUID_SIZE]);
        
        if (obj.TryWriteBytes(buffer))
        {
            writer.WriteBytes(buffer);
            return;
        }
        throw new Exception();
    }

    public override void SerializeObject(object? obj, in ISerializeWriter writer)
    {
        if (obj is Guid value)
        {
            var buffer = (stackalloc byte[GUID_SIZE]);
            if (value.TryWriteBytes(buffer))
            {
                writer.WriteBytes(buffer);
                return;
            }
            throw new Exception();
        }
        throw new Exception();
    }

    public override int ObjectSerializedSize(Guid obj) => GUID_SIZE;

}
