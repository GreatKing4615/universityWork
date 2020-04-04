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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ASMProdWell.Components;
using ASMProdWell.Components.Fluids;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text.RegularExpressions;
using System.Drawing;
using System.ComponentModel;
using ASMProdWell.Components.Pipes;
using ASMProdWell;
using ASMProdWell.Dao;
using ASMProdWell.Utils;
using ASMProdWell.Components.Equipment.Pumps;
using ASMProdWell.Components.Equipment.PlungerLift;
using ASMProdWell.Security;
using ASMProdWell.Utils.GUI;

namespace ASMProdWell
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		static PersistanceContext db = new PersistanceContext();

        public User CurrentUser;

		/// <summary>
		/// Лист компонентов, связывает программу с таблицей
		/// </summary>
		public static List<ComponetGridRow> FluidComponents = new List<ComponetGridRow>();

		/// <summary>
		/// Параметры НКТ
		/// </summary>
		public static List<RowResearchNKT> NktParameter = new List<RowResearchNKT>();

        /// <summary>
        /// Лист скважин, связывает интерфейс с таблицей
        /// </summary>
        public static List<RowWellGrid> Wells = new List<RowWellGrid>();

		/// <summary>
		/// Строки таблицы каталога ступеней ЭЦН
		/// </summary>
		public static List<DataBaseESPRow> ESPRows = new List<DataBaseESPRow>();

		/// <summary>
		/// Строки таблицы каталога насосов ВШН
		/// </summary>
		public static List<DataBasePCPRow> PCPRows = new List<DataBasePCPRow>();

		/// <summary>
		/// Строки таблицы подобранных насосов ЭЦН
		/// </summary>
		public static List<ChosingESPRow> ChosenESPRows = new List<ChosingESPRow>();

		/// <summary>
		/// Строки таблицы подобранных насосов ВШН
		/// </summary>
		public static List<ChosingPCPRow> ChosenPCPRows = new List<ChosingPCPRow>();

		/// <summary>
		/// Каталог ступеней ЭЦН
		/// </summary>
		public List<ElectricSubmersiblePump> CatalogEsp = db.EspPumps.Include("PowerCoefficients").Include("EfficiencyCoefficients").Include("HeadCoefficients").ToList();

		/// <summary>
		/// Каталог насосов ВШН
		/// </summary>
        public List <ProgressiveCavityPump> CatalogPcp = db.PcpPumps.Include("PowerCoefficients").Include("RateCoefficients").Include("TorqueCoefficients").ToList();

		/// <summary>
		/// Выбранный ЭЦН из одной из таблиц для отображения на графике и изменения
		/// </summary>
		public SettingForESP CurrentChosenEsp;

		/// <summary>
		/// Выбранный ВШН из одной из таблиц для отображения на графике и изменения
		/// </summary>
		public SettingForPCP CurrentChosenPcp;

		/// <summary>
		/// Окно добавление ступени ЭЦН
		/// </summary>
		private AddESP AddEspWindow;

		/// <summary>
		/// Окно добавления насоса ВШН
		/// </summary>
        private AddPCP AddPcpWindow;

		/// <summary>
		/// Окно изменения ступени ЭЦН
		/// </summary>
        private UpdateESP UpdateEspWindow;

		/// <summary>
		/// Окно изменения насоса ВШН
		/// </summary>
		private UpdatePCP UpdatePcpWindow;

		/// <summary>
		/// Главное окно
		/// </summary>
		public MainWindow()
        {
            InitializeComponent();
            //new Authorization(this).Show();
            //Hide();
            TestDb.Run();
            this.Top = (SystemParameters.WorkArea.Height - this.Height) / 2;
            this.Left = (SystemParameters.WorkArea.Width - this.Width) / 2;

            //Наполняем таблицу компонентов и связываем её с листом компонентов
            ComponentGrid.ItemsSource = null;
			FluidComponents.Add(new ComponetGridRow { NameComponent = "Метан", FormulaComponent = "CH₄", MoleFractionGridRepresentation = "85,2330" });
			FluidComponents.Add(new ComponetGridRow { NameComponent = "Этан", FormulaComponent = "C₂H₆", MoleFractionGridRepresentation = "4,2070" });
			FluidComponents.Add(new ComponetGridRow { NameComponent = "Пропан", FormulaComponent = "C₃H₈", MoleFractionGridRepresentation = "1,7030" });
            FluidComponents.Add(new ComponetGridRow { NameComponent = "Бутан", FormulaComponent = "n C₄H₁₀", MoleFractionGridRepresentation = "0,5180" });
            FluidComponents.Add(new ComponetGridRow { NameComponent = "Бутан", FormulaComponent = "i C₄H₁₀", MoleFractionGridRepresentation = "0,2580" });
			FluidComponents.Add(new ComponetGridRow { NameComponent = "Пентан", FormulaComponent = "n C₅H₁₂", MoleFractionGridRepresentation = "0,6180" });
			FluidComponents.Add(new ComponetGridRow { NameComponent = "Пентан", FormulaComponent = "i C₅H₁₂", MoleFractionGridRepresentation = "0,0" });
			FluidComponents.Add(new ComponetGridRow { NameComponent = "Азот", FormulaComponent = "N₂", MoleFractionGridRepresentation = "5,3790" });
			FluidComponents.Add(new ComponetGridRow { NameComponent = "Сероводород", FormulaComponent = "H₂S", MoleFractionGridRepresentation = "1,4670" });
			FluidComponents.Add(new ComponetGridRow { NameComponent = "Оксид углерода", FormulaComponent = "CO₂", MoleFractionGridRepresentation = "0,6170" });
			ComponentGrid.ItemsSource = FluidComponents;
			OutputGeneralPercentComponent();

			//Наполняем таблицу скважин и связываем её с листом скважин
			using (PersistanceContext db = new PersistanceContext())
			{
				foreach (WateredWell well in db.WateredWells)
				{
					Wells.Add((new RowWellGrid { NumberWell = Convert.ToString(well.Id), NameWell = well.Name, DateWell = Convert.ToString(well.MeasuringDate) }));
				}
				ShowPCPDataBase();
				if (CatalogPcp.Count != 0)
				{
					CurrentChosenPcp = new SettingForPCP(CatalogPcp.First());
					DrawPcpChart(CurrentChosenPcp);
				}
				ShowESPDataBase();
				if (CatalogEsp.Count != 0)
				{
					CurrentChosenEsp = new SettingForESP(CatalogEsp.First());
					DrawEspChart(CurrentChosenEsp);
				}
			}

			WellGrid.ItemsSource = null;
			WellGrid.ItemsSource = Wells;

			//Выбор действующей установки и блокировка всех остальных 
			ComboBox_PumpEquipment_SelectionChanged(((ComboBox)Equipment.Children[1]), null);
			// Блокируем вкладку системный анализ и оборудование
			TabItem_SistemAnalysis.IsEnabled = false;
            //TabItem_Equipment.IsEnabled = false;
            // Выбираем картинку типа установки
            LoadImage();
        }

		/// <summary>
		/// Отображение таблицы подобранных ступеней ЭЦН
		/// </summary>
		/// <param name="setEspList">Список подобранных ступеней ЭЦН</param>
        public void ShowChosenESP(List<SettingForESP> setEspList)
        {
			DataGrid_ESPChosing.ItemsSource = null;
            ChosenESPRows.Clear();
			foreach (SettingForESP esp in setEspList)
			{
				double efficiency = Math.Round(esp.Efficiency, 1);
				double head = Math.Round(esp.Head, 1);
				double power = Math.Round(esp.Power, 3);
				double NominalH = esp.CalcHead(esp.Rate, esp.Frequency, esp.NumberStages);
				double rate = Math.Round(esp.Rate, 1);

				ChosenESPRows.Add(new ChosingESPRow
				{
					Id = esp.Id,
					Diameter = esp.Diameter,
					Name = esp.Name,
					Rate = rate,
					NumberStages = esp.NumberStages,
					Head = head,
					Power = power,
					Efficiency = efficiency,
					Frequency = esp.Frequency
				});
			}
            DataGrid_ESPChosing.ItemsSource = ChosenESPRows;
        }


		/// <summary>
		/// Отображение таблицы подобранных насосов ВШН
		/// </summary>
		/// <param name="pcpList">список подобранных насосов ВШН</param>
		/// <param name="H">Напор (м)</param>
		/// <param name="speeds">Скорость вращение вала (об/мин), требуемоая для каждого из насосов</param>
		public void ShowChosenPCP(List<SettingForPCP> pcpList)
		{
			DataGrid_PCPChosing.ItemsSource = null;
			ChosenPCPRows.Clear();
			foreach (SettingForPCP pcp in pcpList)
			{
				double power = Math.Round(pcp.Power, 1);
				double rate = Math.Round(pcp.Rate, 1);
				double torque = Math.Round(pcp.Torque, 1);
				double head = Math.Round(pcp.Head, 1);

				ChosenPCPRows.Add(new ChosingPCPRow
				{
					Id = pcp.Id,
					Diameter = pcp.Diameter,
					Name = pcp.Name,
					NominalRate = pcp.NominalRate,
					Rate = rate,
					Head = head,
					Power = power,
					Speed = pcp.Speed,
					Torque = torque
				});
			}
			DataGrid_PCPChosing.ItemsSource = ChosenPCPRows;
		}


		/// <summary>
		/// Отображение таблицы ступеней ЭЦН из базы данных
		/// </summary>
		/// <param name="espList">список подобранных ступеней ЭЦН</param>
		public void ShowESPDataBase()
		{
			DataGrid_DataBaseESP.ItemsSource = null;
			ESPRows.Clear();
			foreach (ElectricSubmersiblePump esp in CatalogEsp)
			{
				double Q = Math.Round(esp.NominalRate, 1);
				double efficiency = Math.Round(esp.CalcEfficiency(Q), 1);
				double head = Math.Round(esp.CalcHead(Q), 1);
				double power = Math.Round(esp.CalcPower(Q), 3);

				ESPRows.Add(new DataBaseESPRow { Id = esp.Id, Diameter = esp.Diameter, Name = esp.Name, NominalRate = esp.NominalRate, Head = head, Power = power, Efficiency = efficiency });
			}
			DataGrid_DataBaseESP.ItemsSource = ESPRows;
		}

		/// <summary>
		/// Отображение таблицы насосов ВШН из базы данных
		/// </summary>
		/// <param name="espList">список подобранных ступеней ЭЦН</param>
		public void ShowPCPDataBase()
		{
			DataGrid_DataBasePCP.ItemsSource = null;
			PCPRows.Clear();
			foreach (ProgressiveCavityPump pcp in CatalogPcp)
			{
				PCPRows.Add(new DataBasePCPRow { Id = pcp.Id, Name = pcp.Name, Diameter = pcp.Diameter,  NominalRate = pcp.NominalRate, NominalSpeed = pcp.BaseSpeed });
			}
			DataGrid_DataBasePCP.ItemsSource = PCPRows;
		}


		/// <summary>
		/// Проверка заданного состава флюида на правильность
		/// </summary>
		private bool isFluidCompositionValid(List<ComponetGridRow> fluidComponents)
		{
			double sum = 0;
			for (int i = 0; i < fluidComponents.Count; i++)
				sum += fluidComponents[i].MoleFractionInPercents;
			if (sum != 100)
			{
				return false;
			}
			return true;
		}

		private bool CheckPreparingForModeling()
		{
			//Проверка состояние флюида
			if (!isFluidCompositionValid(FluidComponents))
			{
				MessageBox.Show("Суммарное процентное количество всех компонентов флюида не соответствует 100%! Проведите нормализацию флюида.", "Ошибка");
				return false;
			}

			//eps - Точность моделирования
			double eps = GetParameter(CoeffAccuracy);
			if (eps > 0.1 || eps < 0.00001)
			{ 
				MessageBox.Show("Точность расчетов должна быть не более 0.1 и не менее 0.00001");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Кнопка запуска моделирования
		/// </summary>
		private void Modeling(Object sender, RoutedEventArgs e)
		{
			if (!CheckPreparingForModeling())
				return;

			double eps = GetParameter(CoeffAccuracy);

			// Считывание параметров в Builder'ы
			CreateLayerFromUserInput();
			CreateTubingFromUserInput();

			bool isWatered = Convert.ToBoolean(IsWateredWell.IsChecked);
			try
			{
				//Проверка состояния тумблера "Обводнение"
				if (isWatered == true)
				{
					WateredWell well = CreateWateredWellFromUserInput();

					//Выбор оборудования обводненной скважины
					switch (((ComboBox)Equipment.Children[1]).SelectedIndex)
					{
						case 0: //Фонтанная скважина
							WateredWellModeling(well, eps);
							break;

						case 1: //Фонтанная скважина со скопление воды на забое 
							WateredWellWithWaterAccumulationModeling(well, eps);
							break;

						case 2: // ЭЦН
							EspModeling(well, eps);
							break;

						case 3: // ВШН
							PcpModeling(well, eps);
							break;

						case 4: //Плунжер-лифт
							PlungerLiftModeling(well, eps);
							break;
					}
				}
				else // сухая скважина
				{
					DryWellModeling(eps);
				}

				// Активируем вкладку системный анализ и оборудование
				TabItem_SistemAnalysis.IsEnabled = true;
                TabItem_Equipment.IsEnabled = true;

            }
			catch (ArgumentException ex)
			{
				MessageBox.Show(ex.Message);
				return;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
				MessageBox.Show("Неверно введены данные моделирования", "Ошибка ввода");
				return;
			}
		}

		/// <summary>
		/// Моделирование фонтанной cухой скважины
		/// </summary>
		/// <param name="eps">Точность</param>
		/// <param name="gasRate">Дебит газа (тыс.куб.м/сут)</param>
		/// <param name="bottomholePressure">Забойное давление (МПа)</param>
		/// <param name="nglRate">Дебит конденсата (т/сут)</param>
		private void DryWellModeling(double eps)
		{
			double bottomholePressure, nglRate, gasRate;

			DryWell well = CreateDryWellFromUserInput();
			gasRate = well.Modeling(eps, out bottomholePressure, out nglRate);
			ShowMainParametersOfResult(bottomholePressure, gasRate, 0, nglRate);

			int num = 100;
			double[] H, P, T;
			double[] P1, P2, Q, waterQ;
			well.GetPointsForNodeAnalize(num, out P1, out P2, out Q);
			well.GetSegmentParametersForTubing(out H, out P, out T);

			DrawNodalAnalysisChart(num, P1, P2, Q);
			LoadGradientNktGrid(H, P, T);
		}

		/// <summary>
		/// Моделирование фонтанной обводненной скважины 
		/// </summary>
		/// <param name="well">Скважина</param>
		/// <param name="eps">Точность</param>
		/// <param name="gasRate">Дебит газа (тыс.куб.м/сут)</param>
		/// <param name="bottomholePressure">Забойное давление (МПа)</param>
		/// <param name="waterRate">Дебит воды (м3/сут)</param>
		/// <param name="nglRate">Дебит конденсата (т/сут)</param>
		private void WateredWellModeling(WateredWell well, double eps)
		{
			double bottomholePressure, nglRate, gasRate, waterRate;
			int num = 100;
			double[] H, P, T;
			double[] P1, P2, Q, waterQ;

			gasRate = well.Modeling(eps, out bottomholePressure, out waterRate, out nglRate);
			well.GetPointsForNodeAnalize(num, out P1, out P2, out Q, out waterQ);
			well.GetSegmentParametersForTubing(out H, out P, out T);

			DrawNodalAnalysisChart(num, P1, P2, Q, waterQ);
			LoadGradientNktGrid(H, P, T);
			DrawPressureGradientChart(H, P);

			ShowMainParametersOfResult(bottomholePressure, gasRate, waterRate, nglRate);
		}


		/// <summary>
		/// Моделирование фонтанной скважины со скопление воды на забое
		/// </summary>
		/// <param name="well">Скважина</param>
		/// <param name="eps">Точность</param>
		/// <param name="gasRate">Дебит газа (тыс.куб.м/сут)</param>
		/// <param name="bottomholePressure">Забойное давление (МПа)</param>
		/// <param name="waterRate">Дебит воды (м3/сут)</param>
		/// <param name="nglRate">Дебит конденсата (т/сут)</param>
		private void WateredWellWithWaterAccumulationModeling(WateredWell well, double eps)
		{
			double bottomholePressure, nglRate, gasRate, waterRate;
			int num = 100;
			double[] H, P, T;
			double[] P1, P2, Q, waterQ;

			gasRate = well.Modeling(eps, out bottomholePressure, out waterRate, out nglRate);
			well.GetPointsForNodeAnalize(num, out P1, out P2, out Q, out waterQ);
			well.GetSegmentParametersForTubing(out H, out P, out T);

			bool fl = false;
			List<double> waterRates;
			double P_water, Q_start;
			double dt = 0.0001;
			double time = 0.02; // сутки
			do
			{
				time += dt;
				double columnWaterVolume = well.Tubing.CalcWaterVolumeAccumulatedOnBottomhole(waterRate, time, out waterRates);
				Q_start = well.Layer.CalcGasRate(bottomholePressure);
				double Hwat = columnWaterVolume / well.Tubing.CrossSectionArea;
				P_water = bottomholePressure + well.WaterFluid.CalcColumnPressure(Hwat);
				if (fl) break;
				if (P_water > well.Layer.ReservoirPressure)
				{
					time -= 2 * dt;
					fl = true;
				}
			} while (time < 1);

			double Q_close = well.Layer.CalcGasRate(P_water);

			bottomholePressure = time * (P_water + bottomholePressure) / 2 + (1 - time) * well.Layer.ReservoirPressure;

			if (fl)
			{
				gasRate = 0;
				waterRate = 0;
				nglRate = 0;
			}

			List<double> gasRates;
			double gasVolumeForCycle = well.Tubing.CalcGasVolumeWithGasRateFalling(Q_start, Q_close, time, out gasRates);
			DrawFallingGasRateChart(time, gasRates, waterRates);
			DrawNodalAnalysisChart(num, P1, P2, Q, waterQ);
			LoadGradientNktGrid(H, P, T);
			DrawPressureGradientChart(H, P);

			ShowMainParametersOfResult(bottomholePressure, gasRate, waterRate, nglRate);
		}

		/// <summary>
		/// Отображение основных параметров результата моделирования
		/// </summary>
		/// <param name="bottomholePressure">Забойное давление (МПа)</param>
		/// <param name="gasRate">Дебит газа (тыс.куб.м/сут)</param>
		/// <param name="waterRate">Дебит воды (м3/сут)</param>
		/// <param name="nglRate">Дебит конденсата (т/сут)</param>
		private void ShowMainParametersOfResult( double bottomholePressure, double gasRate, double waterRate, double nglRate)
		{
			SetParameter_MPa(bottomholePressure, BottomholePressureResult);
			SetParameter_GasRate(gasRate, GasRateResult);
			SetParameter_NglRate(nglRate, NglRateResult);
			SetParameter_WaterRate(waterRate, WaterRateResult);
		}

		/// <summary>
		/// Закрывает дополнительные окна
		/// </summary>
		private void Window_Closing(Object sender, CancelEventArgs e)
		{
			if (AddEspWindow != null) AddEspWindow.Close();
			if (AddPcpWindow != null) AddPcpWindow.Close();
            if (UpdateEspWindow != null)  UpdateEspWindow.Close();
            if (UpdatePcpWindow != null)  UpdatePcpWindow.Close();
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

		#region Вкладка "Общие"
		/// <summary>
		/// Тумблер обводнения включен
		/// </summary>
		private void ToggleButton1_Checked(Object sender, RoutedEventArgs e)
		{
			try
			{
				GroupBox_Water.IsEnabled = true;
				BlockId1.IsEnabled = true;
				BlockId2.IsEnabled = true;

				GroupBox_Water.Foreground = System.Windows.Media.Brushes.Black;
				BlockId2.Foreground = System.Windows.Media.Brushes.Black;
				BlockId1.Foreground = System.Windows.Media.Brushes.Black;

				((TextBox)WaterRateResult.Children[1]).IsEnabled = true;
				((Label)WaterRateResult.Children[0]).IsEnabled = true;
				((ComboBox)WaterRateResult.Children[2]).IsEnabled = true;
				((ComboBox)WaterRateResult.Children[2]).Foreground = System.Windows.Media.Brushes.Black;
				((ComboBox)WaterDensity.Children[2]).Foreground = System.Windows.Media.Brushes.Black;
				((ComboBox)Equipment.Children[1]).Foreground = System.Windows.Media.Brushes.Black;
				LoadImage();
				TabControl_Equipment.IsEnabled = true;
				((ComboBox)EspDynamicLevel.Children[2]).Foreground = System.Windows.Media.Brushes.Black;
				((ComboBox)PcpDynamicLevel.Children[2]).Foreground = System.Windows.Media.Brushes.Black;
                //TabItem_PL_ComboBox1.Foreground = System.Windows.Media.Brushes.Black;
                //TabItem_PL_ComboBox2.Foreground = System.Windows.Media.Brushes.Black;
				CheckBox_LequidFlow.Visibility = Visibility.Visible;

			}
			catch { }
		}

		/// <summary>
		/// Тумблер обводнения выключен
		/// </summary>
		private void ToggleButton1_Unchecked(Object sender, RoutedEventArgs e)
		{
			try
			{
				GroupBox_Water.IsEnabled = false;
				BlockId1.IsEnabled = false;
				BlockId2.IsEnabled = false;

				GroupBox_Water.Foreground = System.Windows.Media.Brushes.Gray;
				BlockId2.Foreground = System.Windows.Media.Brushes.Gray;
				BlockId1.Foreground = System.Windows.Media.Brushes.Gray;

				((TextBox)WaterRateResult.Children[1]).IsEnabled = false;
				((Label)WaterRateResult.Children[0]).IsEnabled = false;
				((ComboBox)WaterRateResult.Children[2]).IsEnabled = false;
				((ComboBox)WaterDensity.Children[2]).Foreground = System.Windows.Media.Brushes.Gray;
				((ComboBox)WaterRateResult.Children[2]).Foreground = System.Windows.Media.Brushes.Gray;
				BlockId2.Foreground = System.Windows.Media.Brushes.Gray;
				BlockId1.Foreground = System.Windows.Media.Brushes.Gray;
				((ComboBox)Equipment.Children[1]).Foreground = System.Windows.Media.Brushes.Gray;
				ImageLeft.Source = new BitmapImage(new Uri(@"Resource\image1.bmp", UriKind.Relative));
				TabControl_Equipment.IsEnabled = false;
				((ComboBox)EspDynamicLevel.Children[2]).Foreground = System.Windows.Media.Brushes.Gray;
				((ComboBox)PcpDynamicLevel.Children[2]).Foreground = System.Windows.Media.Brushes.Gray;
                //TabItem_PL_ComboBox1.Foreground = System.Windows.Media.Brushes.Gray;
                //TabItem_PL_ComboBox2.Foreground = System.Windows.Media.Brushes.Gray;
				CheckBox_LequidFlow.Visibility = Visibility.Collapsed;
			}
			catch { }
		}

		/// <summary>
		/// Функция выбора скважины
		/// </summary>
		private void Button_ChoiceWell(Object sender, RoutedEventArgs e)
		{
			Grid_DataGrid_ChoiceWell.Width = 315;
			Grid_Button_ChoiceWellOk.Width = 315;
		}

		/// <summary>
		/// Кнопка Ок в окне выбора скважины
		/// </summary>
		private void Button_ChoiceWellOk(Object sender, RoutedEventArgs e)
		{
			try
			{
				int i = WellGrid.SelectedIndex;
				WateredWell chosenWell = db.WateredWells.Include("Layer").Include("Tubing").ToList()[i];
				SetParameter_M_Mm(chosenWell.Tubing.PipeRoughness, PipeRoughness);
				SetParameter_M_Mm(chosenWell.Tubing.Length, TubingLength);
				SetParameter_M_Mm(chosenWell.Tubing.Depth, TubingDepth);
				SetParameter_M_Mm(chosenWell.Tubing.PipeDiameter, PipeDiameter);

				SetParameter_MPa(chosenWell.WellheadPressure, WellheadPressure);
				SetParameter_K_C(chosenWell.WellheadTemperature, WellheadTemperature);
				SetParameter_K_C(chosenWell.BottomholeTemperature, BottomholeTemperature);

				SetParameter_MPa(chosenWell.Layer.ReservoirPressure, ReservoirPressure);
				SetParameter_K_C(chosenWell.Layer.NeutralLayerTemperature, NeutralLayerTemperature);

				SetParameter(chosenWell.Layer.a, LayerA);
				SetParameter(chosenWell.Layer.b, LayerB);
				SetParameter(chosenWell.Layer.A, LayerA1);
				SetParameter(chosenWell.Layer.B, LayerB1);
				SetParameter(chosenWell.Layer.Alpha, Alpha);
				SetParameter(chosenWell.Layer.Beta, Beta);

				SetParameter(chosenWell.Tubing.CoeffWaterAccumulationA, WaterAccumulationRateCoeffA);
				SetParameter(chosenWell.Tubing.CoeffWaterAccumulationB, WaterAccumulationRateCoeffB);
				SetParameter(chosenWell.Tubing.CoeffWaterAccumulationC, WaterAccumulationRateCoeffC);
				SetParameter(chosenWell.Tubing.CoeffGasRateFallingA, GasRateFallCoeffA);

				Grid_DataGrid_ChoiceWell.Width = 0;
				Grid_Button_ChoiceWellOk.Width = 0;
			}
			catch (Exception ex) {
                Grid_DataGrid_ChoiceWell.Width = 0;
                Grid_Button_ChoiceWellOk.Width = 0;
                MessageBox.Show("Возникла ошибка при попытке загрузить данные о скважине из БД");
                throw ex;
			}
		}

		/// <summary>
		/// Загрузка левой картинки соответственно выбронного оборудования
		/// </summary>
		private void LoadImage()
		{
			if (((ComboBox)Equipment.Children[1]).SelectedIndex == 0 || ((ComboBox)Equipment.Children[1]).SelectedIndex == 1)
				ImageLeft.Source = new BitmapImage(new Uri(@"Resource\image2.bmp", UriKind.Relative));
			if (((ComboBox)Equipment.Children[1]).SelectedIndex == 2)
				ImageLeft.Source = new BitmapImage(new Uri(@"Resource\image4.bmp", UriKind.Relative));
			if (((ComboBox)Equipment.Children[1]).SelectedIndex == 3)
				ImageLeft.Source = new BitmapImage(new Uri(@"Resource\image3.bmp", UriKind.Relative));
			if (((ComboBox)Equipment.Children[1]).SelectedIndex == 4)
				ImageLeft.Source = new BitmapImage(new Uri(@"Resource\image5.bmp", UriKind.Relative));

		}

		/// <summary>
		/// При закрытии ниспадающего списка установок
		/// </summary>
		private void ComboBox_PumpEquipment_DropDownClosed(Object sender, EventArgs e)
		{
			LoadImage();
		}
		#endregion

		#region Вкладка "Системный анализ"

		/// <summary>
		/// Построение графика узлового анализа
		/// </summary>
		/// <param name="num">Количество точек</param>
		/// <param name="P1">Кривая оттока от забоя</param>
		/// <param name="P2">Кривая притока к забою</param>
		/// <param name="Q">Значения дебита газа для кривых притока и оттока </param>
		/// <param name="waterQ">Значения притока воды по оси ОХ и P2 по оси ОУ</param>
		private void DrawNodalAnalysisChart(int num, double[] P1, double[] P2, double[] Q, double[] waterQ = null)
		{
			ChartBuilder.DrawNodalAnalysisChart(Сhart_NodalAnalysis, num, P1, P2, Q, waterQ);
			if(waterQ != null) CheckBox_LiquidFlow_Click(null, null);
		}

		/// <summary>
		/// Ввод данных в таблицу "параметры НКТ"
		/// </summary>
		/// <param name="H">Высота</param>
		/// <param name="P">Давление</param>
		/// <param name="T">Температура</param>
		private void LoadGradientNktGrid(double[] H, double[] P, double[] T)
		{
			NktParameter.Clear();
			for (int i = 0; i < H.Length; i++)
				NktParameter.Add(new RowResearchNKT { Depth = H[i], Temperature = T[i], Pressure = P[i] });
			ResearchNktGrid.ItemsSource = null;
			ResearchNktGrid.ItemsSource = NktParameter;
		}

		/// <summary>
		/// Загрузка графика градиента давления
		/// </summary>
		private void DrawPressureGradientChart(double[] H, double[] P)
		{
			ChartBuilder.DrawPressureGradientChart(Chart_ResearchNKT, H, P);
		}

		/// <summary>
		/// Построение графикоф падения дебита газа и скорости скопления воды на забое в течение заданного времени
		/// </summary>
		/// <param name="dt">Шаг времени (сут)</param>
		/// <param name="time_work">Время цикла плунжер лифта или время пока скважина не заглохнет (сут)</param>
		/// <param name="gasRates">Дебит газа на каждый шаг времени (тыс.м3/сут)</param>
		/// <param name="waterRates">Скорость скопления воды на забое для каждого шага времени (м3/сут)</param>
		/// <param name="time_rec">Время восстановления (сут)</param>
		private void DrawFallingGasRateChart( double time_work, List<double> gasRates, List<double> waterRates, double time_rec = 0)
		{
			ChartBuilder.DrawFallingGasRateChart(Chart_PL_Rate, time_work, gasRates, waterRates, time_rec);
		}

		/// <summary>
		/// Отображение обводнения в системном анализе
		/// </summary>
		private void CheckBox_LiquidFlow_Click(Object sender, RoutedEventArgs e)
		{
			if (CheckBox_LequidFlow.IsChecked.Value)
				Сhart_NodalAnalysis.Series["Приток жидкости"].Enabled = true;
			else
				Сhart_NodalAnalysis.Series["Приток жидкости"].Enabled = false;
		}



		#endregion

		#region Вкладка "Оборудование"
		#region ЭЦН

		/// <summary>
		/// Моделирование обводненной скважины с идеальным ЭЦН
		/// </summary>
		/// <param name="well">Скважина</param>
		/// <param name="eps">Точность</param>
		/// <param name="gasRate">Дебит газа (тыс.куб.м/сут)</param>
		/// <param name="bottomholePressure">Забойное давление (МПа)</param>
		/// <param name="waterRate">Дебит воды (м3/сут)</param>
		/// <param name="nglRate">Дебит конденсата (т/сут)</param>
		private void EspModeling(WateredWell well, double eps)
		{
			double bottomholePressure, nglRate, gasRate, waterRate;
			


			double dynamicH = GetParameter(EspDynamicLevel);
			List<SettingForESP> chosenESPs;
			chosenESPs = well.ModelingESP(eps, dynamicH, CatalogEsp, out bottomholePressure, out gasRate, out waterRate, out nglRate);
			ShowChosenESP(chosenESPs);

			int num = 100;
			double[] H, P, T;
			double[] P1, P2, Q, waterQ;
			well.GetPointsAnnularTubingNodeAnalize(dynamicH, num, out P1, out P2, out Q, out waterQ);
			well.GetSegmentParametersAnnularTubing(dynamicH, out H, out P, out T);
			DrawNodalAnalysisChart(num, P1, P2, Q, waterQ);
			LoadGradientNktGrid(H, P, T);
			DrawPressureGradientChart(H, P);

			ShowMainParametersOfResult(bottomholePressure, gasRate, waterRate, nglRate);
		}

		/// <summary>
		/// Загрузка графика найстоенного ЭЦН 
		/// </summary>
		private void DrawEspChart(SettingForESP settings)
		{
			ESP_Name.Content = settings.Name;

			ESP_Frequency.Text = settings.Frequency.ToString();
			ESP_NumberStages.Text = settings.NumberStages.ToString();

			ChartBuilder.DrawEspChart(Chart_ESP, settings);
		}


		/// <summary>
		/// Кнопка Обновления графика ЭЦН
		/// </summary>
		public void Button_Click_UpdateGrafESN(Object sender, RoutedEventArgs e)
		{
			if (CurrentChosenEsp == null)
				return;
			CurrentChosenEsp.NumberStages = Convert.ToInt32(ESP_NumberStages.Text);
			CurrentChosenEsp.Frequency = Convert.ToDouble(ESP_Frequency.Text);
			DrawEspChart(CurrentChosenEsp);
		}

		/// <summary>
		/// При переключении фокуса с элемента ESP_NumberStages и ESP_Frequency
		/// </summary>
		private void TB_StageAndFrequency_LostFocus(Object sender, RoutedEventArgs e)
		{
			if (CurrentChosenEsp == null)
				return;
			CurrentChosenEsp.NumberStages = Convert.ToInt32(ESP_NumberStages.Text);
			CurrentChosenEsp.Frequency = Convert.ToDouble(ESP_Frequency.Text);
			DrawEspChart(CurrentChosenEsp);
		}

		/// <summary>
		/// При нажатии клавиши Enter с фокусом на элементе ESP_NumberStages и ESP_Frequency
		/// </summary>
		private void TB_StageAndFrequency_KeyDown(Object sender, KeyEventArgs e)
		{
			if (e.Key.ToString() == "Return")
			{
				if (CurrentChosenEsp == null)
					return;
				CurrentChosenEsp.NumberStages = Convert.ToInt32(ESP_NumberStages.Text);
				CurrentChosenEsp.Frequency = Convert.ToDouble(ESP_Frequency.Text);
				DrawEspChart(CurrentChosenEsp);
			}
        }

		/// <summary>
		/// Кнопка увеличения значения кол-ва ступений ЭЦН
		/// </summary>
		private void Button_Click_NumStagesStepUp(Object sender, RoutedEventArgs e)
		{
			if (Convert.ToInt16(ESP_NumberStages.Text.ToString()) < ElectricSubmersiblePump.MaxNumberStages)
			{
				if (CurrentChosenEsp == null)
					return;
				CurrentChosenEsp.NumberStages++;
				DrawEspChart(CurrentChosenEsp);
            }
		}


		/// <summary>
		/// Кнопка уменьшения значения кол-ва ступений ЭЦН
		/// </summary>
		private void Button_Click_NumStagesStepDown(Object sender, RoutedEventArgs e)
		{
			if (Convert.ToInt16(ESP_NumberStages.Text.ToString()) > ElectricSubmersiblePump.MinNumberStages)
			{
				if (CurrentChosenEsp == null)
					return;
				CurrentChosenEsp.NumberStages--;
				DrawEspChart(CurrentChosenEsp);
            }
		}

		/// <summary>
		/// Кнопка увеличения значения частоты ЭЦН
		/// </summary>
		private void Button_Click_FrequencyStepUp(Object sender, RoutedEventArgs e)
		{
			if (Convert.ToInt16(ESP_Frequency.Text.ToString()) < ElectricSubmersiblePump.MaxFrequency)
			{
				if (CurrentChosenEsp == null)
					return;
				CurrentChosenEsp.Frequency += 1.0;
				DrawEspChart(CurrentChosenEsp);
			}
		}

		/// <summary>
		/// Кнопка уменьшения значения частоты ЭЦН
		/// </summary>
		private void Button_Click_FrequencyStepDown(Object sender, RoutedEventArgs e)
		{
			if (Convert.ToInt16(ESP_Frequency.Text.ToString()) > ElectricSubmersiblePump.MinFrequency)
			{
				if (CurrentChosenEsp == null)
					return;
				CurrentChosenEsp.Frequency -= 1.0;
				DrawEspChart(CurrentChosenEsp);
			}
		}

		/// <summary>
		/// Добавить ступень ЭЦН
		/// </summary>
		private void Button_Click_AddESN(Object sender, RoutedEventArgs e)
		{
            AddEspWindow = new AddESP(this);

            AddEspWindow.Show();
		}

		/// <summary>
		/// Кнопка изменения параметров ступени ЭЦН
		/// </summary>
		private void Button_Click_UpdateESP(Object sender, RoutedEventArgs e)
		{
			if (CurrentChosenEsp != null)
			{
				UpdateEspWindow = new UpdateESP(this, CurrentChosenEsp);
				UpdateEspWindow.Show();
			}
		}

		#endregion

		#region ВШН

		/// <summary>
		/// Моделирование обводненной скважины с идеальным ВШН
		/// </summary>
		/// <param name="well">Скважина</param>
		/// <param name="eps">Точность</param>
		/// <param name="gasRate">Дебит газа (тыс.куб.м/сут)</param>
		/// <param name="bottomholePressure">Забойное давление (МПа)</param>
		/// <param name="waterRate">Дебит воды (м3/сут)</param>
		/// <param name="nglRate">Дебит конденсата (т/сут)</param>
		private void PcpModeling(WateredWell well, double eps)
		{
			double bottomholePressure, nglRate, gasRate, waterRate;

			double dynamicH = GetParameter(PcpDynamicLevel);
			List<SettingForPCP> chosenPCPs = well.ModelingPCP(eps, dynamicH, CatalogPcp, out bottomholePressure, out gasRate, out waterRate, out nglRate);
			ShowChosenPCP(chosenPCPs);

			int num = 100;
			double[] H, P, T;
			double[] P1, P2, Q, waterQ;
			well.GetPointsAnnularTubingNodeAnalize(dynamicH, num, out P1, out P2, out Q, out waterQ);
			well.GetSegmentParametersAnnularTubing(dynamicH, out H, out P, out T);
			DrawNodalAnalysisChart(num, P1, P2, Q, waterQ);
			LoadGradientNktGrid(H, P, T);
			DrawPressureGradientChart(H, P);

			ShowMainParametersOfResult(bottomholePressure, gasRate, waterRate, nglRate);
		}

		/// <summary>
		/// Загрузка графика ВШН
		/// </summary>
		private void DrawPcpChart(SettingForPCP settings)
        {
			PCP_Name.Content = settings.Name;

			PCP_Speed.Text = settings.Speed.ToString();

			ChartBuilder.DrawPcpChart(Chart_PCP, settings);
		}
		
        /// <summary>
        /// Кнопка добавить ВШН
        /// </summary>
        private void Button_Click_AddPCP(Object sender, RoutedEventArgs e)
        {
            AddPcpWindow = new AddPCP(this);
            AddPcpWindow.Show();
        }

        private void Button_Click_UpdatePCP(Object sender, RoutedEventArgs e)
        {
			if (CurrentChosenEsp != null)
			{
				UpdatePcpWindow = new UpdatePCP(this, CurrentChosenPcp);
				UpdatePcpWindow.Show();
			}
        }

		/// <summary>
		/// При переключении фокуса с элемента PCP_Speed
		/// </summary>
		private void TB_Speed_LostFocus(Object sender, RoutedEventArgs e)
		{
			if (CurrentChosenPcp == null)
				return;
			CurrentChosenPcp.Speed = Convert.ToDouble(PCP_Speed.Text);
			DrawPcpChart(CurrentChosenPcp);
		}

		/// <summary>
		/// При нажатии клавиши Enter с фокусом на элементе PCP_Speed
		/// </summary>
		private void TB_Speed_KeyDown(Object sender, KeyEventArgs e)
		{
			if (e.Key.ToString() == "Return")
			{
				if (CurrentChosenPcp == null)
					return;
				CurrentChosenPcp.Speed = Convert.ToDouble(PCP_Speed.Text);
				DrawPcpChart(CurrentChosenPcp);
			}
		}

		/// <summary>
		/// Кнопка увеличения значения скорости ВШН
		/// </summary>
		private void Button_Click_SpeedStepUp(Object sender, RoutedEventArgs e)
		{
			if (Convert.ToInt16(PCP_Speed.Text.ToString()) < ProgressiveCavityPump.MaxSpeed)
			{
				if (CurrentChosenEsp == null)
					return;
				CurrentChosenPcp.Speed += 1.0;
				DrawPcpChart(CurrentChosenPcp);
			}
		}

		/// <summary>
		/// Кнопка уменьшения значения скорости ВШН
		/// </summary>
		private void Button_Click_SpeedStepDown(Object sender, RoutedEventArgs e)
		{
			if (Convert.ToInt16(PCP_Speed.Text.ToString()) > ProgressiveCavityPump.MinSpeed)
			{
				if (CurrentChosenPcp == null)
					return;
				CurrentChosenPcp.Speed -= 1.0;
				DrawPcpChart(CurrentChosenPcp);
			}
		}

		/// <summary>
		/// Кнопка Обновления графика ВШН
		/// </summary>
		public void Button_Click_UpdateGrafPCP(Object sender, RoutedEventArgs e)
		{
			if (CurrentChosenPcp == null)
				return;
			CurrentChosenPcp.Speed = Convert.ToDouble(PCP_Speed.Text);
			DrawPcpChart(CurrentChosenPcp);
		}


		#endregion

		#region ПЛ

		/// <summary>
		/// Моделирование плунжерного лифта
		/// </summary>
		/// <param name="well">Скважина</param>
		/// <param name="eps">Точность</param>
		private void PlungerLiftModeling(WateredWell well, double eps)
		{
			double pressForOneBbl = GetParameter_MPa(PL_PressForOneBbl);
			double pressForPlunger = GetParameter_MPa(PL_PressForPlunger);
			double frictionUnderPlungerCoeff = GetParameter(PL_FrictionUnderPlungerCoeff);
			double speed = GetParameter(PL_PlungerSpeed);

			Plunger plunger = new Plunger(pressForOneBbl, pressForPlunger, frictionUnderPlungerCoeff, speed);

			PlungerModelingResult mr = well.ModelingPlunger(eps, plunger);

			double waterRate = (mr.StartWaterRate + mr.CloseWaterRate) / 2;
			double nglRate = (mr.StartNglRate + mr.CloseNglRate) / 2;

			mr.TimeRecovery = well.Layer.RecoveryBottomholePressure(mr.CloseBtmhPressure, mr.OpenBtmhPressure);
			mr.TimeUp = well.Tubing.Length / speed;

			List<double> waterRates, gasRates;
			mr.TimeWork = well.Tubing.CalcTimeForWaterAccumulationOnBottomhole(waterRate, mr.ColumnWaterVolume, out waterRates);
			mr.GasVolumeForCycle = well.Tubing.CalcGasVolumeWithGasRateFalling(mr.StartGasRate, mr.CloseGasRate, mr.TimeWork, out gasRates);
			DrawFallingGasRateChart(mr.TimeWork, gasRates, waterRates, mr.TimeRecovery);

			mr.TimeCycle = mr.TimeRecovery + mr.TimeWork + (mr.TimeUp / (60 * 24));
			mr.NumCyclesInDay = 1 / mr.TimeCycle;
			mr.AverageGasRate = mr.GasVolumeForCycle * mr.NumCyclesInDay;
			mr.AverageNglRate = well.CalcNglRate(mr.AverageGasRate);

			//В mr.AverageWaterRate входит вода добытая с помощью насоса и вода вынесенная вместе с газом
			mr.AverageWaterRate = (waterRate * mr.TimeWork) * mr.NumCyclesInDay;

			double k1 = mr.TimeRecovery / mr.TimeCycle;
			double k2 = mr.TimeWork / mr.TimeCycle;
			double k3 = (mr.TimeUp / (60 * 24)) / mr.TimeCycle;
			mr.AverageBtmhPressure = k3 * (mr.OpenBtmhPressure + mr.StartBtmhPressure) / 2 + k2 * (mr.StartBtmhPressure + mr.CloseBtmhPressure) / 2 + k1 * (mr.CloseBtmhPressure + mr.OpenBtmhPressure) / 2;

			ShowPlungerLiftModelingResult(mr);
		}

		/// <summary>
		/// Отображение результатов моделирования работы плунжерного лифта
		/// </summary>
		private void ShowPlungerLiftModelingResult(PlungerModelingResult mr)
		{
			SetParameter_MPa(mr.StartBtmhPressure, PL_Pressure_Start_Production);
			SetParameter_MPa(mr.CloseBtmhPressure, PL_Pressure_Valve_Closing);
			SetParameter_MPa(mr.OpenBtmhPressure, PL_Pressure_Valve_Opening);
			SetParameter_GasRate(mr.CritQ, PL_CriticalRate);
			SetParameter_GasRate(mr.StartGasRate, PL_StartGasRate);
			SetParameter_WaterRate(mr.StartWaterRate, PL_StartWaterRate);
			SetParameter_GasRate(mr.CloseGasRate, PL_CloseGasRate);
			SetParameter_WaterRate(mr.CloseWaterRate, PL_CloseWaterRate);
			SetParameter_Time(mr.TimeRecovery, PL_Time_Recovery);
			SetParameter_Time(mr.TimeWork, PL_Time_Production);
			SetParameter_Time(mr.TimeUp, PL_Time_Up);
			SetParameter_M_Mm(mr.ColumnHeight, PL_WaterColumnHeight);
			SetParameter_Volume(mr.ColumnWaterVolume, PL_WaterColumnVolume);
			SetParameter(mr.NumCyclesInDay, PL_NumCycles);

			List<double> time = new List<double>();
			time.Add(0);
			time.Add(Math.Round(mr.TimeUp / (60 * 24), 3));
			time.Add(Math.Round(mr.TimeWork, 3));
			time.Add(Math.Round(mr.TimeRecovery, 3));
			List<double> press = new List<double> { mr.OpenBtmhPressure, mr.StartBtmhPressure, mr.CloseBtmhPressure, mr.OpenBtmhPressure };
			DrawWaterAccumulationChart(time, press);

			ShowMainParametersOfResult(mr.AverageBtmhPressure, mr.AverageGasRate, mr.AverageWaterRate, mr.AverageNglRate);

			//График узлового анализа должен показывать точку системы в момент закрытия клапана
			/*int num = 100;
			double[] H, P, T;
			double[] P1, P2, Q, waterQ;
			well.GetPointsForNodeAnalize(num, out P1, out P2, out Q, out waterQ);
			well.GetSegmentParametersForTubing(out H, out P, out T);
			DrawNodalAnalysisChart(num, P1, P2, Q, waterQ);
			LoadedGridGradientNKT(H, P, T);
			DrawPressureGradientChart(H, P);*/
		}

		/// <summary>
		/// Загрузка графика ПЛ
		/// </summary>
		private void DrawWaterAccumulationChart(List<double> times, List<double> press)
		{
			ChartBuilder.DrawWaterAccumulationChart(Chart_PL, times, press);
		}

		#endregion
		#endregion

		#region Вкладка "Флюид"
		/// <summary>
		/// Обработка изменения значения ячейки таблицы компонентов
		/// </summary>
		private void ComponentGrid_CellEditEnding(Object sender, DataGridCellEditEndingEventArgs e)
		{
			try
			{
				double EditedCellDouble = Convert.ToDouble((e.EditingElement as TextBox).Text);
				if (EditedCellDouble < 0 || EditedCellDouble > 100)
				{
					MessageBox.Show("Значение ячейки таблицы \"Молярная доля\" должно быть числом больше или равно 0 и меньше 100!", "Ошибка");
					Label_ErrorFluidComponents.Text = "Значение ячейки таблицы \"Молярная доля\" должно быть числом больше или равно 0 и меньше 100!";
					(e.EditingElement as TextBox).Text = "0";
				}
			}
			catch
			{
				//MessageBox.Show("Значение ячейки таблицы \"Молярная доля\" должно быть числом больше 0 и меньше 100!", "Ошибка");
				(e.EditingElement as TextBox).Text = "0";
			}
		}

		/// <summary>
		/// Проверка суммы процентов всех компонентов
		/// </summary>
		private void OutputGeneralPercentComponent()
		{
			decimal sum = 0, del;
			for (int i = 0; i < FluidComponents.Count; i++)
			{
				del = (decimal)FluidComponents[i].MoleFractionInPercents;
				sum = sum + del;
			}
			GeneralPercentComponent.Content = ((double)((int)(sum * 1000))) / 1000;
			if ((sum > 95 || sum < 105) && sum != 100)
			{
				GeneralPercentComponent.Foreground = System.Windows.Media.Brushes.Indigo;
				Label_ErrorFluidComponents.Text = "Проведите нормализацию флюида. (" + Math.Round(100 - sum, 9) + "%)";
			}
			if (sum < 95 || sum > 105)
			{
				GeneralPercentComponent.Foreground = System.Windows.Media.Brushes.Red;
				Label_ErrorFluidComponents.Text = "Сумма долей компонентов " + (double)sum + "%. Нормирование не проводиться при отклонении более 5%. Приведите " +
						 "молярные доли компонентов флюида в соответствие.";
			}
			if (sum == 100)
			{
				GeneralPercentComponent.Foreground = System.Windows.Media.Brushes.Green;
				if (Label_ErrorFluidComponents.Text.Length > 0 && Label_ErrorFluidComponents.Text[Label_ErrorFluidComponents.Text.Length - 1] == '%')
					Label_ErrorFluidComponents.Text += ".";
				else
					Label_ErrorFluidComponents.Text = "Суммарное процентное количество всех компонентов " + (double)sum + " %.";
			}
		}

		/// <summary>
		/// Нормировать данные в таблице компонентов флюида 
		/// </summary>
		private void Button_NormalizeFluid(Object sender, RoutedEventArgs e)
		{
			decimal sum = 0, del;
			for (int i = 0; i < FluidComponents.Count; i++)
			{
				del = (decimal)FluidComponents[i].MoleFractionInPercents;
				sum = sum + del;
			}
			if (sum != 100)
				if (sum - 100 < 5 || sum - 100 > -5)
				{
					Label_ErrorFluidComponents.Text = "Сумма долей компонентов " + sum + " %. При нормировании изменена доля метана на " + Math.Round(100 - sum, 9) + "%";
					FluidComponents[0].MoleFractionGridRepresentation = (FluidComponents[0].MoleFractionInPercents - ((double)sum - 100.00)).ToString();
				}
			OutputGeneralPercentComponent();
			return;
		}

		/// <summary>
		/// Автоматическое обновление процентов просле изменение ячейки "доля"
		/// </summary>
		private void ComponentGrid_PreviewKeyUp(Object sender, KeyEventArgs e)
		{
			if ((sender as DataGrid).Name == "ComponentGrid" && (e.Key == Key.Back || e.Key == Key.Delete || (e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)))
				OutputGeneralPercentComponent();
		}
		#endregion

		#region Функции для вывода, вывода параметров

		/// <summary>
		/// Создать класс LayerBuider на основе данных, полученных из интерфейса
		/// </summary>
		public void CreateLayerFromUserInput()
		{
			double reservoirPressure = GetParameter_MPa(ReservoirPressure);
			double a = GetParameter(LayerA);
			double b = GetParameter(LayerB);
			double A = GetParameter(LayerA1);
			double B = GetParameter(LayerB1);
			double alpha = GetParameter(Alpha);
			double beta = GetParameter(Beta); 
			double nglFactor = GetParameter(NglFactor); //Natural gas liquids factor (газоконденсатный фактор)
			double neutralLayerTemperature = GetParametr_K_C(NeutralLayerTemperature);

			LayerBuilder.One = new LayerBuilder(reservoirPressure, a, b, neutralLayerTemperature, nglFactor, A, B, alpha, beta);
		}

		/// <summary>
		/// Создать класс TubingBuider на основе данных, полученных из интерфейса
		/// </summary>
		public void CreateTubingFromUserInput()
		{
			double pipeDiameter = GetParameter_M_Mm(PipeDiameter);
			double pipeWallThickness = GetParameter_M_Mm(WallThickness);
			double pipeRoughness = GetParameter_M_Mm(PipeRoughness);
			double tubingLenght = GetParameter_M_Mm(TubingLength);
			double tubingDepth = GetParameter_M_Mm(TubingDepth);
			double tubingDiameter = GetParameter_M_Mm(OperationalColumnDiameter);

			int numberSegments = (int)GetParameter(NumberOfSegments);
			double wa = GetParameter(WaterAccumulationRateCoeffA);
			double wb = GetParameter(WaterAccumulationRateCoeffB);
			double wc = GetParameter(WaterAccumulationRateCoeffC);
			double grfa = GetParameter(GasRateFallCoeffA);

			TubingBuilder.One = new TubingBuilder(pipeDiameter, pipeWallThickness,
									  pipeRoughness, tubingLenght, tubingDepth, tubingDiameter,
									  numberSegments, wa, wb, wc, grfa);
		}

		/// <summary>
		/// Создать класс WateredWell на основе данных, полученных из интерфейса
		/// </summary>
		private WateredWell CreateWateredWellFromUserInput()
		{
			WateredWell well = new WateredWell();

			double wellheadPressure = GetParameter_MPa(WellheadPressure);
			double wellheadTemperature = GetParametr_K_C(WellheadTemperature);
			double bottomholeTemperature = GetParametr_K_C(BottomholeTemperature);
			double waterDensity = GetParameter_kg_m3(WaterDensity);
			double nglDensity = GetParameter_kg_m3(NglDensity);

			well.InitWell(wellheadPressure, wellheadTemperature, bottomholeTemperature, nglDensity, waterDensity);

			ShowGasFluidParameters(well.GasFluid);
			return well;
		}

		/// <summary>
		/// Создать класс WateredWell на основе данных, полученных из интерфейса
		/// </summary>
		private DryWell CreateDryWellFromUserInput()
		{
			DryWell well = new DryWell();

			double wellheadPressure = GetParameter_MPa(WellheadPressure);
			double wellheadTemperature = GetParametr_K_C(WellheadTemperature);
			double bottomholeTemperature = GetParametr_K_C(BottomholeTemperature);
			double nglDensity = GetParameter_kg_m3(NglDensity);

			well.InitWell(wellheadPressure, wellheadTemperature, bottomholeTemperature, nglDensity);

			ShowGasFluidParameters(well.GasFluid);
			return well;
		}

		/// <summary>
		/// Вывод во вкладку Флюид параметры плотность, критические температура и давление газа 
		/// </summary>
		/// <param name="gasFluid">Газовыый флюид</param>
		public void ShowGasFluidParameters(GasFluid gasFluid = null)
		{
			if (gasFluid == null)
				gasFluid = GasFluid.GetFluid();
			double gasDensity = gasFluid.GetDensity(PhysicalConstants.PressureAtStandardConditions,
									PhysicalConstants.TemperatureAtStandardConditions);
			SetParameter_kg_m3(gasDensity, GasDensity);
			SetParameter_MPa(gasFluid.CriticalPressure, CriticalPressure);
			SetParameter_K_C(gasFluid.CriticalTemperature, CriticalTemperature);
		}

		/// <summary>
		/// Получение безразмерного параметра (безразмерный)
		/// </summary>
		public double GetParameter(StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			double value = Convert.ToDouble(tb.Text);
			return value;
		}

		/// <summary>
		/// Получение параметра в МПа (МПа)
		/// </summary>
		public double GetParameter_MPa(StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			double value = Convert.ToDouble(tb.Text);
			return value;
		}

		/// <summary>
		/// Получение параметра в кг/м3 (кг/м3)
		/// </summary>
		public double GetParameter_kg_m3(StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			double value = Convert.ToDouble(tb.Text);
			return value;
		}

		/// <summary>
		/// Получение параметра в кельвинах (К)
		/// </summary>
		public double GetParametr_K_C(StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			double value;
			if (cb.SelectedIndex == 1)
				value = Convert.ToDouble(CToK(tb.Text));
			else
				value = Convert.ToDouble(tb.Text);
			return value;
		}

		/// <summary>
		/// Получение параметра в метрах (м)
		/// </summary>
		public double GetParameter_M_Mm(StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			double value;
			if (cb.SelectedIndex == 1)
				value = Convert.ToDouble(MmToM(tb.Text));
			else
				value = Convert.ToDouble(tb.Text);
			return value;
		}

		/// <summary>
		/// Задание безразмерного параметра
		/// </summary>
		/// <param name="value">Значение параметра (безразмерная)</param>
		public void SetParameter(double value, StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			tb.Text = Math.Round(value, 3).ToString();
		}

		/// <summary>
		/// Задание параметра температуры
		/// </summary>
		/// <param name="value">Значение параметра (К)</param>
		public void SetParameter_K_C(double value, StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			if (cb.SelectedIndex == 1)
				value -= PhysicalConstants.NullTemperatureByCelsiusInKelvin;
			tb.Text = Math.Round(value, 2).ToString();
		}

		/// <summary>
		/// Задание параметра длины
		/// </summary>
		/// <param name="value">Значение параметра (м)</param>
		public void SetParameter_M_Mm(double value, StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			if (cb.SelectedIndex == 1)
				tb.Text = MmToM(value.ToString());
			else 
				tb.Text = value.ToString();
		}

		/// <summary>
		/// Задание параметра плотности
		/// </summary>
		/// <param name="value">Значение параметра (кг/м3)</param>
		public void SetParameter_kg_m3(double value, StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			tb.Text = Math.Round(value, 3).ToString();
		}

		/// <summary>
		/// Задание параметра давления
		/// </summary>
		/// <param name="value">Значение параметра (МПа)</param>
		public void SetParameter_MPa(double value, StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			tb.Text = Math.Round(value, 3).ToString();
		}

		/// <summary>
		/// Задание параметра дебита газа
		/// </summary>
		/// <param name="value">Значение параметра (тыс.куб.м/сут)</param>
		public void SetParameter_GasRate(double value, StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			tb.Text = Math.Round(value, 3).ToString();
		}

		/// <summary>
		/// Задание параметра дебита воды
		/// </summary>
		/// <param name="value">Значение параметра (м3/сут)</param>
		public void SetParameter_WaterRate(double value, StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			tb.Text = Math.Round(value, 3).ToString();
		}

		/// <summary>
		/// Задание параметра дебита конденсата
		/// </summary>
		/// <param name="value">Значение параметра (т/сут)</param>
		public void SetParameter_NglRate(double value, StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			tb.Text = Math.Round(value, 3).ToString();
		}

		/// <summary>
		/// Задание параметра времени
		/// </summary>
		/// <param name="value">Значение параметра (сут)</param>
		public void SetParameter_Time(double value, StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			tb.Text = Math.Round(value, 3).ToString();
		}

		/// <summary>
		/// Задание параметра объема
		/// </summary>
		/// <param name="value">Значение параметра (м3)</param>
		public void SetParameter_Volume(double value, StackPanel sp)
		{
			TextBox tb = (TextBox)sp.Children[1];
			ComboBox cb = (ComboBox)sp.Children[2];
			tb.Text = Math.Round(value, 3).ToString();
		}

		#endregion

		#region Функции обработки единиц измерения

		/// <summary>
		/// Перевод милиметров в метры
		/// </summary>
		private string MmToM(string s)
		{
			double new_value = Convert.ToDouble(s);
			new_value /= 1000.0;
			return new_value.ToString();
		}

		/// <summary>
		/// Перевод из метров в милиметры
		/// </summary>
		private string MToMm(string s)
		{
			double new_value = Convert.ToDouble(s);
			new_value *= 1000.0;
			return new_value.ToString();
		}

		/// <summary>
		/// Перевод Кельвинов в градусы по Цельсию
		/// </summary>
		private string KToC(string s)
		{
			double new_value = Convert.ToDouble(s);
			new_value -= PhysicalConstants.NullTemperatureByCelsiusInKelvin;
			return new_value.ToString();
		}

		/// <summary>
		/// Перевод градусов по Цельсию в Кельвины
		/// </summary>
		private string CToK(string s)
		{
			double new_value = Convert.ToDouble(s);
			new_value += PhysicalConstants.NullTemperatureByCelsiusInKelvin;
			return new_value.ToString();
		}

		/// <summary>
		/// Изменение единиц измерения параметров задаваемых метрами и милиметрами
		/// </summary>
		private void Mm_M_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox cb = (ComboBox)sender;
			TextBox tb = (TextBox)((StackPanel)cb.Parent).Children[1];
			bool to_mm = cb.SelectedIndex == 1;
			bool to_m = cb.SelectedIndex == 0;
			if (to_mm) tb.Text = MToMm(tb.Text);
			if (to_m) tb.Text = MmToM(tb.Text);
		}

		/// <summary>
		/// Изменение единиц измерения параметров задаваемых Кельвинами и градусами по Цельсию
		/// </summary>
		private void K_C_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox cb = (ComboBox)sender;
			TextBox tb = (TextBox)((StackPanel)cb.Parent).Children[1];
			bool to_C = cb.SelectedIndex == 1;
			bool to_K = cb.SelectedIndex == 0;
			if (to_C) tb.Text = KToC(tb.Text);
			if (to_K) tb.Text = CToK(tb.Text);
		}

		#endregion

		private void Button_Click_GasFluidCalculate(object sender, RoutedEventArgs e)
		{
			ShowGasFluidParameters();
		}

        private void ComboBox_PumpEquipment_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
			if(TabControl_Equipment != null && comboBox.SelectedIndex > 1)
				TabControl_Equipment.SelectedIndex = comboBox.SelectedIndex-2;
			else if(TabControl_Equipment != null && comboBox.SelectedIndex == 1)
				TabControl_Equipment.SelectedIndex = 3;

			try
			{
                if (comboBox.SelectedIndex == 0 || comboBox.SelectedIndex == 1)
                {
                    TabItem_ESP.IsEnabled = false;
					((ComboBox)EspDynamicLevel.Children[2]).Foreground = System.Windows.Media.Brushes.Gray;

                    TabItem_PCP.IsEnabled = false;
					((ComboBox)PcpDynamicLevel.Children[2]).Foreground = System.Windows.Media.Brushes.Gray;

                    TabItem_PL.IsEnabled = false;
                    TabItem_PL_StackPanel.IsEnabled = false;
                    //TabItem_PL_ComboBox1.Foreground = System.Windows.Media.Brushes.Gray;
                    //TabItem_PL_ComboBox2.Foreground = System.Windows.Media.Brushes.Gray;
                }
                if (comboBox.SelectedIndex == 2)
                {
                    TabItem_ESP.IsEnabled = true;
					((ComboBox)EspDynamicLevel.Children[2]).Foreground = System.Windows.Media.Brushes.Black;

                    TabItem_PCP.IsEnabled = false;
					((ComboBox)PcpDynamicLevel.Children[2]).Foreground = System.Windows.Media.Brushes.Gray;

                    TabItem_PL.IsEnabled = false;
                    TabItem_PL_StackPanel.IsEnabled = false;
                    //TabItem_PL_ComboBox1.Foreground = System.Windows.Media.Brushes.Gray;
                    //TabItem_PL_ComboBox2.Foreground = System.Windows.Media.Brushes.Gray;
                }
                if (comboBox.SelectedIndex == 3)
                {
                    TabItem_ESP.IsEnabled = false;
					((ComboBox)EspDynamicLevel.Children[2]).Foreground = System.Windows.Media.Brushes.Gray;

                    TabItem_PCP.IsEnabled = true;
					((ComboBox)PcpDynamicLevel.Children[2]).Foreground = System.Windows.Media.Brushes.Black;

                    TabItem_PL.IsEnabled = false;
                    TabItem_PL_StackPanel.IsEnabled = false;
                    //TabItem_PL_ComboBox1.Foreground = System.Windows.Media.Brushes.Gray;
                    //TabItem_PL_ComboBox2.Foreground = System.Windows.Media.Brushes.Gray;
                }
                if (comboBox.SelectedIndex == 4)
                {
                    TabItem_ESP.IsEnabled = false;
					((ComboBox)EspDynamicLevel.Children[2]).Foreground = System.Windows.Media.Brushes.Gray;

                    TabItem_PCP.IsEnabled = false;
					((ComboBox)PcpDynamicLevel.Children[2]).Foreground = System.Windows.Media.Brushes.Gray;

                    TabItem_PL.IsEnabled = true;
                    TabItem_PL_StackPanel.IsEnabled = true;
                    //TabItem_PL_ComboBox1.Foreground = System.Windows.Media.Brushes.Black;
                    //TabItem_PL_ComboBox2.Foreground = System.Windows.Media.Brushes.Black;
                }
            }
            catch { }
        }

		/// <summary>
		/// Выбор ЭЦН из таблицы каталога
		/// </summary>
		private void DataGrid_ESP_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DataBaseESPRow ClickedRow = (DataBaseESPRow)((DataGrid)sender).SelectedItem;
			if (ClickedRow == null)
				return;
			CurrentChosenEsp = new SettingForESP(db.EspPumps.Include("PowerCoefficients").Include("EfficiencyCoefficients")
				.Include("HeadCoefficients").Where(pump => pump.Id == ClickedRow.Id).First());
			ESP_Frequency.Text = CurrentChosenEsp.Frequency.ToString();
			if (CurrentChosenEsp != null) DrawEspChart(CurrentChosenEsp);
		}

		/// <summary>
		/// Выбор ЭЦН из таблицы подобранных ЭЦН 
		/// </summary>
		private void DataGrid_ChosenESP_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ChosingESPRow CRow = (ChosingESPRow)((DataGrid)sender).SelectedItem;
			if (CRow == null)
				return;
			ElectricSubmersiblePump esp = db.EspPumps.Include("PowerCoefficients").Include("EfficiencyCoefficients")
				.Include("HeadCoefficients").Where(pump => pump.Id == CRow.Id).First();
			CurrentChosenEsp = new SettingForESP(esp, CRow.NumberStages, CRow.Frequency);
			if (this.CurrentChosenEsp != null) DrawEspChart(CurrentChosenEsp);
		}

		/// <summary>
		/// Выбор ВШН из таблицы каталога
		/// </summary>
		private void DataGrid_PCP_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DataBasePCPRow ClickedRow = (DataBasePCPRow)((DataGrid)sender).SelectedItem;
			if (ClickedRow == null)
				return;
			CurrentChosenPcp = new SettingForPCP(db.PcpPumps.Include("PowerCoefficients").Include("RateCoefficients")
				.Include("TorqueCoefficients").Where(pump => pump.Id == ClickedRow.Id).First());
			if (CurrentChosenPcp != null) DrawPcpChart(CurrentChosenPcp);
		}

		/// <summary>
		/// Выбор вшН из таблицы подобранных ЭЦН 
		/// </summary>
		private void DataGrid_ChosenPCP_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ChosingPCPRow CRow = (ChosingPCPRow)((DataGrid)sender).SelectedItem;
			if (CRow == null)
				return;
			ProgressiveCavityPump pcp = db.PcpPumps.Include("PowerCoefficients").Include("RateCoefficients")
				.Include("TorqueCoefficients").Where(pump => pump.Id == CRow.Id).First();
			CurrentChosenPcp = new SettingForPCP(pcp, CRow.Speed, CRow.Head);
			if (this.CurrentChosenPcp != null) DrawPcpChart(CurrentChosenPcp);
		}
	}

	/// <summary>
	/// Класс для связи таблицы компонентов с программным кодом
	/// Строка таблицы компонентов 
	/// </summary>
	public class ComponetGridRow : INotifyPropertyChanged
    {
        /// <summary>
        /// Название компонента
        /// </summary>
        public string NameComponent
        {
            get { return _nameComponent; }
            set { _nameComponent = value; }
        }
		public string _nameComponent;

		/// <summary>
		/// Формула компонента
		/// </summary>
        public string FormulaComponent
        {
            get { return _formulaComponent; }
            set { _formulaComponent = value; }
        }
		public string _formulaComponent;

		/// <summary>
		/// Молярная доля в процентах
		/// </summary>
		public string MoleFractionGridRepresentation 
        {            
            get { return _moleFractionGridRepresentation; }
            set 
            {
                _moleFractionGridRepresentation = value;
                try
                {
                    MoleFractionInPercents = Convert.ToDouble(value);
                }
                catch { MoleFractionInPercents = 0; }
                //Обновление данных в таблице компонентов
                OnPropertyChanged("MoleFractionGridRepresentation");
            }
        }
		public string _moleFractionGridRepresentation;

		/// <summary>
		/// Обработка события изменения значения поля
		/// </summary>
		/// <param name="property">Поле, которое подверглось изменению</param>
		private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        /// <summary>
        /// Событие изменения свойства
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Молярная доля (%)
        /// </summary>
        public double MoleFractionInPercents;
    }

    /// <summary>
    /// Класс для связи таблицы скважин с базой данных
    /// Строка таблицы скважины 
    /// </summary>
    public class RowWellGrid
    {
        /// <summary>
        /// Номер скважины
        /// </summary>
        public string NumberWell
        {
            get { return _numberWell; }
            set { _numberWell = value; }
        }
		public string _numberWell;

		/// <summary>
		/// Название скважины
		/// </summary>
        public string NameWell
        {
            get { return _nameWell; }
            set { _nameWell = value; }
        }
		public string _nameWell;

		/// <summary>
		/// Дата записи данных о скважине
		/// </summary>
        public string DateWell
        {
            get { return _dateWell; }
            set { _dateWell = value; }
        }
		public string _dateWell;
	}

	/// <summary>
	/// Класс для вывода таблицы данных о НКТ
	/// Строка таблицы исследование по НКТ 
	/// </summary>
	public class RowResearchNKT
    {
        /// <summary>
        /// Глубина
        /// </summary>
        public double Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }
		public double _depth;

		/// <summary>
		/// Температура
		/// </summary>
        public double Temperature
        {
            get { return _temperature; }
            set { _temperature = value; }
        }
		public double _temperature;

		/// <summary>
		/// Давление
		/// </summary>
        public double Pressure
        {
            get { return _pressure; }
            set { _pressure = value; }
        }
		public double _pressure;
	}

	/// <summary>
	/// Класс для вывода таблицы подобранных ЭЦН
	/// </summary>
	public class ChosingESPRow : DataBaseESPRow
	{
        public int NumberStages { get; set; }

        public double Frequency { get; set; }

		public double Rate { get; set; }
	}


	/// <summary>
	/// Класс для вывода таблицы базы данных ЭЦН
	/// </summary>
	public class DataBaseESPRow
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public double Diameter { get; set; }

		public double NominalRate { get; set; }

		public double Head { get; set; }

		public double Power { get; set; }

		public double Efficiency { get; set; }
	}

	/// <summary>
	/// Класс для вывода таблицы  подобранных ВШН
	/// </summary>
	public class ChosingPCPRow : DataBasePCPRow
	{
		public double Speed { get; set; }

		public double Rate { get; set; }

		public double Head { get; set; }

		public double Power { get; set; }

		public double Torque { get; set; }
	}

	/// <summary>
	/// Класс для вывода таблицы базы данных ВШН
	/// </summary>
	public class DataBasePCPRow
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public double Diameter { get; set; }

		public double NominalRate { get; set; }

		public double NominalSpeed { get; set; }
	}

	/// <summary>
	/// Класс для обработки операции вставки в текстовое поле (ctrl + v)
	/// </summary>
	public class TextBoxPasteBehavior
    {
        public static readonly DependencyProperty PasteCommandProperty = DependencyProperty.RegisterAttached("PasteCommand", typeof(ICommand),typeof(TextBoxPasteBehavior), 
            new FrameworkPropertyMetadata(PasteCommandChanged));

        public static ICommand GetPasteCommand(DependencyObject target)
        {
            return (ICommand)target.GetValue(PasteCommandProperty);
        }

        public static void SetPasteCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(PasteCommandProperty, value);
        }

        static void PasteCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var newValue = (ICommand)e.NewValue;

            if (newValue != null)
                textBox.AddHandler(CommandManager.ExecutedEvent, new RoutedEventHandler(CommandExecuted), true);
            else
                textBox.RemoveHandler(CommandManager.ExecutedEvent, new RoutedEventHandler(CommandExecuted));

        }

        static void CommandExecuted(object sender, RoutedEventArgs e)
        {
            if (((ExecutedRoutedEventArgs)e).Command != ApplicationCommands.Paste) return;

            var textBox = (TextBox)sender;
            var command = GetPasteCommand(textBox);

            if (command.CanExecute(null))
                command.Execute(textBox);
        }
    }
}
