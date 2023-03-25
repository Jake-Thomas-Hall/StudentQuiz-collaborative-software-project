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
using StudentQuiz.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace StudentQuiz.Pages
{
    public sealed partial class SubjectsPage : Page
    {
        public SubjectsPage()
        {
            this.InitializeComponent();

            ActiveSubjectsList.SubjectMarkedHistorical += ActiveSubjectsList_SubjectMarkedHistorical;
        }

        private void ActiveSubjectsList_SubjectMarkedHistorical(object sender, RoutedEventArgs e)
        {
            HistoricalSubjectsList.RefreshSubjectsList();
        }
    }
}
