using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace StudentQuiz.Entities
{
    public class Test
    {
        public Test()
        {
            Questions = new List<Question>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
        public int? TimeLimitSeconds { get; set; }
        public int TimeLimitSecondsGuard 
        { 
            get { return TimeLimitSeconds ?? 0; }
            set { TimeLimitSeconds = value == 0 ? null : value; } 
        }

        public string TimeLimitMinutesString
        {
            get 
            { 
                return Math.Round((double)TimeLimitSecondsGuard / 60, 2).ToString();  
            }

        }
        // When calculating score this should only ever contain the questions randomly selected to go into the test, not all of them - should only
        // be populated with all possible questions during test edit/create
        public List<Question> Questions { get; set; }
        public int QuestionCount { get; set; }
        public int Score
        {
            get
            {
                int score = 0;
                foreach(var question in Questions)
                {
                    if (question.SelectedAnswer == null)
                    {
                        score += -10;
                        continue;
                    }
                    score += question.SelectedAnswer.IsCorrect ? 30 : -10;
                } 
                    
                return score;
            }
        }

        public int TotalCorrect
        {
            get
            {
                return QuestionCount - TotalIncorrect; // big brain
            }
        }

        public int TotalIncorrect
        {
            get
            {
                int count = 0;
                foreach(Question question in Questions)
                {
                    if (question.SelectedAnswer == null || !question.SelectedAnswer.IsCorrect)
                    {
                        count++;
                    }
                }
                return count;
            }
        }
        
        public double Percentage
        {
            get
            {
                return Math.Round((double)TotalCorrect / (double)Questions.Count * 100, 2);
            }
        }

        public bool IsTimedTest
        {
            get
            {
                return TimeLimitSeconds is not null || TimeLimitSecondsGuard != 0;
            }
        }

        public (bool success, string message) ValidateTest()
        {
            if (QuestionCount < 1)
                return (false, "Test must have at least one question");

            if (Questions.Count == 0)
                return (false, "Test must have at least one question in the question pool");

            if (Questions.Count < QuestionCount)
                return (false, "The question pool must have at least as many questions as specified in the Question Count field.");

            if (string.IsNullOrEmpty(Name?.Trim()))
                return (false, "Test must have a title");

            if (string.IsNullOrEmpty(Description?.Trim()))
                return (false, "Test must have a description");

            if (Subject == null)
                return (false, "Test must have a subject");

            return (true, null);
        }

        public double Progress(int currentQuestion)
        {
            return (((double)currentQuestion / (double)QuestionCount) * 100d);
        }
    }
}
