using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrototypeDryWell.Components.Pipes
{
	public sealed class Pipe
	{
		#region Parameters

        /// <summary>
        /// Диаметр трубы (м)
        /// </summary>
        public double Diameter { get;  }

        /// <summary>
        /// Длина трубы (м)
        /// </summary>
        public double Length { get; }

        /// <summary>
        /// Глубина (разница между верхней и нижней точкой) трубы (м)
        /// </summary>
        public double Depth { get; }

        /// <summary>
        /// Шероховатость трубы (м)
        /// </summary>
        public double Roughness { get; }

        /// <summary>
        /// Относительная шероховатость (безразмерная)
        /// </summary>
        public double RelativeRoughness { get; }

		#endregion

		/// <summary>
		/// Вычисление коэффициента Рейнольдса (безразмерная)
		/// Гриценко стр. 120
		/// </summary>
		/// <param name="flow">Поток, проходящий по трубе</param>
		/// <returns>Значение коэффициента Рейнольдса (безразмерная)</returns>
		private double GetReynoldsNumber(Flow flow)
		{
            double K = 1777;
			double Q = flow.GasDischarge;
            double Rho = flow.RelativeDensity;
            double Nu = flow.DynamicViscosity;
            double D = Diameter * 100;
            return K * Q * Rho / (D * Nu);
		}

		/// <summary>
		/// Вычисление коэффициента гидравлического сопротивления трубы (безразмерная)
		/// Гриценко стр. 118
		/// </summary>
		/// <param name="flow">Поток, проходящий по трубе</param>
		/// <returns>Значение коэффициента гидравлического сопротивления трубы (безразмерная)</returns>
		public double GetHydraulicResistance(Flow flow)
		{
            double Re = GetReynoldsNumber(flow);
            double eps = RelativeRoughness;

			double m = 2.0; // для труб газовой промушленности
			double denominator = m * m * Math.Log10(Math.Pow(6.81 / Re, 1.8 / m) + Math.Pow(7.41 / eps, 2 / m));
			double HydraulicRes = 1.0 / denominator;

			return HydraulicRes;
		}

        public Pipe (double diameter, double length, double depth, double roughness)
        {
			double eps = 0.1; // заданное число для сравнения действительных чисел

			if (diameter > 0.3)	throw new InvalidOperationException();
			if (length < 0 || length < diameter)	throw new InvalidOperationException();
			if (depth < 0 || depth > length+eps)	throw new InvalidOperationException();
			//Гриценко стр. 120
			if (roughness < 0.0015 * 0.001 || roughness > 1 * 0.001)	throw new InvalidOperationException();

			Diameter = diameter;
			Length = length;
            Depth = depth;
            Roughness = roughness;

            RelativeRoughness = 2 * Roughness / Diameter;
        }
    }
}
