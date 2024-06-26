﻿using System.Reflection;
using ASiNet.Data.Serialization.Attributes;
using ASiNet.Data.Serialization.Interfaces;

namespace ASiNet.Data.Serialization;
internal static class Helper
{
    public static IEnumerable<Type> EnumiratePreGenerateModels()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes().Where(x => x.GetCustomAttribute<PreGenerateAttribute>() is not null))
            {
                yield return type;
            }
        }
    }

    public static IEnumerable<Type> EnumirateRegisteredModels()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes().Where(x => x.GetCustomAttribute<RegisterAttribute>() is not null))
            {
                yield return type;
            }
        }
    }

    public static object? InvokeGenerickMethod(object inst, string methodName, Type[] genericParameters, object?[] parameters)
    {
        var method = inst
            .GetType()
            .GetMethods()
            .Where(x => x.Name == methodName)
            .Where(x => x.GetGenericArguments().Length == genericParameters.Length)
            .First();

        return method
            .MakeGenericMethod(genericParameters)
            .Invoke(inst, parameters);
    }


    public static void WriteTypeHash(ISerializeWriter writer, ISerializeModel model)
    {
        writer.WriteBytes(model.TypeHashBytes);
    }

    public static long ReadTypeHash(ISerializeReader reader)
    {
        var buff = (stackalloc byte[sizeof(long)]);
        reader.ReadBytes(buff);
        var hashString = BitConverter.ToInt64(buff);
        return hashString;
    }

    public static IEnumerable<PropertyInfo> EnumerateProperties(Type type)
    {
        // GET ALL PROPERTIES.
        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty)
            .OrderBy(x => x.PropertyType.Name)
            .ThenBy(x => x.Name)
            .Where(x => x.GetIndexParameters().Length == 0 && x.GetCustomAttribute<IgnorePropertyAttribute>() is null);
        foreach (var item in props)
        {
            yield return item;
        }
        yield break;
    }

    public static IEnumerable<FieldInfo> EnumerateFields(Type type)
    {
        // GET ALL FIELDS.
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField | BindingFlags.SetField)
            .OrderBy(x => x.FieldType.Name)
            .ThenBy(x => x.Name)
            .Where(x => x.GetCustomAttribute<IgnoreFieldAttribute>() is null);
        foreach (var item in fields)
        {
            yield return item;
        }
        yield break;
    }
}
