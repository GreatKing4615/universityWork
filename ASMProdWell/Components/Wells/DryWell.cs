using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMProdWell.Components.Fluids;
using ASMProdWell.Components.Pipes;
using ASMProdWell.Components;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ASMProdWell.Utils;

namespace ASMProdWell.Components
{
    [Table("Well")]
    public class DryWell
	{

        public int Id { get; set; }

        public String Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime MeasuringDate { get; set; }

        /// <summary>
        /// Устьевое давление (МПа)
        /// </summary>
        public double WellheadPressure { get; set; }

		[NotMapped]
		/// <summary>
		/// Забойное давление (МПа)
		/// </summary>
		public double BottomholePressure { get; set; }

		/// <summary>
		/// Устьевая температура (К)
		/// </summary>
		public double WellheadTemperature { get; set; }

        /// <summary>
        /// Забойная температура (К)
        /// </summary>
        public double BottomholeTemperature { get; set; }

        /// <summary>
        /// Продуктивный пласт
        /// </summary>
        public Layer Layer { get; set; }

        /// <summary>
        /// НКТ
        /// </summary>
        public Tubing Tubing { get; set; }

        /// <summary>
        /// Газовый флюид
        /// </summary>
        [ForeignKey("Id")]
        public GasFluid GasFluid;

        /// <summary>
        /// Флюид газового конденсата
        /// </summary>
        [ForeignKey("Id")]
        public NaturalGasLiquidsFluid NglFluid { get; set; }

		/// <summary>
		/// Эквивалентная затрубному пространству труба
		/// </summary>
		[NotMapped]
		protected Tubing AnnularTubing { get; set; }

		/// <summary>
		/// Вычислить дебит конденсата (т/сут)
		/// </summary>
		/// <param name="Qgas">Дебит газа (тыс.м3/сут)</param>
		/// <returns></returns>
		public double CalcNglRate(double Qgas)
		{
			double Qngl = NglFluid.CalcMassRate(Layer.NGLFactor, Qgas);
			return Qngl;
		}

		/// <summary>
		/// Задание компонентов сухой скважины
		/// </summary>
		/// <param name="wellheadPressure">Давление на устье (МПа)</param>
		/// <param name="wellheadTemperature">Температура на устье (К)</param>
		/// <param name="bottomholeTemperature">Температура на забое (К)</param>
		/// <param name="nglDensity">Плотность газового конденсата (кг/м3)</param>
		public void InitWell(double wellheadPressure, double wellheadTemperature, 
								double bottomholeTemperature, double nglDensity)
		{
			if (wellheadPressure < PhysicalConstants.AtmosphericPressure || wellheadPressure > 100)
				throw new ArgumentException("Устьевое давление должно быть меньше 100 МПа и больше атмосферного!");
			WellheadPressure = wellheadPressure;

			if (wellheadTemperature < 220 || wellheadTemperature > 400)
				throw new ArgumentException("Устьевая температура должна быть меньше 400 К и больше 220 К !");
			WellheadTemperature = wellheadTemperature;

			if (bottomholeTemperature < 220 || bottomholeTemperature > 400)
				throw new ArgumentException("Забойная температура должна быть меньше 400 К и больше 220 К !");
			BottomholeTemperature = bottomholeTemperature;

			Tubing = TubingBuilder.One.BuildTubing();
			GasFluid = GasFluid.GetFluid();
			Layer = LayerBuilder.One.BuildLayer();
			NglFluid = new NaturalGasLiquidsFluid(nglDensity); 
		}


