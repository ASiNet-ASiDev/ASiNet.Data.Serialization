﻿using System;
using ASiNet.Data.Serialization.Hash;
using ASiNet.Data.Serialization.Interfaces;

namespace ASiNet.Data.Serialization.Models
{
    public class SerializeModel<T> : ISerializeModel<T>
    {

        public SerializeModel(SerializeObjectDelegate<T>? serialize = null,
               DeserializeObjectDelegate<T>? deserialize = null,
               GetObjectSizeDelegate<T>? getSize = null)
        {
            _serializeDelegate = serialize;
            _deserializeDelegate = deserialize;
            _getSizeDelegate = getSize;
        }

        public long TypeHash => _typeHash.Value.Hash;
        public byte[] TypeHashBytes => _typeHash.Value.BytesHash;

        public Type ObjType => _objType.Value;

        private readonly Lazy<Type> _objType = new Lazy<Type>(() => typeof(T));

        private readonly Lazy<(byte[] BytesHash, long Hash)> _typeHash = new Lazy<(byte[] BytesHash, long Hash)>(() =>
        {
            var hash = PolynomialHasher.Shared.CalculateHash(typeof(T).FullName ?? typeof(T).Name);
            var bytes = BitConverter.GetBytes(hash);
            return (bytes, hash);
        });

        public virtual bool ContainsSerializeDelegate => _serializeDelegate != null;
        public virtual bool ContainsDeserializeDelegate => _deserializeDelegate != null;
        public virtual bool ContainsGetSizeDelegate => _deserializeDelegate != null;


        private SerializeObjectDelegate<T>? _serializeDelegate;
        private DeserializeObjectDelegate<T>? _deserializeDelegate;
        private GetObjectSizeDelegate<T>? _getSizeDelegate;

        public virtual void SerializeObject(object? obj, in ISerializeWriter writer)
        {
            if (_serializeDelegate is null)
                throw new NullReferenceException();
            if (obj is T value)
                _serializeDelegate(value, writer);
        }

        public virtual object? DeserializeToObject(in ISerializeReader reader)
        {
            if (_deserializeDelegate is null)
                throw new NullReferenceException();
            return _deserializeDelegate(reader);
        }

        public virtual void Serialize(T obj, in ISerializeWriter writer)
        {
            if (_serializeDelegate is null)
                throw new NullReferenceException();
            _serializeDelegate(obj, writer);
        }

        public virtual T Deserialize(in ISerializeReader reader)
        {
            if (_deserializeDelegate is null)
                throw new NullReferenceException();
            return _deserializeDelegate(reader);
        }

        public void SetSerializeDelegate(SerializeObjectDelegate<T> set) =>
            _serializeDelegate = set;
        public void SetDeserializeDelegate(DeserializeObjectDelegate<T> get) =>
            _deserializeDelegate = get;

        public void SetGetSizeDelegate(GetObjectSizeDelegate<T> get) =>
            _getSizeDelegate = get;

        public virtual int ObjectSerializedSize(object? obj) =>
            ObjectSerializedSize((T)obj);

        public virtual int ObjectSerializedSize(T obj)
        {
            if (_getSizeDelegate is null)
                throw new NullReferenceException();
            return _getSizeDelegate(obj);
        }

        public void Dispose()
        {
            _serializeDelegate = null;
            _deserializeDelegate = null;
            _getSizeDelegate = null;
            GC.SuppressFinalize(this);
        }
    }

}