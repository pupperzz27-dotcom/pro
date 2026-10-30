using System;

namespace Cryptographic
{
	// Token: 0x02000007 RID: 7
	internal static class Arrays
	{
		// Token: 0x06000060 RID: 96 RVA: 0x0000558C File Offset: 0x0000378C
		public static byte[] CopyOfRange(byte[] original, int from, int to)
		{
			int num = to - from;
			byte[] array = new byte[num];
			Array.Copy(original, from, array, 0, num);
			return array;
		}
	}
}
