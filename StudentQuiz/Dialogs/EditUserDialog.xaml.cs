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
using System.Data.Common;
using System.Threading.Tasks;
using StudentQuiz.DataAccess;
using System.Collections.ObjectModel;

namespace StudentQuiz.Dialogs
{
    public sealed partial class EditUserDialog : ContentDialog
    {
        int CurrentUserGroup = 0;
        User User = new();

        ObservableCollection<UserGroup> UserGroups = new()
        {
            new UserGroup { Id = 1, Group = "Students"},
            new UserGroup { Id = 2, Group = "Lecturers"},
            new UserGroup { Id = 3, Group = "Administrators"}
        };

        public EditUserDialog(User user, int currentUserGroup)
        {
            User = user;
            CurrentUserGroup = currentUserGroup;
            this.InitializeComponent();
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
            var validateResult = User.Validate();

            if (!validateResult.success)
            {
                args.Cancel = true;
                ErrorInfobar.Message = validateResult.message;
                ErrorInfobar.IsOpen = true;
                return;
            }

            var deferral = args.GetDeferral();
            try
            {
                await Task.Run(async () =>
                {
                    await UserDataController.UpdateUser(User);
                });
                deferral.Complete();
            }
            catch (DbException)
            {
                args.Cancel = true;
                ErrorInfobar.Message = $"Error updating user '{User.Email}'.";
                ErrorInfobar.IsOpen = true;
                deferral.Complete();
                return;
            }
        }

        private void UserTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            User.UserGroup.Id = Convert.ToInt32(UserTypeComboBox.SelectedValue as int?);
            CourseTitleTextBox.Visibility = User.UserGroup.Id == 1 ? Visibility.Visible : Visibility.Collapsed;
            StudentNumberTextBox.Visibility = User.UserGroup.Id == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UserTypeComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentUserGroup == 2) // If Lecturer...
            {
                UserTypeComboBox.Visibility = Visibility.Collapsed;
            }
            
            UserTypeComboBox.SelectedIndex = UserGroups.IndexOf(UserGroups.Where(ug => ug.Id == User.UserGroup.Id).First());
        }
    }
}
