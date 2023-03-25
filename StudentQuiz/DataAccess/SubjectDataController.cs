using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MySql.Data.MySqlClient;
using StudentQuiz.Entities;
using StudentQuiz.Helpers;

namespace StudentQuiz.DataAccess
{
    public static class SubjectDataController
    {
        /// <summary>
        /// Get a subject by it's <paramref name="subjectId"/>
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Subject> GetSubject(int subjectId)
        {
            Subject subject = null;
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "SELECT * FROM Subjects WHERE Id = @subjectId";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@subjectId", subjectId);
                    var result = await cmd.ExecuteReaderAsync();
                    await result.ReadAsync();
                    subject = new Subject
                    {
                        Id = subjectId,
                        Name = result.GetString("Name"),
                        Status = result.GetString("Status")
                    };
                }
            }
            return subject;
        }

        /// <summary>
        /// Get a list of subjects, only returns active by default, to return historical subjcts set <paramref name="historical"/> to true.
        /// Also supports pagination of queries when a <paramref name="pageSize"/> above 0 is set.
        /// </summary>
        /// <param name="historical">False by default, controls whether active or historical subjects are returned</param>
        /// <param name="pageNum">Controls which page of data is fetched</param>
        /// <param name="pageSize">
        /// Controls returned page sizes, set to one higher than page size so that one additional item is returned.
        /// This therefore makes it possible to tell if a next button should be active or not
        /// </param>
        /// <param name="orderBy">Order of returned data</param>
        /// <param name="search">Filter string for returned data</param>
        /// <returns>List of subjects</returns>
        public static async Task<List<Subject>> GetSubjects(bool historical = false, int pageNum = 0, int pageSize = 0, string orderBy = null, string search = null)
        {
            var subjects = new List<Subject>();
            var statusFilter = historical ? "Historical" : "Active";

            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                // Can return only completed or only incompleted tests based on isCompleted value
                var query = $"SELECT Id, Name, Status, CreatedDateTime, MarkedHistoricalDateTime FROM Subjects WHERE Status = @Status";

                // Add additional parameter fields based on which values are populated
                if (!string.IsNullOrEmpty(search))
                {
                    query += " AND Name LIKE @Search";
                }

                if (orderBy is not null)
                {
                    query += $" ORDER BY {orderBy}";
                }

                if (pageSize > 0)
                {
                    query += " LIMIT @Offset, @Amount";
                }

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Status", statusFilter);

                    if (!string.IsNullOrEmpty(search))
                    {
                        // Add with wildcards so any part of a string can be matched
                        cmd.Parameters.AddWithValue("@Search", $"%{search}%");
                    }

                    if (pageSize > 0)
                    {
                        // Adjust to query with one extra, but to set offset like real page size is one less, to avoid errors
                        var realSize = pageSize - 1;
                        var offset = (realSize * pageNum) - realSize;
                        
                        cmd.Parameters.AddWithValue("@Offset", offset);
                        cmd.Parameters.AddWithValue("@Amount", pageSize);
                    }
                    
                    var result = await cmd.ExecuteReaderAsync();
                    while (await result.ReadAsync())
                    {
                        subjects.Add(new()
                        {
                            Id = result.GetInt32("Id"),
                            Name = result.GetString("Name"),
                            Status = result.GetString("Status"),
                            CreatedDateTime = result.GetDateTime("CreatedDateTime"),
                            MarkedHistoricalDateTime = result.GetNullableDateTime("MarkedHistoricalDateTime")
                        });
                    }
                }
            }

            return subjects;
        }

        /// <summary>
        /// Adds a subject to the DB, throws exception if subject already exists due to DB constraint.
        /// </summary>
        /// <param name="subject">Subject object with at least Name populated</param>
        /// <returns>Id of inserted subject</returns>
        public static async Task<long> AddSubject(Subject subject)
        {
            var id = 0L;

            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "INSERT INTO Subjects (Name) VALUES (@Name);";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Name", subject.Name);
                    await cmd.ExecuteNonQueryAsync();
                    id = cmd.LastInsertedId;
                }
            }

            return id;
        }

        /// <summary>
        /// Updates a given subject. Throws exception if edited name conflicts with existing subject due to DB constraints
        /// </summary>
        /// <param name="subject">Subject passed in with Id and Name to work</param>
        /// <returns>Empty task</returns>
        public static async Task UpdateSubject(Subject subject)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "UPDATE Subjects SET Name = @Name WHERE Id = @Id";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Id", subject.Id);
                    cmd.Parameters.AddWithValue("@Name", subject.Name);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Sets a subject to historical and sets historical date
        /// </summary>
        /// <param name="id">Id of the subject to update</param>
        /// <returns>Empty Task</returns>
        public static async Task SubjectHistorical(int id)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "UPDATE Subjects SET Status = 'Historical', MarkedHistoricalDateTime = CURRENT_TIMESTAMP() WHERE Id = @Id";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
