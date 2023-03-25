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
using StudentQuiz.Entities;
using StudentQuiz.Dialogs;
using StudentQuiz.DataAccess;
using StudentQuiz.Helpers;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace StudentQuiz.Pages
{

    public sealed partial class EditQuestionPage : Page
    {
        Test Test = new();
        Question Question = new();
        ObservableCollection<Answer> DisplayedAnswers = new();

        public EditQuestionPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderHelper.OverrideHeader("Edit Question");
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            (Test, Question) = ((Test, Question))e.Parameter; // it's tuple time
            DisplayedAnswers = new ObservableCollection<Answer>(Question.Answers);
        }

        private async void UpdateQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            var validateResult = Question.Validate();
            if (!validateResult.success)
            {
                ErrorInfobar.Message = validateResult.message;
                ErrorInfobar.IsOpen = true;
                return;
            }
            
            await Task.Run(async () =>
            {
                await TestDataController.UpdateQuestion(Question);
            });

            Frame.Navigate(typeof(EditQuestionsPage), Test);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var question = ((Button)sender).DataContext as Question;

            var validateResult = Question.Validate();
            if (!validateResult.success 
                && validateResult.message.Contains("Question", StringComparison.OrdinalIgnoreCase)) // Only concerned with answers as their edits are permanent from this page
            {
                ErrorInfobar.Message = validateResult.message;
                ErrorInfobar.IsOpen = true;
                return;
            }

            Frame.Navigate(typeof(EditQuestionsPage), Test);
        }

        private async void CreateAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            var question = ((Button)sender).DataContext as Question;

            CreateAnswerDialog dialog = new CreateAnswerDialog(question)
            {
                XamlRoot = Content.XamlRoot,
                RequestedTheme = ThemeHelper.RootTheme
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await Task.Run(async () =>
                {
                    dialog.NewAnswer.Id = await TestDataController.CreateAnswer(Question.Id, dialog.NewAnswer);
                });
                Question.Answers.Add(dialog.NewAnswer);
                DisplayedAnswers = new ObservableCollection<Answer>(Question.Answers);
                AnswersRepeater.ItemsSource = DisplayedAnswers;
            }
        }

        private async void DeleteAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            var answer = ((Button)sender).DataContext as Answer;
            await Task.Run(async () =>
            {
                await TestDataController.DeleteAnswer(answer.Id);
            });
            Question.Answers.Remove(answer);
            DisplayedAnswers = new ObservableCollection<Answer>(Question.Answers);
            AnswersRepeater.ItemsSource = DisplayedAnswers;
        }

    }
}
