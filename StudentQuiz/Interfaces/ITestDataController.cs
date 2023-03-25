using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentQuiz.Interfaces
{
    public interface ITestDataController
    {
        Task<string> GetTestById(int testId);
    }
}
