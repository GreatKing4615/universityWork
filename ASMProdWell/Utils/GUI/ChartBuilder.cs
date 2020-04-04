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

namespace ASMProdWell.Utils.GUI
{
    public class ChartBuilder
    {
		/// <summary>
		/// Загрузка графика узлового анализа
		/// </summary>
		public static void DrawNodalAnalysisChart(Chart Сhart_NodalAnalysis, int num, double[] P1, double[] P2, double[] Q, double[] waterQ = null)
		{
			Сhart_NodalAnalysis.ChartAreas.Clear();
			Сhart_NodalAnalysis.Legends.Clear();
			Сhart_NodalAnalysis.Series.Clear();
			var Area = new ChartArea("Area");
			Area.Position = new ElementPosition(-3, 5, 110, 85);
			Area.InnerPlotPosition = new ElementPosition(12, 10, 75, 80);
			Сhart_NodalAnalysis.ChartAreas.Add(Area);
			Сhart_NodalAnalysis.Legends.Add(new Legend("Legend"));
			Сhart_NodalAnalysis.Palette = ChartColorPalette.Bright;
			Сhart_NodalAnalysis.ChartAreas["Area"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			Сhart_NodalAnalysis.ChartAreas["Area"].AxisY.MajorGrid.LineColor = Color.Gray;
			Сhart_NodalAnalysis.ChartAreas["Area"].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			Сhart_NodalAnalysis.ChartAreas["Area"].AxisX.MajorGrid.LineColor = Color.Gray;
			Сhart_NodalAnalysis.ChartAreas["Area"].AxisX2.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			Сhart_NodalAnalysis.ChartAreas["Area"].AxisX2.MajorGrid.LineColor = Color.Gray;

			// Добавим линию оттока
			Сhart_NodalAnalysis.Series.Add(new Series("Отток газа"));
			Сhart_NodalAnalysis.Series["Отток газа"].ChartArea = "Area";
			Сhart_NodalAnalysis.Series["Отток газа"].Legend = "Legend";
			Сhart_NodalAnalysis.Series["Отток газа"].ChartType = SeriesChartType.Line;
			Сhart_NodalAnalysis.Series["Отток газа"].Points.DataBindXY(Q, P1);
			Сhart_NodalAnalysis.Series["Отток газа"].BorderWidth = 3;

			// Добавим линию притока газа
			Сhart_NodalAnalysis.Series.Add(new Series("Приток газа"));
			Сhart_NodalAnalysis.Series["Приток газа"].ChartArea = "Area";
			Сhart_NodalAnalysis.Series["Приток газа"].Legend = "Legend";
			Сhart_NodalAnalysis.Series["Приток газа"].ChartType = SeriesChartType.Line;
			Сhart_NodalAnalysis.Series["Приток газа"].YAxisType = AxisType.Primary;
			Сhart_NodalAnalysis.Series["Приток газа"].Points.DataBindXY(Q, P2);
			Сhart_NodalAnalysis.Series["Приток газа"].BorderWidth = 3;

			// Добавим линию притока жидкости
			if (waterQ != null)
			{
                List<double> newWaterQ = new List<double>();
                List<double> newP2 = new List<double>();
                for(int i=0; i<waterQ.Count()-1; i++)
                {
                    if (waterQ[i + 1] == 0) continue;
                    newWaterQ.Add(waterQ[i]);
                    newP2.Add(P2[i]);
                }
				Сhart_NodalAnalysis.Series.Add(new Series("Приток жидкости"));
				Сhart_NodalAnalysis.Series["Приток жидкости"].ChartArea = "Area";
				Сhart_NodalAnalysis.Series["Приток жидкости"].Legend = "Legend";
				Сhart_NodalAnalysis.Series["Приток жидкости"].ChartType = SeriesChartType.Line;
				Сhart_NodalAnalysis.Series["Приток жидкости"].YAxisType = AxisType.Primary;
				Сhart_NodalAnalysis.Series["Приток жидкости"].XAxisType = AxisType.Secondary;
				Сhart_NodalAnalysis.Series["Приток жидкости"].Points.DataBindXY(newWaterQ, newP2);
				Сhart_NodalAnalysis.Series["Приток жидкости"].BorderWidth = 3;
			}

			Axis areaAxisAxisY = Area.AxisY;
			areaAxisAxisY.IsStartedFromZero = Area.AxisY.IsStartedFromZero;
			areaAxisAxisY.LabelStyle.Font = Area.AxisY.LabelStyle.Font;
			areaAxisAxisY.Title = "Забойное давление, МПа";
			areaAxisAxisY.TitleForeColor = Color.DarkCyan;
            areaAxisAxisY.LabelStyle.Interval = Math.Ceiling(P2[0]) / 10;
            areaAxisAxisY.Interval = Math.Ceiling(P2[0]) / 10;
            areaAxisAxisY.Minimum = 0;
            areaAxisAxisY.Maximum = Math.Ceiling(P2[0]);
			areaAxisAxisY.LineColor = Color.Black;
			areaAxisAxisY.RoundAxisValues();

			Axis areaAxisAxisX = Area.AxisX;
			areaAxisAxisX.IsStartedFromZero = Area.AxisX.IsStartedFromZero;
			areaAxisAxisX.LabelStyle.Font = Area.AxisX.LabelStyle.Font;
			areaAxisAxisX.Title = "Дебит газа, тыс.м³/сут";
			areaAxisAxisX.TitleForeColor = Color.DarkCyan;
			areaAxisAxisX.LabelStyle.Interval = Q[Q.Length - 1] / 10;
			areaAxisAxisX.Interval = Q[Q.Length - 1] / 10;
			areaAxisAxisX.Minimum = 0;
			areaAxisAxisX.Maximum = Q[Q.Length - 1];
			areaAxisAxisX.LabelStyle.Format = "0";
			areaAxisAxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;

			if (waterQ != null)
			{
				Axis areaAxisAxisX2 = Area.AxisX2;
				areaAxisAxisX2.IsStartedFromZero = Area.AxisX.IsStartedFromZero;
				areaAxisAxisX2.LabelStyle.Font = Area.AxisX.LabelStyle.Font;
				areaAxisAxisX2.Title = "Дебит жидкости, м³/сут";
				areaAxisAxisX2.TitleForeColor = Color.DarkCyan;
				double waterMax;
				if (waterQ[0] > waterQ[waterQ.Length - 1])
				{
					waterMax = waterQ[0];
				}
				else
				{
					waterMax = waterQ[waterQ.Length - 1];
				}
				areaAxisAxisX2.LabelStyle.Interval = waterMax / 10;
				areaAxisAxisX2.Interval = waterMax / 10;
				areaAxisAxisX2.Minimum = 0;
				areaAxisAxisX2.Maximum = waterMax;
				areaAxisAxisX2.IsStartedFromZero = false;
				areaAxisAxisX2.LabelStyle.Format = "0";
				areaAxisAxisX2.IntervalAutoMode = IntervalAutoMode.VariableCount;
			}
		}


		public static void DrawFallingGasRateChart(Chart GasFallingRateAndWaterAccumulation, double time_work, List<double> gasRates, List<double> waterRates, double time_rec = 0)
		{
			int num_cycles = 1;
			double time_cycle = time_work + time_rec;
			int size = waterRates.Count;
			List<double> time = new List<double>();
			double dt = time_work / waterRates.Count;
			for (int i = 0; i < size; i++)
				time.Add(dt * i);

			for (int y = 1; y < num_cycles; y++)
			{
				time.Add(time.Last());
				gasRates.Add(0);
				waterRates.Add(0);

				time.Add(time.Last() + time_rec);
				gasRates.Add(0);
				waterRates.Add(0);

				for (int i = 0; i < size; i++)
				{
					time.Add(time[i % size] + y * time_cycle);
					gasRates.Add(gasRates[i % size]);
					waterRates.Add(waterRates[i % size]);
				}
			}

			GasFallingRateAndWaterAccumulation.ChartAreas.Clear();
			GasFallingRateAndWaterAccumulation.Legends.Clear();
			GasFallingRateAndWaterAccumulation.Series.Clear();
			var Area = new ChartArea("Area");
			Area.Position = new ElementPosition(0, 10, 105, 85);
			Area.InnerPlotPosition = new ElementPosition(12, 0, 70, 90);
			GasFallingRateAndWaterAccumulation.ChartAreas.Add(Area);
			GasFallingRateAndWaterAccumulation.Legends.Add(new Legend("Legend"));
			GasFallingRateAndWaterAccumulation.Palette = ChartColorPalette.Bright;
			GasFallingRateAndWaterAccumulation.ChartAreas["Area"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			GasFallingRateAndWaterAccumulation.ChartAreas["Area"].AxisY.MajorGrid.LineColor = Color.Gray;
			GasFallingRateAndWaterAccumulation.ChartAreas["Area"].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			GasFallingRateAndWaterAccumulation.ChartAreas["Area"].AxisX.MajorGrid.LineColor = Color.Gray;
			GasFallingRateAndWaterAccumulation.ChartAreas["Area"].AxisY2.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			GasFallingRateAndWaterAccumulation.ChartAreas["Area"].AxisY2.MajorGrid.LineColor = Color.Gray;


			// Добавим линии
			GasFallingRateAndWaterAccumulation.Series.Add(new Series("Газ"));
			GasFallingRateAndWaterAccumulation.Series["Газ"].ChartArea = "Area";
			GasFallingRateAndWaterAccumulation.Series["Газ"].Legend = "Legend";
			GasFallingRateAndWaterAccumulation.Series["Газ"].ChartType = SeriesChartType.Line;


			GasFallingRateAndWaterAccumulation.Series.Add(new Series("Скопление воды"));
			GasFallingRateAndWaterAccumulation.Series["Скопление воды"].ChartArea = "Area";
			GasFallingRateAndWaterAccumulation.Series["Скопление воды"].Legend = "Legend";
			GasFallingRateAndWaterAccumulation.Series["Скопление воды"].ChartType = SeriesChartType.Line;
			GasFallingRateAndWaterAccumulation.Series["Скопление воды"].YAxisType = AxisType.Secondary;

			// Добавим данные линии
			double[] x = time.ToArray();
			double[] y1 = gasRates.ToArray();
			GasFallingRateAndWaterAccumulation.Series["Газ"].Points.DataBindXY(x, y1);
			GasFallingRateAndWaterAccumulation.Series["Газ"].BorderWidth = 3;

			double[] y2 = waterRates.ToArray();
			GasFallingRateAndWaterAccumulation.Series["Скопление воды"].Points.DataBindXY(x, y2);
			GasFallingRateAndWaterAccumulation.Series["Скопление воды"].BorderWidth = 3;

			Axis areaAxisAxisY = Area.AxisY;
			areaAxisAxisY.IsStartedFromZero = Area.AxisY.IsStartedFromZero;
			areaAxisAxisY.LabelStyle.Font = Area.AxisY.LabelStyle.Font;
			areaAxisAxisY.Title = "Дебит газа, тыс.м³/сут";
			areaAxisAxisY.TitleForeColor = Color.DarkCyan;
			double deltaWG = Math.Abs(gasRates.Last() - gasRates.First());
			areaAxisAxisY.LabelStyle.Interval = deltaWG / 6;
			areaAxisAxisY.Interval = gasRates[0] / 6;
			if (gasRates.Last() - 0.5 * deltaWG < 0)
				areaAxisAxisY.Minimum = 0;
			else
				areaAxisAxisY.Minimum = gasRates.Last() - 0.5 * deltaWG;
			areaAxisAxisY.Maximum = gasRates.First() + 0.5 * deltaWG;
			areaAxisAxisY.LabelStyle.Format = "0.00";
			//areaAxisAxisY.LineWidth = 3;

			Axis areaAxisAxisY2 = Area.AxisY2;
			areaAxisAxisY2.IsStartedFromZero = Area.AxisY.IsStartedFromZero;
			areaAxisAxisY2.LabelStyle.Font = Area.AxisY.LabelStyle.Font;
			areaAxisAxisY2.Title = "Скопление воды на забое, м³/сут";
			areaAxisAxisY2.TitleForeColor = Color.DarkCyan;
			double deltaWR = Math.Abs(waterRates.Last() - waterRates.First());
			areaAxisAxisY2.LabelStyle.Interval = deltaWR * 2 / 6;
			areaAxisAxisY2.Interval = deltaWR * 2 / 6;
			areaAxisAxisY2.Minimum = 0;
			areaAxisAxisY2.Maximum = deltaWR * 3;
			areaAxisAxisY2.LabelStyle.Format = "0.00";
			//areaAxisAxisY2.LineWidth = 3;

			Axis areaAxisAxisX = Area.AxisX;
			areaAxisAxisX.IsStartedFromZero = Area.AxisX.IsStartedFromZero;
			areaAxisAxisX.LabelStyle.Font = Area.AxisX.LabelStyle.Font;
			areaAxisAxisX.Title = "Время, сут";
			areaAxisAxisX.TitleForeColor = Color.DarkCyan;
			areaAxisAxisX.LabelStyle.Interval = time[time.Count - 1] / 3;
			areaAxisAxisX.Interval = time[time.Count - 1] / 3;
			areaAxisAxisX.Minimum = 0;
			areaAxisAxisX.Maximum = time_cycle - time_rec;
			areaAxisAxisX.LabelStyle.Format = "0.000";
		}

		/// <summary>
		/// Загрузка графика градиента давления
		/// </summary>
		public static void DrawPressureGradientChart(Chart Chart_ResearchNKT, double[] H, double[] P)
		{
			Chart_ResearchNKT.ChartAreas.Clear();
			Chart_ResearchNKT.Legends.Clear();
			Chart_ResearchNKT.Series.Clear();
			var Area = new ChartArea("Area");
			Area.Position = new ElementPosition(-3, 10, 110, 85);
			Area.InnerPlotPosition = new ElementPosition(12, 0, 75, 90);
			Chart_ResearchNKT.ChartAreas.Add(Area);
			Chart_ResearchNKT.Legends.Add(new Legend("Legend"));
			Chart_ResearchNKT.Palette = ChartColorPalette.Bright;
			Chart_ResearchNKT.ChartAreas["Area"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			Chart_ResearchNKT.ChartAreas["Area"].AxisY.MajorGrid.LineColor = Color.Gray;
			Chart_ResearchNKT.ChartAreas["Area"].AxisX2.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			Chart_ResearchNKT.ChartAreas["Area"].AxisX2.MajorGrid.LineColor = Color.Gray;

			// Добавим линию оттока
			Chart_ResearchNKT.Series.Add(new Series("Градиент"));
			Chart_ResearchNKT.Series["Градиент"].ChartArea = "Area";
			Chart_ResearchNKT.Series["Градиент"].Legend = "Legend";
			Chart_ResearchNKT.Series["Градиент"].ChartType = SeriesChartType.Line;
			Chart_ResearchNKT.Series["Градиент"].XAxisType = AxisType.Secondary;

			// Добавим данные линии
			Chart_ResearchNKT.Series["Градиент"].Points.DataBindXY(P, H);
			Chart_ResearchNKT.Series["Градиент"].BorderWidth = 3;

			Axis areaAxisAxisY = Area.AxisY;
			areaAxisAxisY.IsReversed = true;
			areaAxisAxisY.Title = "Глубина, м";
			areaAxisAxisY.TitleForeColor = Color.DarkCyan;
			areaAxisAxisY.LineWidth = 1;
			areaAxisAxisY.LineColor = Color.Black;
			areaAxisAxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;

			Axis areaAxisAxisX2 = Area.AxisX2;
			areaAxisAxisX2.Title = "Давление, МПа";
			areaAxisAxisX2.TitleForeColor = Color.DarkCyan;
			areaAxisAxisX2.LabelStyle.Interval = (P[P.Length - 1] - P[0]) / 10;
			areaAxisAxisX2.LabelStyle.TruncatedLabels = false;
			areaAxisAxisX2.LabelStyle.Format = "0.000";
			areaAxisAxisX2.LabelAutoFitMaxFontSize = 9;
			areaAxisAxisX2.IntervalAutoMode = IntervalAutoMode.VariableCount;
			areaAxisAxisX2.Interval = (P[P.Length - 1] - P[0]) / 10;
		}


		/// <summary>
		/// Загрузка графика ПЛ
		/// </summary>
		public static void DrawWaterAccumulationChart(Chart Chart_PL, List<double> times, List<double> press)
		{
			int num_cycles = 3;
			int size = times.Count;
			List<double> Xtime = new List<double>();
			List<double> Ypress = new List<double>();
			for (int i = 0; i < size * (num_cycles); i++)
			{
				if (i == 0)
					Xtime.Add(times[i % size]);
				else
					Xtime.Add(Xtime.Last() + times[i % size]);
				Ypress.Add(press[i % size]);
			}

			Chart_PL.ChartAreas.Clear();
			Chart_PL.Legends.Clear();
			Chart_PL.Series.Clear();
			var Area = new ChartArea("Area");
			Area.Position = new ElementPosition(-5, 10, 115, 85);
			Area.InnerPlotPosition = new ElementPosition(12, 0, 75, 90);
			Chart_PL.ChartAreas.Add(Area);
			Chart_PL.Legends.Add(new Legend("Legend"));
			Chart_PL.Palette = ChartColorPalette.Bright;
			Chart_PL.ChartAreas["Area"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			Chart_PL.ChartAreas["Area"].AxisY.MajorGrid.LineColor = Color.Gray;
			Chart_PL.ChartAreas["Area"].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			Chart_PL.ChartAreas["Area"].AxisX.MajorGrid.LineColor = Color.Gray;


			// Добавим линию
			Chart_PL.Series.Add(new Series("Забойное давление"));
			Chart_PL.Series["Забойное давление"].ChartArea = "Area";
			Chart_PL.Series["Забойное давление"].Legend = "Legend";
			Chart_PL.Series["Забойное давление"].ChartType = SeriesChartType.Line;


			Chart_PL.Series.Add(new Series("Точки графика"));
			Chart_PL.Series["Точки графика"].ChartArea = "Area";
			Chart_PL.Series["Точки графика"].ChartType = SeriesChartType.Point;
			Chart_PL.Series["Точки графика"].IsVisibleInLegend = false;
			Chart_PL.Series["Точки графика"].MarkerStyle = MarkerStyle.Circle;
			Chart_PL.Series["Точки графика"].MarkerColor = Color.DarkRed;
			Chart_PL.Series["Точки графика"].MarkerSize = 10;
			//Chart_PL.Series["Точки графика"].MarkerSize = 2;

			// Добавим данные линии
			double[] x = Xtime.ToArray();
			double[] y = Ypress.ToArray();
			Chart_PL.Series["Забойное давление"].Points.DataBindXY(x, y);
			Chart_PL.Series["Забойное давление"].BorderWidth = 3;

			Chart_PL.Series["Точки графика"].Points.DataBindXY(x, y);
			Chart_PL.Series["Точки графика"].Color = Color.Green;

			Axis areaAxisAxisY = Area.AxisY;
			areaAxisAxisY.IsStartedFromZero = Area.AxisY.IsStartedFromZero;
			areaAxisAxisY.LabelStyle.Font = Area.AxisY.LabelStyle.Font;
			areaAxisAxisY.Title = "Давление, МПа";
			areaAxisAxisY.TitleForeColor = Color.DarkCyan;
			areaAxisAxisY.IsStartedFromZero = false;

			Axis areaAxisAxisX = Area.AxisX;
			areaAxisAxisX.IsStartedFromZero = Area.AxisX.IsStartedFromZero;
			areaAxisAxisX.LabelStyle.Font = Area.AxisX.LabelStyle.Font;
			areaAxisAxisX.Title = "Время, сут";
			areaAxisAxisX.TitleForeColor = Color.DarkCyan;
			areaAxisAxisX.LabelStyle.Interval = Xtime[Xtime.Count - 1] / 12;
			areaAxisAxisX.Interval = Xtime[Xtime.Count - 1] / 12;
			areaAxisAxisX.Minimum = 0;
			areaAxisAxisX.Maximum = Xtime[Xtime.Count - 1];
			areaAxisAxisX.LabelStyle.Format = "0.0000";
		}



		/// <summary>
		/// Загрузка графика ВШН
		/// </summary>
		public static void DrawPcpChart(Chart Chart_PCP, SettingForPCP settings)
		{
			Chart_PCP.ChartAreas.Clear();
			Chart_PCP.Legends.Clear();
			Chart_PCP.Series.Clear();
			var Area = new ChartArea("Area");
			Area.Position = new ElementPosition(10, 10, 91, 85);
			Area.InnerPlotPosition = new ElementPosition(12, 0, 75, 90);
			Chart_PCP.ChartAreas.Add(Area);
			Chart_PCP.Legends.Add(new Legend("Legend"));
			Chart_PCP.Palette = ChartColorPalette.Bright;
			Chart_PCP.ChartAreas["Area"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			Chart_PCP.ChartAreas["Area"].AxisY.MajorGrid.LineColor = Color.Gray;
			Chart_PCP.ChartAreas["Area"].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			Chart_PCP.ChartAreas["Area"].AxisX.MajorGrid.LineColor = Color.Gray;


			// Добавим линию расхода
			Chart_PCP.Series.Add(new Series("Расход"));
			Chart_PCP.Series["Расход"].ChartArea = "Area";
			Chart_PCP.Series["Расход"].Legend = "Legend";
			Chart_PCP.Series["Расход"].ChartType = SeriesChartType.Line;

			// Добавим линию Кр. момент
			Chart_PCP.Series.Add(new Series("Кр. момент"));
			Chart_PCP.Series["Кр. момент"].ChartArea = "Area";
			Chart_PCP.Series["Кр. момент"].Legend = "Legend";
			Chart_PCP.Series["Кр. момент"].ChartType = SeriesChartType.Line;
			Chart_PCP.Series["Кр. момент"].YAxisType = AxisType.Primary;

			// Добавим линию Мощность
			Chart_PCP.Series.Add(new Series("Мощность"));
			Chart_PCP.Series["Мощность"].ChartArea = "Area";
			Chart_PCP.Series["Мощность"].Legend = "Legend";
			Chart_PCP.Series["Мощность"].ChartType = SeriesChartType.Line;


			// Добавим данные линии
			double[] xArrayGasH = getXArrayGasH_PCP(settings);
			double[] yArrayRate = getYArrayLine_PCP(xArrayGasH, Chart_PCP.Series["Расход"].Name, settings);
			Chart_PCP.Series["Расход"].Points.DataBindXY(xArrayGasH, yArrayRate);
			Chart_PCP.Series["Расход"].BorderWidth = 3;

			double[] yArrayEfficiency = getYArrayLine_PCP(xArrayGasH, Chart_PCP.Series["Кр. момент"].Name, settings);
			Chart_PCP.Series["Кр. момент"].Points.DataBindXY(xArrayGasH, yArrayEfficiency);
			Chart_PCP.Series["Кр. момент"].BorderWidth = 3;

			double[] yArrayPower = getYArrayLine_PCP(xArrayGasH, Chart_PCP.Series["Мощность"].Name, settings);
			Chart_PCP.Series["Мощность"].Points.DataBindXY(xArrayGasH, yArrayPower);
			Chart_PCP.Series["Мощность"].BorderWidth = 3;

			// Дополнительные шкалы для графиков
			CreateYAxis(Chart_PCP, Area, Chart_PCP.Series["Расход"], 6, 17, 18, true);
			CreateYAxis(Chart_PCP, Area, Chart_PCP.Series["Мощность"], 67, 26, -3, false);

			Axis areaAxisAxisY = Area.AxisY;
			areaAxisAxisY.IsStartedFromZero = Area.AxisY.IsStartedFromZero;
			areaAxisAxisY.LabelStyle.Font = Area.AxisY.LabelStyle.Font;
			areaAxisAxisY.Title = Chart_PCP.Series["Кр. момент"].Name + ", кгс·м";
			areaAxisAxisY.TitleForeColor = Color.DarkCyan;
			areaAxisAxisY.Minimum = 0;
			areaAxisAxisY.LineWidth = 3;
			areaAxisAxisY.LineColor = Chart_PCP.Series["Кр. момент"].Color;

			Axis areaAxisAxisX = Area.AxisX;
			areaAxisAxisX.Minimum = 0;
			areaAxisAxisX.IsStartedFromZero = Area.AxisX.IsStartedFromZero;
			areaAxisAxisX.LabelStyle.Font = Area.AxisX.LabelStyle.Font;
			areaAxisAxisX.Title = "Напор, м";
			areaAxisAxisX.TitleForeColor = Color.DarkCyan;
		}


		/// <summary>
		/// Формирование массива значений оси Х графика ВШН
		/// </summary>
		public static double[] getXArrayGasH_PCP(SettingForPCP pcp)
		{
			double dH = 4.0;
			int size = 0;
			for (size = 0; size < 2000; size++)
				if (pcp.CalcRate(size * dH, pcp.Speed) <= 0)
					break;

			double[] arr = new double[size];
			for (int i = 0; i < size; i++)
				arr[i] = i * dH;

			return arr;
		}

		/// <summary>
		/// Формирование массива значений оси Y
		/// </summary>
		public static double[] getYArrayLine_PCP(double[] xValue, string Line, SettingForPCP pcp)
		{
			double speed = pcp.Speed;

			int size = xValue.Length;
			double[] arr = new double[size];

			for (int i = 0; i < size; i++)
			{
				double H = xValue[i];

				switch (Line)
				{
					case "Расход":
						arr[i] = pcp.CalcRate(H, speed);
						break;
					case "Кр. момент":
						arr[i] = pcp.CalcTorque(H, speed);
						break;
					case "Мощность":
						arr[i] = pcp.CalcPower(H, speed);
						break;
					default:
						MessageBox.Show("Ошибка при выводе графика ВШН", "Ошибка!");
						return null;
				}
			}

			return arr;
		}

		
		/// <summary>
		/// Загрузка графика найстоенного ЭЦН 
		/// </summary>
		public static void DrawEspChart(Chart Chart_ESP, SettingForESP settings)
		{
			Chart_ESP.ChartAreas.Clear();
			Chart_ESP.Legends.Clear();
			Chart_ESP.Series.Clear();
			var Area = new ChartArea("Area");
			Area.Position = new ElementPosition(10, 10, 91, 85);
			Area.InnerPlotPosition = new ElementPosition(12, 0, 75, 90);
			Chart_ESP.ChartAreas.Add(Area);
			Chart_ESP.Legends.Add(new Legend("Legend"));
			Chart_ESP.Palette = ChartColorPalette.Bright;
			Chart_ESP.ChartAreas["Area"].AxisY.Maximum = 100;
			Chart_ESP.ChartAreas["Area"].AxisY2.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			Chart_ESP.ChartAreas["Area"].AxisY2.MajorGrid.LineColor = Color.Gray;
			Chart_ESP.ChartAreas["Area"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			Chart_ESP.ChartAreas["Area"].AxisY.MajorGrid.LineColor = Color.Gray;
			Chart_ESP.ChartAreas["Area"].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			Chart_ESP.ChartAreas["Area"].AxisX.MajorGrid.LineColor = Color.Gray;

			//Добавим линию R
			Chart_ESP.Series.Add(new Series("R"));
			Chart_ESP.Series["R"].IsVisibleInLegend = false;
			Chart_ESP.Series["R"].Color = Color.PaleGreen;
			Chart_ESP.Series["R"].Color = Color.FromArgb(70, Color.Green);
			Chart_ESP.Series["R"].ChartType = SeriesChartType.Column;
			Chart_ESP.Series["R"].YAxisType = AxisType.Primary;
			Chart_ESP.Series["R"]["PointWidth"] = ((int)(settings.MaxRecRate - settings.MinRecRate)).ToString();
			List<double> xR = new List<double>() { (settings.MaxRecRate + settings.MinRecRate) / 2 };
			List<double> yR = new List<double>() { 100 };
			Chart_ESP.Series["R"].Points.DataBindXY(xR, yR);

			// Добавим линию напора
			Chart_ESP.Series.Add(new Series("Напор"));
			Chart_ESP.Series["Напор"].ChartArea = "Area";
			Chart_ESP.Series["Напор"].Legend = "Legend";
			Chart_ESP.Series["Напор"].ChartType = SeriesChartType.Line;

			// Добавим линию КПД
			Chart_ESP.Series.Add(new Series("КПД"));
			Chart_ESP.Series["КПД"].ChartArea = "Area";
			Chart_ESP.Series["КПД"].Legend = "Legend";
			Chart_ESP.Series["КПД"].ChartType = SeriesChartType.Line;
			Chart_ESP.Series["КПД"].YAxisType = AxisType.Primary;

			// Добавим линию Мощность
			Chart_ESP.Series.Add(new Series("Мощность"));
			Chart_ESP.Series["Мощность"].ChartArea = "Area";
			Chart_ESP.Series["Мощность"].Legend = "Legend";
			Chart_ESP.Series["Мощность"].ChartType = SeriesChartType.Line;
			Chart_ESP.Series["Мощность"].YAxisType = AxisType.Secondary;

			// Добавим данные линии
			double[] xArrayGasQ = getXArrayGasQ(settings);
			double[] yArrayHead = getYArrayLine(xArrayGasQ, Chart_ESP.Series["Напор"].Name, settings); ;
			Chart_ESP.Series["Напор"].Points.DataBindXY(xArrayGasQ, yArrayHead);
			Chart_ESP.Series["Напор"].BorderWidth = 3;

			double[] yArrayEfficiency = getYArrayLine(xArrayGasQ, Chart_ESP.Series["КПД"].Name, settings);
			Chart_ESP.Series["КПД"].Points.DataBindXY(xArrayGasQ, yArrayEfficiency);
			Chart_ESP.Series["КПД"].BorderWidth = 3;

			double[] yArrayPower = getYArrayLine(xArrayGasQ, Chart_ESP.Series["Мощность"].Name, settings); ;
			Chart_ESP.Series["Мощность"].Points.DataBindXY(xArrayGasQ, yArrayPower);
			Chart_ESP.Series["Мощность"].BorderWidth = 3;

			// Дополнительные шкалы для графиков
			CreateYAxis(Chart_ESP, Area, Chart_ESP.Series["Напор"], 6, 17, 18, true);

			Axis areaAxisAxisY = Area.AxisY;
			areaAxisAxisY.IsStartedFromZero = Area.AxisY.IsStartedFromZero;
			areaAxisAxisY.LabelStyle.Font = Area.AxisY.LabelStyle.Font;
			areaAxisAxisY.Title = Chart_ESP.Series["КПД"].Name + ", %";
			areaAxisAxisY.TitleForeColor = Color.DarkCyan;
			areaAxisAxisY.Minimum = 0;
			areaAxisAxisY.LineWidth = 3;
			areaAxisAxisY.LineColor = Chart_ESP.Series["КПД"].Color;

			Axis areaAxisAxisY2 = Area.AxisY2;
			areaAxisAxisY2.IsStartedFromZero = Area.AxisY2.IsStartedFromZero;
			areaAxisAxisY2.LabelStyle.Font = Area.AxisY2.LabelStyle.Font;
			areaAxisAxisY2.Title = Chart_ESP.Series["Мощность"].Name + ", кВт";
			areaAxisAxisY2.TitleForeColor = Color.DarkCyan;
			areaAxisAxisY2.Maximum = yArrayPower.Last() * 2;
			areaAxisAxisY2.Minimum = 0;
			areaAxisAxisY2.LineWidth = 3;
			areaAxisAxisY2.LineColor = Chart_ESP.Series["Мощность"].Color;
			areaAxisAxisY2.LabelStyle.Format = "0.000";

			Axis areaAxisAxisX = Area.AxisX;
			areaAxisAxisX.IsStartedFromZero = Area.AxisX.IsStartedFromZero;
			areaAxisAxisX.LabelStyle.Font = Area.AxisX.LabelStyle.Font;
			areaAxisAxisX.Title = "Расход, м³/сут";
			areaAxisAxisX.Minimum = 0;
			areaAxisAxisX.TitleForeColor = Color.DarkCyan;

			//Добавим линию R1
			Chart_ESP.Series.Add(new Series("R1"));
			Chart_ESP.Series["R1"].IsVisibleInLegend = false;
			Chart_ESP.Series["R1"].ChartType = SeriesChartType.StepLine;
			Chart_ESP.Series["R1"].YAxisType = AxisType.Primary;
			Chart_ESP.Series["R1"].BorderDashStyle = ChartDashStyle.Dash;
			Chart_ESP.Series["R1"].Color = Color.FromArgb(48, 30, 12);
			Chart_ESP.Series["R1"].BorderWidth = 2;
			double[] xR1 = new double[] { settings.NomRate, settings.NomRate };
			double[] yR1 = new double[] { 100, 0 };
			Chart_ESP.Series["R1"].Points.DataBindXY(xR1, yR1);
		}


		/// <summary>
		/// Формирование массива значений оси Х
		/// </summary>
		private static double[] getXArrayGasQ(SettingForESP setEsp)
		{
			double freq = setEsp.Frequency;
			double a = setEsp.MinAvailRate;
			double b = setEsp.MaxAvailRate;
			double dx = 1;
			double x = a;
			int size = Convert.ToInt32(Math.Floor((b - a) / dx));
			double[] arr = new double[size];


			for (int i = 0; i < size; i++)
			{
				arr[i] = a;
				a += dx;
			}
			return arr;
		}

		/// <summary>
		/// Формирование массива значений оси Y
		/// </summary>
		private static double[] getYArrayLine(double[] xValue, string Line, SettingForESP setEsp)
		{
			double freq = setEsp.Frequency;
			int numS = setEsp.NumberStages;

			int size = xValue.Length;
			double[] arr = new double[size];

			for (int i = 0; i < size; i++)
			{
				double Q = xValue[i];

				switch (Line)
				{
					case "Напор":
						arr[i] = setEsp.CalcHead(Q, freq, numS);
						break;
					case "КПД":
						arr[i] = setEsp.CalcEfficiency(Q, freq);
						break;
					case "Мощность":
						arr[i] = setEsp.CalcPower(Q, freq, numS);
						break;
					default:
						MessageBox.Show("Ошибка при выводе графика ЭЦН", "Ошибка!");
						return null;
				}
			}

			return arr;
		}

		/// <summary>
		/// Функция создания дополнительных шкал
		/// </summary>
		public static void CreateYAxis(Chart chart, ChartArea area, Series series,
			float axisX, float axisWidth, float labelsSize, bool alignLeft, double maxY = 0)
		{
			chart.ApplyPaletteColors();
			// Создать новую область диаграммы для исходного ряда
			ChartArea areaSeries = chart.ChartAreas.Add("CAs_" + series.Name);
			areaSeries.BackColor = Color.Transparent;
			areaSeries.BorderColor = Color.Transparent;
			areaSeries.Position.FromRectangleF(area.Position.ToRectangleF());
			areaSeries.InnerPlotPosition.FromRectangleF(area.InnerPlotPosition.ToRectangleF());
			areaSeries.AxisX.MajorGrid.Enabled = false;
			areaSeries.AxisX.MajorTickMark.Enabled = false;
			areaSeries.AxisX.LabelStyle.Enabled = false;
			areaSeries.AxisX.LabelStyle.Font = new Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			areaSeries.AxisY.MajorGrid.Enabled = false;
			areaSeries.AxisY.MajorTickMark.Enabled = false;
			areaSeries.AxisY.LabelStyle.Enabled = false;
			areaSeries.AxisY.IsStartedFromZero = area.AxisY.IsStartedFromZero;
			// Aссоциируйте серию с новой областью
			series.ChartArea = areaSeries.Name;

			// Создать новую область диаграммы для axis
			ChartArea areaAxis = chart.ChartAreas.Add("CA_AxY_" + series.ChartArea);

			areaAxis.BackColor = Color.Transparent;
			areaAxis.BorderColor = Color.Transparent;
			RectangleF oRect = area.Position.ToRectangleF();
			areaAxis.Position = new ElementPosition(oRect.X, oRect.Y, axisWidth, oRect.Height);
			areaAxis.InnerPlotPosition.FromRectangleF(areaSeries.InnerPlotPosition.ToRectangleF());

			// Создание копии указанной серии
			Series seriesCopy = chart.Series.Add(series.Name + "_Copy");
			seriesCopy.ChartType = series.ChartType;
			seriesCopy.YAxisType = alignLeft ? AxisType.Primary : AxisType.Secondary;

			foreach (DataPoint point in series.Points)
			{
				seriesCopy.Points.AddXY(point.XValue, point.YValues[0]);
			}
			// Скрыть скопированные серии
			seriesCopy.IsVisibleInLegend = false;
			seriesCopy.Color = Color.Transparent;
			seriesCopy.BorderColor = Color.Transparent;
			seriesCopy.ChartArea = areaAxis.Name;

			// Отключить линии сетки и галочки
			areaAxis.AxisX.LineWidth = 0;
			areaAxis.AxisX.MajorGrid.Enabled = false;
			areaAxis.AxisX.MajorTickMark.Enabled = false;
			areaAxis.AxisX.LabelStyle.Enabled = false;

			Axis areaAxisAxisY = alignLeft ? areaAxis.AxisY : areaAxis.AxisY2;
			areaAxisAxisY.MajorGrid.Enabled = false;
			areaAxisAxisY.IsStartedFromZero = area.AxisY.IsStartedFromZero;
			areaAxisAxisY.LabelStyle.Font = area.AxisY.LabelStyle.Font;

			areaAxisAxisY.Title = series.Name;
			if (series.Name == "Напор")
				areaAxisAxisY.Title += ", м";
			if (series.Name == "Мощность")
			{
				areaAxisAxisY.Title += ", кВт";
				if (maxY != 0)
					areaAxisAxisY.Maximum = maxY;
			}
			if (series.Name == "Расход")
				areaAxisAxisY.Title += ", м³/ сут";
			areaAxisAxisY.LineColor = series.Color;
			areaAxisAxisY.TitleForeColor = Color.DarkCyan;
			areaAxisAxisY.LineWidth = 2;

			// Регулировка положения области
			areaAxis.Position.X = axisX;
			areaAxis.InnerPlotPosition.X += labelsSize;
		}


	}


}
