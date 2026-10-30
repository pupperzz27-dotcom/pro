using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace KeyAuth
{
	// Token: 0x02000003 RID: 3
	public static class encryption
	{
		// Token: 0x0600002C RID: 44
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

		// Token: 0x0600002D RID: 45
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetCurrentProcess();

		// Token: 0x0600002E RID: 46 RVA: 0x00003F64 File Offset: 0x00002164
		public static string HashHMAC(string enckey, string resp)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(enckey);
			byte[] bytes2 = Encoding.UTF8.GetBytes(resp);
			HMACSHA256 hmacsha = new HMACSHA256(bytes);
			return encryption.byte_arr_to_str(hmacsha.ComputeHash(bytes2));
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00003FA4 File Offset: 0x000021A4
		public static string byte_arr_to_str(byte[] ba)
		{
			StringBuilder stringBuilder = new StringBuilder(ba.Length * 2);
			foreach (byte b in ba)
			{
				stringBuilder.AppendFormat("{0:x2}", b);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00003FF0 File Offset: 0x000021F0
		public static byte[] str_to_byte_arr(string hex)
		{
			byte[] array2;
			try
			{
				int length = hex.Length;
				byte[] array = new byte[length / 2];
				for (int i = 0; i < length; i += 2)
				{
					array[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
				}
				array2 = array;
			}
			catch
			{
				api.error("The session has ended, open program again.");
				encryption.TerminateProcess(encryption.GetCurrentProcess(), 1U);
				array2 = null;
			}
			return array2;
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00004068 File Offset: 0x00002268
		public static string iv_key()
		{
			return Guid.NewGuid().ToString().Substring(0, 16);
		}
	}
}
