namespace MatrixCalculator.ViewModels;

public sealed class MatrixVisualViewModel
{
    public MatrixVisualViewModel(string title, int rows, int columns, IReadOnlyList<MatrixCellViewModel> cells)
    {
        Title = title;
        Rows = rows;
        Columns = columns;
        Cells = cells;
    }

    public string Title { get; }

    public int Rows { get; }

    public int Columns { get; }

    public IReadOnlyList<MatrixCellViewModel> Cells { get; }
}
