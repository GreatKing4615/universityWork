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
	public partial class UpdateESP : Window
	{
		private static MainWindow MainWindow;

		/// <summary>
		/// Выпранная ступень ЭЦН 
		/// </summary>
		private ElectricSubmersiblePump ChosenPump;

		public UpdateESP(MainWindow mainWindow, ElectricSubmersiblePump chosenPump)
		{
			InitializeComponent();
			ConditionalDimensionComboBox.ItemsSource = ElectricSubmersiblePump.DimensionDiameter.Keys.ToList();
			MainWindow = mainWindow;
			Top = MainWindow.Top + 50;
			Left = MainWindow.Left + 50;

			ChosenPump = new ElectricSubmersiblePump(chosenPump);

			ConditionalDimensionComboBox.SelectedIndex = ElectricSubmersiblePump.DimensionDiameter.Keys.ToList().IndexOf(chosenPump.ConditionalDimension);

			NameTB.Text = chosenPump.Name;
			TB_BaseFrequency.Text = chosenPump.BaseFrequency.ToString();
			TB_NominalDischarge.Text = chosenPump.NominalRate.ToString();
			TB_MinRecomendedDischarge.Text = chosenPump.MinRecomendedRate.ToString();
			TB_MaxRecomendedDischarge.Text = chosenPump.MaxRecomendedRate.ToString();
			TB_MinAvailableDischarge.Text = chosenPump.MinAvailableRate.ToString();
			TB_MaxAvailableDischarge.Text = chosenPump.MaxAvailableRate.ToString();
			SetHeadCoefficients();
			SetPowerCoefficients();
			SetEfficiencyCoefficients(); 
		}

		/// <summary>
		/// Отображение в окне коэффициентов напора
		/// </summary>
		private void SetHeadCoefficients()
		{
			for (int i = 0; i < ChosenPump.HeadCoefficients.Count; i++)
			{
				int id_textBox = ChosenPump.HeadCoefficients[i].Order + 1;
				string value = ChosenPump.HeadCoefficients[i].string_value;
				((TextBox)((StackPanel)HeadCoefficients.Children[id_textBox]).Children[1]).Text = value;
			}
		}

		/// <summary>
		/// Отображение в окне коэффициентов мощности
		/// </summary>
		private void SetPowerCoefficients()
		{
			for (int i = 0; i < ChosenPump.PowerCoefficients.Count; i++)
			{
				int id_textBox = ChosenPump.PowerCoefficients[i].Order + 1;
				string value = ChosenPump.PowerCoefficients[i].string_value;
				((TextBox)((StackPanel)PowerCoefficients.Children[id_textBox]).Children[1]).Text = value;
			}
		}

		/// <summary>
		/// Отображение в окне коэффициентов КПД
		/// </summary>
		private void SetEfficiencyCoefficients()
		{
			for (int i = 0; i < ChosenPump.EfficiencyCoefficients.Count; i++)
			{
				int id_textBox = ChosenPump.EfficiencyCoefficients[i].Order + 1;
				string value = ChosenPump.EfficiencyCoefficients[i].string_value;
				((TextBox)((StackPanel)EfficiencyCoefficients.Children[id_textBox]).Children[1]).Text = value;
			}
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
			TextBox[] HeadTextBoxes = {TB_HeadX0, TB_HeadX1, TB_HeadX2, TB_HeadX3, TB_HeadX4,
				TB_HeadX5, TB_HeadX6, TB_HeadX7, TB_HeadX8, TB_HeadX9};
			TextBox[] PowerTextBoxes = {TB_Power0, TB_Power1, TB_Power2, TB_Power3, TB_Power4,
				TB_Power5, TB_Power6, TB_Power7, TB_Power8, TB_Power9};
			TextBox[] EfficiencyTextBoxes = {TB_Efficiency0, TB_Efficiency1, TB_Efficiency2, TB_Efficiency3, TB_Efficiency4,
				TB_Efficiency5, TB_Efficiency6, TB_Efficiency7, TB_Efficiency8, TB_Efficiency9};

			ChosenPump.HeadCoefficients.Clear();
			ChosenPump.PowerCoefficients.Clear();
			ChosenPump.EfficiencyCoefficients.Clear();

			for (int i = 0; i < HeadTextBoxes.Count(); i++)
			{
				TextBox box = HeadTextBoxes[i];
				ChosenPump.HeadCoefficients.Add(new HeadCoefficient(i, double.Parse(box.Text)));
			}

			for (int i = 0; i < PowerTextBoxes.Count(); i++)
			{
				TextBox box = PowerTextBoxes[i];
				ChosenPump.PowerCoefficients.Add(new PowerCoefficient(i, double.Parse(box.Text)));
			}
			
			for (int i = 0; i < EfficiencyTextBoxes.Count(); i++)
			{

				TextBox box = EfficiencyTextBoxes[i];
				ChosenPump.EfficiencyCoefficients.Add(new EfficiencyCoefficient(i, double.Parse(box.Text)));
			}

			ChosenPump.Name = NameTB.Text;
			ChosenPump.BaseFrequency = Convert.ToDouble(TB_BaseFrequency.Text);
			ChosenPump.ConditionalDimension = ConditionalDimensionComboBox.Text;
			ChosenPump.NominalRate = Convert.ToDouble(TB_NominalDischarge.Text);
			ChosenPump.MinAvailableRate = Convert.ToDouble(TB_MinAvailableDischarge.Text);
			ChosenPump.MaxAvailableRate = Convert.ToDouble(TB_MaxAvailableDischarge.Text);
			ChosenPump.MinRecomendedRate = Convert.ToDouble(TB_MinRecomendedDischarge.Text);
			ChosenPump.MaxRecomendedRate = Convert.ToDouble(TB_MaxRecomendedDischarge.Text);

			this.Hide();


			using (PersistanceContext db = new PersistanceContext())
			{
				
				ElectricSubmersiblePump espForChange = db.EspPumps.Include("PowerCoefficients")
					.Include("EfficiencyCoefficients").Include("HeadCoefficients").Where(esp => esp.Id == ChosenPump.Id).ToList()[0];
				db.EspPumps.Remove(espForChange);
				espForChange = ChosenPump;
				db.EspPumps.Add(espForChange);
				db.SaveChanges();
				List<ElectricSubmersiblePump> espList = db.EspPumps.Include("PowerCoefficients")
					.Include("EfficiencyCoefficients").Include("HeadCoefficients").ToList();

				//Обновляем каталог
				MainWindow.CatalogEsp = espList;
				//Отображаем таблицу каталога ЭЦН
				MainWindow.ShowESPDataBase();
			}
			MainWindow.Button_Click_UpdateGrafESN(null, null);
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

