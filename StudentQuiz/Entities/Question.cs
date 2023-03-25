using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentQuiz.Entities
{
    public class Question
    {
        public Question()
        {
            Answers = new List<Answer>();
        }

        public int Id { get; set; }
        public string QuestionText { get; set; }
        public List<Answer> Answers { get; set; }
        public Answer SelectedAnswer { get; set; }

        public (bool success, string message) ValidateNewAnswer(Answer newAnswer)
        {
            if (string.IsNullOrEmpty(newAnswer.AnswerText?.Trim()))
                return (false, "Answer text cannot be empty");

            if (Answers.Any(a => a.AnswerText == newAnswer.AnswerText))
                return (false, "Answer text must be unique");

            if (Answers.Where(item => item.IsCorrect).Count() >= 1 && newAnswer.IsCorrect)
                return (false, "Only one answer can be correct");

            return (true, null);
        }

        public (bool success, string message) Validate()
        {
            if (string.IsNullOrEmpty(QuestionText?.Trim()))
                return (false, "Question text cannot be empty");

            if (!(Answers.Count == 4))
                return (false, "Question must have 4 answers");

            if (Answers.Where(answer => answer.IsCorrect).Count() != 1)
                return (false, "There must be one correct answer");

            return (true, null);
        }
    }
}
