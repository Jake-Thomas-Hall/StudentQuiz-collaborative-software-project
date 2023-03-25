using StudentQuiz.Entities;
using System;
using Xunit;

namespace StudentQuiz.Tests.Entities
{
    public class SubjectTests
    {
        [Fact]
        public void Subject_Historical_True_WhenDateSet()
        {
            // Arrange
            var subject = new Subject
            {
                MarkedHistoricalDateTime = DateTime.Now
            };

            // Assert/Act
            Assert.True(subject.Historical);
        }

        [Fact]
        public void Subject_Historical_False_WhenDateNotSet()
        {
            // Arrange
            var subject = new Subject
            {
                MarkedHistoricalDateTime = null
            };

            // Assert/Act
            Assert.False(subject.Historical);
        }

        [Fact]
        public void Subject_Validate_False_WhenNameNotSet()
        {
            // Arrange
            var subject = new Subject
            {
                Name = null
            };

            // Act
            var result = subject.Validate();

            // Assert
            Assert.False(result.success);
        }

        [Fact]
        public void Subject_Validate_True_WhenNameSet()
        {
            // Arrange
            var subject = new Subject
            {
                Name = "Test name"
            };

            // Act
            var result = subject.Validate();

            // Assert
            Assert.True(result.success);
        }
    }
}
