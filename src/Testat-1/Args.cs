namespace Testat_1;
class Args
{
    public static bool Calibrate { get; private set; }
    public static bool NoAlign { get; private set; }

    public static void Parse(string[] args)
    {
        Calibrate = args.Contains("--calibrate");
        NoAlign = args.Contains("--no-align");
    }
}