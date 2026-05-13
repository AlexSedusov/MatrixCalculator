using System.Globalization;
using System.IO;
using System.Windows;
using FormsHelp = System.Windows.Forms.Help;
using HelpNavigator = System.Windows.Forms.HelpNavigator;

namespace MatrixCalculator.Services;

public static class HelpService
{
    public static readonly DependencyProperty HelpIdProperty = DependencyProperty.RegisterAttached(
        "HelpId",
        typeof(int),
        typeof(HelpService),
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.Inherits));

    public static void SetHelpId(DependencyObject element, int value)
    {
        element.SetValue(HelpIdProperty, value);
    }

    public static int GetHelpId(DependencyObject element)
    {
        return (int)element.GetValue(HelpIdProperty);
    }

    public static void ShowHelp()
    {
        FormsHelp.ShowHelp(null, GetHelpFilePath());
    }

    public static void ShowHelp(int helpContextId)
    {
        if (helpContextId <= 0)
        {
            ShowHelp();
            return;
        }

        FormsHelp.ShowHelp(
            null,
            GetHelpFilePath(),
            HelpNavigator.TopicId,
            helpContextId.ToString(CultureInfo.InvariantCulture));
    }

    private static string GetHelpFilePath()
    {
        return Path.Combine(AppContext.BaseDirectory, "Help", "MatrixCalculator.chm");
    }
}
