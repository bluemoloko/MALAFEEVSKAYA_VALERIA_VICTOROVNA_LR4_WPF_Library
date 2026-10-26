using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using static LibraryApp.Models;

namespace LibraryApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page
    {
        private const string LibrarianCode = "LIB2026";

        public RegistrationPage()
        {
            InitializeComponent();
        }

        private string SelectedRole
        {
            get
            {
                var item = cbRole.SelectedItem as ComboBoxItem;
                if (item.Tag is string role)
                {
                    return role;
                }

                return "Reader";
            }
        }

        private void CbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (panelCode == null) return;  
            bool lib = SelectedRole == "Librarian";
            panelCode.Visibility = lib ? Visibility.Visible : Visibility.Collapsed;
            panelAddress.Visibility = lib ? Visibility.Collapsed : Visibility.Visible;
        }

        private string CheckErrors()
        {
            string login = txtLogin.Text;
            if (login.Length < 3) 
                return "Логин должен содержать не менее 3 символов";
            if (pbPassword.Password.Length < 6) 
                return "Пароль должен содержать не менее 6 символов";
            if (txtFullName.Text.Trim().Length < 5) 
                return "Введите ФИО полностью";
            if (SelectedRole == "Librarian" && pbCode.Password != LibrarianCode) 
                return "Неверный код библиотекаря";
            return null;
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string error = CheckErrors();
            if (error != null)
            {
                MessageBox.Show(error, "Проверьте данные", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new LibraryEntities())
                {
                    string login = txtLogin.Text.Trim();
                    var user = new Users
                    {
                        user_login = login,
                        user_password = pbPassword.Password,
                        user_role = SelectedRole
                    };
                    db.Users.Add(user);
                    db.SaveChanges();   

                    if (SelectedRole == "Librarian")
                    {
                        db.Librarians.Add(new Librarians
                        {
                            user_id = user.user_id,
                            full_name = txtFullName.Text.Trim(),
                            phone = txtPhone.Text.Trim()
                        });
                    }
                    else
                    {
                        string addr = txtAddress.Text.Trim();
                        db.Readers.Add(new Readers
                        {
                            user_id = user.user_id,
                            full_name = txtFullName.Text.Trim(),
                            phone = txtPhone.Text.Trim(),
                            user_address = addr.Length == 0 ? null : addr
                        });
                    }
                    db.SaveChanges();
                }

                MessageBox.Show("Регистрация прошла успешно. Теперь войдите в систему.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new LoginPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось зарегистрироваться:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }
    }
}
