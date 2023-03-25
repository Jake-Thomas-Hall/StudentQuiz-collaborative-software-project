
using System;

namespace StudentQuiz.Entities
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? MarkedHistoricalDateTime { get; set; }
        public bool Historical {
            get 
            {
                return MarkedHistoricalDateTime is not null;
            } 
        }
        public (bool success, string message) Validate()
        {
            if (string.IsNullOrEmpty(Name?.Trim()))
            {
                return (false, "Name cannot be empty for a subject");
            }

            return (true, null);
        }
    }
}
