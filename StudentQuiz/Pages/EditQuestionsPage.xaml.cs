// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using StudentQuiz.DataAccess;
using StudentQuiz.Entities;
using StudentQuiz.Helpers;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace StudentQuiz.Pages
{
    public sealed partial class EditQuestionsPage : Page
    {
        Test Test = new();
        ObservableCollection<Question> DisplayedQuestions = new();

        public EditQuestionsPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderHelper.OverrideHeader("Edit Questions");
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Test = (Test)e.Parameter ?? Test;
            await CheckForNewQuestions();
            DisplayedQuestions = new ObservableCollection<Question>(Test.Questions);
            QuestionsRepeater.ItemsSource = DisplayedQuestions;
        }

        /// <summary>
        /// Check for new questions being added from the CreateQuestionPage
        /// </summary>
        /// <returns></returns>
        private async Task CheckForNewQuestions()
        {
            List<Question> newQuestions = Test.Questions.Where(q => q.Id == 0).ToList(); // New questions have no Id

            foreach(var question in newQuestions)
            {
                await Task.Run(async () =>
                {
                    question.Id = await TestDataController.CreateQuestion(Test.Id, question);
                });
            }
        }

        private void NewQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreateQuestionPage), Test);
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(EditTestPage), Test);
        }

        private void EditQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            var question = ((Button)sender).DataContext as Question;
            Frame.Navigate(typeof(EditQuestionPage), (Test, question)); // send Test context and selected question
        }

        private async void DeleteQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            var question = ((Button)sender).DataContext as Question;

            await Task.Run(async () =>
            {
                await TestDataController.DeleteQuestion(question.Id);
            });

            Test.Questions.Remove(question);
            DisplayedQuestions.Remove(question);
        }
    }
}
