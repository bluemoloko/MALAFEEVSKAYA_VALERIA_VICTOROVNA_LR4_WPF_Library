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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = pbPassword.Password;

            if (login.Length == 0 || password.Length == 0)
            {
                MessageBox.Show("Введите логин и пароль.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new LibraryEntities())
                {
                    string hash = Security.Hash(password);


                    var user = db.Users.FirstOrDefault(u => u.user_login == login &&
                                                            (u.user_password == hash || u.user_password == password));
                    if (user == null)
                    {
                        MessageBox.Show("Неверный логин или пароль.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (user.user_role == "Librarian")
                    {
                        var lib = db.Librarians.FirstOrDefault(l => l.user_id == user.user_id);
                        string displayName;
                        int? librarianId = lib?.librarian_id;
                        if (lib != null)
                        {
                            displayName = lib.full_name;
                        }
                        else
                        {
                            displayName = user.user_login;
                        }

                        Session.Login(
                            user.user_id,
                            "Librarian",
                            displayName,
                            null,
                            librarianId
                        );
                        NavigationService.Navigate(new BookCatalogPage());
                    }
                    else
                    {
                        var reader = db.Readers.FirstOrDefault(r => r.user_id == user.user_id);
                        if (reader == null)
                        {
                            MessageBox.Show("Для этого пользователя не найден профиль читателя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        Session.Login(user.user_id, "Reader", reader.full_name, reader.reader_id, null);
                        NavigationService.Navigate(new BookCatalogPage());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к базе данных:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new RegistrationPage());

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            Session.Logout();
            NavigationService.Navigate (new BookCatalogPage());
        }
    }
}
