using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMProdWell.Components.Fluids;

namespace ASMProdWell.Components
{
    /// <summary>
    /// Газовый поток находящийся в сегменте НКТ, сегмент это часть НКТ для которой применяется барометрическая формула
    /// </summary>
    public sealed class GasFlow
    {

        /// <summary>
        /// Флюид потока
        /// </summary>
        private GasFluid Fluid { get; }

        #region Parameters

        /// <summary>
        /// Дебит газа при стандартных условиях (тыс.м3/сут)
        /// </summary>
        public double RateAtStandardConditions { get; private set; }

		/// <summary>
		/// Температура в самом низу сегмента потока (К)
		/// </summary>
        public double TopTemperature { get; private set; }

		/// <summary>
		/// Давление в самом низу сегмента потока (МПа)
		/// </summary>
		public double TopPressure { get; private set; }

        /// <summary>
        /// Средняя температура потока (К)
        /// </summary>
        public double AvgTemperature { get; private set; }

        /// <summary>
        /// Среднее давление потока (МПа)
        /// </summary>
        public double AvgPressure { get; private set; }

        /// <summary>
        /// Температура в самом верху сегмента потока (К)
        /// </summary>
        public double BottomTemperature
		{
			get
			{
				return _bottomTemperature;
			}
			private set
			{
				if (value < 0)
                    throw new InvalidOperationException("Ошибка класса GasFlow: Попытка присвоить температуре BottomTemperature отрицательное значение.");
				_bottomTemperature = value;
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
			private set {
				if (value < 0)
                    throw new InvalidOperationException("Ошибка класса GasFlow: Попытка присвоить давлению BottomPressure отрицательное значение.");
				_bottomPressure = value;                             
            }
		}
		private double _bottomPressure;


		/// <summary>
		/// Динамическая вязкость (сП | мПа*с)
		/// </summary>
		public double DynamicViscosity
        {
            get {
                if (_dynamicViscosity == 0)
                    throw new Exception("Ошибка класса GasFlow: Попытка использовать неинициальзованный параметр DynamicViscosity");
                return _dynamicViscosity;
            }
            private set {
                _dynamicViscosity = value;
            }
        }
        private double _dynamicViscosity;

        /// <summary>
		/// Вычисление Z коэффициента сверхжимаемости газа (безразмерная)
		/// </summary>
		/// <returns></returns>
        public double SupercompressibilityFactor
        {
            get {
                if (_supercompressibilityFactor == 0)
                    throw new Exception("Ошибка класса GasFlow: Попытка использовать неинициальзованный параметр SupercompressibilityFactor");
                return _supercompressibilityFactor;
            }
            private set {
                _supercompressibilityFactor = value;
            }
        }
        private double _supercompressibilityFactor;


        /// <summary>
        /// Относительная плотность газа при стандартных условиях (безразмерная)
        /// </summary>
        public double RelativeDensity
        {
            get {
                return Fluid.GetRelativeDensity();
            }
        }


        /// <summary>
        /// Плотность газа при стандартных условиях (кг/м3)
        /// </summary>
        public double DensityAtStandardConditions
        {
            get {
                return Fluid.DensityAtStandardConditions;
            }
        }

        #endregion

        /// <summary>
        /// Завершение определения потока через определение параметров на забоя
        /// </summary>
        /// <param name="bottomPressure">Забойное давление (МПа)</param>
        /// <param name="bottomTemperature">Забойная температура (К)</param>
        public void BottomParametersDefinition(double bottomPressure, double bottomTemperature)
        {
            BottomPressure = bottomPressure;
            BottomTemperature = bottomTemperature;

            AvgTemperature = (BottomTemperature + TopTemperature) / 2.0;
            AvgPressure = (BottomPressure + TopPressure) / 2.0;

            DynamicViscosity = Fluid.CalcDynamicViscosity(AvgPressure, AvgTemperature);
            SupercompressibilityFactor = Fluid.CalcSupercompressibilityFactor(AvgPressure, AvgTemperature);
        }

		/// <summary>
		/// Завершение определения потока через определение параметров на устье
		/// </summary>
		/// <param name="topPressure">Устьевое давление (МПа)</param>
		/// <param name="topTemperature">Устьевая температура (К)</param>
		public void TopParametersDefinition(double topPressure, double topTemperature)
		{
			TopPressure = topPressure;
			TopTemperature = topTemperature;

			AvgTemperature = (BottomTemperature + TopTemperature) / 2.0;
			AvgPressure = (BottomPressure + TopPressure) / 2.0;

			DynamicViscosity = Fluid.CalcDynamicViscosity(AvgPressure, AvgTemperature);
			SupercompressibilityFactor = Fluid.CalcSupercompressibilityFactor(AvgPressure, AvgTemperature);
		}

		/// <summary>
		/// Перерасчет плотности газа из стандартных условий к рабочим условиям (кг/м3)
		/// </summary>
		/// <returns>Плотность газа в рабочих условиях (кг/м3)</returns>
		public double CalcDensityAtOperatingConditions()
        {
			double RHOgasSt = Fluid.DensityAtStandardConditions;
			double Pavg = AvgPressure;
            double Tst = PhysicalConstants.TemperatureAtStandardConditions;
            double Pat = PhysicalConstants.AtmosphericPressure;
            double Tavg = AvgTemperature;
            double Zavg = SupercompressibilityFactor;
            double RHOgasWork = RHOgasSt * Pavg * Tst / (Pat * Tavg * Zavg);
			return RHOgasWork;
		}


		/// <summary>
		/// Перерасчет дебита газа из стандартных условий к рабочим условиям (тыс.м3/сут)
		/// </summary>
		/// <returns>Расход газа в рабочих условиях (тыс.м3/сут)</returns>
        public double CalcRateAtOperatingConditions()
        {
            double Qg = RateAtStandardConditions;
            double Pat = PhysicalConstants.AtmosphericPressure;
            double Tavg = AvgTemperature;
            double Zavg = SupercompressibilityFactor;
            double Pavg = AvgPressure;
            double Tst = PhysicalConstants.TemperatureAtStandardConditions;
            double Qgw = Qg * Pat * Tavg * Zavg / (Pavg * Tst);
            return Qgw;
        }

        /// <summary>
        /// Генерация нижнего потока
        /// </summary>
        public GasFlow GenerateBottomFlow()
		{
			GasFlow generatedFlow = new GasFlow(Fluid, RateAtStandardConditions, BottomPressure, BottomTemperature);
			return generatedFlow;
		}

		/// <summary>
		/// Генерация верхнего потока
		/// </summary>
		public GasFlow GenerateTopFlow()
		{
			GasFlow generatedFlow = new GasFlow(Fluid, RateAtStandardConditions, TopPressure, TopTemperature, true);
			return generatedFlow;
		}

		/// <summary>
		/// Газовый поток для вычисления забойного давления сверху вниз
		/// </summary>
		/// <param name="fluid">Газовый флюид</param>
		/// <param name="gasRate">Расход газа в стандартных условиях (тыс.м3/сут)</param>
		/// <param name="topPressure">Давление на устье (МПа)</param>
		/// <param name="topTemperature">Температура на устье (МПа)</param>
		public GasFlow (GasFluid fluid, double gasRate, double topPressure, double topTemperature)
        {
			if(gasRate < 0) throw new ArgumentOutOfRangeException("Дебит газа меньше нуля при инициализации GasFlow");
			RateAtStandardConditions = gasRate;
			Fluid = fluid;

			if(topPressure < 0) throw new ArgumentOutOfRangeException("Давление на устье потока меньше нуля при инициализации GasFlow");
			if (topTemperature < 0) throw new ArgumentOutOfRangeException("Температура на устье потока меньше нуля при инициализации GasFlow"); ;

			TopPressure = topPressure;
			TopTemperature = topTemperature;
		}

		/// <summary>
		/// Газовый поток для вычисления устьевого давления снизу вверх
		/// </summary>
		/// <param name="fluid">Газовый флюид</param>
		/// <param name="gasRate">Расход газа в стандартных условиях (тыс.м3/сут)</param>
		/// <param name="bottomPressure">Давление на забое (МПа)</param>
		/// <param name="bottomTemperature">Температура на забое (МПа)</param>
		/// <param name="fromBottomToUp">Флаг для отличия от конструктора GasFlow, вычисляющего забойное давление</param>
		public GasFlow(GasFluid fluid, double gasRate, double bottomPressure, double bottomTemperature, bool fromBottomToUp)
		{
			if (!fromBottomToUp)
				return;
			if (gasRate < 0) throw new ArgumentOutOfRangeException("Дебит газа меньше нуля при инициализации GasFlow");
			RateAtStandardConditions = gasRate;
			Fluid = fluid;

			if (bottomPressure < 0) throw new ArgumentOutOfRangeException("Давление на забое потока меньше нуля при инициализации GasFlow");
			if (bottomTemperature < 0) throw new ArgumentOutOfRangeException("Температура на забое потока меньше нуля при инициализации GasFlow"); ;

			BottomPressure = bottomPressure;
			BottomTemperature = bottomTemperature;
		}

	}
}
