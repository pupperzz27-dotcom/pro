using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Cryptographic
{
	// Token: 0x02000006 RID: 6
	public class Ed25519
	{
		// Token: 0x0600004D RID: 77 RVA: 0x00004B28 File Offset: 0x00002D28
		private static byte[] ComputeHash(byte[] m)
		{
			byte[] array;
			using (SHA512 sha = SHA512.Create())
			{
				array = sha.ComputeHash(m);
			}
			return array;
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00004B64 File Offset: 0x00002D64
		private static BigInteger ExpMod(BigInteger number, BigInteger exponent, BigInteger modulo)
		{
			BigInteger bigInteger = BigInteger.One;
			BigInteger bigInteger2 = number.Mod(modulo);
			while (exponent > 0L)
			{
				bool flag = !exponent.IsEven;
				if (flag)
				{
					bigInteger = (bigInteger * bigInteger2).Mod(modulo);
				}
				bigInteger2 = (bigInteger2 * bigInteger2).Mod(modulo);
				exponent /= 2;
			}
			return bigInteger;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00004BD4 File Offset: 0x00002DD4
		private static BigInteger Inv(BigInteger x)
		{
			bool flag = !Ed25519.InverseCache.ContainsKey(x);
			if (flag)
			{
				Ed25519.InverseCache[x] = Ed25519.ExpMod(x, Ed25519.Qm2, Ed25519.Q);
			}
			return Ed25519.InverseCache[x];
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00004C24 File Offset: 0x00002E24
		private static BigInteger RecoverX(BigInteger y)
		{
			BigInteger bigInteger = y * y;
			BigInteger bigInteger2 = (bigInteger - 1) * Ed25519.Inv(Ed25519.D * bigInteger + 1);
			BigInteger bigInteger3 = Ed25519.ExpMod(bigInteger2, Ed25519.Qp3 / Ed25519.Eight, Ed25519.Q);
			bool flag = !(bigInteger3 * bigInteger3 - bigInteger2).Mod(Ed25519.Q).Equals(BigInteger.Zero);
			if (flag)
			{
				bigInteger3 = (bigInteger3 * Ed25519.I).Mod(Ed25519.Q);
			}
			bool flag2 = !bigInteger3.IsEven;
			if (flag2)
			{
				bigInteger3 = Ed25519.Q - bigInteger3;
			}
			return bigInteger3;
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00004CEC File Offset: 0x00002EEC
		private static Tuple<BigInteger, BigInteger> Edwards(BigInteger px, BigInteger py, BigInteger qx, BigInteger qy)
		{
			BigInteger bigInteger = px * qx;
			BigInteger bigInteger2 = py * qy;
			BigInteger bigInteger3 = Ed25519.D * bigInteger * bigInteger2;
			BigInteger bigInteger4 = (px * qy + qx * py) * Ed25519.Inv(1 + bigInteger3);
			BigInteger bigInteger5 = (py * qy + bigInteger) * Ed25519.Inv(1 - bigInteger3);
			return new Tuple<BigInteger, BigInteger>(bigInteger4.Mod(Ed25519.Q), bigInteger5.Mod(Ed25519.Q));
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00004D90 File Offset: 0x00002F90
		private static Tuple<BigInteger, BigInteger> EdwardsSquare(BigInteger x, BigInteger y)
		{
			BigInteger bigInteger = x * x;
			BigInteger bigInteger2 = y * y;
			BigInteger bigInteger3 = Ed25519.D * bigInteger * bigInteger2;
			BigInteger bigInteger4 = 2 * x * y * Ed25519.Inv(1 + bigInteger3);
			BigInteger bigInteger5 = (bigInteger2 + bigInteger) * Ed25519.Inv(1 - bigInteger3);
			return new Tuple<BigInteger, BigInteger>(bigInteger4.Mod(Ed25519.Q), bigInteger5.Mod(Ed25519.Q));
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00004E2C File Offset: 0x0000302C
		private static Tuple<BigInteger, BigInteger> ScalarMul(Tuple<BigInteger, BigInteger> point, BigInteger scalar)
		{
			Tuple<BigInteger, BigInteger> tuple = new Tuple<BigInteger, BigInteger>(BigInteger.Zero, BigInteger.One);
			Tuple<BigInteger, BigInteger> tuple2 = point;
			while (scalar > 0L)
			{
				bool flag = !scalar.IsEven;
				if (flag)
				{
					tuple = Ed25519.Edwards(tuple.Item1, tuple.Item2, tuple2.Item1, tuple2.Item2);
				}
				tuple2 = Ed25519.EdwardsSquare(tuple2.Item1, tuple2.Item2);
				scalar >>= 1;
			}
			return tuple;
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00004EAC File Offset: 0x000030AC
		public static byte[] EncodeInt(BigInteger y)
		{
			byte[] array = y.ToByteArray();
			byte[] array2 = new byte[Math.Max(array.Length, 32)];
			Array.Copy(array, array2, array.Length);
			return array2;
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00004EE4 File Offset: 0x000030E4
		public static byte[] EncodePoint(BigInteger x, BigInteger y)
		{
			byte[] array = Ed25519.EncodeInt(y);
			byte[] array2 = array;
			int num = array.Length - 1;
			array2[num] |= (x.IsEven ? 0 : 128);
			return array;
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00004F20 File Offset: 0x00003120
		private static int GetBit(byte[] h, int i)
		{
			return (h[i / 8] >> i % 8) & 1;
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00004F40 File Offset: 0x00003140
		public static byte[] PublicKey(byte[] signingKey)
		{
			byte[] array = Ed25519.ComputeHash(signingKey);
			BigInteger bigInteger = Ed25519.TwoPowBitLengthMinusTwo;
			for (int i = 3; i < 254; i++)
			{
				int bit = Ed25519.GetBit(array, i);
				bool flag = bit != 0;
				if (flag)
				{
					bigInteger += Ed25519.TwoPowCache[i];
				}
			}
			Tuple<BigInteger, BigInteger> tuple = Ed25519.ScalarMul(Ed25519.B, bigInteger);
			return Ed25519.EncodePoint(tuple.Item1, tuple.Item2);
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00004FC0 File Offset: 0x000031C0
		private static BigInteger HashInt(byte[] m)
		{
			byte[] array = Ed25519.ComputeHash(m);
			BigInteger bigInteger = BigInteger.Zero;
			for (int i = 0; i < 512; i++)
			{
				int bit = Ed25519.GetBit(array, i);
				bool flag = bit != 0;
				if (flag)
				{
					bigInteger += Ed25519.TwoPowCache[i];
				}
			}
			return bigInteger;
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00005020 File Offset: 0x00003220
		public static byte[] Signature(byte[] message, byte[] signingKey, byte[] publicKey)
		{
			byte[] array = Ed25519.ComputeHash(signingKey);
			BigInteger bigInteger = Ed25519.TwoPowBitLengthMinusTwo;
			for (int i = 3; i < 254; i++)
			{
				int bit = Ed25519.GetBit(array, i);
				bool flag = bit != 0;
				if (flag)
				{
					bigInteger += Ed25519.TwoPowCache[i];
				}
			}
			BigInteger bigInteger2;
			using (MemoryStream memoryStream = new MemoryStream(32 + message.Length))
			{
				memoryStream.Write(array, 32, 32);
				memoryStream.Write(message, 0, message.Length);
				bigInteger2 = Ed25519.HashInt(memoryStream.ToArray());
			}
			Tuple<BigInteger, BigInteger> tuple = Ed25519.ScalarMul(Ed25519.B, bigInteger2);
			byte[] array2 = Ed25519.EncodePoint(tuple.Item1, tuple.Item2);
			BigInteger bigInteger3;
			using (MemoryStream memoryStream2 = new MemoryStream(32 + publicKey.Length + message.Length))
			{
				memoryStream2.Write(array2, 0, array2.Length);
				memoryStream2.Write(publicKey, 0, publicKey.Length);
				memoryStream2.Write(message, 0, message.Length);
				bigInteger3 = (bigInteger2 + Ed25519.HashInt(memoryStream2.ToArray()) * bigInteger).Mod(Ed25519.L);
			}
			byte[] array4;
			using (MemoryStream memoryStream3 = new MemoryStream(64))
			{
				memoryStream3.Write(array2, 0, array2.Length);
				byte[] array3 = Ed25519.EncodeInt(bigInteger3);
				memoryStream3.Write(array3, 0, array3.Length);
				array4 = memoryStream3.ToArray();
			}
			return array4;
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000051C8 File Offset: 0x000033C8
		private static bool IsOnCurve(BigInteger x, BigInteger y)
		{
			BigInteger bigInteger = x * x;
			BigInteger bigInteger2 = y * y;
			BigInteger bigInteger3 = Ed25519.D * bigInteger2 * bigInteger;
			return (bigInteger2 - bigInteger - bigInteger3 - 1).Mod(Ed25519.Q).Equals(BigInteger.Zero);
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00005230 File Offset: 0x00003430
		private static BigInteger DecodeInt(byte[] s)
		{
			return new BigInteger(s) & Ed25519.Un;
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00005254 File Offset: 0x00003454
		private static Tuple<BigInteger, BigInteger> DecodePoint(byte[] pointBytes)
		{
			BigInteger bigInteger = new BigInteger(pointBytes) & Ed25519.Un;
			BigInteger bigInteger2 = Ed25519.RecoverX(bigInteger);
			bool flag = (bigInteger2.IsEven ? 0 : 1) != Ed25519.GetBit(pointBytes, 255);
			if (flag)
			{
				bigInteger2 = Ed25519.Q - bigInteger2;
			}
			Tuple<BigInteger, BigInteger> tuple = new Tuple<BigInteger, BigInteger>(bigInteger2, bigInteger);
			bool flag2 = !Ed25519.IsOnCurve(bigInteger2, bigInteger);
			if (flag2)
			{
				throw new ArgumentException("Decoding point that is not on curve");
			}
			return tuple;
		}

		// Token: 0x0600005D RID: 93 RVA: 0x000052D4 File Offset: 0x000034D4
		public static bool CheckValid(byte[] signature, byte[] message, byte[] publicKey)
		{
			Console.Write(".");
			bool flag = signature.Length != 64;
			if (flag)
			{
				throw new ArgumentException("Signature length is wrong");
			}
			bool flag2 = publicKey.Length != 32;
			if (flag2)
			{
				throw new ArgumentException("Public key length is wrong");
			}
			byte[] array = Arrays.CopyOfRange(signature, 0, 32);
			Tuple<BigInteger, BigInteger> tuple = Ed25519.DecodePoint(array);
			Tuple<BigInteger, BigInteger> tuple2 = Ed25519.DecodePoint(publicKey);
			byte[] array2 = Arrays.CopyOfRange(signature, 32, 64);
			BigInteger bigInteger = Ed25519.DecodeInt(array2);
			BigInteger bigInteger2;
			using (MemoryStream memoryStream = new MemoryStream(32 + publicKey.Length + message.Length))
			{
				byte[] array3 = Ed25519.EncodePoint(tuple.Item1, tuple.Item2);
				memoryStream.Write(array3, 0, array3.Length);
				memoryStream.Write(publicKey, 0, publicKey.Length);
				memoryStream.Write(message, 0, message.Length);
				bigInteger2 = Ed25519.HashInt(memoryStream.ToArray());
			}
			Console.Write(".");
			Tuple<BigInteger, BigInteger> tuple3 = Ed25519.ScalarMul(Ed25519.B, bigInteger);
			Console.Write(".");
			Tuple<BigInteger, BigInteger> tuple4 = Ed25519.ScalarMul(tuple2, bigInteger2);
			Tuple<BigInteger, BigInteger> tuple5 = Ed25519.Edwards(tuple.Item1, tuple.Item2, tuple4.Item1, tuple4.Item2);
			bool flag3 = !tuple3.Item1.Equals(tuple5.Item1) || !tuple3.Item2.Equals(tuple5.Item2);
			return !flag3;
		}

		// Token: 0x04000010 RID: 16
		private static readonly Dictionary<BigInteger, BigInteger> InverseCache = new Dictionary<BigInteger, BigInteger>();

		// Token: 0x04000011 RID: 17
		private const int BitLength = 256;

		// Token: 0x04000012 RID: 18
		private static readonly BigInteger TwoPowBitLengthMinusTwo = BigInteger.Pow(2, 254);

		// Token: 0x04000013 RID: 19
		private static readonly BigInteger[] TwoPowCache = (from i in Enumerable.Range(0, 512)
			select BigInteger.Pow(2, i)).ToArray<BigInteger>();

		// Token: 0x04000014 RID: 20
		private static readonly BigInteger Q = BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564819949");

		// Token: 0x04000015 RID: 21
		private static readonly BigInteger Qm2 = BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564819947");

		// Token: 0x04000016 RID: 22
		private static readonly BigInteger Qp3 = BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564819952");

		// Token: 0x04000017 RID: 23
		private static readonly BigInteger L = BigInteger.Parse("7237005577332262213973186563042994240857116359379907606001950938285454250989");

		// Token: 0x04000018 RID: 24
		private static readonly BigInteger D = BigInteger.Parse("-4513249062541557337682894930092624173785641285191125241628941591882900924598840740");

		// Token: 0x04000019 RID: 25
		private static readonly BigInteger I = BigInteger.Parse("19681161376707505956807079304988542015446066515923890162744021073123829784752");

		// Token: 0x0400001A RID: 26
		private static readonly BigInteger By = BigInteger.Parse("46316835694926478169428394003475163141307993866256225615783033603165251855960");

		// Token: 0x0400001B RID: 27
		private static readonly BigInteger Bx = BigInteger.Parse("15112221349535400772501151409588531511454012693041857206046113283949847762202");

		// Token: 0x0400001C RID: 28
		private static readonly Tuple<BigInteger, BigInteger> B = new Tuple<BigInteger, BigInteger>(Ed25519.Bx.Mod(Ed25519.Q), Ed25519.By.Mod(Ed25519.Q));

		// Token: 0x0400001D RID: 29
		private static readonly BigInteger Un = BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564819967");

		// Token: 0x0400001E RID: 30
		private static readonly BigInteger Two = new BigInteger(2);

		// Token: 0x0400001F RID: 31
		private static readonly BigInteger Eight = new BigInteger(8);
	}
}
