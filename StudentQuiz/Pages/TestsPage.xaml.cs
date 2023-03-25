// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StudentQuiz.DataAccess;
using StudentQuiz.Entities;
using StudentQuiz.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace StudentQuiz.Pages
{
    public sealed partial class TestsPage : Page
    {
        List<TestAssignment> OriginalDueTestAssignments = new List<TestAssignment>();
        List<TestAssignment> OriginalCompletedTestAssignments = new List<TestAssignment>();
        List<Subject> Subjects = new List<Subject>();
        private int UserId = 1;

        public TestsPage()
        {
            this.InitializeComponent();
        }

        private void DueSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            FilterTestAssignments();
        }

        private async void TakeTest_Click(object sender, RoutedEventArgs e)
        {
            var test = ((Button)sender).DataContext as TestAssignment;

            ContentDialog confirmStartTestDialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                RequestedTheme = ThemeHelper.RootTheme,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Are you sure?",
                Content = "Make sure you are ready to take the test, you cannot take a test again and must complete it in one sitting",
                PrimaryButtonText = "Ready",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = await confirmStartTestDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Frame.Navigate(typeof(TakeTest), test);
            }
        }

        private async void TestPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (UserDataController.LoggedInUser is not null)
            {
                UserId = UserDataController.LoggedInUser.Id;
            }

            var testAssignmentData = new List<TestAssignment>();
            var completedTestAssignmentData = new List<TestAssignment>();
            var subjects = new List<Subject>();

            await Task.Run(async () =>
            {
                // User Id for test assignments is hardcoded for now
                testAssignmentData = await TestAssignmentDataController.GetTestAssignments(UserId, false);
                completedTestAssignmentData = await TestAssignmentDataController.GetTestAssignments(UserId, true);
                subjects = await SubjectDataController.GetSubjects();
            });

            OriginalDueTestAssignments = testAssignmentData;
            OriginalCompletedTestAssignments = completedTestAssignmentData.OrderByDescending(x => x.CompletedDate).ToList();
            Subjects = subjects;

            DueTestsRepeater.ItemsSource = new ObservableCollection<TestAssignment>(testAssignmentData);
            CompletedTestsRepeater.ItemsSource = new ObservableCollection<TestAssignment>(completedTestAssignmentData);
        }

        private void FilterTestAssignments()
        {
            var searchString = DueSearchBox.Text;
            var filterSubject = SubjectFilterAutoSuggest.Text;
            var orderBy = DueOrderByBox.SelectedItem as string;

            // Reset to default list if all filter options are blank
            if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(filterSubject))
            {
                SetRepeaterItems(OriginalDueTestAssignments, orderBy);
                ResetDueFilters.IsEnabled = false;
                return;
            }

            var foundTests = OriginalDueTestAssignments
                .Where(x => x.Test.Name.ToLower().Contains(searchString.ToLower()) && (string.IsNullOrEmpty(filterSubject) || x.Test.Subject.Name == filterSubject))
                .ToList();

            SetRepeaterItems(foundTests, orderBy);
            
            ResetDueFilters.IsEnabled = true;
        }

        private void ResetDueFilters_Click(object sender, RoutedEventArgs e)
        {
            DueSearchBox.Text = string.Empty;
            SubjectFilterAutoSuggest.Text = null;
            ResetDueFilters.IsEnabled = false;
        }

        private void SubjectFilterAutoSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (string.IsNullOrEmpty(sender.Text))
                {
                    FilterTestAssignments();
                    return;
                }

                var foundSubjects = Subjects.Where(x => x.Name.ToLower().Contains(sender.Text.ToLower())).ToList();
                SubjectFilterAutoSuggest.ItemsSource = foundSubjects;
            }
            
            if (args.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen || args.Reason == AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
            {
                FilterTestAssignments();
            }
        }

        private void DueOrderByBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterTestAssignments();
        }

        private void SetRepeaterItems(List<TestAssignment> testAssignments, string order)
        {
            switch (order)
            {
                case "Due Date Ascending":
                    DueTestsRepeater.ItemsSource = new ObservableCollection<TestAssignment>(testAssignments.OrderBy(x => x.DueDate).ToList());
                    break;
                case "Due Date Descending":
                    DueTestsRepeater.ItemsSource = new ObservableCollection<TestAssignment>(testAssignments.OrderByDescending(x => x.DueDate).ToList());
                    break;
                default:
                    DueTestsRepeater.ItemsSource = new ObservableCollection<TestAssignment>(testAssignments.ToList());
                    break;
            }
        }

        private void CompletedSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var searchString = CompletedSearchBox.Text;

            if (string.IsNullOrEmpty(searchString))
            {
                CompletedTestsRepeater.ItemsSource = new ObservableCollection<TestAssignment>(OriginalCompletedTestAssignments);
                return;
            }

            CompletedTestsRepeater.ItemsSource = new ObservableCollection<TestAssignment>(OriginalCompletedTestAssignments
                .Where(x => x.Test.Name.ToLower().Contains(searchString.ToLower())));
        }
    }
}
