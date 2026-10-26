using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using LibraryApp.Entities;

namespace LibraryApp
{
    internal class Models
    {
        public static class Session
        {
            public static int UserId { get; private set; }
            public static string Role { get; private set; }          
            public static string FullName { get; private set; }
            public static int? ReaderId { get; private set; }
            public static int? LibrarianId { get; private set; }

            public static bool IsAuth => Role != null;
            public static bool IsReader => Role == "Reader";
            public static bool IsLibrarian => Role == "Librarian";

            public static event Action Changed;

            public static void Login(int userId, string role, string fullName, int? readerId, int? librarianId)
            {
                UserId = userId; Role = role; FullName = fullName;
                ReaderId = readerId; LibrarianId = librarianId;
                Changed?.Invoke();
            }

            public static void Logout()
            {
                UserId = 0; Role = null; FullName = null;
                ReaderId = null; LibrarianId = null;
                Changed?.Invoke();
            }
        }

        public static class Security
        {
            public static string Hash(string password)
            {
                using (var sha = SHA256.Create())
                {
                    var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                    return string.Concat(bytes.Select(b => b.ToString("x2")));
                }
            }
        }

        public static class StatusHelper
        {
            public static string ToRu(string status)
            {
                switch (status)
                {
                    case "Reserved": return "Забронировано";
                    case "Issued": return "Выдано";
                    case "Returned": return "Возвращено";
                    case "Overdue": return "Просрочено";
                    default: return status;
                }
            }
        }


        public class BookView
        {
            public int BookId { get; set; }
            public string Title { get; set; }
            public int Year { get; set; }
            public int AuthorId { get; set; }
            public string AuthorName { get; set; }
            public string Country { get; set; }
            public bool IsAvailable { get; set; }

            public string StatusText => IsAvailable ? "Доступна" : "Забронирована / выдана";
            public string Display => Title + " — " + AuthorName;
        }


        public class LoanView
        {
            public int LoanId { get; set; }
            public int BookId { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public string ReaderName { get; set; }
            public string ReaderPhone { get; set; }
            public DateTime LoanDate { get; set; }
            public DateTime? ReturnDate { get; set; }
            public string Status { get; set; }

            public string StatusText => StatusHelper.ToRu(Status);
        }

        public static class Queries
        {
            
            public static IQueryable<BookView> Books(LibraryEntities db)
            {
                return from b in db.Books
                       join a in db.Authors on b.author_id equals a.author_id
                       select new BookView
                       {
                           BookId = b.book_id,
                           Title = b.title,
                           Year = b.publish_year,
                           AuthorId = a.author_id,
                           AuthorName = a.author_name,
                           Country = a.country,
                           IsAvailable = !db.Loans.Any(l => l.book_id == b.book_id && l.loan_status != "Returned")
                       };
            }

            public static IQueryable<LoanView> Loans(LibraryEntities db)
            {
                return from l in db.Loans
                       join b in db.Books on l.book_id equals b.book_id
                       join a in db.Authors on b.author_id equals a.author_id
                       join r in db.Readers on l.reader_id equals r.reader_id
                       select new LoanView
                       {
                           LoanId = l.loan_id,
                           BookId = b.book_id,
                           Title = b.title,
                           AuthorName = a.author_name,
                           ReaderName = r.full_name,
                           ReaderPhone = r.phone,
                           LoanDate = l.loan_date,
                           ReturnDate = l.return_date,
                           Status = l.loan_status
                       };
            }
        }
    }
}
