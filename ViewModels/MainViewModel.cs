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
    private List<VisualStep> _visualSteps = [];
    private MatrixVisualViewModel _visualMatrixA = CreatePlaceholderVisual("A");
    private MatrixVisualViewModel _visualMatrixB = CreatePlaceholderVisual("B");
    private MatrixVisualViewModel _visualResultMatrix = CreatePlaceholderVisual("?");

    private int _matrixARows = 2;
    private int _matrixAColumns = 2;
    private int _matrixBRows = 2;
    private int _matrixBColumns = 2;
    private int _selectedQuestionCount = 5;
    private int _currentQuestionNumber;
    private int _correctAnswers;
    private int _currentVisualStepIndex;
    private bool _hasMatrixResult;
    private bool _isAnswered;
    private bool _showAllVisualSteps;
    private string _scalarValue = "2";
    private string _resultText = string.Empty;
    private string _statusMessage;
    private string _numericAnswer = string.Empty;
    private string _testFeedback;
    private string _solutionText = string.Empty;
    private string _currentQuestionText;
    private string _visualStepTitle = string.Empty;
    private string _visualStepDescription = string.Empty;
    private string _visualStepFormula = string.Empty;
    private string _visualRuleText = string.Empty;
    private string _visualHintText = string.Empty;
    private string _visualProgressText = string.Empty;
    private double _visualProgressValue;
    private MatrixProblem? _currentProblem;
    private MatrixProblem? _visualProblem;
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
        _visualStepTitle = T("CurrentStepEmpty");
        _visualStepDescription = T("StartTestForSteps");
        _visualStepFormula = string.Empty;
        _visualRuleText = T("RulePlaceholder");
        _visualHintText = T("HintPlaceholder");
        _visualProgressText = string.Format(_localization.Culture, T("StepProgressFormat"), 0, 0, 0);

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
        ResetVisualStepCommand = new RelayCommand(_ => ResetVisualStep());
        PreviousVisualStepCommand = new RelayCommand(_ => PreviousVisualStep());
        NextVisualStepCommand = new RelayCommand(_ => NextVisualStep());
        ShowAllVisualStepsCommand = new RelayCommand(_ => ShowAllVisualSteps());
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

    public RelayCommand ResetVisualStepCommand { get; }

    public RelayCommand PreviousVisualStepCommand { get; }

    public RelayCommand NextVisualStepCommand { get; }

    public RelayCommand ShowAllVisualStepsCommand { get; }

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

    public string CurrentStepLabel => T("CurrentStepLabel");

    public string TipsAndRulesLabel => T("TipsAndRulesLabel");

    public string RuleLabel => T("RuleLabel");

    public string HintLabel => T("HintLabel");

    public string FormulaLabel => T("FormulaLabel");

    public string ResetStepLabel => T("ResetStepLabel");

    public string PreviousStepLabel => T("PreviousStepLabel");

    public string NextStepLabel => T("NextStepLabel");

    public string ShowAllLabel => T("ShowAllLabel");

    public string MatrixOperationBoardTitle => T("MatrixOperationBoardTitle");

    public bool HasActiveVisualStep => _currentVisualStepIndex >= 0 && _visualProblem is not null;

    public DataView MatrixAView => _matrixATable.DefaultView;

    public DataView MatrixBView => _matrixBTable.DefaultView;

    public DataView ResultView => _resultTable.DefaultView;

    public DataView TestMatrixAView => _testMatrixATable.DefaultView;

    public DataView TestMatrixBView => _testMatrixBTable.DefaultView;

    public DataView AnswerView => _answerTable.DefaultView;

    public MatrixVisualViewModel VisualMatrixA
    {
        get => _visualMatrixA;
        private set => SetProperty(ref _visualMatrixA, value);
    }

    public MatrixVisualViewModel VisualMatrixB
    {
        get => _visualMatrixB;
        private set => SetProperty(ref _visualMatrixB, value);
    }

    public MatrixVisualViewModel VisualResultMatrix
    {
        get => _visualResultMatrix;
        private set => SetProperty(ref _visualResultMatrix, value);
    }

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

    public string VisualStepTitle
    {
        get => _visualStepTitle;
        private set => SetProperty(ref _visualStepTitle, value);
    }

    public string VisualStepDescription
    {
        get => _visualStepDescription;
        private set => SetProperty(ref _visualStepDescription, value);
    }

    public string VisualStepFormula
    {
        get => _visualStepFormula;
        private set => SetProperty(ref _visualStepFormula, value);
    }

    public string VisualRuleText
    {
        get => _visualRuleText;
        private set => SetProperty(ref _visualRuleText, value);
    }

    public string VisualHintText
    {
        get => _visualHintText;
        private set => SetProperty(ref _visualHintText, value);
    }

    public string VisualProgressText
    {
        get => _visualProgressText;
        private set => SetProperty(ref _visualProgressText, value);
    }

    public double VisualProgressValue
    {
        get => _visualProgressValue;
        private set => SetProperty(ref _visualProgressValue, value);
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

    public bool HasVisualMatrixB => _visualProblem?.MatrixB is not null;

    public bool HasVisualScalar => _visualProblem?.Scalar is not null;

    public string VisualScalarValueText => _visualProblem?.Scalar is null ? string.Empty : FormatNumber(_visualProblem.Scalar.Value);

    public string VisualOperationSymbol => _visualProblem?.Operation switch
    {
        MatrixOperation.Add => "+",
        MatrixOperation.Subtract => "-",
        MatrixOperation.MultiplyMatrix => "x",
        MatrixOperation.MultiplyScalar => "x",
        MatrixOperation.Transpose => "T",
        MatrixOperation.Determinant => "det",
        MatrixOperation.Inverse => "-1",
        _ => "+"
    };

    public string TestScalarValueText => _currentProblem?.Scalar is null ? string.Empty : FormatNumber(_currentProblem.Scalar.Value);

    public bool IsMatrixAnswerExpected => _currentProblem?.ExpectsMatrix == true;

    public bool IsNumericAnswerExpected => _currentProblem is not null && !_currentProblem.ExpectsMatrix;

    public string ScoreText => $"{T("ScoreCaption")}: {_correctAnswers}/{SelectedQuestionCount}";

    private void SetLanguage(string language)
    {
        _localization.SetLanguage(language);
        RefreshOperationOptions();
        RefreshCurrentQuestionText();

        if (_visualProblem is not null && !string.IsNullOrWhiteSpace(SolutionText))
        {
            SolutionText = _solutionFormatter.Build(_visualProblem, _localization.Language);
        }

        if (_visualProblem is not null)
        {
            _visualSteps = BuildVisualSteps(_visualProblem);
            UpdateVisualStep();
        }
        else
        {
            VisualStepTitle = T("CurrentStepEmpty");
            VisualStepDescription = T("StartTestForSteps");
            VisualRuleText = T("RulePlaceholder");
            VisualHintText = T("HintPlaceholder");
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
        ClearVisualProblem();

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
            var problem = operation switch
            {
                MatrixOperation.Add => MatrixProblem.Create(operation, matrixA, ReadMatrix(_matrixBTable, "B")),
                MatrixOperation.Subtract => MatrixProblem.Create(operation, matrixA, ReadMatrix(_matrixBTable, "B")),
                MatrixOperation.MultiplyMatrix => MatrixProblem.Create(operation, matrixA, ReadMatrix(_matrixBTable, "B")),
                MatrixOperation.MultiplyScalar => MatrixProblem.Create(operation, matrixA, scalar: ParseNumberOrThrow(ScalarValue, ScalarLabel)),
                MatrixOperation.Transpose => MatrixProblem.Create(operation, matrixA),
                MatrixOperation.Determinant => MatrixProblem.Create(operation, matrixA),
                MatrixOperation.Inverse => MatrixProblem.Create(operation, matrixA),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (problem.MatrixAnswer is not null)
            {
                SetMatrixResult(problem.MatrixAnswer);
            }
            else
            {
                SetTextResult(FormatNumber(problem.NumberAnswer ?? 0d));
            }

            LoadVisualProblem(problem);

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
        LoadVisualProblem(_currentProblem);

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
        var solutionProblem = _visualProblem ?? _currentProblem;
        if (solutionProblem is null)
        {
            SolutionText = T("NoCurrentProblem");
            return;
        }

        SolutionText = _solutionFormatter.Build(solutionProblem, _localization.Language);
    }

    private void ResetVisualStep()
    {
        if (_visualProblem is null)
        {
            VisualStepDescription = T("StartTestForSteps");
            return;
        }

        _currentVisualStepIndex = -1;
        _showAllVisualSteps = false;
        UpdateVisualStep();
    }

    private void PreviousVisualStep()
    {
        if (_visualProblem is null || _visualSteps.Count == 0)
        {
            return;
        }

        _showAllVisualSteps = false;
        _currentVisualStepIndex = Math.Max(-1, _currentVisualStepIndex - 1);
        UpdateVisualStep();
    }

    private void NextVisualStep()
    {
        if (_visualProblem is null || _visualSteps.Count == 0)
        {
            return;
        }

        _showAllVisualSteps = false;
        _currentVisualStepIndex = Math.Min(_visualSteps.Count - 1, _currentVisualStepIndex + 1);
        UpdateVisualStep();
    }

    private void ShowAllVisualSteps()
    {
        if (_visualProblem is null || _visualSteps.Count == 0)
        {
            return;
        }

        _showAllVisualSteps = true;
        _currentVisualStepIndex = _visualSteps.Count - 1;
        UpdateVisualStep();
    }

    private void UpdateVisualStep()
    {
        if (_visualProblem is null || _visualSteps.Count == 0)
        {
            VisualMatrixA = CreatePlaceholderVisual("A");
            VisualMatrixB = CreatePlaceholderVisual("B");
            VisualResultMatrix = CreatePlaceholderVisual("?");
            VisualStepTitle = T("CurrentStepEmpty");
            VisualStepDescription = T("StartTestForSteps");
            VisualStepFormula = string.Empty;
            VisualRuleText = T("RulePlaceholder");
            VisualHintText = T("HintPlaceholder");
            VisualProgressText = string.Format(_localization.Culture, T("StepProgressFormat"), 0, 0, 0);
            VisualProgressValue = 0;
            OnVisualPropertiesChanged();
            return;
        }

        if (_currentVisualStepIndex < 0 && !_showAllVisualSteps)
        {
            VisualMatrixA = BuildVisualMatrix(BuildMatrixTitle(MatrixAHeader, _visualProblem.MatrixA), _visualProblem.MatrixA, []);
            VisualMatrixB = _visualProblem.MatrixB is null
                ? CreatePlaceholderVisual("B")
                : BuildVisualMatrix(BuildMatrixTitle(MatrixBHeader, _visualProblem.MatrixB), _visualProblem.MatrixB, []);
            VisualResultMatrix = BuildResultVisualMatrix(_visualProblem, [], []);
            VisualStepTitle = T("VisualReadyTitle");
            VisualStepDescription = T("VisualReadyDescription");
            VisualStepFormula = string.Empty;
            VisualRuleText = BuildRuleText(_visualProblem.Operation);
            VisualHintText = BuildHintText(_visualProblem.Operation);
            VisualProgressText = string.Format(_localization.Culture, T("StepProgressFormat"), 0, 0, _visualSteps.Count);
            VisualProgressValue = 0;
            OnVisualPropertiesChanged();
            return;
        }

        var step = _visualSteps[Math.Clamp(_currentVisualStepIndex, 0, _visualSteps.Count - 1)];
        var completedResultCells = _visualSteps
            .Take(_showAllVisualSteps ? _visualSteps.Count : _currentVisualStepIndex + 1)
            .SelectMany(item => item.ResultCells)
            .Distinct()
            .ToArray();
        var completedSteps = _showAllVisualSteps ? _visualSteps.Count : _currentVisualStepIndex + 1;
        var progress = (int)Math.Round(completedSteps * 100d / _visualSteps.Count);

        VisualMatrixA = BuildVisualMatrix(
            BuildMatrixTitle(MatrixAHeader, _visualProblem.MatrixA),
            _visualProblem.MatrixA,
            step.HighlightA);

        VisualMatrixB = _visualProblem.MatrixB is null
            ? CreatePlaceholderVisual("B")
            : BuildVisualMatrix(BuildMatrixTitle(MatrixBHeader, _visualProblem.MatrixB), _visualProblem.MatrixB, step.HighlightB);

        VisualResultMatrix = BuildResultVisualMatrix(_visualProblem, step.HighlightResult, completedResultCells);
        VisualStepTitle = step.Title;
        VisualStepDescription = step.Description;
        VisualStepFormula = step.Formula;
        VisualRuleText = BuildRuleText(_visualProblem.Operation);
        VisualHintText = BuildHintText(_visualProblem.Operation);
        VisualProgressText = string.Format(_localization.Culture, T("StepProgressFormat"), progress, completedSteps, _visualSteps.Count);
        VisualProgressValue = progress;

        OnVisualPropertiesChanged();
    }

    private void LoadVisualProblem(MatrixProblem problem)
    {
        _visualProblem = problem;
        _visualSteps = BuildVisualSteps(problem);
        _currentVisualStepIndex = -1;
        _showAllVisualSteps = false;
        SolutionText = string.Empty;
        UpdateVisualStep();
    }

    private void ClearVisualProblem()
    {
        _visualProblem = null;
        _visualSteps = [];
        _currentVisualStepIndex = -1;
        _showAllVisualSteps = false;
        SolutionText = string.Empty;
        UpdateVisualStep();
    }

    private void OnVisualPropertiesChanged()
    {
        OnPropertyChanged(nameof(HasActiveVisualStep));
        OnPropertyChanged(nameof(HasVisualMatrixB));
        OnPropertyChanged(nameof(HasVisualScalar));
        OnPropertyChanged(nameof(VisualScalarValueText));
        OnPropertyChanged(nameof(VisualOperationSymbol));
    }

    private List<VisualStep> BuildVisualSteps(MatrixProblem problem)
    {
        return problem.Operation switch
        {
            MatrixOperation.Add => BuildElementwiseSteps(problem, "+"),
            MatrixOperation.Subtract => BuildElementwiseSteps(problem, "-"),
            MatrixOperation.MultiplyMatrix => BuildMatrixMultiplicationSteps(problem),
            MatrixOperation.MultiplyScalar => BuildScalarSteps(problem),
            MatrixOperation.Transpose => BuildTransposeSteps(problem),
            MatrixOperation.Determinant => BuildSingleResultStep(problem),
            MatrixOperation.Inverse => BuildInverseSteps(problem),
            _ => []
        };
    }

    private List<VisualStep> BuildElementwiseSteps(MatrixProblem problem, string sign)
    {
        var matrixB = problem.MatrixB ?? throw new InvalidOperationException();
        var answer = problem.MatrixAnswer ?? throw new InvalidOperationException();
        var steps = new List<VisualStep>();
        var total = answer.Rows * answer.Columns;

        for (var row = 0; row < answer.Rows; row++)
        {
            for (var column = 0; column < answer.Columns; column++)
            {
                var stepNumber = steps.Count + 1;
                var position = new CellPosition(row, column);
                var operationName = _localization.OperationName(problem.Operation);
                var description = IsRussian()
                    ? $"Шаг {stepNumber} из {total}: берем элементы [{row + 1},{column + 1}] из матриц A и B."
                    : $"Step {stepNumber} of {total}: take entries [{row + 1},{column + 1}] from matrices A and B.";
                var formula = $"A[{row + 1},{column + 1}] {sign} B[{row + 1},{column + 1}] = {FormatNumber(problem.MatrixA[row, column])} {sign} {FormatNumber(matrixB[row, column])} = {FormatNumber(answer[row, column])}";

                steps.Add(new VisualStep(
                    BuildStepTitle(operationName, row, column),
                    description,
                    formula,
                    [position],
                    [position],
                    [position],
                    [position]));
            }
        }

        return steps;
    }

    private List<VisualStep> BuildMatrixMultiplicationSteps(MatrixProblem problem)
    {
        var matrixB = problem.MatrixB ?? throw new InvalidOperationException();
        var answer = problem.MatrixAnswer ?? throw new InvalidOperationException();
        var steps = new List<VisualStep>();
        var total = answer.Rows * answer.Columns;

        for (var row = 0; row < answer.Rows; row++)
        {
            for (var column = 0; column < answer.Columns; column++)
            {
                var stepNumber = steps.Count + 1;
                var products = new List<string>();
                for (var index = 0; index < problem.MatrixA.Columns; index++)
                {
                    products.Add($"{FormatNumber(problem.MatrixA[row, index])}*{FormatNumber(matrixB[index, column])}");
                }

                var description = IsRussian()
                    ? $"Шаг {stepNumber} из {total}: умножаем строку {row + 1} матрицы A на столбец {column + 1} матрицы B."
                    : $"Step {stepNumber} of {total}: multiply row {row + 1} of A by column {column + 1} of B.";
                var resultCell = new CellPosition(row, column);

                steps.Add(new VisualStep(
                    BuildStepTitle(_localization.OperationName(problem.Operation), row, column),
                    description,
                    $"C[{row + 1},{column + 1}] = {string.Join(" + ", products)} = {FormatNumber(answer[row, column])}",
                    RowPositions(row, problem.MatrixA.Columns),
                    ColumnPositions(column, matrixB.Rows),
                    [resultCell],
                    [resultCell]));
            }
        }

        return steps;
    }

    private List<VisualStep> BuildScalarSteps(MatrixProblem problem)
    {
        var scalar = problem.Scalar ?? throw new InvalidOperationException();
        var answer = problem.MatrixAnswer ?? throw new InvalidOperationException();
        var steps = new List<VisualStep>();
        var total = answer.Rows * answer.Columns;

        for (var row = 0; row < answer.Rows; row++)
        {
            for (var column = 0; column < answer.Columns; column++)
            {
                var position = new CellPosition(row, column);
                var stepNumber = steps.Count + 1;
                var description = IsRussian()
                    ? $"Шаг {stepNumber} из {total}: умножаем элемент A[{row + 1},{column + 1}] на число {FormatNumber(scalar)}."
                    : $"Step {stepNumber} of {total}: multiply A[{row + 1},{column + 1}] by {FormatNumber(scalar)}.";

                steps.Add(new VisualStep(
                    BuildStepTitle(_localization.OperationName(problem.Operation), row, column),
                    description,
                    $"A[{row + 1},{column + 1}] * {FormatNumber(scalar)} = {FormatNumber(problem.MatrixA[row, column])} * {FormatNumber(scalar)} = {FormatNumber(answer[row, column])}",
                    [position],
                    [],
                    [position],
                    [position]));
            }
        }

        return steps;
    }

    private List<VisualStep> BuildTransposeSteps(MatrixProblem problem)
    {
        var answer = problem.MatrixAnswer ?? throw new InvalidOperationException();
        var steps = new List<VisualStep>();
        var total = problem.MatrixA.Rows * problem.MatrixA.Columns;

        for (var row = 0; row < problem.MatrixA.Rows; row++)
        {
            for (var column = 0; column < problem.MatrixA.Columns; column++)
            {
                var source = new CellPosition(row, column);
                var target = new CellPosition(column, row);
                var stepNumber = steps.Count + 1;
                var description = IsRussian()
                    ? $"Шаг {stepNumber} из {total}: переносим элемент из строки {row + 1}, столбца {column + 1} в строку {column + 1}, столбец {row + 1}."
                    : $"Step {stepNumber} of {total}: move row {row + 1}, column {column + 1} to row {column + 1}, column {row + 1}.";

                steps.Add(new VisualStep(
                    BuildStepTitle(_localization.OperationName(problem.Operation), column, row),
                    description,
                    $"B[{column + 1},{row + 1}] = A[{row + 1},{column + 1}] = {FormatNumber(answer[column, row])}",
                    [source],
                    [],
                    [target],
                    [target]));
            }
        }

        return steps;
    }

    private List<VisualStep> BuildSingleResultStep(MatrixProblem problem)
    {
        var result = problem.NumberAnswer ?? 0d;
        var description = IsRussian()
            ? "Определитель вычисляется по формуле для квадратной матрицы. Для больших матриц используется приведение к треугольному виду."
            : "The determinant is computed for a square matrix. Larger matrices use triangular reduction.";

        return
        [
            new VisualStep(
                T("CurrentStepDeterminant"),
                description,
                $"det(A) = {FormatNumber(result)}",
                AllPositions(problem.MatrixA.Rows, problem.MatrixA.Columns),
                [],
                [new CellPosition(0, 0)],
                [new CellPosition(0, 0)])
        ];
    }

    private List<VisualStep> BuildInverseSteps(MatrixProblem problem)
    {
        var answer = problem.MatrixAnswer ?? throw new InvalidOperationException();
        var steps = new List<VisualStep>();
        var total = answer.Rows * answer.Columns;

        for (var row = 0; row < answer.Rows; row++)
        {
            for (var column = 0; column < answer.Columns; column++)
            {
                var position = new CellPosition(row, column);
                var stepNumber = steps.Count + 1;
                var description = IsRussian()
                    ? $"Шаг {stepNumber} из {total}: берем элемент обратной матрицы после преобразования [A | I]."
                    : $"Step {stepNumber} of {total}: take the inverse entry after transforming [A | I].";

                steps.Add(new VisualStep(
                    BuildStepTitle(_localization.OperationName(problem.Operation), row, column),
                    description,
                    $"A^(-1)[{row + 1},{column + 1}] = {FormatNumber(answer[row, column])}",
                    AllPositions(problem.MatrixA.Rows, problem.MatrixA.Columns),
                    [],
                    [position],
                    [position]));
            }
        }

        return steps;
    }

    private MatrixVisualViewModel BuildVisualMatrix(string title, Matrix matrix, IReadOnlyCollection<CellPosition> highlightedCells)
    {
        var cells = new List<MatrixCellViewModel>();
        for (var row = 0; row < matrix.Rows; row++)
        {
            for (var column = 0; column < matrix.Columns; column++)
            {
                cells.Add(new MatrixCellViewModel(
                    FormatNumber(matrix[row, column]),
                    highlightedCells.Contains(new CellPosition(row, column)),
                    false));
            }
        }

        return new MatrixVisualViewModel(title, matrix.Rows, matrix.Columns, cells);
    }

    private MatrixVisualViewModel BuildResultVisualMatrix(
        MatrixProblem problem,
        IReadOnlyCollection<CellPosition> highlightedCells,
        IReadOnlyCollection<CellPosition> completedCells)
    {
        if (problem.MatrixAnswer is null)
        {
            var isKnown = completedCells.Contains(new CellPosition(0, 0));
            var text = isKnown ? FormatNumber(problem.NumberAnswer ?? 0d) : "?";
            return new MatrixVisualViewModel(
                $"{ResultLabel} (1x1)",
                1,
                1,
                [new MatrixCellViewModel(text, highlightedCells.Contains(new CellPosition(0, 0)), !isKnown)]);
        }

        var answer = problem.MatrixAnswer;
        var cells = new List<MatrixCellViewModel>();

        for (var row = 0; row < answer.Rows; row++)
        {
            for (var column = 0; column < answer.Columns; column++)
            {
                var position = new CellPosition(row, column);
                var isKnown = completedCells.Contains(position);
                cells.Add(new MatrixCellViewModel(
                    isKnown ? FormatNumber(answer[row, column]) : "?",
                    highlightedCells.Contains(position),
                    !isKnown));
            }
        }

        return new MatrixVisualViewModel(BuildMatrixTitle(ResultLabel, answer), answer.Rows, answer.Columns, cells);
    }

    private string BuildStepTitle(string operationName, int row, int column)
    {
        return IsRussian()
            ? $"Текущий шаг: {operationName} элементов [{row + 1},{column + 1}]"
            : $"Current step: {operationName} for entries [{row + 1},{column + 1}]";
    }

    private string BuildMatrixTitle(string title, Matrix matrix)
    {
        return $"{title} ({matrix.Rows}x{matrix.Columns})";
    }

    private string BuildRuleText(MatrixOperation operation)
    {
        var ru = IsRussian();
        return operation switch
        {
            MatrixOperation.Add => ru ? "Сложение матриц возможно только для матриц одинакового размера." : "Matrix addition is possible only for matrices of the same size.",
            MatrixOperation.Subtract => ru ? "Вычитание матриц выполняется поэлементно для одинаковых размеров." : "Matrix subtraction is element-wise and requires equal sizes.",
            MatrixOperation.MultiplyMatrix => ru ? "Число столбцов первой матрицы должно равняться числу строк второй." : "The first matrix column count must equal the second matrix row count.",
            MatrixOperation.MultiplyScalar => ru ? "При умножении на число каждый элемент матрицы умножается на этот множитель." : "Scalar multiplication multiplies every entry by the same number.",
            MatrixOperation.Transpose => ru ? "При транспонировании строки становятся столбцами." : "When transposing, rows become columns.",
            MatrixOperation.Determinant => ru ? "Определитель существует только у квадратной матрицы." : "A determinant exists only for a square matrix.",
            MatrixOperation.Inverse => ru ? "Обратная матрица существует только для квадратной матрицы с ненулевым определителем." : "An inverse exists only for a square matrix with a non-zero determinant.",
            _ => string.Empty
        };
    }

    private string BuildHintText(MatrixOperation operation)
    {
        var ru = IsRussian();
        return operation switch
        {
            MatrixOperation.Add => ru ? "Результат находится путем поэлементного суммирования." : "The result is found by adding matching entries.",
            MatrixOperation.Subtract => ru ? "Следите за знаком: из элемента A вычитается соответствующий элемент B." : "Watch the sign: subtract the matching B entry from A.",
            MatrixOperation.MultiplyMatrix => ru ? "Считайте одно число результата как скалярное произведение строки и столбца." : "Compute one result entry as a row-column dot product.",
            MatrixOperation.MultiplyScalar => ru ? "Можно идти по строкам слева направо, чтобы не пропустить элементы." : "Move row by row from left to right to avoid missing entries.",
            MatrixOperation.Transpose => ru ? "Индекс [i,j] превращается в [j,i]." : "Index [i,j] becomes [j,i].",
            MatrixOperation.Determinant => ru ? "Для 2x2 используйте a11*a22 - a12*a21." : "For 2x2 use a11*a22 - a12*a21.",
            MatrixOperation.Inverse => ru ? "Удобно работать с расширенной матрицей [A | I]." : "It is convenient to work with the augmented matrix [A | I].",
            _ => string.Empty
        };
    }

    private static IReadOnlyList<CellPosition> RowPositions(int row, int columns)
    {
        return Enumerable.Range(0, columns).Select(column => new CellPosition(row, column)).ToArray();
    }

    private static IReadOnlyList<CellPosition> ColumnPositions(int column, int rows)
    {
        return Enumerable.Range(0, rows).Select(row => new CellPosition(row, column)).ToArray();
    }

    private static IReadOnlyList<CellPosition> AllPositions(int rows, int columns)
    {
        return Enumerable.Range(0, rows)
            .SelectMany(row => Enumerable.Range(0, columns).Select(column => new CellPosition(row, column)))
            .ToArray();
    }

    private static MatrixVisualViewModel CreatePlaceholderVisual(string title)
    {
        return new MatrixVisualViewModel(title, 1, 1, [new MatrixCellViewModel("?", false, true)]);
    }

    private bool IsRussian() => _localization.Language == "ru";

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
        OnVisualPropertiesChanged();
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

    private readonly record struct CellPosition(int Row, int Column);

    private sealed record VisualStep(
        string Title,
        string Description,
        string Formula,
        IReadOnlyList<CellPosition> HighlightA,
        IReadOnlyList<CellPosition> HighlightB,
        IReadOnlyList<CellPosition> HighlightResult,
        IReadOnlyList<CellPosition> ResultCells);
}
