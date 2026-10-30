using System;
using System.Numerics;

namespace Cryptographic
{
	// Token: 0x02000008 RID: 8
	internal static class BigIntegerHelpers
	{
		// Token: 0x06000061 RID: 97 RVA: 0x000055B8 File Offset: 0x000037B8
		public static BigInteger Mod(this BigInteger num, BigInteger modulo)
		{
			BigInteger bigInteger = num % modulo;
			return (bigInteger < 0L) ? (bigInteger + modulo) : bigInteger;
		}
	}
}
