using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMProdWell.Components.Equipment.Pumps
{
    [Table("ProgressiveCavityPump")]
	public class ProgressiveCavityPump : Pump
	{
		/// <summary>
		/// Максимальная скорость (об/мин)
		/// </summary>
		public static readonly int MaxSpeed = 500;

		/// <summary>
		/// Минимальная скорость (об/мин)
		/// </summary>
		public static readonly int MinSpeed = 50;

		/// <summary>
		/// Коэффициенты полинома характеризующего расход насоса от напора 
		/// </summary>
		[ForeignKey("PumpId")]
        public List<RateCoefficient> RateCoefficients { get; set; }

        /// <summary>
        /// Коэффициенты полинома характеризующего крутящий момент от напора 
        /// </summary>
        [ForeignKey("PumpId")]
        public List<TorqueCoefficient> TorqueCoefficients { get; set; }

		/// <summary>
		/// Номинальная скорость (об/мин)
		/// </summary>
		public double BaseSpeed { get; set; }


		/// <summary>
		/// Вычислить значение крутящего момента при заданном напоре (килограммсилы-метр)
		/// </summary>
		/// <param name="H">Напор (м)</param>
		/// <returns></returns>
		public double CalcTorque(double H, double speed = 0)
		{
			if (speed == 0) speed = BaseSpeed;
			double sum = 0;
			for (int i = 0; i < TorqueCoefficients.Count; i++)
			{
				sum += Math.Pow(H, TorqueCoefficients[i].Order) * TorqueCoefficients[i].Value;
			}
			sum = sum * speed / BaseSpeed;
			if (sum < 0) sum = 0;

			return sum;
		}

		/// <summary>
		/// Вычислить значение расхода при заданном напоре (куб.м/сут)
		/// </summary>
		/// <param name="H">Напор (м)</param>
		/// <returns></returns>
		public double CalcRate(double H, double speed = 0)
		{
			if (speed == 0) speed = BaseSpeed;
			double sum = 0;
			for (int i = 0; i < RateCoefficients.Count; i++)
			{
				sum += Math.Pow(H, RateCoefficients[i].Order) * RateCoefficients[i].Value;
			}
			sum = sum * speed / BaseSpeed;
			if (sum < 0) sum = 0;

			return sum;
		}

		/// <summary>
		/// Вычислить значение мощности при заданном напоре (кВт)
		/// </summary>
		/// <param name="H">Напор (м)</param>
		/// <returns></returns>
		public double CalcPower(double H, double speed = 0)
		{
			if (speed == 0) speed = BaseSpeed;
			double sum = 0;
			for (int i = 0; i < PowerCoefficients.Count; i++)
			{
				sum += Math.Pow(H, PowerCoefficients[i].Order) * PowerCoefficients[i].Value;
			}
			sum = sum * speed / BaseSpeed;
			if (sum < 0) sum = 0;

			return sum;
		}

		/// <summary>
		/// Подбора насоса ВШН
		/// </summary>
		/// <param name="diameterTubing">Внутренний диаметр обсаднйо колонны (м)</param>
		/// <param name="Q">Требуемый расход воды (м3 / сут)</param>
		/// <param name="H">Требуемый напор (м)</param>
		/// <param name="catalog">Каталог насосов ВШН</param>
		/// <returns></returns>
		public static List<SettingForPCP> ChosePCP(double diameterTubing, double Q, double H, List<ProgressiveCavityPump> catalog)
		{
			List<SettingForPCP> PCPs = new List<SettingForPCP>();
			foreach (ProgressiveCavityPump pcp in catalog)
			{
				if (pcp.Diameter / 1000 >= diameterTubing)
					continue;

				for (double sp = 50; sp <= 400; sp += 1)
				{
					double Qpcp = pcp.CalcRate(H, sp);
					if (Qpcp < Q) continue;
					PCPs.Add(new SettingForPCP(pcp, sp, H));
					break;
				}
			}
			return PCPs;
		}

		/// <summary>
		/// Электроцентробежный насос
		/// </summary>
		/// <param name="discharge">Расход (куб.м/сут)</param>
		/// <param name="torque">Крутящий момент (килограммсилы-метр) </param>
		/// <param name="power">Мощность (кВт)</param>
		public ProgressiveCavityPump(List<RateCoefficient> discharge, List<TorqueCoefficient> torque, List<PowerCoefficient> power)
		{
			RateCoefficients = discharge;
			TorqueCoefficients = torque;
			PowerCoefficients = power;
		}

		/// <summary>
		/// Винтовой штанговый насос
		/// </summary>
		public ProgressiveCavityPump(ProgressiveCavityPump pcp)
		{
			Id = pcp.Id;
			Name = pcp.Name;
			Diameter = pcp.Diameter;
			NominalRate = pcp.NominalRate;

			BaseSpeed = pcp.BaseSpeed;

			RateCoefficients = pcp.RateCoefficients;
			TorqueCoefficients = pcp.TorqueCoefficients;
			PowerCoefficients = pcp.PowerCoefficients;
		}

		public ProgressiveCavityPump() { }
	}
}
