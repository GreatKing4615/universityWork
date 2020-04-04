using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMProdWell.Components.Fluids
{
	/// <summary>
	/// Водный флюид
	/// </summary>
	public sealed class WaterFluid
	{

		/// <summary>
		/// Плотность пластовой воды (кг/м3)
		/// </summary>
		public double Density { get; }

		/// <summary>
		/// Вычислить давление создаваемое столбом жидкости высотой H
		/// </summary>
		/// <param name="H">Высота столба жидкости (м)</param>
		/// <returns></returns>
		public double CalcColumnPressure(double H)
		{
			double g = PhysicalConstants.GravitationalAcceleration;
			// Давление в МПа
			double P = Density* g * H * Math.Pow(10, -6);

			return P;
		}


		/// <summary>
		/// Вычислить высоту столба жидкости, которое создаст давление P
		/// </summary>
		/// <param name="P">Давление столба жидкости (МПа)</param>
		/// <returns></returns>
		public double CalcColumnHeight(double P)
		{
			if (P < 0)
				throw new ArgumentOutOfRangeException("Ошибка в функции CalculateColumnHeight(): P < 0, чего быть не должно.");

			double g = PhysicalConstants.GravitationalAcceleration;
			double H = P / (Density * g) * Math.Pow(10, 6);

			return H;
		}
		/// <summary>
		/// Вычислить объем столба жидкости (м3)
		/// </summary>
		/// <param name="H">Высота столба, м</param>
		/// <param name="S">Площадь поперечного сечения трубы, м2</param>
		/// <returns></returns>
		public static double CalcVolumeOfColumnHeight(double H, double S)
		{
			if (H < 0 || S < 0)
				throw new ArgumentOutOfRangeException("Ошибка в функции CalculateVolumeOfColumnHeight(): H < 0 || S < 0, чего быть не должно.");

			double V = H*S;

			return V;
		}

		/// <summary>
		/// Водный флиюд
		/// </summary>
		/// <param name="density">Плотность воды (кг/м3)</param>
		public WaterFluid(double density)
		{
			Density = density;
		}
	}
}
