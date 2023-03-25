using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using StudentQuiz.DataAccess;
using StudentQuiz.Dialogs;
using StudentQuiz.Entities;
using StudentQuiz.Helpers;
using StudentQuiz.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;


namespace StudentQuiz.Pages
{
    public sealed partial class ManageTestsPage : Page
    {
        List<Test> Tests = new();
        ObservableCollection<Test> DisplayedTests = new();

        List<Subject> Subjects = new();

        List<object> OrderOptions = new()
        {
            new { Visible = "Name (A-Z)", Value = "Name" },
            new { Visible = "Name (Z-A)", Value = "Name DESC" },
            new { Visible = "Question Count", Value = "Question"},
            new { Visible = "Question Count Descending", Value = "Question DESC"},
            new { Visible = "Time Limit", Value = "TimeLimit"},
            new { Visible = "Time Limit Descending", Value = "TimeLimit DESC"}
        };
        
        public ManageTestsPage()
        {
            this.InitializeComponent();
        }
        
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                Subjects = await SubjectDataController.GetSubjects();
            });
            Subjects.Add(new Subject { Name = "All", Id = 0 });
            Subjects = Subjects.OrderBy(s => s.Name).ToList();
            SubjectFilterBox.ItemsSource = Subjects;

            await PopulateTestsList();
            RefreshTestsUI();
        }

        private async Task PopulateTestsList()
        {
            await Task.Run(async () =>
            {
                Tests = await TestDataController.GetTests();
            });
        }

        private void RefreshTestsUI()
        {
            DisplayedTests = new ObservableCollection<Test>(Tests);
            TestsRepeater.ItemsSource = DisplayedTests;
            ApplyFilters();
        }
        
        private void ApplyFilters()
        {
            var testSearch = SearchBox.Text;
            var testSubject = SubjectFilterBox.SelectedValue as int?;
            var testOrder = OrderByBox.SelectedValue as string;

            var filteredTests = Tests.Where(t => t.Subject.Id == testSubject || testSubject == null || testSubject == 0);
            var searchedTests = filteredTests.Where(t => t.Name.Contains(testSearch, StringComparison.OrdinalIgnoreCase) ||
                                                 t.Description.Contains(testSearch, StringComparison.OrdinalIgnoreCase));
            List<Test> orderTests = new();
            switch (testOrder)
            {
                case "Name":
                    orderTests = searchedTests.OrderBy(t => t.Name).ToList();
                    break;
                case "Name DESC":
                    orderTests = searchedTests.OrderByDescending(t => t.Name).ToList();
                    break;
                case "Question":
                    orderTests = searchedTests.OrderBy(t => t.QuestionCount).ToList();
                    break;
                case "Question DESC":
                    orderTests = searchedTests.OrderByDescending(t => t.QuestionCount).ToList();
                    break;
                case "TimeLimit":
                    orderTests = searchedTests.OrderBy(t => t.TimeLimitSecondsGuard).ToList();
                    break;
                case "TimeLimit DESC":
                    orderTests = searchedTests.OrderByDescending(t => t.TimeLimitSecondsGuard).ToList();
                    break;
                default:
                    orderTests = searchedTests.ToList();
                    break;
            }

            DisplayedTests = new ObservableCollection<Test>(orderTests);
            TestsRepeater.ItemsSource = DisplayedTests;
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ApplyFilters();
        }

        private void SubjectFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void OrderByBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void NewTestButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreateTestPage));
        }
        
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var test = (sender as Button).DataContext as Test;
            Frame.Navigate(typeof(EditTestPage), test);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var test = (sender as Button).DataContext as Test;

            // Check if test has already been attempted
            bool testAttempted = false;
            testAttempted = await Task.Run(async () =>
            {
                 return await TestDataController.IsTestAttempted(test.Id);
            });

            var normalWarning = "Deleting a test is permanent and cannot be undone.";
            var testAttemptedWarning = "Students have attempted this test. Deleting it will remove all attempts, results, associated points. This action is permanent and cannot be undone.";

            ContentDialog deleteUserDialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                RequestedTheme = ThemeHelper.RootTheme,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Are you sure?",
                Content = testAttempted ? testAttemptedWarning : normalWarning,
                PrimaryButtonText = "Delete Test",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = await deleteUserDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await Task.Run(async () =>
                {
                    await TestDataController.DeleteTest(test.Id);
                });

                DisplayedTests.Remove(test);
            }
        }

        private async void AssignButton_Click(object sender, RoutedEventArgs e)
        {
            var testId = Convert.ToInt32((sender as Button).DataContext);

            AssignTestDialog assignTestDialog = new AssignTestDialog(testId)
            {
                XamlRoot = Content.XamlRoot,
                RequestedTheme = ThemeHelper.RootTheme
            };

            var result = await assignTestDialog.ShowAsync();
        }
    }
}
