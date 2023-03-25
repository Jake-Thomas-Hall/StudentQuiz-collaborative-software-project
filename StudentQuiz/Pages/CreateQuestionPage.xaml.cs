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
using StudentQuiz.Helpers;
using StudentQuiz.Pages;
using StudentQuiz.Dialogs;
using Windows.Security.EnterpriseData;
using System.Collections.ObjectModel;


namespace StudentQuiz.Pages
{
    public sealed partial class CreateQuestionPage : Page
    {
        Test NewTest = new();
        Question NewQuestion = new();
        ObservableCollection<Answer> Answers = new();
        Type SenderPageType;

        public CreateQuestionPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderHelper.OverrideHeader("Create Question");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SenderPageType = Frame.BackStack.Last().SourcePageType;
            NewTest = (Test)e.Parameter ?? NewTest; // recieve Test data (passed by reference)
            Answers = new(NewQuestion.Answers);
        }

        private void CreateQuestionButton_Click(object sender, RoutedEventArgs e)
        {

            var validateResult = NewQuestion.Validate();
            if (!validateResult.success)
            {
                ErrorInfobar.Message = validateResult.message;
                ErrorInfobar.IsOpen = true;
                return;
            }

            NewTest.Questions.Add(NewQuestion);
            Frame.Navigate(SenderPageType, NewTest);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(SenderPageType, NewTest);
        }
        
        private void ValidateQuestion()
        {
            
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
                NewQuestion.Answers.Add(dialog.NewAnswer);
                Answers.Add(dialog.NewAnswer);
            }
        }

        private void DeleteAnswer_Click(object sender, RoutedEventArgs e)
        {
            var answer = ((Button)sender).DataContext as Answer;
            NewQuestion.Answers.Remove(answer);
            Answers.Remove(answer);
        }
    }
}
