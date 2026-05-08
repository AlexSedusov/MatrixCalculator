using System.Globalization;

namespace MatrixCalculator.Services;

public sealed class LocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _resources = new()
    {
        ["ru"] = new Dictionary<string, string>
        {
            ["WindowTitle"] = "Матричный калькулятор",
            ["LanguageMenuHeader"] = "Язык",
            ["RussianLabel"] = "Русский",
            ["EnglishLabel"] = "English",
            ["CalculatorTabTitle"] = "Калькулятор",
            ["TestTabTitle"] = "Тестирование",
            ["OperationLabel"] = "Операция",
            ["MatrixAHeader"] = "Матрица A",
            ["MatrixBHeader"] = "Матрица B",
            ["RowsLabel"] = "Строк",
            ["ColumnsLabel"] = "Столбцов",
            ["ResizeLabel"] = "Изменить размер",
            ["ScalarLabel"] = "Число",
            ["CalculateLabel"] = "Вычислить",
            ["ClearLabel"] = "Очистить",
            ["ResultLabel"] = "Результат",
            ["StatusReady"] = "Готово. Заполните матрицы и выберите операцию.",
            ["CalculationCompleted"] = "Вычисление выполнено.",
            ["CalculationError"] = "Ошибка вычисления: {0}",
            ["TestSettingsHeader"] = "Настройки теста",
            ["QuestionCountLabel"] = "Количество вопросов",
            ["StartTestLabel"] = "Начать тест",
            ["EndTestLabel"] = "Завершить тест",
            ["QuestionLabel"] = "Задание",
            ["ProblemDataLabel"] = "Данные задания",
            ["UserAnswerLabel"] = "Ваш ответ",
            ["NumberAnswerLabel"] = "Числовой ответ",
            ["CheckAnswerLabel"] = "Проверить",
            ["NextQuestionLabel"] = "Следующий вопрос",
            ["ShowSolutionLabel"] = "Показать решение",
            ["SolutionLabel"] = "Пошаговое решение",
            ["CurrentStepLabel"] = "Текущий шаг",
            ["TipsAndRulesLabel"] = "Советы и правила",
            ["RuleLabel"] = "Правило:",
            ["HintLabel"] = "Подсказка:",
            ["FormulaLabel"] = "Формула:",
            ["ResetStepLabel"] = "Сброс",
            ["PreviousStepLabel"] = "Предыдущий шаг",
            ["NextStepLabel"] = "Следующий шаг",
            ["ShowAllLabel"] = "Показать всё",
            ["MatrixOperationBoardTitle"] = "Визуальное решение",
            ["CurrentStepEmpty"] = "Текущий шаг: нет активного задания",
            ["StartTestForSteps"] = "Начните тест, чтобы увидеть пошаговую визуализацию.",
            ["VisualReadyTitle"] = "Пошаговое решение готово",
            ["VisualReadyDescription"] = "Нажмите «Следующий шаг», чтобы начать. Результирующая матрица пока пустая.",
            ["RulePlaceholder"] = "Правило появится после генерации задания.",
            ["HintPlaceholder"] = "Подсказка появится вместе с текущим шагом.",
            ["StepProgressFormat"] = "Завершено {0}% (шаг {1} из {2})",
            ["CurrentStepDeterminant"] = "Текущий шаг: вычисление определителя",
            ["ScoreCaption"] = "Счет",
            ["AllOperations"] = "Все сразу",
            ["CorrectAnswer"] = "Верно!",
            ["WrongAnswer"] = "Неверно. Правильный ответ: {0}",
            ["AnswerFormatError"] = "Не удалось прочитать ответ: {0}",
            ["CheckBeforeNext"] = "Сначала проверьте ответ.",
            ["TestNotStarted"] = "Выберите настройки и начните тест.",
            ["QuestionTitle"] = "Вопрос {0} из {1}: {2}",
            ["TestFinished"] = "Тест завершен. Итог: {0} из {1}.",
            ["EnterMatrixAnswer"] = "Введите матрицу-ответ.",
            ["EnterNumberAnswer"] = "Введите число.",
            ["NoCurrentProblem"] = "Нет активного задания.",
            ["Operation.Add"] = "Сложение",
            ["Operation.Subtract"] = "Вычитание",
            ["Operation.MultiplyMatrix"] = "Умножение матриц",
            ["Operation.MultiplyScalar"] = "Умножение на число",
            ["Operation.Transpose"] = "Транспонирование",
            ["Operation.Determinant"] = "Определитель",
            ["Operation.Inverse"] = "Обратная матрица",
            ["Instruction.Add"] = "Найдите A + B.",
            ["Instruction.Subtract"] = "Найдите A - B.",
            ["Instruction.MultiplyMatrix"] = "Найдите A x B.",
            ["Instruction.MultiplyScalar"] = "Умножьте A на число {0}.",
            ["Instruction.Transpose"] = "Найдите транспонированную матрицу A.",
            ["Instruction.Determinant"] = "Найдите det(A).",
            ["Instruction.Inverse"] = "Найдите A^(-1)."
        },
        ["en"] = new Dictionary<string, string>
        {
            ["WindowTitle"] = "Matrix Calculator",
            ["LanguageMenuHeader"] = "Language",
            ["RussianLabel"] = "Русский",
            ["EnglishLabel"] = "English",
            ["CalculatorTabTitle"] = "Calculator",
            ["TestTabTitle"] = "Practice test",
            ["OperationLabel"] = "Operation",
            ["MatrixAHeader"] = "Matrix A",
            ["MatrixBHeader"] = "Matrix B",
            ["RowsLabel"] = "Rows",
            ["ColumnsLabel"] = "Columns",
            ["ResizeLabel"] = "Resize",
            ["ScalarLabel"] = "Scalar",
            ["CalculateLabel"] = "Calculate",
            ["ClearLabel"] = "Clear",
            ["ResultLabel"] = "Result",
            ["StatusReady"] = "Ready. Fill the matrices and choose an operation.",
            ["CalculationCompleted"] = "Calculation completed.",
            ["CalculationError"] = "Calculation error: {0}",
            ["TestSettingsHeader"] = "Test settings",
            ["QuestionCountLabel"] = "Number of questions",
            ["StartTestLabel"] = "Start test",
            ["EndTestLabel"] = "Finish test",
            ["QuestionLabel"] = "Question",
            ["ProblemDataLabel"] = "Problem data",
            ["UserAnswerLabel"] = "Your answer",
            ["NumberAnswerLabel"] = "Numeric answer",
            ["CheckAnswerLabel"] = "Check",
            ["NextQuestionLabel"] = "Next question",
            ["ShowSolutionLabel"] = "Show solution",
            ["SolutionLabel"] = "Step-by-step solution",
            ["CurrentStepLabel"] = "Current step",
            ["TipsAndRulesLabel"] = "Tips and rules",
            ["RuleLabel"] = "Rule:",
            ["HintLabel"] = "Hint:",
            ["FormulaLabel"] = "Formula:",
            ["ResetStepLabel"] = "Reset",
            ["PreviousStepLabel"] = "Previous step",
            ["NextStepLabel"] = "Next step",
            ["ShowAllLabel"] = "Show all",
            ["MatrixOperationBoardTitle"] = "Visual solution",
            ["CurrentStepEmpty"] = "Current step: no active problem",
            ["StartTestForSteps"] = "Start a test to see the step-by-step visualization.",
            ["VisualReadyTitle"] = "Step-by-step solution is ready",
            ["VisualReadyDescription"] = "Press Next step to begin. The result matrix is empty for now.",
            ["RulePlaceholder"] = "The rule will appear after a problem is generated.",
            ["HintPlaceholder"] = "The hint will appear with the current step.",
            ["StepProgressFormat"] = "Completed {0}% (step {1} of {2})",
            ["CurrentStepDeterminant"] = "Current step: determinant calculation",
            ["ScoreCaption"] = "Score",
            ["AllOperations"] = "All operations",
            ["CorrectAnswer"] = "Correct!",
            ["WrongAnswer"] = "Incorrect. Correct answer: {0}",
            ["AnswerFormatError"] = "Could not read the answer: {0}",
            ["CheckBeforeNext"] = "Check the answer first.",
            ["TestNotStarted"] = "Choose settings and start the test.",
            ["QuestionTitle"] = "Question {0} of {1}: {2}",
            ["TestFinished"] = "Test finished. Final score: {0} of {1}.",
            ["EnterMatrixAnswer"] = "Enter the answer matrix.",
            ["EnterNumberAnswer"] = "Enter a number.",
            ["NoCurrentProblem"] = "There is no active problem.",
            ["Operation.Add"] = "Addition",
            ["Operation.Subtract"] = "Subtraction",
            ["Operation.MultiplyMatrix"] = "Matrix multiplication",
            ["Operation.MultiplyScalar"] = "Scalar multiplication",
            ["Operation.Transpose"] = "Transpose",
            ["Operation.Determinant"] = "Determinant",
            ["Operation.Inverse"] = "Inverse matrix",
            ["Instruction.Add"] = "Find A + B.",
            ["Instruction.Subtract"] = "Find A - B.",
            ["Instruction.MultiplyMatrix"] = "Find A x B.",
            ["Instruction.MultiplyScalar"] = "Multiply A by {0}.",
            ["Instruction.Transpose"] = "Find the transpose of A.",
            ["Instruction.Determinant"] = "Find det(A).",
            ["Instruction.Inverse"] = "Find A^(-1)."
        }
    };

    public string Language { get; private set; } = "ru";

    public CultureInfo Culture => Language == "ru" ? CultureInfo.GetCultureInfo("ru-RU") : CultureInfo.GetCultureInfo("en-US");

    public void SetLanguage(string language)
    {
        Language = _resources.ContainsKey(language) ? language : "ru";
    }

    public string T(string key)
    {
        if (_resources[Language].TryGetValue(key, out var value))
        {
            return value;
        }

        return _resources["ru"].TryGetValue(key, out var fallback) ? fallback : key;
    }

    public string OperationName(MatrixOperation operation)
    {
        return T($"Operation.{operation}");
    }
}
