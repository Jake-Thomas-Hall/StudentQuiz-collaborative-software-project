// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using StudentQuiz.Entities;
using StudentQuiz.DataAccess;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using StudentQuiz.Helpers;
using StudentQuiz.Dialogs;

namespace StudentQuiz.Pages
{
    public sealed partial class UsersPage : Page
    {
        int CurrentUserGroup = 3;
        int CurrentUserId = 1;
        List<User> Users = new();
        ObservableCollection<User> DisplayedUsers = new();

        List<object> OrderOptions = new()
        {
            new { Visible = "Last Login Ascending", Value = "LastLoginDateTime" },
            new { Visible = "Last Login Descending", Value = "LastLoginDateTime DESC" },
            new { Visible = "Full Name (A-Z)", Value = "Name" },
            new { Visible = "Full Name (Z-A)", Value = "Name DESC" }
        };

        List<object> UserGroups = new()
        {
            new { Visible = "All", Value = 0 },
            new { Visible = "Students", Value = 1 },
            new { Visible = "Lecturers", Value = 2 },
            new { Visible = "Administrators", Value = 3 }
        };

        public UsersPage()
        {
            this.InitializeComponent();
        }
        
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (UserDataController.LoggedInUser is not null)
            {
                CurrentUserGroup = UserDataController.LoggedInUser.UserGroup.Id;
                CurrentUserId = UserDataController.LoggedInUser.Id;
            }
            await PopulateUsersList();
            RefreshTableUI();
        }

        private async Task PopulateUsersList()
        {
            await Task.Run(async () => {
                Users = await UserDataController.GetUsers();
            });
        }

        private void RefreshTableUI()
        {
            // Filter users acording to current user's UserGroup
            if (CurrentUserGroup == 2) // If Lecturer...
            {
                Users = Users.Where((user) => user.UserGroup.Id == 1).ToList();
                UserGroupFilterBox.Visibility = Visibility.Collapsed;
            }

            DisplayedUsers = new ObservableCollection<User>(Users);
            UsersItemsControl.ItemsSource = DisplayedUsers;
            ApplyFilters();
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ApplyFilters();
        }

        private void UserGroupFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void OrderByBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var userSearch = UsersSearchBox.Text;
            var userGroup = UserGroupFilterBox.SelectedValue as int?;
            var userOrder = OrderByBox.SelectedValue as string;
            var onlyApprovals = DisplayApprovalsCheckBox.IsChecked;

            var filteredUsers = Users.Where(u => u.UserGroup.Id == userGroup || userGroup == null || userGroup == 0);
            var searchedUsers = filteredUsers.Where(u => u.FullName.Contains(userSearch, StringComparison.OrdinalIgnoreCase) || u.Email.Contains(userSearch, StringComparison.OrdinalIgnoreCase));
            List<User> orderedUsers = new();
            
            switch(userOrder) // not pretty (amongst other things) but it works
            {
                case "LastLoginDateTime":
                    orderedUsers = searchedUsers.OrderBy((u) => u.LastLogin).ToList();
                    break;
                case "LastLoginDateTime DESC":
                    orderedUsers = searchedUsers.OrderByDescending((u) => u.LastLogin).ToList();
                    break;
                case "Name":
                    orderedUsers = searchedUsers.OrderBy((u) => u.FullName).ToList();
                    break;
                case "Name DESC":
                    orderedUsers = searchedUsers.OrderByDescending((u) => u.FullName).ToList();
                    break;
                case null:
                    orderedUsers = searchedUsers.ToList();
                    break;
            }

            if (onlyApprovals == true)
            {
                orderedUsers = orderedUsers.Where((u) => !u.IsConfirmed).ToList();
            }

            DisplayedUsers = new ObservableCollection<User>(orderedUsers);

            UsersItemsControl.ItemsSource = DisplayedUsers;
        }

        private async void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button).DataContext as User;

            await Task.Run(async () =>
            {
                await UserDataController.ApproveUser(user.Id);
            });
            user.IsConfirmed = true;
            ApplyFilters();
        }

        private async void AccessButton_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button).DataContext as User;
            var newStatus = !user.IsDisabled;
            await Task.Run(async () =>
            {
                await UserDataController.AccountAccess(user.Id, newStatus);
            });
            user.IsDisabled = newStatus;
            ApplyFilters(); // Updated bindings - need to do this to get the Lock/Unlock buttons to switch
        }

        private async void CreateUserButton_Click(object sender, RoutedEventArgs e)
        {
            AddUserDialog addUserDialog = new AddUserDialog()
            {
                XamlRoot = Content.XamlRoot,
                RequestedTheme = ThemeHelper.RootTheme
            };
            var result = await addUserDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await PopulateUsersList();
                RefreshTableUI();
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button).DataContext as User;
            EditUserDialog editUserDialog = new EditUserDialog(user, CurrentUserGroup)
            {
                XamlRoot = Content.XamlRoot,
                RequestedTheme = ThemeHelper.RootTheme
            };

            var result = await editUserDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await PopulateUsersList();
                RefreshTableUI();
            }
        }

        private async void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button).DataContext as User;

            int adminCount = 0;
            await Task.Run(async () =>
            {
                adminCount = await UserDataController.GetUserGroupCount(3); // Get number of admins
            });

            // Stop the last admin from being deleted
            if (adminCount <= 1 && user.UserGroup.Id == 3)
            {
                ErrorInfobar.Title = "Error Deleting User";
                ErrorInfobar.Message = "The last admin cannot be deleted from the system.";
                ErrorInfobar.IsOpen = true;
                return;
            }

            // Stop users from deleting themselves from this page
            if (user.Id == CurrentUserId)
            {
                ErrorInfobar.Title = "Error Deleting User";
                ErrorInfobar.Message = "You cannot delete your own account from this page. Use the Account page to perform this action.";
                ErrorInfobar.IsOpen = true;
                return;
            }

            ContentDialog deleteUserDialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                RequestedTheme = ThemeHelper.RootTheme,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Are you sure?",
                Content = "Deleting a user is permanent and cannot be undone.",
                PrimaryButtonText = "Delete User",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = await deleteUserDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await Task.Run(async () =>
                {
                    await UserDataController.DeleteUser(user.Id);
                    await PopulateUsersList();
                });

                RefreshTableUI();
            }
        }

        private void DisplayApprovalsCheckBox_CheckChange(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

    }
}
