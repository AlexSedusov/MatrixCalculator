using System.Globalization;
using System.Text;
using MatrixCalculator.Models;

namespace MatrixCalculator.Services;

public sealed class MatrixSolutionFormatter
{
    public string Build(MatrixProblem problem, string language)
    {
        var culture = language == "ru" ? CultureInfo.GetCultureInfo("ru-RU") : CultureInfo.GetCultureInfo("en-US");
        var ru = language == "ru";
        var builder = new StringBuilder();

        builder.AppendLine(ru ? "Дано:" : "Given:");
        builder.AppendLine("A =");
        builder.AppendLine(problem.MatrixA.ToDisplayString(culture));

        if (problem.MatrixB is not null)
        {
            builder.AppendLine();
            builder.AppendLine("B =");
            builder.AppendLine(problem.MatrixB.ToDisplayString(culture));
        }

        if (problem.Scalar is not null)
        {
            builder.AppendLine();
            builder.AppendLine((ru ? "Число: " : "Scalar: ") + Format(problem.Scalar.Value, culture));
        }

        builder.AppendLine();

        switch (problem.Operation)
        {
            case MatrixOperation.Add:
                AppendElementwiseSolution(builder, problem, culture, ru, '+');
                break;
            case MatrixOperation.Subtract:
                AppendElementwiseSolution(builder, problem, culture, ru, '-');
                break;
            case MatrixOperation.MultiplyMatrix:
                AppendMatrixMultiplicationSolution(builder, problem, culture, ru);
                break;
            case MatrixOperation.MultiplyScalar:
                AppendScalarMultiplicationSolution(builder, problem, culture, ru);
                break;
            case MatrixOperation.Transpose:
                AppendTransposeSolution(builder, problem, culture, ru);
                break;
            case MatrixOperation.Determinant:
                AppendDeterminantSolution(builder, problem, culture, ru);
                break;
            case MatrixOperation.Inverse:
                AppendInverseSolution(builder, problem, culture, ru);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(problem), problem.Operation, null);
        }

