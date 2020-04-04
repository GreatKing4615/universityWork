using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMProdWell;
using ASMProdWell.Components;
using ASMProdWell.Components.Fluids;
using ASMProdWell.Components.Pipes;
using System.ComponentModel.DataAnnotations.Schema;
using ASMProdWell.Components.Equipment.Pumps;
using ASMProdWell.Components.Equipment.PlungerLift;
using ASMProdWell.Utils;

namespace ASMProdWell.Components
{
	[Table("Well")]
	class WateredWell : DryWell
	{
        /// <summary>
        /// Водный флиюд
        /// </summary>
        [NotMapped]
        public WaterFluid WaterFluid { get; set; }


		/// <summary>
		/// Задание компонентов обводненной скважины
		/// </summary>
		/// <param name="wellheadPressure">Давление на устье (МПа)</param>
		/// <param name="wellheadTemperature">Температура на устье (К)</param>
		/// <param name="bottomholeTemperature">Температура на забое (К)</param>
		public void InitWell(double wellheadPressure, double wellheadTemperature, 
								double bottomholeTemperature, double nglDensity, double waterDensity)
		{
			base.InitWell( wellheadPressure, wellheadTemperature, bottomholeTemperature, nglDensity);
			WaterFluid = new WaterFluid(waterDensity); // Не работает
			NglFluid = new NaturalGasLiquidsFluid(nglDensity); // Не работает
		}

		/// <summary>
		/// Критический дебит газа, для выноса жидкости
		/// </summary>
		/// <param name="P">Среднее давление газа (МПа)</param>
		/// <returns></returns>
		public double CalcCriticalGasRate(double P)
		{
			double t = 86400; // секунд в сутках
			P = P * 145.04; //Перевод МПа в фунт/кв. дюйм
			double Vc = 4.434 * Math.Pow((67 - 0.0031 * P), 0.25) / Math.Pow((0.0031 * P), 0.5); // футов / сек
			Vc = Vc * 0.3048; // Перевод фут/с в м/с
			double S = Tubing.CrossSectionArea;
			//Расход измеряется в тыс.куб.м/сут
			double Qc = S * Vc * t / 1000;
			return Qc;
		}


		/// <summary>
		/// Функция моделирования работы обводненной фонтанной скважины, возвращает дебит газа (тыс.м3/сут)
		/// </summary>
		/// <param name="eps">Точность с которой будет расчитываться забойное давление (безразмерная)</param>
		/// <param name="A">Коэффициент притока жидкости А (безразмерная)</param>
		/// <param name="B">Коэффициент притока жидкости B (безразмерная)</param>
		/// <param name="bottomHolePressure">Забойное давление (МПа)</param>
		/// <param name="waterRate">Расход жидкости (м3/сут)</param>
		/// <param name="nglRate">Расход газового конденсата (м3/сут)</param>
		/// <returns>Дебит газа в стандартных условиях (тыс.куб.м/сут)</returns>
		public double Modeling(double eps, out double bottomHolePressure, out double waterRate, out double nglRate)
		{

			double maxGasQ = Layer.CalcMaxGasRate();
			double gasQ = maxGasQ / 2;
			double step = gasQ / 2;
			double P1, P2, waterQ;
			do
			{
				P2 = Layer.CalcBottomholePressure(gasQ);

				waterQ = Layer.CalcWaterRate(P2);
				double nglQ = NglFluid.CalcVolumeRate(Layer.NGLFactor, gasQ);
				LiquidFlow liquidFlow = new LiquidFlow(nglQ, NglFluid, waterQ, WaterFluid);
				GasFlow gasFlow = new GasFlow(GasFluid, gasQ, WellheadPressure, WellheadTemperature);
				P1 = Tubing.CalcBottomholePressure(gasFlow, liquidFlow, BottomholeTemperature);

				// Если увеличиваем Q, то:
				//	-	P1 увеличивается, т.к. устьевое давление константно, а дебит увеличен, следовательно, 
				// увеличивается забойное давление.
				//	-	P2 уменьшается, т.к. пластовое давление константно, а приток к забою увеличивается, следовательно,
				// должно уменьшится забойное давление.
				//
				// Аналогично, когда Q уменьшается, то P1 уменьшается а P2 увеличивается.
				// 
				// Отсюда, если давление P1 больше давления P2, значит нужно брать левое половинное деление иначе 
				// берем правое половинное значение.
				if (Math.Abs(P1 - P2) <= eps) break;
				if (P1 > P2) gasQ = gasQ - step;
				else gasQ = gasQ + step;

				if (gasQ < 0 || gasQ > maxGasQ)
				{
					gasQ = 0;
					break;
				}

				step = step / 2;
			} while (true);
            bottomHolePressure = P1;
            waterRate = waterQ;
			nglRate = CalcNglRate(gasQ);
            return gasQ;
		}

