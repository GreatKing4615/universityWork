using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMProdWell.Components.Pipes
{
	public class Pipe
	{
        public int Id;

		#region Parameters

        /// <summary>
        /// Диаметр трубы (м)
        /// </summary>
        public double Diameter { get;  }

        /// <summary>
        /// Длина трубы (м)
        /// </summary>
        public double Length { get; }

        /// <summary>
        /// Глубина (разница между верхней и нижней точкой) трубы (м)
        /// </summary>
        public double Depth { get; }

        /// <summary>
        /// Шероховатость трубы (м)
        /// </summary>
        public double Roughness { get; }

        /// <summary>
        /// Относительная шероховатость (безразмерная)
        /// </summary>
        public double RelativeRoughness { get; }

		#endregion
	


		/// <summary>
		/// Вычисление коэффициента Рейнольдса (безразмерная)
		/// Гриценко стр. 120
		/// </summary>
		/// <param name="flow">Поток, проходящий по трубе</param>
		/// <returns>Значение коэффициента Рейнольдса (безразмерная)</returns>
		private double CalcReynoldsNumber(GasFlow flow)
		{
            double K = 1777;
			double Q = flow.RateAtStandardConditions;
            double Rho = flow.RelativeDensity;
			//Динамическая вязкость (мПа*с)
            double Nu = flow.DynamicViscosity;
			//Внутренний диаметр (м*10^-2)
            double D = Diameter * 100;
            double Re = K * Q * Rho / (D * Nu);
			return Re;
		}

		/// <summary>
		/// Вычисление коэффициента гидравлического сопротивления трубы (безразмерная)
		/// Гриценко стр. 118
		/// </summary>
		/// <param name="flow">Поток, проходящий по трубе</param>
		/// <returns>Значение коэффициента гидравлического сопротивления трубы (безразмерная)</returns>
		public double CalcHydraulicResistance(GasFlow flow)
		{
            double Re = CalcReynoldsNumber(flow);
            double eps = RelativeRoughness;

			double m = 2.0; // для труб газовой промышленности
			double denominator = m * m * Math.Log10(Math.Pow(6.81 / Re, 1.8 / m) + Math.Pow(7.41 / eps, 2 / m));
			double HydraulicRes = 1.0 / denominator;

			return HydraulicRes;
		}


		/// <summary>
		/// Вычисление коэффициента s для сухой скважины
		/// Гриценко стр. 117
		/// </summary>
		/// <param name="flow">Поток</param>
		/// <returns>Значение коэффициента s</returns>
		private double CalcSCoeff(GasFlow flow)
		{
			double rho = flow.RelativeDensity;
			double L = Depth;
			double Z = flow.SupercompressibilityFactor;
			double T = flow.AvgTemperature;
			double s = (0.0683 * rho * L / (Z * T)) / 2;
			return s;
		}


		/// <summary>
		/// Вычисление Тета коэффициента (сухая скважина)
		/// Гриценко стр. 117
		/// </summary>
		/// <param name="flow">Поток протекающий по трубе</param>
		/// <param name="pipe">Труба</param>
		/// <returns>Значение Тета коэффициента</returns>
		public double CalcTetaCoeff(GasFlow flow, double sCoeff = 0)
		{
			double Z = flow.SupercompressibilityFactor;
			double T = flow.AvgTemperature;
			double D = Diameter;
			double s = sCoeff;
			if (sCoeff == 0)
				s = CalcSCoeff(flow);
			double lambda = CalcHydraulicResistance(flow);
			double Teta = 0.01413 * Math.Pow(10, -10) * Z * Z * T * T * (Math.Exp(2 * s) - 1) * lambda
												 / Math.Pow(D, 5);
			return Teta;
		}

		/// <summary>
		/// Вычисление давления в самом низу сегмента (можно сказать "забой" сегмента) НКТ 
		/// остановленной скважины по барометрической формуле. 
		/// Гриценко стр. 117 и 113 (для сухой скважины). 
		/// </summary>
		/// <param name="gasFlow">Поток сегмента НКТ</param>
		/// <param name="bottomTemperature">Температура внизу сегмента (К)</param>
		/// <returns>Давление на забое НКТ (МПа)</returns>
		public void CalcStaticBottomPressure(GasFlow gasFlow, double bottomTemperature)
		{
			double Ptop = gasFlow.TopPressure;

            gasFlow.BottomParametersDefinition(Ptop, bottomTemperature);

			double orientirS = CalcSCoeff(gasFlow);
			double orientirP = Ptop * Math.Exp(orientirS) ;

            gasFlow.BottomParametersDefinition(orientirP, bottomTemperature);

            double Pbtm = 0;
			for (int i = 0; i < 3; i++)
			{
				double s = CalcSCoeff(gasFlow);
				Pbtm = Ptop * Math.Exp(s);
                gasFlow.BottomParametersDefinition(Pbtm, bottomTemperature);
            }
		}

		/// <summary>
		/// Вычисление давления в самом верху сегмента (можно сказать "устье" сегмента) НКТ 
		/// остановленной скважины по барометрической формуле. 
		/// Гриценко стр. 117 и 113 (для сухой скважины). 
		/// </summary>
		/// <param name="gasFlow">Поток сегмента НКТ</param>
		/// <param name="topTemperature">Температура внизу сегмента (К)</param>
		/// <returns>Давление на забое НКТ (МПа)</returns>
		public void CalcStaticTopPressure(GasFlow gasFlow, double topTemperature)
		{
			double Q = gasFlow.RateAtStandardConditions;
			double Pbtm = gasFlow.BottomPressure;

			gasFlow.TopParametersDefinition(Pbtm, topTemperature);

			double orientirS = CalcSCoeff(gasFlow);
			double orientirP = Pbtm / Math.Exp(orientirS);

			gasFlow.TopParametersDefinition(orientirP, topTemperature);

			double Ptop = 0;
			for (int i = 0; i < 3; i++)
			{
				double s = CalcSCoeff(gasFlow);
				Ptop = Pbtm / Math.Exp(s);
				gasFlow.TopParametersDefinition(Ptop, topTemperature);
			}
		}


		/// <summary>
		/// Вычисление давления в самом низу сегмента (можно сказать "забой" сегмента) НКТ по барометрической формуле. 
		/// Гриценко стр. 117 и 113 (для сухой скважины). 
		/// </summary>
		/// <param name="gasFlow">Поток сегмента НКТ</param>
		/// <param name="btmTemperature">Температура внизу сегмента (К)</param>
		/// <returns>Давление на забое НКТ (МПа)</returns>
		public void CalcPipePressure(GasFlow gasFlow, double btmTemperature)
		{
			double Q = gasFlow.RateAtStandardConditions;
			double Ptop = gasFlow.TopPressure;

            gasFlow.BottomParametersDefinition(Ptop, btmTemperature);

            double orientirS = CalcSCoeff(gasFlow);
			double orientirTeta = CalcTetaCoeff(gasFlow, orientirS);
			double orientirP = Math.Sqrt(Ptop * Ptop * Math.Exp(2 * orientirS) + orientirTeta * Q * Q);

            gasFlow.BottomParametersDefinition(orientirP, btmTemperature);

            double Pbtm = 0;
			for (int i = 0; i < 3; i++)
			{
				double s = CalcSCoeff(gasFlow);
				double Teta = CalcTetaCoeff(gasFlow, s);
				Pbtm = Math.Sqrt(Ptop * Ptop * Math.Exp(2 * s) + Teta * Q * Q);
                gasFlow.BottomParametersDefinition(Pbtm, btmTemperature);
            }
		}


		public Pipe (double diameter, double length, double depth, double roughness)
        {
			double eps = 0.1; // заданное число для сравнения действительных чисел

			if (diameter > 0.3)	throw new InvalidOperationException();
			if (length < 0 || length < diameter)	throw new InvalidOperationException();
			if (depth < 0 || depth > length+eps)	throw new InvalidOperationException();
			//Гриценко стр. 120
			if (roughness < 0.0015 * 0.001 || roughness > 1 * 0.001)	throw new InvalidOperationException();

			Diameter = diameter;
			Length = length;
            Depth = depth;
            Roughness = roughness;

            RelativeRoughness = 2 * Roughness / Diameter;
        }
    }
}
