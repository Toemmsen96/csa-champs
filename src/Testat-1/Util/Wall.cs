namespace Testat_1.Util;

public sealed class Wall(Point start, Point end)
{
    public Point Start { get; } = start;
    public Point End { get; } = end;
    public double Angle { get; } = Math.Atan2(end.Y - start.Y, end.X - start.X) * 180 / Math.PI;
}