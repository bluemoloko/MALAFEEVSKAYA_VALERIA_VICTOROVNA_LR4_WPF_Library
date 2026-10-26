using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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
    /// Логика взаимодействия для AllBookingsPage.xaml
    /// </summary>
    public partial class AllBookingsPage : Page
    {
        private bool _ready;

        public AllBookingsPage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                if (!Session.IsLibrarian)
                {
                    MessageBox.Show("Данная страница доступна только для библиотекарей", "Нет доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                    NavigationService.Navigate(new LoginPage());
                    return;
                }
                _ready = true;
                LoadLoans();
            };
        }

        private void LoadLoans()
        {
            if (!_ready) return;

            string search = (txtSearch.Text).Trim();
            string status = "";
            if (cbStatus.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                status = tag;
            }

            try
            {
                using (var db = new LibraryEntities())
                {
                    var q = Queries.Loans(db);
                    if (status.Length > 0)
                        q = q.Where(l => l.Status == status);
                    if (search.Length > 0)
                        q = q.Where(l => l.ReaderName.Contains(search) || l.Title.Contains(search));

                    dgLoans.ItemsSource = q.OrderByDescending(l => l.LoanDate).ThenByDescending(l => l.LoanId).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки броней\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e) => LoadLoans();

        private LoanView GetSelected()
        {
            var loan = dgLoans.SelectedItem as LoanView;
            return loan;
        }

        private void ChangeStatus(string newStatus)
        {
            var loan = GetSelected();
            if (loan == null) return;

            try
            {
                using (var db = new LibraryEntities())
                {
                    var entity = db.Loans.Find(loan.LoanId);
                    if (entity == null) { LoadLoans(); return; }

                    entity.loan_status = newStatus;
                    entity.return_date = newStatus == "Returned" ? (DateTime?)DateTime.Today : null;
                    db.SaveChanges();
                }
                LoadLoans();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось изменить статус\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnIssue_Click(object sender, RoutedEventArgs e)
        {
            ChangeStatus("Issued");
        }
        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            ChangeStatus("Returned");
        }
        private void BtnOverdue_Click(object sender, RoutedEventArgs e)
        {
            ChangeStatus("Overdue");
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var loan = GetSelected();
            if (loan == null) return;

            if (MessageBox.Show($"Удалить бронь {loan.LoanId} {loan.Title}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new LibraryEntities())
                {
                    var entity = db.Loans.Find(loan.LoanId);
                    if (entity != null)
                    {
                        db.Loans.Remove(entity);
                        db.SaveChanges();
                    }
                }
                LoadLoans();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить бронь\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
