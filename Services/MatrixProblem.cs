using MatrixCalculator.Models;

namespace MatrixCalculator.Services;

public sealed class MatrixProblem
{
    private MatrixProblem(
        MatrixOperation operation,
        Matrix matrixA,
        Matrix? matrixB,
        double? scalar,
        Matrix? matrixAnswer,
        double? numberAnswer)
    {
        Operation = operation;
        MatrixA = matrixA;
        MatrixB = matrixB;
        Scalar = scalar;
        MatrixAnswer = matrixAnswer;
        NumberAnswer = numberAnswer;
    }

    public MatrixOperation Operation { get; }

    public Matrix MatrixA { get;}

    public Matrix? MatrixB { get; }

    public double? Scalar { get; }

    public Matrix? MatrixAnswer { get; }

    public double? NumberAnswer { get; }

    public bool ExpectsMatrix => MatrixAnswer is not null;

    public static MatrixProblem Create(MatrixOperation operation, Matrix matrixA, Matrix? matrixB = null, double? scalar = null)
    {
        return operation switch
        {
            MatrixOperation.Add => new MatrixProblem(operation, matrixA, matrixB, null, matrixA.Add(RequireMatrixB(matrixB)), null),
            MatrixOperation.Subtract => new MatrixProblem(operation, matrixA, matrixB, null, matrixA.Subtract(RequireMatrixB(matrixB)), null),
            MatrixOperation.MultiplyMatrix => new MatrixProblem(operation, matrixA, matrixB, null, matrixA.Multiply(RequireMatrixB(matrixB)), null),
            MatrixOperation.MultiplyScalar => new MatrixProblem(operation, matrixA, null, scalar, matrixA.Multiply(RequireScalar(scalar)), null),
            MatrixOperation.Transpose => new MatrixProblem(operation, matrixA, null, null, matrixA.Transpose(), null),
            MatrixOperation.Determinant => new MatrixProblem(operation, matrixA, null, null, null, matrixA.Determinant()),
            MatrixOperation.Inverse => new MatrixProblem(operation, matrixA, null, null, matrixA.Inverse(), null),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    }

    private static Matrix RequireMatrixB(Matrix? matrixB)
    {
        return matrixB ?? throw new ArgumentNullException(nameof(matrixB), "The operation requires a second matrix.");
    }

    private static double RequireScalar(double? scalar)
    {
        return scalar ?? throw new ArgumentNullException(nameof(scalar), "The operation requires a scalar.");
    }
}
