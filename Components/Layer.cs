using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrototypeDryWell.Components
{
	public sealed class Layer
	{
		/// <summary>
		/// Пластовое давление (МПа)
		/// </summary>
		public readonly double ReservoirPressure;

		/// <summary>
		/// Температура нейтрального слоя
		/// </summary>
		public readonly double NeutralLayerTemperature;

		/// <summary>
		/// Коэффициент фильтрационного сопротивления a
		/// </summary>
		private double aCoefficient;

		/// <summary>
		/// Коэффициент фильтрационного сопротивления b
		/// </summary>
		private double bCoefficient;

		/// <summary>
		/// Вычисление забойного давления (МПа) по формуле притока газа к забою скважины 
		/// Гриценко стр. 182
		/// </summary>
		/// <param name="discharge">Расход или дебит скважины (м3/сут)</param>
		/// <returns>Забойное давление (МПа)</returns>
		public double GetBottomholePressure(double discharge) {
			double Q = discharge;
			double Pr = ReservoirPressure;
			double a = aCoefficient;
			double b = bCoefficient;

			double bottomholePressure = Math.Sqrt(Pr * Pr - Q * (a + b * Q));

			return bottomholePressure;
		}

		/// <summary>
		/// Вычисление максимального притока к забою
		/// Гриценко стр. 182
		/// </summary>
		public double GetMaxDischarge()
		{
			// Вместо забойного принимаем атмосферное давление для подсчета максимально возможного притока к забою
			double Pb = PhysicalConstants.AtmosphericPressure;
			double Pr = ReservoirPressure;
			double a = aCoefficient;
			double b = bCoefficient;
			double Q1, Q2, Q;
			if (SolveQuadraticEquation(b, a, (Pb * Pb - Pr * Pr), out Q1, out Q2))
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

		private Layer() { }

		/// <summary>
		/// Создание газонесущего пласта
		/// </summary>
		/// <param name="reservoirPressure">Пластовое давление (МПа)</param>
		/// <param name="a">Коэффициент фильтрационного сопротивления a</param>
		/// <param name="b">Коэффициент фильтрационного сопротивления b</param>
		public Layer(double reservoirPressure, double a, double b, double neutralLayerTemperature)
		{
			Console.WriteLine("");
			ReservoirPressure = reservoirPressure;
			aCoefficient = a;
			bCoefficient = b;
			NeutralLayerTemperature = neutralLayerTemperature;
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
	}
}
