﻿using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public static class ObjectCopier
{
  /// <summary>Perform a deep Copy of the object.</summary>
  /// <typeparam name="T">The type of object being copied.</typeparam>
  /// <param name="source">The object instance to copy.</param>
  /// <returns>The copied object.</returns>
  public static T Clone<T>(this T source)
  {
    if(!typeof(T).IsSerializable)
      throw new ArgumentException("The type must be serializable.", nameof(source));
    if(ReferenceEquals(source, null))
      return default(T);
    IFormatter formatter = new BinaryFormatter();
    Stream serializationStream = new MemoryStream();
    using(serializationStream)
    {
      formatter.Serialize(serializationStream, source);
      serializationStream.Seek(0L, SeekOrigin.Begin);
      return (T) formatter.Deserialize(serializationStream);
    }
  }
}