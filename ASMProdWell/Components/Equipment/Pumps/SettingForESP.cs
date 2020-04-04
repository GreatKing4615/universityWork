using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMProdWell.Components.Equipment.Pumps
{
	/// <summary>
	/// Настройки для насоса ЭЦН
	/// </summary>
	[NotMapped]
	public sealed class SettingForESP : ElectricSubmersiblePump
	{
		/// <summary>
		/// Требуемое количество ступеней в ЭЦН
		/// </summary>
		public int NumberStages
		{
			get
			{
				return _numberStages;
			}
			set {
				if (value < MinNumberStages)
				{
					_numberStages = MinNumberStages;
				}
				else if (value > MaxNumberStages)
				{
					_numberStages = MaxNumberStages;
				}
				else
					_numberStages = value;

				Head = CalcHead(Rate, Frequency, NumberStages);
				Power = CalcPower(Rate, Frequency, NumberStages);
				Efficiency = CalcEfficiency(Rate, Frequency);
			}
		}
		private int _numberStages;


		/// <summary>
		/// Требуемая частота вращения вала (Гц)
		/// </summary>
		public double Frequency
		{
			get
			{
				return _frequency;
			}
			set
			{
				if (value < MinFrequency)
				{
					_frequency = MinFrequency;
				}
				else if (value > MaxFrequency)
				{
					_frequency = MaxFrequency;
				}
				else
					_frequency = value;

				NomRate = CalcRate(_frequency, NominalRate);
				MinRecRate = CalcRate(_frequency, MinRecomendedRate);
				MaxRecRate = CalcRate(_frequency, MaxRecomendedRate);
				MinAvailRate = CalcRate(_frequency, MinAvailableRate);
				MaxAvailRate = CalcRate(_frequency, MaxAvailableRate);

				Head = CalcHead(Rate, Frequency, NumberStages);
				Power = CalcPower(Rate, Frequency, NumberStages);
				Efficiency = CalcEfficiency(Rate, Frequency);
			}
		}
		private double _frequency;

		/// <summary>
		/// Номинальный расход при новой частоте (м3/сут)
		/// </summary>
		public double NomRate { get; private set; }

		/// <summary>
		/// Максимальный рекомендуемый расход при новой частоте (м3/сут)
		/// </summary>
		public double MaxRecRate { get; private set; }

		/// <summary>
		/// Минимальный рекомендуемый расход при новой частоте (м3/сут)
		/// </summary>
		public double MinRecRate { get; private set; }

		/// <summary>
		/// Максимальный допустимый расход при новой частоте (м3/сут)
		/// </summary>
		public double MaxAvailRate { get; private set; }

		/// <summary>
		/// Минимальный допустимый расход при новой частоте (м3/сут)
		/// </summary>
		public double MinAvailRate { get; private set; }

		/// <summary>
		/// Требуемый расход (тыс.м3/сут)
		/// </summary>
		public double Rate { get; private set; }

		/// <summary>
		/// Напор, при заданом расходе Rate, частоте Frequency и кол-ве ступеней NumStages (м)
		/// </summary>
		public double Head { get; private set; }

		/// <summary>
		/// Мощность, при заданом расходе Rate, частоте Frequency и кол-ве ступеней NumStages (кВт)
		/// </summary>
		public double Power { get; private set; }

		/// <summary>
		/// КПД, при заданом расходе Rate, частоте Frequency и кол-ве ступеней NumStages (%)
		/// </summary>
		public double Efficiency { get; private set; }


		/// <summary>
		/// Настройки для насоса ЭЦН
		/// </summary>
		/// <param name="esp">Ступень ЭЦН</param>
		/// <param name="numStages">Количество ступеней в ЭЦН</param>
		/// <param name="frequency">Частота вращения вала (Гц)</param>
		public SettingForESP(ElectricSubmersiblePump esp, int numStages, double frequency, double rate = 0) : base(esp)
		{
			if (rate == 0)
				Rate = NominalRate;
			else
				Rate = rate;
			Frequency = frequency;
			NumberStages = numStages;
		}

		/// <summary>
		/// Настройки для насоса ЭЦН по умолчанию
		/// </summary>
		/// <param name="esp">Ступень ЭЦН</param>
		public SettingForESP(ElectricSubmersiblePump esp) : base(esp)
		{
			Rate = NominalRate;
			Frequency = BaseFrequency;
			NumberStages = 1;
		}
	}
}
