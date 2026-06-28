namespace KeyEngine.Graphs;

/// <summary>
/// Represents a point on a graph.
/// </summary>
/// <typeparam name="TValue">
/// The value type.
/// </typeparam>
public sealed class GraphPoint
{
    /// <summary>
    /// Gets the X coordinate.
    /// </summary>
    public double X
    {
        get;
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    public double Y
    {
        get;
    }

    /// <summary>
    /// Initializes a new graph point.
    /// </summary>
    /// <param name="x">
    /// The X coordinate.
    /// </param>
    /// <param name="y">
    /// The Y coordinate.
    /// </param>
    public GraphPoint(
        double x,
        double y)
    {
        X = x;
        Y = y;
    }
}