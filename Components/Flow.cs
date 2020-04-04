using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrototypeDryWell.Components.Fluids;

namespace PrototypeDryWell.Components
{
    /// <summary>
    /// Поток находящийся в сегменте НКТ, сегмент это часть НКТ для которой применяется барометрическая формула
    /// </summary>
    public sealed class Flow
    {
		#region Parameters

        /// <summary>
        /// Дебит газа (тыс.м3/сут)
        /// </summary>
        public double GasDischarge { get; private set; }

		/// <summary>
		/// Температура в самом низу сегмента потока (К)
		/// </summary>
        public double TopTemperature { get; private set; }

		/// <summary>
		/// Давление в самом низу сегмента потока (МПа)
		/// </summary>
		public double TopPressure { get; private set; }

		/// <summary>
		/// Температура в самом верху сегмента потока (К)
		/// </summary>
		public double BottomTemperature
		{
			get
			{
				return _bottomTemperature;
			}
			set
			{
				if (value < 0) throw new InvalidOperationException();
				_bottomTemperature = value;
				AvgTemperature = (_bottomTemperature + TopTemperature) / 2.0;
			}
		}
		private double _bottomTemperature;

		/// <summary>
		/// Давление в самом верху сегмента потока (МПа)
		/// </summary>
		public double BottomPressure {
			get {
				return _bottomPressure;
			}
			set {
				if (value < 0) throw new InvalidOperationException();
				_bottomPressure = value;
				AvgPressure = (_bottomPressure + TopPressure) / 2.0;
			}
		}
		private double _bottomPressure;

		/// <summary>
		/// Средняя температура потока (К)
		/// </summary>
		public double AvgTemperature { get; private set; }

		/// <summary>
		/// Среднее давление потока (МПа)
		/// </summary>
		public double AvgPressure { get; private set; }

		#endregion

		/// <summary>
		/// Флюид потока
		/// </summary>
		private Fluid Fluid { get; }

		/// <summary>
		/// Относительная плотность (безразмерная)
		/// </summary>
		public double RelativeDensity
        {
            get
            {
                return this.Fluid.GetRelativeDensity();
            }
        }

        /// <summary>
        /// Динамическая вязкость (сП | мПа*с)
        /// </summary>
        public double DynamicViscosity
        {
            get
            {
                return this.Fluid.GetDynamicViscosity(AvgPressure, AvgTemperature);
            }
        }

        /// <summary>
		/// Вычисление коэффициента сверхжимаемости газа
		/// </summary>
		/// <returns></returns>
        public double SupercompressibilityFactor
		{
			get
			{
				return Fluid.GetSupercompressibilityFactor(AvgPressure, AvgTemperature);
			}
		}

		/// <summary>
		/// Установка параметров в самом верху потока (можно сказать "устье" потока)
		/// и обнуление bottom и average значений этих парамметров
		/// </summary>
		public Flow Generate()
		{
			Flow generatedFlow = new Flow(this.Fluid, this.GasDischarge, this.BottomPressure, this.BottomTemperature);
			return generatedFlow;
		}

		public Flow (Fluid fluid, double gasDischarge, double topPressure, double topTemperature/*, double avgPressure=0, double avgTemperature=0*/)
        {
			if(gasDischarge < 0) throw new InvalidOperationException();
			GasDischarge = gasDischarge;
			Fluid = fluid;

			if(topPressure < 0) throw new InvalidOperationException();
			if(topTemperature < 0) throw new InvalidOperationException();

			TopPressure = topPressure;
			TopTemperature = topTemperature;
		}


		/*
		public Flow(Flow flow) {
			Fluid = flow.Fluid;
			GasDischarge = flow.GasDischarge;
			TopPressure = flow.TopPressure;
			TopTemperature = flow.TopTemperature;
			AvgPressure = flow.AvgPressure;
			AvgTemperature = flow.AvgTemperature;
		}*/
		
	}
}
