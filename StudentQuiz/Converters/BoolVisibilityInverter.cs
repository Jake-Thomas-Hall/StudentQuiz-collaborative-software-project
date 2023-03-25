using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace StudentQuiz.Converters
{
    public class BoolVisibilityInverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be a Visibility enum");

            var visibilityBool = (bool)value;

            return visibilityBool ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // Not implemented for one way conversion
            throw new NotImplementedException();
        }

        #endregion
    }
}
