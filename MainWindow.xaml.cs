using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MatrixCalculator;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MatrixGrid_AutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (e.Column is not DataGridTextColumn textColumn)
        {
            return;
        }

        textColumn.Binding = new Binding($"[{e.PropertyName}]")
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        textColumn.ElementStyle = CreateMatrixTextBlockStyle();
        textColumn.EditingElementStyle = CreateMatrixTextBoxStyle();
    }

    private static Style CreateMatrixTextBlockStyle()
    {
        var style = new Style(typeof(TextBlock));
        style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
        style.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
        style.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center));
        style.Setters.Add(new Setter(TextBlock.FontSizeProperty, 28d));
        style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold));
        return style;
    }

    private static Style CreateMatrixTextBoxStyle()
    {
        var style = new Style(typeof(TextBox));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
        style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        style.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
        style.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch));
        style.Setters.Add(new Setter(Control.FontSizeProperty, 28d));
        style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(0)));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
        return style;
    }
}
