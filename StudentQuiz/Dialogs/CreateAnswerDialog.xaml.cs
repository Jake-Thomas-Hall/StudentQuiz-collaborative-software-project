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

namespace StudentQuiz.Dialogs
{
    public sealed partial class CreateAnswerDialog : ContentDialog
    {
        public Question ParentQuestion = new();
        public Answer NewAnswer = new();
        
        public CreateAnswerDialog(Question question)
        {
            this.InitializeComponent();
            ParentQuestion = question;

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

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var validateResult = ParentQuestion.ValidateNewAnswer(NewAnswer);
            if (!validateResult.success)
            {
                args.Cancel = true;
                ErrorInfobar.Message = validateResult.message;
                ErrorInfobar.IsOpen = true;
                return;
            }
        }
    }
}
