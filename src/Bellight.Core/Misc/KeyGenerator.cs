using System.Security.Cryptography;

namespace Bellight.Core.Misc;

public static class KeyGenerator
{
    private static readonly char[] _numerics = "1234567890".ToCharArray();
    private static readonly char[] _alphabets = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private static readonly char[] _alphanumerics = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    public static string Generate(int length = 7)
    {
        return GenerateRandom(_alphanumerics, length);
    }

    public static string GenerateNumerics(int length = 7)
    {
        return GenerateRandom(_numerics, length);
    }

    public static string GenerateAlphabets(int length = 7)
    {
        return GenerateRandom(_alphabets, length);
    }

    private static string GenerateRandom(char[] chars, int length)
    {
        using (var crypto = RandomNumberGenerator.Create())
        {
            var data = new byte[length];
            // If chars.Length isn't a power of 2 then there is a bias if
            // we simply use the modulus operator. The first characters of
            // chars will be more probable than the last ones.

            // buffer used if we encounter an unusable random byte. We will
            // regenerate it in this buffer
            byte[]? smallBuffer = null;

            // Maximum random number that can be used without introducing a
            // bias
            int maxRandom = byte.MaxValue - ((byte.MaxValue + 1) % chars.Length);

            crypto.GetBytes(data);

            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                byte v = data[i];

                while (v > maxRandom)
                {
                    if (smallBuffer == null)
                    {
                        smallBuffer = new byte[1];
                    }

                    crypto.GetBytes(smallBuffer);
                    v = smallBuffer[0];
                }

                result[i] = chars[v % chars.Length];
            }

            return new string(result);
        }
    }
}