﻿using System.Linq.Expressions;
using ASiNet.Data.Serialization.Contexts;
using ASiNet.Data.Serialization.Exceptions;
using ASiNet.Data.Serialization.Generators.Helpers;
using ASiNet.Data.Serialization.Interfaces;
using ASiNet.Data.Serialization.Models;

namespace ASiNet.Data.Serialization.Generators;
public class NullableModelsGenerator : IModelsGenerator
{

    public SerializeModel<T> GenerateModel<T>(ISerializerContext serializeContext, in GeneratorsSettings settings)
    {
        try
        {
            var model = new SerializeModel<T>();

            serializeContext.AddModel(model);

            model.SetSerializeDelegate(GenerateSerializeLambda<T>(serializeContext, settings));
            model.SetDeserializeDelegate(GenerateDeserializeLambda<T>(serializeContext, settings));
            model.SetGetSizeDelegate(GenerateGetSizeDelegate<T>(serializeContext, settings));

            return model;
        }
        catch (Exception ex)
        {
            throw new GeneratorException(ex);
        }
    }


    public SerializeObjectDelegate<T> GenerateSerializeLambda<T>(ISerializerContext serializeContext, in GeneratorsSettings settings)
    {
        var type = typeof(T);
        var inst = Expression.Parameter(typeof(T), "inst");
        var writer = Expression.Parameter(typeof(ISerializeWriter).MakeByRefType(), "writer");

        var model = ExpressionsHelper.GetOrGenerateModelGenerateTime(Nullable.GetUnderlyingType(type)!, serializeContext);

        var body = Expression.IfThenElse(
            HashValue(inst),
            Expression.Block(
                ExpressionsHelper.WriteNullableByteGenerateTime(writer, 1),
                ExpressionsHelper.CallSerialize(
                    model,
                    Value(inst),
                    writer)
                ),
            ExpressionsHelper.WriteNullableByteGenerateTime(writer, 0));

        var lambda = Expression.Lambda<SerializeObjectDelegate<T>>(body, inst, writer);
        return lambda.Compile();
    }

    public DeserializeObjectDelegate<T> GenerateDeserializeLambda<T>(ISerializerContext serializeContext, in GeneratorsSettings settings)
    {
        var type = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(type)!;

        var reader = Expression.Parameter(typeof(ISerializeReader).MakeByRefType(), "reader");

        var model = ExpressionsHelper.GetOrGenerateModelGenerateTime(underlyingType, serializeContext);

        var inst = Expression.Parameter(type, "inst");
        var constructor = type.GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, [underlyingType]) ??
            throw new GeneratorException(new NullReferenceException($"{type.FullName}: constructor not found"));

        var body = Expression.Block([inst],
            Expression.IfThenElse(
                Expression.Equal(
                    ExpressionsHelper.ReadNullableByteGenerateTime(reader),
                    Expression.Constant((byte)1)),
                Expression.Assign(
                    inst,
                    Expression.New(
                        constructor,
                        ExpressionsHelper.CallDeserialize(model, reader))),
                Expression.Assign(
                    inst,
                    Expression.Default(type))
                ),
            inst
            );

        var lambda = Expression.Lambda<DeserializeObjectDelegate<T>>(body, reader);
        return lambda.Compile();
    }

    public GetObjectSizeDelegate<T> GenerateGetSizeDelegate<T>(ISerializerContext serializeContext, in GeneratorsSettings settings)
    {
        var type = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(type)!;

        var inst = Expression.Parameter(typeof(T), "inst");
        var result = Expression.Parameter(typeof(int), "size");

        var model = ExpressionsHelper.GetOrGenerateModelGenerateTime(underlyingType, serializeContext);

        var body = Expression.Block([result],
            Expression.Assign(result, Expression.Constant(1, typeof(int))),
            Expression.IfThen(
                HashValue(inst),
                Expression.AddAssign(result, ExpressionsHelper.CallGetSizeGenerateTime(model, Expression.Convert(inst, underlyingType)))
                ),
            result
            );

        var lambda = Expression.Lambda<GetObjectSizeDelegate<T>>(body, inst);
        return lambda.Compile();
    }

    private static Expression HashValue(Expression inst) =>
        Expression.Property(inst, nameof(Nullable<byte>.HasValue));

    private static Expression Value(Expression inst) =>
        Expression.Property(inst, nameof(Nullable<byte>.Value));
}
