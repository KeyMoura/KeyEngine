using System.Numerics;

namespace KeyEngine.Numerics;

/// <summary>
/// Provides common mathematical helper methods.
/// </summary>
public static class Maths
{
    /// <summary>
    /// Restricts a value to the specified range.
    /// </summary>
    /// <typeparam name="T">
    /// The numeric type.
    /// </typeparam>
    /// <param name="value">
    /// The value to clamp.
    /// </param>
    /// <param name="min">
    /// The minimum allowed value.
    /// </param>
    /// <param name="max">
    /// The maximum allowed value.
    /// </param>
    /// <returns>
    /// The clamped value.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="min"/> is greater than
    /// <paramref name="max"/>.
    /// </exception>
    public static T Clamp<T>(
        T value,
        T min,
        T max)
        where T : INumber<T>
    {
        if (min > max)
        {
            throw new ArgumentException(
                "The minimum value cannot be greater than the maximum value.");
        }

        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

    /// <summary>
    /// Linearly interpolates between two values.
    /// </summary>
    /// <typeparam name="T">
    /// The numeric type.
    /// </typeparam>
    /// <param name="start">
    /// The starting value.
    /// </param>
    /// <param name="end">
    /// The ending value.
    /// </param>
    /// <param name="amount">
    /// The interpolation amount in the range [0, 1].
    /// </param>
    /// <returns>
    /// The interpolated value.
    /// </returns>
    public static T Lerp<T>(
        T start,
        T end,
        T amount)
        where T : IFloatingPoint<T>
    {
        amount = Clamp(amount, T.Zero, T.One);

        return start + ((end - start) * amount);
    }

    /// <summary>
    /// Calculates the interpolation amount between two values.
    /// </summary>
    /// <typeparam name="T">
    /// The numeric type.
    /// </typeparam>
    /// <param name="start">
    /// The starting value.
    /// </param>
    /// <param name="end">
    /// The ending value.
    /// </param>
    /// <param name="value">
    /// The value to evaluate.
    /// </param>
    /// <returns>
    /// The interpolation amount in the range [0, 1].
    /// </returns>
    public static T InverseLerp<T>(
        T start,
        T end,
        T value)
        where T : IFloatingPoint<T>
    {
        if (start == end)
        {
            return T.Zero;
        }

        return Clamp(
            (value - start) / (end - start),
            T.Zero,
            T.One);
    }

    /// <summary>
    /// Maps a value from one range to another.
    /// </summary>
    /// <typeparam name="T">
    /// The numeric type.
    /// </typeparam>
    /// <param name="value">
    /// The value to map.
    /// </param>
    /// <param name="inputMin">
    /// The input range minimum.
    /// </param>
    /// <param name="inputMax">
    /// The input range maximum.
    /// </param>
    /// <param name="outputMin">
    /// The output range minimum.
    /// </param>
    /// <param name="outputMax">
    /// The output range maximum.
    /// </param>
    /// <returns>
    /// The mapped value.
    /// </returns>
    public static T Remap<T>(
        T value,
        T inputMin,
        T inputMax,
        T outputMin,
        T outputMax)
        where T : IFloatingPoint<T>
    {
        return Lerp(
            outputMin,
            outputMax,
            InverseLerp(
                inputMin,
                inputMax,
                value));
    }
}