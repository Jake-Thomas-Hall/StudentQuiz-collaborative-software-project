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
using StudentQuiz.Entities.DataEntities;
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
    public sealed partial class LeaderboardPage : Page
    {
        private List<Subject> Subjects = new List<Subject>
        { 
            new()
            {
                Name = "All",
                Id = 0
            }
        };
        private List<Leaderboard> Leaderboards = new();

        private List<object> OrderOptions = new()
        {
            new { Visible = "Default", Value = "Default" },
            new { Visible = "Total Points Ascending", Value = "TotalPoints" },
            new { Visible = "Total Points Descending", Value = "TotalPoints DESC" },
            new { Visible = "Correct Answers Ascending", Value = "CorrectAnswers" },
            new { Visible = "Correct Answers Descending", Value = "CorrectAnswers DESC" },
            new { Visible = "Percentage Correct Ascending", Value = "PercentageCorrect" },
            new { Visible = "Percentage Correct Descending", Value = "PercentageCorrect DESC" }
        };

        public LeaderboardPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var subjects = new List<Subject> { };
            // need to store and query on page load
            await Task.Run(async () =>
            {
                subjects = await SubjectDataController.GetSubjects();
            });

            Subjects.AddRange(subjects);

            SubjectComboBox.ItemsSource = Subjects;
            SubjectComboBox.SelectedIndex = 0;
        }

        private async void SubjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedSubject = SubjectComboBox.SelectedItem as Subject;

            var leaderboards = new List<Leaderboard> { };
            // need to store and query on page load
            await Task.Run(async () =>
            {
                leaderboards = await LeaderboardDataController.GetLeaderboard(subjectId: selectedSubject.Id);
            });

            LeaderboardSearchBox.Text = null;
            LeaderboardOrderComboBox.SelectedValue = null;

            // Only take top 10 to display for score, percentage and question count charts
            StudentScoreChartSeries.ItemsSource = leaderboards.Take(10).ToList();
            StudentPercentageChartSeries.ItemsSource = leaderboards.OrderByDescending(x => x.PercentageCorrect).Take(10).ToList();
            StudentAnswerCorrectChartSeries.ItemsSource = leaderboards.OrderByDescending(x => x.CorrectQuestionCount).Take(10).ToList();
            AllLeaderboardListControl.ItemsSource = leaderboards;
            Leaderboards = leaderboards;
            ApplyFilters();
        }

        private void LeaderboardOrderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void LeaderboardSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var leaderboardSearch = LeaderboardSearchBox.Text;
            var leaderboardOrder = LeaderboardOrderComboBox.SelectedValue as string;

            var filteredLeaderboard = Leaderboards.Where(x => x.FullName.Contains(leaderboardSearch, StringComparison.OrdinalIgnoreCase)).ToList();

            switch (leaderboardOrder)
            {
                case "TotalPoints":
                    filteredLeaderboard = filteredLeaderboard.OrderBy(x => x.ScoreCount).ToList();
                    break;
                case "TotalPoints DESC":
                    filteredLeaderboard = filteredLeaderboard.OrderByDescending(x => x.ScoreCount).ToList();
                    break;
                case "CorrectAnswers":
                    filteredLeaderboard = filteredLeaderboard.OrderBy(x => x.CorrectQuestionCount).ToList();
                    break;
                case "CorrectAnswers DESC":
                    filteredLeaderboard = filteredLeaderboard.OrderByDescending(x => x.CorrectQuestionCount).ToList();
                    break;
                case "PercentageCorrect":
                    filteredLeaderboard = filteredLeaderboard.OrderBy(x => x.PercentageCorrect).ToList();
                    break;
                case "PercentageCorrect DESC":
                    filteredLeaderboard = filteredLeaderboard.OrderByDescending(x => x.PercentageCorrect).ToList();
                    break;
                default:
                    break;
            }

            AllLeaderboardListControl.ItemsSource = filteredLeaderboard;
        }
    }
}
