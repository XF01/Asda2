﻿using Cell.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using WCell.Core.Cryptography;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Core.Network
{
  /// <summary>
  /// Reads primitive data types from an array of binary data.
  /// </summary>
  public abstract class PacketIn : BinaryReader
  {
    private static Logger log = LogManager.GetCurrentClassLogger();
    public static Encoding DefaultEncoding = Encoding.UTF8;
    protected PacketId _packetID;
    protected BufferSegment _segment;
    protected int _offset;

    /// <summary>
    /// Constructs a PacketIn object given the buffer to read, the offset to read from, and the number of bytes to read.
    /// </summary>
    /// <param name="segment">The buffer container wrapping our data</param>
    /// <param name="offset">The offset to read from the data array</param>
    /// <param name="length">The number of bytes to read</param>
    protected PacketIn(BufferSegment segment, int offset, int length)
      : base(new MemoryStream(segment.Buffer.Array, segment.Offset + offset, length),
        DefaultEncoding)
    {
      _segment = segment;
      _offset = offset;
    }

    /// <summary>The packet header size.</summary>
    /// <returns>The header size in bytes.</returns>
    public abstract int HeaderSize { get; }

    /// <summary>The ID of this packet.</summary>
    /// <example>RealmServerOpCode.SMSG_QUESTGIVER_REQUEST_ITEMS</example>
    public PacketId PacketId
    {
      get { return _packetID; }
    }

    /// <summary>The position within the current packet.</summary>
    public int Position
    {
      get { return (int) BaseStream.Position; }
      set { BaseStream.Position = value; }
    }

    /// <summary>The length in bytes of the packet.</summary>
    public int Length
    {
      get
      {
        if(BaseStream == null)
          return 0;
        return (int) BaseStream.Length;
      }
    }

    /// <summary>The length in bytes of the packet.</summary>
    public int ContentLength
    {
      get { return (int) BaseStream.Length - HeaderSize; }
    }

    /// <summary>Number of bytes available in the packet data.</summary>
    public int RemainingLength
    {
      get { return (int) (BaseStream.Length - BaseStream.Position); }
    }

    /// <summary>Whether or not this packet has been disposed.</summary>
    public bool Disposed
    {
      get { return _segment.Uses == 0; }
    }

    private bool EnsureData(int length)
    {
      if(Length - Position >= length)
        return true;
      log.Error("Not enough data available - Available: {0}, Required: {1}",
        Length - Position, length);
      return false;
    }

    /// <summary>
    /// Reads a 2-byte big endian integer value from the current stream and advances the current position of the stream by two bytes.
    /// </summary>
    /// <returns>A 2-byte big endian integer value read from the current stream.</returns>
    public ushort ReadUInt16BE()
    {
      return (ushort) ((uint) ReadByte() << 8 | ReadByte());
    }

    /// <summary>
    /// Reads a 4-byte floating point value from the current stream and advances the current position of the stream by four bytes.
    /// </summary>
    /// <returns>A 4-byte floating point value read from the current stream.</returns>
    public float ReadFloat()
    {
      return ReadSingle();
    }

    /// <summary>Reads a null-terminated UTF-8 encoded string.</summary>
    /// <returns>the string that was read</returns>
    public string ReadCString()
    {
      List<byte> byteList = new List<byte>();
      byte num;
      while((num = ReadByte()) != 0)
        byteList.Add(num);
      return DefaultEncoding.GetString(byteList.ToArray());
    }

    /// <summary>
    /// Reads a string from the current stream, and reverses it. The string is ended with a NULL byte.
    /// </summary>
    /// <returns>The string being read.</returns>
    public string ReadReversedString()
    {
      List<byte> byteList = new List<byte>();
      byte num;
      while((num = ReadByte()) != 0)
        byteList.Add(num);
      char[] chars = DefaultEncoding.GetChars(byteList.ToArray());
      chars.Reverse();
      return new string(chars);
    }

    /// <summary>
    /// Reads a string from the current stream. The string is prefixed with the length, encoded as an integer seven bits at a time.
    /// </summary>
    /// <returns>The string being read.</returns>
    public string ReadPascalString()
    {
      int num = ReadByte();
      if(!EnsureData(num))
        return "";
      return new string(ReadChars(num));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>The string being read.</returns>
    public string ReadPascalStringUShort()
    {
      int num = ReadUInt16();
      if(!EnsureData(num))
        return "";
      return new string(ReadChars(num));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>The string being read.</returns>
    public string ReadPascalStringUInt()
    {
      int num = ReadInt32();
      if(!EnsureData(num))
        return "";
      return new string(ReadChars(num));
    }

    /// <summary>
    /// Reads a string from the current stream, and reverses it. The string read is of length bytes.
    /// </summary>
    /// <returns>The string being read.</returns>
    public string ReadReversedPascalString(int length)
    {
      if(length < 1)
        throw new ArgumentOutOfRangeException(nameof(length), "string length must be greater than zero!");
      if(!EnsureData(length))
        return "";
      char[] arr = ReadChars(length);
      arr.Reverse();
      return new string(arr);
    }

    public string ReadAsciiString(Locale locale)
    {
      List<byte> byteList = new List<byte>();
      try
      {
        byte num;
        while((num = ReadByte()) != 0)
          byteList.Add(num);
      }
      catch(EndOfStreamException ex)
      {
      }

      return Asda2EncodingHelper.Decode(byteList.ToArray(), locale);
    }

    public string ReadAsdaString(int len, Locale locale)
    {
      List<byte> byteList = new List<byte>();
      byte num;
      while((num = ReadByte()) != 0)
        byteList.Add(num);
      if(byteList.Count < len)
        Position += len - byteList.Count - 1;
      return Asda2EncodingHelper.Decode(byteList.ToArray(), locale);
    }

    public string ReadFourCC()
    {
      if(!EnsureData(4))
        return "";
      char[] arr = ReadChars(4);
      if(arr[3] == char.MinValue)
      {
        char ch = arr[2];
        arr[2] = arr[0];
        arr[0] = ch;
      }
      else
        arr.Reverse();

      return new string(arr);
    }

    /// <summary>
    /// Reads a <see cref="T:WCell.Core.Cryptography.BigInteger" /> from the current stream. The <see cref="T:WCell.Core.Cryptography.BigInteger" /> is of length bytes.
    /// </summary>
    /// <param name="length">The length in bytes of the <see cref="T:WCell.Core.Cryptography.BigInteger" />.</param>
    /// <returns>The <see cref="T:WCell.Core.Cryptography.BigInteger" /> representation of the bytes.</returns>
    public BigInteger ReadBigInteger(int length)
    {
      if(length < 1)
        throw new ArgumentOutOfRangeException(nameof(length), "BigInteger length must be greater than zero!");
      if(!EnsureData(length))
        return new BigInteger(0L);
      byte[] inData = ReadBytes(length);
      if(inData.Length < length)
        return new BigInteger(0L);
      return new BigInteger(inData);
    }

    /// <summary>
    /// Reads a <see cref="T:WCell.Core.Cryptography.BigInteger" /> from the current stream. The <see cref="T:WCell.Core.Cryptography.BigInteger" /> is prefixed by the length.
    /// </summary>
    /// <returns>The <see cref="T:WCell.Core.Cryptography.BigInteger" /> representation of the bytes.</returns>
    public BigInteger ReadBigIntegerLengthValue()
    {
      byte num = ReadByte();
      return num != (byte) 0 ? ReadBigInteger(num) : new BigInteger();
    }

    /// <summary>
    /// Reads an <see cref="T:Cell.Core.XmlIPAddress" /> from the current stream.
    /// </summary>
    /// <returns>The <see cref="T:Cell.Core.XmlIPAddress" /> representation of the bytes.</returns>
    public XmlIPAddress ReadIPAddress()
    {
      if(!EnsureData(4))
        return new XmlIPAddress();
      return new XmlIPAddress(ReadBytes(4));
    }

    /// <summary>
    /// Reads a <see cref="T:WCell.Util.Graphics.Vector3" /> from the current stream.
    /// </summary>
    /// <returns>The <see cref="T:WCell.Util.Graphics.Vector3" /> representation of the bytes.</returns>
    public Vector3 ReadVector3()
    {
      return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
    }

    /// <summary>
    /// Reads a <see cref="T:WCell.Util.Graphics.Vector4" /> from the current stream.
    /// </summary>
    /// <returns>The <see cref="T:WCell.Util.Graphics.Vector4" /> representation of the bytes.</returns>
    public Vector4 ReadVector4()
    {
      return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
    }

    /// <summary>
    /// Reads an EntityId from this stream and advances the current position of the stream by eight bytes.
    /// </summary>
    /// <returns>An EntityId read from the current stream.</returns>
    public EntityId ReadEntityId()
    {
      EnsureData(8);
      return new EntityId(ReadBytes(8));
    }

    /// <summary>Reads an EntitiyId packed</summary>
    /// <returns></returns>
    public EntityId ReadPackedEntityId()
    {
      byte num = ReadByte();
      byte[] fullRaw = new byte[8];
      for(int index = 0; index < 8; ++index)
      {
        if((num & 1 << index) != 0)
          fullRaw[index] = ReadByte();
      }

      return new EntityId(fullRaw);
    }

    /// <summary>
    /// Advances the position of the current stream by num bytes.
    /// </summary>
    /// <param name="num">The number of bytes to advance.</param>
    public void SkipBytes(int num)
    {
      BaseStream.Seek(num, SeekOrigin.Current);
    }

    /// <summary>Sets the position within the current stream.</summary>
    /// <param name="offset">A byte offset relative to the origin parameter.</param>
    /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position.</param>
    /// <returns>The new position within the current stream.</returns>
    public long Seek(int offset, SeekOrigin origin)
    {
      return BaseStream.Seek(offset, origin);
    }

    /// <summary>Gets the name of the packet ID. (ie. CMSG_PING)</summary>
    /// <returns>a string containing the packet's canonical name</returns>
    public override string ToString()
    {
      return _packetID.ToString();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public string ToHexDump()
    {
      return WCellUtil.ToHex(PacketId, _segment.Buffer.Array, _segment.Offset + _offset,
        Length);
    }

    protected override void Dispose(bool disposing)
    {
      if(Interlocked.Exchange(ref _offset, -1) == -1)
        throw new InvalidOperationException("BAD BUG Packet " + this + " was already disposed!");
      _segment.DecrementUsage();
      base.Dispose(disposing);
    }
  }
}