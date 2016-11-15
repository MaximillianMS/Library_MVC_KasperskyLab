using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library_MVC_KasperskyLab
{
    class Program
    {
        /// <summary>
        /// Уровень модели (Книга)
        /// </summary>
        class BookModel
        {
            int Id;
            bool IsAvailable;
            string Title;
            string Author;
            DateTime IssueDate;
            DateTime ExpiryDate;
            public BookModel(int id, string title, string author)
            {
                this.Id = id;
                this.Title = title;
                this.Author = author;
                IsAvailable = true;
                IssueDate = System.DateTime.Today;
                ExpiryDate = System.DateTime.Today;
            }
            public void Issue(System.DateTime issueDate, System.DateTime expiryDate)
            {
                IsAvailable = false;
                this.IssueDate = issueDate;
                this.ExpiryDate = expiryDate;
            }
            public void Recieve()
            {
                IsAvailable = true;
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
        class VisitorModel
        {
            public VisitorModel(int v_id, string v_name)
            {
                Books = new List<BookModel>();
                Id = v_id;
                Name = v_name;
            }
            public int Id;
            public string Name;
            public List<BookModel> Books;
            public void GiveBook(BookModel book)
            {
                Books.Add(book);
            }
            public bool TakeBackBook(BookModel book)
            {
                return Books.Remove(book);
            }
            public string GetInfo()
            {
                return Id + "\t" + Name + "\n";
            }
        }
        /// <summary>
        /// Уровень Бизнес-Логики (контроллер)
        /// </summary>
        class BusinessLogic
        {
            /// <summary>
            /// Модели книг
            /// </summary>
            List<BookModel> Books;
            /// <summary>
            /// Модели посетителей
            /// </summary>
            List<VisitorModel> Visitors;
            public BusinessLogic()
            {
                Books=new List<BookModel>();
                Visitors=new List<VisitorModel>();
            }
            // Проверка количества книг на руках у посетителя
            public bool IsBookLimitReached(int VisitorId)
            {
                return ((Visitors[VisitorId].Books.Count >= 3));
            }
            public string IssueBook(int VisitorId, int BookId, System.DateTime CurrentDate)
            {
                if (IsBookLimitReached(VisitorId))
                {
                    return "Данный посетитель уже имеет максимально допустимое количество книг.";
                }
                else
                {
                    if (!Books[BookId].isAvailable())
                    {
                        return "Данная книга уже выдана.";
                    }
                    else
                    {
                        var Book = Books[BookId];
                        // Выдать на один месяц
                        Book.Issue(CurrentDate, CurrentDate.AddMonths(1));
                        Books[BookId] = Book;
                        Visitors[VisitorId].Books.Add(Book);
                        return "Книга успешно выдана.";
                    }
                }
            }
            public string ReceiveBook(int VisitorId, int BookId)
            {
                if (Visitors[VisitorId].TakeBackBook(Books[BookId]))
                {
                    var Book = Books[BookId];
                    Book.Recieve();
                    Books[BookId] = Book;
                    return "Книга успешно возвращена";
                }
                else
                {
                    return "Данная книга отсутствует у данного посетителя.";
                }
            }
            public string GetBookList()
            {
                string books = "";
                foreach (BookModel Book in Books)
                {
                    books += Book.GetLongInfo();
                }
                return books;
            }
            public string GetVisitorList()
            {
                string visitors = "";
                foreach (VisitorModel Visitor in Visitors)
                {
                    visitors += Visitor.GetInfo();
                    foreach (BookModel Book in Visitor.Books)
                    {
                        visitors += "Book: " + Book.GetShortInfo()+"\r\n";
                    }
                    visitors += "\r\n";
                }
                return visitors;
            }
            public string GetExpiredBooks()
            {
                string visitors = "";
                foreach (VisitorModel Visitor in Visitors)
                {
                    string books="";
                    foreach (BookModel Book in Visitor.Books)
                    {
                        if (Book.GetExpiryDate() < System.DateTime.Today)
                        {
                            books += "Book: " + Book.GetShortInfo()+"\r\n";
                        }
                    }
                    if (books != "")
                    {
                        visitors += Visitor.GetInfo() + books+"\r\n";
                    }
                }
                return visitors;
            }
            public void AddVisitor(string Name)
            {
                Visitors.Add(new VisitorModel(Visitors.Count,Name));
            }
            public void AddBook(string Title, string Author)
            {
                Books.Add(new BookModel(Books.Count, Title, Author));
            }
            
        }
        /// <summary>
        /// Презентационная логика
        /// </summary>
        class Viewer
        {
            BusinessLogic Library;
            int VisitorsCount;
            int BooksCount;
            public Viewer()
            {
                Library = new BusinessLogic();
                InitLibrary();
            }
            void InitLibrary()
            {
                Library.AddBook("Time Machine      ", "H. G. Wells");
                Library.AddBook("Brave New World   ", "A. Huxley");
                Library.AddBook("The Air Seller    ", "A. Beliaev");
                Library.AddBook("Ariel             ", "A. Beliaev");
                Library.AddBook("Montezuma's Dauter", "H. R. Haggard");
                Library.AddBook("Around the Moon", "J. G. Verne");
                Library.AddVisitor("Maxim Nesterov");
                Library.AddVisitor("Mihail Yahlakov");
                Library.AddVisitor("Boris Stepanov");
                VisitorsCount = 3;
                BooksCount = 6;
                Library.IssueBook(0, 0, DateTime.Today.AddMonths(-10));
                Library.IssueBook(0, 4, DateTime.Today);
                Library.IssueBook(1, 3, DateTime.Today.AddMonths(-2));
            }
            public void StartModel()
            {
                bool exit=false;
                while (!exit)
                {
                    ShowMenu();
                    char Key = Console.ReadKey(true).KeyChar;
                    Console.WriteLine();
                    switch (Key)
                    {
                        case '1':
                            {
                                ShowBooks();
                                break;
                            }
                        case '2':
                            {
                                ShowVisitors();
                                break;
                            }
                        case '3':
                            {
                                IssueBook();
                                break;
                            }
                        case '4':
                            {
                                ReceiveBook();
                                break;
                            }
                        case '5':
                            {
                                ShowExpiredBooks();
                                break;
                            }
                        case '6':
                            {
                                exit=true;
                                break;
                            }
                        default:
                            {
                                Console.WriteLine("Wrong menu option.");
                                break;
                            }
                    }
                    Console.WriteLine();
                }
            }
            void ShowMenu()
            {
                Console.WriteLine("1 - Show list of books" + "\n" +
                                  "2 - Show list of visitors" + "\n" +
                                  "3 - Issue a book" + "\n" +
                                  "4 - Receive a book" + "\n" +
                                  "5 - Show expired books" + "\n" +
                                  "6 - Exit" + "\n");
            }
            void ShowVisitors()
            {
                Console.WriteLine("Id\tVisitor\r");
                Console.WriteLine(Library.GetVisitorList());
            }
            void ShowBooks()
            {
                Console.Write("Id Title               \tAuthor      \tAvailable\tIssue      Expire\r\n");
                Console.WriteLine(Library.GetBookList());
            }
            void IssueBook()
            {
                Console.Write("Enter visitor's id: ");
                int V_Id = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine();
                if ((V_Id >= 0) && (V_Id < VisitorsCount))
                {
                    Console.Write("Enter book's id: ");
                    int B_Id = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine();
                    if ((B_Id >= 0) && (B_Id < BooksCount))
                    {
                        Console.WriteLine(Library.IssueBook(V_Id, B_Id,System.DateTime.Today));
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
            void ReceiveBook()
            {
                Console.Write("Enter visitor's id: ");
                int V_Id = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine();
                if ((V_Id >= 0) && (V_Id < VisitorsCount))
                {
                    Console.Write("Enter book's id: ");
                    int B_Id = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine();
                    if ((B_Id >= 0) && (B_Id < BooksCount))
                    {
                        Console.WriteLine(Library.ReceiveBook(V_Id, B_Id));
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
            void ShowExpiredBooks()
            {
                Console.WriteLine("Id\tName");
                string books = Library.GetExpiredBooks();
                Console.WriteLine(books == "" ? "None" : books);
            }
        }
        static void Main(string[] args)
        {
            string greeting = "\t\t\t*********************************\n" +
                              "\t\t\t*      Система учета книг       *\n" +
                              "\t\t\t*        в библиотеке           *\n" +
                              "\t\t\t*       Максим Нестеров         *\n" +
                              "\t\t\t*********************************\n";
            Console.WriteLine(greeting);
            Viewer LibViewer=new Viewer();
            LibViewer.StartModel();
        }
    }
}
