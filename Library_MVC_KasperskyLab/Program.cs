using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Library_MVC_KasperskyLab
{
    class Program
    {
        static string greeting = "\t\t\t*********************************\n" +
                                 "\t\t\t*      Система учета книг       *\n" +
                                 "\t\t\t*        в библиотеке v2        *\n" +
                                 "\t\t\t*       Максим Нестеров         *\n" +
                                 "\t\t\t*********************************\n";
        class ISource
        {
            public struct Book
            {
                public int Id;
                public string Title;
                public string Author;
                public Book(int Id, string Title, string Author)
                {
                    this.Id = Id;
                    this.Title = Title;
                    this.Author = Author;
                }
            }
            public struct Visitor
            {
                public int Id;
                public string Name;
                public Visitor(int Id, string Name)
                {
                    this.Id = Id;
                    this.Name = Name;
                }
            }
            public struct Issue
            {
                public int Book_Id;
                public int Visitor_Id;
                public System.DateTime Issue_Date;
                public Issue(int VisitorId, int BookId, System.DateTime IssueDate)
                {
                    Book_Id = BookId;
                    Visitor_Id = VisitorId;
                    Issue_Date = IssueDate;
                }
            }
            public List<Book> Books;
            public List<Visitor> Visitors;
            public List<Issue> Issues;
            public bool Ready;
            public ISource()
            {
                Books = new List<Book>();
                Visitors = new List<Visitor>();
                Issues = new List<Issue>();
                Ready = false;
            }
        }
        class TextFileSource : ISource
        {
            public string file_path;
            public TextFileSource(string path)
            {
                file_path = path;
                Ready=ReadSource();
            }
            public TextFileSource()
            {
                file_path = "";
            }
            public void Reload()
            {
                Ready = ReadSource();
            }
            public void AddBook(int Id, string Title, string Author)
            {
                Books.Add(new Book(Id, Title, Author));
            }
            public void AddVisitor(int Id, string Name)
            {
                Visitors.Add(new Visitor(Id, Name));
            }
            public void AddIssue(int VisitorId, int BookId, System.DateTime IssueDate)
            {
                Issues.Add(new Issue(VisitorId, BookId, IssueDate));
            }
            bool ReadSource()
            {
                try
                {
                    using (FileStream file = File.OpenRead(file_path))
                    {
                        System.Globalization.CultureInfo cultureinfo =
        new System.Globalization.CultureInfo("ru-RU",false);
                        using (StreamReader sr = new StreamReader(file))
                        {
                            while (!sr.EndOfStream)
                            {
                                string line = sr.ReadLine();
                                if(line.Length>0)
                                switch (line[0])
                                {
                                    case 'B':
                                        {
                                            line = line.Substring(2);
                                            string[] splitstr = line.Split(';');
                                            if (splitstr.Length >= 3)
                                            {
                                                AddBook(Convert.ToInt32(splitstr[0]), splitstr[1], splitstr[2]);
                                            }
                                            break;
                                        }
                                    case 'V':
                                        {
                                            line = line.Substring(2);
                                            string[] splitstr = line.Split(';');
                                            if (splitstr.Length >= 2)
                                            {
                                                AddVisitor(Convert.ToInt32(splitstr[0]), splitstr[1]);
                                            }
                                            break;
                                        }
                                    case 'I':
                                        {
                                            line = line.Substring(2);
                                            string[] splitstr = line.Split(';');
                                            if (splitstr.Length >= 3)
                                            {
                                                AddIssue(Convert.ToInt32(splitstr[0]),Convert.ToInt32(splitstr[1]), System.DateTime.Parse(splitstr[2],cultureinfo));
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            break;
                                        }
                                }
                            }
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }
        class HardcodedSource : ISource
        {
            public void AddBook(string Title, string Author)
            {
                Books.Add(new Book(Books.Count, Title, Author));
            }
            public void AddVisitor(string Name)
            {
                Visitors.Add(new Visitor(Visitors.Count, Name));
            }
            public void AddIssue(int VisitorId, int BookId, System.DateTime IssueDate)
            {
                Issues.Add(new Issue(VisitorId, BookId, IssueDate));
            }
        }
        public delegate void SendInfo(string message);
        public delegate SendInfo GetDelegate();
        /// <summary>
        /// Модель Библиотеки с бизнес-логикой
        /// </summary>
        class LibraryModel
        {
            /// <summary>
            /// Уровень модели (Книга)
            /// </summary>
            public class BookModel
            {
                int Id;
                public int GetId()
                {
                    return Id;
                }
                bool IsAvailable;
                string Title;
                string Author;
                DateTime IssueDate;
                DateTime ExpiryDate;
                SendInfo ToView;
                public BookModel(int id, string title, string author, SendInfo Func)
                {
                    this.Id = id;
                    this.Title = title;
                    this.Author = author;
                    IsAvailable = true;
                    IssueDate = System.DateTime.Today;
                    ExpiryDate = System.DateTime.Today;
                    ToView = Func;
                    RefreshView();
                }
                public void Issue(System.DateTime issueDate, System.DateTime expiryDate)
                {
                    IsAvailable = false;
                    this.IssueDate = issueDate;
                    this.ExpiryDate = expiryDate;
                    RefreshView();
                }
                // Обратная связь с View (активная MVC)
                void RefreshView()
                {
                    ToView("LI:" + GetLongInfo());
                    ToView("SI:" + GetShortInfo());
                }
                public void Recieve()
                {
                    IsAvailable = true;
                    RefreshView();
                }
                public string GetLongInfo()
                {
                    return Id + "  " + Title + "\t" + Author + "\t" + IsAvailable.ToString
                            () + "    \t" + IssueDate.ToShortDateString() + " " + ExpiryDate.ToShortDateString() + "\r\n";
                }
                public string GetShortInfo()
                {
                    return "Id: "+Id + ", Title: \"" + Title.TrimEnd() + "\", Author: " + Author +
                                    ", Exp. Date: " + ExpiryDate.ToShortDateString();
                }
                public System.DateTime GetExpiryDate()
                {
                    return ExpiryDate;
                }
                public bool isAvailable()
                {
                    return IsAvailable;
                }
            }
            /// <summary>
            /// Уровень модели (Посетитель)
            /// </summary>
            public class VisitorModel
            {
                public VisitorModel(int v_id, string v_name, SendInfo Func)
                {
                    Books = new List<BookModel>();
                    Id = v_id;
                    Name = v_name;
                    ToView = Func;
                    RefreshView();
                }
                int Id;

                string Name;
                List<BookModel> Books;
                SendInfo ToView;
                void RefreshView()
                {
                    ToView(GetInfo());
                }
                public int GetId()
                {
                    return Id;
                }
                public int GetBooksCount()
                {
                    return Books.Count;
                }
                public void GiveBook(BookModel book)
                {
                    Books.Add(book);
                }
                public bool TakeBackBook(BookModel book)
                {
                    return Books.Remove(book);
                }
                public List<BookModel> GetBooks()
                {
                    return Books;
                }
                public string GetInfo()
                {
                    return Id + "\t" + Name + "\n";
                }
            }
            /// <summary>
            /// Модели книг
            /// </summary>
            public List<BookModel> Books;
            /// <summary>
            /// Модели посетителей
            /// </summary>
            public List<VisitorModel> Visitors;
            SendInfo ToView;
            public LibraryModel()
            {
                Books=new List<BookModel>();
                Visitors=new List<VisitorModel>();
            }
            // Проверка количества книг на руках у посетителя
            public bool IsBookLimitReached(int VisitorId)
            {
                return ((Visitors[VisitorId].GetBooksCount() >= 3));
            }
            void ThrowString(string s)
            {
                RefreshView("S:" + s);
            }
            public void IssueBook(int VisitorId, int BookId, System.DateTime CurrentDate)
            {
                VisitorId = Visitors.FindIndex(x => x.GetId() == VisitorId);
                BookId = Books.FindIndex(x => x.GetId() == BookId);
                if (IsBookLimitReached(VisitorId))
                {
                    ThrowString("Данный посетитель уже имеет максимально допустимое количество книг.");
                }
                else
                {
                    if (!Books[BookId].isAvailable())
                    {
                        ThrowString("Данная книга уже выдана.");
                    }
                    else
                    {
                        var Book = Books[BookId];
                        // Выдать на один месяц
                        Book.Issue(CurrentDate, CurrentDate.AddMonths(1));
                        Books[BookId] = Book;
                        Visitors[VisitorId].GiveBook(Book);
                        ThrowString("Книга успешно выдана.");
                    }
                }
            }
            public void ReceiveBook(int VisitorId, int BookId)
            {
                VisitorId = Visitors.FindIndex(x => x.GetId() == VisitorId);
                BookId = Books.FindIndex(x => x.GetId() == BookId);
                if (Visitors[VisitorId].TakeBackBook(Books[BookId]))
                {
                    var Book = Books[BookId];
                    Book.Recieve();
                    Books[BookId] = Book;
                    ThrowString("Книга успешно возвращена");
                }
                else
                {
                    ThrowString("Данная книга отсутствует у данного посетителя.");
                }
            }
            public void GetBookList()
            {
                string result = "";
                foreach (BookModel Book in Books)
                {
                    result += "B:"+Convert.ToString(Book.GetId())+"\t";
                }
                RefreshView(result);
            }
            public void GetVisitorList()
            {
                string result = "";
                foreach (VisitorModel Visitor in Visitors)
                {
                    result += "V:" + Visitor.GetId() + '\t';
                    foreach (BookModel Book in Visitor.GetBooks())
                    {
                        result += "B:" + Book.GetId() + '\t';
                    }
                }
                RefreshView(result);
            }
            public void GetExpiredBooks()
            {
                string result = "";
                foreach (VisitorModel Visitor in Visitors)
                {
                    string res_books = "";
                    foreach (BookModel Book in Visitor.GetBooks())
                    {
                        if (Book.GetExpiryDate() < System.DateTime.Today)
                        {
                            res_books += "B:" + Book.GetId() + '\t';
                        }
                    }
                    if (res_books != "")
                    {
                        result += "V:" + Visitor.GetId() + '\t' + res_books;
                    }
                }
                RefreshView(result);
            }
            public void RegisterDel(SendInfo Func)
            {
                ToView=Func;
            }
            public void RefreshView(string Result)
            {
                ToView(Result);
            }
            public void LoadFromISource(ISource Source, GetDelegate Book_Func, GetDelegate Visitor_Func)
            {
                ThrowString(Source.Ready ? "Источник готов" : "Источник не готов.");
                if (Source.Ready)
                {
                    foreach (ISource.Book Book in Source.Books)
                    {
                        Books.Add(new LibraryModel.BookModel(Book.Id, Book.Title, Book.Author, Book_Func()));
                    }
                    foreach (ISource.Visitor Visitor in Source.Visitors)
                    {
                        Visitors.Add(new LibraryModel.VisitorModel(Visitor.Id, Visitor.Name, Visitor_Func()));
                    }
                    foreach (ISource.Issue Issue in Source.Issues)
                    {
                        var Book = Books[Books.FindIndex(x=>x.GetId()==Issue.Book_Id)];
                        // Выдать на один месяц
                        Book.Issue(Issue.Issue_Date, Issue.Issue_Date.AddMonths(1));
                        Books[Books.FindIndex(x => x.GetId() == Issue.Book_Id)] = Book;
                        Visitors[Visitors.FindIndex(x => x.GetId() == Issue.Visitor_Id)].GiveBook(Book);
                    }
                }
            }
        }
        /// <summary>
        /// Контроллер
        /// </summary>
        class Controller
        {
            // Связь с моделью библиотеки
            LibraryModel Library;
            Viewer View;
            public Controller(ref LibraryModel Library)
            {
                this.Library=Library;
            }
            // Функции управления моделью
            public void AddVisitor(string Name, SendInfo Func)
            {
                Library.Visitors.Add(new LibraryModel.VisitorModel(Library.Visitors.Count, Name, Func));

            }
            public void AddBook(string Title, string Author, SendInfo Func)
            {
                Library.Books.Add(new LibraryModel.BookModel(Library.Books.Count, Title, Author, Func));

            }
            // Трансляция функций в бизнес-логику моделей
            public void IssueBook(int VisitorId, int BookId, System.DateTime Date)
            {
                Library.IssueBook(VisitorId, BookId, Date);
            }
            public void ReceiveBook(int VisitorId, int BookId)
            {
                Library.ReceiveBook(VisitorId, BookId);
            }
            public void GetBookList()
            {
                Library.GetBookList();
            }
            public void GetVisitorList()
            {
                Library.GetVisitorList();
            }
            public void GetExpiredBooks()
            {
                Library.GetExpiredBooks();
            }
            public void LoadLibrary(ISource Source, GetDelegate Book_Func, GetDelegate Visitor_Func)
            {
                Library.LoadFromISource(Source, Book_Func, Visitor_Func);
            }
            // Передача делегата для обратной связи
            public void RegisterDel(SendInfo Func)
            {
                Library.RegisterDel(Func);
            }
            // Передача ссылки на View в контроллер
            public void RegisterView(ref Viewer View)
            {
                this.View = View;
            }
            // Реакция на ввод пользователя
            public bool GetMenuOption()
            {
                char Key = Console.ReadKey(true).KeyChar;
                switch (Key)
                {
                    case '1':
                        {
                            View.ShowBooks();
                            break;
                        }
                    case '2':
                        {
                            View.ShowVisitors();
                            break;
                        }
                    case '3':
                        {
                            View.IssueBook();
                            break;
                        }
                    case '4':
                        {
                            View.ReceiveBook();
                            break;
                        }
                    case '5':
                        {
                            View.ShowExpiredBooks();
                            break;
                        }
                    case '6':
                        {
                            Console.Clear();
                            break;
                        }
                    case '7':
                        {
                            Console.WriteLine(greeting);
                            break;
                        }
                    case '8':
                        {
                            return true;
                        }
                    default:
                        {
                            Console.WriteLine("Wrong menu option.");
                            break;
                        }
                }
                Console.WriteLine();
                return false;
            }
            public int GetId()
            {
                return Convert.ToInt32(Console.ReadLine());
            }
            public void GetSource()
            {
                char Key = Console.ReadKey(true).KeyChar;
                switch (Key)
                {
                    case '1':
                        {
                            Console.WriteLine("Hardcoded Library 1 has been chosen.");
                            View.LoadLibrary(0);
                            break;
                        }
                    case '2':
                        {
                            Console.WriteLine("Hardcoded Library 2 has been chosen.");
                            View.LoadLibrary(1);
                            break;
                        }
                    case '3':
                        {
                            Console.WriteLine("TextFile Library has been chosen.");
                            View.LoadLibrary(2);
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Hardcoded Library 1 has been chosen by default.");
                            View.LoadLibrary(0);
                            break;
                        }
                }
            }
        }
        /// <summary>
        /// Общий view
        /// </summary>
        class Viewer
        {
            /// <summary>
            /// View книги
            /// </summary>
            class BookView
            {
                public BookView()
                {
                    LongInfo = "";
                    ShortInfo = "";
                }
                string LongHeader = "Id Title               \tAuthor      \tAvailable\tIssue      Expire\r\n";
                string ShortHeader = "Book: ";
                string LongInfo;
                string ShortInfo;
                public int Id;
                public void RefreshState(string Info)
                {
                    if (Info.Substring(0, 3) == "LI:")
                    {
                        SetLongInfo(Info.Substring(3));
                        Id = Convert.ToInt32(Info.Substring(3, Info.Substring(3).IndexOf(" ")));
                    }
                    else
                    {
                        if (Info.Substring(0, 3) == "SI:")
                            SetShortInfo(Info.Substring(3));
                        else
                        {
                            Console.WriteLine("Ошибка обновления модели книги");
                        }
                    }
                }
                public void SetLongInfo(string Info)
                {
                    LongInfo = Info;
                }
                public void SetShortInfo(string Info)
                {
                    ShortInfo = Info;
                }
                public void View(bool LongFormat = true, bool WithHeader = true)
                {
                    if (LongFormat)
                    {
                        Console.Write((WithHeader) ? LongHeader+"\n" : "");
                        Console.WriteLine(LongInfo);
                    }
                    else
                    {
                        Console.Write((WithHeader) ? ShortHeader : "");
                        Console.WriteLine(ShortInfo);
                    }
                }
            }
            /// <summary>
            /// View Посетителя
            /// </summary>
            class VisitorView
            {
                public VisitorView()
                {
                    Info = "";
                }
                string Header = "Id\tName\r\n";
                string Info;
                public int Id;
                public void RefreshState(string Info)
                {
                    this.Info = Info;
                    Id=Convert.ToInt32(Info.Substring(0,Info.IndexOf('\t')));
                }
                public void View(bool WithHeader = true)
                {
                    Console.Write((WithHeader) ? Header+"\n" : "");
                    Console.WriteLine(Info);
                }
            }
            List<BookView> Books;
            List<VisitorView> Visitors;
            Controller LibraryController;
            // Ответ от модели
            string[] result;
            List<ISource> Sources;
            public Viewer(ref Controller LibraryController, List<ISource> Sources)
            {
                this.LibraryController = LibraryController;
                Books = new List<BookView>();
                Visitors = new List<VisitorView>();
                LibraryController.RegisterDel(RefreshState);
                this.Sources = Sources;
            }
            void AddBook(string Title, string Author)
            {
                Books.Add(new BookView());
                LibraryController.AddBook(Title, Author, (SendInfo)Books[Books.Count - 1].RefreshState);
            }
            SendInfo AddBook()
            {
                Books.Add(new BookView());
                return (SendInfo)Books[Books.Count - 1].RefreshState;
            }
            void AddVisitor(string Name)
            {
                Visitors.Add(new VisitorView());
                LibraryController.AddVisitor(Name, (SendInfo)Visitors[Visitors.Count - 1].RefreshState);
            }
            SendInfo AddVisitor()
            {
                Visitors.Add(new VisitorView());
                return (SendInfo)Visitors[Visitors.Count - 1].RefreshState;
            }
            void InitLibrary()
            {
                Console.WriteLine("Choose Source:\n1 - Hardcoded L1\n2 - Hardcoded L2\n3 - TextFile");
                LibraryController.GetSource();
                Console.WriteLine(GetStringFromResult());
                Console.WriteLine();
            }
            // Загрузка библиотеки с источника. Передаются функции, возвращающие делегат для обратной связи.
            public void LoadLibrary(int source_id)
            {
                LibraryController.LoadLibrary(Sources[source_id], AddBook, AddVisitor);
            }
            // Главный цикл
            public void StartModel()
            {
                InitLibrary();
                bool exit=false;
                while (!exit)
                {
                    ShowMenu();
                    exit = LibraryController.GetMenuOption();
                }
            }
            void ShowMenu()
            {
                Console.WriteLine("1 - Show list of books" + "\n" +
                                  "2 - Show list of visitors" + "\n" +
                                  "3 - Issue a book" + "\n" +
                                  "4 - Receive a book" + "\n" +
                                  "5 - Show expired books" + "\n" +
                                  "6 - Clear Screen" + "\n" +
                                  "7 - Show Info" + "\n" +
                                  "8 - Exit");
            }
            public void ShowVisitors()
            {
                Console.WriteLine("Visitors:");
                LibraryController.GetVisitorList();
                List<Object> Result = ParseResult();
                bool first = true;
                foreach (Object V in Result)
                {
                    if (V.GetType() == typeof(VisitorView))
                    {
                        Console.WriteLine();
                        if (first)
                        {
                            VisitorView V_Converted = (VisitorView)V;
                            V_Converted.View();
                            first = false;
                        }
                        else
                        {
                            VisitorView V_Converted = (VisitorView)V;
                            V_Converted.View(false);
                        }
                    }
                    else
                    {
                        if (V.GetType() == typeof(BookView))
                        {
                            BookView V_Converted = (BookView)V;
                            V_Converted.View(false);
                        }
                        else
                        {
                            if (V.GetType() == typeof(string))
                            {
                                Console.WriteLine((string)V);
                                return;
                            }
                        }
                    }
                }
                Console.WriteLine((first) ? "None":"");
            }
            public void ShowBooks()
            {
                LibraryController.GetBookList();
                List<Object> Result = ParseResult();
                bool first = true;
                foreach (Object V in Result)
                {
                    if (V.GetType() == typeof(BookView))
                    {
                        if (first)
                        {
                            BookView V_Converted = (BookView)V;
                            V_Converted.View();
                            first = false;
                        }
                        else
                        {
                            BookView V_Converted = (BookView)V;
                            V_Converted.View(true,false);
                        }
                    }
                    else
                    {
                        if (V.GetType() == typeof(string))
                        {
                            Console.WriteLine((string)V);
                            return;
                        }
                    }
                }
            }
            public void IssueBook()
            {
                Console.Write("Enter visitor's id: ");
                int V_Id = LibraryController.GetId();
                Console.WriteLine();
                if ((V_Id >= 0) && (V_Id < Visitors.Count))
                {
                    Console.Write("Enter book's id: ");
                    int B_Id = LibraryController.GetId();
                    Console.WriteLine();
                    if ((B_Id >= 0) && (B_Id < Books.Count))
                    {
                        LibraryController.IssueBook(V_Id, B_Id,System.DateTime.Today);
                        Console.WriteLine(GetStringFromResult());
                    }
                    else
                    {
                        Console.WriteLine("Wrong book's id.");
                    }
                }
                else
                {
                    Console.WriteLine("Wrong visitor's id.");
                }
            }
            public void ReceiveBook()
            {
                Console.Write("Enter visitor's id: ");
                int V_Id = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine();
                if ((V_Id >= 0) && (V_Id < Visitors.Count))
                {
                    Console.Write("Enter book's id: ");
                    int B_Id = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine();
                    if ((B_Id >= 0) && (B_Id < Books.Count))
                    {
                        LibraryController.ReceiveBook(V_Id, B_Id);
                        Console.WriteLine(GetStringFromResult());
                    }
                    else
                    {
                        Console.WriteLine("Wrong book's id.");
                    }
                }
                else
                {
                    Console.WriteLine("Wrong visitor's id.");
                }
            }
            public void ShowExpiredBooks()
            {
                Console.WriteLine("Visitors with expired books:");
                LibraryController.GetExpiredBooks();
                List<Object> Result = ParseResult();
                bool first = true;
                foreach (Object V in Result)
                {
                    if (V.GetType() == typeof(VisitorView))
                    {
                        Console.WriteLine();
                        if (first)
                        {
                            VisitorView V_Converted = (VisitorView)V;
                            V_Converted.View();
                            first = false;
                        }
                        else
                        {
                            VisitorView V_Converted = (VisitorView)V;
                            V_Converted.View(false);
                        }
                    }
                    else
                    {
                        if (V.GetType() == typeof(BookView))
                        {
                            BookView V_Converted = (BookView)V;
                            V_Converted.View(false);
                        }
                        else
                        {
                            if (V.GetType() == typeof(string))
                            {
                                Console.WriteLine((string)V);
                                return;
                            }
                        }
                    }
                }
                Console.WriteLine((first) ? "None" : "");
            }
            // Чтение обратной связи
            void RefreshState(string Info)
            {
                result = Info.Split('\t');
            }
            // Парсинг данных от модели в виде B:Id \ V:Id \ S:"..."
            List<Object> ParseResult()
            {
                List<Object> Result = new List<Object>();
                foreach (string str in result)
                {
                    switch ((str.Length>0) ? str[0]:'D')
                    {
                        case 'B':
                            {
                                int id = Convert.ToInt32(str.Substring(2));
                                if ((id >= 0) && (id < Books.Count))
                                {
                                    Result.Add(Books[Books.FindIndex(x=>x.Id==id)]);
                                }
                                break;
                            }
                        case 'S':
                            {
                                Result.Add(str.Substring(2));
                                return Result;
                            }
                        case 'V':
                            {
                                int id = Convert.ToInt32(str.Substring(2));
                                if ((id >= 0) && (id < Visitors.Count))
                                {
                                    Result.Add(Visitors[Visitors.FindIndex(x=>x.Id==id)]);
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                return Result;
            }
            // Реакиця view на ответ модели в виде строки
            string GetStringFromResult()
            {
                List<Object> Result = ParseResult();
                if (Result.Count > 0)
                {
                    return (Result[0].GetType() == typeof(string)) ? (string)Result[0] : "";
                }
                else
                {
                    return "";
                }
            }
        }
        static void ShowGreeting()
        {
            string stars = "";
            for (int j = 0; j < 80; j++)
            {
                stars += "*";

            }
            for (int i = 0; i < 60; i++)
            {
                Console.WriteLine(stars);
                System.Threading.Thread.Sleep(1);
            }
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(greeting.Substring(i * (greeting.Length / 5), (greeting.Length / 5)));
                System.Threading.Thread.Sleep(200);
            }
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(stars);
                System.Threading.Thread.Sleep(200);
            }
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(stars);
                System.Threading.Thread.Sleep(1);
            }
            for (int i = 0; i < 120; i++)
            {
                Console.WriteLine();
                System.Threading.Thread.Sleep(5);
            }
        }
        static void Main(string[] args)
        {
            ShowGreeting();
            Console.Clear();
            List<ISource> Sources = new List<ISource>();
            HardcodedSource L1 = new HardcodedSource();
            {
                L1.AddBook("Time Machine      ", "H. G. Wells");
                L1.AddBook("Brave New World   ", "A. Huxley");
                L1.AddBook("The Air Seller    ", "A. Beliaev");
                L1.AddBook("Ariel             ", "A. Beliaev");
                L1.AddBook("Montezuma's Dauter", "H. R. Haggard");
                L1.AddBook("Around the Moon", "J. G. Verne");
                L1.AddVisitor("Maxim Nesterov");
                L1.AddVisitor("Mihail Yahlakov");
                L1.AddVisitor("Boris Stepanov");
                L1.AddIssue(0, 0, DateTime.Today.AddMonths(-10));
                L1.AddIssue(0, 4, DateTime.Today);
                L1.AddIssue(1, 3, DateTime.Today.AddMonths(-2));
                L1.Ready = true;
            }
            Sources.Add(L1);
            HardcodedSource L2 = new HardcodedSource();
            {
                L2.AddBook("War and Peace      ", "L. Tolstoy");
                L2.AddBook("Children's Book    ", "B. Akunin");
                L2.AddBook("We                 ", "E. Zamyatin");
                L2.AddBook("A Scandal in Bohemia", "A. C. Doyle");
                L2.AddBook("Harry Potter Series", "J. K. Rowling");
                L2.AddBook("Amphibian Man      ", "A. Beliaev");
                L2.AddVisitor("Maxim Nesterov");
                L2.AddVisitor("Mihail Yahlakov");
                L2.AddVisitor("Boris Stepanov");
                L2.AddIssue(0, 2, System.DateTime.Today);
                L2.AddIssue(1, 3, System.DateTime.Today);
                L2.AddIssue(2, 1, System.DateTime.Today.AddMonths(-5));
                L2.Ready = true;
            }
            Sources.Add(L2);
            Console.WriteLine("Type path for textfile source(or leave empty for 1.txt):");
            string path = Console.ReadLine();
            TextFileSource F1 = new TextFileSource(path.Length == 0 ? "1.txt" : path);
            Sources.Add(F1);
            LibraryModel Lib1 = new LibraryModel();
            Controller Cntr1 = new Controller(ref Lib1);
            Viewer LibViewer=new Viewer(ref Cntr1, Sources);
            Cntr1.RegisterView(ref LibViewer);
            LibViewer.StartModel();
        }
    }
}
