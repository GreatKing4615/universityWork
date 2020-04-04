using ASMProdWell.Dao;
using ASMProdWell.Security;
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
using System.Windows.Shapes;

namespace ASMProdWell
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        MainWindow mainWindow;
        public Authorization(MainWindow mainWindow)
        {
			InitializeComponent();
            this.mainWindow = mainWindow;
            this.Top = (SystemParameters.WorkArea.Height - this.Height) / 2;
            this.Left = (SystemParameters.WorkArea.Width - this.Width) / 2;
		}

		private void Button_Click_OK(Object sender, RoutedEventArgs e)
        {
            using (PersistanceContext db = new PersistanceContext())
            {
                List<User> users = db.Users.Where(u => u.Login.Equals(Login.Text)).ToList();
            /////////////////////////////////////////////////
            ///Проверка правильности ввода логина и пароля///
                /////////////////////////////////////////////////

                // По результатам проверки - вывод
                if (users.Count == 0 || !users[0].Password.Equals(Password.Password))
                {
                    MessageBox.Show("Неверно введен логин или пароль!", "Ошибка авторизации!");
                    
                }
                else
                {
                    User CurrentUser = users[0]; ;
                    mainWindow.CurrentUser = CurrentUser;
                    bool isEngineer = CurrentUser.Role.Equals("engineer");
                    mainWindow.AddPcpButton.IsEnabled = isEngineer;
                    mainWindow.AddEsnButton.IsEnabled = isEngineer;
                    mainWindow.EditPcpButton.IsEnabled = isEngineer;
                    mainWindow.EditEsnButton.IsEnabled = isEngineer;
                    mainWindow.Show();
                    Close();
                }
            }
             
        }

        private void Button_Click_Cansel(Object sender, RoutedEventArgs e)
        {
            mainWindow.Close();
            Close();
        }

        private void Window_Closing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!mainWindow.Activate())
                mainWindow.Close();
        }
    }
}