		/// <summary>
		/// Функция моделирования работы сухой скважины
		/// </summary>
		/// <param name="eps">Точность с которой будет расчитываться забойное давление (безразмерная)</param>
		/// <param name="bottomHolePressure">Забойное давление (МПа)</param>
        /// <param name="nglRate">Дебит конденсата (т/сут)</param>
		/// <returns>Дебит газа в стандартных условиях (тыс.куб.м/сут)</returns>
		public virtual double Modeling(double eps, out double bottomHolePressure, out double nglRate)
		{
			double maxQ = Layer.CalcMaxGasRate();
			double gasQ = maxQ / 2;
			double step = gasQ / 2;
			double P1, P2;
			do
			{
				GasFlow gasFlow = new GasFlow(GasFluid, gasQ, WellheadPressure, WellheadTemperature);
				if (Layer.NGLFactor > 0)
				{
					double nglQ = NglFluid.CalcVolumeRate(Layer.NGLFactor, gasQ);
					LiquidFlow liquidFlow = new LiquidFlow(nglQ, NglFluid);
					P1 = Tubing.CalcBottomholePressure(gasFlow, liquidFlow, BottomholeTemperature);
				}
				else
				{
					P1 = Tubing.CalcBottomholePressure(gasFlow, BottomholeTemperature);
				}
				P2 = Layer.CalcBottomholePressure(gasQ);

				// Если увеличиваем Q, то:
				//	-	P1 увеличивается, т.к. устьевое давление константно, а дебит увеличен, следовательно, 
				// увеличивается забойное давление.
				//	-	P2 уменьшается, т.к. пластовое давление константно, а приток к забою увеличивается, следовательно,
				// должно уменьшится забойное давление.
				//
				// Аналогично, когда Q уменьшается, то P1 уменьшается а P2 увеличивается.
				// 
				// Отсюда, если давление P1 больше давления P2, значит нужно брать левое половинное деление, иначе 
				// берем правое половинное значение.
				if (Math.Abs(P1 - P2) <= eps) break;
				else if (P1 > P2) gasQ = gasQ - step;
				else gasQ = gasQ + step;

				if (gasQ < 0 || gasQ > maxQ)
				{
					gasQ = 0;
					break;
				}
				step = step / 2;

			} while (true);
            
			bottomHolePressure = P1;
			nglRate = CalcNglRate(gasQ);
			return gasQ;
		}


		/// <summary>
		/// Функция получения точек кривых притока P2 и оттока P1 для узлового анализа сухой скважины
		/// </summary>
		/// <param name="num">Число точек</param>
		/// <param name="P1">Массив значений забойного давления для точек кривой оттока с забоя (МПа)</param>
		/// <param name="P2">Массив значений забойного давления для точек кривой притока из пласта к забою (МПа)</param>
		/// <param name="Q">Массив значений дебита газа (тыс.куб.м/сут)</param>
		public virtual void GetPointsForNodeAnalize(int num, out double[] P1, out double[] P2, out double[] Q)
		{
			P1 = new double[num];
			P2 = new double[num];
			Q = new double[num];
			double maxQ = Layer.CalcMaxGasRate();
			double step = maxQ / num;
			double gasQ = 0;
			//создается новый Tubing чтобы не перезаписывать SegmentFlows
			Tubing tubing = new Tubing(Tubing);
			for (int i=0; i<num; i++ )
			{
				Q[i] = gasQ;
				GasFlow gasFlow = new GasFlow(GasFluid, gasQ, WellheadPressure, WellheadTemperature);
				if (Layer.NGLFactor > 0)
				{
					double nglQ = NglFluid.CalcVolumeRate(Layer.NGLFactor, gasQ);
					LiquidFlow liquidFlow = new LiquidFlow(nglQ, NglFluid);
					P1[i] = Tubing.CalcBottomholePressure(gasFlow, liquidFlow, BottomholeTemperature);
				}
				else
				{
					P1[i] = tubing.CalcBottomholePressure(gasFlow, BottomholeTemperature);
				}
				P2[i] = Layer.CalcBottomholePressure(gasQ);
				gasQ += step;
			}
		}

