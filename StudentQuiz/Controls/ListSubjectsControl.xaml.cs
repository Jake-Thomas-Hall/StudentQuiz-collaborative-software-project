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
using StudentQuiz.Dialogs;
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

namespace StudentQuiz.Controls
{
    public sealed partial class ListSubjectsControl : UserControl
    {
        private int PageNumber = 1;
        private List<object> OrderOptions = new()
        {
            new { Visible = "Created Ascending", Value = "CreatedDateTime" },
            new { Visible = "Created Descending", Value = "CreatedDateTime DESC" },
            new { Visible = "Name (A-Z)", Value = "Name" },
            new { Visible = "Name (Z-A)", Value = "Name DESC" }
        };

        public event RoutedEventHandler SubjectMarkedHistorical;

        public ListSubjectsControl()
        {
            this.InitializeComponent();
        }

        public bool IsHistorical { get; set; }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsHistorical)
            {
                OrderOptions.AddRange(new List<object>
                {
                    new { Visible = "Historical Ascending", Value = "MarkedHistoricalDateTime" },
                    new { Visible = "Historical Descending", Value = "MarkedHistoricalDateTime DESC" },
                });
            }
            QuerySubjects();
        }

        private void SubjectsGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var columns = Math.Ceiling(ActualWidth / 450);
            ((ItemsWrapGrid)SubjectsGridView.ItemsPanelRoot).ItemWidth = e.NewSize.Width / columns;
        }

        private void SubjectsPreviousButton_Click(object sender, RoutedEventArgs e)
        {
            PageNumber--;
            SubjectPageNumber.Text = PageNumber.ToString();
            QuerySubjects();
        }

        private void SubjectsNextButton_Click(object sender, RoutedEventArgs e)
        {
            PageNumber++;
            SubjectPageNumber.Text = PageNumber.ToString();

            QuerySubjects();
        }

        private void SubjectsOrderBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PageNumber = 1;
            SubjectPageNumber.Text = PageNumber.ToString();
            SubjectsPreviousButton.IsEnabled = PageNumber > 1;
            QuerySubjects();
        }

        private void SubjectSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            PageNumber = 1;
            QuerySubjects();
        }

        private async void QuerySubjects()
        {
            EnablePaginationControls(false);

            var SubjectsSearch = SubjectSearch.Text;
            var SubjectsOrder = SubjectsOrderBox.SelectedValue as string;

            var subjects = new List<Subject>();
            await Task.Run(async () =>
            {
                subjects = await SubjectDataController.GetSubjects(historical: IsHistorical, pageNum: PageNumber, pageSize: 13, orderBy: SubjectsOrder, search: SubjectsSearch);
            });
            EnablePaginationControls(true);
            SubjectsNextButton.IsEnabled = subjects.Count > 12;
            SubjectsPreviousButton.IsEnabled = PageNumber > 1;
            SubjectsGridView.ItemsSource = new ObservableCollection<Subject>(subjects.Take(12).ToList());
        }

        public void RefreshSubjectsList()
        {
            QuerySubjects();
        }

        private void EnablePaginationControls(bool enable)
        {
            SubjectsNextButton.IsEnabled = enable;
            SubjectsPreviousButton.IsEnabled = enable;
        }

        private async void AddSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            // Content dialogs sit outside of the actual window context, so need to be provided with the XamlRoot they should sit wtihin and need to instructed which theme to use
            AddSubjectDialog addSubjectDialog = new AddSubjectDialog()
            {
                XamlRoot = Content.XamlRoot,
                RequestedTheme = ThemeHelper.RootTheme
            };
            await addSubjectDialog.ShowAsync();

            if (addSubjectDialog.Update)
            {
                QuerySubjects();
            }
        }

        private async void EditSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            var subject = ((Button)sender).DataContext as Subject;

            // Is essential to make sure new entity is passed in here, otherwise the original referenced entity will save name even when cancelling
            EditSubjectDialog editSubjectDialog = new EditSubjectDialog(
                new Subject
                {
                    Id = subject.Id,
                    Name = subject.Name
                })
            {
                XamlRoot = Content.XamlRoot,
                RequestedTheme = ThemeHelper.RootTheme
            };
            await editSubjectDialog.ShowAsync();

            if (editSubjectDialog.Update)
            {
                QuerySubjects();
            }
        }

        private async void MarkHistoricSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            var subject = ((Button)sender).DataContext as Subject;

            ContentDialog markHistoricalDialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                RequestedTheme = ThemeHelper.RootTheme,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Are you sure?",
                Content = "Once you mark a subject as historical this cannot be undone",
                PrimaryButtonText = "Mark Historical",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = await markHistoricalDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await Task.Run(async () =>
                {
                    await SubjectDataController.SubjectHistorical(subject.Id);
                });
                SubjectMarkedHistorical?.Invoke(sender, e);
                QuerySubjects();

            }
        }
    }
}
