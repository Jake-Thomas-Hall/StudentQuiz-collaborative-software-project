using System;

namespace StudentQuiz.Entities.DataEntities
{
    public class Leaderboard
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StudentNumber { get; set; }
        public int ScoreCount { get; set; }
        public int IncorrectCount { get; set; }
        public int QuestionCount { get; set; }
        public string FullName { 
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
        public int CorrectQuestionCount
        {
            get
            {
                return QuestionCount - IncorrectCount;
            }
        }
        public double PercentageCorrect { 
            get 
            { 
                return Math.Round((double)CorrectQuestionCount / QuestionCount * 100d, 2);
            } 
        }
        public string PercentageCorrectString
        {
            get
            {
                return $"{PercentageCorrect.ToString("0.00")}%";
            }
        }
    }
}
