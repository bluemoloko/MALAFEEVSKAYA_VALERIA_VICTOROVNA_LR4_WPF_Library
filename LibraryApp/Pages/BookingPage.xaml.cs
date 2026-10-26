using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LibraryApp.Entities;
using static System.Collections.Specialized.BitVector32;
using static LibraryApp.Models;

namespace LibraryApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для BookingPage.xaml
    /// </summary>
    public partial class BookingPage : Page
    {
        private readonly int? _preselectedBookId;

        public BookingPage() : this(null) { }

        public BookingPage(int? bookId)
        {
            InitializeComponent();
            _preselectedBookId = bookId;
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Session.IsReader)
            {
                MessageBox.Show("Страница доступна только читателям.", "Нет доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.Navigate(new LoginPage());
                return;
            }

            txtReader.Text = Session.FullName;
            txtDate.Text = DateTime.Today.ToString("dd.MM.yyyy");
            LoadBooks();
        }

        private void LoadBooks()
        {
            try
            {
                using (var db = new LibraryEntities())
                {
                    var available = Queries.Books(db).Where(b => b.IsAvailable).OrderBy(b => b.Title).ToList();
                    cbBook.ItemsSource = available;

                    if (_preselectedBookId.HasValue && available.Any(b => b.BookId == _preselectedBookId.Value))
                        cbBook.SelectedValue = _preselectedBookId.Value;
                    else if (available.Count > 0)
                        cbBook.SelectedIndex = 0;
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки книг:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CbBook_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var b = cbBook.SelectedItem as BookView;
            
        }

        private void BtnReserve_Click(object sender, RoutedEventArgs e)
        {
            if (!Session.IsReader || Session.ReaderId == null)
            {
                MessageBox.Show("Бронировать могут только читатели.", "Нет доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!(cbBook.SelectedValue is int bookId))
            {
                MessageBox.Show("Выберите книгу.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using (var db = new LibraryEntities())
                {
                    bool busy = db.Loans.Any(l => l.book_id == bookId && l.loan_status != "Returned");
                    if (busy)
                    {
                        MessageBox.Show("Эту книгу уже забронировали", "Недоступно", MessageBoxButton.OK, MessageBoxImage.Warning);
                        LoadBooks();
                        return;
                    }

                    db.Loans.Add(new Loans
                    {
                        book_id = bookId,
                        reader_id = Session.ReaderId.Value,
                        loan_date = DateTime.Today,
                        loan_status = "Reserved"
                    });
                    db.SaveChanges();
                }

                MessageBox.Show("Книга успешно забронирована", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new MyBookingsPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось забронировать книгу\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCatalog_Click(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new BookCatalogPage());
        }
        
    }
}
