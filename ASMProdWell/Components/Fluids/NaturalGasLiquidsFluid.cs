using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMProdWell.Components.Fluids
{
    /// <summary>
    /// Флюид газового конденсата
    /// </summary>
    [Table("NaturalGasLiquidsFluid")]
    public sealed class NaturalGasLiquidsFluid
	{
        public int Id { get; set; }

		/// <summary>
		/// Плотность газового конденсата (кг/м3)
		/// </summary>
		public double Density { get; }


		/// <summary>
		/// Дебит газового конденсата (м3/сут)
		/// </summary>
		/// <param name="nglFactor">Конденсатогазовый фактор (т/тыс.м3)</param>
		/// <param name="gasQ">Дебит газа (тыс.м3/сут)</param>
		/// <returns></returns>
		public double CalcVolumeRate(double nglFactor, double gasQ)
		{
			double mass = nglFactor * gasQ; //Измеряется в тоннах в сут (т/сут)
			double volume = mass*1000 / Density;
			return volume;
		}

		/// <summary>
		/// Дебит газового конденсата (т/сут)
		/// </summary>
		/// <param name="nglFactor">Конденсатогазовый фактор (безразмерная)</param>
		/// <param name="gasQ">Дебит газа (м3/сут)</param>
		/// <returns></returns>
		public double CalcMassRate(double nglFactor, double gasQ)
		{
			double mass = nglFactor * gasQ;
			return mass;
		}

		/// <summary>
		/// Флюид газового конденсата
		/// </summary>
		/// <param name="density">Плотность газового конденсата (кг/м3)</param>
		public NaturalGasLiquidsFluid(double density)
		{
			Density = density;
		}
	}
}
