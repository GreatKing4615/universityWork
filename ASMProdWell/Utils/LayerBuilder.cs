using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMProdWell.Components;

namespace ASMProdWell.Utils
{
	public class LayerBuilder
	{
		public static LayerBuilder One;

		/// <summary>
		/// Пластовое давление (МПа)
		/// </summary>
		public double ReservoirPressure { get;}

		/// <summary>
		/// Температура нейтрального слоя (К)
		/// </summary>
		public double NeutralLayerTemperature { get;}

		/// <summary>
		/// Коэффициент фильтрационного сопротивления a (безразмерная)
		/// </summary>
		public double a { get;}

		/// <summary>
		/// Коэффициент фильтрационного сопротивления b (безразмерная)
		/// </summary>
		public double b { get; }

		/// <summary>
		/// Коэффициент притока пластовой жидкости A (безразмерная)
		/// </summary>
		public double A { get; }

		/// <summary>
		/// Коэффициент притока пластовой жидкости B (безразмерная)
		/// </summary>
		public double B { get; }

		/// <summary>
		/// Коэффициент восстановления давления Alpha
		/// Гриценко стр 268
		/// </summary>
		private double Alpha;

		/// <summary>
		/// Коэффициент восстановления давления Beta
		/// Гриценко стр 268
		/// </summary>
		private double Beta;

		/// <summary>
		/// Коэффициент скопления воды A
		/// </summary>
		public double CoeffWaterAccumulationA;

		/// <summary>
		/// Коэффициент скопления воды B
		/// </summary>
		public double CoeffWaterAccumulationB;

		/// <summary>
		/// Коэффициент скопления воды C
		/// </summary>
		public double CoeffWaterAccumulationC;

		/// <summary>
		/// Конденсатогазовый фактор (т/тыс.м3)
		/// </summary>
		public double NaturalGasLiquidsFactor { get; }

		public Layer BuildLayer()
		{
			Layer layer =  new Layer(ReservoirPressure, a, b, NeutralLayerTemperature, NaturalGasLiquidsFactor, A, B, Alpha, Beta);
			return layer;
		}

		/// <summary>
		///	Построитель класса Layer
		/// </summary>
		/// <param name="layerReservoirPressure">Давление в пласте (МПа)</param>
		/// <param name="a">Коэффициент (безразмерная)</param>
		/// <param name="b">Коэффициент (безразмерная)</param>
		/// <param name="neutralLayerTemperature">Температура нейтрального слоя (К)</param>
		/// <param name="nglFactor">Конденсатогазовый фактор (т/тыс.м3)</param>
		/// <param name="A">Коэффициент притока A (безразмерная)</param>
		/// <param name="B">Коэффициент притока B (безразмерная)</param>
		public LayerBuilder(double reservoirPressure, double a, double b, double neutralLayerTemperature,
							double nglFactor, double A = Double.PositiveInfinity, double B = Double.PositiveInfinity, 
							double alpha = 0, double beta = 0)
		{
			ReservoirPressure = reservoirPressure;
			this.a = a;
			this.b = b;
			NeutralLayerTemperature = neutralLayerTemperature;
			NaturalGasLiquidsFactor = nglFactor;
			this.A = A;
			this.B = B;
			this.Alpha = alpha;
			this.Beta = beta;
		}
	}
}
