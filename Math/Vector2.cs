namespace KeyEngine.Math;

/// <summary>
/// Represents a two-dimensional vector.
/// </summary>
public readonly partial struct Vector2
    : IEquatable<Vector2>
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
    /// Represents the zero vector.
    /// </summary>
    public static readonly Vector2 Zero = new(0f, 0f);

    /// <summary>
    /// Represents a vector whose components are all one.
    /// </summary>
    public static readonly Vector2 One = new(1f, 1f);

    /// <summary>
    /// Represents the unit X vector.
    /// </summary>
    public static readonly Vector2 UnitX = new(1f, 0f);

    /// <summary>
    /// Represents the unit Y vector.
    /// </summary>
    public static readonly Vector2 UnitY = new(0f, 1f);

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector2"/> struct.
    /// </summary>
    /// <param name="x">
    /// The X component.
    /// </param>
    /// <param name="y">
    /// The Y component.
    /// </param>
    public Vector2(
        float x,
        float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Adds two vectors.
    /// </summary>
    public static Vector2 operator +(
        Vector2 left,
        Vector2 right)
    {
        return new(
            left.X + right.X,
            left.Y + right.Y);
    }

    /// <summary>
    /// Subtracts two vectors.
    /// </summary>
    public static Vector2 operator -(
        Vector2 left,
        Vector2 right)
    {
        return new(
            left.X - right.X,
            left.Y - right.Y);
    }

    /// <summary>
    /// Negates a vector.
    /// </summary>
    public static Vector2 operator -(
        Vector2 value)
    {
        return new(
            -value.X,
            -value.Y);
    }

    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    public static Vector2 operator *(
        Vector2 value,
        float scalar)
    {
        return new(
            value.X * scalar,
            value.Y * scalar);
    }

    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    public static Vector2 operator *(
        float scalar,
        Vector2 value)
    {
        return value * scalar;
    }

    /// <summary>
    /// Divides a vector by a scalar.
    /// </summary>
    public static Vector2 operator /(
        Vector2 value,
        float scalar)
    {
        return new(
            value.X / scalar,
            value.Y / scalar);
    }

    /// <summary>
    /// Gets the squared length of the vector.
    /// </summary>
    public float LengthSquared =>
        (X * X) + (Y * Y);

    /// <summary>
    /// Gets the length of the vector.
    /// </summary>
    public float Length =>
        float.Sqrt(LengthSquared);

    /// <summary>
    /// Gets the normalized vector.
    /// </summary>
    public Vector2 Normalized
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
    /// <param name="left">
    /// The first vector.
    /// </param>
    /// <param name="right">
    /// The second vector.
    /// </param>
    /// <returns>
    /// The dot product.
    /// </returns>
    public static float Dot(
        Vector2 left,
        Vector2 right)
    {
        return (left.X * right.X) +
               (left.Y * right.Y);
    }

    /// <summary>
    /// Computes the squared distance between two vectors.
    /// </summary>
    /// <param name="left">
    /// The first vector.
    /// </param>
    /// <param name="right">
    /// The second vector.
    /// </param>
    /// <returns>
    /// The squared distance.
    /// </returns>
    public static float DistanceSquared(
        Vector2 left,
        Vector2 right)
    {
        return (right - left).LengthSquared;
    }

    /// <summary>
    /// Computes the distance between two vectors.
    /// </summary>
    /// <param name="left">
    /// The first vector.
    /// </param>
    /// <param name="right">
    /// The second vector.
    /// </param>
    /// <returns>
    /// The distance.
    /// </returns>
    public static float Distance(
        Vector2 left,
        Vector2 right)
    {
        return (right - left).Length;
    }

    /// <summary>
    /// Linearly interpolates between two vectors.
    /// </summary>
    /// <param name="start">
    /// The starting vector.
    /// </param>
    /// <param name="end">
    /// The ending vector.
    /// </param>
    /// <param name="amount">
    /// The interpolation amount in the range [0, 1].
    /// </param>
    /// <returns>
    /// The interpolated vector.
    /// </returns>
    public static Vector2 Lerp(
        Vector2 start,
        Vector2 end,
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
    /// <param name="start">
    /// The starting vector.
    /// </param>
    /// <param name="end">
    /// The ending vector.
    /// </param>
    /// <param name="amount">
    /// The interpolation amount.
    /// </param>
    /// <returns>
    /// The interpolated vector.
    /// </returns>
    public static Vector2 LerpUnclamped(
        Vector2 start,
        Vector2 end,
        float amount)
    {
        return start + ((end - start) * amount);
    }

    /// <summary>
    /// Returns the normalized vector.
    /// </summary>
    /// <param name="value">
    /// The vector.
    /// </param>
    /// <returns>
    /// The normalized vector.
    /// </returns>
    public static Vector2 Normalize(
        Vector2 value)
    {
        return value.Normalized;
    }

    /// <summary>
    /// Returns the component-wise minimum of two vectors.
    /// </summary>
    public static Vector2 Min(
        Vector2 left,
        Vector2 right)
    {
        return new(
            float.Min(left.X, right.X),
            float.Min(left.Y, right.Y));
    }

    /// <summary>
    /// Returns the component-wise maximum of two vectors.
    /// </summary>
    public static Vector2 Max(
        Vector2 left,
        Vector2 right)
    {
        return new(
            float.Max(left.X, right.X),
            float.Max(left.Y, right.Y));
    }

    /// <summary>
    /// Deconstructs the vector.
    /// </summary>
    /// <param name="x">
    /// The X component.
    /// </param>
    /// <param name="y">
    /// The Y component.
    /// </param>
    public void Deconstruct(
        out float x,
        out float y)
    {
        x = X;
        y = Y;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    /// <summary>
    /// Determines whether two vectors are equal.
    /// </summary>
    public static bool operator ==(
        Vector2 left,
        Vector2 right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two vectors are not equal.
    /// </summary>
    public static bool operator !=(
        Vector2 left,
        Vector2 right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc/>
    public bool Equals(
        Vector2 other)
    {
        return X == other.X &&
               Y == other.Y;
    }

    /// <inheritdoc/>
    public override bool Equals(
        object? obj)
    {
        return obj is Vector2 other &&
               Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            X,
            Y);
    }
}