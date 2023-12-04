using ConsoleTables;
using LibraryPBL.Model;
using System;
using System.Collections.Generic;
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
        //
        List<Book> allBooks = new List<Book>();
        //
        public IEnumerable<Book> ProgrammingBooks { get; set; }
        public IEnumerable<Book> DesignBooks { get; set; }
        public IEnumerable<Book> WebBooks { get; set; }
        public IEnumerable<Book> AIBooks { get; set; }
        //
        Book selectedBook;
        List<Book> selectedBooks = new();
        //
        #endregion

        public MenuViewModel()
        {
            #region читаем данные из файла | создаём книги | заполняем категории

            using (StreamReader bookUrl = new StreamReader("libray.txt"))
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
            ProgrammingBooks = allBooks.Where(item => item.Category.Contains(Category.Programming));
            DesignBooks = allBooks.Where(item => item.Category.Contains(Category.Design));
            WebBooks = allBooks.Where(item => item.Category.Contains(Category.Web));
            AIBooks = allBooks.Where(item => item.Category.Contains(Category.AI));

            #endregion
        }

        //вывод книг
        private void TablePrint(IEnumerable<Book> BookType)
        {
            bool headerPrinted = false;

            foreach (var (index, book) in BookType.Select((value, index) => (index, value)))
            {
                if (!headerPrinted)
                {
                    Console.WriteLine($"{"Name",-30} | {"Author",-20} | {"ISBN",-15} | {"Publish",-15} | {"Language",-20} | {"Category",-20} |");
                    Console.WriteLine(new string('-', 140));
                    headerPrinted = true;
                }
                Console.BackgroundColor = index % 2 != 0 ? ConsoleColor.Black : ConsoleColor.Gray;
                Console.WriteLine($"{book.Name,-30} | {book.Author,-20} | {book.ISBN,-15} | {book.Publish,-15} | {string.Join(", ", book.Language),-20} | {string.Join(", ", book.Category),-20} |");
                Console.WriteLine(new string('-', 140));

                Console.ResetColor();
            }
        }

        //выбор книги
        private Book SelectedBook(IEnumerable<Book> BookType)
        {
            selectedBook = null;
            var _RandomBooks = RandomBooks();

            Console.WriteLine("\nВведите название интересующей Вас книги");
            Console.Write($"--> ");

            while (selectedBook == null)
            {
                string b = Console.ReadLine();
                if (!String.IsNullOrEmpty(b))
                {
                    try
                    {
                        selectedBook = BookType.Where(t => t.Name == b).First();
                        selectedBooks.Add(selectedBook);
                        Console.ForegroundColor = _RandomBooks.Item1;
                        Console.WriteLine($"Вы выбрали {selectedBook.smallInfo()} таких книг осталось {_RandomBooks.Item2}");
                        Console.ResetColor();
                        UserEndMenu();
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

        //рандомайзер книг
        private (ConsoleColor, string) RandomBooks()
        {
            var rnd = new Random();
            int r = rnd.Next(0,20);
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

        #region админка

        //меню
        public void AdminFuncSelected(string d)
        {
            switch (d)
            {
                case "add":
                    Console.Clear();
                    Console.WriteLine($"Создание книги: \n");
                    addNewBook();
                    break;
                case "st":
                    Console.Clear();
                    Console.WriteLine($"Забронированные книги: \n");
                        foreach(Book b in selectedBooks) Console.WriteLine(b.smallInfo());
                    break;
                default:
                    UserType("a");
                    break;
            }
        }
        //создание книги
        private void addNewBook()
        {

            using (StreamWriter sw = new StreamWriter("libray.txt", true))
            {

                string Name, Author, ISBN, Publish, Category, Language;

                Console.Write("Название книги: ");
                Name = Console.ReadLine();
                Console.Write("Автор: ");
                Author = Console.ReadLine();
                Console.Write("ISBN: ");
                ISBN = Console.ReadLine();
                Console.Write("Издатель: ");
                Publish = Console.ReadLine();
                Console.Write("Категория: (Programming | Design | Web | AI) --> ");
                Category = Console.ReadLine();
                Console.Write("Язык: (Russian | English | Uzbek) --> ");
                Language = Console.ReadLine();

                sw.WriteLine($"\n{Name};{Author};{ISBN};{Publish};{Category};{Language}\n");
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Книга добавлена!");
            Console.ResetColor();
            
            Console.WriteLine("Введите:\n[a] что-бы вернуться в меню\n[u] перейти к книгам");
            Console.Write("--> ");
            string d; d = Console.ReadLine();
            UserType(d);
        }

        #endregion

        #region навигация

        public void UserType(string d)
        {
            if (d == "a")
            {
                Console.Clear();
                Console.WriteLine("О, что же это я, не узнал Вас того самого администратора! Скажите, чем вы хотите заняться? \n[add] пополним коллекцию книг\n[st] статистика");
                Console.Write("\n--> "); string s = Console.ReadLine(); AdminFuncSelected(s);
            }
            else if (d == "u")
            {
                Console.Clear();
                Console.WriteLine("Добро пожаловать уважаемый читатель! Пожалуйста укажите интересующий Вас раздел... \n");
                categoriesMenuAndInfo(); Console.Write("\n--> "); string s = Console.ReadLine(); CategoriesSelected(s);
            }
            else
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"--- ERROR: Вы говорите что-то совсем несуразное, скажите чётче ---");
                Console.ResetColor();
                UserMenu();
            }
        }

        private void UserEndPoint(string d)
        {
            if (d == "b")
            {
                Console.Clear();
                UserType("u");

            }
            else if (d == "i")
            {
                Console.WriteLine(selectedBook.allInfo());
            }
            else
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"--- ERROR: Вы говорите что-то совсем несуразное, скажите чётче ---");
                Console.ResetColor();
                UserEndMenu();
            }
        }

        private void UserEndMenu()
        {
            Console.WriteLine("[b] Вернуться в меню\n[i] Узнать подробности о книге");
            Console.Write("\n--> ");
            string d = Console.ReadLine();
            UserEndPoint(d.ToLower());
        }

        public void UserMenu()
        {
            Console.WriteLine($"Добро пожаловать в библиотеку! Подскажите Вы тот самый администратор, или странствующий читатель? \n[a] - да, я администратор \n[u] - что у вас есть интересного для чтения? \n");
            Console.Write("\n--> ");
            string d = Console.ReadLine();
            UserType(d.ToLower());
        }

        // вывод названий категорий и кол-во книг
        public void categoriesMenuAndInfo()
        {
            int programmingBookCount = ProgrammingBooks.Count();
            int webBookCount = WebBooks.Count();
            int designBookCount = DesignBooks.Count();
            int phoneBookCount = AIBooks.Count();

            Console.WriteLine($"[p] Программирование.... [{programmingBookCount}]\n[w] Web................. [{webBookCount}]\n[d] Дизайн.............. [{designBookCount}]\n[a] AI.................. [{phoneBookCount}]\n\n[back] Главное меню");
        }

        //реализация меню
        public void CategoriesSelected(string selectedCategory)
        {
            switch (selectedCategory)
            {
                case "p":
                    Console.Clear();
                    Console.WriteLine($"Книги по программированию: \n");
                    TablePrint(ProgrammingBooks);
                    SelectedBook(ProgrammingBooks);
                    break;
                case "d":
                    Console.Clear();
                    Console.WriteLine($"Книги по дизайну: \n");
                    TablePrint(DesignBooks);
                    SelectedBook(DesignBooks);
                    break;
                case "w":
                    Console.Clear();
                    Console.WriteLine($"Книги по web: \n");
                    TablePrint(WebBooks);
                    SelectedBook(WebBooks);
                    break;
                case "a":
                    Console.Clear();
                    Console.WriteLine($"Книги по AI: \n");
                    TablePrint(AIBooks);
                    SelectedBook(AIBooks);
                    break;
                case "back":
                    Console.Clear();
                    UserMenu();
                    break;
                default:
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine();
                    Console.WriteLine($"--- ERROR: Похоже вы выбрали не существующий раздел попробуйте еще раз ---");
                    Console.ResetColor(); categoriesMenuAndInfo();
                    Console.WriteLine(); Console.Write("\n--> "); string a = Console.ReadLine();
                    CategoriesSelected(a);
                    break;
            }
        }
    }
    #endregion
}