        return builder.ToString();
    }

    private static void AppendElementwiseSolution(StringBuilder builder, MatrixProblem problem, CultureInfo culture, bool ru, char operationSign)
    {
        var matrixB = problem.MatrixB ?? throw new InvalidOperationException();
        var answer = problem.MatrixAnswer ?? throw new InvalidOperationException();

        builder.AppendLine(ru
            ? "Работаем поэлементно: элементы с одинаковыми индексами складываются или вычитаются."
            : "Work element by element: combine entries with the same indexes.");

        for (var row = 0; row < answer.Rows; row++)
        {
            for (var column = 0; column < answer.Columns; column++)
            {
                builder.AppendLine(
                    $"c{row + 1}{column + 1} = {Format(problem.MatrixA[row, column], culture)} {operationSign} {Format(matrixB[row, column], culture)} = {Format(answer[row, column], culture)}");
            }
        }

        AppendMatrixAnswer(builder, answer, culture, ru);
    }

    private static void AppendMatrixMultiplicationSolution(StringBuilder builder, MatrixProblem problem, CultureInfo culture, bool ru)
    {
        var matrixB = problem.MatrixB ?? throw new InvalidOperationException();
        var answer = problem.MatrixAnswer ?? throw new InvalidOperationException();

        builder.AppendLine(ru
            ? "Каждый элемент C равен сумме произведений элементов строки A и столбца B."
            : "Each entry of C is the dot product of a row of A and a column of B.");

        for (var row = 0; row < answer.Rows; row++)
        {
            for (var column = 0; column < answer.Columns; column++)
            {
                var products = new List<string>();
                for (var index = 0; index < problem.MatrixA.Columns; index++)
                {
                    products.Add($"{Format(problem.MatrixA[row, index], culture)}*{Format(matrixB[index, column], culture)}");
                }

                builder.AppendLine($"c{row + 1}{column + 1} = {string.Join(" + ", products)} = {Format(answer[row, column], culture)}");
            }
        }

        AppendMatrixAnswer(builder, answer, culture, ru);
    }

    private static void AppendScalarMultiplicationSolution(StringBuilder builder, MatrixProblem problem, CultureInfo culture, bool ru)
    {
        var scalar = problem.Scalar ?? throw new InvalidOperationException();
        var answer = problem.MatrixAnswer ?? throw new InvalidOperationException();

        builder.AppendLine(ru
            ? "Умножаем каждый элемент матрицы на заданное число."
            : "Multiply every matrix entry by the scalar.");

        for (var row = 0; row < answer.Rows; row++)
        {
            for (var column = 0; column < answer.Columns; column++)
            {
                builder.AppendLine(
                    $"c{row + 1}{column + 1} = {Format(problem.MatrixA[row, column], culture)}*{Format(scalar, culture)} = {Format(answer[row, column], culture)}");
            }
        }

        AppendMatrixAnswer(builder, answer, culture, ru);
    }

    private static void AppendTransposeSolution(StringBuilder builder, MatrixProblem problem, CultureInfo culture, bool ru)
    {
        var answer = problem.MatrixAnswer ?? throw new InvalidOperationException();

        builder.AppendLine(ru
            ? "При транспонировании строки исходной матрицы становятся столбцами результата."
            : "When transposing, rows of the original matrix become columns of the result.");

        for (var row = 0; row < problem.MatrixA.Rows; row++)
        {
            for (var column = 0; column < problem.MatrixA.Columns; column++)
            {
                builder.AppendLine(
                    $"b{column + 1}{row + 1} = a{row + 1}{column + 1} = {Format(problem.MatrixA[row, column], culture)}");
            }
        }

        AppendMatrixAnswer(builder, answer, culture, ru);
    }

    private static void AppendDeterminantSolution(StringBuilder builder, MatrixProblem problem, CultureInfo culture, bool ru)
    {
        var matrix = problem.MatrixA;
        var determinant = problem.NumberAnswer ?? throw new InvalidOperationException();

        if (matrix.Rows == 2)
        {
            builder.AppendLine(ru
                ? "Для матрицы 2x2 используем формулу det(A) = a11*a22 - a12*a21."
                : "For a 2x2 matrix use det(A) = a11*a22 - a12*a21.");
            builder.AppendLine(
                $"det(A) = {Format(matrix[0, 0], culture)}*{Format(matrix[1, 1], culture)} - {Format(matrix[0, 1], culture)}*{Format(matrix[1, 0], culture)} = {Format(determinant, culture)}");
        }
        else if (matrix.Rows == 3)
        {
            builder.AppendLine(ru
                ? "Раскладываем определитель 3x3 по первой строке."
                : "Expand the 3x3 determinant along the first row.");

            var firstMinor = matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1];
            var secondMinor = matrix[1, 0] * matrix[2, 2] - matrix[1, 2] * matrix[2, 0];
            var thirdMinor = matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0];

            builder.AppendLine($"M11 = {Format(matrix[1, 1], culture)}*{Format(matrix[2, 2], culture)} - {Format(matrix[1, 2], culture)}*{Format(matrix[2, 1], culture)} = {Format(firstMinor, culture)}");
            builder.AppendLine($"M12 = {Format(matrix[1, 0], culture)}*{Format(matrix[2, 2], culture)} - {Format(matrix[1, 2], culture)}*{Format(matrix[2, 0], culture)} = {Format(secondMinor, culture)}");
            builder.AppendLine($"M13 = {Format(matrix[1, 0], culture)}*{Format(matrix[2, 1], culture)} - {Format(matrix[1, 1], culture)}*{Format(matrix[2, 0], culture)} = {Format(thirdMinor, culture)}");
            builder.AppendLine(
                $"det(A) = {Format(matrix[0, 0], culture)}*{Format(firstMinor, culture)} - {Format(matrix[0, 1], culture)}*{Format(secondMinor, culture)} + {Format(matrix[0, 2], culture)}*{Format(thirdMinor, culture)} = {Format(determinant, culture)}");
        }
        else
        {
            builder.AppendLine(ru
                ? "Используем метод Гаусса: приводим матрицу к верхнетреугольному виду и умножаем диагональные элементы."
                : "Use Gaussian elimination: transform the matrix to upper triangular form and multiply the diagonal entries.");
        }

        builder.AppendLine();
        builder.AppendLine((ru ? "Ответ: " : "Answer: ") + Format(determinant, culture));
    }

    private static void AppendInverseSolution(StringBuilder builder, MatrixProblem problem, CultureInfo culture, bool ru)
    {
        var matrix = problem.MatrixA;
        var size = matrix.Rows;
        var augmented = new double[size, size * 2];

        for (var row = 0; row < size; row++)
        {
            for (var column = 0; column < size; column++)
            {
                augmented[row, column] = matrix[row, column];
            }

            augmented[row, size + row] = 1d;
        }

        builder.AppendLine(ru
            ? "Составляем расширенную матрицу [A | I] и методом Жордана-Гаусса превращаем левую часть в единичную."
            : "Build the augmented matrix [A | I] and use Gauss-Jordan elimination to turn the left side into the identity matrix.");
        builder.AppendLine(FormatAugmented(augmented, size, culture));

        for (var pivotColumn = 0; pivotColumn < size; pivotColumn++)
        {
            var pivotRow = FindPivotRow(augmented, pivotColumn, size);
            if (Math.Abs(augmented[pivotRow, pivotColumn]) < 1e-10)
            {
                builder.AppendLine(ru ? "Обратной матрицы нет: найден нулевой ведущий элемент." : "No inverse: a zero pivot was found.");
                return;
            }

            if (pivotRow != pivotColumn)
            {
                SwapRows(augmented, pivotRow, pivotColumn);
                builder.AppendLine(ru
                    ? $"Меняем местами строки {pivotRow + 1} и {pivotColumn + 1}."
                    : $"Swap rows {pivotRow + 1} and {pivotColumn + 1}.");
                builder.AppendLine(FormatAugmented(augmented, size, culture));
            }

            var pivot = augmented[pivotColumn, pivotColumn];
            builder.AppendLine(ru
                ? $"Делим строку {pivotColumn + 1} на ведущий элемент {Format(pivot, culture)}."
                : $"Divide row {pivotColumn + 1} by pivot {Format(pivot, culture)}.");

            for (var column = 0; column < size * 2; column++)
            {
                augmented[pivotColumn, column] /= pivot;
            }

            builder.AppendLine(FormatAugmented(augmented, size, culture));

            for (var row = 0; row < size; row++)
            {
                if (row == pivotColumn)
                {
                    continue;
                }

                var factor = augmented[row, pivotColumn];
                if (Math.Abs(factor) < 1e-10)
                {
                    continue;
                }

                builder.AppendLine(ru
                    ? $"Вычитаем из строки {row + 1} строку {pivotColumn + 1}, умноженную на {Format(factor, culture)}."
                    : $"Subtract row {pivotColumn + 1} multiplied by {Format(factor, culture)} from row {row + 1}.");

                for (var column = 0; column < size * 2; column++)
                {
                    augmented[row, column] -= factor * augmented[pivotColumn, column];
                }

                builder.AppendLine(FormatAugmented(augmented, size, culture));
            }
        }

        var answer = problem.MatrixAnswer ?? throw new InvalidOperationException();
        AppendMatrixAnswer(builder, answer, culture, ru);
    }

    private static void AppendMatrixAnswer(StringBuilder builder, Matrix answer, CultureInfo culture, bool ru)
    {
        builder.AppendLine();
        builder.AppendLine(ru ? "Ответ:" : "Answer:");
        builder.AppendLine(answer.ToDisplayString(culture));
    }

    private static string FormatAugmented(double[,] matrix, int leftSize, CultureInfo culture)
    {
        var builder = new StringBuilder();
        var columns = matrix.GetLength(1);

        for (var row = 0; row < matrix.GetLength(0); row++)
        {
            builder.Append("[ ");
            for (var column = 0; column < columns; column++)
            {
                if (column == leftSize)
                {
                    builder.Append("| ");
                }

                builder.Append(Format(matrix[row, column], culture));
                if (column < columns - 1)
                {
                    builder.Append('\t');
                }
            }

            builder.AppendLine(" ]");
        }

        return builder.ToString();
    }

    private static int FindPivotRow(double[,] matrix, int pivotColumn, int size)
    {
        var pivotRow = pivotColumn;
        var bestPivot = Math.Abs(matrix[pivotRow, pivotColumn]);

        for (var row = pivotColumn + 1; row < size; row++)
        {
            var candidate = Math.Abs(matrix[row, pivotColumn]);
            if (candidate > bestPivot)
            {
                bestPivot = candidate;
                pivotRow = row;
            }
        }

        return pivotRow;
    }

    private static void SwapRows(double[,] matrix, int firstRow, int secondRow)
    {
        var columns = matrix.GetLength(1);
        for (var column = 0; column < columns; column++)
        {
            (matrix[firstRow, column], matrix[secondRow, column]) = (matrix[secondRow, column], matrix[firstRow, column]);
        }
    }

    private static string Format(double value, CultureInfo culture)
    {
        var clean = Math.Abs(value) < 1e-10 ? 0d : value;
        return clean.ToString("0.###", culture);
    }
}
