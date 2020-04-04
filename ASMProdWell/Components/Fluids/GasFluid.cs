using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;


namespace ASMProdWell.Components.Fluids
{
    [Table("GasFluid")]
    public class GasFluid
    {
        public int Id { get; set; }

        #region Parameters

        /// <summary>
        /// Молярная масса флюида (г/моль)
        /// </summary>
        public double MolarMass { get; }

        /// <summary>
        /// Псевдокритическое давление (МПа)
        /// </summary>
        public virtual double CriticalPressure { get; }

        /// <summary>
        /// Псевдокритическая температура (К)
        /// </summary>
        public virtual double CriticalTemperature { get; }

        /// <summary>
        /// Плотность газового флюида в стандартных условиях (кг/м3)
		/// Гриценко стр. 41
        /// </summary>
        public virtual double DensityAtStandardConditions { get; }


        #endregion

        /// <summary>
        /// Компоненты флюида
        /// </summary>
        public List<GasFluidComponent> GasFluidComponents { get; set; }

        /// <summary>
        /// Вычисление плотности флюида (кг/м3)
        /// Гриценко стр. 41
        /// </summary>
        /// <param name="pressure">Текущее давление (МПа)</param>
        /// <param name="temperature">Текущая температура (K)</param>
        /// <returns>Плотность флюида (кг/м3)</returns>
        public double GetDensity(double pressure, double temperature)
		{
			double Density = 0;
			foreach (GasFluidComponent fc in GasFluidComponents)
			{
                Density += fc.CalcDensity(pressure, temperature)*fc.X;
            }
			return Density;
		}


		/// <summary>
		/// Вычисление коэффициента сверхсжимаемости газа (безразмерная)
		/// Выражение В.В. Латонова - Г.Р. Гуревича, которое является аппроксимацией графиков Брауна
		/// https://poznayka.org/s77617t1.html
		/// </summary>
		/// <param name="pressure">Давление (МПа)</param>
		/// <param name="temperature">Температура (К)</param>
		/// <returns>Значение коэффициента сверхсжимаемости газа (безразмерная)</returns>
		public double CalcSupercompressibilityFactor(double pressure, double temperature)
        {
            double reducedTemperature = temperature / CriticalTemperature;
            double reducedPressure = pressure / CriticalPressure;
            return Math.Pow((0.4 * Math.Log10(reducedTemperature) + 0.73), reducedPressure) + 0.1*reducedPressure;
        }
        
        /// <summary>
        /// Вычисление динамической вязкости флюида (cП | мПа*с)
        /// Ли, Ваттенбаргер стр. 41
        /// </summary>
        /// <param name="pressure">Текущее давление (МПа)</param>
        /// <param name="temperature">Текущая температура (К)</param>
        /// <returns>Динамическая вязкость флюида (cП | мПа*с)</returns>
        public double CalcDynamicViscosity(double pressure, double temperature)
        {
			//Перевод из МПа в бар
			double p = pressure * 10;
            double M = MolarMass;
            double z = CalcSupercompressibilityFactor(pressure,temperature);
            double T = temperature;

            double rho = 1.202 * 0.001 * (p * M / (z * T));
            double K = (9.379 + 0.01607 * M) * Math.Pow(9.0 / 5.0 * T, 1.5) /
                       (209.2 + 19.26 * M + 9.0 / 5.0 * T);
            double X = 3.448 + 548.0 / T + 0.01009 * M;
            double Y = 2.447 - 0.2224 * X;
            double nu = 0.0001 * K * Math.Exp(X * Math.Pow(rho, Y));
			return nu;
        }

		/// <summary>
		/// Вычисление относительной плотности флюида (кг/м3)
		/// Гриценко стр. 43
		/// </summary>
		/// <param name="pressure">Текущее давление (МПа)</param>
		/// <param name="temperature">Текущая температура (K)</param>
		/// <returns>Относительная плотность флюида (кг/м3)</returns>
		public double GetRelativeDensity()
		{
            double relativeDensity = DensityAtStandardConditions / PhysicalConstants.AirDensityAtStandartConditions;
            return relativeDensity;
		}

		/// <summary>
		/// Газовый флюид
		/// </summary>
		/// <param name="FluidComponents">Компоненты газовой смеси</param>
		public GasFluid(List<GasFluidComponent> FluidComponents)
        {
            double totalX = 0;
            foreach (GasFluidComponent component in FluidComponents)
            {
                MolarMass += component.MolarMass * component.X;
                CriticalPressure += component.CriticalPressure * component.X;
                CriticalTemperature += component.CriticalTemperature * component.X;
                DensityAtStandardConditions += component.GetDensityAtStandardConditions() * component.X;
                totalX += component.X;
            }
            this.GasFluidComponents = FluidComponents;
            double eps = 0.01;
            if (Math.Abs(totalX - 1.0) > eps) throw new ArgumentException("Ошибка: Неправильно задан флюид");
        }

        private GasFluid() { }

		/// <summary>
		/// Получение флюида, заданного пользователем
		/// </summary>
		/// <returns></returns>
        public static GasFluid GetFluid()
        {
            List<GasFluidComponent> components = new List<GasFluidComponent>();
            components.Add(new GasFluidComponent(ChemicalCompound.Constants.CH4, MainWindow.FluidComponents[0].MoleFractionInPercents));
            components.Add(new GasFluidComponent(ChemicalCompound.Constants.C2H6, MainWindow.FluidComponents[1].MoleFractionInPercents));
            components.Add(new GasFluidComponent(ChemicalCompound.Constants.C3H8, MainWindow.FluidComponents[2].MoleFractionInPercents));
            components.Add(new GasFluidComponent(ChemicalCompound.Constants.n_C4H10, MainWindow.FluidComponents[3].MoleFractionInPercents));
            components.Add(new GasFluidComponent(ChemicalCompound.Constants.i_C4H10, MainWindow.FluidComponents[4].MoleFractionInPercents));
            components.Add(new GasFluidComponent(ChemicalCompound.Constants.n_C5H12, MainWindow.FluidComponents[5].MoleFractionInPercents));
            components.Add(new GasFluidComponent(ChemicalCompound.Constants.i_C5H12, MainWindow.FluidComponents[6].MoleFractionInPercents));
            components.Add(new GasFluidComponent(ChemicalCompound.Constants.N2, MainWindow.FluidComponents[7].MoleFractionInPercents));
            components.Add(new GasFluidComponent(ChemicalCompound.Constants.H2S, MainWindow.FluidComponents[8].MoleFractionInPercents));
            components.Add(new GasFluidComponent(ChemicalCompound.Constants.CO2, MainWindow.FluidComponents[9].MoleFractionInPercents));
            return new GasFluid(components);
        }
	}
}
