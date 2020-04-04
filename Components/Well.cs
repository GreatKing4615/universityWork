using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrototypeDryWell.Components.Fluids;
using PrototypeDryWell.Components.Pipes;

namespace PrototypeDryWell.Components
{
    

	public sealed class Well
	{

        /// <summary>
        /// Глубина скважины (м)
        /// </summary>
        private double Depth;

		/// <summary>
		/// Устьевое давление (МПа)
		/// </summary>
		private double WellHeadPressure;

        /// <summary>
		/// Устьевая температура (К)
		/// </summary>
		private double WellHeadTemperature;

        /// <summary>
        /// Забойное давление (МПа)
        /// </summary>
        private double BottomholePressure;

		/// <summary>
		/// Забойная температура (К)
		/// </summary>
		private double BottomholeTemperature;

		/// <summary>
		/// Дебит (м3/сут)
		/// </summary>
		private double Discharge = 0;

		/// <summary>
		/// Продуктивный пласт
		/// </summary>
		private Layer Layer;

		/// <summary>
		/// НКТ
		/// </summary>
		private Tubing Tubing;

		/// <summary>
		/// Флюид
		/// </summary>
		private Fluid Fluid;

		/// <summary>
		/// Задание компонентов скважины
		/// </summary>
		public void InitTest()
		{
            WellHeadPressure = 3.18;
			WellHeadTemperature = 286;
            BottomholeTemperature = 302.74;
            Tubing = new Tubing( 0.076, 0.000115, 1725, 1725);
            Fluid = Fluid.GetTestFluid();
            Layer = new Layer(4.46, 0.012361, 0.00001427, 273+30);
        }

        public double Modeling(double eps)
		{
            Console.WriteLine("Well.Modeling():");

			double maxQ = Layer.GetMaxDischarge();
			double Q = maxQ / 2;

			Flow flow = new Flow(Fluid, Q, WellHeadPressure, WellHeadTemperature);

			double P1 = Tubing.GetBottomholePressure(flow, BottomholeTemperature);
            double P2 = Layer.GetBottomholePressure(Q);

            double step = Q / 2;
            Console.WriteLine("Q = {0} \nP1 = {1} \nP2 = {2} \n\nWellCycle:\n", Q, P1, P2);

            while ( Math.Abs(P1-P2) > eps)
            {
                // Если увеличиваем Q, то:
                //	-	P1 увеличивается, т.к. устьевое давление константно, а дебит увеличен, следовательно, 
                // увеличивается забойное давление.
                //	-	P2 уменьшается, т.к. пластовое давление константно, а приток к забою увеличивается, следовательно,
                // должно уменьшится забойное давление.
                //
                // Аналогично, когда Q уменьшается, то P1 уменьшается а P2 увеличивается.
                // 
                // Отсюда, если давление P1 больше давления P2, значит нужно брать правое половинное деление иначе 
                // берем левое половинное значение.
                if (P1 > P2) Q = Q - step;
                else Q = Q + step;
                step = step / 2;
				Console.WriteLine("\tQ = " + Q);

				flow = new Flow(Fluid, Q, WellHeadPressure, WellHeadTemperature);

				P1 = Tubing.GetBottomholePressure(flow, BottomholeTemperature);
                P2 = Layer.GetBottomholePressure(Q);
                Console.WriteLine("\tP1 = {0} \n\tP2 = {1}\n\n", P1, P2);
            }
            return Q;
		}
	}
}
