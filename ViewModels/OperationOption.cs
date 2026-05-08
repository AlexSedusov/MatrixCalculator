using MatrixCalculator.Services;

namespace MatrixCalculator.ViewModels;

public sealed class OperationOption
{
    public OperationOption(MatrixOperation? operation, string displayName)
    {
        Operation = operation;
        DisplayName = displayName;
    }

    public MatrixOperation? Operation { get; }

    public string DisplayName { get; }
}
