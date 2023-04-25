using System;
using System.Collections.Generic;
using System.Text;

namespace EmServiceLib
{
	public class Conversions
	{
		#region Numbers

		#region Bytes To Number

		#region From 2 Bytes

		#region Integer

		// 1 слово беззнаковое целое
		public static ushort bytes_2_ushort(ref byte[] array, int shift)
		{
			return (ushort)(array[shift + 1] * 0x100 + array[shift]);
		}

		// 1 слово знаковое целое
		public static short bytes_2_short(ref byte[] array, int shift)
		{
			return (short)((array[shift + 1] << 8) + array[shift]);
		}

		// 1 слово беззнаковое целое
		public static ushort bytes_2_ushortBackToFront(ref byte[] array, int shift)
		{
			return (ushort)(array[shift] * 0x100 + array[shift + 1]);
		}

		#endregion

		#region Real

		// 4 словa в беззнаковое действительное
		public static double bytes_2_double(ref byte[] array, int shift)
		{
			try
			{
				return BitConverter.ToDouble(array, shift);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in bytes_2_double():");
				throw;
			}
		}

		// 1 слово знаковое действительное
		// формат Q1.15: SINN NNNN NNNN NNNN
		public static float bytes_2_signed_float_Q_1_15(ref byte[] array, int shift)
		{
			//bool sign = (Array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			//short i = (short)(Array[shift + 1] * 0x100 + Array[shift]);
			//if (sign) i = (short)-(int)i;
			//float f = (float)i / 32768;
			//return sign ? -f : f;

			short i = (short)((array[shift + 1] << 8) + array[shift]);

			float f = (float)i / 32768;
			return f;
		}

		// 1 слово беззнаковое действительное
		// формат Q1.15: SINN NNNN NNNN NNNN
		public static float bytes_2_usigned_float_Q_1_15(ref byte[] array, int shift)
		{
			//bool sign = (Array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			//short i = (short)(Array[shift + 1] * 0x100 + Array[shift]);
			//if (sign) i = (short)-(int)i;
			//float f = (float)i / 32768;
			//return sign ? -f : f;
			ushort i = (ushort)((array[shift + 1] << 8) + array[shift]);
			float f = (float)i / 32768;
			return f;
		}

		// 1 слово знаковое действительное
		// формат Q6.9: SIII IIIN NNNN NNNN
		public static float bytes_2_signed_float_Q_6_9(ref byte[] array, int shift)
		{
			//bool sign = (Array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			short i = (short)((array[shift + 1] << 8) + array[shift]);
			//ushort c = (ushort)(i >> 9);
			//ushort d = (ushort)(i & 0x1FF);
			//if (sign) i = (short)-(int)i;
			//float f = (float)c * ((float)d / 512.0f); //(float)i / 512;
			//return sign ? -f : f;
			//return f;

			//short ii = (short)((Array[shift + 1] << 8) + Array[shift]);
			float f = (float)i / 512;
			return f;
		}

		// 1 слово знаковое действительное
		public static float bytes_2_signed_float8192(ref byte[] array, int shift)
		{
			//bool sign = (Array[shift + 1] >> 7 == 1);
			//short i = (short)(Array[shift + 1] * 0x100 + Array[shift]);
			////if (sign) i = (short)-(int)i;
			//float f = (float)i / 8192;
			//return sign ? -f : f;

			short i = (short)((array[shift + 1] << 8) + array[shift]);
			float f = (float)i / 8192;
			return f;
		}

        // 1 слово знаковое действительное
        public static float bytes_2_signed_float65536(ref byte[] array, int shift)
        {
            short i = (short)((array[shift + 1] << 8) + array[shift]);
            float f = (float)i / 65536;
            return f;
        }

		// 1 слово знаковое действительное
		// bits: SIII NNNN NNNN NNNN
		public static float bytes_2_signed_float4096(ref byte[] array, int shift)
		{
			//bool sign = (Array[shift + 1] >> 7 == 1);
			//short i = (short)(Array[shift + 1] * 0x100 + Array[shift]);
			//if (sign) i = (short)-(int)i;
			//float f = (float)i / 4096;
			//return sign ? -f: f;
			short i = (short)((array[shift + 1] << 8) + array[shift]);
			float f = (float)i / 4096;
			return f;
		}

		// 1 слово знаковое действительное
		// bits: SIII IIII INNN NNNN
		public static float bytes_2_signed_float128(ref byte[] array, int shift)
		{
			bool sign = (array[shift + 1] / 0x80 == 1);
			//bool sign = (array[shift + 1] >> 7 == 1);
			short i = (short)(array[shift + 1] * 0x100 + array[shift]);
			if (sign) i = (short)-(int)i;
			float f = (float)((float)i / 0x80);
			return sign ? -f : f;
		}

		// 1 слово беззнаковое действительное
		// bits: SIII IINN NNNN NNNN
		public static float bytes_2_unsigned_float1024(ref byte[] array, int shift)
		{
			//short i = (short)(Array[shift + 1] * 0x100 + Array[shift]);
			ushort i = (ushort)(array[shift + 1] * 0x100 + array[shift]);
			float f = (float)((float)i / 0x400);
			return f;
		}

