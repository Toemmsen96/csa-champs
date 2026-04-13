using Util;
using ZumoLib;

public class LidarTools
{
    public static int GetAlignmentCorrection()
    {
        int nearestWallAngle = GetNearestWallAngle();
        const int sampleOffset = 10;

        var (left, right) = GetDistanceSample(nearestWallAngle, sampleOffset, 10);
        if (left <= 0 || right <= 0)
        {
            Console.WriteLine("Skipping alignment correction due to invalid Lidar samples.");
            return 0;
        }

        int leftAngle = (nearestWallAngle - sampleOffset + 360) % 360;
        int rightAngle = (nearestWallAngle + sampleOffset) % 360;
        double alpha = sampleOffset * Math.PI / 180.0;
        double invLeft = 1.0 / left;
        double invRight = 1.0 / right;
        double denominator = invRight + invLeft;
        if (Math.Abs(denominator) < 1e-9)
        {
            return 0;
        }

        // For symmetric rays around wall normal, this estimates yaw error directly.
        double tanError = ((invRight - invLeft) / denominator) / Math.Tan(alpha);
        double correction = Math.Atan(tanError) * 180.0 / Math.PI;

        Console.WriteLine($"Wall@{nearestWallAngle}° | Left({leftAngle}°): {left:F1}mm, Right({rightAngle}°): {right:F1}mm -> Correction: {correction:F2}°");
        return (int)Math.Round(correction);
    }

    private static int GetNearestWallAngle()
    {
        int? nearestWallAngle = null;
        int nearestWallDistance = int.MaxValue;
        foreach (var direction in Enum.GetValues<Direction>())
        {
            double distance = Zumo.Instance.Lidar[(int)direction].Distance;
            if (distance != 0 && distance <= 200 && distance < nearestWallDistance)
            {
                nearestWallAngle = (int)direction;
                nearestWallDistance = (int)distance;
            }
        }

        if (!nearestWallAngle.HasValue)
        {
            throw new InvalidDataException("No valid Lidar readings for alignment correction.");
        }

        return nearestWallAngle.Value;
    }

    private static (double left, double right) GetDistanceSample(int nearestWallAngle, int sampleOffset = 10, int sampleCount = 5)
    {
        List<int> sampleSetLeft = [];
        List<int> sampleSetRight = [];

        for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
        {
            int leftAngle = (nearestWallAngle - sampleOffset + 360) % 360;
            int rightAngle = (nearestWallAngle + sampleOffset) % 360;

            int leftDistance = Zumo.Instance.Lidar[leftAngle].Distance;
            int rightDistance = Zumo.Instance.Lidar[rightAngle].Distance;

            if (leftDistance != 0)
            {
                sampleSetLeft.Add(leftDistance);
            }

            if (rightDistance != 0)
            {
                sampleSetRight.Add(rightDistance);
            }

            Thread.Sleep(100);
        }

        return (GetMedian(sampleSetLeft), GetMedian(sampleSetRight));
    }

    private static double GetMedian(List<int> samples)
    {
        if (samples.Count == 0)
        {
            return 0;
        }

        samples.Sort();
        int middle = samples.Count / 2;
        if (samples.Count % 2 == 0)
        {
            return (samples[middle - 1] + samples[middle]) / 2.0;
        }

        return samples[middle];
    }

}