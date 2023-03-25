using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentQuiz.Entities.DataEntities
{
    public class TakeTestResponse
    {
        public int TestId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int QuestionId { get; set; }
        public string Name { get; set; }
        public int? TimeLimitSeconds { get; set; }
        public int QuestionCount { get; set; }
        public string Question { get; set; }
        public int AnswerId { get; set; }
        public string Answer { get; set; }
        public bool Correct { get; set; }
    }
}
