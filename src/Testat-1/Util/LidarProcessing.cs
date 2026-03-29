using ZumoLib;

namespace Testat_1.Util;

public class LidarProcessing
{
    const double DEG_TO_RAD = Math.PI / 180;
    const double RAD_TO_DEG = 180 / Math.PI;
    const double ANGLE_THRESHOLD = 10.0;
    const double DISTANCE_JUMP_THRESHOLD = 0.3;
    const int MAX_CONSECUTIVE_NULLS = 3;

    private static Point?[] GetLidarPointsAsCartesian()
    {
        Point?[] points = new Point?[360];
        for (int deg = 0; deg < 360; deg++)
        {
            double distance = Zumo.Instance.Lidar[deg].Distance;
            if (distance == 0)
            {
                continue;
            }

            points[deg] = new Point(distance * Math.Cos(deg * DEG_TO_RAD), distance * Math.Sin(deg * DEG_TO_RAD), deg, distance);
        }
        return points;
    }

    private static double NormalizeAngle(double angle)
    {
        return (angle + 360.0) % 360.0;
    }

    private static double GetAngularDifference(double angle1, double angle2)
    {
        double normalized1 = NormalizeAngle(angle1);
        double normalized2 = NormalizeAngle(angle2);
        double diff = Math.Abs(normalized1 - normalized2);

        return NormalizeAngle(diff);
    }

    private static bool IsDistanceChangeAcceptable(double prevDistance, double currentDistance)
    {
        double avgDistance = (prevDistance + currentDistance) / 2;
        double relativeChange = Math.Abs(currentDistance - prevDistance) / avgDistance;
        return relativeChange <= DISTANCE_JUMP_THRESHOLD;
    }

    public static List<Wall> GetWalls()
    {
        var points = GetLidarPointsAsCartesian();
        var walls = new List<Wall>();
        var processed = new bool[360];

        for (int i = 0; i < 360; i++)
        {
            if (processed[i])
            {
                continue;
            }

            var currentPoint = points[i];
            if (currentPoint == null)
            {
                continue;
            }

            Point? prevPoint = null;
            for (int j = 1; j < 360; j++)
            {
                prevPoint = points[(i - j + 360) % 360];
                if (prevPoint != null)
                {
                    break;
                }
            }

            if (prevPoint == null)
            {
                continue;
            }

            double currentWallAngle = Math.Atan2(currentPoint.Y - prevPoint.Y, currentPoint.X - prevPoint.X) * RAD_TO_DEG;
            Point wallStart = prevPoint;
            Point wallEnd = currentPoint;
            int wallEndIndex = i;
            int consecutiveNulls = 0;

            while (wallEndIndex < 359)
            {
                int nextIndex = wallEndIndex + 1;
                var nextPoint = points[nextIndex];

                if (nextPoint == null)
                {
                    consecutiveNulls++;
                    if (consecutiveNulls > MAX_CONSECUTIVE_NULLS)
                    {
                        break;
                    }
                    wallEndIndex++;
                    continue;
                }

                consecutiveNulls = 0;
                double nextWallAngle = Math.Atan2(nextPoint.Y - wallEnd.Y, nextPoint.X - wallEnd.X) * RAD_TO_DEG;
                double angleDiff = GetAngularDifference(currentWallAngle, nextWallAngle);
                bool distanceAcceptable = IsDistanceChangeAcceptable(wallEnd.Distance, nextPoint.Distance);

                if (angleDiff > ANGLE_THRESHOLD || !distanceAcceptable)
                {
                    break;
                }

                wallEnd = nextPoint;
                wallEndIndex = nextIndex;
                processed[nextIndex] = true;

            }

            walls.Add(new Wall(wallStart, wallEnd));
            processed[i] = true;
        }

        if (walls.Count > 1)
        {
            var firstWall = walls[0];
            var lastWall = walls[^1];
            double wrapAngle = Math.Atan2(firstWall.Start.Y - lastWall.End.Y, firstWall.Start.X - lastWall.End.X) * RAD_TO_DEG;

            if (GetAngularDifference(firstWall.Angle, wrapAngle) < ANGLE_THRESHOLD &&
                GetAngularDifference(lastWall.Angle, wrapAngle) < ANGLE_THRESHOLD)
            {
                walls[0] = new Wall(lastWall.Start, firstWall.End);
                walls.RemoveAt(walls.Count - 1);
            }
        }

        return walls;
    }
}