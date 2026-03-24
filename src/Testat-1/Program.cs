using ZumoLib;

namespace ZumoApp;

class Program
{

    static void Main(string[] args)
    {
        for (int i = 0; i < 4; i++)
        {
            Zumo.Instance.Drive.Forward(250, 50, 100);
            Thread.Sleep(5000);
            Zumo.Instance.Drive.Rotate(-90, 50, 1000);
            Thread.Sleep(2000);
        }
    }
}
