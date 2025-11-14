using System;
using System.Security.Cryptography;
using System.Text;

namespace ReQuantum.Utilities;

/// <summary>
/// 加密工具类，提供基于设备特定密钥的AES加密和解密功能
/// </summary>
public static class Encryption
{
    /// <summary>
    /// 设备特定的密钥和初始化向量，基于尽可能唯一的设备标识生成。
    /// </summary>
    private static readonly Lazy<(byte[] Key, byte[] IV)> DeviceSpecificKeyIv = new(() =>
    {
        var deviceId = GetStableDeviceId();

        var salt = "ReQuantum.DeviceEncryption.Salt.v1"u8.ToArray();

        var keyMaterial = Rfc2898DeriveBytes.Pbkdf2(
            password: deviceId,
            salt: salt,
            iterations: 100_000,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 48);

        var key = keyMaterial.AsSpan(0, 32).ToArray();
        var iv = keyMaterial.AsSpan(32, 16).ToArray();

        return (key, iv);
    });

    /// <summary>
    /// 获取尽可能稳定的设备唯一标识符。
    /// 此方法不保证全局唯一，但在同一设备上应保持稳定。
    /// </summary>
    private static string GetStableDeviceId()
    {
        // TODO: 改进此方法以获取更稳定和安全的设备标识符
        return $"{Environment.MachineName}_{Environment.UserName}";
    }

    /// <summary>
    /// 使用AES算法加密字符串
    /// </summary>
    /// <param name="plainText">要加密的明文</param>
    /// <returns>Base64编码的密文</returns>
    public static string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = DeviceSpecificKeyIv.Value.Key;
        aes.IV = DeviceSpecificKeyIv.Value.IV;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return Convert.ToBase64String(cipherBytes);
    }

    /// <summary>
    /// 使用AES算法解密字符串
    /// </summary>
    /// <param name="cipherText">要解密的Base64编码密文</param>
    /// <returns>解密后的明文</returns>
    public static string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = DeviceSpecificKeyIv.Value.Key;
        aes.IV = DeviceSpecificKeyIv.Value.IV;

        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
