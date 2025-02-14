﻿using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace WCell.Core.Cryptography
{
  [GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
  [Serializable]
  public class BigInteger : ISerializable
  {
    /// <summary>
    /// Primes smaller than 2000 to test the generated prime number.
    /// </summary>
    public static readonly int[] primesBelow2000 = new int[303]
    {
      2,
      3,
      5,
      7,
      11,
      13,
      17,
      19,
      23,
      29,
      31,
      37,
      41,
      43,
      47,
      53,
      59,
      61,
      67,
      71,
      73,
      79,
      83,
      89,
      97,
      101,
      103,
      107,
      109,
      113,
      sbyte.MaxValue,
      131,
      137,
      139,
      149,
      151,
      157,
      163,
      167,
      173,
      179,
      181,
      191,
      193,
      197,
      199,
      211,
      223,
      227,
      229,
      233,
      239,
      241,
      251,
      257,
      263,
      269,
      271,
      277,
      281,
      283,
      293,
      307,
      311,
      313,
      317,
      331,
      337,
      347,
      349,
      353,
      359,
      367,
      373,
      379,
      383,
      389,
      397,
      401,
      409,
      419,
      421,
      431,
      433,
      439,
      443,
      449,
      457,
      461,
      463,
      467,
      479,
      487,
      491,
      499,
      503,
      509,
      521,
      523,
      541,
      547,
      557,
      563,
      569,
      571,
      577,
      587,
      593,
      599,
      601,
      607,
      613,
      617,
      619,
      631,
      641,
      643,
      647,
      653,
      659,
      661,
      673,
      677,
      683,
      691,
      701,
      709,
      719,
      727,
      733,
      739,
      743,
      751,
      757,
      761,
      769,
      773,
      787,
      797,
      809,
      811,
      821,
      823,
      827,
      829,
      839,
      853,
      857,
      859,
      863,
      877,
      881,
      883,
      887,
      907,
      911,
      919,
      929,
      937,
      941,
      947,
      953,
      967,
      971,
      977,
      983,
      991,
      997,
      1009,
      1013,
      1019,
      1021,
      1031,
      1033,
      1039,
      1049,
      1051,
      1061,
      1063,
      1069,
      1087,
      1091,
      1093,
      1097,
      1103,
      1109,
      1117,
      1123,
      1129,
      1151,
      1153,
      1163,
      1171,
      1181,
      1187,
      1193,
      1201,
      1213,
      1217,
      1223,
      1229,
      1231,
      1237,
      1249,
      1259,
      1277,
      1279,
      1283,
      1289,
      1291,
      1297,
      1301,
      1303,
      1307,
      1319,
      1321,
      1327,
      1361,
      1367,
      1373,
      1381,
      1399,
      1409,
      1423,
      1427,
      1429,
      1433,
      1439,
      1447,
      1451,
      1453,
      1459,
      1471,
      1481,
      1483,
      1487,
      1489,
      1493,
      1499,
      1511,
      1523,
      1531,
      1543,
      1549,
      1553,
      1559,
      1567,
      1571,
      1579,
      1583,
      1597,
      1601,
      1607,
      1609,
      1613,
      1619,
      1621,
      1627,
      1637,
      1657,
      1663,
      1667,
      1669,
      1693,
      1697,
      1699,
      1709,
      1721,
      1723,
      1733,
      1741,
      1747,
      1753,
      1759,
      1777,
      1783,
      1787,
      1789,
      1801,
      1811,
      1823,
      1831,
      1847,
      1861,
      1867,
      1871,
      1873,
      1877,
      1879,
      1889,
      1901,
      1907,
      1913,
      1931,
      1933,
      1949,
      1951,
      1973,
      1979,
      1987,
      1993,
      1997,
      1999
    };

    /// <summary>Holds bytes from the BigInteger.</summary>
    private uint[] data;

    /// <summary>Maximum length of the BigInteger in uint. (4 bytes)</summary>
    /// <remarks>Change this to suit the required level of precision.</remarks>
    private const int maxLength = 70;

    public int dataLength;
    private SerializationInfo info;

    public BigInteger()
    {
      data = new uint[70];
      dataLength = 1;
    }

    public BigInteger(long value)
    {
      data = new uint[70];
      long num = value;
      for(dataLength = 0; value != 0L && dataLength < 70; ++dataLength)
      {
        data[dataLength] = (uint) ((ulong) value & uint.MaxValue);
        value >>= 32;
      }

      if(num > 0L)
      {
        if(value != 0L || ((int) data[69] & int.MinValue) != 0)
          throw new ArithmeticException("Positive overflow in constructor.");
      }
      else if(num < 0L && (value != -1L || ((int) data[dataLength - 1] & int.MinValue) == 0))
        throw new ArithmeticException("Negative underflow in constructor.");

      if(dataLength != 0)
        return;
      dataLength = 1;
    }

    public BigInteger(ulong value)
    {
      data = new uint[70];
      for(dataLength = 0; value != 0UL && dataLength < 70; ++dataLength)
      {
        data[dataLength] = (uint) (value & uint.MaxValue);
        value >>= 32;
      }

      if(value != 0UL || ((int) data[69] & int.MinValue) != 0)
        throw new ArithmeticException("Positive overflow in constructor.");
      if(dataLength != 0)
        return;
      dataLength = 1;
    }

    public BigInteger(BigInteger bi)
    {
      SetValue(bi);
    }

    public void SetValue(BigInteger bi)
    {
      data = new uint[70];
      dataLength = bi.dataLength;
      for(int index = 0; index < dataLength; ++index)
        data[index] = bi.data[index];
    }

    public BigInteger(string value, int radix)
    {
      BigInteger bigInteger1 = new BigInteger(1L);
      BigInteger bigInteger2 = new BigInteger();
      value = value.ToUpper().Trim();
      int num1 = 0;
      if(value[0] == '-')
        num1 = 1;
      for(int index = value.Length - 1; index >= num1; --index)
      {
        int num2 = value[index];
        int num3 = num2 < 48 || num2 > 57 ? (num2 < 65 || num2 > 90 ? 9999999 : num2 - 65 + 10) : num2 - 48;
        if(num3 >= radix)
          throw new ArithmeticException("Invalid string in constructor.");
        if(value[0] == '-')
          num3 = -num3;
        bigInteger2 += bigInteger1 * num3;
        if(index - 1 >= num1)
          bigInteger1 *= radix;
      }

      if(value[0] == '-')
      {
        if(((int) bigInteger2.data[69] & int.MinValue) == 0)
          throw new ArithmeticException("Negative underflow in constructor.");
      }
      else if(((int) bigInteger2.data[69] & int.MinValue) != 0)
        throw new ArithmeticException("Positive overflow in constructor.");

      data = new uint[70];
      for(int index = 0; index < bigInteger2.dataLength; ++index)
        data[index] = bigInteger2.data[index];
      dataLength = bigInteger2.dataLength;
    }

    public BigInteger(byte[] inData)
    {
      inData = (byte[]) inData.Clone();
      Reverse(inData);
      dataLength = inData.Length >> 2;
      int num = inData.Length & 3;
      if(num != 0)
        ++dataLength;
      if(dataLength > 70)
        throw new ArithmeticException("Byte overflow in constructor.");
      data = new uint[70];
      int index1 = inData.Length - 1;
      int index2 = 0;
      while(index1 >= 3)
      {
        data[index2] =
          (uint) ((inData[index1 - 3] << 24) + (inData[index1 - 2] << 16) +
                  (inData[index1 - 1] << 8)) + inData[index1];
        index1 -= 4;
        ++index2;
      }

      switch(num)
      {
        case 1:
          data[dataLength - 1] = inData[0];
          break;
        case 2:
          data[dataLength - 1] = ((uint) inData[0] << 8) + inData[1];
          break;
        case 3:
          data[dataLength - 1] =
            (uint) ((inData[0] << 16) + (inData[1] << 8)) + inData[2];
          break;
      }

      while(dataLength > 1 && data[dataLength - 1] == 0U)
        --dataLength;
    }

    public BigInteger(byte[] inData, int inLen)
    {
      dataLength = inLen >> 2;
      int num = inLen & 3;
      if(num != 0)
        ++dataLength;
      if(dataLength > 70 || inLen > inData.Length)
        throw new ArithmeticException("Byte overflow in constructor.");
      data = new uint[70];
      int index1 = inLen - 1;
      int index2 = 0;
      while(index1 >= 3)
      {
        data[index2] =
          (uint) ((inData[index1 - 3] << 24) + (inData[index1 - 2] << 16) +
                  (inData[index1 - 1] << 8)) + inData[index1];
        index1 -= 4;
        ++index2;
      }

      switch(num)
      {
        case 1:
          data[dataLength - 1] = inData[0];
          break;
        case 2:
          data[dataLength - 1] = ((uint) inData[0] << 8) + inData[1];
          break;
        case 3:
          data[dataLength - 1] =
            (uint) ((inData[0] << 16) + (inData[1] << 8)) + inData[2];
          break;
      }

      if(dataLength == 0)
        dataLength = 1;
      while(dataLength > 1 && data[dataLength - 1] == 0U)
        --dataLength;
    }

    public BigInteger(uint[] inData)
    {
      dataLength = inData.Length;
      if(dataLength > 70)
        throw new ArithmeticException("Byte overflow in constructor.");
      data = new uint[70];
      int index1 = dataLength - 1;
      int index2 = 0;
      while(index1 >= 0)
      {
        data[index2] = inData[index1];
        --index1;
        ++index2;
      }

      while(dataLength > 1 && data[dataLength - 1] == 0U)
        --dataLength;
    }

    public BigInteger(Random rand, int bitLength)
    {
      data = new uint[70];
      dataLength = 1;
      GenerateRandomBits(bitLength, rand);
    }

    public static explicit operator BigInteger(long value)
    {
      return new BigInteger(value);
    }

    public static explicit operator BigInteger(ulong value)
    {
      return new BigInteger(value);
    }

    public static explicit operator BigInteger(int value)
    {
      return new BigInteger(value);
    }

    public static explicit operator BigInteger(uint value)
    {
      return new BigInteger((ulong) value);
    }

    public static implicit operator BigInteger(byte[] value)
    {
      return new BigInteger(value);
    }

    public static BigInteger operator +(BigInteger bi1, BigInteger bi2)
    {
      BigInteger bigInteger = new BigInteger();
      bigInteger.dataLength = bi1.dataLength > bi2.dataLength ? bi1.dataLength : bi2.dataLength;
      long num1 = 0;
      for(int index = 0; index < bigInteger.dataLength; ++index)
      {
        long num2 = bi1.data[index] + (long) bi2.data[index] + num1;
        num1 = num2 >> 32;
        bigInteger.data[index] = (uint) ((ulong) num2 & uint.MaxValue);
      }

      if(num1 != 0L && bigInteger.dataLength < 70)
      {
        bigInteger.data[bigInteger.dataLength] = (uint) num1;
        ++bigInteger.dataLength;
      }

      while(bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0U)
        --bigInteger.dataLength;
      int index1 = 69;
      if(((int) bi1.data[index1] & int.MinValue) == ((int) bi2.data[index1] & int.MinValue) &&
         ((int) bigInteger.data[index1] & int.MinValue) != ((int) bi1.data[index1] & int.MinValue))
        throw new ArithmeticException();
      return bigInteger;
    }

    public static BigInteger operator +(BigInteger bi1, long bi2)
    {
      return bi1 + (BigInteger) bi2;
    }

    public static BigInteger operator +(BigInteger bi1, ulong bi2)
    {
      return bi1 + (BigInteger) bi2;
    }

    public static BigInteger operator +(BigInteger bi1, int bi2)
    {
      return bi1 + (BigInteger) bi2;
    }

    public static BigInteger operator +(BigInteger bi1, uint bi2)
    {
      return bi1 + (BigInteger) bi2;
    }

    public static BigInteger operator ++(BigInteger bi1)
    {
      BigInteger bigInteger = new BigInteger(bi1);
      long num1 = 1;
      int index1;
      for(index1 = 0; num1 != 0L && index1 < 70; ++index1)
      {
        long num2 = bigInteger.data[index1] + 1L;
        bigInteger.data[index1] = (uint) ((ulong) num2 & uint.MaxValue);
        num1 = num2 >> 32;
      }

      if(index1 > bigInteger.dataLength)
      {
        bigInteger.dataLength = index1;
      }
      else
      {
        while(bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0U)
          --bigInteger.dataLength;
      }

      int index2 = 69;
      if(((int) bi1.data[index2] & int.MinValue) == 0 && ((int) bigInteger.data[index2] & int.MinValue) !=
         ((int) bi1.data[index2] & int.MinValue))
        throw new ArithmeticException("Overflow in ++.");
      return bigInteger;
    }

    public static BigInteger operator -(BigInteger bi1)
    {
      if(bi1.dataLength == 1 && bi1.data[0] == 0U)
        return new BigInteger();
      BigInteger bigInteger = new BigInteger(bi1);
      for(int index = 0; index < 70; ++index)
        bigInteger.data[index] = ~bi1.data[index];
      long num1 = 1;
      for(int index = 0; num1 != 0L && index < 70; ++index)
      {
        long num2 = bigInteger.data[index] + 1L;
        bigInteger.data[index] = (uint) ((ulong) num2 & uint.MaxValue);
        num1 = num2 >> 32;
      }

      if(((int) bi1.data[69] & int.MinValue) == ((int) bigInteger.data[69] & int.MinValue))
        throw new ArithmeticException("Overflow in negation.\n");
      bigInteger.dataLength = 70;
      while(bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0U)
        --bigInteger.dataLength;
      return bigInteger;
    }

    public static BigInteger operator -(BigInteger bi1, BigInteger bi2)
    {
      BigInteger bigInteger = new BigInteger();
      bigInteger.dataLength = bi1.dataLength > bi2.dataLength ? bi1.dataLength : bi2.dataLength;
      long num1 = 0;
      for(int index = 0; index < bigInteger.dataLength; ++index)
      {
        long num2 = bi1.data[index] - (long) bi2.data[index] - num1;
        bigInteger.data[index] = (uint) ((ulong) num2 & uint.MaxValue);
        num1 = num2 >= 0L ? 0L : 1L;
      }

      if(num1 != 0L)
      {
        for(int dataLength = bigInteger.dataLength; dataLength < 70; ++dataLength)
          bigInteger.data[dataLength] = uint.MaxValue;
        bigInteger.dataLength = 70;
      }

      while(bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0U)
        --bigInteger.dataLength;
      int index1 = 69;
      if(((int) bi1.data[index1] & int.MinValue) != ((int) bi2.data[index1] & int.MinValue) &&
         ((int) bigInteger.data[index1] & int.MinValue) != ((int) bi1.data[index1] & int.MinValue))
        throw new ArithmeticException();
      return bigInteger;
    }

    public static BigInteger operator -(BigInteger bi1, long bi2)
    {
      return bi1 - (BigInteger) bi2;
    }

    public static BigInteger operator -(BigInteger bi1, ulong bi2)
    {
      return bi1 - (BigInteger) bi2;
    }

    public static BigInteger operator -(BigInteger bi1, int bi2)
    {
      return bi1 - (BigInteger) bi2;
    }

    public static BigInteger operator -(BigInteger bi1, uint bi2)
    {
      return bi1 - (BigInteger) bi2;
    }

    public static BigInteger operator --(BigInteger bi1)
    {
      BigInteger bigInteger = new BigInteger(bi1);
      bool flag = true;
      int index1;
      for(index1 = 0; flag && index1 < 70; ++index1)
      {
        long num = bigInteger.data[index1] - 1L;
        bigInteger.data[index1] = (uint) ((ulong) num & uint.MaxValue);
        if(num >= 0L)
          flag = false;
      }

      if(index1 > bigInteger.dataLength)
        bigInteger.dataLength = index1;
      while(bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0U)
        --bigInteger.dataLength;
      int index2 = 69;
      if(((int) bi1.data[index2] & int.MinValue) != 0 && ((int) bigInteger.data[index2] & int.MinValue) !=
         ((int) bi1.data[index2] & int.MinValue))
        throw new ArithmeticException("Underflow in --.");
      return bigInteger;
    }

    public static BigInteger operator *(BigInteger bi1, BigInteger bi2)
    {
      int index1 = 69;
      bool flag1 = false;
      bool flag2 = false;
      try
      {
        if(((int) bi1.data[index1] & int.MinValue) != 0)
        {
          flag1 = true;
          bi1 = -bi1;
        }

        if(((int) bi2.data[index1] & int.MinValue) != 0)
        {
          flag2 = true;
          bi2 = -bi2;
        }
      }
      catch(Exception ex)
      {
      }

      BigInteger bigInteger = new BigInteger();
      try
      {
        for(int index2 = 0; index2 < bi1.dataLength; ++index2)
        {
          if(bi1.data[index2] != 0U)
          {
            ulong num1 = 0;
            int index3 = 0;
            int index4 = index2;
            while(index3 < bi2.dataLength)
            {
              ulong num2 = bi1.data[index2] * (ulong) bi2.data[index3] +
                           bigInteger.data[index4] + num1;
              bigInteger.data[index4] = (uint) (num2 & uint.MaxValue);
              num1 = num2 >> 32;
              ++index3;
              ++index4;
            }

            if(num1 != 0UL)
              bigInteger.data[index2 + bi2.dataLength] = (uint) num1;
          }
        }
      }
      catch(Exception ex)
      {
        throw new ArithmeticException("Multiplication overflow.");
      }

      bigInteger.dataLength = bi1.dataLength + bi2.dataLength;
      if(bigInteger.dataLength > 70)
        bigInteger.dataLength = 70;
      while(bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0U)
        --bigInteger.dataLength;
      if(((int) bigInteger.data[index1] & int.MinValue) != 0)
      {
        if(flag1 != flag2 && bigInteger.data[index1] == 2147483648U)
        {
          if(bigInteger.dataLength == 1)
            return bigInteger;
          bool flag3 = true;
          for(int index2 = 0; index2 < bigInteger.dataLength - 1 && flag3; ++index2)
          {
            if(bigInteger.data[index2] != 0U)
              flag3 = false;
          }

          if(flag3)
            return bigInteger;
        }

        throw new ArithmeticException("Multiplication overflow.");
      }

      if(flag1 != flag2)
        return -bigInteger;
      return bigInteger;
    }

    public static BigInteger operator *(BigInteger bi1, long bi2)
    {
      return bi1 * (BigInteger) bi2;
    }

    public static BigInteger operator *(BigInteger bi1, ulong bi2)
    {
      return bi1 * (BigInteger) bi2;
    }

    public static BigInteger operator *(BigInteger bi1, int bi2)
    {
      return bi1 * (BigInteger) bi2;
    }

    public static BigInteger operator *(BigInteger bi1, uint bi2)
    {
      return bi1 * (BigInteger) bi2;
    }

    private static void multiByteDivide(BigInteger bi1, BigInteger bi2, BigInteger outQuotient,
      BigInteger outRemainder)
    {
      uint[] numArray = new uint[70];
      int length1 = bi1.dataLength + 1;
      uint[] buffer = new uint[length1];
      uint num1 = 2147483648;
      uint num2 = bi2.data[bi2.dataLength - 1];
      int shiftVal = 0;
      int num3 = 0;
      while(num1 != 0U && ((int) num2 & (int) num1) == 0)
      {
        ++shiftVal;
        num1 >>= 1;
      }

      for(int index = 0; index < bi1.dataLength; ++index)
        buffer[index] = bi1.data[index];
      shiftLeft(buffer, shiftVal);
      bi2 <<= shiftVal;
      int num4 = length1 - bi2.dataLength;
      int index1 = length1 - 1;
      ulong num5 = bi2.data[bi2.dataLength - 1];
      ulong num6 = bi2.data[bi2.dataLength - 2];
      int length2 = bi2.dataLength + 1;
      uint[] inData = new uint[length2];
      for(; num4 > 0; --num4)
      {
        ulong num7 = ((ulong) buffer[index1] << 32) + buffer[index1 - 1];
        ulong num8 = num7 / num5;
        ulong num9 = num7 % num5;
        bool flag = false;
        while(!flag)
        {
          flag = true;
          if(num8 == 4294967296UL || num8 * num6 > (num9 << 32) + buffer[index1 - 2])
          {
            --num8;
            num9 += num5;
            if(num9 < 4294967296UL)
              flag = false;
          }
        }

        for(int index2 = 0; index2 < length2; ++index2)
          inData[index2] = buffer[index1 - index2];
        BigInteger bigInteger1 = new BigInteger(inData);
        BigInteger bigInteger2 = bi2 * (long) num8;
        while(bigInteger2 > bigInteger1)
        {
          --num8;
          bigInteger2 -= bi2;
        }

        BigInteger bigInteger3 = bigInteger1 - bigInteger2;
        for(int index2 = 0; index2 < length2; ++index2)
          buffer[index1 - index2] = bigInteger3.data[bi2.dataLength - index2];
        numArray[num3++] = (uint) num8;
        --index1;
      }

      outQuotient.dataLength = num3;
      int index3 = 0;
      int index4 = outQuotient.dataLength - 1;
      while(index4 >= 0)
      {
        outQuotient.data[index3] = numArray[index4];
        --index4;
        ++index3;
      }

      for(; index3 < 70; ++index3)
        outQuotient.data[index3] = 0U;
      while(outQuotient.dataLength > 1 && outQuotient.data[outQuotient.dataLength - 1] == 0U)
        --outQuotient.dataLength;
      if(outQuotient.dataLength == 0)
        outQuotient.dataLength = 1;
      outRemainder.dataLength = shiftRight(buffer, shiftVal);
      int index5;
      for(index5 = 0; index5 < outRemainder.dataLength; ++index5)
        outRemainder.data[index5] = buffer[index5];
      for(; index5 < 70; ++index5)
        outRemainder.data[index5] = 0U;
    }

    private static void singleByteDivide(BigInteger bi1, BigInteger bi2, BigInteger outQuotient,
      BigInteger outRemainder)
    {
      uint[] numArray = new uint[70];
      int num1 = 0;
      for(int index = 0; index < 70; ++index)
        outRemainder.data[index] = bi1.data[index];
      outRemainder.dataLength = bi1.dataLength;
      while(outRemainder.dataLength > 1 && outRemainder.data[outRemainder.dataLength - 1] == 0U)
        --outRemainder.dataLength;
      ulong num2 = bi2.data[0];
      int index1 = outRemainder.dataLength - 1;
      ulong num3 = outRemainder.data[index1];
      if(num3 >= num2)
      {
        ulong num4 = num3 / num2;
        numArray[num1++] = (uint) num4;
        outRemainder.data[index1] = (uint) (num3 % num2);
      }

      ulong num5;
      for(int index2 = index1 - 1; index2 >= 0; outRemainder.data[index2--] = (uint) (num5 % num2))
      {
        num5 = ((ulong) outRemainder.data[index2 + 1] << 32) + outRemainder.data[index2];
        ulong num4 = num5 / num2;
        numArray[num1++] = (uint) num4;
        outRemainder.data[index2 + 1] = 0U;
      }

      outQuotient.dataLength = num1;
      int index3 = 0;
      int index4 = outQuotient.dataLength - 1;
      while(index4 >= 0)
      {
        outQuotient.data[index3] = numArray[index4];
        --index4;
        ++index3;
      }

      for(; index3 < 70; ++index3)
        outQuotient.data[index3] = 0U;
      while(outQuotient.dataLength > 1 && outQuotient.data[outQuotient.dataLength - 1] == 0U)
        --outQuotient.dataLength;
      if(outQuotient.dataLength == 0)
        outQuotient.dataLength = 1;
      while(outRemainder.dataLength > 1 && outRemainder.data[outRemainder.dataLength - 1] == 0U)
        --outRemainder.dataLength;
    }

    public static BigInteger operator /(BigInteger bi1, BigInteger bi2)
    {
      BigInteger outQuotient = new BigInteger();
      BigInteger outRemainder = new BigInteger();
      int index = 69;
      bool flag1 = false;
      bool flag2 = false;
      if(((int) bi1.data[index] & int.MinValue) != 0)
      {
        bi1 = -bi1;
        flag2 = true;
      }

      if(((int) bi2.data[index] & int.MinValue) != 0)
      {
        bi2 = -bi2;
        flag1 = true;
      }

      if(bi1 < bi2)
        return outQuotient;
      if(bi2.dataLength == 1)
        singleByteDivide(bi1, bi2, outQuotient, outRemainder);
      else
        multiByteDivide(bi1, bi2, outQuotient, outRemainder);
      if(flag2 != flag1)
        return -outQuotient;
      return outQuotient;
    }

    public static BigInteger operator /(BigInteger bi1, long bi2)
    {
      return bi1 / (BigInteger) bi2;
    }

    public static BigInteger operator /(BigInteger bi1, ulong bi2)
    {
      return bi1 / (BigInteger) bi2;
    }

    public static BigInteger operator /(BigInteger bi1, int bi2)
    {
      return bi1 / (BigInteger) bi2;
    }

    public static BigInteger operator /(BigInteger bi1, uint bi2)
    {
      return bi1 / (BigInteger) bi2;
    }

    public static BigInteger operator %(BigInteger bi1, BigInteger bi2)
    {
      BigInteger outQuotient = new BigInteger();
      BigInteger outRemainder = new BigInteger(bi1);
      int index = 69;
      bool flag = false;
      if(((int) bi1.data[index] & int.MinValue) != 0)
      {
        bi1 = -bi1;
        flag = true;
      }

      if(((int) bi2.data[index] & int.MinValue) != 0)
        bi2 = -bi2;
      if(bi1 < bi2)
        return outRemainder;
      if(bi2.dataLength == 1)
        singleByteDivide(bi1, bi2, outQuotient, outRemainder);
      else
        multiByteDivide(bi1, bi2, outQuotient, outRemainder);
      if(flag)
        return -outRemainder;
      return outRemainder;
    }

    public static BigInteger operator %(BigInteger bi1, long bi2)
    {
      return bi1 % (BigInteger) bi2;
    }

    public static BigInteger operator %(BigInteger bi1, ulong bi2)
    {
      return bi1 % (BigInteger) bi2;
    }

    public static BigInteger operator %(BigInteger bi1, int bi2)
    {
      return bi1 % (BigInteger) bi2;
    }

    public static BigInteger operator %(BigInteger bi1, uint bi2)
    {
      return bi1 % (BigInteger) bi2;
    }

    public static BigInteger operator %(long bi1, BigInteger bi2)
    {
      return (BigInteger) bi1 % bi2;
    }

    public static BigInteger operator %(ulong bi1, BigInteger bi2)
    {
      return (BigInteger) bi1 % bi2;
    }

    public static BigInteger operator %(int bi1, BigInteger bi2)
    {
      return (BigInteger) bi1 % bi2;
    }

    public static BigInteger operator %(uint bi1, BigInteger bi2)
    {
      return (BigInteger) bi1 % bi2;
    }

    public static BigInteger operator <<(BigInteger bi1, int shiftVal)
    {
      BigInteger bigInteger = new BigInteger(bi1);
      bigInteger.dataLength = shiftLeft(bigInteger.data, shiftVal);
      return bigInteger;
    }

    private static int shiftLeft(uint[] buffer, int shiftVal)
    {
      int num1 = 32;
      int length = buffer.Length;
      while(length > 1 && buffer[length - 1] == 0U)
        --length;
      int num2 = shiftVal;
      while(num2 > 0)
      {
        if(num2 < num1)
          num1 = num2;
        ulong num3 = 0;
        for(int index = 0; index < length; ++index)
        {
          ulong num4 = (ulong) buffer[index] << num1 | num3;
          buffer[index] = (uint) (num4 & uint.MaxValue);
          num3 = num4 >> 32;
        }

        if(num3 != 0UL && length + 1 <= buffer.Length)
        {
          buffer[length] = (uint) num3;
          ++length;
        }

        num2 -= num1;
      }

      return length;
    }

    public static BigInteger operator >>(BigInteger bi1, int shiftVal)
    {
      BigInteger bigInteger = new BigInteger(bi1);
      bigInteger.dataLength = shiftRight(bigInteger.data, shiftVal);
      if(((int) bi1.data[69] & int.MinValue) != 0)
      {
        for(int index = 69; index >= bigInteger.dataLength; --index)
          bigInteger.data[index] = uint.MaxValue;
        uint num = 2147483648;
        for(int index = 0;
          index < 32 && ((int) bigInteger.data[bigInteger.dataLength - 1] & (int) num) == 0;
          ++index)
        {
          bigInteger.data[bigInteger.dataLength - 1] |= num;
          num >>= 1;
        }

        bigInteger.dataLength = 70;
      }

      return bigInteger;
    }

    private static int shiftRight(uint[] buffer, int shiftVal)
    {
      int num1 = 32;
      int num2 = 0;
      int length = buffer.Length;
      while(length > 1 && buffer[length - 1] == 0U)
        --length;
      int num3 = shiftVal;
      while(num3 > 0)
      {
        if(num3 < num1)
        {
          num1 = num3;
          num2 = 32 - num1;
        }

        ulong num4 = 0;
        for(int index = length - 1; index >= 0; --index)
        {
          ulong num5 = (ulong) buffer[index] >> num1 | num4;
          num4 = (ulong) buffer[index] << num2;
          buffer[index] = (uint) num5;
        }

        num3 -= num1;
      }

      while(length > 1 && buffer[length - 1] == 0U)
        --length;
      return length;
    }

    /// <summary>NOT operator (1's complement)</summary>
    /// <param name="bi1">the number</param>
    /// <returns></returns>
    public static BigInteger operator ~(BigInteger bi1)
    {
      BigInteger bigInteger = new BigInteger(bi1);
      for(int index = 0; index < 70; ++index)
        bigInteger.data[index] = ~bi1.data[index];
      bigInteger.dataLength = 70;
      while(bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0U)
        --bigInteger.dataLength;
      return bigInteger;
    }

    public static BigInteger operator &(BigInteger bi1, BigInteger bi2)
    {
      BigInteger bigInteger = new BigInteger();
      int num1 = bi1.dataLength > bi2.dataLength ? bi1.dataLength : bi2.dataLength;
      for(int index = 0; index < num1; ++index)
      {
        uint num2 = bi1.data[index] & bi2.data[index];
        bigInteger.data[index] = num2;
      }

      bigInteger.dataLength = 70;
      while(bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0U)
        --bigInteger.dataLength;
      return bigInteger;
    }

    public static BigInteger operator |(BigInteger bi1, BigInteger bi2)
    {
      BigInteger bigInteger = new BigInteger();
      int num1 = bi1.dataLength > bi2.dataLength ? bi1.dataLength : bi2.dataLength;
      for(int index = 0; index < num1; ++index)
      {
        uint num2 = bi1.data[index] | bi2.data[index];
        bigInteger.data[index] = num2;
      }

      bigInteger.dataLength = 70;
      while(bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0U)
        --bigInteger.dataLength;
      return bigInteger;
    }

    public static BigInteger operator ^(BigInteger bi1, BigInteger bi2)
    {
      BigInteger bigInteger = new BigInteger();
      int num1 = bi1.dataLength > bi2.dataLength ? bi1.dataLength : bi2.dataLength;
      for(int index = 0; index < num1; ++index)
      {
        uint num2 = bi1.data[index] ^ bi2.data[index];
        bigInteger.data[index] = num2;
      }

      bigInteger.dataLength = 70;
      while(bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0U)
        --bigInteger.dataLength;
      return bigInteger;
    }

    public static bool operator ==(BigInteger bi1, BigInteger bi2)
    {
      if((object) bi1 == null && (object) bi2 == null)
        return true;
      if((object) bi1 == null || (object) bi2 == null)
        return false;
      return bi1.Equals(bi2);
    }

    public static bool operator ==(BigInteger bi1, uint bi2)
    {
      return bi1 == (BigInteger) bi2;
    }

    public static bool operator ==(BigInteger bi1, int bi2)
    {
      return bi1 == (BigInteger) bi2;
    }

    public static bool operator ==(BigInteger bi1, long bi2)
    {
      return bi1 == (BigInteger) bi2;
    }

    public static bool operator ==(BigInteger bi1, ulong bi2)
    {
      return bi1 == (BigInteger) bi2;
    }

    public static bool operator !=(BigInteger bi1, BigInteger bi2)
    {
      if((object) bi1 == null && (object) bi2 == null)
        return false;
      if((object) bi1 == null || (object) bi2 == null)
        return true;
      return !bi1.Equals(bi2);
    }

    public static bool operator !=(BigInteger bi1, uint bi2)
    {
      return bi1 != (BigInteger) bi2;
    }

    public static bool operator !=(BigInteger bi1, int bi2)
    {
      return bi1 != (BigInteger) bi2;
    }

    public static bool operator !=(BigInteger bi1, long bi2)
    {
      return bi1 != (BigInteger) bi2;
    }

    public static bool operator !=(BigInteger bi1, ulong bi2)
    {
      return bi1 != (BigInteger) bi2;
    }

    public override bool Equals(object o)
    {
      BigInteger bigInteger = (BigInteger) o;
      if(dataLength != bigInteger.dataLength)
        return false;
      for(int index = 0; index < dataLength; ++index)
      {
        if((int) data[index] != (int) bigInteger.data[index])
          return false;
      }

      return true;
    }

    public static bool operator >(BigInteger bi1, BigInteger bi2)
    {
      int index1 = 69;
      if(((int) bi1.data[index1] & int.MinValue) != 0 && ((int) bi2.data[index1] & int.MinValue) == 0)
        return false;
      if(((int) bi1.data[index1] & int.MinValue) == 0 && ((int) bi2.data[index1] & int.MinValue) != 0)
        return true;
      int index2 = (bi1.dataLength > bi2.dataLength ? bi1.dataLength : bi2.dataLength) - 1;
      while(index2 >= 0 && (int) bi1.data[index2] == (int) bi2.data[index2])
        --index2;
      return index2 >= 0 && bi1.data[index2] > bi2.data[index2];
    }

    public static bool operator >(BigInteger bi1, long bi2)
    {
      return bi1 > (BigInteger) bi2;
    }

    public static bool operator >(BigInteger bi1, ulong bi2)
    {
      return bi1 > (BigInteger) bi2;
    }

    public static bool operator >(BigInteger bi1, int bi2)
    {
      return bi1 > (BigInteger) bi2;
    }

    public static bool operator >(BigInteger bi1, uint bi2)
    {
      return bi1 > (BigInteger) bi2;
    }

    public static bool operator <(BigInteger bi1, BigInteger bi2)
    {
      int index1 = 69;
      if(((int) bi1.data[index1] & int.MinValue) != 0 && ((int) bi2.data[index1] & int.MinValue) == 0)
        return true;
      if(((int) bi1.data[index1] & int.MinValue) == 0 && ((int) bi2.data[index1] & int.MinValue) != 0)
        return false;
      int index2 = (bi1.dataLength > bi2.dataLength ? bi1.dataLength : bi2.dataLength) - 1;
      while(index2 >= 0 && (int) bi1.data[index2] == (int) bi2.data[index2])
        --index2;
      return index2 >= 0 && bi1.data[index2] < bi2.data[index2];
    }

    public static bool operator <(BigInteger bi1, long bi2)
    {
      return bi1 < (BigInteger) bi2;
    }

    public static bool operator <(BigInteger bi1, ulong bi2)
    {
      return bi1 < (BigInteger) bi2;
    }

    public static bool operator <(BigInteger bi1, int bi2)
    {
      return bi1 < (BigInteger) bi2;
    }

    public static bool operator <(BigInteger bi1, uint bi2)
    {
      return bi1 < (BigInteger) bi2;
    }

    public static bool operator >=(BigInteger bi1, BigInteger bi2)
    {
      return bi1 == bi2 || bi1 > bi2;
    }

    public static bool operator >=(BigInteger bi1, long bi2)
    {
      return bi1 >= (BigInteger) bi2;
    }

    public static bool operator >=(BigInteger bi1, ulong bi2)
    {
      return bi1 >= (BigInteger) bi2;
    }

    public static bool operator >=(BigInteger bi1, int bi2)
    {
      return bi1 >= (BigInteger) bi2;
    }

    public static bool operator >=(BigInteger bi1, uint bi2)
    {
      return bi1 >= (BigInteger) bi2;
    }

    public static bool operator <=(BigInteger bi1, BigInteger bi2)
    {
      return bi1 == bi2 || bi1 < bi2;
    }

    public static bool operator <=(BigInteger bi1, long bi2)
    {
      return bi1 <= (BigInteger) bi2;
    }

    public static bool operator <=(BigInteger bi1, ulong bi2)
    {
      return bi1 <= (BigInteger) bi2;
    }

    public static bool operator <=(BigInteger bi1, int bi2)
    {
      return bi1 <= (BigInteger) bi2;
    }

    public static bool operator <=(BigInteger bi1, uint bi2)
    {
      return bi1 <= (BigInteger) bi2;
    }

    public BigInteger Max(BigInteger bi)
    {
      if(this > bi)
        return new BigInteger(this);
      return new BigInteger(bi);
    }

    public BigInteger Min(BigInteger bi)
    {
      if(this < bi)
        return new BigInteger(this);
      return new BigInteger(bi);
    }

    public BigInteger Abs()
    {
      if(((int) data[69] & int.MinValue) != 0)
        return -this;
      return new BigInteger(this);
    }

    public override int GetHashCode()
    {
      return ToString().GetHashCode();
    }

    public override string ToString()
    {
      return "0x" + ToString(16);
    }

    public string ToString(int radix)
    {
      if(radix < 2 || radix > 36)
        throw new ArgumentException("Radix must be >= 2 and <= 36");
      string str1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      string str2 = "";
      BigInteger bi1 = this;
      bool flag = false;
      if(((int) bi1.data[69] & int.MinValue) != 0)
      {
        flag = true;
        try
        {
          bi1 = -bi1;
        }
        catch(Exception ex)
        {
        }
      }

      BigInteger outQuotient = new BigInteger();
      BigInteger outRemainder = new BigInteger();
      BigInteger bi2 = new BigInteger(radix);
      if(bi1.dataLength == 1 && bi1.data[0] == 0U)
      {
        str2 = "0";
      }
      else
      {
        for(; bi1.dataLength > 1 || bi1.dataLength == 1 && bi1.data[0] != 0U; bi1 = outQuotient)
        {
          singleByteDivide(bi1, bi2, outQuotient, outRemainder);
          str2 = outRemainder.data[0] >= 10U
            ? ((int) str1[(int) outRemainder.data[0] - 10]) + str2
            : ((int) outRemainder.data[0]) + str2;
        }

        if(flag)
          str2 = "-" + str2;
      }

      return str2;
    }

    public string ToHexString()
    {
      string str = data[dataLength - 1].ToString("X");
      for(int index = dataLength - 2; index >= 0; --index)
        str += data[index].ToString("X8");
      return str;
    }

    public BigInteger ModPow(BigInteger exp, BigInteger n)
    {
      if(((int) exp.data[69] & int.MinValue) != 0)
        throw new ArithmeticException("Positive exponents only.");
      BigInteger bigInteger1 = (BigInteger) 1;
      bool flag = false;
      BigInteger bigInteger2;
      if(((int) data[69] & int.MinValue) != 0)
      {
        bigInteger2 = -this % n;
        flag = true;
      }
      else
        bigInteger2 = this % n;

      if(((int) n.data[69] & int.MinValue) != 0)
        n = -n;
      BigInteger bigInteger3 = new BigInteger();
      int index1 = n.dataLength << 1;
      bigInteger3.data[index1] = 1U;
      bigInteger3.dataLength = index1 + 1;
      BigInteger constant = bigInteger3 / n;
      int num1 = exp.BitCount();
      int num2 = 0;
      for(int index2 = 0; index2 < exp.dataLength; ++index2)
      {
        uint num3 = 1;
        for(int index3 = 0; index3 < 32; ++index3)
        {
          if(((int) exp.data[index2] & (int) num3) != 0)
            bigInteger1 = BarrettReduction(bigInteger1 * bigInteger2, n, constant);
          num3 <<= 1;
          bigInteger2 = BarrettReduction(bigInteger2 * bigInteger2, n, constant);
          if(bigInteger2.dataLength == 1 && bigInteger2.data[0] == 1U)
          {
            if(flag && ((int) exp.data[0] & 1) != 0)
              return -bigInteger1;
            return bigInteger1;
          }

          ++num2;
          if(num2 == num1)
            break;
        }
      }

      if(flag && ((int) exp.data[0] & 1) != 0)
        return -bigInteger1;
      return bigInteger1;
    }

    private static BigInteger BarrettReduction(BigInteger x, BigInteger n, BigInteger constant)
    {
      int dataLength = n.dataLength;
      int index1 = dataLength + 1;
      int num1 = dataLength - 1;
      BigInteger bigInteger1 = new BigInteger();
      int index2 = num1;
      int index3 = 0;
      while(index2 < x.dataLength)
      {
        bigInteger1.data[index3] = x.data[index2];
        ++index2;
        ++index3;
      }

      bigInteger1.dataLength = x.dataLength - num1;
      if(bigInteger1.dataLength <= 0)
        bigInteger1.dataLength = 1;
      BigInteger bigInteger2 = bigInteger1 * constant;
      BigInteger bigInteger3 = new BigInteger();
      int index4 = index1;
      int index5 = 0;
      while(index4 < bigInteger2.dataLength)
      {
        bigInteger3.data[index5] = bigInteger2.data[index4];
        ++index4;
        ++index5;
      }

      bigInteger3.dataLength = bigInteger2.dataLength - index1;
      if(bigInteger3.dataLength <= 0)
        bigInteger3.dataLength = 1;
      BigInteger bigInteger4 = new BigInteger();
      int num2 = x.dataLength > index1 ? index1 : x.dataLength;
      for(int index6 = 0; index6 < num2; ++index6)
        bigInteger4.data[index6] = x.data[index6];
      bigInteger4.dataLength = num2;
      BigInteger bigInteger5 = new BigInteger();
      for(int index6 = 0; index6 < bigInteger3.dataLength; ++index6)
      {
        if(bigInteger3.data[index6] != 0U)
        {
          ulong num3 = 0;
          int index7 = index6;
          for(int index8 = 0; index8 < n.dataLength && index7 < index1; ++index7)
          {
            ulong num4 = bigInteger3.data[index6] * (ulong) n.data[index8] +
                         bigInteger5.data[index7] + num3;
            bigInteger5.data[index7] = (uint) (num4 & uint.MaxValue);
            num3 = num4 >> 32;
            ++index8;
          }

          if(index7 < index1)
            bigInteger5.data[index7] = (uint) num3;
        }
      }

      bigInteger5.dataLength = index1;
      while(bigInteger5.dataLength > 1 && bigInteger5.data[bigInteger5.dataLength - 1] == 0U)
        --bigInteger5.dataLength;
      BigInteger bigInteger6 = bigInteger4 - bigInteger5;
      if(((int) bigInteger6.data[69] & int.MinValue) != 0)
      {
        BigInteger bigInteger7 = new BigInteger();
        bigInteger7.data[index1] = 1U;
        bigInteger7.dataLength = index1 + 1;
        bigInteger6 += bigInteger7;
      }

      while(bigInteger6 >= n)
        bigInteger6 -= n;
      return bigInteger6;
    }

    public BigInteger GCD(BigInteger bi)
    {
      BigInteger bigInteger1 = ((int) data[69] & int.MinValue) == 0 ? this : -this;
      BigInteger bigInteger2 = ((int) bi.data[69] & int.MinValue) == 0 ? bi : -bi;
      BigInteger bigInteger3 = bigInteger2;
      while(bigInteger1.dataLength > 1 || bigInteger1.dataLength == 1 && bigInteger1.data[0] != 0U)
      {
        bigInteger3 = bigInteger1;
        bigInteger1 = bigInteger2 % bigInteger1;
        bigInteger2 = bigInteger3;
      }

      return bigInteger3;
    }

    public void GenerateRandomBits(int bits, Random rand)
    {
      int num1 = bits >> 5;
      int num2 = bits & 31;
      if(num2 != 0)
        ++num1;
      if(num1 > 70)
        throw new ArithmeticException("Number of required bits > maxLength.");
      for(int index = 0; index < num1; ++index)
        data[index] = (uint) (rand.NextDouble() * 4294967296.0);
      for(int index = num1; index < 70; ++index)
        data[index] = 0U;
      if(num2 != 0)
      {
        uint num3 = (uint) (1 << num2 - 1);
        data[num1 - 1] |= num3;
        uint num4 = uint.MaxValue >> 32 - num2;
        data[num1 - 1] &= num4;
      }
      else
        data[num1 - 1] |= 2147483648U;

      dataLength = num1;
      if(dataLength != 0)
        return;
      dataLength = 1;
    }

    public int BitCount()
    {
      while(dataLength > 1 && data[dataLength - 1] == 0U)
        --dataLength;
      uint num1 = data[dataLength - 1];
      uint num2 = 2147483648;
      int num3 = 32;
      while(num3 > 0 && ((int) num1 & (int) num2) == 0)
      {
        --num3;
        num2 >>= 1;
      }

      return num3 + (dataLength - 1 << 5);
    }

    public bool FermatLittleTest(int confidence)
    {
      BigInteger bigInteger1 = ((int) data[69] & int.MinValue) == 0 ? this : -this;
      if(bigInteger1.dataLength == 1)
      {
        if(bigInteger1.data[0] == 0U || bigInteger1.data[0] == 1U)
          return false;
        if(bigInteger1.data[0] == 2U || bigInteger1.data[0] == 3U)
          return true;
      }

      if(((int) bigInteger1.data[0] & 1) == 0)
        return false;
      int num = bigInteger1.BitCount();
      BigInteger bigInteger2 = new BigInteger();
      BigInteger exp = bigInteger1 - new BigInteger(1L);
      Random rand = new Random();
      for(int index = 0; index < confidence; ++index)
      {
        bool flag = false;
        while(!flag)
        {
          int bits = 0;
          while(bits < 2)
            bits = (int) (rand.NextDouble() * num);
          bigInteger2.GenerateRandomBits(bits, rand);
          int dataLength = bigInteger2.dataLength;
          if(dataLength > 1 || dataLength == 1 && bigInteger2.data[0] != 1U)
            flag = true;
        }

        BigInteger bigInteger3 = bigInteger2.GCD(bigInteger1);
        if(bigInteger3.dataLength == 1 && bigInteger3.data[0] != 1U)
          return false;
        BigInteger bigInteger4 = bigInteger2.ModPow(exp, bigInteger1);
        int dataLength1 = bigInteger4.dataLength;
        if(dataLength1 > 1 || dataLength1 == 1 && bigInteger4.data[0] != 1U)
          return false;
      }

      return true;
    }

    public bool RabinMillerTest(int confidence)
    {
      BigInteger bigInteger1 = ((int) data[69] & int.MinValue) == 0 ? this : -this;
      if(bigInteger1.dataLength == 1)
      {
        if(bigInteger1.data[0] == 0U || bigInteger1.data[0] == 1U)
          return false;
        if(bigInteger1.data[0] == 2U || bigInteger1.data[0] == 3U)
          return true;
      }

      if(((int) bigInteger1.data[0] & 1) == 0)
        return false;
      BigInteger bigInteger2 = bigInteger1 - new BigInteger(1L);
      int num1 = 0;
      for(int index1 = 0; index1 < bigInteger2.dataLength; ++index1)
      {
        uint num2 = 1;
        for(int index2 = 0; index2 < 32; ++index2)
        {
          if(((int) bigInteger2.data[index1] & (int) num2) != 0)
          {
            index1 = bigInteger2.dataLength;
            break;
          }

          num2 <<= 1;
          ++num1;
        }
      }

      BigInteger exp = bigInteger2 >> num1;
      int num3 = bigInteger1.BitCount();
      BigInteger bigInteger3 = new BigInteger();
      Random rand = new Random();
      for(int index1 = 0; index1 < confidence; ++index1)
      {
        bool flag1 = false;
        while(!flag1)
        {
          int bits = 0;
          while(bits < 2)
            bits = (int) (rand.NextDouble() * num3);
          bigInteger3.GenerateRandomBits(bits, rand);
          int dataLength = bigInteger3.dataLength;
          if(dataLength > 1 || dataLength == 1 && bigInteger3.data[0] != 1U)
            flag1 = true;
        }

        BigInteger bigInteger4 = bigInteger3.GCD(bigInteger1);
        if(bigInteger4.dataLength == 1 && bigInteger4.data[0] != 1U)
          return false;
        BigInteger bigInteger5 = bigInteger3.ModPow(exp, bigInteger1);
        bool flag2 = false;
        if(bigInteger5.dataLength == 1 && bigInteger5.data[0] == 1U)
          flag2 = true;
        for(int index2 = 0; !flag2 && index2 < num1; ++index2)
        {
          if(bigInteger5 == bigInteger2)
          {
            flag2 = true;
            break;
          }

          bigInteger5 = bigInteger5 * bigInteger5 % bigInteger1;
        }

        if(!flag2)
          return false;
      }

      return true;
    }

    public bool SolovayStrassenTest(int confidence)
    {
      BigInteger bigInteger1 = ((int) data[69] & int.MinValue) == 0 ? this : -this;
      if(bigInteger1.dataLength == 1)
      {
        if(bigInteger1.data[0] == 0U || bigInteger1.data[0] == 1U)
          return false;
        if(bigInteger1.data[0] == 2U || bigInteger1.data[0] == 3U)
          return true;
      }

      if(((int) bigInteger1.data[0] & 1) == 0)
        return false;
      int num = bigInteger1.BitCount();
      BigInteger a = new BigInteger();
      BigInteger bigInteger2 = bigInteger1 - 1;
      BigInteger exp = bigInteger2 >> 1;
      Random rand = new Random();
      for(int index = 0; index < confidence; ++index)
      {
        bool flag = false;
        while(!flag)
        {
          int bits = 0;
          while(bits < 2)
            bits = (int) (rand.NextDouble() * num);
          a.GenerateRandomBits(bits, rand);
          int dataLength = a.dataLength;
          if(dataLength > 1 || dataLength == 1 && a.data[0] != 1U)
            flag = true;
        }

        BigInteger bigInteger3 = a.GCD(bigInteger1);
        if(bigInteger3.dataLength == 1 && bigInteger3.data[0] != 1U)
          return false;
        BigInteger bigInteger4 = a.ModPow(exp, bigInteger1);
        if(bigInteger4 == bigInteger2)
          bigInteger4 = (BigInteger) (-1);
        BigInteger bigInteger5 = (BigInteger) Jacobi(a, bigInteger1);
        if(bigInteger4 != bigInteger5)
          return false;
      }

      return true;
    }

    public bool LucasStrongTest()
    {
      BigInteger thisVal = ((int) data[69] & int.MinValue) == 0 ? this : -this;
      if(thisVal.dataLength == 1)
      {
        if(thisVal.data[0] == 0U || thisVal.data[0] == 1U)
          return false;
        if(thisVal.data[0] == 2U || thisVal.data[0] == 3U)
          return true;
      }

      if(((int) thisVal.data[0] & 1) == 0)
        return false;
      return LucasStrongTestHelper(thisVal);
    }

    private static bool LucasStrongTestHelper(BigInteger thisVal)
    {
      long num1 = 5;
      long num2 = -1;
      long num3 = 0;
      bool flag1 = false;
      while(!flag1)
      {
        int num4;
        switch(Jacobi((BigInteger) num1, thisVal))
        {
          case -1:
            flag1 = true;
            goto label_11;
          case 0:
            num4 = !(thisVal > Math.Abs(num1)) ? 1 : 0;
            break;
          default:
            num4 = 1;
            break;
        }

        if(num4 == 0)
          return false;
        if(num3 == 20L)
        {
          BigInteger bigInteger = thisVal.Sqrt();
          if(bigInteger * bigInteger == thisVal)
            return false;
        }

        num1 = (Math.Abs(num1) + 2L) * num2;
        num2 = -num2;
        label_11:
        ++num3;
      }

      long num5 = 1L - num1 >> 2;
      BigInteger bigInteger1 = thisVal + 1;
      int num6 = 0;
      for(int index1 = 0; index1 < bigInteger1.dataLength; ++index1)
      {
        uint num4 = 1;
        for(int index2 = 0; index2 < 32; ++index2)
        {
          if(((int) bigInteger1.data[index1] & (int) num4) != 0)
          {
            index1 = bigInteger1.dataLength;
            break;
          }

          num4 <<= 1;
          ++num6;
        }
      }

      BigInteger k = bigInteger1 >> num6;
      BigInteger bigInteger2 = new BigInteger();
      int index3 = thisVal.dataLength << 1;
      bigInteger2.data[index3] = 1U;
      bigInteger2.dataLength = index3 + 1;
      BigInteger constant = bigInteger2 / thisVal;
      BigInteger[] bigIntegerArray1 =
        LucasSequenceHelper((BigInteger) 1, (BigInteger) num5, k, thisVal, constant, 0);
      bool flag2 = false;
      if(bigIntegerArray1[0].dataLength == 1 && bigIntegerArray1[0].data[0] == 0U ||
         bigIntegerArray1[1].dataLength == 1 && bigIntegerArray1[1].data[0] == 0U)
        flag2 = true;
      for(int index1 = 1; index1 < num6; ++index1)
      {
        if(!flag2)
        {
          bigIntegerArray1[1] =
            BarrettReduction(bigIntegerArray1[1] * bigIntegerArray1[1], thisVal, constant);
          bigIntegerArray1[1] = (bigIntegerArray1[1] - (bigIntegerArray1[2] << 1)) % thisVal;
          if(bigIntegerArray1[1].dataLength == 1 && bigIntegerArray1[1].data[0] == 0U)
            flag2 = true;
        }

        bigIntegerArray1[2] =
          BarrettReduction(bigIntegerArray1[2] * bigIntegerArray1[2], thisVal, constant);
      }

      if(flag2)
      {
        BigInteger bigInteger3 = thisVal.GCD((BigInteger) num5);
        if(bigInteger3.dataLength == 1 && bigInteger3.data[0] == 1U)
        {
          if(((int) bigIntegerArray1[2].data[69] & int.MinValue) != 0)
          {
            BigInteger[] bigIntegerArray2;
            (bigIntegerArray2 = bigIntegerArray1)[2] = bigIntegerArray2[2] + thisVal;
          }

          BigInteger bigInteger4 = num5 * Jacobi((BigInteger) num5, thisVal) % thisVal;
          if(((int) bigInteger4.data[69] & int.MinValue) != 0)
            bigInteger4 += thisVal;
          if(bigIntegerArray1[2] != bigInteger4)
            flag2 = false;
        }
      }

      return flag2;
    }

    public bool IsProbablePrime(int confidence)
    {
      BigInteger bigInteger1 = ((int) data[69] & int.MinValue) == 0 ? this : -this;
      for(int index = 0; index < primesBelow2000.Length; ++index)
      {
        BigInteger bigInteger2 = (BigInteger) primesBelow2000[index];
        if(!(bigInteger2 >= bigInteger1))
        {
          if((bigInteger1 % bigInteger2).IntValue() == 0)
            return false;
        }
        else
          break;
      }

      return bigInteger1.RabinMillerTest(confidence);
    }

    public bool IsProbablePrime()
    {
      BigInteger bigInteger1 = ((int) data[69] & int.MinValue) == 0 ? this : -this;
      if(bigInteger1.dataLength == 1)
      {
        if(bigInteger1.data[0] == 0U || bigInteger1.data[0] == 1U)
          return false;
        if(bigInteger1.data[0] == 2U || bigInteger1.data[0] == 3U)
          return true;
      }

      if(((int) bigInteger1.data[0] & 1) == 0)
        return false;
      for(int index = 0; index < primesBelow2000.Length; ++index)
      {
        BigInteger bigInteger2 = (BigInteger) primesBelow2000[index];
        if(!(bigInteger2 >= bigInteger1))
        {
          if((bigInteger1 % bigInteger2).IntValue() == 0)
            return false;
        }
        else
          break;
      }

      BigInteger bigInteger3 = bigInteger1 - new BigInteger(1L);
      int num1 = 0;
      for(int index1 = 0; index1 < bigInteger3.dataLength; ++index1)
      {
        uint num2 = 1;
        for(int index2 = 0; index2 < 32; ++index2)
        {
          if(((int) bigInteger3.data[index1] & (int) num2) != 0)
          {
            index1 = bigInteger3.dataLength;
            break;
          }

          num2 <<= 1;
          ++num1;
        }
      }

      BigInteger bigInteger4 = ((BigInteger) 2).ModPow(bigInteger3 >> num1, bigInteger1);
      bool flag = false;
      if(bigInteger4.dataLength == 1 && bigInteger4.data[0] == 1U)
        flag = true;
      for(int index = 0; !flag && index < num1; ++index)
      {
        if(bigInteger4 == bigInteger3)
        {
          flag = true;
          break;
        }

        bigInteger4 = bigInteger4 * bigInteger4 % bigInteger1;
      }

      if(flag)
        flag = LucasStrongTestHelper(bigInteger1);
      return flag;
    }

    public static BigInteger genPseudoPrime(int bits, int confidence, Random rand)
    {
      BigInteger bigInteger = new BigInteger();
      for(bool flag = false; !flag; flag = bigInteger.IsProbablePrime(confidence))
      {
        bigInteger.GenerateRandomBits(bits, rand);
        bigInteger.data[0] |= 1U;
      }

      return bigInteger;
    }

    public BigInteger genCoPrime(int bits, Random rand)
    {
      bool flag = false;
      BigInteger bigInteger1 = new BigInteger();
      while(!flag)
      {
        bigInteger1.GenerateRandomBits(bits, rand);
        BigInteger bigInteger2 = bigInteger1.GCD(this);
        if(bigInteger2.dataLength == 1 && bigInteger2.data[0] == 1U)
          flag = true;
      }

      return bigInteger1;
    }

    public byte LeastSignificantByte()
    {
      return ByteValue();
    }

    public byte ByteValue()
    {
      return (byte) data[0];
    }

    public int IntValue()
    {
      return (int) data[0];
    }

    public long LongValue()
    {
      long num = data[0];
      try
      {
        num |= (long) data[1] << 32;
      }
      catch(Exception ex)
      {
        if(((int) data[0] & int.MinValue) != 0)
          num = (int) data[0];
      }

      return num;
    }

    public static int Jacobi(BigInteger a, BigInteger b)
    {
      if(((int) b.data[0] & 1) == 0)
        throw new ArgumentException("Jacobi defined only for odd integers.");
      if(a >= b)
        a %= b;
      if(a.dataLength == 1 && a.data[0] == 0U)
        return 0;
      if(a.dataLength == 1 && a.data[0] == 1U)
        return 1;
      if(a < 0)
      {
        if(((int) (b - 1).data[0] & 2) == 0)
          return Jacobi(-a, b);
        return -Jacobi(-a, b);
      }

      int num1 = 0;
      for(int index1 = 0; index1 < a.dataLength; ++index1)
      {
        uint num2 = 1;
        for(int index2 = 0; index2 < 32; ++index2)
        {
          if(((int) a.data[index1] & (int) num2) != 0)
          {
            index1 = a.dataLength;
            break;
          }

          num2 <<= 1;
          ++num1;
        }
      }

      BigInteger b1 = a >> num1;
      int num3 = 1;
      if((num1 & 1) != 0 && (((int) b.data[0] & 7) == 3 || ((int) b.data[0] & 7) == 5))
        num3 = -1;
      if(((int) b.data[0] & 3) == 3 && ((int) b1.data[0] & 3) == 3)
        num3 = -num3;
      if(b1.dataLength == 1 && b1.data[0] == 1U)
        return num3;
      return num3 * Jacobi(b % b1, b1);
    }

    public BigInteger ModInverse(BigInteger modulus)
    {
      BigInteger[] bigIntegerArray1 = new BigInteger[2]
      {
        (BigInteger) 0,
        (BigInteger) 1
      };
      BigInteger[] bigIntegerArray2 = new BigInteger[2];
      BigInteger[] bigIntegerArray3 = new BigInteger[2]
      {
        (BigInteger) 0,
        (BigInteger) 0
      };
      int num = 0;
      BigInteger bi1 = modulus;
      BigInteger bi2 = this;
      while(bi2.dataLength > 1 || bi2.dataLength == 1 && bi2.data[0] != 0U)
      {
        BigInteger outQuotient = new BigInteger();
        BigInteger outRemainder = new BigInteger();
        if(num > 1)
        {
          BigInteger bigInteger = (bigIntegerArray1[0] - bigIntegerArray1[1] * bigIntegerArray2[0]) % modulus;
          bigIntegerArray1[0] = bigIntegerArray1[1];
          bigIntegerArray1[1] = bigInteger;
        }

        if(bi2.dataLength == 1)
          singleByteDivide(bi1, bi2, outQuotient, outRemainder);
        else
          multiByteDivide(bi1, bi2, outQuotient, outRemainder);
        bigIntegerArray2[0] = bigIntegerArray2[1];
        bigIntegerArray3[0] = bigIntegerArray3[1];
        bigIntegerArray2[1] = outQuotient;
        bigIntegerArray3[1] = outRemainder;
        bi1 = bi2;
        bi2 = outRemainder;
        ++num;
      }

      if(bigIntegerArray3[0].dataLength > 1 ||
         bigIntegerArray3[0].dataLength == 1 && bigIntegerArray3[0].data[0] != 1U)
        throw new ArithmeticException("No inverse!");
      BigInteger bigInteger1 = (bigIntegerArray1[0] - bigIntegerArray1[1] * bigIntegerArray2[0]) % modulus;
      if(((int) bigInteger1.data[69] & int.MinValue) != 0)
        bigInteger1 += modulus;
      return bigInteger1;
    }

    public byte[] GetBytes()
    {
      int num = BitCount();
      int numBytes = num >> 3;
      if((num & 7) != 0)
        ++numBytes;
      return GetBytes(numBytes);
    }

    public byte[] GetBytesBE()
    {
      int num = BitCount();
      int numBytes = num >> 3;
      if((num & 7) != 0)
        ++numBytes;
      return GetBytesBE(numBytes);
    }

    public byte[] GetBytes(int numBytes)
    {
      byte[] numArray = new byte[numBytes];
      int num1 = BitCount();
      int num2 = num1 >> 3;
      if((num1 & 7) != 0)
        ++num2;
      for(int index1 = 0; index1 < num2; ++index1)
      {
        for(int index2 = 0; index2 < 4; ++index2)
        {
          if(index1 * 4 + index2 >= num2)
            return numArray;
          numArray[index1 * 4 + index2] = (byte) (data[index1] >> index2 * 8 & byte.MaxValue);
        }
      }

      return numArray;
    }

    protected static void Reverse<T>(T[] buffer, int length)
    {
      for(int index = 0; index < length / 2; ++index)
      {
        T obj = buffer[index];
        buffer[index] = buffer[length - index - 1];
        buffer[length - index - 1] = obj;
      }
    }

    protected static void Reverse<T>(T[] buffer)
    {
      Reverse(buffer, buffer.Length);
    }

    public byte[] GetBytesBE(int numBytes)
    {
      byte[] bytes = GetBytes(numBytes);
      Reverse(bytes);
      return bytes;
    }

    public void SetBit(uint bitNum)
    {
      uint num1 = bitNum >> 5;
      uint num2 = 1U << (byte) (bitNum & 31U);
      data[num1] |= num2;
      if(num1 < dataLength)
        return;
      dataLength = (int) num1 + 1;
    }

    public void UnsetBit(uint bitNum)
    {
      uint num1 = bitNum >> 5;
      if(num1 >= dataLength)
        return;
      uint num2 = uint.MaxValue ^ 1U << (byte) (bitNum & 31U);
      data[num1] &= num2;
      if(dataLength > 1 && data[dataLength - 1] == 0U)
        --dataLength;
    }

    public BigInteger Sqrt()
    {
      uint num1 = (uint) BitCount();
      uint num2 = ((int) num1 & 1) == 0 ? num1 >> 1 : (num1 >> 1) + 1U;
      uint num3 = num2 >> 5;
      byte num4 = (byte) (num2 & 31U);
      BigInteger bigInteger = new BigInteger();
      uint num5;
      if(num4 == 0)
      {
        num5 = 2147483648U;
      }
      else
      {
        num5 = 1U << num4;
        ++num3;
      }

      bigInteger.dataLength = (int) num3;
      for(int index = (int) num3 - 1; index >= 0; --index)
      {
        while(num5 != 0U)
        {
          bigInteger.data[index] ^= num5;
          if(bigInteger * bigInteger > this)
            bigInteger.data[index] ^= num5;
          num5 >>= 1;
        }

        num5 = 2147483648U;
      }

      return bigInteger;
    }

    public static BigInteger[] LucasSequence(BigInteger P, BigInteger Q, BigInteger k, BigInteger n)
    {
      if(k.dataLength == 1 && k.data[0] == 0U)
        return new BigInteger[3]
        {
          (BigInteger) 0,
          2 % n,
          1 % n
        };
      BigInteger bigInteger = new BigInteger();
      int index1 = n.dataLength << 1;
      bigInteger.data[index1] = 1U;
      bigInteger.dataLength = index1 + 1;
      BigInteger constant = bigInteger / n;
      int s = 0;
      for(int index2 = 0; index2 < k.dataLength; ++index2)
      {
        uint num = 1;
        for(int index3 = 0; index3 < 32; ++index3)
        {
          if(((int) k.data[index2] & (int) num) != 0)
          {
            index2 = k.dataLength;
            break;
          }

          num <<= 1;
          ++s;
        }
      }

      BigInteger k1 = k >> s;
      return LucasSequenceHelper(P, Q, k1, n, constant, s);
    }

    private static BigInteger[] LucasSequenceHelper(BigInteger P, BigInteger Q, BigInteger k, BigInteger n,
      BigInteger constant, int s)
    {
      BigInteger[] bigIntegerArray = new BigInteger[3];
      if(((int) k.data[0] & 1) == 0)
        throw new ArgumentException("Argument k must be odd.");
      uint num = (uint) (1 << (k.BitCount() & 31) - 1);
      BigInteger bigInteger1 = 2 % n;
      BigInteger bigInteger2 = 1 % n;
      BigInteger bigInteger3 = P % n;
      BigInteger bigInteger4 = bigInteger2;
      bool flag = true;
      for(int index = k.dataLength - 1; index >= 0; --index)
      {
        while(num != 0U && (index != 0 || num != 1U))
        {
          if(((int) k.data[index] & (int) num) != 0)
          {
            bigInteger4 = bigInteger4 * bigInteger3 % n;
            bigInteger1 = (bigInteger1 * bigInteger3 - P * bigInteger2) % n;
            bigInteger3 = (BarrettReduction(bigInteger3 * bigInteger3, n, constant) -
                           (bigInteger2 * Q << 1)) % n;
            if(flag)
              flag = false;
            else
              bigInteger2 = BarrettReduction(bigInteger2 * bigInteger2, n, constant);
            bigInteger2 = bigInteger2 * Q % n;
          }
          else
          {
            bigInteger4 = (bigInteger4 * bigInteger1 - bigInteger2) % n;
            bigInteger3 = (bigInteger1 * bigInteger3 - P * bigInteger2) % n;
            bigInteger1 = (BarrettReduction(bigInteger1 * bigInteger1, n, constant) -
                           (bigInteger2 << 1)) % n;
            if(flag)
            {
              bigInteger2 = Q % n;
              flag = false;
            }
            else
              bigInteger2 = BarrettReduction(bigInteger2 * bigInteger2, n, constant);
          }

          num >>= 1;
        }

        num = 2147483648U;
      }

      BigInteger bigInteger5 = (bigInteger4 * bigInteger1 - bigInteger2) % n;
      BigInteger bigInteger6 = (bigInteger1 * bigInteger3 - P * bigInteger2) % n;
      if(flag)
        flag = false;
      else
        bigInteger2 = BarrettReduction(bigInteger2 * bigInteger2, n, constant);
      BigInteger bigInteger7 = bigInteger2 * Q % n;
      for(int index = 0; index < s; ++index)
      {
        bigInteger5 = bigInteger5 * bigInteger6 % n;
        bigInteger6 = (bigInteger6 * bigInteger6 - (bigInteger7 << 1)) % n;
        if(flag)
        {
          bigInteger7 = Q % n;
          flag = false;
        }
        else
          bigInteger7 = BarrettReduction(bigInteger7 * bigInteger7, n, constant);
      }

      bigIntegerArray[0] = bigInteger5;
      bigIntegerArray[1] = bigInteger6;
      bigIntegerArray[2] = bigInteger7;
      return bigIntegerArray;
    }

    public BigInteger(SerializationInfo info, StreamingContext context)
    {
      this.info = info;
    }

    public SerializationInfo SerializationInfo
    {
      get { return info; }
      set { info = value; }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if(SerializationInfo == null)
        return;
      foreach(SerializationEntry serializationEntry in SerializationInfo)
        info.AddValue(serializationEntry.Name, serializationEntry.Value);
    }
  }
}