		/// <summary>
		/// Функция моделирования работы обводненной скважины с погружным насосом, возвращает дебит газа (тыс.м3/сут)
		/// </summary>
		/// <param name="eps">Точность с которой будет расчитываться забойное давление (безразмерная)</param>
		/// <param name="dynamicH">Динамический уровень пластовой жидкости, отсчитывается от входа в насос (м)</param>
		/// <param name="bottomHolePressure">Забойное давление (МПа)</param>
		/// <param name="waterRate">Расход жидкости (м3/сут)</param>
		/// <param name="nglRate">Дебит газового конденсата (т/сут)</param>
		/// <returns>Дебит газа в стандартных условиях (тыс.куб.м/сут)</returns>
		public double ModelingPump(double eps, double dynamicH,	out double bottomHolePressure, out double waterRate, out double nglRate)
		{

			//Скважина делится на 3 части: 
			//topPartWell - часть затрубного пространства, выше динамического уровня
			//bottomPartWell - часть затрубного пространства, ниже динамического уровня
			//innerPartWell - НКТ
			double topPartLength = Tubing.Length - dynamicH;
			double dynamicHTemperature = BottomholeTemperature + (WellheadTemperature - BottomholeTemperature) * (dynamicH / Tubing.Length);

			
			AnnularTubing = new Tubing(Tubing);
			AnnularTubing.TubingDiameter = Tubing.CalcEquivalentAnnularDiameter();

			//Для затрубного пространства свободного от жидкости создаем эквивалент сухой скважины
			DryWell topPartWell = new DryWell(this);
			topPartWell.Tubing = AnnularTubing;
			topPartWell.BottomholePressure = dynamicHTemperature;


			double Ph; //давление в точке динамического уровня
			double gasQ = topPartWell.Modeling(eps, out Ph, out nglRate);

			bottomHolePressure = Ph + WaterFluid.CalcColumnPressure(dynamicH);
			//Давление для притока воды берется как давление на середине динамического уровня
			double waterPress = Ph + WaterFluid.CalcColumnPressure(dynamicH / 2);
			waterRate = Layer.CalcWaterRate(bottomHolePressure);

			return gasQ;
		}

		/// <summary>
		/// Моделирование работы скважины с идеальным ЭЦН и подбор реальных ЭЦН соответствующих идеальному. Возвращает список подобранных насосов. 
		/// </summary>
		/// <param name="eps">Точность с которой будет расчитываться забойное давление (безразмерная)</param>
		/// <param name="dynamicH">Динамический уровень пластовой жидкости, отсчитывается от входа в насос (м)</param>
		/// <param name="CatalogEsp">Каталог ЭЦН</param>
		/// <param name="bottomholePressure">Забойное давление (МПа)</param>
		/// <param name="gasRate">Дебит газа (тыс.м3/сут)</param>
		/// <param name="waterRate">Дебит жидкости (м3/сут)</param>
		/// <param name="nglRate">Дебит газового конденсата (т/сут)</param>
		/// <returns></returns>
		public List<SettingForESP> ModelingESP(double eps, double dynamicH, List<ElectricSubmersiblePump> CatalogEsp, out double bottomholePressure, 
												out double gasRate, out double waterRate, out double nglRate)
		{
			gasRate = ModelingPump(eps, dynamicH, out bottomholePressure, out waterRate, out nglRate);
			List<SettingForESP> chosenPumps = new List<SettingForESP>();
			double head = Pump.CalcNeededHeadHeight(bottomholePressure, WellheadPressure, Tubing.Depth, WaterFluid);
			chosenPumps = ElectricSubmersiblePump.ChoseESP(Tubing.TubingDiameter, waterRate, head, CatalogEsp);

			return chosenPumps;
		}

