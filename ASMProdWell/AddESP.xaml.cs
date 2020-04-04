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
    /// Логика взаимодействия для AddESN.xaml
    /// </summary>
    public partial class AddESP : Window
    {
        private MainWindow MainWindow;

        public AddESP(MainWindow mainWindow)
        {
            
            InitializeComponent();
            ConditionalDimensionComboBox.ItemsSource = ElectricSubmersiblePump.DimensionDiameter.Keys.ToList();
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
                    if (((TextBox)sender is TextBox) && (Regex.IsMatch((sender as TextBox).Name, "TB_HeadX*") || Regex.IsMatch((sender as TextBox).Name, "TB_Efficiency*") || Regex.IsMatch((sender as TextBox).Name, "TB_Power*"))
                        && (e.Text == "E" || e.Text == "e" || e.Text == "-" || e.Text == "+"))
                        return;
                    if (((DataGrid)sender is DataGrid) && e.Text == ".")
                        return;
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
            ElectricSubmersiblePump pump = new ElectricSubmersiblePump();

            pump.HeadCoefficients = new List<HeadCoefficient>();
            pump.PowerCoefficients = new List<PowerCoefficient>();
            pump.EfficiencyCoefficients = new List<EfficiencyCoefficient>();

            TextBox[] HeadTextBoxes = {TB_HeadX0, TB_HeadX1, TB_HeadX2, TB_HeadX3, TB_HeadX4,
                TB_HeadX5, TB_HeadX6, TB_HeadX7, TB_HeadX8, TB_HeadX9};
            TextBox[] PowerTextBoxes = {TB_Power0, TB_Power1, TB_Power2, TB_Power3, TB_Power4,
                TB_Power5, TB_Power6, TB_Power7, TB_Power8, TB_Power9};
            TextBox[] EfficiencyTextBoxes = {TB_Efficiency0, TB_Efficiency1, TB_Efficiency2, TB_Efficiency3, TB_Efficiency4,
                TB_Efficiency5, TB_Efficiency6, TB_Efficiency7, TB_Efficiency8, TB_Efficiency9};

			try
			{
				for (int i = 0; i < HeadTextBoxes.Count(); i++)
				{
					TextBox box = HeadTextBoxes[i];
					pump.HeadCoefficients.Add(new HeadCoefficient(i, double.Parse(box.Text)));
				}

				for (int i = 0; i < PowerTextBoxes.Count(); i++)
				{
					TextBox box = PowerTextBoxes[i];
					pump.PowerCoefficients.Add(new PowerCoefficient(i, double.Parse(box.Text)));
				}

				for (int i = 0; i < EfficiencyTextBoxes.Count(); i++)
				{
					TextBox box = EfficiencyTextBoxes[i];
					pump.EfficiencyCoefficients.Add(new EfficiencyCoefficient(i, double.Parse(box.Text)));
				}
				pump.Name = NameTB.Text;
				pump.BaseFrequency = Convert.ToDouble(TB_BaseFrequency.Text);
				pump.ConditionalDimension = ConditionalDimensionComboBox.Text;
				pump.NominalRate = Convert.ToDouble(TB_NominalRate.Text);
				pump.MinAvailableRate = Convert.ToDouble(TB_MinAvailableRate.Text);
				pump.MaxAvailableRate = Convert.ToDouble(TB_MaxAvailableRate.Text);
				pump.MinRecomendedRate = Convert.ToDouble(TB_MinRecomendedRate.Text);
				pump.MaxRecomendedRate = Convert.ToDouble(TB_MaxRecomendedRate.Text);

			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка Неправильно задано одно из полей." );
				return;
			}
			this.Hide();
            MainWindow.Button_Click_UpdateGrafESN(null, null);

			using (PersistanceContext db = new PersistanceContext())
			{
				db.EspPumps.Add(pump);
				db.SaveChanges();
				//Обновляем каталог
				MainWindow.CatalogEsp = db.EspPumps.Include("PowerCoefficients")
					.Include("EfficiencyCoefficients").Include("HeadCoefficients").ToList();

				//Отображаем таблицу каталога ЭЦН
				MainWindow.ShowESPDataBase();
			}
			
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

