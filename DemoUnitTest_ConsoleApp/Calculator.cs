namespace DemoUnitTest_ConsoleApp
{
    public class Calculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public bool IsEven(int number)
        {
            return number % 2 == 0;
        }
    }
}