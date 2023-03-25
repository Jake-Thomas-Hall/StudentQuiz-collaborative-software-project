// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using StudentQuiz.Entities.DataEntities;
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
using StudentQuiz.DataAccess;

namespace StudentQuiz.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var currentTheme = ThemeHelper.RootTheme.ToString();
            (ThemePanel.Children.Cast<RadioButton>()).FirstOrDefault(x => x?.Tag?.ToString() == currentTheme).IsChecked = true;

            var users = new List<User> { };
            await Task.Run(async () =>
            {
                users = await UserDataController.GetUsers();
            });

            if (UserDataController.LoggedInUser is not null)
            {
                var currentUser = users.First(x => x.Id == UserDataController.LoggedInUser.Id);

                UserComboBox.SelectedItem = currentUser;
            }
            UserComboBox.ItemsSource = users;
        }

        private void OnThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var selectedTheme = ((RadioButton)sender)?.Tag?.ToString();

            if (selectedTheme != null)
            {
                ThemeHelper.RootTheme = App.GetEnum<ElementTheme>(selectedTheme);
            }
        }

        private void UserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedUser = UserComboBox.SelectedItem as User;

            if (selectedUser.Id != UserDataController.LoggedInUser?.Id)
            {
                UserDataController.LoggedInUser = selectedUser;
                EventManager.InvokeLogin(sender, e);
            }
        }
    }
}
