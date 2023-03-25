using MySql.Data.MySqlClient;
using StudentQuiz.Entities;
using StudentQuiz.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System;

namespace StudentQuiz.DataAccess
{
    public static class TestAssignmentDataController
    {
        /// <summary>
        /// Get a test assignment and associated data by test assignment Id, used to show test results
        /// </summary>
        /// <param name="testAssignmentId">Id of the assignment to return</param>
        /// <returns>Test Assignment with related Test data and subject</returns>
        public static async Task<TestAssignment> GetTestAssignment(int testAssignmentId)
        {
            var testAssignment = new TestAssignment();

            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                // Can return only completed or only incompleted tests based on isCompleted value
                var query = @$"SELECT TA.Id, TA.TestId, TA.AssignedDate, TA.DueDate, TA.CompletedDate, TA.ScoreCount, TA.IncorrectCount, T.QuestionsCount, T.Name, T.Description, T.SubjectId, T.TimeLimitSeconds, S.Name AS SubjectName 
                            FROM TestAssignments TA 
                            INNER JOIN Tests T ON TA.TestId = T.Id INNER JOIN Subjects S ON T.SubjectId = S.Id 
                            WHERE TA.Id = @Id";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Id", testAssignmentId);
                    var result = await cmd.ExecuteReaderAsync();
                    await result.ReadAsync();

                    testAssignment = new()
                    {
                        Id = result.GetInt32("Id"),
                        TestId = result.GetInt32("TestId"),
                        AssignedDate = result.GetDateTime("AssignedDate"),
                        DueDate = result.GetDateTime("DueDate"),
                        CompletedDate = result.GetNullableDateTime("CompletedDate"),
                        ScoreCount = result.GetNullableInt("ScoreCount"),
                        IncorrectCount = result.GetNullableInt("IncorrectCount"),
                        Test = new()
                        {
                            QuestionCount = result.GetInt32("QuestionsCount"),
                            Name = result.GetString("Name"),
                            Description = result.GetString("Description"),
                            TimeLimitSeconds = result.GetNullableInt("TimeLimitSeconds"),
                            Subject = new()
                            {
                                Id = result.GetInt32("SubjectId"),
                                Name = result.GetString("SubjectName")
                            }
                        }
                    };

                }
            }

            return testAssignment;
        }

        /// <summary>
        /// Gets a list of due or completed test assignments by <paramref name="userId"/>. Provide <paramref name="isCompleted"/> as true to return completed tests instead.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isCompleted"></param>
        /// <returns></returns>
        public static async Task<List<TestAssignment>> GetTestAssignments(int userId, bool isCompleted)
        {
            var testAssignments = new List<TestAssignment>();

            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                // Can return only completed or only incompleted tests based on isCompleted value
                var query = @$"SELECT TA.Id, TA.TestId, TA.AssignedDate, TA.DueDate, TA.CompletedDate, TA.ScoreCount, TA.IncorrectCount, T.QuestionsCount, T.Name, T.Description, T.SubjectId, T.TimeLimitSeconds, S.Name AS SubjectName 
                            FROM TestAssignments TA 
                            INNER JOIN Tests T ON TA.TestId = T.Id INNER JOIN Subjects S ON T.SubjectId = S.Id 
                            WHERE TA.UserId = @Id AND TA.CompletedDate {(isCompleted ? "IS NOT NULL" : "IS NULL")}";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Id", userId);
                    var result = await cmd.ExecuteReaderAsync();
                    while (await result.ReadAsync())
                    {
                        testAssignments.Add(new()
                        {
                            Id = result.GetInt32("Id"),
                            TestId = result.GetInt32("TestId"),
                            AssignedDate = result.GetDateTime("AssignedDate"),
                            DueDate = result.GetDateTime("DueDate"),
                            CompletedDate = result.GetNullableDateTime("CompletedDate"),
                            ScoreCount = result.GetNullableInt("ScoreCount"),
                            IncorrectCount = result.GetNullableInt("IncorrectCount"),
                            Test = new()
                            {
                                QuestionCount = result.GetInt32("QuestionsCount"),
                                Name = result.GetString("Name"),
                                Description = result.GetString("Description"),
                                TimeLimitSeconds = result.GetNullableInt("TimeLimitSeconds"),
                                Subject = new()
                                {
                                    Id = result.GetInt32("SubjectId"),
                                    Name = result.GetString("SubjectName")
                                }
                            }
                        });
                    }
                }
            }

            return testAssignments;
        }

        /// <summary>
        /// Updates a test assignment, intended to be used for updating a test assignment once a test has been completed
        /// </summary>
        /// <param name="testAssignment">TestAssignment with Id, CompletedDate, ScoreCount and IncorrectCount populated</param>
        /// <returns>Id of test assignment</returns>
        public static async Task<int> UpdateTestAssignment(TestAssignment testAssignment)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "UPDATE TestAssignments SET CompletedDate = @Completed, ScoreCount = @Score, IncorrectCount = @Incorrect WHERE Id = @Id";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Completed", testAssignment.CompletedDate);
                    cmd.Parameters.AddWithValue("@Score", testAssignment.ScoreCount);
                    cmd.Parameters.AddWithValue("@Incorrect", testAssignment.IncorrectCount);
                    cmd.Parameters.AddWithValue("@Id", testAssignment.Id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return testAssignment.Id;
        }

        /// <summary>
        /// Add test assignments by via <paramref name="users"/> list
        /// </summary>
        /// <param name="users">List of users to assign tests to</param>
        /// <param name="dueDate">Due date of all test assignments</param>
        /// <param name="testId">Id of test to assign to</param>
        /// <returns></returns>
        public static async Task CreateTestAssignments(List<User> users, DateTime dueDate, int testId)
        {
            foreach (var user in users)
            {
                await CreateTestAssignment(user, dueDate, testId);
            }
        }

        /// <summary>
        /// Assign a <paramref name="user"/> to the specified <paramref name="testId"/>
        /// </summary>
        /// <param name="user">User to assign test to</param>
        /// <param name="dueDate"></param>
        /// <param name="testId"></param>
        /// <returns></returns>
        public static async Task CreateTestAssignment(User user, DateTime dueDate, int testId)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "INSERT INTO TestAssignments (TestId, UserId, DueDate) VALUES (@TestId, @UserId, @DueDate)";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@TestId", testId);
                    cmd.Parameters.AddWithValue("@UserId", user.Id);
                    cmd.Parameters.AddWithValue("@DueDate", dueDate);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
