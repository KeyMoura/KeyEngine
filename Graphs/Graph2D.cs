namespace KeyEngine.Graphs;

/// <summary>
/// Represents a two-dimensional graph.
/// </summary>
/// <typeparam name="TY">
/// The Y value type.
/// </typeparam>
public sealed class Graph2D
{
    private readonly List<GraphPoint> _points = [];

    /// <summary>
    /// Gets all graph points.
    /// </summary>
    public IReadOnlyList<GraphPoint> Points =>
        _points;

    /// <summary>
    /// Gets the number of points.
    /// </summary>
    public int Count =>
        _points.Count;

    /// <summary>
    /// Gets the interpolated value at the specified X coordinate.
    /// </summary>
    /// <param name="x">
    /// The X coordinate.
    /// </param>
    public double this[double x] =>
        Get(x);

    /// <summary>
    /// Adds a point to the graph.
    /// </summary>
    /// <param name="x">
    /// The X coordinate.
    /// </param>
    /// <param name="y">
    /// The Y value.
    /// </param>
    public void Add(
        double x,
        double y)
    {
        GraphPoint point = new(
            x,
            y);

        _points.Add(point);

        _points.Sort(
            static (left, right) =>
                left.X.CompareTo(right.X));
    }

    /// <summary>
    /// Removes the point at the specified X coordinate.
    /// </summary>
    /// <param name="x">
    /// The X coordinate.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the point was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Remove(
        double x)
    {
        GraphPoint? point =
            _points.FirstOrDefault(
                p => p.X == x);

        if (point is null)
        {
            return false;
        }

        return _points.Remove(point);
    }

    /// <summary>
    /// Gets the interpolated Y value at the specified X coordinate.
    /// </summary>
    /// <param name="x">
    /// The X coordinate.
    /// </param>
    /// <returns>
    /// The interpolated Y value.
    /// </returns>
    public double Get(
        double x)
    {
        if (_points.Count == 0)
        {
            throw new InvalidOperationException(
                "The graph contains no points.");
        }

        if (_points.Count == 1)
        {
            return _points[0].Y;
        }

        // Clamp to the first point.
        if (x <= _points[0].X)
        {
            return _points[0].Y;
        }

        // Clamp to the last point.
        if (x >= _points[^1].X)
        {
            return _points[^1].Y;
        }

        for (int i = 0; i < _points.Count - 1; i++)
        {
            GraphPoint left = _points[i];
            GraphPoint right = _points[i + 1];

            if (x >= left.X &&
                x <= right.X)
            {
                double amount =
                    (x - left.X) /
                    (right.X - left.X);

                return left.Y +
                       ((right.Y - left.Y) * amount);
            }
        }

        throw new InvalidOperationException(
            "Interpolation failed.");
    }

    /// <summary>
    /// Attempts to retrieve the point at the specified X coordinate.
    /// </summary>
    /// <param name="x">
    /// The X coordinate.
    /// </param>
    /// <param name="point">
    /// The retrieved point.
    /// <returns>
    /// <see langword="true"/> if the point exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool TryGetPoint(
        double x,
        out GraphPoint? point)
    {
        point = _points.FirstOrDefault(
            p => p.X == x);

        return point is not null;
    }

    /// <summary>
    /// Determines whether a point exists at the specified X coordinate.
    /// </summary>
    /// <param name="x">
    /// The X coordinate.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a point exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool Contains(
        double x)
    {
        return _points.Any(
            point => point.X == x);
    }

    /// <summary>
    /// Removes all points from the graph.
    /// </summary>
    public void Clear()
    {
        _points.Clear();
    }
}