		/// <summary>
		/// Моделирование работы скважины с идеальным ВШН и подбор реальных ВШН соответствующего идеальному. Возвращает список подобранных насосов. 
		/// </summary>
		/// <param name="eps">Точность с которой будет расчитываться забойное давление (безразмерная)</param>
		/// <param name="dynamicH">Динамический уровень пластовой жидкости, отсчитывается от входа в насос (м)</param>
		/// <param name="CatalogPcp">Каталог ВШН</param>
		/// <param name="bottomholePressure">Забойное давление (МПа)</param>
		/// <param name="gasRate">Дебит газа (тыс.м3/сут)</param>
		/// <param name="waterRate">Дебит жидкости (м3/сут)</param>
		/// <param name="nglRate">Дебит газового конденсата (т/сут)</param>
		/// <returns></returns>
		public List<SettingForPCP> ModelingPCP(double eps, double dynamicH, List<ProgressiveCavityPump> CatalogPcp, out double bottomholePressure,
												out double gasRate, out double waterRate, out double nglRate)
		{
			gasRate = ModelingPump(eps, dynamicH, out bottomholePressure, out waterRate, out nglRate);
			List<SettingForPCP> chosenPumps = new List<SettingForPCP>();
			double head = Pump.CalcNeededHeadHeight(bottomholePressure, WellheadPressure, Tubing.Depth, WaterFluid);
			chosenPumps = ProgressiveCavityPump.ChosePCP(Tubing.TubingDiameter, waterRate, head, CatalogPcp);

			return chosenPumps;
		}

		/// <summary>
		/// Функция моделирования работы плунжерного лифта
		/// </summary>
		/// <param name="eps">Точность с которой будет расчитываться забойное давление (безразмерная)</param>
		/// <param name="plunger">Плунжер</param>
		public PlungerModelingResult ModelingPlunger(double eps, Plunger plunger)
		{
			PlungerModelingResult mr = new PlungerModelingResult();

			mr.StartGasRate = Modeling(eps, out mr.StartBtmhPressure, out mr.StartWaterRate, out mr.StartNglRate);

			double P_avg = (WellheadPressure + mr.StartBtmhPressure) / 2;
			mr.CritQ = CalcCriticalGasRate(P_avg);

			//создается новый Tubing чтобы не перезаписывать SegmentFlows
			Tubing tubing = new Tubing(Tubing);

			mr.CloseGasRate = (mr.StartGasRate - mr.CritQ) / 2;
			double step = (mr.StartGasRate - mr.CritQ) / 4;
			int i = 0;
			do
			{
				i++;
				mr.CloseBtmhPressure = Layer.CalcBottomholePressure(mr.CloseGasRate);

				mr.CloseWaterRate = Layer.CalcWaterRate(mr.CloseBtmhPressure);
				double closeNglVolumeRate = NglFluid.CalcVolumeRate(Layer.NGLFactor, mr.CloseGasRate);
				mr.CloseNglRate = CalcNglRate(mr.CloseGasRate);
				LiquidFlow liquidFlow = new LiquidFlow(closeNglVolumeRate, NglFluid, mr.CloseWaterRate, WaterFluid);

				GasFlow gasFlow = new GasFlow(GasFluid, mr.CloseGasRate, WellheadPressure, WellheadTemperature);

				//Давление выше столба жидкость высотой h
				double Ph = tubing.CalcBottomholePressure(gasFlow, liquidFlow, BottomholeTemperature);
				//разница  (P2 - P1) = rho*g*h создается столбом жидкости на забое
				mr.ColumnHeight = WaterFluid.CalcColumnHeight(mr.CloseBtmhPressure - Ph);

				// Объем столба воды над плунжером
				mr.ColumnWaterVolume = WaterFluid.CalcVolumeOfColumnHeight(mr.ColumnHeight, Tubing.CrossSectionArea);

				//Находим затрубное давление необходимое для поднятия плунжера с водой
				double Pc_max = plunger.CalcMaxCasingPressure(WellheadPressure, Tubing.Length, mr.ColumnWaterVolume,
											Tubing.AnnularCrossSectionArea, Tubing.CrossSectionArea);
				//Забойное давление необходимое для поднятия плунжера с водой
				mr.OpenBtmhPressure = CalcStaticBottomholePressureByAnnularTubing(Pc_max);
				if (mr.OpenBtmhPressure < Layer.ReservoirPressure)
				{
					return mr;
				}

				if (Ph > mr.CloseBtmhPressure)
					mr.CloseGasRate = mr.CloseGasRate - step;
				else
					mr.CloseGasRate = mr.CloseGasRate + step;

				step = step / 2;
				if (i > 100)
					break;
			} while (true);

			mr = new PlungerModelingResult();
			return mr;
		}


