using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data;
using StudentQuiz.Entities;
using StudentQuiz.Helpers;
using System.Data;
using System.Xml.Linq;

namespace StudentQuiz.DataAccess
{
    public static class UserDataController
    {
        public static User LoggedInUser { get; set; }

        /// <summary>
        /// Creates a new user in the database with the given user object
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<long> CreateUser(User user, string password="")
        {
            var userId = 0L;
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                await conn.OpenAsync();
                
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Users (FirstName, LastName, Email, Password, UserGroupId, PhoneNumber, IsConfirmed, CourseTitle, StudentNumber) VALUES (@FirstName, @LastName, @Email, @Password, @UserGroupId, @PhoneNumber, @IsConfirmed, @CourseTitle, @StudentNumber)";
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@UserGroupId", user.UserGroup.Id);
                    cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                    cmd.Parameters.AddWithValue("@IsConfirmed", user.IsConfirmed);
                    cmd.Parameters.AddWithValue("@CourseTitle", user.CourseTitle);
                    cmd.Parameters.AddWithValue("@StudentNumber", user.StudentNumber);
                    await cmd.ExecuteNonQueryAsync();
                    userId = cmd.LastInsertedId;
                }
            }
            return userId;
        }

        /// <summary>
        /// Get a user by its <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<User> GetUserById(int id)
        {
            var user = new User();
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT U.Id, U.UserGroupId, UG.`Group` AS UserGroup, U.Email, U.PhoneNumber, U.StudentNumber, U.FirstName, U.LastName, U.IsConfirmed, U.IsDisabled, U.IsTerminated, U.LastLoginDate, U.CourseTitle FROM Users U INNER JOIN UserGroups UG ON UG.Id = U.UserGroupId WHERE U.Id = @Id";
                    cmd.Parameters.AddWithValue("@Id", id);
                    var result = await cmd.ExecuteReaderAsync();
                    await result.ReadAsync();

                    user = new User
                    {
                        Id = id,
                        UserGroup = new UserGroup
                        {
                            Id = result.GetInt32("UserGroupId"),
                            Group = result.GetString("UserGroup")
                        },
                        Email = result.GetString("Email"),
                        FirstName = result.GetString("FirstName"),
                        LastName = result.GetString("LastName"),
                        StudentNumber = result.GetNullableString("StudentNumber"),
                        PhoneNumber = result.GetNullableString("PhoneNumber"),
                        IsConfirmed = result.GetBoolean("IsConfirmed"),
                        IsDisabled = result.GetBoolean("IsDisabled"),
                        IsTerminated = result.GetBoolean("IsTerminated"),
                        LastLogin = result.GetNullableDateTime("LastLoginDate"),
                        CourseTitle = result.GetNullableString("CourseTitle")
                    };
                }
            }
            return user;
        }

        /// <summary>
        /// Get a user by it <paramref name="email"/>
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static async Task<User> GetUserByEmail(string email)
        {
            var user = new User();
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                await conn.OpenAsync();

                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT U.Id, U.UserGroupId, UG.`Group` AS UserGroup, U.Email, U.FirstName, U.LastName, U.PhoneNumber, U.StudentNumber, U.IsConfirmed, U.IsDisabled, U.IsTerminated, U.LastLoginDate, U.CourseTitle FROM Users U INNER JOIN UserGroups UG ON UG.Id = U.UserGroupId WHERE U.Email = @Email";
                    cmd.Parameters.AddWithValue("@Email", email);
                    var result = await cmd.ExecuteReaderAsync();
                    await result.ReadAsync();

                    user = new User
                    {
                        Id = result.GetInt32("Id"),
                        UserGroup = new UserGroup
                        {
                            Id = result.GetInt32("UserGroupId"),
                            Group = result.GetString("UserGroup")
                        },
                        Email = email,
                        FirstName = result.GetString("FirstName"),
                        LastName = result.GetString("LastName"),
                        StudentNumber = result.GetNullableString("StudentNumber"),
                        PhoneNumber = result.GetNullableString("PhoneNumber"),
                        IsConfirmed = result.GetBoolean("IsConfirmed"),
                        IsDisabled = result.GetBoolean("IsDisabled"),
                        IsTerminated = result.GetBoolean("IsTerminated"),
                        LastLogin = result.GetNullableDateTime("LastLoginDate"),
                        CourseTitle = result.GetString("CourseTitle")
                    };
                }
            }
            return user;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        public static async Task<List<User>> GetUsers(string search = default)
        {
            var users = new List<User>();
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                await conn.OpenAsync();

                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT U.Id, U.UserGroupId, UG.`Group` AS UserGroup, U.Email, U.FirstName, U.LastName, U.PhoneNumber, U.StudentNumber, U.IsConfirmed, U.IsDisabled, U.IsTerminated, U.LastLoginDate, U.CourseTitle FROM Users U INNER JOIN UserGroups UG ON UG.Id = U.UserGroupId";

                    // If search value is provided, set search filter clause and only show student users
                    if (!string.IsNullOrEmpty(search?.Trim()))
                    {
                        cmd.CommandText += " WHERE CONCAT(U.FirstName, ' ' ,U.LastName) LIKE @Search AND U.UserGroupId = 1";
                        cmd.Parameters.AddWithValue("@Search", $"%{search}%");
                    }

                    var result = await cmd.ExecuteReaderAsync();
                    while (await result.ReadAsync())
                    {
                        users.Add(new User
                        {
                            Id = result.GetInt32("Id"),
                            UserGroup = new UserGroup
                            {
                                Id = result.GetInt32("UserGroupId"),
                                Group = result.GetString("UserGroup")
                            },
                            Email = result.GetString("Email"),
                            FirstName = result.GetString("FirstName"),
                            LastName = result.GetString("LastName"),
                            StudentNumber = result.GetNullableString("StudentNumber"),
                            PhoneNumber = result.GetNullableString("PhoneNumber"),
                            IsConfirmed = result.GetBoolean("IsConfirmed"),
                            IsDisabled = result.GetBoolean("IsDisabled"),
                            IsTerminated = result.GetBoolean("IsTerminated"),
                            LastLogin = result.GetNullableDateTime("LastLoginDate"),
                            CourseTitle = result.GetString("CourseTitle")
                        });
                    }
                }
            }
            return users;
        }

        /// <summary>
        /// Get the number of users assigned to a user group
        /// </summary>
        /// <param name="userGroupId"></param>
        /// <returns></returns>
        public static async Task<int> GetUserGroupCount(int userGroupId)
        {
            var count = 0;
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                await conn.OpenAsync();

                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE UserGroupId = @UserGroupId";
                    cmd.Parameters.AddWithValue("@UserGroupId", userGroupId);
                    count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
            return count;
        }

        /// <summary>
        /// Update a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task UpdateUser(User user)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                await conn.OpenAsync();

                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users SET Email = @Email, FirstName = @FirstName, LastName = @LastName, UserGroupId = @UserGroupId, CourseTitle = @CourseTitle, StudentNumber = @StudentNumber, PhoneNumber = @PhoneNumber WHERE Id = @Id";
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName);
                    cmd.Parameters.AddWithValue("@UserGroupId", user.UserGroup.Id);
                    cmd.Parameters.AddWithValue("@CourseTitle", user.CourseTitle);
                    cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                    cmd.Parameters.AddWithValue("@StudentNumber", user.StudentNumber);
                    cmd.Parameters.AddWithValue("@Id", user.Id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Approve user (<paramref name="userId"/>)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task ApproveUser(int userId)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                await conn.OpenAsync();

                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users SET IsConfirmed = 1 WHERE Id = @Id";
                    cmd.Parameters.AddWithValue("@Id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Set a user's account to be locked (disabled)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isDisabled"></param>
        /// <returns></returns>
        public static async Task AccountAccess(int userId, bool isDisabled)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                await conn.OpenAsync();

                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users SET IsDisabled = @IsDisabled WHERE Id = @Id";
                    cmd.Parameters.AddWithValue("@Id", userId);
                    cmd.Parameters.AddWithValue("@IsDisabled", isDisabled);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task DeleteUser(int userId)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                await conn.OpenAsync();

                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Users WHERE Id = @Id";
                    cmd.Parameters.AddWithValue("@Id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Used to update basic user details from the user account (user self management) page
        /// </summary>
        /// <param name="user">The user entity to update</param>
        /// <returns>Empty task</returns>
        public static async Task AccountUpdateDetails(User user)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = "UPDATE Users SET FirstName = @FirstName, LastName = @LastName, PhoneNumber = @PhoneNumber WHERE Id = @Id";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Id", user.Id);
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName);
                    cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // Sign up Page
        public static async Task<long> AddUser(User user)
        {
            var id = 0L;
            using (var conn = new MySqlConnection(DatabaseConnectionString.cnnString))
            {
                var query = @"INSERT INTO Users (UserGroupId, Password, Email, FirstName, LastName, PhoneNumber, CourseTitle, StudentNumber) 
                            VALUES (@UserGroupId, @Password, @Email, @FirstName, @LastName, @PhoneNumber, @CourseTitle, @StudentNumber )  ";

                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@UserGroupId", user.UserGroup.Id);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName);
                    cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                    cmd.Parameters.AddWithValue("@CourseTitle", user.CourseTitle);
                    cmd.Parameters.AddWithValue("@StudentNumber", user.StudentNumber);
                    await cmd.ExecuteNonQueryAsync();
                    id = cmd.LastInsertedId;
                }
            }
            return id;
        }
    }
}