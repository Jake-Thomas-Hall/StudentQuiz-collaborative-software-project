using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using StudentQuiz.Entities;
using StudentQuiz.Helpers;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace StudentQuiz.Pages
{
    public sealed partial class CreateQuestionsPage : Page
    {
        Test NewTest = new();
        ObservableCollection<Question> Questions = new();
        
        public CreateQuestionsPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderHelper.OverrideHeader("Create Questions");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NewTest = (Test)e.Parameter ?? NewTest; // recieve Test data (passed by reference
            Questions = new(NewTest.Questions);
        }

        private void NewQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreateQuestionPage), NewTest);
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreateTestPage), NewTest);
        }

        private void DeleteQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            var question = ((Button)sender).DataContext as Question;
            NewTest.Questions.Remove(question);
            Questions.Remove(question);
        }
    }
}

