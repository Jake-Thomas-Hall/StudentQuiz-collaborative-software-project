using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data;
using StudentQuiz.Entities;
using StudentQuiz.Entities.DataEntities;
using StudentQuiz.Helpers;

namespace StudentQuiz.DataAccess
{
    public static class TestDataController
    {
        public static async Task<string> GetTestById(int testId)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                await conn.OpenAsync();

                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Tests WHERE Id = @Id";
                    cmd.Parameters.AddWithValue("@Id", testId);
                    var result = await cmd.ExecuteReaderAsync();
                    await result.ReadAsync();
                    if (!result.HasRows)
                        return null;
                    return result.GetString(2);
                }
            }
        }
		
		public static async Task<int> CreateTest(Test test)
        {
            var testId = 0;
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "INSERT INTO Tests (SubjectId, Name, Description, TimeLimitSeconds, QuestionsCount) VALUES (@subject, @name, @description, @timeLimit, @questionsCount);";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@subject", test.Subject.Id);
                    cmd.Parameters.AddWithValue("@name", test.Name);
                    cmd.Parameters.AddWithValue("@description", test.Description);
                    cmd.Parameters.AddWithValue("@timeLimit", test.TimeLimitSeconds);
                    cmd.Parameters.AddWithValue("@questionsCount", test.QuestionCount);
                    await cmd.ExecuteScalarAsync();
                    testId = Convert.ToInt32(cmd.LastInsertedId);
                }

            }
            
            foreach(var question in test.Questions)
            {
                await CreateQuestion(testId, question);
            }

            return testId;
        }
		
		public static async Task<int> CreateQuestion(int testId, Question question)
        {

            var questionId = 0;
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "INSERT INTO Questions (TestId, Question) VALUES (@test, @description);";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@test", testId);
                    cmd.Parameters.AddWithValue("@description", question.QuestionText);
                    await cmd.ExecuteScalarAsync();
                    questionId = Convert.ToInt32(cmd.LastInsertedId);
                }
            }

            foreach(var answer in question.Answers)
            {
                await CreateAnswer(questionId, answer);
            }    

            return questionId;
        }
		
		public static async Task<int> CreateAnswer(int questionId, Answer answer)
        {
            var answerId = 0;
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "INSERT INTO Answers (QuestionId, Answer, Correct) VALUES (@question, @answer, @isCorrect);";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@question", questionId);
                    cmd.Parameters.AddWithValue("@answer", answer.AnswerText);
                    cmd.Parameters.AddWithValue("@isCorrect", answer.IsCorrect);
                    await cmd.ExecuteScalarAsync();
                    answerId = Convert.ToInt32(cmd.LastInsertedId);
                }
            }
            return answerId;
        }

        public static async Task<Test> GetTest(int testId)
        {
            var test = new Test();
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "SELECT * FROM Tests WHERE Id = @testId";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@testId", testId);
                    var result = await cmd.ExecuteReaderAsync();
                    await result.ReadAsync();
                    test.Id = testId;
                    test.Subject = await SubjectDataController.GetSubject(result.GetInt32("SubjectId"));
                    test.Name = result.GetString("Name");
                    test.Description = result.GetString("Description");
                    test.TimeLimitSeconds = result.GetInt32("TimeLimitSeconds");
                }
            }
            return test;
        }

        public static async Task<List<Test>> GetTests()
        {
            var tests = new List<Test>();
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "SELECT T.Id, T.SubjectId, T.Name, T.Description, T.TimeLimitSeconds, T.QuestionsCount, S.Name AS SubjectName, S.Status AS SubjectStatus FROM Tests T INNER JOIN Subjects S on T.SubjectId = S.Id;";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    var result = await cmd.ExecuteReaderAsync();
                    while (await result.ReadAsync())
                    {
                        tests.Add(new Test
                        {
                            Id = result.GetInt32("Id"),
                            Subject = new Subject
                            {
                                Id = result.GetInt32("SubjectId"),
                                Name = result.GetString("SubjectName"),
                                Status = result.GetString("SubjectStatus")
                            },
                            Name = result.GetString("Name"),
                            Description = result.GetString("Description"),
                            TimeLimitSeconds = result.GetNullableInt("TimeLimitSeconds"),
                            QuestionCount = result.GetInt32("QuestionsCount")
                        });
                    }
                }
            }
            return tests;
        }

        public static async Task<List<Question>> GetTestQuestions(int testId)
        {
            var questions = new List<Question>();
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "SELECT * FROM Questions WHERE TestId = @testId";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@testId", testId);
                    var result = await cmd.ExecuteReaderAsync();
                    while(await result.ReadAsync())
                    {
                        questions.Add(new Question
                        {
                            Id = result.GetInt32("Id"),
                            Answers = await GetQuestionAnswers(result.GetInt32("Id")),
                            QuestionText = result.GetString("Question")
                        });
                    }
                }
            }
            return questions;
        }

        public static async Task<List<Answer>> GetQuestionAnswers(int questionId)
        {
            var answers = new List<Answer>();
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "SELECT * FROM Answers WHERE QuestionId = @questionId";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@questionId", questionId);
                    var result = await cmd.ExecuteReaderAsync();
                    while(await result.ReadAsync())
                    {
                        answers.Add(new Answer
                        {
                            Id = result.GetInt32("Id"),
                            AnswerText = result.GetString("Answer"),
                            IsCorrect = result.GetBoolean("Correct")
                        });
                    }
                }
            }
            return answers;
        }

        public static async Task<Test> GetTakeTest(int testId)
        {
            var test = new Test();
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = @"SELECT T.Id, T.SubjectId, S.Name as SubjectName, T.Name, T.TimeLimitSeconds, T.QuestionsCount, Q.Question, A.QuestionId, A.Answer, A.Id as AnswerId, A.Correct 
                            FROM Tests T 
                            JOIN Questions Q ON T.Id = Q.TestId 
                            JOIN Answers A ON A.QuestionId = Q.Id 
                            JOIN Subjects S ON T.SubjectId = S.Id 
                            WHERE T.Id = @Id";

                await conn.OpenAsync();

                var takeTestResponse = new List<TakeTestResponse>();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Id", testId);
                    var result = await cmd.ExecuteReaderAsync();
                    while (await result.ReadAsync())
                    {
                        takeTestResponse.Add(new TakeTestResponse
                        {
                            TestId = result.GetInt32("Id"),
                            SubjectId = result.GetInt32("SubjectId"),
                            SubjectName = result.GetString("SubjectName"),
                            Name = result.GetString("Name"),
                            TimeLimitSeconds = result.GetNullableInt("TimeLimitSeconds"),
                            QuestionCount = result.GetInt32("QuestionsCount"),
                            Question = result.GetString("Question"),
                            QuestionId = result.GetInt32("QuestionId"),
                            AnswerId = result.GetInt32("AnswerId"),
                            Answer = result.GetString("Answer"),
                            Correct = result.GetBoolean("Correct")
                        });
                    }

                    // Unflatten the response into a test via LINQ
                    test = takeTestResponse.GroupBy(x => x.TestId).Select(x => x.First()).Select(test => new Test
                    {
                        Id = test.TestId,
                        Name = test.Name,
                        QuestionCount = test.QuestionCount,
                        TimeLimitSeconds = test.TimeLimitSeconds,
                        Subject = new()
                        {
                            Id = test.SubjectId,
                            Name = test.SubjectName
                        },
                        Questions = takeTestResponse.GroupBy(y => y.QuestionId).Select(y => y.First()).Select(q => new Question
                        {
                            Id = q.QuestionId,
                            QuestionText = q.Question,
                            Answers = takeTestResponse.Where(y => y.QuestionId == q.QuestionId).GroupBy(y => y.AnswerId).Select(x => x.First()).Select(a => new Answer
                            {
                                Id = a.AnswerId,
                                AnswerText = a.Answer,
                                IsCorrect = a.Correct
                            }).ToList(),
                        }).ToList()
                    }).First();
                }
            }

            return test;
        }

        public static async Task<bool> IsTestAttempted(int testId)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "SELECT CASE WHEN MIN(CompletedDate) IS NULL THEN false ELSE True END AS Taken FROM TestAssignments WHERE TestId = @testId GROUP BY TestId";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@testId", testId);
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToBoolean(result);
                }
            }
        }

        public static async Task UpdateTest(Test test)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "UPDATE Tests SET Name = @Name, Description = @Description, TimeLimitSeconds = @TimeLimitSeconds, QuestionsCount = @QuestionsCount, SubjectId = @SubjectId WHERE Id = @Id";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Name", test.Name);
                    cmd.Parameters.AddWithValue("@Description", test.Description);
                    cmd.Parameters.AddWithValue("@TimeLimitSeconds", test.TimeLimitSeconds);
                    cmd.Parameters.AddWithValue("@QuestionsCount", test.QuestionCount);
                    cmd.Parameters.AddWithValue("@SubjectId", test.Subject.Id);
                    cmd.Parameters.AddWithValue("@Id", test.Id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task UpdateQuestion(Question question)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "UPDATE Questions SET Question = @Question WHERE Id = @Id";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Question", question.QuestionText);
                    cmd.Parameters.AddWithValue("@Id", question.Id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task UpdateAnswer(Answer answer)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "UPDATE Answers SET Answer = @Answer, Correct = @Correct WHERE Id = @Id";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Answer", answer.AnswerText);
                    cmd.Parameters.AddWithValue("@Correct", answer.IsCorrect);
                    cmd.Parameters.AddWithValue("@Id", answer.Id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task DeleteTest(int testId)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "DELETE FROM Tests WHERE Id = @Id";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Id", testId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task DeleteQuestion(int questionId)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "DELETE FROM Questions WHERE Id = @Id";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Id", questionId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task DeleteAnswer(int answerId)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "DELETE FROM Answers WHERE Id = @Id";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Id", answerId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
