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
	/// Логика взаимодействия для UpdatePCP.xaml
	/// </summary>
	public partial class UpdatePCP : Window
	{
		private MainWindow MainWindow;

		
		/// <summary>
		/// Выпранная ступень ЭЦН 
		/// </summary>
		private ProgressiveCavityPump ChosenPump;

		public UpdatePCP(MainWindow mainWindow, ProgressiveCavityPump chosenPump)
		{
			InitializeComponent();
			MainWindow = mainWindow;
			Top = MainWindow.Top + 50;
			Left = MainWindow.Left + 50;

			ChosenPump = new ProgressiveCavityPump(chosenPump);
			((TextBox)NamePCP.Children[1]).Text = chosenPump.Name;
			((TextBox)Diameter.Children[1]).Text = chosenPump.Diameter.ToString();
			((TextBox)NominalRate.Children[1]).Text = chosenPump.NominalRate.ToString();
			((TextBox)BaseSpeed.Children[1]).Text = chosenPump.BaseSpeed.ToString();

			SetRateCoefficients();
			SetPowerCoefficients();
			SetTorqueCoefficients();
		}

		/// <summary>
		/// Отображение в окне коэффициентов напора
		/// </summary>
		private void SetRateCoefficients()
		{
			for (int i = 0; i < ChosenPump.RateCoefficients.Count; i++)
			{
				int id_textBox = ChosenPump.RateCoefficients[i].Order + 1;
				string value = ChosenPump.RateCoefficients[i].string_value;
				((TextBox)((StackPanel)RateCoefficients.Children[id_textBox]).Children[1]).Text = value;
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
		private void SetTorqueCoefficients()
		{
			for (int i = 0; i < ChosenPump.TorqueCoefficients.Count; i++)
			{
				int id_textBox = ChosenPump.TorqueCoefficients[i].Order + 1;
				string value = ChosenPump.TorqueCoefficients[i].string_value;
				((TextBox)((StackPanel)TorqueCoefficients.Children[id_textBox]).Children[1]).Text = value;
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
					if ((TextBox)sender is TextBox)
					{
						string Name = (((sender as TextBox).Parent as StackPanel).Parent as StackPanel).Name;
						if (Name == "RateCoefficients" || Name == "TorqueCoefficients" || Name == "PowerCoefficients")
						{   
							if(e.Text == "E" || e.Text == "e" || e.Text == "-" || e.Text == "+")
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
			ChosenPump.RateCoefficients = new List<RateCoefficient>();
			ChosenPump.PowerCoefficients = new List<PowerCoefficient>();
			ChosenPump.TorqueCoefficients = new List<TorqueCoefficient>();

			try
			{

				for (int i = 1; i < RateCoefficients.Children.Count; i++)
				{
					TextBox box = (TextBox)(RateCoefficients.Children[i] as StackPanel).Children[1];
					int order = i - 1;
					ChosenPump.RateCoefficients.Add(new RateCoefficient(order, double.Parse(box.Text)));
				}

				for (int i = 1; i < PowerCoefficients.Children.Count; i++)
				{
					TextBox box = (TextBox)(PowerCoefficients.Children[i] as StackPanel).Children[1];
					int order = i - 1;
					ChosenPump.PowerCoefficients.Add(new PowerCoefficient(order, double.Parse(box.Text)));
				}

				for (int i = 1; i < TorqueCoefficients.Children.Count; i++)
				{

					TextBox box = (TextBox)(TorqueCoefficients.Children[i] as StackPanel).Children[1];
					int order = i - 1;
					ChosenPump.TorqueCoefficients.Add(new TorqueCoefficient(order, double.Parse(box.Text)));
				}

				ChosenPump.Name = ((TextBox)NamePCP.Children[1]).Text;
				ChosenPump.Diameter = Convert.ToDouble(((TextBox)Diameter.Children[1]).Text);
				ChosenPump.NominalRate = Convert.ToDouble(((TextBox)NominalRate.Children[1]).Text);
				ChosenPump.BaseSpeed = Convert.ToDouble(((TextBox)BaseSpeed.Children[1]).Text);
			}
			catch
			{
				MessageBox.Show("Ошибка Неправильно задано одно из полей.");
				return;
			}

			this.Hide();


			using (PersistanceContext db = new PersistanceContext())
			{

				ProgressiveCavityPump pcpForChange = db.PcpPumps.Include("PowerCoefficients").Include("RateCoefficients")
											.Include("TorqueCoefficients").Where(pcp => pcp.Id == ChosenPump.Id).ToList()[0];
				db.PcpPumps.Remove(pcpForChange);
				pcpForChange = ChosenPump;
				db.PcpPumps.Add(pcpForChange);
				db.SaveChanges();
				List<ProgressiveCavityPump> pcpList = db.PcpPumps.Include("PowerCoefficients").Include("RateCoefficients")
											.Include("TorqueCoefficients").ToList();

				//Обновляем каталог
				MainWindow.CatalogPcp = pcpList;
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
