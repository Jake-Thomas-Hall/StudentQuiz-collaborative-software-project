// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.AllJoyn;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace StudentQuiz.Pages
{
    public sealed partial class TakeTest : Page
    {
        private Test Test = new();
        private TestAssignment TestAssignment;
        private int CurrentQuestion = 0;
        private DispatcherTimer TimedTestTimer;
        private int TickCount = 0;
        private Random RandomGenerator = new Random();

        public TakeTest()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            TestAssignment = e.Parameter as TestAssignment;
            
            base.OnNavigatedTo(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            EventManager.InvokeStartTest(sender, e);

            var test = new Test();
            // need to store and query on page load
            await Task.Run(async () =>
            {
                test = await TestDataController.GetTakeTest(TestAssignment.TestId);
            });

            Test = test;
            
            Test.Questions = Test.Questions.OrderBy(a => RandomGenerator.Next()).Take(Test.QuestionCount).ToList();

            UpdateAnswers();
            TestNameTextBlock.Text = Test.Name;
            TestSubjectTextBlock.Text = Test.Subject.Name;
            QuestionTextBlock.Text = Test.Questions[CurrentQuestion].QuestionText;
            QuestionCurrentText.Text = $"{CurrentQuestion + 1}";
            QuestionCountText.Text = $"{Test.QuestionCount}";
            QuestionProgressBar.Value = Test.Progress(CurrentQuestion + 1);

            if (Test.TimeLimitSeconds is not null)
            {
                TestTimer.Visibility = Visibility.Visible;
                TimedTestTimer = new DispatcherTimer()
                {
                    Interval = new TimeSpan(0, 0, 1)
                };
                TimedTestTimer.Tick += TimedTestTimer_Tick;
                TimedTestTimer.Start();
            }
        }

        private void TimedTestTimer_Tick(object sender, object e)
        {
            TickCount++;
            TimerRing.Value = (double)TickCount / Test.TimeLimitSeconds.Value * 100d;
            var time = TimeSpan.FromSeconds(Test.TimeLimitSeconds.Value - TickCount);

            TimerDisplay.Text = time.ToString(@"hh\:mm\:ss");

            if (TickCount >= Test.TimeLimitSeconds)
            {
                TimedTestTimer.Stop();
                FinishTest(sender, new RoutedEventArgs());
            }
        }

        private async void EndTestButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog confirmEndTestDialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                RequestedTheme = ThemeHelper.RootTheme,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Are you sure?",
                Content = "Make sure you have selected an answer for every question, questions without a selected answer will always count as incorrect.",
                PrimaryButtonText = "End Test",
                CloseButtonText = "Continue",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = await confirmEndTestDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                FinishTest(sender, e);
            }
        }

        private void AnswerRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] is not null)
            {
                Test.Questions[CurrentQuestion].SelectedAnswer = Test.Questions[CurrentQuestion].Answers[AnswerRadioButtons.SelectedIndex];
            }
        }

        private void TestPreviousButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentQuestion--;
            AnswerRadioButtons.SelectedIndex = -1;
            UpdateAnswers();

            if (Test.Questions[CurrentQuestion].SelectedAnswer is not null)
            {
                AnswerRadioButtons.SelectedIndex = Test.Questions[CurrentQuestion].Answers.IndexOf(Test.Questions[CurrentQuestion].SelectedAnswer);
            }

            UpdateQuestion();
        }

        private void TestNextButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentQuestion++;
            AnswerRadioButtons.SelectedIndex = -1;
            UpdateAnswers();

            if (Test.Questions[CurrentQuestion].SelectedAnswer is not null)
            {
                AnswerRadioButtons.SelectedIndex = Test.Questions[CurrentQuestion].Answers.IndexOf(Test.Questions[CurrentQuestion].SelectedAnswer);
            }

            UpdateQuestion();
        }

        private void UpdateAnswers()
        {
            AnswerItem1.Content = Test.Questions[CurrentQuestion].Answers[0].ToString();
            AnswerItem2.Content = Test.Questions[CurrentQuestion].Answers[1].ToString();
            AnswerItem3.Content = Test.Questions[CurrentQuestion].Answers[2].ToString();
            AnswerItem4.Content = Test.Questions[CurrentQuestion].Answers[3].ToString();
        }

        private void UpdateQuestion()
        {
            QuestionTextBlock.Text = Test.Questions[CurrentQuestion].QuestionText;
            QuestionCurrentText.Text = $"{CurrentQuestion + 1}";
            QuestionProgressBar.Value = Test.Progress(CurrentQuestion + 1);
            TestPreviousButton.IsEnabled = CurrentQuestion > 0;
            TestNextButton.IsEnabled = CurrentQuestion + 1 < Test.QuestionCount;
        }

        private async void FinishTest(object sender, RoutedEventArgs e)
        {
            TestAssignment.CompletedDate = DateTime.Now;
            TestAssignment.IncorrectCount = Test.TotalIncorrect;
            TestAssignment.ScoreCount = Test.Score;

            int testAssignmentId = 0;
            await Task.Run(async () =>
            {
                testAssignmentId = await TestAssignmentDataController.UpdateTestAssignment(TestAssignment);
            });

            EventManager.InvokeEndTest(sender, e);
            // Only pass through assignment Id, refetch data on test results page (so that the page can be reused if needed)
            Frame.Navigate(typeof(TestResultPage), testAssignmentId);

            // Remove the test page from navigation history so clicking back button does not go back to it.
            if (Frame.CanGoBack)
            {
                Frame.BackStack.Remove(Frame.BackStack.Last());
            }
        }
    }
}
