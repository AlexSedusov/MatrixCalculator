using MatrixCalculator.Models;

namespace MatrixCalculator.Services;

public sealed class MatrixProblemGenerator
{
    private static readonly MatrixOperation[] Operations =
    [
        MatrixOperation.Add,
        MatrixOperation.Subtract,
        MatrixOperation.MultiplyMatrix,
        MatrixOperation.MultiplyScalar,
        MatrixOperation.Transpose,
        MatrixOperation.Determinant,
        MatrixOperation.Inverse
    ];

    private readonly Random _random = new();

    public MatrixProblem Generate(MatrixOperation? operation)
    {
        var selectedOperation = operation ?? Operations[_random.Next(Operations.Length)];

        return selectedOperation switch
        {
            MatrixOperation.Add => GenerateSameSizeProblem(MatrixOperation.Add),
            MatrixOperation.Subtract => GenerateSameSizeProblem(MatrixOperation.Subtract),
            MatrixOperation.MultiplyMatrix => GenerateMatrixMultiplicationProblem(),
            MatrixOperation.MultiplyScalar => GenerateScalarMultiplicationProblem(),
            MatrixOperation.Transpose => GenerateTransposeProblem(),
            MatrixOperation.Determinant => GenerateDeterminantProblem(),
            MatrixOperation.Inverse => GenerateInverseProblem(),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    }

    private MatrixProblem GenerateSameSizeProblem(MatrixOperation operation)
    {
        var rows = _random.Next(2, 4);
        var columns = _random.Next(2, 4);
        var matrixA = RandomMatrix(rows, columns, -6, 7);
        var matrixB = RandomMatrix(rows, columns, -6, 7);

        return MatrixProblem.Create(operation, matrixA, matrixB);
    }

    private MatrixProblem GenerateMatrixMultiplicationProblem()
    {
        var rowsA = _random.Next(2, 4);
        var common = _random.Next(2, 4);
        var columnsB = _random.Next(2, 4);
        var matrixA = RandomMatrix(rowsA, common, -4, 6);
        var matrixB = RandomMatrix(common, columnsB, -4, 6);

        return MatrixProblem.Create(MatrixOperation.MultiplyMatrix, matrixA, matrixB);
    }

    private MatrixProblem GenerateScalarMultiplicationProblem()
    {
        var rows = _random.Next(2, 4);
        var columns = _random.Next(2, 4);
        var scalar = RandomNonZero(-5, 6);

        return MatrixProblem.Create(MatrixOperation.MultiplyScalar, RandomMatrix(rows, columns, -5, 6), scalar: scalar);
    }

    private MatrixProblem GenerateTransposeProblem()
    {
        var rows = _random.Next(2, 4);
        var columns = _random.Next(2, 5);

        return MatrixProblem.Create(MatrixOperation.Transpose, RandomMatrix(rows, columns, -7, 8));
    }

    private MatrixProblem GenerateDeterminantProblem()
    {
        var size = _random.Next(2, 4);

        return MatrixProblem.Create(MatrixOperation.Determinant, RandomMatrix(size, size, -5, 6));
    }

    private MatrixProblem GenerateInverseProblem()
    {
        for (var attempt = 0; attempt < 100; attempt++)
        {
            var candidate = RandomMatrix(2, 2, -5, 6);
            var determinant = Math.Abs(candidate.Determinant());
            if (determinant is >= 1d and <= 8d)
            {
                return MatrixProblem.Create(MatrixOperation.Inverse, candidate);
            }
        }

        return MatrixProblem.Create(MatrixOperation.Inverse, new Matrix(new double[,] { { 1, 2 }, { 3, 5 } }));
    }

    private Matrix RandomMatrix(int rows, int columns, int minValue, int maxValueExclusive)
    {
        var matrix = new Matrix(rows, columns);

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                matrix[row, column] = _random.Next(minValue, maxValueExclusive);
            }
        }

        return matrix;
    }

    private int RandomNonZero(int minValue, int maxValueExclusive)
    {
        int value;
        do
        {
            value = _random.Next(minValue, maxValueExclusive);
        }
        while (value == 0);

        return value;
    }
}
