namespace Testat_1.Util;

public sealed class Point(double x, double y, double angle, double distance)
{
    public double X { get; } = x;
    public double Y { get; } = y;
    public double Angle { get; } = angle;
    public double Distance { get; } = distance;
}