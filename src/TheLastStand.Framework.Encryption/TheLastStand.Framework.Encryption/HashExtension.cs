using System.Security.Cryptography;
using System.Text;

namespace TheLastStand.Framework.Encryption;

public static class HashExtension
{
	public static string Hash(this byte[] rawData)
	{
		using SHA256 sHA = SHA256.Create();
		byte[] array = sHA.ComputeHash(rawData);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2").ToUpper());
		}
		return stringBuilder.ToString();
	}
}
