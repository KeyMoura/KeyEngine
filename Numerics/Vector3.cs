namespace KeyEngine.Numerics;

/// <summary>
/// Represents a three-dimensional vector.
/// </summary>
public readonly partial struct Vector3
    : IEquatable<Vector3>
{
    /// <summary>
    /// Gets the X component.
    /// </summary>
    public float X { get; }

    /// <summary>
    /// Gets the Y component.
    /// </summary>
    public float Y { get; }

    /// <summary>
    /// Gets the Z component.
    /// </summary>
    public float Z { get; }

    /// <summary>
    /// Represents the zero vector.
    /// </summary>
    public static readonly Vector3 Zero = new(0f, 0f, 0f);

    /// <summary>
    /// Represents a vector whose components are all one.
    /// </summary>
    public static readonly Vector3 One = new(1f, 1f, 1f);

    /// <summary>
    /// Represents the unit X vector.
    /// </summary>
    public static readonly Vector3 UnitX = new(1f, 0f, 0f);

    /// <summary>
    /// Represents the unit Y vector.
    /// </summary>
    public static readonly Vector3 UnitY = new(0f, 1f, 0f);

    /// <summary>
    /// Represents the unit Z vector.
    /// </summary>
    public static readonly Vector3 UnitZ = new(0f, 0f, 1f);

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3"/> struct.
    /// </summary>
    public Vector3(
        float x,
        float y,
        float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Adds two vectors.
    /// </summary>
    public static Vector3 operator +(
        Vector3 left,
        Vector3 right)
    {
        return new(
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z);
    }

    /// <summary>
    /// Subtracts two vectors.
    /// </summary>
    public static Vector3 operator -(
        Vector3 left,
        Vector3 right)
    {
        return new(
            left.X - right.X,
            left.Y - right.Y,
            left.Z - right.Z);
    }

    /// <summary>
    /// Negates a vector.
    /// </summary>
    public static Vector3 operator -(
        Vector3 value)
    {
        return new(
            -value.X,
            -value.Y,
            -value.Z);
    }

    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    public static Vector3 operator *(
        Vector3 value,
        float scalar)
    {
        return new(
            value.X * scalar,
            value.Y * scalar,
            value.Z * scalar);
    }

    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    public static Vector3 operator *(
        float scalar,
        Vector3 value)
    {
        return value * scalar;
    }

    /// <summary>
    /// Divides a vector by a scalar.
    /// </summary>
    public static Vector3 operator /(
        Vector3 value,
        float scalar)
    {
        return new(
            value.X / scalar,
            value.Y / scalar,
            value.Z / scalar);
    }

    /// <summary>
    /// Gets the squared length of the vector.
    /// </summary>
    public float LengthSquared =>
        (X * X) + (Y * Y) + (Z * Z);

    /// <summary>
    /// Gets the length of the vector.
    /// </summary>
    public float Length =>
        float.Sqrt(LengthSquared);

    /// <summary>
    /// Gets the normalized vector.
    /// </summary>
    public Vector3 Normalized
    {
        get
        {
            float length = Length;

            if (length == 0f)
            {
                return Zero;
            }

            return this / length;
        }
    }

    /// <summary>
    /// Computes the dot product of two vectors.
    /// </summary>
    public static float Dot(
        Vector3 left,
        Vector3 right)
    {
        return (left.X * right.X) +
               (left.Y * right.Y) +
               (left.Z * right.Z);
    }

    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    public static Vector3 Cross(
        Vector3 left,
        Vector3 right)
    {
        return new(
            (left.Y * right.Z) - (left.Z * right.Y),
            (left.Z * right.X) - (left.X * right.Z),
            (left.X * right.Y) - (left.Y * right.X));
    }

    /// <summary>
    /// Computes the squared distance between two vectors.
    /// </summary>
    public static float DistanceSquared(
        Vector3 left,
        Vector3 right)
    {
        return (right - left).LengthSquared;
    }

    /// <summary>
    /// Computes the distance between two vectors.
    /// </summary>
    public static float Distance(
        Vector3 left,
        Vector3 right)
    {
        return (right - left).Length;
    }

    /// <summary>
    /// Returns the normalized vector.
    /// </summary>
    public static Vector3 Normalize(
        Vector3 value)
    {
        return value.Normalized;
    }

    /// <summary>
    /// Linearly interpolates between two vectors.
    /// </summary>
    public static Vector3 Lerp(
        Vector3 start,
        Vector3 end,
        float amount)
    {
        amount = float.Clamp(
            amount,
            0f,
            1f);

        return LerpUnclamped(
            start,
            end,
            amount);
    }

    /// <summary>
    /// Linearly interpolates between two vectors without clamping.
    /// </summary>
    public static Vector3 LerpUnclamped(
        Vector3 start,
        Vector3 end,
        float amount)
    {
        return start + ((end - start) * amount);
    }

    /// <summary>
    /// Returns the component-wise minimum of two vectors.
    /// </summary>
    public static Vector3 Min(
        Vector3 left,
        Vector3 right)
    {
        return new(
            float.Min(left.X, right.X),
            float.Min(left.Y, right.Y),
            float.Min(left.Z, right.Z));
    }

    /// <summary>
    /// Returns the component-wise maximum of two vectors.
    /// </summary>
    public static Vector3 Max(
        Vector3 left,
        Vector3 right)
    {
        return new(
            float.Max(left.X, right.X),
            float.Max(left.Y, right.Y),
            float.Max(left.Z, right.Z));
    }

    /// <summary>
    /// Deconstructs the vector.
    /// </summary>
    public void Deconstruct(
        out float x,
        out float y,
        out float z)
    {
        x = X;
        y = Y;
        z = Z;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

    /// <summary>
    /// Determines whether two vectors are equal.
    /// </summary>
    public static bool operator ==(
        Vector3 left,
        Vector3 right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two vectors are not equal.
    /// </summary>
    public static bool operator !=(
        Vector3 left,
        Vector3 right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc/>
    public bool Equals(
        Vector3 other)
    {
        return X == other.X &&
               Y == other.Y &&
               Z == other.Z;
    }

    /// <inheritdoc/>
    public override bool Equals(
        object? obj)
    {
        return obj is Vector3 other &&
               Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            X,
            Y,
            Z);
    }
}