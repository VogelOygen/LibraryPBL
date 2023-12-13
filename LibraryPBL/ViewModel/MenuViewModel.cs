using LibraryPBL.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace LibraryPBL.ViewModel
{
        
    internal class NavigationService
    {
        internal class MenuViewModel
        {
            #region переменые 

            List<Book> allBooks = new();
            List<Book> selectedBooks = new();
            Book selectedBook;
            public IEnumerable<Book> ProgrammingBooks { get; set; }
            public IEnumerable<Book> DesignBooks { get; set; }
            public IEnumerable<Book> WebBooks { get; set; }
            public IEnumerable<Book> AIBooks { get; set; }
            IEnumerable<Book> selectedCategory;
            IEnumerable<Book> searchSelected;

            #endregion

            public MenuViewModel()
            {
                RefreshData();
                RunMenu();
            }

            #region сервисы

            #region инфраструктура

            //смотрим выбранные книги --> [CORE]
            private void SelectedBookPrint()
            {
                Console.WriteLine("Забронированные книги:\n");
                if (selectedBooks.Count != 0)
                {
                    foreach (Book b in selectedBooks)
                    {
                        Console.WriteLine(b.smallInfo());
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nНет забронированных книг");
                    Console.ResetColor();
                }

            }

            //вывод книг --> [CORE]
            private void TablePrint(IEnumerable<Book> BookType)
            {
                Console.Clear();

                bool headerPrinted = false;

                foreach (var (index, book) in BookType.Select((value, index) => (index, value)))
                {
                    if (!headerPrinted)
                    {
                        Console.WriteLine($"{"Name",-30} | {"Author",-20} | {"ISBN",-15} | {"Publish",-15} | {"Language",-20} | {"Category",-20} |");
                        Console.WriteLine(new string('-', 137));
                        headerPrinted = true;
                    }

                    Console.BackgroundColor = index % 2 != 0 ? ConsoleColor.Black : ConsoleColor.DarkGray;
                    Console.ForegroundColor = index % 2 != 0 ? ConsoleColor.Black : ConsoleColor.White;
                    Console.WriteLine($"{book.Name,-30} | {book.Author,-20} | {book.ISBN,-15} | {book.Publish,-15} | {string.Join(", ", book.Language),-20} | {string.Join(", ", book.Category),-20} |");
                    Console.WriteLine(new string('-', 137));

                    Console.ResetColor();
                }
            }

            //рандомайзер книг --> [CORE]
            private (ConsoleColor, string) RandomBooks()
            {
                var rnd = new Random();
                int r = rnd.Next(0, 20);
                string res;
                ConsoleColor color = new();

                if (r == 0)
                {
                    res = ("0, все экземпляры этой книги разобраны! :-(");
                    color = ConsoleColor.Red;
                }
                else
                {
                    res = ($"{r}. Да, мы выписали Вам одну книгу");
                    color = ConsoleColor.Green;
                }
                return (color, res);
            }

            //ищем книгу по всем разделам --> [CORE]
            private bool SearchBookServices()
            {
                selectedBook = null;

                while (selectedBook == null)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine("\nВведите название книги или введите 'back' для возврата");
                    Console.Write("--> ");
                    Console.ResetColor();
                    string s = Console.ReadLine();

                    if (s.ToLower() == "back") return false;

                    if (String.IsNullOrEmpty(s))
                    {
                        Console.WriteLine("Желаете продолжить поиск?");
                        Console.Write("[y] - да, [n] - нет: ");
                        string continueSearch = Console.ReadLine();

                        if (continueSearch.ToLower() == "n")
                        {
                            return false;
                        }
                    }
                    else
                    {
                        try
                        {
                            selectedBook = allBooks.FirstOrDefault(i => i.Name.ToLower().Contains(s.ToLower()));

                            if (selectedBook != null)
                            {
                                selectedBooks.Add(selectedBook);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Да, нашли {selectedBook.smallInfo()}");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("Такой книги нет :(\n\nВы желаете продолжить\n[y] - да\n[n] - нет");
                                Console.ResetColor();
                                Console.Write("\n--> ");
                                string e = Console.ReadLine();

                                if (e.ToLower() == "n")
                                {
                                    return false;
                                }
                            }
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            #endregion

            #region работа с файлом

            //инициируем книги --> [FILEMANAGER]
            private void BookInitialization()
            {
                try
                {
                    allBooks.Clear();

                    using (StreamReader bookUrl = new StreamReader("library.txt"))
                    {
                        while (bookUrl.Peek() > 0)
                        {
                            string s = bookUrl.ReadLine();
                            var _s = s.Split(';');

                            string[] _d = _s[4].Split('_');
                            string[] _p = _s[5].Split('_');

                            List<Language> lang = new List<Language>();
                            List<Category> cat = new List<Category>();

                            foreach (var lan in _p) lang.Add((Language)Enum.Parse(typeof(Language), lan));
                            foreach (var c in _d) cat.Add((Category)Enum.Parse(typeof(Category), c));

                            allBooks.Add(new Book
                            {
                                Name = _s[0],
                                Author = _s[1],
                                ISBN = _s[2],
                                Publish = _s[3],
                                Category = cat,
                                Language = lang
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
                }
            }
            
            //добавляем новую книгу --> [FILEMANAGER]
            private void AddBook()
            {
                using (StreamWriter sw = new StreamWriter("library.txt", true))
                {

                    string Name, Author, ISBN, Publish, Category, Language;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("\nНазвание книги или [back] - для выхода: ");
                    Console.ResetColor();
                    Name = Console.ReadLine();
                    if (Name == "back") return;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("Автор: ");
                    Console.ResetColor();
                    Author = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("ISBN: ");
                    Console.ResetColor();
                    ISBN = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("Издатель: ");
                    Console.ResetColor();
                    Publish = Console.ReadLine();

                    #region категория
                    string[] categories;
                    List<Category> selectedCategories;
                    do
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write("Категории: \"Programming\" | \"Web\" | \"Design\" | \"AI\" | --> [для 2х значений используйте: \"_\"]: ");
                        Console.ResetColor();
                        Category = Console.ReadLine();

                        categories = Category.Split('_');
                        selectedCategories = new List<Category>();

                        foreach (var cat in categories)
                        {
                            if (Enum.TryParse(cat, out Category category))
                            {
                                selectedCategories.Add(category);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Ошибка: Неверная категория '{cat}'.");
                                Console.Write("Пожалуйста, введите корректные категории.");
                                Console.ResetColor();
                                break;
                            }
                        }

                    } while (selectedCategories.Count != categories.Length);
                    Category = string.Join("_", selectedCategories.Select(c => c.ToString()));
                    #endregion

                    #region язык
                    string[] languages;
                    List<Language> selectedLanguages;

                    do
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write("Языки: \"Russian\" | \"English\" | \"Uzbek\" | --> [для 2х значений используйте знак \"_\"]: ");
                        Console.ResetColor();

                        Language = Console.ReadLine();
                        languages = Language.Split('_');
                        selectedLanguages = new List<Language>();

                        foreach (var lang in languages)
                        {
                            if (Enum.TryParse(lang, out Language language))
                            {
                                selectedLanguages.Add(language);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Ошибка: Неверный язык '{lang}'.");
                                Console.Write("Пожалуйста, введите корректные языки.");
                                Console.ResetColor();
                                break;
                            }
                        }

                    } while (selectedLanguages.Count != languages.Length);

                    Language = string.Join("_", selectedLanguages.Select(l => l.ToString()));
                    #endregion

                    Console.ForegroundColor = ConsoleColor.Green;
                    sw.WriteLine($"{Name};{Author};{ISBN};{Publish};{Category};{Language}");
                    Console.ResetColor();
                }
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\nНовая книга успешно добавлена!");
                Console.ResetColor();
                RefreshData();
                Console.WriteLine("Нажмите любую клавишу для перехода назад.");
                Console.ReadLine();
            }

            //редактируем книгу --> [FILEMANAGER]
            private void EditBook()
            {
                bool a = SearchBookServices();

                if (a)
                {
                    Console.WriteLine($"Введите новые данные для этой книги:\n{selectedBook.allInfo()}");

                    string Name, Author, ISBN, Publish, Category, Language;

                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("\nНазвание книги: ");
                    Console.ResetColor();
                    Name = Console.ReadLine();
                    if (String.IsNullOrEmpty(Name)) Name = selectedBook.Name;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("Автор: ");
                    Console.ResetColor();
                    Author = Console.ReadLine();
                    if (String.IsNullOrEmpty(Author)) Author = selectedBook.Author;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("ISBN: ");
                    Console.ResetColor();
                    ISBN = Console.ReadLine();
                    if (String.IsNullOrEmpty(ISBN)) ISBN = selectedBook.ISBN;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("Издатель: ");
                    Console.ResetColor();
                    Publish = Console.ReadLine();
                    if (String.IsNullOrEmpty(Publish)) Publish = selectedBook.Publish;

                    #region категория
                    string[] categories;
                    List<Category> selectedCategories;
                    do
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write("Категории: \"Programming\" | \"Web\" | \"Design\" | \"AI\" | --> [для 2х значений используйте: \"_\"]: ");
                        Console.ResetColor();
                        Category = Console.ReadLine();
                        if (String.IsNullOrEmpty(Category)) Category = selectedBook.Category.ToString();
                        categories = Category.Split('_');
                        selectedCategories = new List<Category>();

                        foreach (var cat in categories)
                        {
                            if (Enum.TryParse(cat, out Category category))
                            {
                                selectedCategories.Add(category);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Ошибка: Неверная категория '{cat}'.");
                                Console.Write("Пожалуйста, введите корректные категории.");
                                Console.ResetColor();
                                break;
                            }
                        }

                    } while (selectedCategories.Count != categories.Length);
                    Category = string.Join("_", selectedCategories.Select(c => c.ToString()));
                    #endregion
                    #region язык
                    string[] languages;
                    List<Language> selectedLanguages;

                    do
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write("Языки: \"Russian\" | \"English\" | \"Uzbek\" | --> [для 2х значений используйте знак \"_\"]: ");
                        Console.ResetColor();
                        Language = Console.ReadLine();
                        if (String.IsNullOrEmpty(Language)) Language = selectedBook.Language.ToString();
                        languages = Language.Split('_');
                        selectedLanguages = new List<Language>();

                        foreach (var lang in languages)
                        {
                            if (Enum.TryParse(lang, out Language language))
                            {
                                selectedLanguages.Add(language);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Ошибка: Неверная категория '{lang}'.");
                                Console.Write("Пожалуйста, введите корректные категории.");
                                Console.ResetColor();
                                break;
                            }
                        }

                    } while (selectedLanguages.Count != languages.Length);

                    Language = string.Join("_", selectedLanguages.Select(l => l.ToString()));
                    #endregion

                    selectedBook.Name = Name;
                    selectedBook.Author = Author;
                    selectedBook.ISBN = ISBN;
                    selectedBook.Publish = Publish;
                    selectedBook.Category = selectedCategories;
                    selectedBook.Language = selectedLanguages;

                    allBooks.Add(selectedBook);

                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"Изменения сохранены!");
                    Console.ResetColor();

                    SaveDataToFile();
                    RefreshData();
                    Console.WriteLine(selectedBook.allInfo());
                }
                else return;
            }

            //сохранение данных в файл --> [FILEMANAGER]
            private void SaveDataToFile()
            {
                using (StreamWriter sw = new StreamWriter("library.txt"))
                {
                    foreach (Book b in allBooks)
                    {

                        sw.WriteLine($"{b.Name};{b.Author};{b.ISBN};{b.Publish};{string.Join("_", b.Category)};{string.Join("_", b.Language)}");
                    }
                }

            }

            //обновление данных --> [FILEMANAGER]
            private void RefreshData()
            {
                BookInitialization();
                ProgrammingBooks = allBooks.Where(item => item.Category.Contains(Category.Programming));
                DesignBooks = allBooks.Where(item => item.Category.Contains(Category.Design));
                WebBooks = allBooks.Where(item => item.Category.Contains(Category.Web));
                AIBooks = allBooks.Where(item => item.Category.Contains(Category.AI));
            }

            #endregion

            #region навигация

            //завершение функции --> [NAV]
            private void SelectEnter()
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("\nНажмите ENTER для продолжения...");
                Console.ResetColor();
                while (Console.ReadKey().Key != ConsoleKey.Enter) ;
            }

            //заголовок меню
            private int SectionTitle(string Title, string Description, string[] menuItem)
            {
                Console.Clear();
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{Title}");
                Console.ResetColor();
                Console.WriteLine($"{Description}\n");

                int i = 0;
                foreach (var item in menuItem)
                {
                    i++;
                    PrintNavItem($"{i.ToString()}", $"{item}");
                }

                return i;
            }

            //форматируем текст префиксов меню --> [NAV]
            private void PrintNavItem(string prefix, string item)
            {
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write(prefix);
                Console.ResetColor();
                Console.Write($"] {item}\n");
            }

            //проверка ввода в меню --> [NAV]
            static int GetUserChoice(int maxChoice)
            {
                int choice;

                while (true)
                {
                    Console.Write("\n--> ");
                    if (int.TryParse(Console.ReadLine(), out choice) && choice >= 1 && choice <= maxChoice)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Invalid choice. Please enter a number between 1 and {maxChoice}.");
                    }
                }

                return choice;
            }

            #endregion

            #endregion

            #region методы пользователя
            //выбор книги --> [CORE]
            private void SelectedBook(IEnumerable<Book> bookType)
            {
                selectedBook = null;
                var randomBooks = RandomBooks();

                Console.WriteLine("\nВведите название интересующей Вас книги или команду [back] - для выхода");
                Console.Write($"--> ");

                while (selectedBook == null)
                {
                    string userInput = Console.ReadLine();

                    if (!string.IsNullOrEmpty(userInput))
                    {
                        try
                        {
                            if (userInput == "back")
                            {

                                return;
                            }

                            else
                            {
                                searchSelected = bookType.Where(t => t.Name.ToLower().Contains(userInput.ToLower())).ToList();

                                if (searchSelected.Any())
                                {
                                    int index = 0;

                                    foreach (Book book in searchSelected)
                                    {
                                        Console.WriteLine($"[{index}] {book.smallInfo()}");
                                        index++;
                                    }

                                    try
                                    {
                                        int selectedBookIndex = Convert.ToInt32(Console.ReadLine());

                                        if (selectedBookIndex >= 0 && selectedBookIndex < searchSelected.Count())
                                        {
                                            selectedBook = searchSelected.ElementAtOrDefault(selectedBookIndex);
                                            if (selectedBook != null)
                                            {
                                                selectedBooks.Add(selectedBook);
                                                Console.ForegroundColor = randomBooks.Item1;
                                                Console.WriteLine($"Вы выбрали {selectedBook.smallInfo()} таких книг осталось {randomBooks.Item2}");
                                                Console.ResetColor();
                                            }
                                            else
                                            {
                                                Console.WriteLine("Вы ввели неверный индекс");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Вы ввели неверный индекс");
                                        }
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine("Введите целое число");
                                    }

                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Книги с таким названием не найдены, попробуйте вновь или введите [back] - для выхода");
                                    Console.ResetColor();
                                    Console.Write("--> ");
                                }
                            }
                        }

                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            selectedBook = null;
                            Console.WriteLine($"ERROR: {ex.Message}\n");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        selectedBook = null;
                        Console.WriteLine("ERROR [пустая строка]: Введите название книги или команду [back]\n");
                        Console.ResetColor();
                    }
                }
            }


            //выбор раздела
            private void SelectedCategory()
            {
                while (selectedCategory == null)
                {
                    Console.Write("\n--> ");
                    string userInput = Console.ReadLine();

                    if (userInput == "back") return;
                    else
                    {
                        if (!String.IsNullOrEmpty(userInput))
                        {
                            try
                            {
                                switch (userInput)
                                {
                                    case "p":
                                        selectedCategory = ProgrammingBooks;
                                        break;
                                    case "w":
                                        selectedCategory = WebBooks;
                                        break;
                                    case "d":
                                        selectedCategory = DesignBooks;
                                        break;
                                    case "a":
                                        selectedCategory = AIBooks;
                                        break;
                                }
                            }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                selectedBook = null; Console.WriteLine("ERROR: Вы говорите что-то совсем несуразное, скажите чётче \n"); Console.Write("--> ");
                                Console.ResetColor();
                            }
                        }
                    }

                }
            }

            //бронирование книги
            private void BookingBook(string SelectedSubLvl)
            {
                selectedCategory = null;

                Console.Clear();
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{SelectedSubLvl}");
                Console.ResetColor();
                Console.WriteLine("Поиск по категориям\n");

                #region вывожу категории
                int programmingBookCount = ProgrammingBooks.Count();
                int webBookCount = WebBooks.Count();
                int designBookCount = DesignBooks.Count();
                int phoneBookCount = AIBooks.Count();

                Console.WriteLine($"[p] Программирование.... [{programmingBookCount}]\n[w] Web................. [{webBookCount}]\n[d] Дизайн.............. [{designBookCount}]\n[a] AI.................. [{phoneBookCount}]\n---\n[back] Вернуться");
                #endregion

                SelectedCategory();

                if (selectedCategory != null)
                {
                    TablePrint(selectedCategory);
                    SelectedBook(selectedCategory);
                }
            }

            //поиск книги по всем разделам
            private void SearchBook(string SelectedSubLvl)
            {
                Console.Clear();
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{SelectedSubLvl}");
                Console.ResetColor();
                Console.WriteLine("Поиск по названию\n");

                bool a = SearchBookServices();

                if (a)
                {
                    Console.Write("Мы выписали Вам 1 экземпляр");
                }
                else return;

            }

            #endregion

            //корневое меню --> [CORE]
            void RunMenu()
            {
                int choice;

                do
                {
                    int MaxChoices = SectionTitle("РЕГИСТРАЦИЯ", "Добро пожаловать! Подскажите, Вы администратор или читатель?", ["Администратор", "Читатель", "Выйти"]);

                    choice = GetUserChoice(MaxChoices);

                    switch (choice)
                    {
                        case 1:
                            AdminMenu("Панель администратора");
                            break;
                        case 2:
                            UserMenu("Библиотека");
                            break;
                        case 3:
                            Environment.Exit(0);
                            break;
                    }

                } while (true);
            }

            //sub-menu admin
            private void AdminMenu(string SelectedSubLvl)
            {
                int choice;

                do
                {
                    int MaxChoices = SectionTitle($"{SelectedSubLvl}", "Приветствую сэр! Укажите необходимую Вам функцию", ["Добавить книгу", "Редактировать книгу", "Просмотреть забронированные книги", "Вернуться в главное меню"]);

                    choice = GetUserChoice(MaxChoices);

                    switch (choice)
                    {
                        case 1:
                            AdminServices(1, SelectedSubLvl);
                            break;
                        case 2:
                            AdminServices(2, SelectedSubLvl);
                            break;
                        case 3:
                            AdminServices(3, SelectedSubLvl);
                            break;
                        case 4:
                            return;
                    }
                } while (true);
            }
                private void AdminServices(int choiceId, string SelectedSubLvl)
            {
                switch (choiceId)
                {
                    case 1:
                        //создаём книгу
                        AddBook();
                        SelectEnter();
                        break;
                    case 2:
                        //редактируем книгу
                        EditBook();
                        SelectEnter();
                        break;
                    case 3:
                        //смотрим забронированные книги
                        SelectedBookPrint();
                        SelectEnter();
                        break;
                }
            }

            //sub-menu user
            private void UserMenu(string SelectedSubLvl)
            {
                int choice;

                do
                {
                    int MaxChoices = SectionTitle($"{SelectedSubLvl}", "Добро пожаловать! Приступим к поиску...", ["Поиск по категориям", "Поиск по названию", "Вернуться в главное меню"]);

                    choice = GetUserChoice(MaxChoices);

                    switch (choice)
                    {
                        case 1:
                            UserServices(1, SelectedSubLvl);
                            break;
                        case 2:
                            UserServices(2, SelectedSubLvl);
                            break;
                        case 3:
                            return;
                    }
                } while (true);
            }
                private void UserServices(int choiceId, string SelectedSubLvl)
            {
                switch (choiceId)
                {
                    case 1:
                        //Поиск по категориям
                        BookingBook(SelectedSubLvl);
                        SelectEnter();
                        break;
                    case 2:
                        //Поиск по названию
                        SearchBook(SelectedSubLvl);
                        SelectEnter();
                        break;
                }
            }
        }
    }
}
