using LibraryPBL.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace LibraryPBL.ViewModel
{
    internal class MenuViewModel
    {
        #region переменные

        List<Book> allBooks = new();
        List<Book> selectedBooks = new();
        Book selectedBook;
        public IEnumerable<Book> ProgrammingBooks { get; set; }
        public IEnumerable<Book> DesignBooks { get; set; }
        public IEnumerable<Book> WebBooks { get; set; }
        public IEnumerable<Book> AIBooks { get; set; }
        IEnumerable<Book> selectedCategory;

        #endregion

        public MenuViewModel()
        {
            RefreshData();
            RunMenu();
        }

        #region сервисы

        //форматируем текст префиксов меню --> [CORE]
        private void PrintNavItem(string prefix, string item)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(prefix);
            Console.ResetColor();
            Console.Write($"] {item}\n");
        }

        //корневое меню --> [CORE]
        void RunMenu()
        {
            int choice;

            do
            {
                Console.Clear();
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"РЕГИСТРАЦИЯ");
                Console.ResetColor();
                Console.WriteLine("Добро пожаловать! Подскажите, Вы администратор или читатель?\n");
                PrintNavItem("1", "Администратор");
                PrintNavItem("2", "Читатель");
                PrintNavItem("3", "Выход\n");

                choice = GetUserChoice(3);

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

        //проверка ввода в меню --> [CORE]
        static int GetUserChoice(int maxChoice)
        {
            int choice;

            while (true)
            {
                Console.Write("--> ");
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

        //обновление данных --> [CORE]
        private void RefreshData()
        {
            BookInitialization();
            ProgrammingBooks = allBooks.Where(item => item.Category.Contains(Category.Programming));
            DesignBooks = allBooks.Where(item => item.Category.Contains(Category.Design));
            WebBooks = allBooks.Where(item => item.Category.Contains(Category.Web));
            AIBooks = allBooks.Where(item => item.Category.Contains(Category.AI));
        }

        //инициируем книги --> [CORE]
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

        //выбор книги --> [CORE]
        private Book SelectedBook(IEnumerable<Book> BookType, Action GoBack)
        {
            selectedBook = null;
            var _RandomBooks = RandomBooks();

            Console.WriteLine("\nВведите название интересующей Вас книги или команду [back] - для выхода");
            Console.Write($"--> ");

            while (selectedBook == null)
            {
                string b = Console.ReadLine();
                if (!String.IsNullOrEmpty(b))
                {
                    try
                    {
                        if (b == "back")
                        {
                            GoBack();
                        }
                        else
                        {
                            selectedBook = BookType.Where(t => t.Name == b).First();
                            selectedBooks.Add(selectedBook);
                            Console.ForegroundColor = _RandomBooks.Item1;
                            Console.WriteLine($"Вы выбрали {selectedBook.smallInfo()} таких книг осталось {_RandomBooks.Item2}");
                            Console.ResetColor();
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
            return selectedBook;
        }

        //вывод категорий --> [CORE]
        private void PrintCategories()
        {
            int programmingBookCount = ProgrammingBooks.Count();
            int webBookCount = WebBooks.Count();
            int designBookCount = DesignBooks.Count();
            int phoneBookCount = AIBooks.Count();

            Console.WriteLine($"[p] Программирование.... [{programmingBookCount}]\n[w] Web................. [{webBookCount}]\n[d] Дизайн.............. [{designBookCount}]\n[a] AI.................. [{phoneBookCount}]\n---\n[back] Вернуться");
        }

        //выбор раздела --> [CORE]
        private IEnumerable<Book> SelectedCategory(Action GoBack)
        {
            while (selectedCategory == null)
            {
                Console.Write("\n--> ");
                string userInput = Console.ReadLine();

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
                            case "back":
                                GoBack();
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
            return selectedCategory;
        }

        //ищем книгу по всем разделам --> [USER]
        private void SearchBookServices()
        {
            selectedBook = null;

            while (selectedBook == null)
            {
                Console.ForegroundColor= ConsoleColor.Black;
                Console.WriteLine("\nВведите название книги");
                Console.Write("--> ");
                Console.ResetColor();
                string s = Console.ReadLine();

                if (!String.IsNullOrEmpty(s))
                {
                    try
                    {
                        selectedBook = allBooks.Where(i => i.Name == s).First();
                        selectedBooks.Add(selectedBook);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Да, нашли {selectedBook.smallInfo()}");
                        Console.ResetColor();
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Такой книги нет :(\n\nВы желаете продолжить\n[y] - да\n[n] - нет");
                        Console.ResetColor();
                        Console.Write("\n--> ");
                        string e = Console.ReadLine();

                        if (e == "y") Console.ForegroundColor = ConsoleColor.Red; Console.Write("Ок, удачи..."); Console.ResetColor();
                        if (e == "n") return;
                    }
                }
            }
        }

        //добавляем новую книгу --> [ADMIN]
        private void AddBookServices()
        {
            using (StreamWriter sw = new StreamWriter("library.txt", true))
            {

                string Name, Author, ISBN, Publish, Category, Language;
                Console.ForegroundColor= ConsoleColor.Black;
                Console.Write("\nНазвание книги: ");
                Console.ResetColor();
                Name = Console.ReadLine();
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

        //редактируем книгу --> [ADMIN]
        private void EditBookServices()
        {
            SearchBookServices();
            Console.WriteLine($"Введите новые данные для этой книги:\n{selectedBook.allInfo()}");

            string Name, Author, ISBN, Publish, Category, Language;
            
            Console.ForegroundColor= ConsoleColor.Black;
            Console.Write("\nНазвание книги: ");
            Console.ResetColor();
            Name = Console.ReadLine();
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

            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor= ConsoleColor.White;
            Console.WriteLine($"Изменения сохранены!");
            Console.ResetColor();
            RefreshData();
            Console.WriteLine(selectedBook.allInfo());
        }

        #endregion

        #region Панель администратора

        //sub-menu
        private void AdminMenu(string SelectedSubLvl)
        {
            int choice;

            do
            {
                Console.Clear();
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{SelectedSubLvl}");
                Console.ResetColor();
                Console.WriteLine("Приветствую сэр! Укажите необходимую Вам функцию\n");
                PrintNavItem("1", "Добавить книгу");
                PrintNavItem("2", "Редактировать книгу");
                PrintNavItem("3", "Просмотреть забронированные книги");
                PrintNavItem("4", "Вернуться в главное меню\n");

                choice = GetUserChoice(4);

                switch (choice)
                {
                    case 1:
                        AddBook(() => AdminMenu(SelectedSubLvl), SelectedSubLvl);
                        Console.ReadLine();
                        break;
                    case 2:
                        EditBook(() => AdminMenu(SelectedSubLvl), SelectedSubLvl);
                        Console.ReadLine();
                        break;
                    case 3:
                        Booking(() => AdminMenu(SelectedSubLvl), SelectedSubLvl);
                        Console.ReadLine();
                        break;
                    case 4:
                        return;
                }
            } while (true);
        }

        //создаем книгу
        private void AddBook(Action backToMenu, string SelectedSubLvl)
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{SelectedSubLvl}");
            Console.ResetColor();
            Console.WriteLine("Добавление новой книги");
            AddBookServices();
            backToMenu.Invoke();
        }

        //смотрим забронированные книги
        private void Booking(Action backToMenu, string SelectedSubLvl)
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{SelectedSubLvl}");
            Console.ResetColor();
            Console.WriteLine("Забронированные книги:\n");
            foreach (Book b in selectedBooks)
            {
                Console.WriteLine(b.smallInfo());
            }

            Console.ReadLine();
            backToMenu.Invoke();
        }

        //редактируем существующие книги
        private void EditBook(Action backToMenu, string SelectedSubLvl)
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{SelectedSubLvl}");
            Console.ResetColor();
            Console.WriteLine("Редактирование книги");
            EditBookServices();

            Console.ReadLine();
            backToMenu.Invoke();
        }

        #endregion

        #region Пользовательский интерфейс

        //sub-menu
        private void UserMenu(string SelectedSubLvl)
        {
            int choice;

            do
            {

                Console.Clear();
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{SelectedSubLvl}");
                Console.ResetColor();
                Console.WriteLine("Добро пожаловать! Приступим к поиску...\n");
                PrintNavItem("1", "Поиск по категориям");
                PrintNavItem("2", "Поиск по названию");
                PrintNavItem("3", "Вернуться в главное меню\n");

                choice = GetUserChoice(3);

                switch (choice)
                {
                    case 1:
                        BookingBook(() => UserMenu(SelectedSubLvl), SelectedSubLvl);
                        Console.ReadLine();
                        break;
                    case 2:
                        SearchBook(() => UserMenu(SelectedSubLvl), SelectedSubLvl);
                        Console.ReadLine();
                        break;
                    case 3:
                        return;
                }

            } while (true);
        }

        //бронирование книги
        private void BookingBook(Action backToMenu, string SelectedSubLvl)
        {
            selectedCategory = null;
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{SelectedSubLvl}");
            Console.ResetColor();
            Console.WriteLine("Поиск по категориям\n");

            PrintCategories();
            SelectedCategory(() => UserMenu("Библиотека"));
            TablePrint(selectedCategory);
            SelectedBook(selectedCategory, () => BookingBook(backToMenu, SelectedSubLvl));

            Console.ReadLine();
            backToMenu.Invoke();
        }

        //поиск книги по всем разделам
        private void SearchBook(Action backToMenu, string SelectedSubLvl)
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{SelectedSubLvl}");
            Console.ResetColor();
            Console.WriteLine("Поиск по названию\n");

            SearchBookServices();
            Console.Write("Мы выписали Вам 1 экземпляр");
            Console.ReadLine();
            backToMenu.Invoke();
        }

        #endregion

    }
}