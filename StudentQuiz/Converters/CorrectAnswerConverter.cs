using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentQuiz.Converters
{
    public class CorrectAnswerConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((bool)value) ? "Correct": "Incorrect";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ((string)value) == "Correct";
        }

        #endregion
    }
}
