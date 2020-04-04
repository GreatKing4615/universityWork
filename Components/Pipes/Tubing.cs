using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrototypeDryWell.Components.Pipes
{
    public sealed class Tubing
    {
        /// <summary>
        /// Набор параметров потока в местах соединения секторов НКТ
        /// </summary>
        public List<Flow> SegmentFlows;
		
        public Pipe Pipe { get; }

		/// <summary>
		/// Диаметр трубы (м)
		/// </summary>
        public double PipeDiameter { get; }

		/// <summary>
		/// Абсолютная шероховатость трубы (безразмерная)
		/// </summary>
        public double PipeRoughness { get; }

        /// <summary>
        /// Длина НКТ (м)
        /// </summary>
        public double Length;

        /// <summary>
        /// Глубина НКТ (м)
        /// </summary>
        public double Depth;

        /// <summary>
        /// Количество секций НКТ (безразмерная)
        /// </summary>
        public readonly int NumberOfSegment = 1;

		/// <summary>
		/// Вычисление забойного давления НКТ по барометрической формуле (МПа)
		/// Гриценко стр. 117 и 113
		/// </summary>
		/// <param name="topFlow">Поток в самом верхнем сегменте НКТ</param>
		/// <param name="bottomholeTemperature">Температура на забое НКТ (K)</param>
		/// <returns>Давление на забое НКТ (МПа)</returns>
		public double GetBottomholePressure(Flow topFlow, double bottomholeTemperature)
        {

			double length = Length / NumberOfSegment;
            double depth = length;
			Pipe pipe = new Pipe(PipeDiameter, length, depth,  PipeRoughness);

			double dT = (bottomholeTemperature - topFlow.TopTemperature) / NumberOfSegment;

			Flow flow = topFlow;
			for (int i = 0; i < NumberOfSegment; i++)
            {
				flow.BottomTemperature = flow.TopTemperature + dT;
				flow.BottomPressure = GetPipePressure(flow, pipe); // давление в месте соединения сегментов НКТ

				SegmentFlows.Add(flow);

				flow = flow.Generate();
            }

            return SegmentFlows.Last().BottomPressure;
        }

		/// <summary>
		/// Вычисление давления в самом низу сегмента (можно сказать "забой" сегмента) НКТ по барометрической формуле
		/// Гриценко стр. 117 и 113
		/// </summary>
		/// <param name="flow">Поток сегмента НКТ</param>
		/// <param name="discharge">Дебит НКТ (тыс. м3/сут)</param>
		/// <returns>Давление на забое НКТ (МПа)</returns>
		public double GetPipePressure(Flow flow, Pipe pipe)
        {
			double Q = flow.GasDischarge;
			double Ptop = flow.TopPressure;

			flow.BottomPressure = Ptop;

			double orientirS = GetSCoeffitient(flow, pipe);
            double orientirTeta = GetTetaCoeffitient(flow, pipe, orientirS);
            double orientirP = Math.Sqrt(Ptop * Ptop * Math.Exp(2 * orientirS) + orientirTeta * Q * Q);

			flow.BottomPressure = orientirP;

			double Pbtm = 0;
			for (int i = 0; i < 3; i++)
			{
				double s = GetSCoeffitient(flow, pipe);
				double Teta = GetTetaCoeffitient(flow, pipe, s);
				Pbtm = Math.Sqrt(Ptop * Ptop * Math.Exp(2 * s) + Teta * Q * Q);
				flow.BottomPressure = Pbtm;
			}

			return flow.BottomPressure;
        }

        /// <summary>
        /// Вычисление Тета коэффициента 
        /// Гриценко стр. 117
        /// </summary>
        /// <param name="flow">Поток протекающий по трубе</param>
        /// <param name="pipe">Труба</param>
        /// <returns>Значение Тета коэффициента</returns>
        public double GetTetaCoeffitient(Flow flow, Pipe pipe, double sCoeff = 0)
        {
			double Z = flow.SupercompressibilityFactor;
			double T = flow.AvgTemperature;
            double D = pipe.Diameter;
			double s = sCoeff;
			if (sCoeff == 0)
				s = GetSCoeffitient(flow, pipe);
            double lambda = pipe.GetHydraulicResistance(flow);
            double Teta = 0.01413 * Math.Pow(10, -10) * Z * Z * T * T * (Math.Exp(2 * s) - 1) * lambda
                                                 / Math.Pow(D, 5);
            return Teta;
        }

        /// <summary>
        /// Вычисление s коэффициента
        /// Гриценко стр. 117
        /// </summary>
        /// <param name="flow">Поток</param>
        /// <returns>Значение коэффициента s</returns>
        private double GetSCoeffitient(Flow flow, Pipe pipe)
        {
			double rho = flow.RelativeDensity;
            double L = pipe.Depth;
			double Z = flow.SupercompressibilityFactor;
            double T = flow.TopTemperature;
			double s = (0.0683 * rho * L / (Z * T)) / 2;
            return s;
        }

		/// <summary>
		/// НКТ
		/// </summary>
		/// <param name="pipeDiameter">Диаметр трубы НКТ (м)</param>
		/// <param name="pipeRoughness">Абсолютная шероховатость НКТ (м)</param>
		/// <param name="length">Длина НКТ (м)</param>
		/// <param name="depth">Глубина НКТ (м)</param>
		public Tubing(double pipeDiameter, double pipeRoughness, double length, double depth)
        {
			SegmentFlows = new List<Flow>();
            PipeDiameter = pipeDiameter;
            PipeRoughness = pipeRoughness;
            Length = length;
            Depth = depth;
        }


    }
}
