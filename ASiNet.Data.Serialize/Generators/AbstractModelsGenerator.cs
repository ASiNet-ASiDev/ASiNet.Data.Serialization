﻿using System.Linq.Expressions;
using ASiNet.Data.Serialization.Exceptions;
using ASiNet.Data.Serialization.Generators.Helpers;
using ASiNet.Data.Serialization.Interfaces;
using ASiNet.Data.Serialization.Models;

namespace ASiNet.Data.Serialization.Generators;
public class AbstractModelsGenerator : IModelsGenerator
{
    public bool CanGenerateModelForType(Type type) => type.IsInterface || type.IsAbstract;

    public bool CanGenerateModelForType<T>() => typeof(T).IsInterface || typeof(T).IsAbstract;

    public SerializeModel<T> GenerateModel<T>(ISerializerContext serializeContext, in GeneratorsSettings settings)
    {
        try
        {
            var model = new AutoGeneratedModel<T>();

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
        var inst = Expression.Parameter(type, "inst");
        var writer = Expression.Parameter(typeof(ISerializeWriter).MakeByRefType(), "writer");
        var typeProp = Expression.Parameter(typeof(Type), "type");
        var contextProperty = Expression.Parameter(typeof(ISerializerContext), "context");
        var modelProp = Expression.Parameter(typeof(ISerializeModel), "model");

        var body = Expression.IfThenElse(
            // CHECK NULL VALUE
            Expression.NotEqual(
                inst,
                Expression.Default(type)),


            // WRITE PROPERTIES AND NULLABLR BYTE!
            Expression.Block([typeProp, contextProperty, modelProp],

                Expression.Assign(
                    typeProp,
                    ExpressionsHelper.CallGetType(inst)
                    ),

                Expression.Assign(
                    contextProperty,
                    Expression.Constant(serializeContext)
                    ),

                Expression.Assign(
                    modelProp,
                    ExpressionsHelper.GetOrGenerateModelSerializeTime(
                        typeProp,
                        contextProperty)
                    ),

                ExpressionsHelper.WriteNullableByteGenerateTime(writer, 1),

                ExpressionsHelper.SerializeTypeHash(writer, modelProp),

                ExpressionsHelper.CallSerializeObject(
                    modelProp,
                    Expression.Convert(
                        inst,
                        typeof(object)
                        ),
                    writer,
                    contextProperty
                    )
                ),

            // WRITE NULLABLR BYTE!
            ExpressionsHelper.WriteNullableByteGenerateTime(writer, 0));


        var lambda = Expression.Lambda<SerializeObjectDelegate<T>>(body, inst, writer);
        return lambda.Compile();
    }

    public DeserializeObjectDelegate<T> GenerateDeserializeLambda<T>(ISerializerContext serializeContext, in GeneratorsSettings settings)
    {
        var type = typeof(T);
        var inst = Expression.Parameter(type, "inst");
        var reader = Expression.Parameter(typeof(ISerializeReader).MakeByRefType(), "reader");

        var contextProperty = Expression.Parameter(typeof(ISerializerContext), "context");
        var modelProp = Expression.Parameter(typeof(ISerializeModel), "model");

        var body = Expression.IfThen(
            // READ NULLABLE BYTE
            Expression.Equal(
                ExpressionsHelper.ReadNullableByteGenerateTime(reader),
                Expression.Constant((byte)1)),

            // READ PROPERTIES TO OBJECT
            Expression.Block([contextProperty, modelProp],

                Expression.Assign(
                    contextProperty,
                    Expression.Constant(serializeContext)
                    ),

                Expression.Assign(
                    modelProp,
                    ExpressionsHelper.GetOrGenerateModelByHash(
                        reader,
                        contextProperty)
                    ),

                Expression.Assign(
                    inst,
                    Expression.Convert(
                        ExpressionsHelper.CallDeserializeObject(modelProp, reader, contextProperty),
                        type)
                    )
                )
            );

        var lambda = Expression.Lambda<DeserializeObjectDelegate<T>>(Expression.Block([inst], body, inst), reader);
        return lambda.Compile();
    }

    public GetObjectSizeDelegate<T> GenerateGetSizeDelegate<T>(ISerializerContext serializeContext, in GeneratorsSettings settings)
    {
        var type = typeof(T);

        var inst = Expression.Parameter(typeof(T), "inst");
        var result = Expression.Parameter(typeof(int), "size");

        var typeProp = Expression.Parameter(typeof(Type), "type");
        var contextProperty = Expression.Parameter(typeof(ISerializerContext), "context");
        var modelProp = Expression.Parameter(typeof(ISerializeModel), "model");

        var body = Expression.Block([result],
            Expression.Assign(result, Expression.Constant(1, typeof(int))),
            Expression.IfThen(
                Expression.NotEqual(
                    inst,
                    Expression.Default(type)),
                Expression.Block([typeProp, contextProperty, modelProp],

                    Expression.Assign(
                        typeProp,
                        ExpressionsHelper.CallGetType(inst)
                        ),

                    Expression.Assign(
                        contextProperty,
                        Expression.Constant(serializeContext)
                        ),

                    Expression.Assign(
                        modelProp,
                        ExpressionsHelper.GetOrGenerateModelSerializeTime(
                            typeProp,
                            contextProperty)
                        ),
                    Expression.AddAssign(result, Expression.Constant(sizeof(long))),

                    Expression.AddAssign(
                        result,
                        ExpressionsHelper.CallGetSizeGenerateTime(
                            modelProp,
                            inst,
                            contextProperty
                            )
                        )
                    )
                ),
            result
            );
        var lambda = Expression.Lambda<GetObjectSizeDelegate<T>>(body, inst);
        return lambda.Compile();
    }
}
