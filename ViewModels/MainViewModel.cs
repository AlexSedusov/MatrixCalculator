using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using MatrixCalculator.Models;
using MatrixCalculator.Services;

namespace MatrixCalculator.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private const double AnswerTolerance = 0.05;

    private readonly LocalizationService _localization = new();
    private readonly MatrixProblemGenerator _problemGenerator = new();
    private readonly MatrixSolutionFormatter _solutionFormatter = new();

    private DataTable _matrixATable;
    private DataTable _matrixBTable;
    private DataTable _resultTable;
    private DataTable _testMatrixATable;
    private DataTable _testMatrixBTable;
    private DataTable _answerTable;

    private int _matrixARows = 2;
    private int _matrixAColumns = 2;
    private int _matrixBRows = 2;
    private int _matrixBColumns = 2;
    private int _selectedQuestionCount = 5;
    private int _currentQuestionNumber;
    private int _correctAnswers;
    private bool _hasMatrixResult;
    private bool _isAnswered;
    private string _scalarValue = "2";
    private string _resultText = string.Empty;
    private string _statusMessage;
    private string _numericAnswer = string.Empty;
    private string _testFeedback;
    private string _solutionText = string.Empty;
    private string _currentQuestionText;
    private MatrixProblem? _currentProblem;
    private OperationOption? _selectedCalculatorOperation;
    private OperationOption? _selectedTestOperationOption;

    public MainViewModel()
    {
        DimensionOptions = Enumerable.Range(1, 5).ToArray();
        QuestionCountOptions = Enumerable.Range(1, 10).ToArray();

        _matrixATable = CreateMatrixTable(_matrixARows, _matrixAColumns, _localization.Culture);
        _matrixBTable = CreateMatrixTable(_matrixBRows, _matrixBColumns, _localization.Culture);
        _resultTable = CreateMatrixTable(1, 1, _localization.Culture);
        _testMatrixATable = CreateMatrixTable(1, 1, _localization.Culture);
        _testMatrixBTable = CreateMatrixTable(1, 1, _localization.Culture);
        _answerTable = CreateMatrixTable(1, 1, _localization.Culture);
        _statusMessage = T("StatusReady");
        _testFeedback = T("TestNotStarted");
        _currentQuestionText = T("TestNotStarted");

        CalculatorOperations = [];
        TestOperationOptions = [];
        RefreshOperationOptions();

        SetLanguageCommand = new RelayCommand(parameter => SetLanguage(Convert.ToString(parameter, CultureInfo.InvariantCulture) ?? "ru"));
        ResizeMatrixACommand = new RelayCommand(_ => ResizeMatrixA());
        ResizeMatrixBCommand = new RelayCommand(_ => ResizeMatrixB());
        CalculateCommand = new RelayCommand(_ => Calculate());
        ClearCalculatorCommand = new RelayCommand(_ => ClearCalculator());
        StartTestCommand = new RelayCommand(_ => StartTest());
        CheckAnswerCommand = new RelayCommand(_ => CheckAnswer());
        NextQuestionCommand = new RelayCommand(_ => NextQuestion());
        ShowSolutionCommand = new RelayCommand(_ => ShowSolution());
    }

    public IReadOnlyList<int> DimensionOptions { get; }

    public IReadOnlyList<int> QuestionCountOptions { get; }

    public ObservableCollection<OperationOption> CalculatorOperations { get; }

    public ObservableCollection<OperationOption> TestOperationOptions { get; }

    public RelayCommand SetLanguageCommand { get; }

    public RelayCommand ResizeMatrixACommand { get; }

    public RelayCommand ResizeMatrixBCommand { get; }

    public RelayCommand CalculateCommand { get; }

    public RelayCommand ClearCalculatorCommand { get; }

    public RelayCommand StartTestCommand { get; }

    public RelayCommand CheckAnswerCommand { get; }

    public RelayCommand NextQuestionCommand { get; }

    public RelayCommand ShowSolutionCommand { get; }

    public string WindowTitle => T("WindowTitle");

    public string LanguageMenuHeader => T("LanguageMenuHeader");

    public string RussianLabel => T("RussianLabel");

    public string EnglishLabel => T("EnglishLabel");

    public string CalculatorTabTitle => T("CalculatorTabTitle");

    public string TestTabTitle => T("TestTabTitle");

    public string OperationLabel => T("OperationLabel");

    public string MatrixAHeader => T("MatrixAHeader");

    public string MatrixBHeader => T("MatrixBHeader");

    public string RowsLabel => T("RowsLabel");

    public string ColumnsLabel => T("ColumnsLabel");

    public string ResizeLabel => T("ResizeLabel");

    public string ScalarLabel => T("ScalarLabel");

    public string CalculateLabel => T("CalculateLabel");

    public string ClearLabel => T("ClearLabel");

    public string ResultLabel => T("ResultLabel");

    public string TestSettingsHeader => T("TestSettingsHeader");

    public string QuestionCountLabel => T("QuestionCountLabel");

    public string StartTestLabel => T("StartTestLabel");

    public string QuestionLabel => T("QuestionLabel");

    public string ProblemDataLabel => T("ProblemDataLabel");

    public string UserAnswerLabel => T("UserAnswerLabel");

    public string NumberAnswerLabel => T("NumberAnswerLabel");

    public string CheckAnswerLabel => T("CheckAnswerLabel");

    public string NextQuestionLabel => T("NextQuestionLabel");

    public string ShowSolutionLabel => T("ShowSolutionLabel");

    public string SolutionLabel => T("SolutionLabel");

    public DataView MatrixAView => _matrixATable.DefaultView;

    public DataView MatrixBView => _matrixBTable.DefaultView;

    public DataView ResultView => _resultTable.DefaultView;

    public DataView TestMatrixAView => _testMatrixATable.DefaultView;

    public DataView TestMatrixBView => _testMatrixBTable.DefaultView;

    public DataView AnswerView => _answerTable.DefaultView;

    public int MatrixARows
    {
        get => _matrixARows;
        set => SetProperty(ref _matrixARows, ClampDimension(value));
    }

    public int MatrixAColumns
    {
        get => _matrixAColumns;
        set => SetProperty(ref _matrixAColumns, ClampDimension(value));
    }

    public int MatrixBRows
    {
        get => _matrixBRows;
        set => SetProperty(ref _matrixBRows, ClampDimension(value));
    }

    public int MatrixBColumns
    {
        get => _matrixBColumns;
        set => SetProperty(ref _matrixBColumns, ClampDimension(value));
    }

    public int SelectedQuestionCount
    {
        get => _selectedQuestionCount;
        set
        {
            if (SetProperty(ref _selectedQuestionCount, Math.Clamp(value, 1, 10)))
            {
                OnPropertyChanged(nameof(ScoreText));
                RefreshCurrentQuestionText();
            }
        }
    }

    public OperationOption? SelectedCalculatorOperation
    {
        get => _selectedCalculatorOperation;
        set
        {
            if (SetProperty(ref _selectedCalculatorOperation, value))
            {
                OnPropertyChanged(nameof(IsCalculatorMatrixBVisible));
                OnPropertyChanged(nameof(IsCalculatorScalarVisible));
            }
        }
    }

    public OperationOption? SelectedTestOperationOption
    {
        get => _selectedTestOperationOption;
        set => SetProperty(ref _selectedTestOperationOption, value);
    }

    public string ScalarValue
    {
        get => _scalarValue;
        set => SetProperty(ref _scalarValue, value);
    }

    public string ResultText
    {
        get => _resultText;
        private set
        {
            if (SetProperty(ref _resultText, value))
            {
                OnPropertyChanged(nameof(HasTextResult));
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string NumericAnswer
    {
        get => _numericAnswer;
        set => SetProperty(ref _numericAnswer, value);
    }

    public string TestFeedback
    {
        get => _testFeedback;
        private set => SetProperty(ref _testFeedback, value);
    }

    public string SolutionText
    {
        get => _solutionText;
        private set => SetProperty(ref _solutionText, value);
    }

    public string CurrentQuestionText
    {
        get => _currentQuestionText;
        private set => SetProperty(ref _currentQuestionText, value);
    }

    public bool HasMatrixResult
    {
        get => _hasMatrixResult;
        private set
        {
            if (SetProperty(ref _hasMatrixResult, value))
            {
                OnPropertyChanged(nameof(HasTextResult));
            }
        }
    }

    public bool HasTextResult => !string.IsNullOrWhiteSpace(ResultText);

    public bool IsCalculatorMatrixBVisible => SelectedCalculatorOperation?.Operation is MatrixOperation.Add or MatrixOperation.Subtract or MatrixOperation.MultiplyMatrix;

    public bool IsCalculatorScalarVisible => SelectedCalculatorOperation?.Operation == MatrixOperation.MultiplyScalar;

    public bool HasTestMatrixB => _currentProblem?.MatrixB is not null;

    public bool HasTestScalar => _currentProblem?.Scalar is not null;

    public string TestScalarValueText => _currentProblem?.Scalar is null ? string.Empty : FormatNumber(_currentProblem.Scalar.Value);

    public bool IsMatrixAnswerExpected => _currentProblem?.ExpectsMatrix == true;

    public bool IsNumericAnswerExpected => _currentProblem is not null && !_currentProblem.ExpectsMatrix;

    public string ScoreText => $"{T("ScoreCaption")}: {_correctAnswers}/{SelectedQuestionCount}";

    private void SetLanguage(string language)
    {
        _localization.SetLanguage(language);
        RefreshOperationOptions();
        RefreshCurrentQuestionText();

        if (_currentProblem is not null && !string.IsNullOrWhiteSpace(SolutionText))
        {
            SolutionText = _solutionFormatter.Build(_currentProblem, _localization.Language);
        }

        OnAllPropertiesChanged();
    }

    private void RefreshOperationOptions()
    {
        var calculatorOperation = SelectedCalculatorOperation?.Operation ?? MatrixOperation.Add;
        var testOperation = SelectedTestOperationOption?.Operation;

        CalculatorOperations.Clear();
        foreach (var operation in Enum.GetValues<MatrixOperation>())
        {
            CalculatorOperations.Add(new OperationOption(operation, _localization.OperationName(operation)));
        }

        TestOperationOptions.Clear();
        TestOperationOptions.Add(new OperationOption(null, T("AllOperations")));
        foreach (var operation in Enum.GetValues<MatrixOperation>())
        {
            TestOperationOptions.Add(new OperationOption(operation, _localization.OperationName(operation)));
        }

        SelectedCalculatorOperation = CalculatorOperations.First(option => option.Operation == calculatorOperation);
        SelectedTestOperationOption = TestOperationOptions.First(option => option.Operation == testOperation);
    }

    private void ResizeMatrixA()
    {
        _matrixATable = ResizeMatrixTable(_matrixATable, MatrixARows, MatrixAColumns, _localization.Culture);
        OnPropertyChanged(nameof(MatrixAView));
    }

    private void ResizeMatrixB()
    {
        _matrixBTable = ResizeMatrixTable(_matrixBTable, MatrixBRows, MatrixBColumns, _localization.Culture);
        OnPropertyChanged(nameof(MatrixBView));
    }

    private void ClearCalculator()
    {
        _matrixATable = CreateMatrixTable(MatrixARows, MatrixAColumns, _localization.Culture);
        _matrixBTable = CreateMatrixTable(MatrixBRows, MatrixBColumns, _localization.Culture);
        _resultTable = CreateMatrixTable(1, 1, _localization.Culture);
        ResultText = string.Empty;
        HasMatrixResult = false;
        StatusMessage = T("StatusReady");

        OnPropertyChanged(nameof(MatrixAView));
        OnPropertyChanged(nameof(MatrixBView));
        OnPropertyChanged(nameof(ResultView));
    }

    private void Calculate()
    {
        try
        {
            var operation = SelectedCalculatorOperation?.Operation ?? MatrixOperation.Add;
            var matrixA = ReadMatrix(_matrixATable, "A");

            switch (operation)
            {
                case MatrixOperation.Add:
                    SetMatrixResult(matrixA.Add(ReadMatrix(_matrixBTable, "B")));
                    break;
                case MatrixOperation.Subtract:
                    SetMatrixResult(matrixA.Subtract(ReadMatrix(_matrixBTable, "B")));
                    break;
                case MatrixOperation.MultiplyMatrix:
                    SetMatrixResult(matrixA.Multiply(ReadMatrix(_matrixBTable, "B")));
                    break;
                case MatrixOperation.MultiplyScalar:
                    SetMatrixResult(matrixA.Multiply(ParseNumberOrThrow(ScalarValue, ScalarLabel)));
                    break;
                case MatrixOperation.Transpose:
                    SetMatrixResult(matrixA.Transpose());
                    break;
                case MatrixOperation.Determinant:
                    SetTextResult(FormatNumber(matrixA.Determinant()));
                    break;
                case MatrixOperation.Inverse:
                    SetMatrixResult(matrixA.Inverse());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            StatusMessage = T("CalculationCompleted");
        }
        catch (Exception ex)
        {
            StatusMessage = string.Format(_localization.Culture, T("CalculationError"), ex.Message);
        }
    }

    private void SetMatrixResult(Matrix matrix)
    {
        _resultTable = MatrixToTable(matrix, _localization.Culture);
        ResultText = string.Empty;
        HasMatrixResult = true;
        OnPropertyChanged(nameof(ResultView));
    }

    private void SetTextResult(string text)
    {
        ResultText = text;
        HasMatrixResult = false;
    }

    private void StartTest()
    {
        _correctAnswers = 0;
        _currentQuestionNumber = 1;
        LoadProblem();
        OnPropertyChanged(nameof(ScoreText));
    }

    private void LoadProblem()
    {
        _currentProblem = _problemGenerator.Generate(SelectedTestOperationOption?.Operation);
        _isAnswered = false;
        NumericAnswer = string.Empty;
        TestFeedback = _currentProblem.ExpectsMatrix ? T("EnterMatrixAnswer") : T("EnterNumberAnswer");
        SolutionText = string.Empty;
        _testMatrixATable = MatrixToTable(_currentProblem.MatrixA, _localization.Culture);
        _testMatrixBTable = _currentProblem.MatrixB is null
            ? CreateMatrixTable(1, 1, _localization.Culture)
            : MatrixToTable(_currentProblem.MatrixB, _localization.Culture);
        _answerTable = _currentProblem.MatrixAnswer is null
            ? CreateMatrixTable(1, 1, _localization.Culture)
            : CreateMatrixTable(_currentProblem.MatrixAnswer.Rows, _currentProblem.MatrixAnswer.Columns, _localization.Culture);

        RefreshCurrentQuestionText();
        OnProblemPropertiesChanged();
    }

    private void CheckAnswer()
    {
        if (_currentProblem is null)
        {
            TestFeedback = T("NoCurrentProblem");
            return;
        }

        if (_isAnswered)
        {
            return;
        }

        try
        {
            string expectedText;
            bool isCorrect;

            if (_currentProblem.ExpectsMatrix)
            {
                var expected = _currentProblem.MatrixAnswer ?? throw new InvalidOperationException();
                var userAnswer = ReadMatrix(_answerTable, UserAnswerLabel);
                isCorrect = userAnswer.AlmostEquals(expected, AnswerTolerance);
                expectedText = Environment.NewLine + expected.ToDisplayString(_localization.Culture);
            }
            else
            {
                var expected = _currentProblem.NumberAnswer ?? throw new InvalidOperationException();
                var userAnswer = ParseNumberOrThrow(NumericAnswer, NumberAnswerLabel);
                isCorrect = Math.Abs(userAnswer - expected) <= AnswerTolerance;
                expectedText = FormatNumber(expected);
            }

            if (isCorrect)
            {
                _correctAnswers++;
                TestFeedback = T("CorrectAnswer");
            }
            else
            {
                TestFeedback = string.Format(_localization.Culture, T("WrongAnswer"), expectedText);
            }

            _isAnswered = true;
            OnPropertyChanged(nameof(ScoreText));
        }
        catch (Exception ex)
        {
            TestFeedback = string.Format(_localization.Culture, T("AnswerFormatError"), ex.Message);
        }
    }

    private void NextQuestion()
    {
        if (_currentProblem is null)
        {
            TestFeedback = T("NoCurrentProblem");
            return;
        }

        if (!_isAnswered)
        {
            TestFeedback = T("CheckBeforeNext");
            return;
        }

        if (_currentQuestionNumber >= SelectedQuestionCount)
        {
            CurrentQuestionText = string.Format(_localization.Culture, T("TestFinished"), _correctAnswers, SelectedQuestionCount);
            TestFeedback = CurrentQuestionText;
            SolutionText = string.Empty;
            return;
        }

        _currentQuestionNumber++;
        LoadProblem();
    }

    private void ShowSolution()
    {
        if (_currentProblem is null)
        {
            SolutionText = T("NoCurrentProblem");
            return;
        }

        SolutionText = _solutionFormatter.Build(_currentProblem, _localization.Language);
    }

    private void RefreshCurrentQuestionText()
    {
        if (_currentProblem is null)
        {
            CurrentQuestionText = T("TestNotStarted");
            return;
        }

        var operationName = _localization.OperationName(_currentProblem.Operation);
        var title = string.Format(_localization.Culture, T("QuestionTitle"), _currentQuestionNumber, SelectedQuestionCount, operationName);
        var instruction = BuildInstruction(_currentProblem);
        CurrentQuestionText = $"{title}{Environment.NewLine}{instruction}";
    }

    private string BuildInstruction(MatrixProblem problem)
    {
        var key = $"Instruction.{problem.Operation}";
        return problem.Scalar is null
            ? T(key)
            : string.Format(_localization.Culture, T(key), FormatNumber(problem.Scalar.Value));
    }

    private void OnProblemPropertiesChanged()
    {
        OnPropertyChanged(nameof(TestMatrixAView));
        OnPropertyChanged(nameof(TestMatrixBView));
        OnPropertyChanged(nameof(AnswerView));
        OnPropertyChanged(nameof(HasTestMatrixB));
        OnPropertyChanged(nameof(HasTestScalar));
        OnPropertyChanged(nameof(TestScalarValueText));
        OnPropertyChanged(nameof(IsMatrixAnswerExpected));
        OnPropertyChanged(nameof(IsNumericAnswerExpected));
    }

    private Matrix ReadMatrix(DataTable table, string matrixName)
    {
        var matrix = new Matrix(table.Rows.Count, table.Columns.Count);

        for (var row = 0; row < table.Rows.Count; row++)
        {
            for (var column = 0; column < table.Columns.Count; column++)
            {
                var rawValue = Convert.ToString(table.Rows[row][column], CultureInfo.CurrentCulture) ?? string.Empty;
                if (!TryParseNumber(rawValue, out var value))
                {
                    throw new FormatException($"{matrixName}[{row + 1},{column + 1}] = \"{rawValue}\"");
                }

                matrix[row, column] = value;
            }
        }

        return matrix;
    }

    private double ParseNumberOrThrow(string text, string fieldName)
    {
        if (TryParseNumber(text, out var value))
        {
            return value;
        }

        throw new FormatException($"{fieldName}: \"{text}\"");
    }

    private bool TryParseNumber(string text, out double value)
    {
        var normalized = (text ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            value = 0d;
            return true;
        }

        return double.TryParse(normalized, NumberStyles.Float, _localization.Culture, out value)
            || double.TryParse(normalized, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
            || double.TryParse(normalized.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static DataTable CreateMatrixTable(int rows, int columns, CultureInfo culture)
    {
        var table = CreateEmptyTable(columns);

        for (var row = 0; row < rows; row++)
        {
            var dataRow = table.NewRow();
            for (var column = 0; column < columns; column++)
            {
                dataRow[column] = 0d.ToString("0.###", culture);
            }

            table.Rows.Add(dataRow);
        }

        return table;
    }

    private static DataTable MatrixToTable(Matrix matrix, CultureInfo culture)
    {
        var table = CreateEmptyTable(matrix.Columns);

        for (var row = 0; row < matrix.Rows; row++)
        {
            var dataRow = table.NewRow();
            for (var column = 0; column < matrix.Columns; column++)
            {
                dataRow[column] = matrix[row, column].ToString("0.###", culture);
            }

            table.Rows.Add(dataRow);
        }

        return table;
    }

    private static DataTable ResizeMatrixTable(DataTable oldTable, int rows, int columns, CultureInfo culture)
    {
        var table = CreateMatrixTable(rows, columns, culture);
        var rowsToCopy = Math.Min(rows, oldTable.Rows.Count);
        var columnsToCopy = Math.Min(columns, oldTable.Columns.Count);

        for (var row = 0; row < rowsToCopy; row++)
        {
            for (var column = 0; column < columnsToCopy; column++)
            {
                table.Rows[row][column] = oldTable.Rows[row][column];
            }
        }

        return table;
    }

    private static DataTable CreateEmptyTable(int columns)
    {
        var table = new DataTable();
        for (var column = 0; column < columns; column++)
        {
            table.Columns.Add($"C{column + 1}", typeof(string));
        }

        return table;
    }

    private string FormatNumber(double value)
    {
        var clean = Math.Abs(value) < 1e-10 ? 0d : value;
        return clean.ToString("0.###", _localization.Culture);
    }

    private string T(string key) => _localization.T(key);

    private static int ClampDimension(int value) => Math.Clamp(value, 1, 5);
}