        /// <summary>
        /// Получение параметров сегментов НКТ
        /// </summary>
        /// <param name="H">Глубина отсчитывается от устья(м)</param>
        /// <param name="P">Давление (МПа)</param>
        /// <param name="T">Температура (К)</param>
        public void GetSegmentParametersForTubing(out double[] H, out double[] P, out double[] T)
        {
            int num = Tubing.NumberOfSegments + 1;
            P = new double[num];
            T = new double[num];
            H = new double[num];

            double stepH = Tubing.Depth / (num - 1);
            double depth = 0;

            for (int i = 0; i < Tubing.NumberOfSegments; i++)
            {
                H[i] = depth;
                P[i] = Tubing.SegmentFlows[i].TopPressure;
                T[i] = Tubing.SegmentFlows[i].TopTemperature;
                depth += stepH;
            }

            H[num - 1] = depth;
            P[num - 1] = Tubing.SegmentFlows.Last().BottomPressure;
            T[num - 1] = Tubing.SegmentFlows.Last().BottomTemperature;
        }


		/// <summary>
		/// Функция высчитывания забойного давления остановленной скважины по затрубному давлению (МПа)
		/// </summary>
		public double CalcStaticBottomholePressureByAnnularTubing(double annularWellheadPressure)
		{
			AnnularTubing = new Tubing(Tubing);
			AnnularTubing.TubingDiameter = Tubing.CalcEquivalentAnnularDiameter();

			DryWell well = new DryWell(this);
			well.Tubing = AnnularTubing;
			well.WellheadPressure = annularWellheadPressure;

			GasFlow gasFlow = new GasFlow(GasFluid, 0, annularWellheadPressure, WellheadTemperature);
			double Pb = well.Tubing.CalcStaticBottomholePressure(gasFlow, BottomholeTemperature);

			return Pb;
		}

		/// <summary>
		/// Функция высчитывания затрубного давления на устье остановленной скважины по забойному давлению (МПа)
		/// </summary>
		public double CalcStaticAnnularTubingByBottomholePressure(double bottomholePressure)
		{
			AnnularTubing = new Tubing(Tubing);
			AnnularTubing.TubingDiameter = Tubing.CalcEquivalentAnnularDiameter();

			DryWell well = new DryWell(this);
			well.Tubing = AnnularTubing;
			well.WellheadPressure = 0;
			well.BottomholePressure = bottomholePressure;

			GasFlow gasFlow = new GasFlow(GasFluid, 0, bottomholePressure, BottomholeTemperature, true);
			
			double annularWellheadPressure = well.Tubing.CalcStaticWellheadPressure(gasFlow, WellheadTemperature);

			return annularWellheadPressure;
		}

		/// <summary>
		/// Сухая скважина
		/// </summary>
		/// <param name="layer">Пласт</param>
		/// <param name="gasFluid">Газовый флюид</param>
		/// <param name="tubing">Обсадная колонна и НКТ</param>
		/// <param name="wellheadPressure">Устьевое давление (МПа)</param>
		/// <param name="wellheadTemperature">Устьевая температура (К)</param>
		/// <param name="bottomholeTemperature">Забойная температура (К)</param>
		/// <param name="nglFluid">Флюид конденсата</param>
		public DryWell(Layer layer, GasFluid gasFluid, 
                        Tubing tubing, double wellheadPressure,
                        double wellheadTemperature, double bottomholeTemperature,
						NaturalGasLiquidsFluid nglFluid)
        {
            Layer = layer;
            GasFluid = gasFluid;
            Tubing = tubing;
			WellheadPressure = wellheadPressure;
			WellheadTemperature = wellheadTemperature;
            BottomholeTemperature = bottomholeTemperature;
			NglFluid = nglFluid;
        }

		/// <summary>
		/// Конструктор копирования DryWell
		/// </summary>
		/// <param name="dryWell"></param>
		public DryWell(DryWell dryWell)
		{

			Layer = dryWell.Layer;
			GasFluid = dryWell.GasFluid;
			Tubing = new Tubing(dryWell.Tubing);
			WellheadPressure = dryWell.WellheadPressure;
			WellheadTemperature = dryWell.WellheadTemperature;
			BottomholeTemperature = dryWell.BottomholeTemperature;
			NglFluid = dryWell.NglFluid;
		}

		public DryWell() { }

    }
}
