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
using System.Threading.Tasks;
using StudentQuiz.DataAccess;
using System.Data.Common;

namespace StudentQuiz.Dialogs
{
    public sealed partial class AddSubjectDialog : ContentDialog
    {
        public Subject Subject { get; set; } = new();
        public bool Update { get; set; }

        public AddSubjectDialog()
        {
            this.InitializeComponent();

            ErrorInfobar.Closed += ErrorInfobar_Closed;
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
            // Ensure subject is valid, do not continue if not.
            var validateResult = Subject.Validate();
            if (!validateResult.success)
            {
                args.Cancel = true;
                SubjectDuplicateInfobar.Margin = new Thickness(0, 0, 0, 16);
                ErrorInfobar.Message = validateResult.message;
                ErrorInfobar.IsOpen = true;
                return;
            }

            var deferral = args.GetDeferral();
            try
            {
                await Task.Run(async () =>
                {
                    await SubjectDataController.AddSubject(Subject);
                });
                deferral.Complete();
            }
            catch(DbException)
            {
                args.Cancel = true;
                SubjectDuplicateInfobar.Margin = new Thickness(0, 0, 0, 16);
                ErrorInfobar.Message = $"Could not insert subject '{Subject.Name}', this active subject already exists";
                ErrorInfobar.IsOpen = true;
                deferral.Complete();
                return;
            }

            Update = true;
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Update = false;
        }

        private void ErrorInfobar_Closed(InfoBar sender, InfoBarClosedEventArgs args)
        {
            // Note to self: Don't forget to re-set thickness when showing an error.
            SubjectDuplicateInfobar.Margin = new Thickness(0);
        }
    }
}
