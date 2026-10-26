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
    /// Логика взаимодействия для MyBookingsPage.xaml
    /// </summary>
    public partial class MyBookingsPage : Page
    {
        public MyBookingsPage()
        {
            InitializeComponent();
            Loaded += LoadedLoads;
        }
        private void LoadedLoads(object sender, RoutedEventArgs e)
        {
            LoadLoans();
        }
        private void LoadLoans()
        {
            try
            {
                int readerId = Session.ReaderId.Value;
                using (var db = new LibraryEntities())
                {
                    var rid = readerId;
                    dgLoans.ItemsSource = Queries.Loans(db)
                        .Where(l => db.Loans.Any(x => x.loan_id == l.LoanId && x.reader_id == rid))
                        .OrderByDescending(l => l.LoanDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки броней:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var loan = dgLoans.SelectedItem as LoanView;
            if (loan == null)
            {
                MessageBox.Show("Выберите бронь в таблице.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (loan.Status != "Reserved")
            {
                MessageBox.Show("Отменить можно только бронь со статусом «Забронировано».", "Нельзя отменить", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show($"Отменить бронь книги «{loan.Title}»?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new LibraryEntities())
                {
                    var entity = db.Loans.Find(loan.LoanId);
                    if (entity != null && entity.reader_id == Session.ReaderId && entity.loan_status == "Reserved")
                    {
                        db.Loans.Remove(entity);
                        db.SaveChanges();
                    }
                }
                LoadLoans();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось отменить бронь:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new BookingPage());
    }
}
