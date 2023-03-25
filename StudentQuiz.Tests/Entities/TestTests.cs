using StudentQuiz.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StudentQuiz.Tests.Entities
{
    public class TestTests
    {
        [Fact]
        public void Test_Score_ReturnsCorrectScore()
        {
            // Arrange
            var test = new Test
            {
                Questions = new List<Question>
                {
                    new Question
                    {
                        SelectedAnswer = new Answer
                        {
                            IsCorrect = true
                        }
                    },
                    new Question
                    {
                        SelectedAnswer= new Answer
                        {
                            IsCorrect = false
                        }
                    },
                    new Question
                    {
                        SelectedAnswer= new Answer
                        {
                            IsCorrect = true
                        }
                    },
                    new Question
                    {
                        SelectedAnswer = new Answer
                        {
                            IsCorrect = false
                        }
                    }
                }
            };

            // Act
            var result = test.Score;

            // Assert
            Assert.Equal(40, result);
        }

        [Fact]
        public void Test_Score_CorrectScoreWithNullAnswer()
        {
            // Arrange
            var test = new Test
            {
                Questions = new List<Question>
                {
                    new Question
                    {
                        SelectedAnswer = new Answer
                        {
                            IsCorrect = true
                        }
                    },
                    new Question
                    {

                    },
                    new Question
                    {
                        SelectedAnswer= new Answer
                        {
                            IsCorrect = true
                        }
                    },
                    new Question
                    {
                        SelectedAnswer = new Answer
                        {
                            IsCorrect = false
                        }
                    }
                }
            };

            // Act
            var result = test.Score;

            // Assert
            Assert.Equal(40, result);
        }

        [Fact]
        public void Test_TotalIncorrect_CorrectCount()
        {
            // Arrange
            var test = new Test
            {
                Questions = new List<Question>
                {
                    new Question
                    {
                        SelectedAnswer = new Answer
                        {
                            IsCorrect = true
                        }
                    },
                    new Question
                    {
                        SelectedAnswer = new Answer
                        {
                            IsCorrect = false
                        }
                    },
                    new Question
                    {
                        SelectedAnswer= new Answer
                        {
                            IsCorrect = true
                        }
                    },
                    new Question
                    {
                        SelectedAnswer = new Answer
                        {
                            IsCorrect = false
                        }
                    }
                }
            };

            // Act
            var incorrectCount = test.TotalIncorrect;

            // Assert
            Assert.Equal(2, incorrectCount);
        }

        [Fact]
        public void Test_TotalIncorrect_CorrectNullCount()
        {
            // Arrange
            var test = new Test
            {
                Questions = new List<Question>
                {
                    new Question
                    {
                        SelectedAnswer = new Answer
                        {
                            IsCorrect = true
                        }
                    },
                    new Question
                    {

                    },
                    new Question
                    {
                        SelectedAnswer= new Answer
                        {
                            IsCorrect = true
                        }
                    },
                    new Question
                    {
                        SelectedAnswer = new Answer
                        {
                            IsCorrect = false
                        }
                    }
                }
            };

            // Act
            var incorrectCount = test.TotalIncorrect;

            // Assert
            Assert.Equal(2, incorrectCount);
        }

        [Fact]
        public void Test_Percentage_CorrectPercentage()
        {
            // Arrange
            var test = new Test
            {
                QuestionCount = 4,
                Questions = new List<Question>
                {
                    new Question
                    {
                        SelectedAnswer = new Answer
                        {
                            IsCorrect = true
                        }
                    },
                    new Question
                    {
                        SelectedAnswer = new Answer
                        {
                            IsCorrect = true
                        }
                    },
                    new Question
                    {
                        SelectedAnswer = new Answer
                        {
                            IsCorrect = true
                        }
                    },
                    new Question
                    {
                        SelectedAnswer = new Answer
                        {
                            IsCorrect = false
                        }
                    }
                }
            };

            // Act
            var percentage = test.Percentage;

            // Assert
            Assert.Equal(75.0, percentage);
        }

        [Fact]
        public void Test_TimeLimitSecounds_SetTimeLimitSecoundsGuard()
        {
            // Arrange
            var test = new Test();
            test.TimeLimitSecondsGuard = 10;

            // Act, Assert
            Assert.Equal(10, test.TimeLimitSeconds);
        }

        [Fact]
        public void Test_TimeLimitSecounds_SetTimeLimitSecoundsGuardNull ()
        {
            // Arrange
            var test = new Test();
            test.TimeLimitSecondsGuard = 0;

            // Act, Assert
            Assert.Null(test.TimeLimitSeconds);
        }

        [Fact]
        public void Test_TimeLimitSecounds_GetTimeLimitSecoundsGuard()
        {
            // Arrange
            var test = new Test();
            test.TimeLimitSeconds = 10;

            // Act, Assert
            Assert.Equal(10, test.TimeLimitSecondsGuard);
        }

        [Fact]
        public void Test_TimeLimitSecounds_GetTimeLimitSecoundsGuardNull()
        {
            // Arrange
            var test = new Test();
            test.TimeLimitSeconds = null;

            // Act, Assert
            Assert.Equal(0, test.TimeLimitSecondsGuard);
        }

        [Fact]
        public void Test_ValidateTest_QuestionCount()
        {
            // Arrange
            var test = new Test();

            // Act
            var result = test.ValidateTest();

            // Assert
            Assert.Equal("Test must have at least one question", result.message);
        }

        [Fact]
        public void Test_ValidateTest_QuestionPoolCount()
        {

            // Arrange
            var test = new Test()
            {
                QuestionCount = 1,
                Questions = new List<Question>()
            };

            // Act
            var result = test.ValidateTest();

            // Assert
            Assert.Equal("Test must have at least one question in the question pool", result.message);
        }

        [Fact]
        public void Test_ValidateTest_QuestionPoolSmallerThanCount()
        {
            // Arrange
            var test = new Test()
            {
                QuestionCount = 2,
                Questions = new List<Question>()
                {
                    new Question()
                }
            };

            // Act
            var result = test.ValidateTest();

            // Assert
            Assert.Equal("The question pool must have at least as many questions as specified in the Question Count field.", result.message);
        }


        [Fact]
        public void Test_ValidateTest_HasTitle()
        {
            // Arrange
            var test = new Test()
            {
                QuestionCount = 1,
                Questions = new List<Question>()
                {
                    new Question()
                }
            };

            // Act
            var result = test.ValidateTest();

            // Assert
            Assert.Equal("Test must have a title", result.message);
        }

        [Fact]
        public void Test_ValidateTest_HasDescription()
        {
            // Arrange
            var test = new Test()
            {
                Name = "Test",
                QuestionCount = 1,
                Questions = new List<Question>()
                {
                    new Question()
                }
            };

            // Act
            var result = test.ValidateTest();

            // Assert
            Assert.Equal("Test must have a description", result.message);
        }

        [Fact]
        public void Test_ValidateTest_HasSubject()
        {
            // Arrange
            var test = new Test()
            {
                Name = "Test",
                Description = "Description",
                QuestionCount = 1,
                Questions = new List<Question>()
                {
                    new Question()
                }
            };

            // Act
            var result = test.ValidateTest();

            // Assert
            Assert.Equal("Test must have a subject", result.message);
        }

        [Fact]
        public void Test_ValidateTest_PassValidation()
        {
            // Arrange
            var test = new Test()
            {
                Name = "Test",
                Description = "Description",
                QuestionCount = 1,
                Subject = new Subject(),
                Questions = new List<Question>()
                {
                    new Question()
                }
            };

            // Act
            var result = test.ValidateTest();

            // Assert
            Assert.True(result.success);
        }

        [Fact]
        public void Test_TimeLimitMinutesSTring_ReturnsCorrectValue_WitNull()
        {
            // Arrange
            var test = new Test();
            test.TimeLimitSeconds = null;

            // Act, Assert
            Assert.Equal("0", test.TimeLimitMinutesString);
        }

        [Fact]
        public void Test_TimeLimitMinutesSTring_ReturnsCorrectValue()
        {
            // Arrange
            var test = new Test();
            test.TimeLimitSeconds = 120;

            // Act, Assert
            Assert.Equal("2", test.TimeLimitMinutesString);
        }

        [Fact]
        public void Test_Progress_ReturnsCorrectValue()
        {
            // Arrange
            var test = new Test();
            test.QuestionCount = 10;

            // Act, Assert
            Assert.Equal(50d, test.Progress(5));
        }
    }
}
