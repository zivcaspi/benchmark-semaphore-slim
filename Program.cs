using System;

class Program
{
    static void Main(string[] args)
    {
        var testClass = new AsyncSemaphoreTests();

        testClass.TestAsyncSemaphoreStress();

        Console.WriteLine("Done. Hit ENTER to quit");
        Console.ReadLine();
    }
}
