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
    /// Логика взаимодействия для BookCatalogPage.xaml
    /// </summary>
    public partial class BookCatalogPage : Page
    {
        private bool _ready;

        public BookCatalogPage()
        {
            InitializeComponent();
            if(Session.IsReader)
                btnReserve.Visibility = Visibility.Visible;
            else
                btnReserve.Visibility = Visibility.Collapsed;
            if(Session.IsReader)
                txtGuestHint.Visibility = Visibility.Visible;
            else txtGuestHint.Visibility = Visibility.Collapsed;
            if(Session.IsLibrarian)
                txtGuestHint.Visibility = Visibility.Collapsed;
            else
                txtGuestHint.Visibility= Visibility.Visible;

            try
            {
                LoadAuthors();
                _ready = true;
                LoadBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAuthors()
        {
            using (var db = new LibraryEntities())
            {
                var authors = db.Authors.OrderBy(a => a.author_name).ToList();
                authors.Insert(0, new Authors { author_id = 0, author_name = "Все авторы" });
                cbAuthor.ItemsSource = authors;
                cbAuthor.SelectedIndex = 0;
            }
        }

        private void LoadBooks()
        {
            if (!_ready) return;

            string search = (txtSearch.Text ?? "").Trim();
            int authorId;
            if (cbAuthor.SelectedValue is int id)
            {
                authorId = id;
            }
            else
            {
                authorId = 0;
            }

            try
            {
                using (var db = new LibraryEntities())
                {
                    var q = Queries.Books(db);

                    if (authorId > 0)
                        q = q.Where(b => b.AuthorId == authorId);
                    if (search.Length > 0)
                        q = q.Where(b => b.Title.Contains(search) || b.AuthorName.Contains(search));

                    dgBooks.ItemsSource = q.OrderBy(b => b.Title).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки книг:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadBooks();
        }
        private void CbAuthor_SelectionChanged(object sender, SelectionChangedEventArgs e) { 
            LoadBooks(); 
        }

        

        private void BtnReserve_Click(object sender, RoutedEventArgs e)
        {
            if (!Session.IsReader)
            {
                MessageBox.Show("Бронировать книги могут только авторизованные читатели.", "Нет доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var book = dgBooks.SelectedItem as BookView;
            if (book == null)
            {
                MessageBox.Show("Выберите книгу в таблице.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!book.IsAvailable)
            {
                MessageBox.Show("Эта книга уже забронирована или выдана.", "Недоступно", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NavigationService.Navigate(new BookingPage(book.BookId));
        }
    }
}
