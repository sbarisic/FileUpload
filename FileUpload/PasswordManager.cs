using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace FileUpload {
	public static class PasswordManager {
		static byte[] StringToByteArray(string Str) {
			int NumChars = Str.Length;
			byte[] Bytes = new byte[NumChars / 2];

			for (int i = 0; i < NumChars; i += 2)
				Bytes[i / 2] = Convert.ToByte(Str.Substring(i, 2), 16);

			return Bytes;
		}

		static string ByteArrayToString(byte[] Bytes) {
			return new string(Bytes.SelectMany(B => B.ToString("X2")).ToArray());
		}

		public static string GenerateSalt() {
			const int SaltLen = 16;

			using (RNGCryptoServiceProvider Crypto = new RNGCryptoServiceProvider()) {
				byte[] Bytes;
				Crypto.GetBytes(Bytes = new byte[SaltLen]);

				return ByteArrayToString(Bytes);
			}
		}

		public static string HashPassword(string Password, string SaltHex) {
			using (Rfc2898DeriveBytes Derive = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(Password), StringToByteArray(SaltHex), 10000))
				return ByteArrayToString(Derive.GetBytes(32));
		}

		public static bool IsValidPassword(string Password, string Salt, string Hash) {
			try {
				byte[] HashBytes = StringToByteArray(Hash);
				byte[] NewHash = StringToByteArray(HashPassword(Password, Salt));

				if (HashBytes.Length != NewHash.Length)
					return false;

				for (int i = 0; i < HashBytes.Length; i++)
					if (HashBytes[i] != NewHash[i])
						return false;

				return true;
			} catch (Exception) when (!Debugger.IsAttached) {
			}

			return false;
		}
	}
}