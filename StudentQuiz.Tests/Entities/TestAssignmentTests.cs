using StudentQuiz.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StudentQuiz.Tests.Entities
{
    public class TestAssignmentTests
    {
        [Fact]
        public void TestAssignment_Percentage_ZeroOnNullTestOrCount()
        {
            // Arrange
            var testAssignment = new TestAssignment
            {
                IncorrectCount = null,
                Test = null
            };

            // Act
            var percentage = testAssignment.Percentage;

            // Assert
            Assert.Equal(0, percentage);
        }

        [Fact]
        public void TestAssignment_Percentage_CorrectWhenCalculated()
        {
            // Arrange
            var testAssignment = new TestAssignment
            {

                IncorrectCount = 5,
                Test = new()
                {
                    QuestionCount = 10
                }
            };

            // Act
            var percentage = testAssignment.Percentage;

            // Assert
            Assert.Equal(50, percentage);
        }

        [Fact]
        public void TestAssignment_IsTimedTest_True()
        {
            // Arrange
            var testAssignment = new TestAssignment
            {
                Test = new()
                {
                    TimeLimitSeconds = 10
                }
            };

            // Assert/Act
            Assert.True(testAssignment.IsTimedTest);
        }

        [Fact]
        public void TestAssignment_IsTimedTest_False()
        {
            // Arrange
            var testAssignment = new TestAssignment
            {
                Test = new()
                {
                    TimeLimitSeconds = null
                }
            };

            // Assert/Act
            Assert.False(testAssignment.IsTimedTest);
        }
    }
}
