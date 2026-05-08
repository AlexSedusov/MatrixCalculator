namespace MatrixCalculator.ViewModels;

public sealed class MatrixCellViewModel
{
    public MatrixCellViewModel(string text, bool isHighlighted, bool isMuted)
    {
        Text = text;
        IsHighlighted = isHighlighted;
        IsMuted = isMuted;
    }

    public string Text { get; }

    public bool IsHighlighted { get; }

    public bool IsMuted { get; }
}