		/// <summary>
		/// Функция получения точек кривых притока P2 и оттока P1 для узлового анализа обводненной скважины (без откачки)
		/// </summary>
		/// <param name="num">Число точек</param>
		/// <param name="P1">Массив значений забойного давления для точек кривой оттока с забоя (МПа)</param>
		/// <param name="P2">Массив значений забойного давления для точек кривой притока из пласта к забою (МПа)</param>
		/// <param name="gasDischarge">Массив значений дебита газа (тыс.куб.м/сут)</param>
		/// <param name="waterDischarge">Массив значений дебит воды (м3/сут)</param>
		public void GetPointsForNodeAnalize(int num, out double[] P1, out double[] P2, out double[] gasDischarge, out double[] waterDischarge)
		{
			P1 = new double[num];
			P2 = new double[num];
			gasDischarge = new double[num];
			waterDischarge = new double[num];
			double maxQ = Layer.CalcMaxGasRate();
			double step = maxQ / num;
			double gasQ = 0;
			//создается новый Tubing чтобы не перезаписывать SegmentFlows
			Tubing tubing = new Tubing(Tubing);

			for (int i = 0; i < num; i++)
			{
				gasDischarge[i] = gasQ;
				P2[i] = Layer.CalcBottomholePressure(gasQ);

				double waterQ = Layer.CalcWaterRate(P2[i]);
				double nglQ = NglFluid.CalcVolumeRate(Layer.NGLFactor, gasQ);
				LiquidFlow liquidFlow = new LiquidFlow(nglQ, NglFluid, waterQ, WaterFluid);
				GasFlow gasFlow = new GasFlow(GasFluid, gasQ, WellheadPressure, WellheadTemperature);

				P1[i] = tubing.CalcBottomholePressure(gasFlow, liquidFlow, BottomholeTemperature);
				waterDischarge[i] = waterQ;
				gasQ += step;
			}
		}

