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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace StudentQuiz.Dialogs
{
    public sealed partial class AssignTestDialog : ContentDialog
    {
        private List<User> SelectedUsers = new();
        private int TestId = 0;

        public AssignTestDialog(int testId)
        {
            this.InitializeComponent();
            TestId = testId;
            Loaded += ContentDialog_Loaded;
            AssignDueDate.MinDate = DateTimeOffset.Now;
            AssignDueTime.Time = new TimeSpan(14, 0, 0);
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // Needed to fix a Win UI bug where dialog background overlay doesn't overlap the custom title bar
            var parent = VisualTreeHelper.GetParent(this);
            var child = VisualTreeHelper.GetChild(parent, 0);
            var frame = (Microsoft.UI.Xaml.Shapes.Rectangle)child;
            frame.Margin = new Thickness(0);
            frame.RegisterPropertyChangedCallback(
                MarginProperty,
                (DependencyObject sender, DependencyProperty dp) =>
                {
                    if (dp == MarginProperty)
                        sender.ClearValue(dp);
                });
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var date = AssignDueDate.Date;
            var time = AssignDueTime.Time;

            // Show errors if no date is provided or if no selected users are specified
            if (date is null)
            {
                args.Cancel = true;
                ErrorInfobar.IsOpen = true;
                ErrorInfobar.Message = "You must provide a due date and time.";
                return;
            }

            if (SelectedUsers.Count == 0)
            {
                args.Cancel = true;
                ErrorInfobar.IsOpen = true;
                ErrorInfobar.Message = "You must select at least one user to assign to a test.";
                return;
            }

            // Set the selected time into the date offset
            var dueDateTime = date.Value.Date + time;

            var deferral = args.GetDeferral();
            await Task.Run(async () =>
            {
                await TestAssignmentDataController.CreateTestAssignments(SelectedUsers, dueDateTime, TestId);
            });
            deferral.Complete();
        }

        private async void AssignStudentSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var search = AssignStudentSearch.Text;

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && !string.IsNullOrEmpty(search?.Trim()))
            {
                var users = new List<User>();
                await Task.Run(async () =>
                {
                    users = await UserDataController.GetUsers(search);
                });

                FoundUsersListControl.ItemsSource = users;

                if (users.Count > 0)
                {
                    FoundUsersStackPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    FoundUsersStackPanel.Visibility = Visibility.Collapsed;
                }

                return;
            }

            FoundUsersStackPanel.Visibility = Visibility.Collapsed;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button).DataContext as User;

            SelectedUsers.Remove(user);

            SelectedUsersListControl.ItemsSource = new ObservableCollection<User>(SelectedUsers);

            if (SelectedUsers.Count == 0)
            {
                SelectedUsersStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button).DataContext as User;

            if (SelectedUsers.Contains(user))
            {
                return;
            }

            SelectedUsers.Add(user);
            SelectedUsersListControl.ItemsSource = new ObservableCollection<User>(SelectedUsers);

            if (SelectedUsers.Count > 0)
            {
                SelectedUsersStackPanel.Visibility = Visibility.Visible;
            }
        }
    }
}
