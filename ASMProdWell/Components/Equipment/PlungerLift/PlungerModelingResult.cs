using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMProdWell.Components.Equipment.PlungerLift
{
	/// <summary>
	/// Результат моделирования плунжерного лифта
	/// </summary>
	public class PlungerModelingResult
	{
		/// <summary>
		/// Критический дебит газа (тыс.куб.м/сут)
		/// </summary>
		public double CritQ;

		/// <summary>
		/// Дебит газа в начале интервала фонтанной добычи (тыс.куб.м/сут)
		/// </summary>
		public double StartGasRate;

		/// <summary>
		/// Дебит газа в конце интервала фонтанной добычи (тыс.куб.м/сут)
		/// </summary>
		public double CloseGasRate;

		/// <summary>
		/// Средний дебит газа за сутки (м3/сут)
		/// </summary>
		public double AverageGasRate;

		/// <summary>
		/// Высота столба жидкости (м)
		/// </summary>
		public double ColumnHeight;

		/// <summary>
		/// Объём столба жидкости (м3)
		/// </summary>
		public double ColumnWaterVolume;

		/// <summary>
		/// Объем газа добываемый за цикл (тыс.куб.м)
		/// </summary>
		public double GasVolumeForCycle;

		/// <summary>
		/// Забойное давление в момент открытия приводного клапана (МПа)
		/// </summary>
		public double OpenBtmhPressure;

		/// <summary>
		/// Забойное давление в начале интервала фонтанной добычи (МПа)
		/// </summary>
		public double StartBtmhPressure;

		/// <summary>
		/// Забойное давление в конце интервала фонтанной добычи (МПа)
		/// </summary>
		public double CloseBtmhPressure;

		/// <summary>
		/// Среднее забойное давление на протяжение цикла (МПа)
		/// </summary>
		public double AverageBtmhPressure;

		/// <summary>
		/// Приток воды к забою в начале интервала фонтанной добычи (м3/сут)
		/// </summary>
		public double StartWaterRate;

		/// <summary>
		/// Приток воды к забою в конце интервала фонтанной добычи (м3/сут)
		/// </summary>
		public double CloseWaterRate;

		/// <summary>
		/// Средний приток воды к забою за сутки (м3/сут)
		/// </summary>
		public double AverageWaterRate;

		/// <summary>
		/// Приток конденсата к забою в начале интервала фонтанной добычи (т/сут)
		/// </summary>
		public double StartNglRate;

		/// <summary>
		/// Приток конденсата к забою в конце интервала фонтанной добычи (т/сут)
		/// </summary>
		public double CloseNglRate;

		/// <summary>
		/// Средний приток конденсата к забою за сутки (т/сут)
		/// </summary>
		public double AverageNglRate;

		/// <summary>
		/// Время восстановления забойного давления (сут)
		/// </summary>
		public double TimeRecovery;

		/// <summary>
		/// Время работы скважины в фонтанном режиме (сут)
		/// </summary>
		public double TimeWork;

		/// <summary>
		/// Время подъема плунжера с забоя к устью (мин)
		/// </summary>
		public double TimeUp;

		/// <summary>
		/// Время полного цикла плунжерного лифта (сут)
		/// </summary>
		public double TimeCycle;

		/// <summary>
		/// Количество циклов плунжер лифта в день
		/// </summary>
		public double NumCyclesInDay;
	}
}
