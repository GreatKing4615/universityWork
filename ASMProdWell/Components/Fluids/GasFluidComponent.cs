using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ASMProdWell.Components.Fluids
{
    [Table("GasFluidComponent")]
	public sealed class GasFluidComponent
	{
        public int Id { get; set; }

        #region Parameters
        [NotMapped]
        public CompoundName Name
        {
            get { return ChemicalCompound.Name; }
        }

        /// <summary>
        /// Критическое давление (МПа)
        /// </summary>
        [NotMapped]
        public double CriticalPressure
        {
            get { return ChemicalCompound.CriticalPressure; }
        }

        /// <summary>
        /// Критическая температура (К)
        /// </summary>
        [NotMapped]
        public double CriticalTemperature
        {
            get { return ChemicalCompound.CriticalTemperature; }
        }

        /// <summary>
        /// Расчет молярной массы компонента (г/моль)
        /// </summary>
        /// <returns></returns>
        [NotMapped]
        public double MolarMass
        {
            get { return ChemicalCompound.MolecularMass; }
        }

        /// <summary>
        /// Мольная доля, величина от 0.0 до 1.0 (безразмерная)
        /// </summary>
        public double X { get; set; }

		#endregion

		/// <summary>
		/// Вычисление плотности компонента (кг/м3)
		/// Гриценко стр. 41
		/// </summary>
		/// <param name="pressure">Текущее давление (МПа)</param>
		/// <param name="temperature">Текущая температура (K)</param>
		/// <returns>Плотность компонента (кг/м3)</returns>
		public double CalcDensity(double pressure, double temperature)
        {
            double Rho = ChemicalCompound.DensityAtStandardConditions;
            double P = pressure;
            double Tst = PhysicalConstants.TemperatureAtStandardConditions;
            double Pat = PhysicalConstants.AtmosphericPressure;
            double Z = CalcSupercompressibilityFactor(pressure, temperature);
            double T = temperature;
            return Rho * P * Tst / (Pat * Z * T);
        }



        /// <summary>
        /// Получить плотность компонента при стандартных условиях (кг/м3)
        /// Гриценко стр. 32
        /// </summary>
        /// <returns></returns>
        public double GetDensityAtStandardConditions()
        {
            return ChemicalCompound.DensityAtStandardConditions;
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
		public double CalcSupercompressibilityFactor(double pressure, double temperature)
        {
			//Формула работает при давление до 50 МПа
			if (pressure > 50) throw new InvalidOperationException();
            double reducedTemperature = temperature / CriticalTemperature;
            double reducedPressure = pressure / CriticalPressure;
			double Z = Math.Pow((0.4 * Math.Log10(reducedTemperature) + 0.73), reducedPressure) + 0.1*reducedPressure;
			return Z; 
        }

        [ForeignKey("Id")]
		public ChemicalCompound ChemicalCompound { get; set; }

		/// <summary>
		/// Компонент газового флюида
		/// </summary>
		/// <param name="chemicalCompound">Химический компонент</param>
		/// <param name="x">Доля, заданная или в процентах или в числом от 0.0 до 1.0</param>
		/// <param name="usePercent">Испльзуем ли мы проценты для определении доли</param>
		public GasFluidComponent(ChemicalCompound chemicalCompound, double x, bool usePercent = true)
        {
            ChemicalCompound = chemicalCompound;
			if (usePercent)
				X = x / 100;
			else
				X = x;
        }

        public GasFluidComponent() { }
	}



}
