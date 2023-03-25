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
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace StudentQuiz.Pages
{
    public sealed partial class AccountPage : Page
    {
        private User User = new();

        public AccountPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var user = new User();
            await Task.Run(async () =>
            {
                user = await UserDataController.GetUserById(UserDataController.LoggedInUser.Id);
            });

            User = user;

            if (!User.IsStudent)
            {
                StudentCourseContainer.Visibility = Visibility.Collapsed;
                StudentNumberContainer.Visibility = Visibility.Collapsed;
            }

            UserFirstNameTextBox.Text = User.FirstName;
            UserLastNameTextBox.Text = User.LastName;
            UserPhoneNumberTextBox.Text = User.PhoneNumber;
            UserStudentNumberText.Text = User.StudentNumber;
            UserCourseText.Text = User.CourseTitle;
            UserEmailText.Text = User.Email;
            UserTypeText.Text = User.UserGroup.Group;
        }

        private async void UpdateUserButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                await UserDataController.AccountUpdateDetails(User);
            });
            UpdateDetailsInfobar.IsOpen = true;
        }

        private async void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog confirmDeleteDialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                RequestedTheme = ThemeHelper.RootTheme,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Are you sure?",
                Content = "Please note that this action is permanent, you cannot undo account deletion.",
                PrimaryButtonText = "Delete Account",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = await confirmDeleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await Task.Run(async () =>
                {
                    await UserDataController.DeleteUser(User.Id);
                });

                // Navigate away from page and clear navigation history, would perform logout action here, go to any other page for now.
                Frame.Navigate(typeof(SubjectsPage));
                Frame.BackStack.Clear();
            }
        }
    }
}