		// 1 слово знаковое действительное
		// bits: SIII IINN NNNN NNNN
		public static float bytes_2_signed_float1024(ref byte[] array, int shift)
		{
			//bool sign = (Array[shift + 1] / 0x80 == 1);
			////bool sign = (Array[shift + 1] >> 7 == 1);
			//short i = (short)(Array[shift + 1] * 0x100 + Array[shift]);
			//if (sign) i = (short)-(int)i;
			//float f = (float)i / 1024;
			//return sign ? -f : f;
			short i = (short)((array[shift + 1] << 8) + array[shift]);
			float f = (float)i / 0x400;
			return f;
		}

		// 1 слово беззнаковое действительное
		public static float bytes_2_float1w65536(ref byte[] array, int shift)
		{
			return (float)Math.Round(((float)(array[shift + 1] * 0x100 + array[shift + 0]) / 0x10000), 3);
		}

		// 1 слово беззнаковое действительное * 100 %
		public static float bytes_2_float1w65536_percent(ref byte[] array, int shift)
		{
			return (float)Math.Round(((float)(array[shift + 1] * 0x100 + array[shift + 0]) * 
				100 / 0x10000), 3);
		}

		// 1 слово знаковое действительное
		public static float bytes_2_signed_float1w65536(ref byte[] array, int shift)
		{
			bool sign = (array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			short i = (short)(array[shift + 1] * 0x100 + array[shift]);
			if (sign) i = (short)-i;
			float f = (float)Math.Round((float)i / 0x10000, 4);
			return sign ? -f : f;
		}

		// 1 слово знаковое действительное проценты
		public static float bytes_2_signed_float1w65536_percent(ref byte[] array, int shift)
		{
			bool sign = (array[shift + 1] / 128 == 1);
			//bool sign = (array[shift + 1] >> 7 == 1);
			short i = (short)(array[shift + 1] * 0x100 + array[shift]);
			if (sign) i = (short)-i;
			float f = (float)Math.Round((float)i / 0x10000, 4);
			return sign ? -f * 100 : f * 100;
		}

		#endregion

		#endregion

		#region From 4 Bytes

		#region Integer

		// 2 слова знаковое целое
		public static int bytes_2_int(ref byte[] array, int shift)
		{
			/*int i = array[shift + 3] * 0x1000000 + array[shift + 2] * 0x10000 +
				array[shift + 1] * 0x100 + array[shift];
			return i;*/ // этот вариант полностью идентичен второму

			//bool sign = (Array[shift + 3] / 128 == 1);
			uint lo, hi;
			lo = (uint)(array[shift + 1] * 0x100 + array[shift]);
			hi = (uint)(array[shift + 3] * 0x100 + array[shift + 2]);
			return (int)(hi * 0x10000 + lo);
			//return sign ? -res : res;
		}

		// 2 слова беззнаковое целое
		//public static uint bytes_2_uint(ref byte[] array, int shift)
		//{
		//    uint i = array[shift + 3] * 0x1000000 + array[shift + 2] * 0x10000 +
		//        array[shift + 1] * 0x100 + array[shift];
		//    return i;
		//}

		// 2 слова беззнаковое целое
		public static uint bytes_2_uint(ref byte[] array, int shift)
		{
			uint lo, hi;
			lo = (uint)(array[shift + 3] * 0x100 + array[shift + 2]);
			hi = (uint)(array[shift + 1] * 0x100 + array[shift]);
			return hi * 0x10000 + lo;
		}

		// 2 слова беззнаковое целое
		public static uint bytes_2_uint_new(ref byte[] array, int shift)
		{
			uint lo, hi;
			lo = (uint)(array[shift + 1] * 0x100 + array[shift]);
			hi = (uint)(array[shift + 3] * 0x100 + array[shift + 2]);
			return hi * 0x10000 + lo;
		}

		#endregion

		#region Real

		// 2 слова действительное знаковое с 10 битами после запятой
		public static float bytes_2_signed_float2w1024(ref byte[] array, int shift)
		{
			bool sign = (array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			int i = array[shift + 1] * 0x1000000 + array[shift] * 0x10000 + array[shift + 3] * 0x100 +
				array[shift + 2];
			if (sign) i = -i;
			float f = (float)i / 1024;
			return sign ? -f : f;
		}

		public static float bytes_2_signed_float_Q_6_25(ref byte[] array, int shift)
		{
			bool sign = (array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			int i = array[shift + 1] * 0x1000000 + array[shift] * 0x10000 + array[shift + 3] * 0x100 + 
				array[shift + 2];
			if (sign) i = -i;
			float f = (float)i / 33554432;
			return sign ? -f : f;
		}

		// 1 слово беззнаковое действительное
		public static float bytes_2_signed_float_Q_7_8(ref byte[] array, int shift)
		{
			int i = array[shift + 1] * 0x100 + array[shift + 0];
			float f = (float)i / 256;
			return f;
		}

		public static float bytes_2_signed_float_Q_0_31(ref byte[] array, int shift)
		{
			bool sign = (array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			int i = array[shift + 1] * 0x1000000 + array[shift] * 0x10000 + array[shift + 3] * 0x100 + 
				array[shift + 2];
			if (sign) i = -i;
			float f = (float)i / 2147483648;
			return sign ? -f : f;
		}

		public static float bytes_2_signed_float_Q_0_31_new(ref byte[] array, int shift)
		{
			bool sign = (array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			int i = array[shift + 3] * 0x1000000 + array[shift + 2] * 0x10000 + array[shift + 1] * 0x100 +
				array[shift + 0];
			if (sign) i = -i;
			float f = (float)i / 2147483648;
			return sign ? -f : f;
		}

		public static float bytes_2_signed_float_Q_4_27(ref byte[] array, int shift)
		{
			bool sign = (array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			int i = array[shift + 1] * 0x1000000 + array[shift] * 0x10000 + array[shift + 3] * 0x100 + 
				array[shift + 2];
			if (sign) i = -i;
			float f = (float)i / 134217728;
			return sign ? -f : f;
		}

		public static float bytes_2_signed_float_Q_4_27_new(ref byte[] array, int shift)
		{
			bool sign = (array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			int i = array[shift + 3] * 0x1000000 + array[shift + 2] * 0x10000 + array[shift + 1] * 0x100 +
				array[shift + 0];
			if (sign) i = -i;
			float f = (float)i / 134217728;
			return sign ? -f : f;
		}

		public static float bytes_2_unsigned_float_Q_4_27(ref byte[] array, int shift)
		{
			uint i = (uint)(array[shift + 1]) * 0x1000000 + (uint)(array[shift]) * 0x10000
					+ (uint)(array[shift + 3]) * 0x100 + (uint)(array[shift + 2]);
			float f = (float)i / 134217728;
			return f;
		}

		public static float bytes_2_signed_float_Q_5_26(ref byte[] array, int shift)
		{
			bool sign = (array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			int i = array[shift + 1] * 0x1000000 + array[shift] * 0x10000 + array[shift + 3] * 0x100 + array[shift + 2];
			if (sign) i = -i;
			float f = (float)i / 67108864;
			return sign ? -f : f;
		}

		public static float bytes_2_signed_float_Q_8_23(ref byte[] array, int shift)
		{
			bool sign = (array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			int i = array[shift + 1] * 0x1000000 + array[shift] * 0x10000 + array[shift + 3] * 0x100 + array[shift + 2];
			if (sign) i = -i;
			float f = (float)i / 8388608;
			return sign ? -f : f;
		}

		public static float bytes_2_signed_float_Q_10_21(ref byte[] array, int shift)
		{
			bool sign = (array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			int i = array[shift + 1] * 0x1000000 + array[shift] * 0x10000 + array[shift + 3] * 0x100 + array[shift + 2];
			if (sign) i = -i;
			float f = (float)i / 2097152;
			return sign ? -f : f;
		}

		public static float bytes_2_signed_float_Q_1_30(ref byte[] array, int shift)
		{
			//bool sign = (Array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			int i = array[shift + 1] * 0x1000000 + array[shift] * 0x10000 + array[shift + 3] * 0x100 + array[shift + 2];
			float f = (float)i / 1073741824;
			return f;
		}

		public static float bytes_2_signed_float_Q_7_24(ref byte[] array, int shift)
		{
			//short i = (short)((Array[shift + 1] << 8) + Array[shift]);
			//float f = (float)i / 512;
			//return f;

			int i = array[shift + 1] * 0x1000000 + array[shift] * 0x10000 + array[shift + 3] * 0x100 + array[shift + 2];
			float f = (float)i / 16777216;
			return f;
		}

        public static float bytes_2_signed_float_Q_15_16_new(ref byte[] array, int shift)
        {
            //bool sign = (array[shift + 1] / 128 == 1);
            //bool sign = (array[shift + 1] >> 7 == 1);
			int i = array[shift + 3] * 0x1000000 + array[shift + 2] * 0x10000 + 
				array[shift + 1] * 0x100 + array[shift + 0];
            float f = (float)i / 65536;
            return f;
        }

		// 2 слова знаковое действительное в процентах!
		public static float bytes_2_signed_float2w65536_percent(ref byte[] Array, int shift)
		{
			bool sign = (Array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			int i = Array[shift + 1] * 0x1000000 + Array[shift] * 0x10000 + Array[shift + 3] * 0x100 + Array[shift + 2];
			if (sign) i = -i;
			float f = (float)Math.Round((float)i / 0x10000, 4);
			return sign ? -f * 100 : f * 100;
		}

		// 2 слова беззнаковое действительное
		public static float bytes_2_float2w65536(ref byte[] Array, int shift)
		{
			return (float)Math.Round(((float)(Array[shift + 1] * 0x100 + Array[shift + 0]) + (float)(Array[shift + 3] * 0x100 + Array[shift + 2]) / 0x10000), 4);
		}

		// 2 слова беззнаковое действительное
		public static float bytes_2_float2wIEEE754(ref byte[] array, int shift, bool round)
		{
			//byte[] buf = new byte[] { Array[shift + 2], Array[shift + 3], Array[shift + 0], Array[shift + 1] };
			byte[] buf = new byte[] { array[shift + 0], array[shift + 1], array[shift + 2], array[shift + 3] };
			float res = BitConverter.ToSingle(buf, 0);
			if (round)
				res = (float)Math.Round(res, 15);
			if (Single.IsNaN(res))
				System.Diagnostics.Debug.WriteLine("NaN in bytes_2_float2wIEEE754() function!!!");
			return Single.IsNaN(res) ? 0 : res;
		}

		// 2 слова беззнаковое действительное
		public static float bytes_2_float2wIEEE754_old(ref byte[] array, int shift, bool round)
		{
			byte[] buf = new byte[] { array[shift + 2], array[shift + 3], array[shift + 0], array[shift + 1] };
			float res = BitConverter.ToSingle(buf, 0);
			if (round)
				res = (float)Math.Round(res, 15);
			if (Single.IsNaN(res))
				System.Diagnostics.Debug.WriteLine("NaN in bytes_2_float2wIEEE754_old() function!!!");
			return Single.IsNaN(res) ? 0 : res;
		}

		// 2 слова знаковое действительное
		public static float bytes_2_signed_float2w65536(ref byte[] array, int shift)
		{
			bool sign = (array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);
			int i = array[shift + 1] * 0x1000000 + array[shift] * 0x10000 + array[shift + 3] * 0x100 + array[shift + 2];
			if (sign) i = -i;
			float f = (float)Math.Round((float)i / 0x10000, 4);
			return sign ? -f : f;
		}

		// 2 слова знаковое действительное
		public static float bytes_2_float2wIEEE754_Et33(ref byte[] array, int shift)
		{
			byte[] buf = new byte[] { array[shift + 3], array[shift + 2], array[shift + 1], array[shift + 0] };
			float res = BitConverter.ToSingle(buf, 0);
			//if (round)
			res = (float)Math.Round(res, 15);
			if (Single.IsNaN(res))
				System.Diagnostics.Debug.WriteLine("NaN in bytes_2_float2wIEEE754_old() function!!!");
			return Single.IsNaN(res) ? 0 : res;
		}

		#endregion

		#endregion

		#region From 6 Bytes

		// 3 слова в знаковое действительное
		public static float bytes_2_signed_float3w65536(ref byte[] Array, int shift)
		{
			bool sign = (Array[shift + 1] / 128 == 1);
			//bool sign = (Array[shift + 1] >> 7 == 1);

			long i = Array[shift + 1] * 0x10000000000 + Array[shift + 0] * 0x100000000 +
				(long)Array[shift + 3] * 0x1000000 + (long)Array[shift + 2] * 0x10000 +
				(long)Array[shift + 5] * 0x100 + (long)Array[shift + 4];
			if (sign) i = -(long)((ulong)i + (ulong)0xFFFF000000000000);
			float f = (float)Math.Round((float)i / 0x10000, 4);
			return sign ? -f : f;
		}

		#endregion

		#endregion

		#region Number To Bytes

		public static void int_2_bytes(int val, ref byte[] array, int shift)
		{
			array[shift + 3] = (byte)((uint)val / 0x1000000 % 0x100);
			array[shift + 2] = (byte)((uint)val / 0x10000 % 0x100);
			array[shift + 1] = (byte)((uint)val / 0x100 % 0x100);
			array[shift + 0] = (byte)((uint)val % 0x100);
		}

		public static bool float2wIEEE754_old_2_bytes(ref byte[] buf, Single value)
		{
			buf = BitConverter.GetBytes(value);
			byte temp = buf[0];
			buf[0] = buf[2]; buf[2] = temp;
			temp = buf[1];
			buf[1] = buf[3]; buf[3] = temp;
			return true;
		}

		// знаковое действительное в 1 слово
		public static void signed_float1024_2_bytes(float f, ref byte[] array, int shift)
		{
			short i = (short)(f * 0x400);
			array[shift] = (byte)(i & 0xFF);
			array[shift + 1] = (byte)((i >> 8) & 0xFF);
		}

        // знаковое действительное в 1 слово
        public static void signed_float65536_2_bytes(float f, ref byte[] array, int shift)
        {
            short i = (short)(f * 0x10000);
            array[shift] = (byte)(i & 0xFF);
            array[shift + 1] = (byte)((i >> 8) & 0xFF);
        }

		// знаковое действительное в 1 слово
		public static void signed_float8192_2_bytes(float f, ref byte[] array, int shift)
		{
			short i = (short)(f * 0x2000);
			array[shift] = (byte)(i & 0xFF);
			array[shift + 1] = (byte)((i >> 8) & 0xFF);
		}

		// знаковое действительное в 1 слово
		public static void signed_float8192_2_bytes_new(float f, ref byte[] array, int shift)
		{
			int i = (int)(f * 0x2000);
			if (i > 0x7FFF) i = 0x7FFF;
			if (i < -0x8000) i = -0x8000;
			short s = (short)i;
			array[shift] = (byte)(s & 0xFF);
			array[shift + 1] = (byte)((s >> 8) & 0xFF);
		}

		// знаковое действительное в 2 слова 
		public static void signed_float2w65536_2_bytes(float f, ref byte[] array, int shift)
		{
			int i = Convert.ToInt32(f * 0x10000);

			array[shift + 1] = (byte)((uint)i / 0x1000000 % 0x100);
			array[shift + 0] = (byte)((uint)i / 0x10000 % 0x100);
			array[shift + 3] = (byte)((uint)i / 0x100 % 0x100);
			array[shift + 2] = (byte)((uint)i % 0x100);
		}

		// знаковое действительное в процентах в 2 слова 
		public static void signed_float_percent_2w65536_2_bytes(float f, ref byte[] array, int shift)
		{
			f /= 100;

			int i = Convert.ToInt32(f * 0x10000);

			array[shift + 1] = (byte)((uint)i / 0x1000000 % 0x100);
			array[shift + 0] = (byte)((uint)i / 0x10000 % 0x100);
			array[shift + 3] = (byte)((uint)i / 0x100 % 0x100);
			array[shift + 2] = (byte)((uint)i % 0x100);
		}

		// знаковое действительное в 2 слова 
		public static void signed_float2w_Q_15_16_2_bytes(float f, ref byte[] array, int shift)
		{
			try
			{
				int i = Convert.ToInt32(f * 0x10000);

				array[shift + 3] = (byte)((uint)i / 0x1000000 % 0x100);
				array[shift + 2] = (byte)((uint)i / 0x10000 % 0x100);
				array[shift + 1] = (byte)((uint)i / 0x100 % 0x100);
				array[shift + 0] = (byte)((uint)i % 0x100);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in signed_float2w_Q_15_16_2_bytes():");
				throw;
			}
		}

		// беззнаковое действительное в 2 слова 
		public static void float2w65536_2_bytes(float f, ref byte[] array, int shift)
		{
			int i = Convert.ToInt32(f * 0x10000);

			array[shift + 1] = (byte)((uint)i / 0x1000000 % 0x100);
			array[shift + 0] = (byte)((uint)i / 0x10000 % 0x100);
			array[shift + 3] = (byte)((uint)i / 0x100 % 0x100);
			array[shift + 2] = (byte)((uint)i % 0x100);
		}

		public static void ushort_2_bytes(ushort value, ref byte[] array, int shift)
		{
			array[shift + 1] = (byte)(value / 0x100);
			array[shift    ] = (byte)(value % 0x100);
		}

		// беззнаковое действительное в 1 слово
		public static void float1w65536_2_bytes(float f, ref byte[] array, int shift)
		{
			ushort i = Convert.ToUInt16(f * 0x10000);
			array[shift + 1] = (byte)((ushort)i / 0x100 % 0x100);
			array[shift + 0] = (byte)((ushort)i % 0x100);
		}

		// беззнаковое действительное в процентах в 1 слово
		public static void float_1w65536_percent_2_bytes(float f, ref byte[] array, int shift)
		{
			f /= 100;
			ushort i = Convert.ToUInt16(f * 0x10000);
			array[shift + 1] = (byte)((ushort)i / 0x100 % 0x100);
			array[shift + 0] = (byte)((ushort)i % 0x100);
		}

		// знаковое действительное в 1 слово
		public static void signed_float1w65536_2_bytes(float f, ref byte[] array, int shift)
		{
			short s = (short)Math.Round(f * 0x10000);
			array[shift + 1] = (byte)((ushort)s / 0x100 % 0x100);
			array[shift + 0] = (byte)((ushort)s % 0x100);
		}

		// знаковое действительное в процентах в 1 слово
		public static void signed_float_percent_1w65536_2_bytes(float f, ref byte[] array, int shift)
		{
			f /= 100;
			short s = (short)Math.Round(f * 0x10000);
			array[shift + 1] = (byte)((ushort)s / 0x100 % 0x100);
			array[shift + 0] = (byte)((ushort)s % 0x100);
		}

		#endregion

		#endregion

		#region DAA

		// Преобразует байт, упакованный в DAA в его реальное значение (0x31 в 31)
		public static byte byte_2_DAA(byte Value)
		{
			return (byte)(Value / 0x10 * 10 + Value % 0x10);
		}

		// Преобразует байт в  упакованный в DAA (31 в 0x31)
		public static byte DAA_2_byte(byte Value)
		{
			byte res = 0;
			res = (byte)(Value / 10 * 0x10 + Value % 10);
			return (byte)(res);
		}

		// Преобразует 2 байта, упакованных в DAA в его реальное значение (0x21, 0x32 в 0x3221)
		public static ushort bytes_2_DAA(ref byte[] array, int shift)
		{
			return (ushort)((array[shift+1] / 0x10 * 10 + array[shift+1] % 0x10) * 100 + array[shift] / 0x10 * 10 + array[shift] % 0x10);
		}

		#endregion

		#region Date Time Conversion

		public static DateTime bytes_2_DateTime(ref byte[] array, int shift)
		{
			try
			{
				return new DateTime(
					bytes_2_DAA(ref array, shift),	//год
					byte_2_DAA(array[shift + 3]),	//мес
					byte_2_DAA(array[shift + 2]),	//ден
					byte_2_DAA(array[shift + 5]),	//час
					byte_2_DAA(array[shift + 4]),	//мин
					byte_2_DAA(array[shift + 6])	//сек
					);
			}
			catch
			{
				return DateTime.MinValue;
			}
		}

		public static TimeSpan bytes_2_TimeSpanHhMm(ref byte[] array, int shift)
		{
			try
			{
				return new TimeSpan(
					array[shift + 1],// / 256,		//часы
					array[shift] / 256,				//мин
					0
					);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				return TimeSpan.Zero;
			}
		}

		public static DateTime bytes_2_DateTimeEtPQP_A(ref byte[] array, int shift, string info)
		{
			ushort iYear = 0, iMo = 0, iDay = 0;
			ushort iHour = 0, iMin = 0, iSec = 0, iMilliSec = 0;
			try
			{
				iYear = Conversions.bytes_2_ushort(ref array, shift + 4);
				iMo = Conversions.bytes_2_ushort(ref array, shift + 2);
				iDay = Conversions.bytes_2_ushort(ref array, shift + 0);
				iHour = Conversions.bytes_2_ushort(ref array, shift + 6);
				iMin = Conversions.bytes_2_ushort(ref array, shift + 8);
				iSec = Conversions.bytes_2_ushort(ref array, shift + 10);
				iMilliSec = Conversions.bytes_2_ushort(ref array, shift + 24);
				if (iMilliSec > 999) iMilliSec = 999;
				return new DateTime(iYear, iMo, iDay, iHour, iMin, iSec, iMilliSec);
			}
			catch (ArgumentOutOfRangeException)
			{
				EmService.WriteToLogFailed("Error in bytes_2_DateTimeEtPQP_A 1: " + info);
				EmService.WriteToLogFailed(
						string.Format("{0}, {1}, {2}, {3}, {4}, {5}", 
						iYear, iMo, iDay, iHour, iMin, iSec));
				return DateTime.MinValue;//throw;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in bytes_2_DateTimeEtPQP_A 2");
				throw;
			}
		}

		public static DateTime bytes_2_DateTimeEtPQP_A_Local(ref byte[] array, int shift, string info)
		{
			ushort iYear = 0, iMo = 0, iDay = 0;
			ushort iHour = 0, iMin = 0, iSec = 0, iMilliSec = 0;
			try
			{
				iYear = Conversions.bytes_2_ushort(ref array, shift + 16);
				iMo = Conversions.bytes_2_ushort(ref array, shift + 14);
				iDay = Conversions.bytes_2_ushort(ref array, shift + 12);
				iHour = Conversions.bytes_2_ushort(ref array, shift + 18);
				iMin = Conversions.bytes_2_ushort(ref array, shift + 20);
				iSec = Conversions.bytes_2_ushort(ref array, shift + 22);
				iMilliSec = Conversions.bytes_2_ushort(ref array, shift + 24);
				if (iMilliSec > 999) iMilliSec = 999;
				ushort zone = bytes_2_ushort(ref array, shift + 26);
				DateTime res = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec, iMilliSec);
				return res;
			}
			catch (ArgumentOutOfRangeException)
			{
				EmService.WriteToLogFailed("Error in bytes_2_DateTimeEtPQP_A 1: " + info);
				EmService.WriteToLogFailed(
						string.Format("{0}, {1}, {2}, {3}, {4}, {5}",
						iYear, iMo, iDay, iHour, iMin, iSec));
				return DateTime.MinValue;//throw;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in bytes_2_DateTimeEtPQP_A 2");
				throw;
			}
		}

		public static DateTime bytes_2_DateTimeEtPQP_A_Local(ref byte[] array, int shift, string info, out ushort zone)
		{
			ushort iYear = 0, iMo = 0, iDay = 0;
			ushort iHour = 0, iMin = 0, iSec = 0, iMilliSec = 0;
			try
			{
				iYear = Conversions.bytes_2_ushort(ref array, shift + 16);
				iMo = Conversions.bytes_2_ushort(ref array, shift + 14);
				iDay = Conversions.bytes_2_ushort(ref array, shift + 12);
				iHour = Conversions.bytes_2_ushort(ref array, shift + 18);
				iMin = Conversions.bytes_2_ushort(ref array, shift + 20);
				iSec = Conversions.bytes_2_ushort(ref array, shift + 22);
				iMilliSec = Conversions.bytes_2_ushort(ref array, shift + 24);
				if (iMilliSec > 999) iMilliSec = 999;
				zone = bytes_2_ushort(ref array, shift + 26);
				DateTime res = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec, iMilliSec);
				return res;
			}
			catch (ArgumentOutOfRangeException)
			{
				EmService.WriteToLogFailed("Error in bytes_2_DateTimeEtPQP_A 1: " + info);
				EmService.WriteToLogFailed(
						string.Format("{0}, {1}, {2}, {3}, {4}, {5}",
						iYear, iMo, iDay, iHour, iMin, iSec));
				zone = 0;
				return DateTime.MinValue;//throw;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in bytes_2_DateTimeEtPQP_A 2");
				throw;
			}
		}

		//public static bool dateTime_2_bytes_EtPQP_A(ref byte[] array, ref DateTime dt)
		//{
			
		//    try
		//    {
		//        Conversions.ushort_2_bytes((ushort)dt.Year, ref array, 4);
		//        Conversions.ushort_2_bytes((ushort)dt.Month, ref array, 2);
		//        Conversions.ushort_2_bytes((ushort)dt.Day, ref array, 0);
		//        Conversions.ushort_2_bytes((ushort)dt.Hour, ref array, 6);
		//        Conversions.ushort_2_bytes((ushort)dt.Minute, ref array, 8);
		//        Conversions.ushort_2_bytes((ushort)dt.Second, ref array, 10);
		//        return true;
		//    }
		//    catch (Exception ex)
		//    {
		//        EmService.DumpException(ex, "Error in dateTime_2_bytes_EtPQP_A");
		//        throw;
		//    }
		//}

		public static DateTime bytes_2_DateTimeSLIP(ref byte[] array, int shift, string info)
		{
			try
			{
				ushort year = Conversions.bytes_2_ushort(ref array, shift + 6);
				if (year < 2008) year += 2000;
				ushort mo = Conversions.bytes_2_ushort(ref array, shift + 8);
				ushort day = Conversions.bytes_2_ushort(ref array, shift + 10);
				ushort hour = Conversions.bytes_2_ushort(ref array, shift + 0);
				ushort min = Conversions.bytes_2_ushort(ref array, shift + 2);
				ushort sec = Conversions.bytes_2_ushort(ref array, shift + 4);

				try
				{
					return new DateTime(year, mo, day, hour, min, sec);
				}
				catch (ArgumentOutOfRangeException)
				{
					EmService.WriteToLogFailed("Error: " + info);
					EmService.WriteToLogFailed(string.Format(
						"bytes_2_DateTimeSLIP: Invalid data: {0}, {1}, {2}, {3}, {4}, {5}",
						year, mo, day, hour, min, sec));

					if (mo == 65535 && day == 65535)
					{
						return DateTime.Now;
					}

					return DateTime.MinValue;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in bytes_2_DateTimeSLIP");
				throw;
			}
		}

		public static DateTime bytes_2_DateTimeSLIP2(ref byte[] array, int shift)
		{
			try
			{
				ushort year = (ushort)(array[0 + shift + 8] + (array[1 + shift + 8] << 8));
				byte day = array[2 + shift + 8];
				byte mo = array[3 + shift + 8];
				byte hour = array[5 + shift + 8];
				byte min = array[4 + shift + 8];
				byte sec = array[6 + shift + 8];

				try
				{
					return new DateTime(year, mo, day, hour, min, sec);
				}
				catch (ArgumentOutOfRangeException)
				{
					EmService.WriteToLogFailed(string.Format(
						"bytes_2_DateTimeSLIP2: Invalid data: {0}, {1}, {2}, {3}, {4}, {5}",
						year, mo, day, hour, min, sec));
					return DateTime.MinValue;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in bytes_2_DateTimeSLIP");
				throw;
			}
		}

		public static DateTime bytes_2_TimeSLIP(ref byte[] array, int shift)
		{
			try
			{
				int year = DateTime.Now.Year;
				int day = DateTime.Now.Day;
				int mo = DateTime.Now.Month;
				byte hour = array[1 + shift];
				byte min = array[0 + shift];
				byte sec = 0;

				try
				{
					return new DateTime(year, mo, day, hour, min, sec);
				}
				catch (ArgumentOutOfRangeException)
				{
					EmService.WriteToLogFailed(string.Format(
						"bytes_2_DateTimeSLIP: Invalid data: {0}, {1}, {2}, {3}, {4}, {5}",
						year, mo, day, hour, min, sec));
					return DateTime.MinValue;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in bytes_2_TimeSLIP");
				throw;
			}
		}

		public static bool StringTimeSLIP_2_bytes(ref byte[] array, string time)
		{
			try
			{
				array = new byte[2];
				TimeSpan ts = TimeSpan.Parse(time);
				array[0] = (byte)ts.Minutes;
				array[1] = (byte)ts.Hours;
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in TimeSLIP_2_bytes");
				throw;
			}
		}

		public static bool TimeSLIP_2_bytes(ref byte[] array, DateTime dt)
		{
			try
			{
				array = new byte[2];
				array[0] = (byte)dt.Minute;
				array[1] = (byte)dt.Hour;
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in TimeSLIP_2_bytes");
				throw;
			}
		}

		#endregion

		#region String Conversion

		public static string bytes_2_ASCII(ref byte[] array, int Index, int Count)
		{
			// fixing ASCII bug
			for(int i = Index; i < Index + Count; i++)
			{
				if (array[i] >= 0xA0 || array[i] == 0x00) array[i] += 0x20;
			}
         
			// Reversing each 2 bytes
			for(int i = Index; i < Index + Count - 2; i += 2)
			{
				byte Char = array[i];
				array[i] = array[i + 1];
				array[i + 1] = Char;
			}

			return (System.Text.Encoding.Default.GetString(array, Index, Count)).Trim();
		}

		public static string bytes_2_string(ref byte[] array, int shift, int count)
		{
			try
			{
				Encoding enc = Encoding.GetEncoding(0);
				char[] cars = enc.GetChars(array, shift, count);
				string res = new string(cars);
				res = res.Replace('\0', ' ');
				res = res.Trim(' ');

				return res;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in bytes_2_string(): " + ex.Message);
				return string.Empty;
			}
		}

		public static byte[] string_2_bytes(string str)
		{
			try
			{
				Encoding enc = Encoding.GetEncoding(0);
				return enc.GetBytes(str);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in string_2_bytes(): " + ex.Message);
				return null;
			}
		}

		public static string bytes_2_string_Et33(ref byte[] array, int shift, int count)
		{
			try
			{
				//List<byte> temp_list = new List<byte>();

				//for (int i = shift; i < count; i += 2)
				//{
				//    if((i + 1) < count) temp_list.Add(array[i + 1]);
				//    temp_list.Add(array[i]);
				//}
				byte temp;
				for (int i = 0; i < count; i += 2)
				{
					if ((i + 1) < count && (i + 1 + shift) < array.Length)
					{
						temp = array[i + 1 + shift];
						array[i + 1 + shift] = array[i + shift];
						array[i + shift] = temp;
					}
				}

				Encoding enc = Encoding.GetEncoding(0);
				char[] cars = enc.GetChars(array, shift, count);
				string res = new string(cars);
				res = res.Replace('\0', ' ');
				res = res.Trim(' ');

				return res;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in bytes_2_string_Et33(): " + ex.Message);
				return string.Empty;
			}
		}

		#endregion

        #region Object Conversion

        public static float object_2_float(object obj)
		{
			try
			{
				float val = 0;
				try
				{
					val = Convert.ToSingle(obj);
				}
				catch
				{
					try
					{
						val = (float)(obj);
					}
					catch
					{
						try
						{
							val = (float)(double)(obj);
						}
						catch
						{
							val = Single.Parse(obj.ToString());
						}
					}
				}
				return val;
			}
			catch (FormatException)
			{
				return 0;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in object_2_float(): ");
				return 0;
			}
        }

		public static bool object_2_float_en_ru(object obj, out float res)
		{
			try
			{
				string s = obj.ToString();
				res = 0.0F;
				if (Single.TryParse(s, out res))
					return true;
				// если не получили число, пытаемся изменить формат строки
				s = s.Replace('.', ',');
				if (Single.TryParse(s, out res))
					return true;
				s = s.Replace(',', '.');
				if (Single.TryParse(s, out res))
					return true;
				return false;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in Conversions.object_2_float_en_ru():");
				res = 0.0F;
				return false;
			}
		}

        public static double object_2_double(object obj)
        {
            try
            {
                string s = obj.ToString();
                double d = 0.0;
                float f = 0.0F;
                if (Double.TryParse(s, out d))
                    return d;
                if (Single.TryParse(s, out f))
                    return (double)f;
                // если не получили число, пытаемся изменить формат строки
                s = s.Replace('.', ',');
                if (Double.TryParse(s, out d))
                    return d;
                if (Single.TryParse(s, out f))
                    return (double)f;
				s = s.Replace(',', '.');
				if (Double.TryParse(s, out d))
					return d;
				if (Single.TryParse(s, out f))
					return (double)f;
                return 0.0;
            }
            catch (Exception e)
            {
                EmService.DumpException(e, "Error in Conversions.object_2_double():");
                return 0.0;
            }
        }

		public static bool object_2_double(object obj, out double d)
		{
			try
			{
				string s = obj.ToString();
				d = 0.0;
				float f = 0.0F;
				if (Double.TryParse(s, out d))
					return true;
				if (Single.TryParse(s, out f))
				{
					d = (double)f;
					return true;
				}
				// если не получили число, пытаемся изменить формат строки
				s = s.Replace('.', ',');
				if (Double.TryParse(s, out d))
					return true;
				if (Single.TryParse(s, out f))
				{
					d = (double)f;
					return true;
				}
				s = s.Replace(',', '.');
				if (Double.TryParse(s, out d))
					return true;
				if (Single.TryParse(s, out f))
				{
					d = (double)f;
					return true;
				}
				return false;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in Conversions.object_2_double():");
				d = 0.0;
				return false;
			}
		}

        #endregion
    }
}
