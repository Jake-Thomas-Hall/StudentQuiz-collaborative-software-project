using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using StudentQuiz.Entities;
using StudentQuiz.DataAccess;
using StudentQuiz.Helpers;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace StudentQuiz.Pages
{
    public sealed partial class CreateTestPage : Page
    {
        Test NewTest = new();
        ObservableCollection<Subject> Subjects = new();

        public CreateTestPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NewTest = (Test)e.Parameter ?? NewTest;
        }
        
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderHelper.OverrideHeader("Create Test");
            List<Subject> subjectList = new();
            await Task.Run(async () =>
            {
                subjectList = await SubjectDataController.GetSubjects();
            });
            Subjects = new(subjectList);
            SubjectComboBox.ItemsSource = Subjects;
            if (NewTest.Subject != null)
            {
                // Select correct subject for combobox using incoming test data
                SubjectComboBox.SelectedItem = Subjects.Where(item => item.Name == NewTest.Subject.Name).ToList()[0];
            }
        }

        private async void CreateTestButton_Click(object sender, RoutedEventArgs e)
        {
            var validateResult = NewTest.ValidateTest();
            if (!validateResult.success)
            {
                ErrorInfobar.Message = validateResult.message;
                ErrorInfobar.IsOpen = true;
                return;
            }

            await Task.Run(async () =>
            {
                await TestDataController.CreateTest(NewTest);
            });
            Frame.Navigate(typeof(ManageTestsPage));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ManageTestsPage));
        }

        private void AddQuestionsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreateQuestionsPage), NewTest);
        }

    }
}
