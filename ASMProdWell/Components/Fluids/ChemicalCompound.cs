using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ASMProdWell.Components.Fluids
{
    /// <summary>
    /// Химическое соединение
    /// </summary>
    [Table("ChemicalCompound")]
    public sealed class ChemicalCompound
    {
		#region Parameters

        public int Id { get; set; }

        public String NameIdentifier
        {
            get { return Name.ToString(); }
            set
            {
                CompoundName newValue;
                Enum.TryParse(value, out newValue);
                Name = newValue;
            }
        }

        [NotMapped]
        public CompoundName Name { get; set; }

        /// <summary>
        /// Критическое давление (МПа)
        /// </summary>
        public double CriticalPressure { get; set; }

        /// <summary>
        /// Критеческая температура (К)
        /// </summary>
        public double CriticalTemperature { get; set; }

        /// <summary>
        /// Молярная масса (г/моль)
        /// </summary>
        public double MolecularMass { get; set; }

        /// <summary>
        /// Плотность при стандартных условиях (кг/м3)
        /// </summary>
        public double DensityAtStandardConditions { get; set; }

		#endregion

		/// <summary>
		/// Химическое соединение
		/// </summary>
		/// <param name="name">Наименование</param>
		/// <param name="molecularMass">Молекулярная масса</param>
		/// <param name="criticalPressure">Критическое давление (МПа)</param>
		/// <param name="criticalTemperature">Критическая температура (К)</param>
		/// <param name="densityAtStandardConditions">Плотность в стандартных условиях (кг/м3)</param>
		private ChemicalCompound(CompoundName name, double molecularMass, double criticalPressure, double criticalTemperature, double densityAtStandardConditions)
        {
            Name = name;
            CriticalPressure = criticalPressure;
            CriticalTemperature = criticalTemperature;
            MolecularMass = molecularMass;
            DensityAtStandardConditions = densityAtStandardConditions;
        }

        public ChemicalCompound() { }
        /// <summary>
        /// Используемые при моделировании химические соединения
        /// </summary>
        public static class Constants
        {
            public static readonly ChemicalCompound CH4 = new ChemicalCompound(CompoundName.CH4, 16.043, 4.695, 190.55, 0.668);
            public static readonly ChemicalCompound C2H6 = new ChemicalCompound(CompoundName.C2H6, 30.068, 4.976, 305.43, 1.263);
            public static readonly ChemicalCompound C3H8 = new ChemicalCompound(CompoundName.C3H8, 44.094, 4.333, 369.82, 1.872);
            public static readonly ChemicalCompound n_C4H10 = new ChemicalCompound(CompoundName.n_C4H10, 58.120, 3.871, 408.13, 2.486);
            public static readonly ChemicalCompound i_C4H10 = new ChemicalCompound(CompoundName.i_C4H10, 58.120, 3.719, 425.16, 2.518);
            public static readonly ChemicalCompound n_C5H12 = new ChemicalCompound(CompoundName.n_C5H12, 72.151, 3.435, 469.65, 3.221);
            public static readonly ChemicalCompound i_C5H12 = new ChemicalCompound(CompoundName.i_C5H12, 72.151, 3.448, 460.39, 3.221);
            public static readonly ChemicalCompound N2 = new ChemicalCompound(CompoundName.N2, 28.016, 3.465, 126.12, 1.165);
            public static readonly ChemicalCompound H2S = new ChemicalCompound(CompoundName.H2S, 34.082, 9.185, 373.6, 1.434);
            public static readonly ChemicalCompound CO2 = new ChemicalCompound(CompoundName.CO2, 44.011, 7.527, 304.2, 1.842);
        }
    }

        /// <summary>
        /// Имена используемых при моделировании химических соединений
        /// </summary>
        public enum CompoundName { CH4, C2H6, C3H8, i_C4H10, n_C4H10, i_C5H12, n_C5H12, N2, H2S, CO2 }

}
