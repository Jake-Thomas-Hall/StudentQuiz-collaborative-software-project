using System;

namespace StudentQuiz.Entities
{
    public class TestAssignment
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public Test Test { get; set; }
        public int UserId { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int? ScoreCount { get; set; }
        public int? IncorrectCount { get; set; }
        public DateTime DueDate { get; set; }
        public double Percentage { 
            get
            {
                if (Test is null || IncorrectCount is null)
                {
                    return 0;
                }

                return Math.Round((Test.QuestionCount - IncorrectCount.Value) / (double)Test.QuestionCount * 100, 2);
            }
        }
        public bool IsTimedTest
        {
            get
            {
                return Test?.TimeLimitSeconds is not null;
            }
        }
    }
}
