using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace StudentQuiz.Helpers;

public static class ThemeHelper
{
    private const string SelectedAppThemeKey = "SelectedAppTheme";

    public static void Initialise()
    {
        var savedTheme = ApplicationData.Current.LocalSettings.Values[SelectedAppThemeKey]?.ToString();

        if (savedTheme != null)
        {
            RootTheme = App.GetEnum<ElementTheme>(savedTheme);
        }
    }

    public static ElementTheme RootTheme
    {
        get
        {
            var mainWindow = (Application.Current as App).Window;

            return ((FrameworkElement)mainWindow.Content).RequestedTheme;
        }
        set
        {
            var mainWindow = (Application.Current as App).Window;

            ((FrameworkElement)mainWindow.Content).RequestedTheme = value;

            ApplicationData.Current.LocalSettings.Values[SelectedAppThemeKey] = value.ToString();

            TitleBarHelper.UpdateTitleBar(value);
        }
    }
}

