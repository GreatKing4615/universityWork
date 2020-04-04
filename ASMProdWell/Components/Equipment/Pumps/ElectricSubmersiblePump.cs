using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMProdWell.Components.Equipment.Pumps
{
	/// <summary>
	/// Электроцентробежный насос (ЭЦН)
	/// </summary>
	[Table("ElectricSubmersiblePump")]
	public class ElectricSubmersiblePump : Pump
	{
		/// <summary>
		/// Максимальная частота (Гц)
		/// </summary>
		public static readonly int MaxFrequency = 100;

		/// <summary>
		/// Минимальная частота (Гц)
		/// </summary>
		public static readonly int MinFrequency = 35;

		/// <summary>
		/// Максимальное количество ступеней
		/// </summary>
		public static readonly int MaxNumberStages = 500;

		/// <summary>
		/// Минимальное количество ступеней
		/// </summary>
		public static readonly int MinNumberStages = 1;


		/// <summary>
		/// Условные габариты и их диаметры (мм)
		/// </summary>
		public static SortedDictionary<string, double> DimensionDiameter = new SortedDictionary<string, double>()
		{
			{"2A", 69}, {"3", 81}, {"4", 86}, {"5", 92}, {"5A", 103},{"6", 114},{"7A", 136},{"8", 172}, {"9", 185}
		};

		/// <summary>
		/// Коэффициенты полинома характеризующего КПД от расхода
		/// </summary>
		[ForeignKey("PumpId")]
		public List<EfficiencyCoefficient> EfficiencyCoefficients { get; set; }

		/// <summary>
		/// Коэффициенты полинома характеризующего напор от расхода 
		/// </summary>
		[ForeignKey("PumpId")]
		public List<HeadCoefficient> HeadCoefficients { get; set; }

		/// <summary>
		/// Условный габарит
		/// </summary>
		public string ConditionalDimension
		{
			get
			{
				return _conditionalDimension;
			}
			set
			{
				try
				{
					Diameter = DimensionDiameter[value];
					_conditionalDimension = value;
				}
				catch (KeyNotFoundException)
				{
					throw new ArgumentException("Неправильно задан условный габарит. Условного габарита " + value +
					" не существует.");
				}
			}
		}
		private string _conditionalDimension;

		/// <summary>
		/// Мин. рекомендуемая подача (куб.м/сут)
		/// </summary>
		public double MinRecomendedRate { get; set; }

		/// <summary>
		/// Макс. рекомендуемая подача (куб.м/сут)
		/// </summary>
		public double MaxRecomendedRate { get; set; }

		/// <summary>
		/// Мин. допустимая подача (куб.м/сут)
		/// </summary>
		public double MinAvailableRate { get; set; }

		/// <summary>
		/// Макс. допустимая подача (куб.м/сут)
		/// </summary>
		public double MaxAvailableRate { get; set; }

		/// <summary>
		/// Базовая частота насоса (Гц)
		/// </summary>
		public double BaseFrequency {
			get
			{
				return _baseFrequency;
			}
			set
			{
				if (value < 35 || value > 100)
					throw new ArgumentOutOfRangeException("Неправильно задана базовая частота! Частота должно быть не меньше 35 Гц и не болеше 100 Гц.");
				_baseFrequency = value;
			}
		}
		private double _baseFrequency;


		/// <summary>
		/// Вычислить значение КПД при заданном расходе жидкости (%)
		/// </summary>
		/// <param name="preRate">Преведенный расход жидкости при частоте frequency (м3/сут)</param>
		/// <param name="frequency">Частота вращения вала (Гц)</param>
		/// <returns></returns>
		public double CalcEfficiency(double preRate, double frequency = 0)
		{
			//nomRate - расход жидкости при базовой частоте
			double nomRate = preRate;
			double sum = 0;
			if (frequency != 0)
				nomRate = preRate * BaseFrequency / frequency;
			for (int i = 0; i < EfficiencyCoefficients.Count; i++)
			{
				sum += Math.Pow(nomRate, EfficiencyCoefficients[i].Order) * EfficiencyCoefficients[i].Value;
			}
			if (sum < 0) sum = 0;

			return sum;
		}

		/// <summary>
		/// Вычислить значение напора при заданном расходе жидкости (м)
		/// </summary>
		/// <param name="preRate">Преведенный расход жидкости при частоте frequency (м3/сут)</param>
		/// <param name="frequency">Частота вращения вала (Гц)</param>
		/// <param name="numStages">Количество ступеней</param>
		/// <returns></returns>
		public double CalcHead(double preRate, double frequency = 0, int numStages = 1)
		{
			//nomRate - расход жидкости при базовой частоте
			double nomRate = preRate;
			double sum = 0;
			if (frequency != 0)
				nomRate = preRate * BaseFrequency / frequency;
			for (int i = 0; i < HeadCoefficients.Count; i++)
			{
				sum += Math.Pow(nomRate, HeadCoefficients[i].Order) * HeadCoefficients[i].Value;
			}
			if (sum < 0) sum = 0;

			if (frequency != 0)
				sum = sum * Math.Pow(frequency / BaseFrequency, 2);
			if (numStages != 1)
				sum = sum * numStages;

			return sum;
		}

		/// <summary>
		/// Вычислить значение мощности при заданном расходе жидкости (кВт)
		/// </summary>
		/// <param name="preRate">Преведенный расход жидкости при частоте frequency (м3/сут)</param>
		/// <param name="frequency">Частота вращения вала (Гц)</param>
		/// <returns></returns>
		public double CalcPower(double preRate, double frequency = 0, int numStages = 1)
		{
			//nomRate - расход жидкости при базовой частоте
			double nomRate = preRate;
			double sum = 0;
			if (frequency != 0)
				nomRate = preRate * BaseFrequency / frequency;
			for (int i = 0; i < PowerCoefficients.Count; i++)
			{
				sum += Math.Pow(nomRate, PowerCoefficients[i].Order) * PowerCoefficients[i].Value;
			}
			if (sum < 0) sum = 0;

			if (frequency != 0 )
				sum = sum * Math.Pow(frequency / BaseFrequency, 3);
			if (numStages != 1 )
				sum = sum * numStages;

			//Переводим Вт в кВт
			sum /= 1000;

			return sum;
		}

		/// <summary>
		/// Вычислить значение преведенного расхода при заданной частоте (м3/сут)
		/// </summary>
		/// <param name="frequency">Частота вращения вала (Гц)</param>
		/// <param name="nomRate">Номинальный расход, относительно которого изменился искомый (м3/сут)</param>
		/// <returns></returns>
		public double CalcRate(double frequency, double nomRate)
		{
			double preRate = nomRate * frequency / BaseFrequency;

			return preRate;
		}

		/// <summary>
		/// Вычислить значение номинального расхода при заданной частоте (м3/сут)
		/// </summary>
		/// <param name="frequency">Частота вращения вала (Гц)</param>
		/// <param name="preRate">Преведенный расход, относительно которого изменился искомый (м3/сут)</param>
		/// <returns></returns>
		public double CalcNomRate(double frequency, double preRate)
		{
			double nomRate = preRate * frequency / BaseFrequency;

			return nomRate;
		}

		/// <summary>
		/// Находится ли требуемый расход в рекомендуемом интервале
		/// </summary>
		/// <param name="Qneeded">Требуемый расход (м3/сут)</param>
		/// <param name="frequency">Частота вращения вала (Гц)</param>
		/// <param name="сentrZoneProc">Процент от рекомендуемого интервала, который занимает его центральная зона (%)</param>
		/// <returns></returns>
		public bool IsInRecomendedInterval(double Qneeded, double frequency = 0, double сentrZoneProc = 0)
		{
			if (сentrZoneProc == 0)
				сentrZoneProc = 100;
			if (frequency == 0)
				frequency = BaseFrequency;

			сentrZoneProc = сentrZoneProc / 100.0;
			double minR = CalcRate(frequency, MinRecomendedRate);
			double maxR = CalcRate(frequency, MaxRecomendedRate);
			double size = сentrZoneProc * (maxR - minR);

			bool flag = Qneeded < minR+size && Qneeded > maxR - size;
			return flag;
		}

		/// <summary>
		/// Подбор частоты для заданного дебита (Гц)
		/// </summary>
		/// <param name="Qneeded">Требуемый расход (м3/сут)</param>
		/// <param name="frequency">Частота вращения вала (Гц)</param>
		/// <returns></returns>
		public bool CalcFrequency(double Qneeded, out double frequency)
		{
			double Vw;
			double HrotQ = 0, chosenFreq = 0;
			for (int i = MinFrequency; i <= MaxFrequency; i++)
			{
				Vw = i;
				bool inRecInterval = IsInRecomendedInterval(Qneeded, Vw, 100);
				if (inRecInterval)
				{
					double Htmp = CalcHead(Qneeded, Vw);
					double Qtmp = CalcNomRate(Vw, Qneeded);
					double tmp = Htmp / Qtmp;
					if (HrotQ < tmp)
					{
						HrotQ = tmp;
						chosenFreq = Vw;
					}
				}
			}
			frequency = chosenFreq;
			if(chosenFreq == 0)
				return false;
			else
				return true;
		}

		/// <summary>
		/// Подбора ступеней ЭЦН
		/// </summary>
		/// <param name="diameterTubing">Внутренний диаметр обсаднйо колонны (м)</param>
		/// <param name="Q">Требуемый расход воды (м3 / сут)</param>
		/// <param name="H">Требуемый напор (м)</param>
		/// <param name="numStages">Количество ступеней для каждого насоса</param>
		/// <param name="catalog">Коталог ступеней ЭЦН</param>
		/// <returns></returns>
		public static List<SettingForESP> ChoseESP(double diameterTubing, double Q, double H, List<ElectricSubmersiblePump> catalog)
		{
			List<SettingForESP> ESPs = new List<SettingForESP>();
			foreach (ElectricSubmersiblePump esp in catalog)
			{
				double freq = esp.BaseFrequency;
				if (esp.Diameter/1000 >= diameterTubing)
					continue;

				if(!esp.CalcFrequency(Q, out freq))
					continue;

				double Hesp = esp.CalcHead(Q, freq);
				int numSt = Convert.ToInt32(Math.Ceiling(H / Hesp));
				if (numSt > 500 || numSt < 1)
					continue;
				
				ESPs.Add(new SettingForESP(esp, numSt, freq, Q));
			}
			return ESPs;
		}

		/// <summary>
		/// Электроцентробежный насос
		/// </summary>
		/// <param name="efficiency">КПД (%)</param>
		/// <param name="head">Напор (м) </param>
		/// <param name="power">Мощность (кВт)</param>
		public ElectricSubmersiblePump(List<EfficiencyCoefficient> efficiency, List<HeadCoefficient> head, List<PowerCoefficient> power)
		{
			EfficiencyCoefficients = efficiency;
			HeadCoefficients = head;
			PowerCoefficients = power;
		}

		/// <summary>
		/// Электроцентробежный насос
		/// </summary>
		/// <param name="esp">Электроцентробежный насос</param>
		public ElectricSubmersiblePump(ElectricSubmersiblePump esp)
		{
			Id = esp.Id;
			Name = esp.Name;
			Diameter = esp.Diameter;
			NominalRate = esp.NominalRate;

			ConditionalDimension = esp.ConditionalDimension;
			BaseFrequency = esp.BaseFrequency;

			MinRecomendedRate = esp.MinRecomendedRate;
			MaxRecomendedRate = esp.MaxRecomendedRate;
			MinAvailableRate = esp.MinAvailableRate;
			MaxAvailableRate = esp.MaxAvailableRate;

			EfficiencyCoefficients = esp.EfficiencyCoefficients;
			HeadCoefficients = esp.HeadCoefficients;
			PowerCoefficients = esp.PowerCoefficients;
		}

		public ElectricSubmersiblePump() { }
    }

}
