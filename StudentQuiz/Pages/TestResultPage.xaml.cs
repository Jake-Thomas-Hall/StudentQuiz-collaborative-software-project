// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
using StudentQuiz.Helpers;
using System.Threading.Tasks;
using StudentQuiz.Entities;
using StudentQuiz.DataAccess;

namespace StudentQuiz.Pages
{
    public sealed partial class TestResultPage : Page
    {
        private TestAssignment TestAssignment = new();
        private int PassedTestAssignmentId = 0;

        public TestResultPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PassedTestAssignmentId = (int)e.Parameter;

            base.OnNavigatedTo(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderHelper.OverrideHeader("Test results");

            var testAssignment = new TestAssignment();
            await Task.Run(async () =>
            {
                testAssignment = await TestAssignmentDataController.GetTestAssignment(PassedTestAssignmentId);
            });

            TestAssignment = testAssignment;
            TestSubject.Text = TestAssignment.Test.Subject.Name;
            TestScore.Text = TestAssignment.ScoreCount.ToString();
            TestQuestionResult.Text = $"{TestAssignment.Test.QuestionCount - TestAssignment.IncorrectCount}/{TestAssignment.Test.QuestionCount}";
            TestPercentage.Text = $"{TestAssignment.Percentage}%";
            TestName.Text = TestAssignment.Test.Name;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(TestsPage));
        }
    }
}
