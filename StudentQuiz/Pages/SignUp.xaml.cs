// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
using StudentQuiz.Helpers;
using System.Threading.Tasks;
using StudentQuiz.DataAccess;

namespace StudentQuiz.Pages
{
    public sealed partial class SignUp : Page
    {
        public SignUp()
        {
            this.InitializeComponent();
        }

        private async void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            var user = new User
            {
                UserGroup = new UserGroup { Id = 1},
                Email = EmailTextBox.Text,
                FirstName = FirstNameTextBox.Text,
                LastName = LastNameTextBox.Text,
                PhoneNumber = PhoneNumberTextBox.Text,
                CourseTitle = CourseTitleTextBox.Text,
                StudentNumber = StudentNumberTextBox.Text
            };
           
            if (string.IsNullOrEmpty(user.Email.Trim()) || 
                string.IsNullOrEmpty(user.FirstName.Trim()) ||
                string.IsNullOrEmpty(user.LastName.Trim()) ||
                string.IsNullOrEmpty(user.PhoneNumber.Trim()) ||
                string.IsNullOrEmpty(user.CourseTitle.Trim()) ||
                string.IsNullOrEmpty(user.StudentNumber.Trim()) ||
                string.IsNullOrEmpty(PasswordTextBox.Password.Trim()) ||
                string.IsNullOrEmpty(PasswordConfirmTextBox.Password.Trim()))
            {
                ErrorInfoBar.Message = "You must provide a value for these fields";
                ErrorInfoBar.IsOpen = true;
                return;
            }

            if (PasswordTextBox.Password != PasswordConfirmTextBox.Password)
            {
                ErrorInfoBar.Message = "Your passwords must match";
                ErrorInfoBar.IsOpen = true;
                return;
            }

            user.Password = PasswordHelper.Hash(PasswordTextBox.Password);

            SignUpButton.IsEnabled = false;

            await Task.Run(async () =>
            {
                await UserDataController.AddUser(user);
            });
        }
    }
}
