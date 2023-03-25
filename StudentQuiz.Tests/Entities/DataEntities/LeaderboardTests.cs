using StudentQuiz.Entities.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StudentQuiz.Tests.Entities.DataEntities
{
    public class LeaderboardTests
    {
        [Fact]
        private void Leaderboard_FullName_Accurate()
        {
            // Arrange
            var leaderboard = new Leaderboard
            {
                FirstName = "Test",
                LastName = "Test"
            };

            // Act/Assert
            Assert.Equal("Test Test", leaderboard.FullName);
        }

        [Fact]
        private void Leaderboard_CorrectQuestionCount_Accurate()
        {
            // Arrange
            var leaderboard = new Leaderboard
            {
                QuestionCount = 100,
                IncorrectCount = 25
            };

            // Act/Assert
            Assert.Equal(75, leaderboard.CorrectQuestionCount);
        }

        [Fact]
        private void Leaderboard_PercentageCorrect_Accurate()
        {
            // Arrange
            var leaderboard = new Leaderboard
            {
                QuestionCount = 100,
                IncorrectCount = 25
            };

            // Act/Assert
            Assert.Equal(75d, leaderboard.PercentageCorrect);
        }

        [Fact]
        private void Leaderboard_PercentageCorrectString_Accurate()
        {
            // Arrange
            var leaderboard = new Leaderboard
            {
                QuestionCount = 100,
                IncorrectCount = 25
            };

            // Act/Assert
            Assert.Equal("75.00%", leaderboard.PercentageCorrectString);
        }
    }
}