		/// <summary>
		/// Функция получения точек кривых притока P2 и оттока P1 по затрубному пространству для узлового анализа
		/// обводненной скважины (с откачкой насосом)
		/// </summary>
		/// <param name="dynamicH">Динамический уровень пластовой жидкости, отсчитывается от входа в насос (м)</param>
		/// <param name="num">Число точек</param>
		/// <param name="P1">Массив значений забойного давления для точек кривой оттока с забоя (МПа)</param>
		/// <param name="P2">Массив значений забойного давления для точек кривой притока из пласта к забою (МПа)</param>
		/// <param name="gasDischarge">Массив значений дебита газа (тыс.куб.м/сут)</param>
		/// <param name="waterDischarge">Массив значений дебит воды (м3/сут)</param>
		public void GetPointsAnnularTubingNodeAnalize(double dynamicH, int num, out double[] P1, out double[] P2, out double[] gasDischarge, out double[] waterDischarge)
		{
			double topPartLength = Tubing.Length - dynamicH;
			double dynamicHTemperature = BottomholeTemperature + (WellheadTemperature - BottomholeTemperature) * (dynamicH / Tubing.Length);


			P1 = new double[num];
			P2 = new double[num];
			gasDischarge = new double[num];
			waterDischarge = new double[num];

			double maxQ = Layer.CalcMaxGasRate();
			double step = maxQ / num;
			double gasQ = 0;

			//создается новый Tubing чтобы не перезаписывать SegmentFlows
			Tubing annularTubing = new Tubing(AnnularTubing);

			//Для затрубного пространства свободного от жидкости создаем эквивалент сухой скважины
			DryWell topPartWell = new DryWell(this);
			topPartWell.Tubing = annularTubing;
			topPartWell.BottomholePressure = dynamicHTemperature;

			for (int i = 0; i < num; i++)
			{
				gasDischarge[i] = gasQ;

				GasFlow gasFlow = new GasFlow(GasFluid, gasQ, WellheadPressure, WellheadTemperature);

				P2[i] = topPartWell.Layer.CalcBottomholePressure(gasQ);
				P1[i]= topPartWell.Tubing.CalcBottomholePressure(gasFlow, dynamicHTemperature);
				//Давление для притока воды берется как давление на середине динамического уровня
				double waterPres = P2[i] + WaterFluid.CalcColumnPressure(dynamicH / 2);
				waterDischarge[i] = Layer.CalcWaterRate(waterPres);

				gasQ += step;
			}
		}

		/// <summary>
		/// Получение параметров сегментов затруба
		/// </summary>
		/// <param name="H">Глубина отсчитывается от устья(м)</param>
		/// <param name="P">Давление (МПа)</param>
		/// <param name="T">Температура (К)</param>
		public void GetSegmentParametersAnnularTubing(double dynamicH, out double[] H, out double[] P, out double[] T)
		{
			int num = AnnularTubing.NumberOfSegments + 2;
			P = new double[num];
			T = new double[num];
			H = new double[num];

			double stepH = AnnularTubing.Depth / (num - 2);
			double depth = 0;

			for (int i = 0; i < AnnularTubing.NumberOfSegments; i++)
			{
				H[i] = depth;
				P[i] = AnnularTubing.SegmentFlows[i].TopPressure;
				T[i] = AnnularTubing.SegmentFlows[i].TopTemperature;
				depth += stepH;
			}

			H[num - 2] = depth;
			P[num - 2] = AnnularTubing.SegmentFlows.Last().BottomPressure;
			T[num - 2] = AnnularTubing.SegmentFlows.Last().BottomTemperature;


			H[num - 1] = Tubing.Depth;
			P[num - 1] = AnnularTubing.SegmentFlows.Last().BottomPressure + WaterFluid.CalcColumnPressure(dynamicH);
			T[num - 1] = BottomholeTemperature;
		}

		/// <summary>
		/// Обводненная скважина
		/// </summary>
		/// <param name="layer">Пласт</param>
		/// <param name="gasFluid">Газовый флюид</param>
		/// <param name="tubing">Обсадная колонна и НКТ</param>
		/// <param name="wellheadPressure">Давление на устье (МПа)</param>
		/// <param name="wellheadTemperature">Температура на устье (К)</param>
		/// <param name="bottomholeTemperature">Температура на забое (К)</param>
		/// <param name="nglFluid">Флиюд газового конденсата</param>
		public WateredWell(Layer layer, GasFluid gasFluid, Tubing tubing, double wellheadPressure, double wellheadTemperature, double bottomholeTemperature, 
				NaturalGasLiquidsFluid nglFluid) : base(layer, gasFluid, tubing, wellheadPressure, wellheadTemperature, bottomholeTemperature, nglFluid)
        {}

        public WateredWell() { }
	}

}
