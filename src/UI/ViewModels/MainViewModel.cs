using TextProcessor.Logic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;
using System.Linq;

namespace UI.ViewModels
{
    public class ConditionUiModel
    {
        public string Name { get; set; }
        public bool IsMet { get; set; }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Свойства для интерфейса
        private string currentOperationTitle = "Нормализация текста";
        public string CurrentOperationTitle
        {
            get
            {
                return this.currentOperationTitle;
            }
            set
            {
                this.currentOperationTitle = value;
                this.Notify("CurrentOperationTitle");
                this.UpdateOperationVisibility();
                this.ResetStatuses();
            }
        }


        private string inputText = "";
        public string InputText
        {
            get
            {
                return this.inputText;
            }
            set
            {
                this.inputText = value;
                this.Notify("InputText");
                this.CheckPreConditions();
            }
        }

        // Ключевое слово для фильтрации
        private string filterKeyword = "";
        public string FilterKeyword
        {
            get { return this.filterKeyword; }
            set
            {
                this.filterKeyword = value;
                this.Notify("FilterKeyword");
                this.CheckPreConditions();
            }
        }

        // Выбранный тип маски
        private string selectedMaskType = "Телефон (10 цифр)";
        public string SelectedMaskType
        {
            get { return this.selectedMaskType; }
            set
            {
                this.selectedMaskType = value;
                this.Notify("SelectedMaskType");
            }
        }

        // Видимость поля фильтрации
        private System.Windows.Visibility isFilteringVisible = System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility IsFilteringVisible
        {
            get { return this.isFilteringVisible; }
            set
            {
                this.isFilteringVisible = value;
                this.Notify("IsFilteringVisible");
            }
        }

        // Видимость выбора маски
        private System.Windows.Visibility isMaskingOptionsVisible = System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility IsMaskingOptionsVisible
        {
            get { return this.isMaskingOptionsVisible; }
            set
            {
                this.isMaskingOptionsVisible = value;
                this.Notify("IsMaskingOptionsVisible");
            }
        }

        // Индикаторы Pre
        private string preColor = "Gray";
        public string PreColor
        {
            get { return this.preColor; }
            set
            {
                this.preColor = value;
                this.Notify("PreColor");
            }
        }

        private string preText = "ОЖИДАНИЕ ПРОВЕРКИ";
        public string PreText
        {
            get { return this.preText; }
            set
            {
                this.preText = value;
                this.Notify("PreText");
            }
        }
        private System.Windows.Visibility isWpVisible = System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility IsWpVisible
        {
            get { return this.isWpVisible; }
            set { this.isWpVisible = value; this.Notify("IsWpVisible"); }
        }

        private System.Windows.Visibility isContractVisible = System.Windows.Visibility.Visible;
        public System.Windows.Visibility IsContractVisible
        {
            get { return this.isContractVisible; }
            set { this.isContractVisible = value; this.Notify("IsContractVisible"); }
        }
        // Методы управления видимостью

