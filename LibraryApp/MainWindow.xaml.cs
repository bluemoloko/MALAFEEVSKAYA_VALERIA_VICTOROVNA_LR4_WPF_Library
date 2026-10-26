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
using LibraryApp.Pages;
using static LibraryApp.Models;

namespace LibraryApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            Session.Changed += UpdateMenu;
            UpdateMenu();
            MainFrame.Navigate(new LoginPage());
        }
        private void UpdateMenu()
        {

            if (Session.IsReader)
            {
                btnBooking.Visibility = Visibility.Visible;
                btnMyBookings.Visibility = Visibility.Visible;
            }
            else
            {
                btnBooking.Visibility = Visibility.Collapsed;
                btnMyBookings.Visibility = Visibility.Collapsed;
            }

            if (Session.IsLibrarian)
            {
                btnAllBookings.Visibility = Visibility.Visible;
            }
            else
            {
                btnAllBookings.Visibility = Visibility.Collapsed;
            }

            if (Session.IsAuth)
            {
                btnLogin.Visibility = Visibility.Collapsed;
                btnLogout.Visibility = Visibility.Visible;
            }
            else
            {
                btnLogin.Visibility = Visibility.Visible;
                btnLogout.Visibility = Visibility.Collapsed;
            }



        }

        private void BtnCatalog_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new BookCatalogPage());
        private void BtnBooking_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new BookingPage());
        private void BtnMyBookings_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new MyBookingsPage());
        private void BtnAllBookings_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AllBookingsPage());
        private void BtnLogin_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new LoginPage());

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Session.Logout();
            MainFrame.Navigate(new LoginPage());
        }
    }
}
