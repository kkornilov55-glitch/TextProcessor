using System.ComponentModel;

namespace UI.ViewModels
{
    public class ContractViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string title = "";
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.Notify("Title"); }
        }

        private string preCondition = "";
        public string PreCondition
        {
            get { return this.preCondition; }
            set { this.preCondition = value; this.Notify("PreCondition"); }
        }

        private string postCondition = "";
        public string PostCondition
        {
            get { return this.postCondition; }
            set { this.postCondition = value; this.Notify("PostCondition"); }
        }

        private string effects = "";
        public string Effects
        {
            get { return this.effects; }
            set { this.effects = value; this.Notify("Effects"); }
        }

        private string validExample = "";
        public string ValidExample
        {
            get { return this.validExample; }
            set { this.validExample = value; this.Notify("ValidExample"); }
        }

        private string invalidExample = "";
        public string InvalidExample
        {
            get { return this.invalidExample; }
            set { this.invalidExample = value; this.Notify("InvalidExample"); }
        }

        public ContractViewModel(string operationTitle)
        {
            this.BuildContract(operationTitle);
        }

        private void BuildContract(string operationTitle)
        {
            this.Title = "Контракт: " + operationTitle;

            if (operationTitle == "Нормализация текста")
            {
                this.PreCondition = "1. Строка содержит данные (не пустая и не null)\n" +
                                    "2. Тип переданного значения — строка (string)";
                this.PostCondition = "1. Возвращаемое значение не равно null\n" +
                                     "2. Возвращаемая строка полностью приведена к нижнему регистру\n" +
                                     "3. Возвращаемая строка не содержит двойных пробелов подряд\n" +
                                     "4. Возвращаемая строка не содержит знаков препинания и спецсимволов";
                this.Effects = "Исходная строка не изменяется. Возвращается новая строка. Если входная строка null — выбрасывается ArgumentException.";
                this.ValidExample = "Вход: \"  ПРИВЕТ, мир!!!  \"\n" +
                                    "Выход: \"привет мир\"\n" +
                                    "Результат: Pre OK, Post OK";
                this.InvalidExample = "Вход: null\n" +
                                      "Результат: Pre FAIL → выбрасывается ArgumentNullException";
            }
            else if (operationTitle == "Маскирование")
            {
                this.PreCondition = "1. Строка содержит данные (не пустая и не null)\n" +
                                    "2. После удаления всех нецифровых символов остаётся ровно 10 цифр (допускается формат 8XXXXXXXXXX или 7XXXXXXXXXX — первая цифра отбрасывается)";
                this.PostCondition = "1. Длина результирующей строки равна 18 символам\n" +
                                     "2. Строка начинается с \"+7 \" (корректный код страны)\n" +
                                     "3. В результирующей строке отсутствуют символы шаблона '#'"; ;
                this.Effects = "Исходная строка не изменяется. Возвращается новая строка в формате +7 (XXX) XXX-XX-XX.\n\n" +
                               "Исключения:\n" +
                               "- Не выбрасываются. При нарушении Pre/Post-условий метод возвращает ProcessResult с isSuccess = false и списком проваленных условий.";
                this.ValidExample = "Вход: \"89001234567\"\n" +
                                    "Выход: \"+7 (900) 123-45-67\"\n" +
                                    "Результат: Pre OK, Post OK";
                this.InvalidExample = "Вход: \"123\"\n" +
                                      "Результат: Pre FAIL → \"Корректное количество цифр\" не выполнено, isSuccess = false, OutputText не заполнен";
            }
            else if (operationTitle == "Фильтрация строк")
            {
                this.PreCondition = "1. Исходный текст не пуст (содержит хотя бы одну строку)\n" +
                                    "2. Ключевое слово задано (не пустая строка и не null)"; ;
                this.PostCondition = "1. Найдены совпадения ключевого слова в тексте (хотя бы одна строка содержит искомое слово)\n" +
                                     "2. Результирующий текст не пуст";
                this.Effects = "Исходный текст не изменяется. Возвращается новый текст, состоящий только из строк, содержащих ключевое слово (поиск без учёта регистра).\n\n" +
                               "Исключения:\n" +
                               "- Не выбрасываются. При нарушении Pre/Post-условий метод возвращает ProcessResult с isSuccess = false и списком проваленных условий.";
                this.ValidExample = "Вход: \"Привет мир\nЭто тестовая строка\nЕщё текст\"\n" +
                                    "Ключевое слово: \"тест\"\n" +
                                    "Выход: \"Это тестовая строка\"\n" +
                                    "Результат: Pre OK, Post OK";

                this.InvalidExample = "Вход: \"Привет мир\\nЕщё текст\"\n" +
                                      "Ключевое слово: \"тест\"\n" +
                                      "Результат: Post FAIL → \"Найдены совпадения в тексте\" не выполнено, isSuccess = false, OutputText не заполнен";
            }
            else if (operationTitle == "WP Калькулятор")
            {
                this.Title = "Справка: WP Калькулятор";

                this.PreCondition = "Что такое слабейшее предусловие (wp)?\n\n" +
                                    "wp(S, R) — это самое слабое (наименее ограничительное) условие, которое должно быть истинно ДО выполнения программы S, чтобы гарантировать истинность постусловия R ПОСЛЕ выполнения.\n\n" +
                                    "Калькулятор автоматически вычисляет wp, протягивая постусловие «с конца к началу» через каждый оператор программы.";

                this.PostCondition = "Как пользоваться калькулятором:\n\n" +
                                     "1. В поле «Код программы» введите фрагмент кода на псевдокоде.\n" +
                                     "2. В поле «Цель (Post-условие)» введите условие, которое должно быть истинно после выполнения кода.\n" +
                                     "3. Нажмите кнопку «Выполнить».\n" +
                                     "4. В блоке результатов появится пошаговый трейс вычисления wp, итоговое предусловие и триада Хоара.";

                this.Effects = "Синтаксис кода программы:\n\n" +
                               "• Присваивание: x := выражение;\n" +
                               "• Ветвление: if (условие) { операторы } else { операторы }\n" +
                               "• Последовательность: операторы разделяются точкой с запятой (;)\n" +
                               "• Поддерживаемые операции: +, -, *, /, abs()\n" +
                               "• Переменные: имена из букв, цифр и подчёркивания\n\n" +
                               "Синтаксис постусловия:\n\n" +
                               "• Операторы сравнения: <, <=, >, >=, ==, !=\n" +
                               "• Логические операторы: && (И), || (ИЛИ)\n" +
                               "• Пример: x > 0 && y < 10";

                this.ValidExample = "Пример 1: Max из двух\n" +
                                    "Код: if (x1 >= x2) { max := x1; } else { max := x2; }\n" +
                                    "Post: max > 100\n" +
                                    "Результат wp: (x1 >= x2 && x1 > 100) || (x1 < x2 && x2 > 100)\n\n" +
                                    "Пример 2: Последовательность присваиваний\n" +
                                    "Код: y := x + 1; x := x + 10;\n" +
                                    "Post: y == x - 9 && x > 15\n" +
                                    "Результат wp: x > 5";

                this.InvalidExample = "Типичные ошибки:\n\n" +
                                      "• Пустое поле «Код программы» или «Цель» → ошибка валидации\n" +
                                      "• Синтаксическая ошибка в коде (например, пропущена точка с запятой) → ошибка парсера\n" +
                                      "• Некорректное постусловие (например, пропущен оператор сравнения) → ошибка парсера\n" +
                                      "• Деление на ноль: калькулятор автоматически добавляет условие определённости (знаменатель ≠ 0)";
            }
            else
            {
                this.PreCondition = "Неизвестная операция.";
                this.PostCondition = "Неизвестная операция.";
                this.Effects = "Нет данных.";
                this.ValidExample = "Нет данных.";
                this.InvalidExample = "Нет данных.";
            }
        }
    }
}