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

namespace StudentQuiz.Dialogs
{
    public sealed partial class AddUserDialog : ContentDialog
    {
        public User User { get; set; } = new();

        List<object> UserGroups = new()
        {
            new { Visible = "Lecturers", Value = 2 },
            new { Visible = "Administrators", Value = 3 }
        };

        public AddUserDialog()
        {
            this.InitializeComponent();
            User.UserGroup = new();
            User.IsConfirmed = true;
            User.CourseTitle = "";
            Loaded += ContentDialog_Loaded;
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
                    await UserDataController.CreateUser(User, Guid.NewGuid().ToString()); // guidv4 for password
                });
                deferral.Complete();
            }
            catch (DbException)
            {
                args.Cancel = true;
                ErrorInfobar.Message = $"Could not create user '{User.Email}', this email address is already in use.";
                ErrorInfobar.IsOpen = true;
                deferral.Complete();
                return;
            }

        }
    }
}
