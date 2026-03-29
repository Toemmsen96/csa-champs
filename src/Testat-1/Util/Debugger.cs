public class Debugger
{
    public static void WaitForDebugger()
    {
        Console.WriteLine("Waiting for debugger to attach...");
        while (!System.Diagnostics.Debugger.IsAttached)
        {
            Thread.Sleep(100);
        }
        Console.WriteLine("Debugger attached.");
    }

    public static void Break()
    {
        if (System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Break();
        }
    }
}