using StudentQuiz.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StudentQuiz.Tests.Entities
{
    public class UserTests
    {
        [Fact]
        public void User_Fullname_CorrectFullName()
        {
            // Arrange
            var user = new User
            {
                FirstName = "First",
                LastName = "Last"
            };

            // Act/Assert
            Assert.Equal("First Last", user.FullName);
        }

        [Fact]
        public void User_IsStudent_False()
        {
            // Arrange
            var user = new User
            {
                UserGroup = new()
                {
                    Group = "Admin"
                }
            };

            // Act/Assert
            Assert.False(user.IsStudent);
        }

        [Fact]
        public void User_IsStudent_True()
        {
            // Arrange
            var user = new User
            {
                UserGroup = new()
                {
                    Group = "Students"
                }
            };

            // Act/Assert
            Assert.True(user.IsStudent);
        }

        [Fact]
        public void User_Validate_FirstNameEmpty()
        {
            // Arrange
            var user = new User();

            // Act
            var result = user.Validate();

            // Assert
            Assert.Equal("First Name cannot be empty", result.message);
        }

        [Fact]
        public void User_Validate_LastNameEmpty()
        {
            // Arrange
            var user = new User
            {
                FirstName = "First"
            };

            // Act
            var result = user.Validate();

            // Assert
            Assert.Equal("Last Name cannot be empty", result.message);
        }

        [Fact]
        public void User_Validate_EmailEmpty()
        {
            // Arrange
            var user = new User
            {
                FirstName = "First",
                LastName = "Last"
            };

            // Act
            var result = user.Validate();

            // Assert
            Assert.Equal("Email cannot be empty", result.message);
        }

        [Fact]
        public void User_Validate_InvalidEmail()
        {
            // Arrange
            var user = new User
            {
                FirstName = "First",
                LastName = "Last",
                Email = "email.com"
            };

            // Act
            var result = user.Validate();

            // Assert
            Assert.Equal("Email is not valid", result.message);
        }

        [Fact]
        public void User_Validate_PhoneNumberEmpty()
        {
            // Arrange
            var user = new User
            {
                FirstName = "First",
                LastName = "Last",
                Email = "email@example.com"
            };

            // Act
            var result = user.Validate();

            // Assert
            Assert.Equal("Phone Number cannot be empty", result.message);
        }

        [Fact]
        public void User_Validate_InvalidPhoneNumber()
        {
            // Arrange
            var user = new User
            {
                FirstName = "First",
                LastName = "Last",
                Email = "email@example.com",
                PhoneNumber = "123456789"
            };

            // Act
            var result = user.Validate();

            // Assert
            Assert.Equal("Phone Number is not valid", result.message);
        }

        [Fact]
        public void User_Validate_UserGroupEmpty()
        {
            // Arrange
            var user = new User
            {
                FirstName = "First",
                LastName = "Last",
                Email = "email@example.com",
                PhoneNumber = "07801334567"
            };

            // Act
            var result = user.Validate();

            // Assert
            Assert.Equal("User must have a User Group", result.message);
        }
    }
}