        private void UpdateOperationVisibility()
        {
            this.IsFilteringVisible = (this.currentOperationTitle == "Фильтрация строк")
                ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            this.IsMaskingOptionsVisible = (this.currentOperationTitle == "Маскирование")
                ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            this.IsWpVisible = (this.currentOperationTitle == "WP Калькулятор")
                ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            this.IsContractVisible = (this.currentOperationTitle == "WP Калькулятор")
                ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        private string wpPostCondition = "";
        public string WpPostCondition
        {
            get { return this.wpPostCondition; }
            set
            {
                this.wpPostCondition = value;
                this.Notify("WpPostCondition");
            }
        }

        private string wpSteps = "";
        public string WpSteps
        {
            get { return this.wpSteps; }
            set
            {
                this.wpSteps = value;
                this.Notify("WpSteps");
            }
        }

        private string wpPre = "";
        public string WpPre
        {
            get { return this.wpPre; }
            set
            {
                this.wpPre = value;
                this.Notify("WpPre");
            }
        }

        private string wpTriad = "";
        public string WpTriad
        {
            get { return this.wpTriad; }
            set
            {
                this.wpTriad = value;
                this.Notify("WpTriad");
            }
        }

        // Индикаторы Post
        private string postColor = "Gray";
        public string PostColor
        {
            get { return this.postColor; }
            set
            {
                this.postColor = value;
                this.Notify("PostColor");
            }
        }

        private string postText = "ОЖИДАНИЕ ПРОВЕРКИ";
        public string PostText
        {
            get { return this.postText; }
            set
            {
                this.postText = value;
                this.Notify("PostText");
            }
        }

        // Кнопки слева
        public void SelectNormalization()
        {
            this.CurrentOperationTitle = "Нормализация текста";
        }

        public void SelectMasking()
        {
            this.CurrentOperationTitle = "Маскирование";
        }
        public void SelectWpCalculator()
        {
            this.CurrentOperationTitle = "WP Калькулятор";
        }
        public void SelectFiltering()
        {
            this.CurrentOperationTitle = "Фильтрация строк";
        }

        // Логика статусов
        private void ResetStatuses()
        {
            this.PreColor = "Gray"; // Серый цвет для нейтрального состояния
            this.PreText = "ОЖИДАНИЕ ПРОВЕРКИ";
            this.PostColor = "Gray";
            this.PostText = "ОЖИДАНИЕ ПРОВЕРКИ";
            this.CheckPreConditions();
        }

        // Кнопка Выполнить
        public void ExecuteOperation()
        {
            try
            {
                if (this.currentOperationTitle == "Нормализация текста")
                {
                    var normalizer = new Normalization();
                    this.inputText = normalizer.Normalize(this.inputText);
                    
                    this.PreColor = "Green";
                    this.PreText = "ВЫПОЛНЕНО";

                    this.PostColor = "Green";
                    this.PostText = "ВЫПОЛНЕНО";
                }
                else if (this.currentOperationTitle == "Маскирование")
                {
                    var masker = new TextMasking();
                    ProcessResult result;

                    // Выбираем метод в зависимости от того, что выбрал пользователь в интерфейсе
                    if (this.selectedMaskType == "Телефон (10 цифр)")
                    {
                        result = masker.RuNumberMask(this.inputText);
                    }
                    else if (this.selectedMaskType == "СНИЛС (11 цифр)")
                    {
                        result = masker.SnilsMask(this.inputText);
                    }
                    else if (this.selectedMaskType == "Банковская карта (16 цифр)")
                    {
                        result = masker.BankCardMask(this.inputText);
                    }
                    else
                    {
                        result = masker.RuNumberMask(this.inputText); // запасной вариант
                    }

                    this.HandleProcessResult(result);
                }
                else if (this.currentOperationTitle == "Фильтрация строк")
                {
                    var filter = new TextFiltering();
                    string keywordToUse = string.IsNullOrWhiteSpace(this.filterKeyword) ? "тест" : this.filterKeyword;
                    ProcessResult result = filter.FilterLinesByKeyword(this.inputText, keywordToUse);
                    this.HandleProcessResult(result);
                }
                else if (this.currentOperationTitle == "WP Калькулятор")
                {
                    this.CalculateWp();
                }

                this.Notify("InputText");
            }
            catch (System.ArgumentNullException ex)
            {
                this.PreColor = "Red";
                this.PreText = "ОШИБКА Pre: " + ex.Message;
                this.PostColor = "Red";
                this.PostText = "ОШИБКА Pre: " + ex.Message;
            }
            catch (System.ArgumentException ex)
            {
                this.PreColor = "Red";
                this.PreText = "ОШИБКА Pre: " + ex.Message;
                this.PostColor = "Red";
                this.PostText = "НЕ ВЫПОЛНЕНО";
            }
            catch (System.InvalidOperationException ex)
            {
                this.PostColor = "Red";
                this.PostText = "ОШИБКА Post: " + ex.Message;
            }
            catch (System.Exception ex)
            {
                this.PostColor = "Red";
                this.PostText = "КРИТИЧЕСКАЯ ОШИБКА: " + ex.Message;
            }
        }

        // Вспомогательный метод для обработки результата
        private void HandleProcessResult(ProcessResult result)
        {
            var failedPres = result.PreConditions.Where(c => !c.IsMet).ToList();

            if (failedPres.Any())
            {
                this.PreColor = "Red";
                string preErrors = string.Join("; ", failedPres.Select(c => c.Name));
                this.PreText = "ОШИБКА: " + preErrors;

                this.PostColor = "Red";
                this.PostText = "НЕ ВЫПОЛНЕНО";
                return;
            }

            this.PreColor = "Green";
            this.PreText = "ВЫПОЛНЕНО";

            if (result.isSuccess == true)
            {
                this.inputText = result.OutputText;
                this.PostColor = "Green";
                this.PostText = "ВЫПОЛНЕНО";
            }
            else
            {
                this.PostColor = "Red";
                var failedPosts = result.PostConditions.Where(c => !c.IsMet).ToList();
                string postErrors = string.Join("; ", failedPosts.Select(c => c.Name));
                this.PostText = "ОШИБКА: " + postErrors;
            }

            this.Notify("InputText");
        }

        // Динамическая проверка Pre-условий
        private void CheckPreConditions()
        {
            bool isMet = false;
            string message = "";

            if (this.currentOperationTitle == "Нормализация текста")
            {
                isMet = !string.IsNullOrWhiteSpace(this.inputText);
                message = isMet ? "Текст содержит данные для обработки" : "Текст пуст или состоит из пробелов";
            }
            else if (this.currentOperationTitle == "Фильтрация строк")
            {
                isMet = !string.IsNullOrWhiteSpace(this.inputText) && !string.IsNullOrWhiteSpace(this.filterKeyword);
                message = isMet ? "Текст и ключевое слово заданы" : "Текст или ключевое слово пусты";
            }
            else if (this.currentOperationTitle == "Маскирование")
            {
                // Для маскирования проверяем содержание цифр
                if (string.IsNullOrWhiteSpace(this.inputText))
                {
                    isMet = false;
                    message = "Текст пуст";
                }
                else
                {
                    // Извлекаем только цифры из текста
                    string digitsOnly = new string(this.inputText.Where(char.IsDigit).ToArray());

                    // Проверяем количество цифр в зависимости от типа маски
                    int requiredDigits = 0;
                    string maskType = "";

                    if (this.selectedMaskType == "Телефон (10 цифр)")
                    {
                        requiredDigits = 10;
                        maskType = "телефона";
                    }
                    else if (this.selectedMaskType == "СНИЛС (11 цифр)")
                    {
                        requiredDigits = 11;
                        maskType = "СНИЛС";
                    }
                    else if (this.selectedMaskType == "Банковская карта (16 цифр)")
                    {
                        requiredDigits = 16;
                        maskType = "банковской карты";
                    }

                    if (digitsOnly.Length == requiredDigits)
                    {
                        isMet = true;
                        message = $"Найдено {requiredDigits} цифр (корректно для {maskType})";
                    }
                    else if (digitsOnly.Length > requiredDigits)
                    {
                        isMet = false;
                        message = $"Найдено {digitsOnly.Length} цифр, нужно {requiredDigits} (слишком много)";
                    }
                    else
                    {
                        isMet = false;
                        message = $"Найдено {digitsOnly.Length} цифр, нужно {requiredDigits} (недостаточно)";
                    }
                }
            }

            // Обновляем UI
            this.PreColor = isMet ? "Green" : "Red";
            this.PreText = isMet ? "ВЫПОЛНЕНО" : "НЕ ВЫПОЛНЕНО: " + message;
        }

        // Кнопка Показать контракт
        public void ShowContract()
        {
            ContractViewModel contractViewModel = new ContractViewModel(this.currentOperationTitle);
            ContractWindow contractWindow = new ContractWindow(contractViewModel);
            contractWindow.ShowDialog();
        }

        // Кнопка Загрузить файл
        public void LoadFile()
        {
            // Создаем диалог открытия файла
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выберите текстовый файл для загрузки";
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            openFileDialog.DefaultExt = ".txt";

            // Показываем диалог и проверяем, нажал ли пользователь "ОК"
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                try
                {
                    // Используем бизнес-логику для чтения файла
                    TextFileIO fileIO = new TextFileIO();
                    string loadedText = fileIO.ImportFromFile(openFileDialog.FileName);

                    // Записываем текст в свойство
                    this.InputText = loadedText;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при чтении файла:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Кнопка Сохранить файл
        public void SaveFile()
        {
            // Создаем диалог сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Выберите место для сохранения файла";
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.FileName = "result.txt"; // Имя по умолчанию

            // Показываем диалог и проверяем, нажал ли пользователь "Сохранить"
            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                try
                {
                    // Используем бизнес-логику для записи файла
                    TextFileIO fileIO = new TextFileIO();
                    fileIO.ExportToFile(saveFileDialog.FileName, this.InputText);

                    MessageBox.Show("Файл успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении файла:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //Новое



        private void CalculateWp()
        {
            // 1. Жёсткая проверка: если поле пустое, сразу показываем понятную ошибку
            if (string.IsNullOrWhiteSpace(this.wpPostCondition))
            {
                this.WpSteps = "Ошибка: Поле 'Цель (Post-условие)' пустое. Проверьте, что вы ввели текст и привязка (Binding) в XAML работает.";
                this.WpPre = "";
                this.WpTriad = "";
                this.PreColor = "Red";
                this.PreText = "ОШИБКА: Постусловие не введено";
                this.PostColor = "Red";
                this.PostText = "НЕ ВЫПОЛНЕНО";
                return;
            }

            try
            {
                string codeText = this.inputText;
                string postConditionText = this.wpPostCondition;

                // Отладочный вывод (видно в окне "Вывод" / "Output" в Visual Studio)
                System.Diagnostics.Debug.WriteLine($"[WP DEBUG] Код программы: '{codeText}'");
                System.Diagnostics.Debug.WriteLine($"[WP DEBUG] Постусловие: '{postConditionText}'");

                var result = TextProcessor.Logic.AbstractSyntaxTree.WpFacade.Calculate(codeText, postConditionText);

                if (result.IsSuccess)
                {
                    this.WpSteps = result.TraceText;
                    this.WpPre = result.FinalWpShort + "\n\n" + result.FinalWpHuman;
                    this.WpTriad = result.TriadNotation + "\n\n" + result.TriadWords;

                    this.PreColor = "Green";
                    this.PreText = "ВЫПОЛНЕНО";
                    this.PostColor = "Green";
                    this.PostText = "ВЫПОЛНЕНО";
                }
                else
                {
                    this.WpSteps = "Ошибка парсера: " + result.ErrorMessage;
                    this.WpPre = "";
                    this.WpTriad = "";

                    this.PreColor = "Red";
                    this.PreText = "ОШИБКА: " + result.ErrorMessage;
                    this.PostColor = "Red";
                    this.PostText = "НЕ ВЫПОЛНЕНО";
                }
            }
            catch (Exception ex)
            {
                this.WpSteps = "Критическая ошибка: " + ex.Message;
                this.PreColor = "Red";
                this.PreText = "ОШИБКА: " + ex.Message;
                this.PostColor = "Red";
                this.PostText = "НЕ ВЫПОЛНЕНО";
            }
        }



    }
        

}