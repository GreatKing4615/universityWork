using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrototypeDryWell.Components.Fluids
{
	public sealed class FluidComponent
	{
        #region Parameters

       
        public CompoundName Name
        {
            get { return ChemicalCompound.Name; }
        }

		/// <summary>
		/// Критическое давление (МПа)
		/// </summary>
        public double CriticalPressure
        {
            get { return ChemicalCompound.CriticalPressure; }
        }

		/// <summary>
		/// Критическая температура (К)
		/// </summary>
        public double CriticalTemperature
        {
            get { return ChemicalCompound.CriticalTemperature; }
        }

        /// <summary>
		/// Расчет молярной массы компонента (г/моль)
		/// </summary>
		/// <returns></returns>
		public double MolarMass
        {
            get { return ChemicalCompound.MolecularMass; }
        }

        /// <summary>
        /// Мольная доля (безразмерная)
        /// </summary>
        public double X { get;  }

		#endregion

		/// <summary>
		/// Вычисление плотности компонента (кг/м3)
		/// Гриценко стр. 41
		/// </summary>
		/// <param name="pressure">Текущее давление (МПа)</param>
		/// <param name="temperature">Текущая температура (K)</param>
		/// <returns>Плотность компонента (кг/м3)</returns>
		public double GetDensity(double pressure, double temperature)
        {
            double Rho = ChemicalCompound.DensityAtStandardConditions;
            double P = pressure;
            double Tst = PhysicalConstants.TemperatureAtStandardConditions;
            double Pat = PhysicalConstants.AtmosphericPressure;
            double Z = GetSupercompressibilityFactor(pressure, temperature);
            double T = temperature;
            return Rho * P * Tst / (Pat * Z * T);
        }

		/// <summary>
		/// Вычисление относительной плотности компонента (безразмерная)
		/// Гриценко стр. 43
		/// </summary>
		/// <returns></returns>
        public double GetRelativeDensity()
        {
            return ChemicalCompound.DensityAtStandardConditions / PhysicalConstants.AirDensityAtStandartConditions;
        }

		/// <summary>
		/// Вычисление коэффициента сверхсжимаемости газа (безразмерная)
		/// Апроксимация Платона-Гуревича
		/// http://info-neft.ru/index.php?action=full_article&id=445
		/// График зависимости Z от приведенных парамметров Гриценко стр. 45
		/// </summary>
		/// <param name="pressure">Давление (МПа)</param>
		/// <param name="temperature">Температура (К)</param>
		/// <returns>Значение коэффициента сверхсжимаемости газа (безразмерная)</returns>
		public double GetSupercompressibilityFactor(double pressure, double temperature)
        {
			//Формула работает при давление до 50 МПа
			if (pressure > 50) throw new InvalidOperationException();
            double reducedTemperature = temperature / CriticalTemperature;
            double reducedPressure = pressure / CriticalPressure;
			double Z = Math.Pow((0.4 * Math.Log10(reducedTemperature) + 0.73), reducedPressure) + 0.1*reducedPressure;
			return Z; 
        }

		private ChemicalCompound ChemicalCompound { get; }

		public FluidComponent(ChemicalCompound chemicalCompound, double x)
        {
            ChemicalCompound = chemicalCompound;
            X = x;
        }
	}



}
