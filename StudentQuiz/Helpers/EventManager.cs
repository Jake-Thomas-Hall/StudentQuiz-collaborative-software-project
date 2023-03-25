using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentQuiz.Helpers
{
    public static class EventManager
    {
        public static event EventHandler StartTest;
        public static event EventHandler EndTest;
        public static event EventHandler Login;

        public static void InvokeStartTest(object sender, RoutedEventArgs e)
        {
            StartTest?.Invoke(sender, EventArgs.Empty);
        }

        public static void InvokeEndTest(object sender, RoutedEventArgs e)
        {
            EndTest?.Invoke(sender, EventArgs.Empty);
        }

        public static void InvokeLogin(object sender, RoutedEventArgs e)
        {
            Login?.Invoke(sender, EventArgs.Empty);
        }
    }
}
