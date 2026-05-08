using System.Globalization;
using System.Text;
using System;

namespace MatrixCalculator.Models;

public sealed class Matrix
{
    private const double Epsilon = 1e-10;
    private readonly double[,] _values;

    public Matrix(int rows, int columns)
    {
        if (rows <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Row count must be positive.");
        }

        if (columns <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(columns), "Column count must be positive.");
        }

        _values = new double[rows, columns];
    }

    public Matrix(double[,] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var rows = values.GetLength(0);
        var columns = values.GetLength(1);
        if (rows == 0 || columns == 0)
        {
            throw new ArgumentException("Matrix dimensions must be positive.", nameof(values));
        }

        _values = new double[rows, columns];
        Array.Copy(values, _values, values.Length);
    }

    public int Rows => _values.GetLength(0);

    public int Columns => _values.GetLength(1);

    public double this[int row, int column]
    {
        get => _values[row, column];
        set => _values[row, column] = value;
    }

    public Matrix Add(Matrix other)
    {
        EnsureSameSize(other);
        var result = new Matrix(Rows, Columns);

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                result[row, column] = Clean(_values[row, column] + other[row, column]);
            }
        }

        return result;
    }

    public Matrix Subtract(Matrix other)
    {
        EnsureSameSize(other);
        var result = new Matrix(Rows, Columns);

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                result[row, column] = Clean(_values[row, column] - other[row, column]);
            }
        }

        return result;
    }

    public Matrix Multiply(Matrix other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Columns != other.Rows)
        {
            throw new InvalidOperationException("For multiplication, the number of columns in the first matrix must equal the number of rows in the second matrix.");
        }

        var result = new Matrix(Rows, other.Columns);

        // Каждый элемент результата равен скалярному произведению строки первой матрицы и столбца второй.
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < other.Columns; column++)
            {
                var sum = 0d;
                for (var index = 0; index < Columns; index++)
                {
                    sum += _values[row, index] * other[index, column];
                }

                result[row, column] = Clean(sum);
            }
        }

        return result;
    }

    public Matrix Multiply(double scalar)
    {
        var result = new Matrix(Rows, Columns);

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                result[row, column] = Clean(_values[row, column] * scalar);
            }
        }

        return result;
    }

    public Matrix Transpose()
    {
        var result = new Matrix(Columns, Rows);

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                result[column, row] = _values[row, column];
            }
        }

        return result;
    }

    public double Determinant()
    {
        EnsureSquare();

        var size = Rows;
        var data = ToArray();
        var sign = 1d;

        // Приводим матрицу к верхнетреугольному виду методом Гаусса.
        // Определитель треугольной матрицы равен произведению элементов главной диагонали.
        for (var pivotColumn = 0; pivotColumn < size; pivotColumn++)
        {
            var pivotRow = pivotColumn;
            var bestPivot = Math.Abs(data[pivotRow, pivotColumn]);

            for (var row = pivotColumn + 1; row < size; row++)
            {
                var candidate = Math.Abs(data[row, pivotColumn]);
                if (candidate > bestPivot)
                {
                    bestPivot = candidate;
                    pivotRow = row;
                }
            }

            if (bestPivot < Epsilon)
            {
                return 0d;
            }

            if (pivotRow != pivotColumn)
            {
                SwapRows(data, pivotRow, pivotColumn);
                sign *= -1d;
            }

            var pivot = data[pivotColumn, pivotColumn];
            for (var row = pivotColumn + 1; row < size; row++)
            {
                var factor = data[row, pivotColumn] / pivot;
                for (var column = pivotColumn; column < size; column++)
                {
                    data[row, column] -= factor * data[pivotColumn, column];
                }
            }
        }

        var determinant = sign;
        for (var index = 0; index < size; index++)
        {
            determinant *= data[index, index];
        }

        return Clean(determinant);
    }

    public Matrix Inverse()
    {
        EnsureSquare();

        var size = Rows;
        var augmented = new double[size, size * 2];

        // Формируем расширенную матрицу [A | I], затем методом Жордана-Гаусса превращаем левую часть в I.
        // Правая часть после преобразований становится обратной матрицей A^(-1).
        for (var row = 0; row < size; row++)
        {
            for (var column = 0; column < size; column++)
            {
                augmented[row, column] = _values[row, column];
            }

            augmented[row, size + row] = 1d;
        }

        for (var pivotColumn = 0; pivotColumn < size; pivotColumn++)
        {
            var pivotRow = FindPivotRow(augmented, pivotColumn, size);
            if (Math.Abs(augmented[pivotRow, pivotColumn]) < Epsilon)
            {
                throw new InvalidOperationException("The matrix is singular and does not have an inverse.");
            }

            if (pivotRow != pivotColumn)
            {
                SwapRows(augmented, pivotRow, pivotColumn);
            }

            var pivot = augmented[pivotColumn, pivotColumn];
            for (var column = 0; column < size * 2; column++)
            {
                augmented[pivotColumn, column] /= pivot;
            }

            for (var row = 0; row < size; row++)
            {
                if (row == pivotColumn)
                {
                    continue;
                }

                var factor = augmented[row, pivotColumn];
                for (var column = 0; column < size * 2; column++)
                {
                    augmented[row, column] -= factor * augmented[pivotColumn, column];
                }
            }
        }

        var inverse = new Matrix(size, size);
        for (var row = 0; row < size; row++)
        {
            for (var column = 0; column < size; column++)
            {
                inverse[row, column] = Clean(augmented[row, size + column]);
            }
        }

        return inverse;
    }

    public bool AlmostEquals(Matrix other, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Rows != other.Rows || Columns != other.Columns)
        {
            return false;
        }

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                if (Math.Abs(_values[row, column] - other[row, column]) > tolerance)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public double[,] ToArray()
    {
        var clone = new double[Rows, Columns];
        Array.Copy(_values, clone, _values.Length);
        return clone;
    }

    public string ToDisplayString(CultureInfo culture)
    {
        var builder = new StringBuilder();

        for (var row = 0; row < Rows; row++)
        {
            builder.Append("[ ");
            for (var column = 0; column < Columns; column++)
            {
                builder.Append(_values[row, column].ToString("0.###", culture));
                if (column < Columns - 1)
                {
                    builder.Append('\t');
                }
            }

            builder.Append(" ]");
            if (row < Rows - 1)
            {
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    public static Matrix CreateIdentity(int size)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Matrix size must be positive.");
        }

        var identity = new Matrix(size, size);
        for (var index = 0; index < size; index++)
        {
            identity[index, index] = 1d;
        }

        return identity;
    }

    private static double Clean(double value) => Math.Abs(value) < Epsilon ? 0d : value;

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
        if (firstRow == secondRow)
        {
            return;
        }

        var columnCount = matrix.GetLength(1);
        for (var column = 0; column < columnCount; column++)
        {
            (matrix[firstRow, column], matrix[secondRow, column]) = (matrix[secondRow, column], matrix[firstRow, column]);
        }
    }

    private void EnsureSameSize(Matrix other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Rows != other.Rows || Columns != other.Columns)
        {
            throw new InvalidOperationException("Matrices must have the same size.");
        }
    }

    private void EnsureSquare()
    {
        if (Rows != Columns)
        {
            throw new InvalidOperationException("The operation is available only for square matrices.");
        }
    }
}
