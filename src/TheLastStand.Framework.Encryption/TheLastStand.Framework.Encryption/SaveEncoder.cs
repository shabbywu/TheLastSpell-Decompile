using System;
using System.IO;
using System.Text;

namespace TheLastStand.Framework.Encryption;

public static class SaveEncoder
{
	public class HashMismatchException : Exception
	{
		public HashMismatchException(string details)
			: base(details)
		{
		}
	}

	public class CorruptSaveGameException : Exception
	{
		public CorruptSaveGameException(string details)
			: base(details)
		{
		}
	}

	public static bool useHashing = true;

	public static bool enableHashCheck = true;

	private const byte SHA_LENGTH = 64;

	private static AES aes = null;

	public static void Initialize(byte[] key, byte[] salt)
	{
		if (aes == null)
		{
			aes = new AES(key, salt);
		}
	}

	public static void Reset()
	{
		aes?.Dispose();
		aes = null;
	}

	public static void Encode(Stream inputStream, Stream outputStream)
	{
		byte[] array;
		MemoryStream memoryStream;
		using (memoryStream = new MemoryStream())
		{
			Compressor.Compress(inputStream, memoryStream);
			array = memoryStream.ToArray();
		}
		using (memoryStream = new MemoryStream(array))
		{
			if (useHashing)
			{
				using BinaryWriter binaryWriter = new BinaryWriter(outputStream, Encoding.UTF8, leaveOpen: true);
				binaryWriter.Write(Encoding.UTF8.GetBytes(array.Hash()));
			}
			aes.Encrypt(memoryStream, outputStream);
		}
	}

	public static byte[] Encode(string input)
	{
		using MemoryStream inputStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
		using MemoryStream memoryStream = new MemoryStream();
		Encode(inputStream, memoryStream);
		return memoryStream.ToArray();
	}

	public static void Decode(Stream inputStream, Stream outputStream)
	{
		string text = string.Empty;
		byte[] array2;
		MemoryStream memoryStream;
		using (memoryStream = new MemoryStream())
		{
			if (useHashing)
			{
				using BinaryReader binaryReader = new BinaryReader(inputStream, Encoding.UTF8, leaveOpen: true);
				byte[] array = new byte[64];
				binaryReader.Read(array, 0, 64);
				text = Encoding.UTF8.GetString(array);
			}
			try
			{
				aes.Decrypt(inputStream, memoryStream);
			}
			catch (AES.AESCryptographyOperationException ex)
			{
				throw new CorruptSaveGameException("The savegame is corrupt and cannot be read: " + ex.ToString() + ".");
			}
			catch (IOException ex2)
			{
				throw new CorruptSaveGameException("The savegame is corrupt and cannot be read: " + ex2.ToString() + ".");
			}
			array2 = memoryStream.ToArray();
		}
		using (memoryStream = new MemoryStream(array2))
		{
			using MemoryStream memoryStream3 = new MemoryStream();
			string text2 = array2.Hash();
			try
			{
				Compressor.Decompress(memoryStream, memoryStream3);
			}
			catch (IOException ex3)
			{
				throw new CorruptSaveGameException("The savegame is corrupt and cannot be read: " + ex3.ToString() + ".");
			}
			memoryStream3.Position = 0L;
			if (!useHashing || !enableHashCheck || text2 == text)
			{
				memoryStream3.CopyTo(outputStream);
				return;
			}
			throw new HashMismatchException("Invalid hash. Expected " + text + " but found " + text2 + ". The save has been modified.");
		}
	}

	public static string Decode(byte[] input)
	{
		using MemoryStream inputStream = new MemoryStream(input);
		using MemoryStream memoryStream = new MemoryStream();
		Decode(inputStream, memoryStream);
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}
}
