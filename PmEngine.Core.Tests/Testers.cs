namespace PmEngine.Core.Tests
{
    public class Tester1 : ITester
    {
        public void Test()
        {
            Console.WriteLine("Im Tester 1!");
        }
    }

    public class Tester2 : ITester
    {
        public void Test()
        {
            Console.WriteLine("Im Tester 2!");
        }
    }
}