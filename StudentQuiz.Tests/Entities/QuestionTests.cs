using StudentQuiz.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StudentQuiz.Tests.Entities
{
    public class QuestionTests
    {
        [Fact]
        public void Question_Validate_QuestionTextEmpty()
        {
            // Arrange
            var question = new Question()
            {
                Answers = new List<Answer>()
                {
                    new Answer() { AnswerText = "asd", IsCorrect = false },
                    new Answer() { AnswerText = "d", IsCorrect = true },
                    new Answer() { AnswerText = "ad", IsCorrect = false },
                    new Answer() { AnswerText = "as", IsCorrect = false }
                }
            };

            // Act
            var result = question.Validate();

            // Assert
            Assert.False(result.success);
        }

        [Fact]
        public void Question_Validate_QuestionFourAnswer()
        {
            // Arrange
            var question = new Question()
            {
                QuestionText = "Test",
                Answers = new List<Answer>()
                {
                    new Answer() { AnswerText = "asd", IsCorrect = false },
                    new Answer() { AnswerText = "d", IsCorrect = true },
                    new Answer() { AnswerText = "as", IsCorrect = false }
                }
            };

            // Act
            var result = question.Validate();

            // Assert
            Assert.False(result.success);
        }

        [Fact]
        public void Question_Validate_QuestionOneCorrectAnswer()
        {
            // Arrange
            var question = new Question()
            {
                QuestionText = "Test",
                Answers = new List<Answer>()
                {
                    new Answer() { AnswerText = "asd", IsCorrect = false },
                    new Answer() { AnswerText = "d", IsCorrect = true },
                    new Answer() { AnswerText = "as", IsCorrect = false },
                    new Answer() { AnswerText = "a", IsCorrect = true }
                }
            };

            // Act
            var result = question.Validate();

            // Assert
            Assert.False(result.success);
        }

        [Fact]
        public void Question_Validate_QuestionPassValidation()
        {
            // Arrange
            var question = new Question()
            {
                QuestionText = "Test",
                Answers = new List<Answer>()
                {
                    new Answer() { AnswerText = "asd", IsCorrect = false },
                    new Answer() { AnswerText = "d", IsCorrect = true },
                    new Answer() { AnswerText = "as", IsCorrect = false },
                    new Answer() { AnswerText = "a", IsCorrect = false }
                }
            };

            // Act
            var result = question.Validate();

            // Assert
            Assert.True(result.success);
        }

        [Fact]
        public void Question_ValidateNewAnswer_HasAnswerText()
        {
            // Arrange
            var question = new Question();
            var answer = new Answer();

            // Act
            var result = question.ValidateNewAnswer(answer);

            // Assert
            Assert.Equal("Answer text cannot be empty", result.message);
        }

        [Fact]
        public void Question_ValidateNewAnswer_AnswerUnique()
        {
            // Arrange
            var question = new Question()
            {
                Answers = new List<Answer>()
                {
                    new Answer() { AnswerText = "Answer" }
                }
            };

            var answer = new Answer()
            {
                AnswerText = "Answer"

            };

            // Act
            var result = question.ValidateNewAnswer(answer);

            // Assert
            Assert.Equal("Answer text must be unique", result.message);
        }

        [Fact]
        public void Question_ValidateNewAnswer_OneCorrectAnswer()
        {
            // Arrange
            var question = new Question()
            {
                Answers = new List<Answer>()
                {
                    new Answer() { AnswerText = "Answer1", IsCorrect = true }
                }
            };

            var answer = new Answer()
            {
                AnswerText = "Answer",
                IsCorrect = true
            };

            // Act
            var result = question.ValidateNewAnswer(answer);

            // Assert
            Assert.Equal("Only one answer can be correct", result.message);
        }

        [Fact]
        public void Question_ValidateNewAnswer_PassValidation()
        {
            // Arrange
            var question = new Question()
            {
                Answers = new List<Answer>()
                {
                    new Answer() { AnswerText = "Answer1", IsCorrect = false }
                }
            };

            var answer = new Answer()
            {
                AnswerText = "Answer",
                IsCorrect = true
            };

            // Act
            var result = question.ValidateNewAnswer(answer);

            // Assert
            Assert.True(result.success);
        }
    }
}
