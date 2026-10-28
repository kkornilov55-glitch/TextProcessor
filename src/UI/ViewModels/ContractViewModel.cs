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
                this.PreCondition = "1. Поле 'Код программы' содержит корректный код на псевдокоде:\n" +
                                    "   - Присваивания: x := выражение;\n" +
                                    "   - Ветвление: if (условие) { ... } else { ... }\n" +
                                    "   - Последовательность операторов через точку с запятой\n" +
                                    "2. Поле 'Цель (Post-условие)' содержит корректный предикат:\n" +
                                    "   - Операторы сравнения: <, <=, >, >=, ==, !=\n" +
                                    "   - Логические операторы: && (И), || (ИЛИ)\n" +
                                    "   - Пример: x > 0 && y < 10";

                this.PostCondition = "1. Успешно вычислено слабейшее предусловие (wp)\n" +
                                     "2. Построена триада Хоара: {wp} код {Post}\n" +
                                     "3. Сгенерирован пошаговый трейс вычислений";

                this.Effects = "Метод вычисляет слабейшее предусловие (weakest precondition) для заданного кода и постусловия.\n" +
                               "Программа рассматривается как преобразователь предикатов.\n\n" +
                               "Правила вычисления:\n" +
                               "- Для присваивания x := e: wp(x:=e, R) = R[x/e] (подстановка e вместо x в R)\n" +
                               "- Для последовательности: wp(S1; S2, R) = wp(S1, wp(S2, R))\n" +
                               "- Для ветвления: wp(if B then S1 else S2, R) = (B ∧ wp(S1,R)) ∨ (¬B ∧ wp(S2,R))";

                this.ValidExample = "Код: if (x1 >= x2) { max := x1; } else { max := x2; }\n" +
                                    "Post: max > 100\n" +
                                    "Результат wp: (x1 >= x2 && x1 > 100) || (x1 < x2 && x2 > 100)\n" +
                                    "Триада: { (x1>=x2 && x1>100) || (x1<x2 && x2>100) }\n" +
                                    "        if (x1 >= x2) { max := x1; } else { max := x2; }\n" +
                                    "        { max > 100 }";

                this.InvalidExample = "Код: y := x / z;\n" +
                                      "Post: y > 5\n" +
                                      "Результат: Ошибка парсера, если в коде синтаксическая ошибка\n" +
                                      "Или: wp = (x/z > 5) && (z != 0) — добавлено условие определённости";
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