using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrototypeDryWell
{
    static class PhysicalConstants
    {
        /// <summary>
        /// Атмосферное давление (МПа)
        /// </summary>
        public static readonly double AtmosphericPressure = 0.101325;

        /// <summary>
        /// Температура при стандартных условиях (К)
        /// </summary>
        public static readonly double TemperatureAtStandardConditions = 293;

        /// <summary>
        /// Давление при стандартных условиях (МПа)
        /// </summary>
        public static readonly double PressureAtStandardConditions = 0.098;

		/// <summary>
		/// Плотность воздуха в стандартных условиях (кг/м3)
		/// </summary>
		public static readonly double AirDensityAtStandartConditions = 1.205;
    }
}
