using Xunit;
using DemoUnitTest_ConsoleApp;

namespace DemoUnitTest_ConsoleApp.Tests
{
    public class CalculatorTests
    {
        [Fact]
        public void Add_ShouldReturnSumOfTwoIntegers()
        {
            // Arrange
            var calculator = new Calculator();
            int a = 5;
            int b = 10;
            int expected = 15;

            // Act
            int result = calculator.Add(a, b);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(2, true)]   // Even number
        [InlineData(3, false)]  // Odd number
        [InlineData(0, true)]   // Zero is even
        [InlineData(-4, true)]  // Negative even
        [InlineData(-5, false)] // Negative odd
        public void IsEven_ShouldReturnCorrectBoolean(int number, bool expected)
        {
            // Arrange
            var calculator = new Calculator();

            // Act
            bool result = calculator.IsEven(number);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}