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
using System.Windows.Shapes;
using ASMProdWell.Components.Equipment.Pumps;
using ASMProdWell.Dao;

namespace ASMProdWell
{
    /// <summary>
    /// Логика взаимодействия для AddPCP.xaml
    /// </summary>
    public partial class AddPCP : Window
    {
        private MainWindow MainWindow;


		public AddPCP(MainWindow mainWindow)
        {
            InitializeComponent();
            //ConditionalDimensionComboBox.ItemsSource = ElectricSubmersiblePump.DimensionDiameter.Keys.ToList();
            MainWindow = mainWindow;
            Top = MainWindow.Top + 50;
            Left = MainWindow.Left + 50;
        }

        private void Window_Closing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            try
            {
                if (!MainWindow.IsActive)
                    e.Cancel = true;
            }
            catch { }
        }

		#region Общие ограничения ввода
		/// <summary>
		/// Проверка данных поля на соответстивия числам с плавающей точкой 
		/// </summary>
		private void PreviewTextInput_DigitalDouble(Object sender, TextCompositionEventArgs e)
		{
			if (!Char.IsDigit(e.Text, 0) && e.Text != ",")
			{
				try
				{
					if ((TextBox)sender is TextBox)
					{
						string Name = (((sender as TextBox).Parent as StackPanel).Parent as StackPanel).Name;
						if (Name == "RateCoefficients" || Name == "TorqueCoefficients" || Name == "PowerCoefficients")
						{
							if (e.Text == "E" || e.Text == "e" || e.Text == "-" || e.Text == "+")
								return;
						}
					}
				}
				catch { }
				e.Handled = true;
			}
		}

		/// <summary>
		/// Проверка данных поля на соответстивия целочисленным данных
		/// </summary>
		private void PreviewTextInput_DigitalInt(Object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Запрет на нажатие пробела
        /// </summary>
        private void PreviewKeyDown_BlocSpace(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }
        #endregion

        /// <summary>
        /// Кнопка добавить
        /// </summary>
        private void Button_Click_OK(Object sender, RoutedEventArgs e)
        {
			ProgressiveCavityPump newPump = new ProgressiveCavityPump();

			newPump.RateCoefficients = new List<RateCoefficient>();
			newPump.PowerCoefficients = new List<PowerCoefficient>();
			newPump.TorqueCoefficients = new List<TorqueCoefficient>();

			try
			{

				for (int i = 1; i < RateCoefficients.Children.Count; i++)
				{
					TextBox box = (TextBox)(RateCoefficients.Children[i] as StackPanel).Children[1];
					int order = i - 1;
					newPump.RateCoefficients.Add(new RateCoefficient(order, double.Parse(box.Text)));
				}

				for (int i = 1; i < PowerCoefficients.Children.Count; i++)
				{
					TextBox box = (TextBox)(PowerCoefficients.Children[i] as StackPanel).Children[1];
					int order = i - 1;
					newPump.PowerCoefficients.Add(new PowerCoefficient(order, double.Parse(box.Text)));
				}

				for (int i = 1; i < TorqueCoefficients.Children.Count; i++)
				{

					TextBox box = (TextBox)(TorqueCoefficients.Children[i] as StackPanel).Children[1];
					int order = i - 1;
					newPump.TorqueCoefficients.Add(new TorqueCoefficient(order, double.Parse(box.Text)));
				}

				newPump.Name = ((TextBox)NamePCP.Children[1]).Text;
				newPump.Diameter = Convert.ToDouble(((TextBox)Diameter.Children[1]).Text);
				newPump.NominalRate = Convert.ToDouble(((TextBox)NominalRate.Children[1]).Text);
				newPump.BaseSpeed = Convert.ToDouble(((TextBox)NominalSpeed.Children[1]).Text);
			}
			catch 
			{
				MessageBox.Show("Ошибка Неправильно задано одно из полей.");
				return;
			}
			this.Hide();
			//MainWindow.Button_Click_UpdateGrafESN(null, null);

			using (PersistanceContext db = new PersistanceContext())
			{
				db.PcpPumps.Add(newPump);
				db.SaveChanges();
				//Обновляем каталог
				MainWindow.CatalogPcp = db.PcpPumps.Include("PowerCoefficients").Include("RateCoefficients").Include("TorqueCoefficients").ToList();

				//Отображаем таблицу каталога ЭЦН
				MainWindow.ShowPCPDataBase();
			}
			MainWindow.Button_Click_UpdateGrafPCP(null, null);
		}

		/// <summary>
		/// Кнопка закрыть
		/// </summary>
		private void Button_Click_Cancel(Object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
