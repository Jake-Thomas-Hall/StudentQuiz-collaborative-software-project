// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using StudentQuiz.DataAccess;
using StudentQuiz.Helpers;
using StudentQuiz.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;

namespace StudentQuiz
{
    public sealed partial class MainWindow : Window
    {
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type page)>
        {
            ("tests", typeof(TestsPage)),
            ("subjects", typeof(SubjectsPage)),
            ("users", typeof(UsersPage)),
            ("account", typeof(AccountPage)),
            ("managetests", typeof(ManageTestsPage)),
            ("leaderboard", typeof(LeaderboardPage)),
            ("home", typeof(HomePage)),
            ("signup", typeof(SignUp))
        };

        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // Handler for content frame navigation
            contentFrame.Navigated += On_Navgiated;

            // Restrict navbar behaviour when taking test, return to usual after end of test.
            EventManager.StartTest += EventManager_StartTest;
            EventManager.EndTest += EventManager_EndTest;
            EventManager.Login += EventManager_Login;

            // Start with all nav content nav items hidden, until "login" happens from tests page
            HideAllNavItems();

            // Means landing page will always be selected by default
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void EventManager_Login(object sender, EventArgs e)
        {
            // In case login changes, rehide all items before showing user specific nav items
            HideAllNavItems();

            contentFrame.BackStack.Clear();

            // Hide sign up page once user "logs in"
            SignUpNavItem.Visibility = Visibility.Collapsed;

            // Show only menu items that are relevant to logged in user type
            switch (UserDataController.LoggedInUser.UserGroup.Group)
            {
                case "Students":
                    LogoutNavItem.Visibility = Visibility.Visible;
                    AccountNavItem.Visibility = Visibility.Visible;
                    TestsNavItem.Visibility = Visibility.Visible;
                    LeaderboardNavItem.Visibility = Visibility.Visible;
                    contentFrame.Navigate(typeof(TestsPage));
                    break;
                case "Lecturer":
                    LogoutNavItem.Visibility = Visibility.Visible;
                    AccountNavItem.Visibility = Visibility.Visible;
                    TestsNavItem.Visibility = Visibility.Visible;
                    LeaderboardNavItem.Visibility = Visibility.Visible;
                    UsersNavItem.Visibility = Visibility.Visible;
                    ManageTestsNavItem.Visibility = Visibility.Visible;
                    contentFrame.Navigate(typeof(ManageTestsPage));
                    break;
                case "Admin":
                    LogoutNavItem.Visibility = Visibility.Visible;
                    AccountNavItem.Visibility = Visibility.Visible;
                    TestsNavItem.Visibility = Visibility.Visible;
                    LeaderboardNavItem.Visibility = Visibility.Visible;
                    UsersNavItem.Visibility = Visibility.Visible;
                    ManageTestsNavItem.Visibility = Visibility.Visible;
                    SubjectsNavItem.Visibility = Visibility.Visible;
                    contentFrame.Navigate(typeof(UsersPage));
                    break;
                default:
                    break;
            }
        }

        private void EventManager_StartTest(object sender, EventArgs e)
        {
            NavView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
            NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
            NavView.IsPaneToggleButtonVisible = false;
            AppTitle.Text = "";
        }

        private void EventManager_EndTest(object sender, EventArgs e)
        {
            NavView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;
            NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
            NavView.IsPaneToggleButtonVisible = true;
            AppTitle.Text = "Student Quiz";
        }

        private void NavView_MenuNavigation(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
            }
            else if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            TryGoBack();
        }

        private bool TryGoBack()
        {
            if (!contentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed
            if (NavView.IsPaneOpen && (NavView.DisplayMode == NavigationViewDisplayMode.Compact || NavView.DisplayMode == NavigationViewDisplayMode.Minimal))
                return false;

            contentFrame.GoBack();
            return true;
        }

        private void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo)
        {
            Type _page = null;

            if (navItemTag == "settings")
            {
                _page = typeof(SettingsPage);
            }
            else
            {
                var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
                _page = item.Page;
            }

            var preNavPageType = contentFrame.CurrentSourcePageType;

            if (!(_page is null) && !Equals(preNavPageType, _page))
            {
                contentFrame.Navigate(_page, null, transitionInfo);
            }
        }

        private void NavView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            Thickness currentMargin = AppTitleBar.Margin;

            if (sender.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                AppTitleBar.Margin = new Thickness() { Left = (sender.CompactPaneLength * 2), Top = currentMargin.Top, Right = currentMargin.Right, Bottom = currentMargin.Bottom };
            }
            else
            {
                AppTitleBar.Margin = new Thickness() { Left = (sender.CompactPaneLength), Top = currentMargin.Top, Right = currentMargin.Right, Bottom = currentMargin.Bottom };
            }

            UpdateAppTitleMargin(sender);
        }

        private void UpdateAppTitleMargin(NavigationView sender)
        {
            AppTitle.TranslationTransition = new Vector3Transition();

            if ((sender.DisplayMode == NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                     sender.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                AppTitle.Translation = new System.Numerics.Vector3(4, 0, 0);
            }
            else
            {
                AppTitle.Translation = new System.Numerics.Vector3(24, 0, 0);
            }
        }

        private void NavView_PaneOpening(NavigationView sender, object args)
        {
            UpdateAppTitleMargin(sender);
        }

        private void NavView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
        {
            UpdateAppTitleMargin(sender);
        }

        private void On_Navgiated(object sender, NavigationEventArgs e)
        {
            if (contentFrame.SourcePageType == typeof(SettingsPage))
            {
                NavView.SelectedItem = NavView.SettingsItem;
                NavView.Header = "Settings";
            }
            else if (contentFrame.SourcePageType != null)
            {
                var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

                // Check through the regular menu items and footer items to find the relevant menu item
                var menuItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(n => n.Tag.Equals(item.Tag));
                var footerItem = NavView.FooterMenuItems.OfType<NavigationViewItem>().FirstOrDefault(n => n.Tag.Equals(item.Tag));
                NavView.SelectedItem = menuItem != null ? menuItem : footerItem;

                NavView.Header = ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
            }
        }

        private void HideAllNavItems()
        {
            LogoutNavItem.Visibility = Visibility.Collapsed;
            AccountNavItem.Visibility = Visibility.Collapsed;
            TestsNavItem.Visibility = Visibility.Collapsed;
            UsersNavItem.Visibility = Visibility.Collapsed;
            SubjectsNavItem.Visibility = Visibility.Collapsed;
            ManageTestsNavItem.Visibility = Visibility.Collapsed;
            LeaderboardNavItem.Visibility = Visibility.Collapsed;
        }

        public void OverrideHeader(string header)
        {
            NavView.Header = header;
        }
    }
}
