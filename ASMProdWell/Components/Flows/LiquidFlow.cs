using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMProdWell.Components.Fluids;

namespace ASMProdWell.Components
{
	/// <summary>
	/// Жидкостной поток
	/// </summary>
	public sealed class LiquidFlow
	{
		/// <summary>
		/// Расход жидкости (м3/сут)
		/// </summary>
		public double Rate { get; private set; }

		/// <summary>
		/// Расход пластовой воды (м3/сут)
		/// </summary>
		public double WaterRate { get; private set; }

		/// <summary>
		/// Расход газового конденсата (м3/сут)
		/// </summary>
		public double NaturalGasLiquidsRate { get; private set; }

		/// <summary>
		/// Средняя плотность жидкости (кг/м3)
		/// </summary>
		public double Density { get; private set; }

		/// <summary>
		/// Газовый конденсат
		/// </summary>
		public NaturalGasLiquidsFluid NaturalGasLiquidsFluid { get; }

		/// <summary>
		/// Пластовая вода
		/// </summary>
		public WaterFluid WaterFluid { get; }


		/// <summary>
		/// Жидкостной поток
		/// </summary>
		/// <param name="nglRate">Расход газового конденсата (м3/сут)</param>
		/// <param name="naturalGasLiquidsFluid">Газовый конденсат</param>
		/// <param name="waterRate">Расход пластовой воды (м3/сут)</param>
		/// <param name="waterFluid">Водный конденсат</param>
		public LiquidFlow(double nglRate, NaturalGasLiquidsFluid naturalGasLiquidsFluid, double waterRate, WaterFluid waterFluid)
		{
			WaterFluid = waterFluid;
			NaturalGasLiquidsFluid = naturalGasLiquidsFluid;

			WaterRate = waterRate;
			NaturalGasLiquidsRate = nglRate;

			Rate = NaturalGasLiquidsRate + WaterRate;

			double k1 = NaturalGasLiquidsRate / Rate;
			double k2 = WaterRate / Rate;
			Density = k1 * NaturalGasLiquidsFluid.Density + k2 * WaterFluid.Density;
		}

		/// <summary>
		/// Жидкостной поток
		/// </summary>
		/// <param name="nglDischarge">Расход газового конденсата (м3/сут)</param>
		/// <param name="nglDensity">Плотность газового конденсата (кг/м3)</param>
		public LiquidFlow(double nglDischarge, NaturalGasLiquidsFluid naturalGasLiquidsFluid)
		{
			NaturalGasLiquidsFluid = naturalGasLiquidsFluid;
			NaturalGasLiquidsRate = nglDischarge;

			Rate = NaturalGasLiquidsRate + WaterRate;
			Density = NaturalGasLiquidsFluid.Density;
		}
	}
}
