using ReQuantum.Services;
using ReQuantum.Utilities;
using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ReQuantum.Extensions;

public static class StorageExtensions
{
    /// <summary>
    /// 检查指定的键是否存在
    /// </summary>
    public static bool ContainsKey(this IStorage storage, string key)
    {
        try
        {
            storage.GetString(key);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取值（JSON反序列化，使用源生成器）
    /// </summary>
    public static T? Get<T>(this IStorage storage, string key)
    {
        var value = storage.GetString(key);

        // 从上下文中获取 TypeInfo
        return storage.JsonContext.GetTypeInfo(typeof(T)) is JsonTypeInfo<T> typeInfo
            ? JsonSerializer.Deserialize(value, typeInfo)
            : throw new NotSupportedException();
    }

    /// <summary>
    /// 尝试获取值
    /// </summary>
    public static bool TryGet<T>(this IStorage storage, string key, out T? value)
    {
        try
        {
            value = storage.GetWithEncryption<T>(key);
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// 设置值（JSON序列化，使用源生成器）
    /// </summary>
    public static void Set<T>(this IStorage storage, string key, T? value)
    {
        if (storage.JsonContext.GetTypeInfo(typeof(T)) is not JsonTypeInfo<T?> typeInfo)
        {
            throw new NotSupportedException();
        }

        var json = JsonSerializer.Serialize(value, typeInfo);
        storage.SetString(key, json);
    }

    /// <summary>
    /// 获取值（JSON反序列化，使用源生成器）
    /// </summary>
    public static T? GetWithEncryption<T>(this IStorage storage, string key)
    {
        var value = storage.GetString(key);
        var decryptedValue = Encryption.Decrypt(value);

        // 从上下文中获取 TypeInfo
        return storage.JsonContext.GetTypeInfo(typeof(T)) is JsonTypeInfo<T> typeInfo
            ? JsonSerializer.Deserialize(decryptedValue, typeInfo)
            : throw new NotSupportedException();
    }

    /// <summary>
    /// 尝试获取值
    /// </summary>
    public static bool TryGetWithEncryption<T>(this IStorage storage, string key, out T? value)
    {
        try
        {
            value = storage.GetWithEncryption<T>(key);
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// 设置加密值（JSON序列化，使用源生成器）
    /// </summary>
    public static void SetWithEncryption<T>(this IStorage storage, string key, T? value)
    {
        if (storage.JsonContext.GetTypeInfo(typeof(T)) is not JsonTypeInfo<T?> typeInfo)
        {
            throw new NotSupportedException();
        }

        var json = JsonSerializer.Serialize(value, typeInfo);
        var encryptedJson = Encryption.Encrypt(json);
        storage.SetString(key, encryptedJson);
    }
}
