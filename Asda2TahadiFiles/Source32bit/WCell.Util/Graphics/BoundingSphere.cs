﻿using System;
using System.Runtime.InteropServices;

namespace WCell.Util.Graphics
{
  /// <summary>Defines a sphere.</summary>
  [StructLayout(LayoutKind.Explicit, Size = 16)]
  public struct BoundingSphere : IEquatable<BoundingSphere>
  {
    /// <summary>The center point of the sphere.</summary>
    [FieldOffset(0)]public Vector3 Center;

    /// <summary>The radius of the sphere.</summary>
    [FieldOffset(12)]public float Radius;

    /// <summary>
    /// Creates a new <see cref="T:WCell.Util.Graphics.BoundingSphere" /> with the given center point and radius.
    /// </summary>
    /// <param name="point">the center point</param>
    /// <param name="radius">the radius of the sphere</param>
    public BoundingSphere(Vector3 point, float radius)
    {
      Center = point;
      Radius = radius;
    }

    /// <summary>
    /// Creates a new <see cref="T:WCell.Util.Graphics.BoundingSphere" /> with the given center point and radius.
    /// </summary>
    /// <param name="x">the X coordinate of the center point</param>
    /// <param name="y">the Y coordinate of the center point</param>
    /// <param name="z">the Z coordinate of the center point</param>
    /// <param name="radius">the radius of the sphere</param>
    public BoundingSphere(float x, float y, float z, float radius)
    {
      Center = new Vector3(x, y, z);
      Radius = radius;
    }

    /// <summary>
    /// Checks whether the <see cref="T:WCell.Util.Graphics.BoundingSphere" /> contains the given point.
    /// </summary>
    /// <param name="point">the point to check for containment.</param>
    /// <returns>true if the point is contained; false otherwise</returns>
    public bool Contains(ref Vector3 point)
    {
      return Center.DistanceSquared(ref point) <= Radius * (double) Radius;
    }

    public ContainmentType Contains(Vector3 point)
    {
      return (double) Vector3.DistanceSquared(point, Center) >= (double) Radius * (double) Radius
        ? ContainmentType.Disjoint
        : ContainmentType.Contains;
    }

    public bool Intersects(BoundingBox box)
    {
      Vector3 result1;
      Vector3.Clamp(ref Center, ref box.Min, ref box.Max, out result1);
      float result2;
      Vector3.DistanceSquared(ref Center, ref result1, out result2);
      return result2 <= Radius * (double) Radius;
    }

    /// <summary>Checks equality of two spheres.</summary>
    /// <param name="other">the other sphere to compare with</param>
    /// <returns>true if both spheres are equal; false otherwise</returns>
    public bool Equals(BoundingSphere other)
    {
      return Center == other.Center && Radius == (double) other.Radius;
    }

    /// <summary>Checks equality with another object.</summary>
    /// <param name="obj">the object to compare</param>
    /// <returns>true if the object is <see cref="T:WCell.Util.Graphics.BoundingSphere" /> and is equal; false otherwise</returns>
    public override bool Equals(object obj)
    {
      return obj is BoundingSphere && Equals((BoundingSphere) obj);
    }

    public override int GetHashCode()
    {
      return Center.GetHashCode() + Radius.GetHashCode();
    }

    public static bool operator ==(BoundingSphere a, BoundingSphere b)
    {
      return a.Equals(b);
    }

    public static bool operator !=(BoundingSphere a, BoundingSphere b)
    {
      return a.Center != b.Center || a.Radius != (double) b.Radius;
    }

    public override string ToString()
    {
      return string.Format("{Center: {0}, Radius: {1}}", Center.ToString(),
        Radius.ToString());
    }
  }
}