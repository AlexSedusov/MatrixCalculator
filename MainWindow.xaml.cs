using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MatrixCalculator.Services;
using WpfBinding = System.Windows.Data.Binding;
using WpfControl = System.Windows.Controls.Control;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace MatrixCalculator;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        HelpService.ShowHelp();
    }

    private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.F1)
        {
            return;
        }

        e.Handled = true;
        HelpService.ShowHelp(FindNearestHelpId() ?? 1000);
    }

    private void MatrixGrid_AutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (e.Column is not DataGridTextColumn textColumn)
        {
            return;
        }

        textColumn.Binding = new WpfBinding($"[{e.PropertyName}]")
        {
            Mode = System.Windows.Data.BindingMode.TwoWay,
            UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
        };
        textColumn.ElementStyle = CreateMatrixTextBlockStyle();
        textColumn.EditingElementStyle = CreateMatrixTextBoxStyle();
    }

    private static Style CreateMatrixTextBlockStyle()
    {
        var style = new Style(typeof(TextBlock));
        style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
        style.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Stretch));
        style.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center));
        style.Setters.Add(new Setter(TextBlock.FontSizeProperty, 28d));
        style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold));
        return style;
    }

    private static Style CreateMatrixTextBoxStyle()
    {
        var style = new Style(typeof(WpfTextBox));
        style.Setters.Add(new Setter(WpfControl.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Center));
        style.Setters.Add(new Setter(WpfControl.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        style.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Stretch));
        style.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch));
        style.Setters.Add(new Setter(WpfControl.FontSizeProperty, 28d));
        style.Setters.Add(new Setter(WpfControl.FontWeightProperty, FontWeights.SemiBold));
        style.Setters.Add(new Setter(WpfControl.PaddingProperty, new Thickness(0)));
        style.Setters.Add(new Setter(WpfControl.BorderThicknessProperty, new Thickness(0)));
        return style;
    }

    private int? FindNearestHelpId()
    {
        var current = Keyboard.FocusedElement as DependencyObject ?? FocusManager.GetFocusedElement(this) as DependencyObject ?? this;

        while (current is not null)
        {
            var helpId = HelpService.GetHelpId(current);
            if (helpId > 0)
            {
                return helpId;
            }

            current = GetParent(current);
        }

        return null;
    }

    private static DependencyObject? GetParent(DependencyObject current)
    {
        if (current is Visual or Visual3D)
        {
            var visualParent = VisualTreeHelper.GetParent(current);
            if (visualParent is not null)
            {
                return visualParent;
            }
        }

        return LogicalTreeHelper.GetParent(current);
    }
}
