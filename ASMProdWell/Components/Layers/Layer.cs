using ASMProdWell;
using ASMProdWell.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMProdWell.Components
{
	[Table("Layer")]
	public class Layer
	{
		public int Id { get; set; }

		/// <summary>
		/// Пластовое давление (МПа)
		/// </summary>
		public double ReservoirPressure { get; set; }

		/// <summary>
		/// Температура нейтрального слоя (К)
		/// </summary>
		public double NeutralLayerTemperature { get; set; }

		/// <summary>
		/// Коэффициент фильтрационного сопротивления a (безразмерная)
		/// </summary>
		public double a { get; set; }

		/// <summary>
		/// Коэффициент фильтрационного сопротивления b (безразмерная)
		/// </summary>
		public double b { get; set; }

		/// <summary>
		/// Коэффициент притока пластовой жидкости A (безразмерная)
		/// </summary>
		public double A { get; set; }

		/// <summary>
		/// Коэффициент притока пластовой жидкости B (безразмерная)
		/// </summary>
		public double B { get; set; }

		[NotMapped]
		/// <summary>
		/// Коэффициент восстановления давления Alpha
		/// Гриценко стр 268
		/// </summary>
		public double Alpha;

		[NotMapped]
		/// <summary>
		/// Коэффициент восстановления давления Beta
		/// Гриценко стр 268
		/// </summary>
		public double Beta;

		/// <summary>
		/// Конденсатогазовый фактор (т/тыс.м3)
		/// Naturala Gas Liquids Factor
		/// </summary>
		public double NGLFactor { get; set; }


		/// <summary>
		/// Вычисление максимального притока к забою (тыс.м3/сут)
		/// Гриценко стр. 182
		/// </summary>
		/// <returns>Приток газа (тыс.м3/сут)</returns>
		public double CalcMaxGasRate()
		{
			// Вместо забойного принимаем атмосферное давление для подсчета теоритически максимально возможного притока к забою
			double bP = PhysicalConstants.AtmosphericPressure;
			double rP = ReservoirPressure;
			double a = this.a;
			double b = this.b;
			double Q1, Q2, Q;
			if (SolveQuadraticEquation(b, a, (bP * bP - rP * rP), out Q1, out Q2))
			{
				if (Q2 > Q1)
					Q = Q2;
				else
					Q = Q1;
			}
			else
				throw new InvalidOperationException();

			return Q;
		}


		/// <summary>
		/// Решение квадратного уравнения
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		/// <param name="x1"></param>
		/// <param name="x2"></param>
		private bool SolveQuadraticEquation(double a, double b, double c, out double x1, out double x2)
		{
			double D = b * b - 4 * a * c;
			if (D < 0)
			{
				x1 = x2 = 0;
				return false;
			}
			x1 = (-b + Math.Sqrt(D)) / (2 * a);
			x2 = (-b - Math.Sqrt(D)) / (2 * a);
			return true;
		}
		

        /// <summary>
		/// Вычисление притока воды к забою скважины при заданном забойном давлении bP (МПа).
		/// формулы притока газа         Гриценко стр 182 ;
		/// формула притока воды     rP - bP = A * liquidQ + B 
		/// </summary>
		/// <param name="bottomholePressure">Забойное давление (МПа)</param>
		/// <param name="A">Коэффициент A притока жидкости (безразмерный)</param>
		/// <param name="B">Коэффициент B притока жидкости (безразмерный)</param>
		/// <returns>Приток жидкости (м3/сут)</returns>
		public double CalcWaterRate(double bottomholePressure)
        {

            double rP = ReservoirPressure;

			double waterQ = (rP - bottomholePressure - B) / A;
			// Если пластовое давление выше забойного, то должен происходить
			// обратный процесс - поглощение жидкости пластом
			if (rP <= bottomholePressure)
				waterQ = 0;

            return waterQ;
        }


		/// <summary>
		/// Вычисление притока газа к забою (тыс.м3/сут)
		/// Гриценко стр. 182
		/// </summary>
		/// <returns>Приток газа (тыс.м3/сут)</returns>
		public double CalcGasRate(double bottomholePressure)
		{
			// Вместо забойного принимаем атмосферное давление для подсчета теоритически максимально возможного притока к забою
			double bP = bottomholePressure;
			double rP = ReservoirPressure;
			double a = this.a;
			double b = this.b;
			double Q1, Q2, Q;
			if (SolveQuadraticEquation(b, a, (bP * bP - rP * rP), out Q1, out Q2))
			{
				if (Q2 > Q1)
					Q = Q2;
				else
					Q = Q1;
			}
			else
				throw new InvalidOperationException("Забойное давление больше чем пластовое, чего быть не должно!");

			return Q;
		}


		/// <summary>
		/// Время за которое восстановится забойного давления (МПа) до пластового, Гриценко стр. 262 и 268
		/// </summary>
		/// <param name="bottomholePressure">Забойное давление, с которого идет восстановление (МПа)</param>
		/// <returns></returns>
		private double RecoveryBottomholePressure(double bottomholePressure)
		{
			double Pr = ReservoirPressure;
			double Pb = bottomholePressure;
			if (Pb >= Pr)
				throw new ArgumentOutOfRangeException("Ошибка в функции RecoveryBottomholePressure(): Pb >= Pr, чего быть не должно.");
			double t = (Alpha - Math.Log(Pr * Pr - Pb * Pb)) / Beta;
			t = t / (24 * 60 * 60);//Переводим секунды в сутки
			return t;
		}

		/// <summary>
		/// Время за которое восстановится забойное давления P1  (МПа) до забойного P2, Гриценко стр. 262
		/// P1 < P2
		/// </summary>
		/// <param name="bottomholePressure">Забойное давление, с которого идет восстановление (МПа)</param>
		/// <returns></returns>
		public double RecoveryBottomholePressure(double P1, double P2)
		{
			if (P1 >= P2)
				throw new ArgumentOutOfRangeException("Ошибка: Pзакр >= Pотк, применение плунжера невозможно.");
			double t1 = RecoveryBottomholePressure(P1);
			double t2 = RecoveryBottomholePressure(P2);
			double t = t2 - t1;
			return t;
		}


		/// <summary>
		/// Вычисление забойного давления (МПа) по формуле притока газа к забою скважины 
		/// Гриценко стр. 182
		/// </summary>
		/// <param name = "rate" > Приток газа к забою (тыс.м3/сут)</param>
		/// <returns>Забойное давление (МПа)</returns>
		public double CalcBottomholePressure(double rate)
		{
			double Q = rate;
			double rP = ReservoirPressure;
			double a = this.a;
			double b = this.b;

			double bottomholePressure = Math.Sqrt(rP * rP - Q * (a + b * Q));

			return bottomholePressure;
		}


		/// <summary>
		/// Пласт
		/// </summary>
		/// <param name="reservoirPressure">Пластовое давление (МПа)</param>
		/// <param name="a">Коэффициент фильтрационного сопротивления a (безразмерная)</param>
		/// <param name="b">Коэффициент фильтрационного сопротивления b (безразмерная)</param>
		/// <param name="neutralLayerTemperature">Температура нейтрального слоя (К)</param>
		/// <param name="nglFactor">Кондесатогазовый фактор (т/тыс.м3)</param>
		/// <param name="A">Коэффициент A притока жидкости (безразмерный)</param>
		/// <param name="B">Коэффициент B притока жидкости (безразмерный)</param>Коэффициент восстановления давления Beta
		/// <param name="alpha">Коэффициент восстановления давления Alpha (безразмерный)</param>
		/// <param name="beta">Коэффициент восстановления давления Beta (безразмерный)</param>
		public Layer(double reservoirPressure,
			double a, double b,
			double neutralLayerTemperature,
			double nglFactor = 0,
			double A = Double.PositiveInfinity, 
			double B = Double.PositiveInfinity,
			double alpha = 0, double beta = 0) 
		{
			if (nglFactor < 0 || nglFactor > 1)
				throw new ArgumentOutOfRangeException("Конденсатногазовый фактор должен быть более 0 и менее 1.");

			ReservoirPressure = reservoirPressure;
			this.a = a;
			this.b = b;
			NeutralLayerTemperature = neutralLayerTemperature;
			NGLFactor = nglFactor;
			this.A = A;
            this.B = B;
			Alpha = alpha;
			Beta = beta;
		}
		
        public Layer() { }

    }
}
