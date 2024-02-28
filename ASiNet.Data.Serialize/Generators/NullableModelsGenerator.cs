﻿using System.Linq.Expressions;
using ASiNet.Data.Serialization.Exceptions;
using ASiNet.Data.Serialization.Interfaces;

namespace ASiNet.Data.Serialization.Generators;
public class NullableModelsGenerator : IModelsGenerator
{

    public SerializeModel<T> GenerateModel<T>(SerializerContext serializeContext, in GeneratorsSettings settings)
    {
        try
        {
            var model = new SerializeModel<T>();

            serializeContext.AddModel(model);

            model.SetSerializeDelegate(GenerateSerializeLambda<T>(serializeContext, settings));
            model.SetDeserializeDelegate(GenerateDeserializeLambda<T>(serializeContext, settings));

            return model;
        }
        catch (Exception ex)
        {
            throw new GeneratorException(ex);
        }
    }


    public SerializeObjectDelegate<T> GenerateSerializeLambda<T>(SerializerContext serializeContext, in GeneratorsSettings settings)
    {
        var type = typeof(T);
        var inst = Expression.Parameter(typeof(T), "inst");
        var writer = Expression.Parameter(typeof(ISerializeWriter), "writer");

        var model = SerializerHelper.GetOrGenerateSerializeModelConstant(Nullable.GetUnderlyingType(type)!, serializeContext);

        var body = Expression.IfThenElse(
            HashValue(inst),
            Expression.Block(
                SerializerHelper.WriteNullableByte(writer, 1),
                SerializerHelper.CallSerialize(
                    model,
                    Value(inst),
                    writer)
                ),
            SerializerHelper.WriteNullableByte(writer, 0));

        var lambda = Expression.Lambda<SerializeObjectDelegate<T>>(body, inst, writer);
        return lambda.Compile();
    }

    public DeserializeObjectDelegate<T> GenerateDeserializeLambda<T>(SerializerContext serializeContext, in GeneratorsSettings settings)
    {
        var type = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(type)!;

        var reader = Expression.Parameter(typeof(ISerializeReader), "reader");

        var model = SerializerHelper.GetOrGenerateSerializeModelConstant(underlyingType, serializeContext);

        var inst = Expression.Parameter(type, "inst");
        var constructor = type.GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, [underlyingType]) ??
            throw new GeneratorException(new NullReferenceException($"{type.FullName}: constructor not found"));

        var body = Expression.Block([inst],
            Expression.IfThenElse(
                Expression.Equal(
                    SerializerHelper.ReadNullableByte(reader),
                    Expression.Constant((byte)1)),
                Expression.Assign(
                    inst,
                    Expression.New(
                        constructor,
                        SerializerHelper.CallDeserialize(model, reader))),
                Expression.Assign(
                    inst,
                    Expression.Default(type))
                ),
            inst
            );

        var lambda = Expression.Lambda<DeserializeObjectDelegate<T>>(body, reader);
        return lambda.Compile();
    }

    private static Expression HashValue(Expression inst) =>
        Expression.Property(inst, nameof(Nullable<byte>.HasValue));

    private static Expression Value(Expression inst) =>
        Expression.Property(inst, nameof(Nullable<byte>.Value));
}