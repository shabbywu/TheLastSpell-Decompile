using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TheLastStand.Framework.Encryption;

public class AES : IDisposable
{
	public class AESCryptographyOperationException : Exception
	{
		public AESCryptographyOperationException(string reason)
			: base(reason)
		{
		}
	}

	private const ushort ITERATIONS = 16;

	private const ushort READ_BUFFER_LENGTH = 1024;

	private readonly byte[] saltBytes = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };

	private readonly byte[] passKey;

	private readonly RijndaelManaged aes;

	public AES(string passKey)
		: this(Encoding.UTF8.GetBytes(passKey))
	{
	}

	public AES(byte[] passKey, byte[] saltBytes = null)
	{
		this.passKey = passKey;
		if (saltBytes != null)
		{
			this.saltBytes = saltBytes;
		}
		Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(this.passKey, this.saltBytes, 16);
		aes = new RijndaelManaged();
		aes.Key = rfc2898DeriveBytes.GetBytes(aes.KeySize / 8);
		aes.IV = rfc2898DeriveBytes.GetBytes(aes.BlockSize / 8);
		aes.Padding = PaddingMode.PKCS7;
		aes.Mode = CipherMode.CFB;
	}

	public byte[] Encrypt(string input)
	{
		using MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
		using MemoryStream memoryStream2 = new MemoryStream();
		if (Encrypt(memoryStream, memoryStream2, memoryStream.ToArray().Length))
		{
			return memoryStream2.ToArray();
		}
		throw new AESCryptographyOperationException("An error occured while encrypting " + input);
	}

	public string Decrypt(byte[] input)
	{
		using MemoryStream memoryStream = new MemoryStream(input);
		using MemoryStream memoryStream2 = new MemoryStream();
		if (Decrypt(memoryStream, memoryStream2, memoryStream.ToArray().Length))
		{
			return Encoding.UTF8.GetString(memoryStream2.ToArray());
		}
		throw new AESCryptographyOperationException($"An error occured while decrypting {input}");
	}

	public bool Encrypt(Stream inputStream, Stream outputStream, int? length = null)
	{
		try
		{
			using BinaryReader binaryReader = new BinaryReader(inputStream);
			using CryptoStream cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
			if (length.HasValue)
			{
				cryptoStream.Write(binaryReader.ReadBytes(length.Value), 0, length.Value);
			}
			else
			{
				byte[] array;
				do
				{
					array = binaryReader.ReadBytes(1024);
					cryptoStream.Write(array, 0, array.Length);
				}
				while (array.Length >= 1024);
			}
		}
		catch (IOException value)
		{
			Console.WriteLine("Error while encrypting");
			Console.WriteLine(value);
			return false;
		}
		return true;
	}

	public bool Decrypt(Stream inputStream, Stream outputStream, int? length = null)
	{
		try
		{
			using BinaryWriter binaryWriter = new BinaryWriter(outputStream);
			using CryptoStream cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
			if (length.HasValue)
			{
				byte[] buffer = new byte[length.Value];
				cryptoStream.Read(buffer, 0, length.Value);
				binaryWriter.Write(buffer);
			}
			else
			{
				int num;
				do
				{
					byte[] buffer2 = new byte[1024];
					num = cryptoStream.Read(buffer2, 0, 1024);
					binaryWriter.Write(buffer2, 0, num);
				}
				while (num >= 1024);
			}
		}
		catch (IOException value)
		{
			Console.WriteLine("Error while decrypting");
			Console.WriteLine(value);
			return false;
		}
		catch (CryptographicException value2)
		{
			Console.WriteLine("Error while decrypting");
			Console.WriteLine(value2);
			return false;
		}
		return true;
	}

	public void Dispose()
	{
		((IDisposable)aes).Dispose();
	}
}
