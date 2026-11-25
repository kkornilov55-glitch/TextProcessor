using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;
using TextProcessor.Logic.AbstractSyntaxTree;
using TextProcessor.Logic.Loops;

namespace UI.ViewModels
{
    public sealed class LoopCell : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Action? ValueChanged { get; set; }

        private int value;
        private bool isProcessed;
        private bool isCurrent;
        private bool isLastProcessed;

        public LoopCell(int index, int value)
        {
            this.Index = index;
            this.value = value;
        }

        public int Index { get; }

        public int Value
        {
            get => this.value;
            set
            {
                if (this.value == value)
                    return;

                this.value = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                this.ValueChanged?.Invoke();
            }
        }

        public string StatusText => this.IsCurrent
            ? "текущий j"
            : this.IsLastProcessed
                ? "изменён"
                : this.IsProcessed
                    ? "обработано"
                    : "ожидание";

        public bool IsProcessed
        {
            get => this.isProcessed;
            set
            {
                this.isProcessed = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsProcessed)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
            }
        }

        public bool IsCurrent
        {
            get => this.isCurrent;
            set
            {
                this.isCurrent = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCurrent)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
            }
        }

        public bool IsLastProcessed
        {
            get => this.isLastProcessed;
            set
            {
                this.isLastProcessed = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLastProcessed)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
            }
        }
    }

    public sealed class LoopViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly DispatcherTimer runTimer = new() { Interval = TimeSpan.FromMilliseconds(350) };
        private readonly Random random = new();

        private LoopPreset selectedPreset = LoopPreset.All[0];
        private LoopSession? session;
        private LoopState? lastBeforeState;
        private LoopStep? lastStep;
        private int threshold;
        private bool isRunning;
        private bool canStep;
        private bool canRun;
        private bool canStop;

        private string arrayLengthText = "8";
        private string minValueText = "-5";
        private string maxValueText = "15";
        private string thresholdText = "3";
        private string invariantWords = string.Empty;
        private string invariantFormula = string.Empty;
        private string invariantHuman = string.Empty;
        private string invariantParseError = string.Empty;
        private string statusMessage = string.Empty;

        private string preText = string.Empty;
        private string postFormulaText = string.Empty;
        private string postWordsText = string.Empty;
        private string variantText = string.Empty;
        private string stateText = string.Empty;
        private string currentIndexText = "—";
        private string stepCountText = "0";
        private string lastStepText = "Шаг ещё не выполнялся";
        private string variantHistoryText = string.Empty;
        private string bodyCodeText = string.Empty;

        private string preColor = "Gray";
        private string postColor = "Gray";
        private string postText = "ОЖИДАНИЕ ПРОВЕРКИ";
        private string invariantBeforeColor = "Gray";
        private string invariantAfterColor = "Gray";
        private string invariantBeforeText = "ОЖИДАНИЕ ШАГА";
        private string invariantAfterText = "ОЖИДАНИЕ ШАГА";

        private int variantValue;
        private int variantMaximum = 1;

        private string verificationCondition = string.Empty;
        private string simplifiedCondition = string.Empty;
        private string verificationHuman = string.Empty;
        private string wpHumanText = string.Empty;
        private string wpTraceText = string.Empty;
        private string preservationColor = "Gray";
        private string preservationText = "ОЖИДАНИЕ ПРОВЕРКИ";

        private string initializationStatus = "ОЖИДАНИЕ ПРОВЕРКИ";
        private string preservationStatus = "ОЖИДАНИЕ ПРОВЕРКИ";
        private string exitStatus = "ОЖИДАНИЕ ПРОВЕРКИ";
        private string terminationStatus = "ОЖИДАНИЕ ПРОВЕРКИ";
        private string initializationColor = "Gray";
        private string preservationStatusColor = "Gray";
        private string exitColor = "Gray";
        private string terminationColor = "Gray";

        public LoopViewModel()
        {
            this.runTimer.Tick += this.RunTimer_Tick;
            this.LoadPresetDefaults();
            this.GenerateArray();
        }

        public IReadOnlyList<LoopPreset> Presets => LoopPreset.All;

        public ObservableCollection<LoopCell> Cells { get; } = [];

        public LoopPreset SelectedPreset
        {
            get => this.selectedPreset;
            set
            {
                if (ReferenceEquals(this.selectedPreset, value))
                    return;

                this.selectedPreset = value;
                this.Notify(nameof(SelectedPreset));
                this.LoadPresetDefaults();
                this.ResetSession();
            }
        }

        public string ArrayLengthText
        {
            get => this.arrayLengthText;
            set { this.arrayLengthText = value; this.Notify(nameof(ArrayLengthText)); }
        }

        public string MinValueText
        {
            get => this.minValueText;
            set { this.minValueText = value; this.Notify(nameof(MinValueText)); }
        }

        public string MaxValueText
        {
            get => this.maxValueText;
            set { this.maxValueText = value; this.Notify(nameof(MaxValueText)); }
        }

        public string ThresholdText
        {
            get => this.thresholdText;
            set { this.thresholdText = value; this.Notify(nameof(ThresholdText)); }
        }

        public string InvariantWords
        {
            get => this.invariantWords;
            set { this.invariantWords = value; this.Notify(nameof(InvariantWords)); }
        }

        public string InvariantFormula
        {
            get => this.invariantFormula;
            set
            {
                this.invariantFormula = value;
                this.Notify(nameof(InvariantFormula));
                this.RefreshInvariantIndicators();
                this.VerifyWp();
                this.RefreshCorrectnessConditions();
            }
        }

        public string InvariantHuman
        {
            get => this.invariantHuman;
            private set { this.invariantHuman = value; this.Notify(nameof(InvariantHuman)); }
        }

        public string InvariantParseError
        {
            get => this.invariantParseError;
            private set { this.invariantParseError = value; this.Notify(nameof(InvariantParseError)); }
        }

        public string StatusMessage
        {
            get => this.statusMessage;
            private set { this.statusMessage = value; this.Notify(nameof(StatusMessage)); }
        }

        public string PreText
        {
            get => this.preText;
            private set { this.preText = value; this.Notify(nameof(PreText)); }
        }

        public string PostFormulaText
        {
            get => this.postFormulaText;
            private set { this.postFormulaText = value; this.Notify(nameof(PostFormulaText)); }
        }

        public string PostWordsText
        {
            get => this.postWordsText;
            private set { this.postWordsText = value; this.Notify(nameof(PostWordsText)); }
        }

        public string VariantText
        {
            get => this.variantText;
            private set { this.variantText = value; this.Notify(nameof(VariantText)); }
        }

        public string GuardText => LoopPreset.GuardCode;

        public string BodyCodeText
        {
            get => this.bodyCodeText;
            private set { this.bodyCodeText = value; this.Notify(nameof(BodyCodeText)); }
        }

        public string StateText
        {
            get => this.stateText;
            private set { this.stateText = value; this.Notify(nameof(StateText)); }
        }

        public string CurrentIndexText
        {
            get => this.currentIndexText;
            private set { this.currentIndexText = value; this.Notify(nameof(CurrentIndexText)); }
        }

        public string StepCountText
        {
            get => this.stepCountText;
            private set { this.stepCountText = value; this.Notify(nameof(StepCountText)); }
        }

        public string LastStepText
        {
            get => this.lastStepText;
            private set { this.lastStepText = value; this.Notify(nameof(LastStepText)); }
        }

        public string VariantHistoryText
        {
            get => this.variantHistoryText;
            private set { this.variantHistoryText = value; this.Notify(nameof(VariantHistoryText)); }
        }

        public int VariantValue
        {
            get => this.variantValue;
            private set { this.variantValue = value; this.Notify(nameof(VariantValue)); }
        }

        public int VariantMaximum
        {
            get => this.variantMaximum;
            private set { this.variantMaximum = Math.Max(1, value); this.Notify(nameof(VariantMaximum)); }
        }

        public string PreColor
        {
            get => this.preColor;
            private set { this.preColor = value; this.Notify(nameof(PreColor)); }
        }

        public string PostColor
        {
            get => this.postColor;
            private set { this.postColor = value; this.Notify(nameof(PostColor)); }
        }

        public string PostText
        {
            get => this.postText;
            private set { this.postText = value; this.Notify(nameof(PostText)); }
        }

        public string InvariantBeforeColor
        {
            get => this.invariantBeforeColor;
            private set { this.invariantBeforeColor = value; this.Notify(nameof(InvariantBeforeColor)); }
        }

        public string InvariantAfterColor
        {
            get => this.invariantAfterColor;
            private set { this.invariantAfterColor = value; this.Notify(nameof(InvariantAfterColor)); }
        }

        public string InvariantBeforeText
        {
            get => this.invariantBeforeText;
            private set { this.invariantBeforeText = value; this.Notify(nameof(InvariantBeforeText)); }
        }

        public string InvariantAfterText
        {
            get => this.invariantAfterText;
            private set { this.invariantAfterText = value; this.Notify(nameof(InvariantAfterText)); }
        }

        public string VerificationCondition
        {
            get => this.verificationCondition;
            private set { this.verificationCondition = value; this.Notify(nameof(VerificationCondition)); }
        }

        public string SimplifiedCondition
        {
            get => this.simplifiedCondition;
            private set { this.simplifiedCondition = value; this.Notify(nameof(SimplifiedCondition)); }
        }

        public string VerificationHuman
        {
            get => this.verificationHuman;
            private set { this.verificationHuman = value; this.Notify(nameof(VerificationHuman)); }
        }

        public string WpHumanText
        {
            get => this.wpHumanText;
            private set { this.wpHumanText = value; this.Notify(nameof(WpHumanText)); }
        }

        public string WpTraceText
        {
            get => this.wpTraceText;
            private set { this.wpTraceText = value; this.Notify(nameof(WpTraceText)); }
        }

        public string PreservationColor
        {
            get => this.preservationColor;
            private set { this.preservationColor = value; this.Notify(nameof(PreservationColor)); }
        }

        public string PreservationText
        {
            get => this.preservationText;
            private set { this.preservationText = value; this.Notify(nameof(PreservationText)); }
        }

        public string InitializationStatus
        {
            get => this.initializationStatus;
            private set { this.initializationStatus = value; this.Notify(nameof(InitializationStatus)); }
        }

        public string PreservationStatus
        {
            get => this.preservationStatus;
            private set { this.preservationStatus = value; this.Notify(nameof(PreservationStatus)); }
        }

        public string ExitStatus
        {
            get => this.exitStatus;
            private set { this.exitStatus = value; this.Notify(nameof(ExitStatus)); }
        }

        public string TerminationStatus
        {
            get => this.terminationStatus;
            private set { this.terminationStatus = value; this.Notify(nameof(TerminationStatus)); }
        }

        public string InitializationColor
        {
            get => this.initializationColor;
            private set { this.initializationColor = value; this.Notify(nameof(InitializationColor)); }
        }

        public string PreservationStatusColor
        {
            get => this.preservationStatusColor;
            private set { this.preservationStatusColor = value; this.Notify(nameof(PreservationStatusColor)); }
        }

        public string ExitColor
        {
            get => this.exitColor;
            private set { this.exitColor = value; this.Notify(nameof(ExitColor)); }
        }

        public string TerminationColor
        {
            get => this.terminationColor;
            private set { this.terminationColor = value; this.Notify(nameof(TerminationColor)); }
        }

        public bool IsRunning
        {
            get => this.isRunning;
            private set { this.isRunning = value; this.Notify(nameof(IsRunning)); }
        }

        public bool CanStep
        {
            get => this.canStep;
            private set { this.canStep = value; this.Notify(nameof(CanStep)); }
        }

        public bool CanRun
        {
            get => this.canRun;
            private set { this.canRun = value; this.Notify(nameof(CanRun)); }
        }

        public bool CanStop
        {
            get => this.canStop;
            private set { this.canStop = value; this.Notify(nameof(CanStop)); }
        }

        public void GenerateArray()
        {
            this.StopRun();

            if (!int.TryParse(this.arrayLengthText, out int length) || length < 1 || length > 40)
            {
                this.StatusMessage = "Длина массива должна быть целым числом от 1 до 40.";
                return;
            }

            if (!int.TryParse(this.minValueText, out int min) || !int.TryParse(this.maxValueText, out int max) || min > max)
            {
                this.StatusMessage = "Диапазон значений задан некорректно.";
                return;
            }

            if (!int.TryParse(this.thresholdText, out int parsedThreshold))
            {
                this.StatusMessage = "Порог T должен быть целым числом.";
                return;
            }

            this.threshold = parsedThreshold;
            this.Cells.Clear();
            for (int i = 0; i < length; i++)
            {
                var cell = new LoopCell(i, this.random.Next(min, max + 1));
                cell.ValueChanged += this.OnCellValueChanged;
                this.Cells.Add(cell);
            }

            this.StatusMessage = $"Сгенерирован массив из {length} элементов, T = {this.threshold}.";
            this.ResetSession();
        }

        public void StepOnce()
        {
            if (this.session is null || this.session.IsCompleted)
                return;

            if (!this.TryParseInvariant(out var invariant) || invariant is null)
                return;

            LoopState before = this.session.State;
            Debug.Assert(this.EvaluateInvariant(invariant, before), "Инвариант нарушен перед шагом цикла.");

            try
            {
                var step = this.session.Step();
                if (step is null)
                    return;

                LoopState after = step.Value.After;
                Debug.Assert(this.EvaluateInvariant(invariant, after), "Инвариант нарушен после шага цикла.");
                Debug.WriteLine($"[LOOP] {this.selectedPreset.Mode}: t {before.Variant} → {after.Variant}, j = {after.Index}, res = {after.Result}");

                this.lastBeforeState = before;
                this.lastStep = step;
                this.StatusMessage = $"Шаг {this.session.Steps.Count}: t {before.Variant} → {after.Variant} (t' < t).";
            }
            catch (Exception ex)
            {
                this.StopRun();
                this.StatusMessage = "Ошибка исполнения цикла: " + ex.Message;
            }

            this.Refresh();
        }

        public void RunToEnd()
        {
            if (this.session is null || this.session.IsCompleted || this.IsRunning)
                return;

            this.IsRunning = true;
            this.RefreshButtons();
            this.runTimer.Start();
        }

        public void StopRun()
        {
            if (this.runTimer.IsEnabled)
                this.runTimer.Stop();

            if (this.IsRunning)
            {
                this.IsRunning = false;
                this.StatusMessage = "Исполнение остановлено.";
            }

            this.RefreshButtons();
        }

        public void ResetSession()
        {
            this.StopRun();
            this.lastBeforeState = null;
            this.lastStep = null;

            if (this.Cells.Count == 0)
            {
                this.session = null;
                this.StatusMessage = "Сначала сгенерируйте массив.";
                this.Refresh();
                return;
            }

            try
            {
                this.session = new LoopSession(this.selectedPreset.Mode, this.Cells.Select(cell => cell.Value), this.threshold);
                this.StatusMessage = "Сессия цикла сброшена. Инвариант выполняется до первого шага.";
            }
            catch (Exception ex)
            {
                this.session = null;
                this.StatusMessage = "Не удалось создать сессию: " + ex.Message;
            }

            this.Refresh();
        }

        private void RunTimer_Tick(object? sender, EventArgs e)
        {
            this.StepOnce();
            if (this.session is null || this.session.IsCompleted || this.session.Steps.Count >= this.Cells.Count + 1)
                this.StopRun();
            else
                this.RefreshButtons();
        }

        private void OnCellValueChanged()
        {
            this.StatusMessage = "Значение ячейки изменено — исполнение начато заново.";
            this.ResetSession();
        }

        private void LoadPresetDefaults()
        {
            this.BodyCodeText = this.selectedPreset.BodyCode;

            switch (this.selectedPreset.Mode)
            {
                case LoopMode.PrefixSum:
                    this.InvariantWords = "res равен сумме элементов префикса a[0..j−1], при этом 0 ≤ j ≤ n и res ≥ 0.";
                    this.InvariantFormula = "res >= 0 && j <= n";
                    this.PreText = "n ≥ 0; до цикла res = 0, j = 0.";
                    this.PostFormulaText = "res == Sum(a, n)";
                    this.PostWordsText = "res равен сумме всех элементов массива.";
                    this.VariantText = "t = n − j";
                    break;
                case LoopMode.CountAboveThreshold:
                    this.InvariantWords = "res равен числу элементов префикса a[0..j−1], больших T; при этом 0 ≤ res ≤ j ≤ n.";
                    this.InvariantFormula = "res >= 0 && res <= j";
                    this.PreText = "n ≥ 0; до цикла res = 0, j = 0, порог T задан.";
                    this.PostFormulaText = "res == CountAbove(a, n, T)";
                    this.PostWordsText = "res равен числу всех элементов массива, строго больших порога T.";
                    this.VariantText = "t = n − j";
                    break;
                case LoopMode.PrefixMax:
                    this.InvariantWords = "res равен максимуму обработанного префикса a[0..j−1]; текущий элемент не больше res.";
                    this.InvariantFormula = "res >= currentElement";
                    this.PreText = "n ≥ 1; до цикла res = a[0], j = 1.";
                    this.PostFormulaText = "res == Max(a, n)";
                    this.PostWordsText = "res равен максимальному элементу всего массива.";
                    this.VariantText = "t = n − j";
                    break;
            }
        }

        private void Refresh()
        {
            if (this.session is null)
            {
                this.StateText = "Сессия не создана";
                this.CurrentIndexText = "—";
                this.StepCountText = "0";
                this.LastStepText = "Шаг ещё не выполнялся";
                this.VariantHistoryText = string.Empty;
                this.VariantValue = 0;
                this.VariantMaximum = 1;
                this.PostColor = "Gray";
                this.PostText = "ОЖИДАНИЕ ПРОВЕРКИ";
                this.PreColor = "Red";
                this.UpdateCellFlags();
                this.RefreshButtons();
                return;
            }

            LoopState state = this.session.State;
            this.StateText = $"j = {state.Index} из {this.Cells.Count} · res = {state.Result} · t = n − j = {state.Variant}";
            this.CurrentIndexText = this.session.CurrentIndex?.ToString() ?? "—";
            this.StepCountText = this.session.Steps.Count.ToString();
            this.VariantValue = state.Variant;
            this.VariantMaximum = this.session.InitialState.Variant;
            this.VariantHistoryText = string.Join(" → ", this.session.VariantHistory);
            this.LastStepText = this.lastStep.HasValue ? this.DescribeStep(this.lastStep.Value) : "Шаг ещё не выполнялся";
            this.PreColor = this.Cells.Count >= 1 || this.selectedPreset.Mode != LoopMode.PrefixMax ? "Green" : "Red";

            if (this.session.IsCompleted)
            {
                bool holds = this.session.PostconditionHolds ?? false;
                this.PostColor = holds ? "Green" : "Red";
                this.PostText = holds
                    ? $"ВЫПОЛНЕНО: res = {state.Result}"
                    : $"НАРУШЕНО: res = {state.Result}, ожидалось {this.ExpectedResult()}";
            }
            else
            {
                this.PostColor = "Gray";
                this.PostText = "ОЖИДАНИЕ ВЫХОДА ИЗ ЦИКЛА";
            }

            this.UpdateCellFlags();
            this.RefreshInvariantIndicators();
            this.VerifyWp();
            this.RefreshCorrectnessConditions();
            this.RefreshButtons();
        }

        private void UpdateCellFlags()
        {
            foreach (LoopCell cell in this.Cells)
            {
                cell.IsProcessed = this.session is not null && cell.Index < this.session.State.Index;
                cell.IsCurrent = this.session?.CurrentIndex == cell.Index;
                cell.IsLastProcessed = this.lastStep.HasValue && cell.Index == this.lastStep.Value.ProcessedIndex;
            }
        }

        private void RefreshButtons()
        {
            bool completed = this.session?.IsCompleted ?? true;
            this.CanStep = !completed && !this.IsRunning;
            this.CanRun = !completed && !this.IsRunning && this.Cells.Count > 0;
            this.CanStop = this.IsRunning;
        }

        private void RefreshInvariantIndicators()
        {
            if (this.session is null || !this.TryParseInvariant(out var invariant) || invariant is null)
            {
                this.InvariantBeforeColor = "Red";
                this.InvariantAfterColor = "Red";
                this.InvariantBeforeText = "ФОРМУЛА НЕ РАЗОБРАНА";
                this.InvariantAfterText = "ФОРМУЛА НЕ РАЗОБРАНА";
                return;
            }

            LoopState before = this.lastBeforeState ?? this.session.State;
            bool beforeHolds = this.EvaluateInvariant(invariant, before);
            bool afterHolds = this.EvaluateInvariant(invariant, this.session.State);

            this.InvariantBeforeColor = beforeHolds ? "Green" : "Red";
            this.InvariantAfterColor = afterHolds ? "Green" : "Red";
            this.InvariantBeforeText = beforeHolds ? "ИСТИННО ДО ШАГА" : "ЛОЖЬ ДО ШАГА";
            this.InvariantAfterText = afterHolds ? "ИСТИННО ПОСЛЕ ШАГА" : "ЛОЖЬ ПОСЛЕ ШАГА";
        }

        private void VerifyWp()
        {
            if (this.session is null || !this.TryParseInvariant(out _))
            {
                this.VerificationCondition = string.Empty;
                this.SimplifiedCondition = string.Empty;
                this.VerificationHuman = this.InvariantParseError;
                this.WpHumanText = this.InvariantParseError;
                this.WpTraceText = string.Empty;
                this.PreservationColor = "Red";
                this.PreservationText = "ОШИБКА ФОРМУЛЫ";
                return;
            }

            this.session.VerifyInvariantBeforeStep();
            var result = this.session.WpVerificationResult;
            if (result is not null)
            {
                this.VerificationCondition = result.VerificationCondition;
                this.SimplifiedCondition = result.SimplifiedVC;
                this.VerificationHuman = this.DescribeFormula(result.VerificationCondition, result.HumanExplanation);
                this.PreservationColor = result.IsInvariantPreserved ? "Green" : "Red";
                this.PreservationText = result.IsInvariantPreserved ? "СОХРАНЯЕТСЯ" : "ТРЕБУЕТ УСЛОВИЙ";
            }

            string scalarBody = this.selectedPreset.BodyCode.Replace("a[j]", "currentElement", StringComparison.Ordinal);
            var wp = WpFacade.Calculate(scalarBody, this.invariantFormula);
            if (wp.IsSuccess)
            {
                this.WpHumanText = wp.FinalWpHuman;
                this.WpTraceText = wp.TraceText;
            }
            else
            {
                this.WpHumanText = "Разбор wp недоступен: " + wp.ErrorMessage;
                this.WpTraceText = string.Empty;
            }
        }

        private void RefreshCorrectnessConditions()
        {
            if (this.session is null || !this.TryParseInvariant(out var invariant) || invariant is null)
            {
                this.InitializationColor = "Red";
                this.InitializationStatus = "Невозможно проверить: формула инварианта не разобрана.";
                this.PreservationStatusColor = "Red";
                this.PreservationStatus = "Невозможно проверить.";
                this.ExitColor = "Gray";
                this.ExitStatus = "ОЖИДАНИЕ ВЫХОДА ИЗ ЦИКЛА";
                this.TerminationColor = "Gray";
                this.TerminationStatus = "ОЖИДАНИЕ ШАГА";
                return;
            }

            bool initializationHolds = this.EvaluateInvariant(invariant, this.session.InitialState);
            this.InitializationColor = initializationHolds ? "Green" : "Red";
            this.InitializationStatus = initializationHolds
                ? "Pre ⇒ Inv: инвариант истинен до первого шага."
                : "Pre ⇒ Inv нарушено в начальном состоянии.";

            bool preserved = this.session.WpVerificationResult?.IsInvariantPreserved ?? false;
            this.PreservationStatusColor = preserved ? "Green" : "Red";
            this.PreservationStatus = preserved
                ? "(Inv && B) ⇒ wp(S, Inv): тело сохраняет инвариант."
                : "(Inv && B) ⇒ wp(S, Inv): нужны дополнительные ограничения.";

            if (this.session.IsCompleted)
            {
                bool exitHolds = this.session.PostconditionHolds ?? false;
                this.ExitColor = exitHolds ? "Green" : "Red";
                this.ExitStatus = exitHolds
                    ? "(Inv && ¬B) ⇒ Post: постусловие выполнено."
                    : "(Inv && ¬B) ⇒ Post: результат не совпадает с Post.";
            }
            else
            {
                this.ExitColor = "Gray";
                this.ExitStatus = "(Inv && ¬B) ⇒ Post: проверится после завершения цикла.";
            }

            bool variantNonNegative = this.session.State.Variant >= 0;
            bool variantDecreased = this.session.Steps.All(step => step.VariantDecreased);
            this.TerminationColor = this.session.Steps.Count > 0 && variantNonNegative && variantDecreased ? "Green" : "Gray";
            this.TerminationStatus = this.session.Steps.Count > 0 && variantNonNegative && variantDecreased
                ? $"t ≥ 0 и t' < t: проверено шагов — {this.session.Steps.Count}."
                : $"t = {this.session.State.Variant} ≥ 0; строгое убывание проверится после шага.";
        }

        private bool TryParseInvariant(out AstPredication? predication)
        {
            var lex = AstLexer.Tokenize(this.invariantFormula);
            if (!lex.Ok || lex.Errors.Count > 0)
            {
                this.InvariantParseError = "Ошибка в формуле: " + lex.Errors[0].Message;
                this.InvariantHuman = string.Empty;
                predication = null;
                return false;
            }

            var parsed = AstParser.ParsePredication(lex);
            if (!parsed.Ok || parsed.Value is null)
            {
                this.InvariantParseError = "Ошибка в формуле: " + parsed.Errors[0].Message;
                this.InvariantHuman = string.Empty;
                predication = null;
                return false;
            }

            predication = parsed.Value;
            this.InvariantHuman = AstHumanText.Describe(parsed.Value);
            this.InvariantParseError = string.Empty;
            return true;
        }

        private bool EvaluateInvariant(AstPredication predication, LoopState state)
        {
            int count = this.Cells.Count;
            int currentIndex = count == 0 ? 0 : Math.Min(state.Index, count - 1);
            var variables = new Dictionary<string, decimal>
            {
                ["j"] = state.Index,
                ["res"] = state.Result,
                ["n"] = count,
                ["T"] = this.threshold,
                ["currentElement"] = currentIndex < count ? this.Cells[currentIndex].Value : 0
            };

            try
            {
                return AstInterpreter.Evalute(predication, variables);
            }
            catch (Exception ex)
            {
                this.InvariantParseError = "Не удалось вычислить инвариант: " + ex.Message;
                return false;
            }
        }

        private string DescribeFormula(string formula, string fallback)
        {
            var lex = AstLexer.Tokenize(formula);
            if (!lex.Ok || lex.Errors.Count > 0)
                return fallback;

            var parsed = AstParser.ParsePredication(lex);
            return parsed.Ok && parsed.Value is not null ? AstHumanText.Describe(parsed.Value) : fallback;
        }

        private string DescribeStep(LoopStep step)
        {
            int index = step.ProcessedIndex;
            return this.selectedPreset.Mode switch
            {
                LoopMode.PrefixSum => $"a[{index}] = {step.ProcessedValue}: res {step.Before.Result} + {step.ProcessedValue} = {step.After.Result}; j: {index} → {step.After.Index}",
                LoopMode.CountAboveThreshold => $"a[{index}] = {step.ProcessedValue} {(step.ProcessedValue > this.threshold ? "> T → res++" : "<= T → res без изменений")}: res = {step.After.Result}",
                _ => $"a[{index}] = {step.ProcessedValue}: res = max({step.Before.Result}, {step.ProcessedValue}) = {step.After.Result}"
            };
        }

        private long ExpectedResult()
        {
            var values = this.Cells.Select(cell => (long)cell.Value).ToList();
            return this.selectedPreset.Mode switch
            {
                LoopMode.PrefixSum => values.Sum(),
                LoopMode.CountAboveThreshold => this.Cells.Count(cell => cell.Value > this.threshold),
                _ => values.Max()
            };
        }

        private void Notify(string propertyName) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
