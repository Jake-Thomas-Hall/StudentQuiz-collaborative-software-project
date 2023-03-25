// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Google.Protobuf.WellKnownTypes;
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace StudentQuiz.Pages
{

    public sealed partial class EditTestPage : Page
    {
        Test Test = new();
        ObservableCollection<Subject> Subjects = new();
        
        public EditTestPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Test = (Test)e.Parameter ?? Test;
            await Task.Run(async () =>
            {
                Test.Questions = await TestDataController.GetTestQuestions(Test.Id);
            });
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderHelper.OverrideHeader("Edit Test");

            List<Subject> subjectList = new();
            await Task.Run(async () =>
            {
                subjectList = await SubjectDataController.GetSubjects();
            });
            Subjects = new(subjectList);
            SubjectComboBox.ItemsSource = Subjects;
            if (Test.Subject != null)
            {
                // Select correct subject for combobox using incoming test data
                var subjectsMatch = Subjects.Where(s => s.Id == Test.Subject.Id).ToList();
                if (subjectsMatch.Count > 0)
                {
                    SubjectComboBox.SelectedItem = subjectsMatch.First();
                }
            }
        }

        private void EditQuestionsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(EditQuestionsPage), Test);
        }

        private async void UpdateTestButton_Click(object sender, RoutedEventArgs e)
        {
            var validateResult = Test.ValidateTest();

            if (!validateResult.success)
            {
                ErrorInfobar.Message = validateResult.message;
                ErrorInfobar.IsOpen = true;
                return;
            }

            await Task.Run(async () =>
            {
                await TestDataController.UpdateTest(Test);
            });
            Frame.Navigate(typeof(ManageTestsPage));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ManageTestsPage));
        }
    }
}
