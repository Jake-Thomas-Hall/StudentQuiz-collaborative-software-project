using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentQuiz.Entities
{
    public class Answer
    {
        public int Id { get; set; }
        public string AnswerText { get; set;  }
        public bool IsCorrect { get; set; }

        public override string ToString()
        {
            return AnswerText;
        }
    }
}
