using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudentQuiz.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string StudentNumber { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsTerminated { get; set; }
        public UserGroup UserGroup { get; set; }
        public DateTime? LastLogin { get; set; }
        public string CourseTitle { get; set; }
        public string FullName { get { return $"{FirstName} {LastName}"; } }
        public string Detailed { get { return $"{FirstName} {LastName}, {UserGroup.Group}"; } }
        public bool IsStudent
        {
            get
            {
                return UserGroup.Group == "Students";
            }
        }

        public (bool success, string message) Validate()
        {
            var regexEmailPattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
            var regexPhoneNumberPattern = @"^(\+44|0)7[0-9]{9}$";

            if (string.IsNullOrEmpty(FirstName?.Trim()))
            {
                return (false, "First Name cannot be empty");
            }

            if (string.IsNullOrEmpty(LastName?.Trim()))
            {
                return (false, "Last Name cannot be empty");
            }

            if (string.IsNullOrEmpty(Email?.Trim()))
            {
                return (false, "Email cannot be empty");
            }
            
            if (!Regex.IsMatch(Email, regexEmailPattern))
            {
                return (false, "Email is not valid");
            }

            if (string.IsNullOrEmpty(PhoneNumber?.Trim()))
            {
                return (false, "Phone Number cannot be empty");
            }

            if (!Regex.IsMatch(PhoneNumber, regexPhoneNumberPattern))
            {
                return (false, "Phone Number is not valid");
            }

            if (UserGroup == null || UserGroup.Id == 0)
            {
                return (false, "User must have a User Group");
            }

            return (true, null);
        }
    }
}
