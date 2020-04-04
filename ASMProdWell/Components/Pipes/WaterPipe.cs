using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMProdWell.Components.Pipes
{
	/// <summary>
	/// Труба, во флюиде потока которой находится жидкость
	/// </summary>
	public class WaterPipe : Pipe
	{
	

		/// <summary>
		/// Вычисление Тета коэффициента (обводненнная скважина)
		/// Гриценко стр. 141
		/// </summary>
		/// <param name="gasFlow">Поток протекающий по трубе</param>
		/// <param name="pipe">Труба</param>
		/// <returns>Значение Тета коэффициента</returns>
		public double CalcTetaCoeff(GasFlow gasFlow, LiquidFlow liquidFlow, double s0Coeff = 0)
		{
			double Z = gasFlow.SupercompressibilityFactor;
			double T = gasFlow.AvgTemperature;
			double D = Diameter;
			double s0 = s0Coeff;
			if (s0Coeff == 0)
				s0 = CalcS0Coeff(gasFlow, liquidFlow);
			double lambda = CalcHydraulicResistance(gasFlow);
			double gasContentParametr = Tubing.CalcGasContentParameter(gasFlow, liquidFlow.Rate, liquidFlow.Density);
			double Teta = 0.01413 * Math.Pow(10, -10) * Z * Z * T * T * (Math.Exp(2 * s0) - 1) * lambda
												 / (gasContentParametr * Math.Pow(D, 5));
			return Teta;
		}


		/// <summary>
		/// Вычисление коэффициента s0 для обводнённой скважины
		/// Гриценко стр. 140
		/// </summary>
		/// <param name="gasFlow">Поток</param>
		/// <returns>Значение коэффициента s0</returns>
		private double CalcS0Coeff(GasFlow gasFlow, LiquidFlow liquidFlow)
		{
			double rho = gasFlow.RelativeDensity;
			double L = Depth;
			double Z = gasFlow.SupercompressibilityFactor;
			double T = gasFlow.AvgTemperature;
			double gasContentParametr = Tubing.CalcGasContentParameter(gasFlow, liquidFlow.Rate, liquidFlow.Density);
			double s0 = 0.03415 * gasContentParametr * rho * L / (Z * T);
			return s0;
		}

		/// <summary>
		/// Вычисление давления в самом низу сегмента (можно сказать "забой" сегмента) НКТ по барометрической формуле. 
		/// Гриценко стр. 117, 113, 140 (для обводненной скважины).
		/// </summary>
		/// <param name="gasFlow">Газовый поток сегмента трубы</param>
        /// <param name="liquidFlow">Жидкостной поток сегмента трубы</param>
        /// <param name="btmTemperature">Температура внизу сегмента (К)</param>
		/// <returns>Давление на забое НКТ (МПа)</returns>
		public double CalcBottomPressure(GasFlow gasFlow, LiquidFlow liquidFlow, double btmTemperature)
		{
			double mixtureQ = Tubing.CalcMixtureRate(gasFlow, liquidFlow);
			double Ptop = gasFlow.TopPressure;

			gasFlow.BottomParametersDefinition(Ptop, btmTemperature);

			double orientirS0 = CalcS0Coeff(gasFlow, liquidFlow);
			double orientirTeta = CalcTetaCoeff(gasFlow, liquidFlow, orientirS0);
			double orientirP = Math.Sqrt(Ptop * Ptop * Math.Exp(2 * orientirS0) + orientirTeta * mixtureQ * mixtureQ);

            gasFlow.BottomParametersDefinition(orientirP, btmTemperature);

            double Pbtm = 0;
			for (int i = 0; i < 3; i++)
			{
				double s0 = CalcS0Coeff(gasFlow, liquidFlow);
				double Teta = CalcTetaCoeff(gasFlow, liquidFlow, s0);
				Pbtm = Math.Sqrt(Ptop * Ptop * Math.Exp(2 * s0) + Teta * mixtureQ * mixtureQ);
                gasFlow.BottomParametersDefinition(Pbtm, btmTemperature);
            }

            return gasFlow.BottomPressure;
		}

		public WaterPipe(double diameter, double length, double depth, double roughness) 
			: base(diameter, length, depth, roughness)
		{}
	}
}
