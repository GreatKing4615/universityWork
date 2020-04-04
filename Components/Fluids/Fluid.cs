using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrototypeDryWell.Components.Fluids
{
    public class Fluid
    {
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


		#endregion

		/// <summary>
		/// Компоненты флюида
		/// </summary>
		private List<FluidComponent> FluidComponents;

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
			foreach (FluidComponent fc in FluidComponents)
			{
                Density += fc.GetDensity(pressure, temperature)*fc.X;
            }
			return Density;
		}

		/// <summary>
		/// Вычисление коэффициента сверхсжимаемости газа (безразмерная)
		/// </summary>
		/// <param name="pressure">Давление (МПа)</param>
		/// <param name="temperature">Температура (К)</param>
		/// <returns>Значение коэффициента сверхсжимаемости газа (безразмерная)</returns>
		public double GetSupercompressibilityFactor(double pressure, double temperature)
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
        public double GetDynamicViscosity(double pressure, double temperature)
        {
			//Перевод из МПа в бар
			double p = pressure * 10;
            double M = MolarMass;
            double z = GetSupercompressibilityFactor(pressure,temperature);
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
            double relativeDensity = 0;
            foreach (FluidComponent fc in FluidComponents)
            {
                relativeDensity += fc.GetRelativeDensity() * fc.X;
            }
            return relativeDensity;
		}

		public Fluid(List<FluidComponent> FluidComponents)
        {
            double totalX = 0;
            foreach (FluidComponent component in FluidComponents)
            {
                MolarMass += component.MolarMass * component.X;
                CriticalPressure += component.CriticalPressure * component.X;
                CriticalTemperature += component.CriticalTemperature * component.X;
                totalX += component.X;
            }
            this.FluidComponents = FluidComponents;
            double eps = 0.01;
            if (Math.Abs(totalX - 1.0) > eps) throw new ArgumentException();
        }

		/// <summary>
		/// Тестовый флюид
		/// </summary>
		/// <returns></returns>
        public static Fluid GetTestFluid()
        {
            List<FluidComponent> components = new List<FluidComponent>();
            components.Add(new FluidComponent(ChemicalCompound.Constants.CH4, 0.85284));
            components.Add(new FluidComponent(ChemicalCompound.Constants.C2H6, 0.04207));
            components.Add(new FluidComponent(ChemicalCompound.Constants.C3H8, 0.01703));
            components.Add(new FluidComponent(ChemicalCompound.Constants.i_C4H10, 0.00258));
            components.Add(new FluidComponent(ChemicalCompound.Constants.n_C4H10, 0.00518));
            components.Add(new FluidComponent(ChemicalCompound.Constants.n_C5H12, 0.00618));
            components.Add(new FluidComponent(ChemicalCompound.Constants.N2, 0.05379));
            components.Add(new FluidComponent(ChemicalCompound.Constants.H2S, 0.01467));
            components.Add(new FluidComponent(ChemicalCompound.Constants.CO2, 0.00617));
            return new Fluid(components);
        }
	}
}
