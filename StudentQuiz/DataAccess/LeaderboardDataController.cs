using MySql.Data.MySqlClient;
using StudentQuiz.Entities;
using StudentQuiz.Entities.DataEntities;
using StudentQuiz.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace StudentQuiz.DataAccess
{
    public class LeaderboardDataController
    {
        /// <summary>
        /// Finds all students scores by all subjects or specified subject Id, summarises Score, incorrect and question counts.
        /// Returns ordered by highest scores, users without any achived scores (ScoreCount is null) are filtered out.
        /// </summary>
        /// <param name="subjectId">Optional, Id of subject to filter leaderboard by</param>
        /// <returns>List of leaderboard items</returns>
        public static async Task<List<Leaderboard>> GetLeaderboard(int subjectId = default)
        {
            var leaderboardItems = new List<Leaderboard>();

            // Only set subject filter string to be present if subject Id is not default value
            var subjectQueryString = string.Empty;
            if (subjectId > 0)
            {
                subjectQueryString = "S.Id = @SubjectId AND";
            }

            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                await conn.OpenAsync();

                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        @$"SELECT 
	                        TA.UserId, 
	                        U.FirstName, 
	                        U.LastName, 
	                        U.StudentNumber, 
	                        SUM(TA.ScoreCount) as ScoreCount, 
	                        SUM(TA.IncorrectCount) as IncorrectCount, 
	                        SUM(T.QuestionsCount) as QuestionCount
                        FROM TestAssignments TA 
                        INNER JOIN Tests T ON TA.TestId = T.Id 
                        INNER JOIN Subjects S ON T.SubjectId = S.Id 
                        INNER JOIN Users U ON TA.UserId = U.Id 
                        WHERE {subjectQueryString} U.UserGroupId = 1 AND ScoreCount IS NOT NULL
                        GROUP BY TA.UserId
                        ORDER BY ScoreCount DESC";
                    
                    // Only set subject parameter if this query will be filtered by subject
                    if (subjectId > 0)
                    {
                        cmd.Parameters.AddWithValue("@SubjectId", subjectId);
                    }
                    
                    var result = await cmd.ExecuteReaderAsync();
                    while (await result.ReadAsync())
                    {
                        leaderboardItems.Add(new Leaderboard
                        {
                            UserId = result.GetInt32("UserId"),
                            FirstName = result.GetString("FirstName"),
                            LastName = result.GetString("LastName"),
                            StudentNumber = result.GetNullableString("StudentNumber"),
                            ScoreCount = result.GetInt32("ScoreCount"),
                            IncorrectCount = result.GetInt32("IncorrectCount"),
                            QuestionCount = result.GetInt32("QuestionCount"),
                        });
                    }
                }
            }

            return leaderboardItems;
        }
    }
}
