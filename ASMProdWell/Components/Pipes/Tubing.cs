using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMProdWell.Components;
using ASMProdWell.Components.Fluids;
using System.ComponentModel.DataAnnotations.Schema;


namespace ASMProdWell.Components.Pipes
{
    [Table("Tubing")]
    public class Tubing
    {

        public int Id { get; set; }

        /// <summary>
        /// Набор параметров потока в местах соединения секторов НКТ
        /// </summary>
        [NotMapped]
        public List<GasFlow> SegmentFlows;

        [NotMapped]
        public Pipe Pipe { get; set; }

		/// <summary>
		/// Внутренний диаметр эксплуатационной колонны (м)
		/// </summary>
		public double TubingDiameter { get; set; }

        /// <summary>
        /// Внутренний диаметр НКТ (м)
        /// </summary>
        public double PipeDiameter { get; set; }

		/// <summary>
		/// Толщина стенки НКТ (м)
		/// </summary>
		public double PipeWallThickness { get; set;	}

        /// <summary>
        /// Абсолютная шероховатость трубы (м)
        /// </summary>
        public double PipeRoughness { get; set; }

        /// <summary>
        /// Длина НКТ (м)
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// Глубина НКТ (м)
        /// </summary>
        public double Depth { get; set; }

		/// <summary>
		/// Площаль поперечного сечения трубного пространства (м2)
		/// </summary>
		public double CrossSectionArea {
			get {
				double D = PipeDiameter;
				return D * D * Math.PI / 4;
			}
		}

		/// <summary>
		/// Площаль поперечного затрубного пространства сечения (м2)
		/// </summary>
		public double AnnularCrossSectionArea
		{
			get
			{
				double Dp = PipeDiameter;
				double Dt = TubingDiameter;
				double Sp = Dp * Dp * Math.PI / 4;
				double Sa = Dt * Dt * Math.PI / 4;
				return Sa - Sp;
			}
		}

		/// <summary>
		/// Количество секций НКТ (безразмерная)
		/// </summary>
		[NotMapped]
        public int NumberOfSegments { get; private set; }


		/// <summary>
		/// Коэффициент скопления воды A
		/// </summary>
		[NotMapped]
		public double CoeffWaterAccumulationA;

		/// <summary>
		/// Коэффициент скопления воды B
		/// </summary>
		[NotMapped]
		public double CoeffWaterAccumulationB;

		/// <summary>
		/// Коэффициент скопления воды C
		/// </summary>
		[NotMapped]
		public double CoeffWaterAccumulationC;

		/// <summary>
		/// Коэффициент падения дебита из-за скопления воды A
		/// </summary>
		[NotMapped]
		public double CoeffGasRateFallingA;

		/// <summary>
		/// Вычисления времени скопления воды по кривой скопления воды на забое, зная объем скопившийся воды (сут)
		/// </summary>
		/// <param name="Q">Дебит жидкости (м3/сут)</param>
		/// <param name="V">Объем жидкости притекший за цикл (м3)</param>
		/// <param name="time">Время добычи (сут)</param>
		/// <param name="dt">Шаг времени (сут)</param>
		/// <returns></returns>
		public double CalcTimeForWaterAccumulationOnBottomhole(double Q, double V, out List<double> waterRates)
		{
			double a = CoeffWaterAccumulationA;
			double b = CoeffWaterAccumulationB;
			double c = CoeffWaterAccumulationC;
			waterRates = new List<double>();
			double time = 0;
			double dt = 0.0001;
			double Vtmp = 0, baseQ;
			int i = 0;
			while (Vtmp < V)
			{
				time += dt;
				baseQ = b + a * Math.Log(time + c);
				Vtmp += dt * baseQ * Q;

				if (i % 10 == 0)
					waterRates.Add(baseQ * Q);
				i++;
			}

			return time;
		}

		/// <summary>
		/// Вычисления объема скопившейся воды забое по кривой скопления воды на забое зная время (м3)
		/// </summary>
		/// <param name="Q">Дебит жидкости (м3/сут)</param>
		/// <param name="time">Время добычи (сут)</param>
		/// <param name="V">Объем жидкости притекший за цикл (м3)</param>
		/// <returns></returns>
		public double CalcWaterVolumeAccumulatedOnBottomhole(double Q, double time, out List<double> waterRates)
		{
			double a = CoeffWaterAccumulationA;
			double b = CoeffWaterAccumulationB;
			double c = CoeffWaterAccumulationC;
			waterRates = new List<double>();
			double V = 0;
			double dt = 0.0001;
			double Ttmp = 0, watR;
			int i = 0;
			while (Ttmp < time)
			{
				Ttmp += dt;
				watR = b + a * Math.Log(Ttmp + c);
				V += dt * watR * Q;

				if (i % 10 == 0)
					waterRates.Add(watR * Q);
				i++;
			}

			return V;
		}


		/// <summary>
		/// Вычисления объема газа при падения дебита газа от скопления жидкости на забое по кривой (тыс.куб.м)
		/// </summary>
		/// <param name="Q_start">Дебит начала добычи (тыс.м3/сут) </param>
		/// <param name="a">Коэффициент a кривой падения дебита из-за скопления жидкости</param>
		/// <param name="time">Время добычи (сут)</param>
		/// <param name="numPoints">Кол-во точек</param>
		/// <param name="dt">Шаг времени (сут)</param>
		/// <returns></returns>
		public double CalcGasVolumeWithGasRateFalling(double Q_start, double Q_close, double time, out List<double> gasRates)
		{
			double a = CoeffGasRateFallingA;
			double gasVolumeForCycle = 0;
			gasRates = new List<double>();
			double dt = 0.0001;
			double Ttmp = 0;
			int i = 0;
			while (Ttmp < time)
			{
				Ttmp += dt;
				double gasR = Math.Exp(a * Ttmp) * (Q_start - Q_close) + Q_close;
				gasVolumeForCycle += gasR * dt;

				if (i % 10 == 0)
					gasRates.Add(gasR);
				i++;
			}
			return gasVolumeForCycle;
		}


		/// <summary>
		/// Вычисление расхода смеси (тыс.м3/сут)
		/// при стандартных условиях T=293K P=0,101325МПа
		/// </summary>
		/// <param name="gasFlow">Газовый поток</param>
		/// <param name="liquidFlow">Жидкостной поток</param>
		/// <returns></returns>
		public static double CalcMixtureRate(GasFlow gasFlow, LiquidFlow liquidFlow)
		{
			double gasQ = gasFlow.RateAtStandardConditions;
			//Расход жидкости (тыс.куб.м/сут)
			double liquidQ = liquidFlow.Rate * 0.001;
			return gasQ + liquidQ;
		}


		/// <summary>
		/// Вычисление параметра, связанного с истинным газосодержанием потока
		/// Гриценко стр. 140
		/// </summary>
		/// <param name="gasFlow">Газовый поток</param>
		/// <param name="liquidRate">Расход жидкости (м3/сут)</param>
		/// <param name="liquidDensity">Плотность жидкости (кг/м3)</param>
		/// <returns>Значение параметра</returns>
		public static double CalcGasContentParameter(GasFlow gasFlow, double liquidRate, double liquidDensity)
		{
			double RHOgop = gasFlow.CalcDensityAtOperatingConditions();
			double RHOl = liquidDensity;
			double phi = CalcTrueFlowGasContent(gasFlow, liquidRate);
			double gasContentParameter = phi + (1 - phi) * RHOl / RHOgop;
			return gasContentParameter;
		}


		/// <summary>
		/// Вычисление истинного газосодержания потока 
		/// Гриценка стр. 140
		/// </summary>
		/// <param name="flow">Газовый поток</param>
		/// <param name="liquidRate">Расход жидкости (м3/сут)</param>
		/// <returns></returns>
		public static double CalcTrueFlowGasContent(GasFlow flow, double liquidRate)
        {
            double Qgop = flow.CalcRateAtOperatingConditions();
			//Расход жидкости (тыс.куб.м/сут)
			double Ql = liquidRate * 0.001;
            double phi = Qgop / (Ql + Qgop);
            return phi;
        }

		/// <summary>
		/// Получить эквивалентный диаметр затрубного пространства (м)
		/// </summary>
		/// <returns>Эквивалентный диаметр затрубного пространства (м)</returns>
		public double CalcEquivalentAnnularDiameter()
		{
			double D = TubingDiameter;
			double d = PipeDiameter + PipeWallThickness;
			return Math.Sqrt(D * D - d * d);
		}


		/// <summary>
		/// Вычисление забойного давления НКТ остановленной скважины по барометрической формуле (МПа)
		/// Гриценко стр. 117 и 113 (для сухой скважины)
		/// </summary>
		/// <param name="topFlow">Поток в самом верхнем сегменте НКТ</param>
		/// <param name="bottomholeTemperature">Температура на забое НКТ (K)</param>
		/// <returns>Давление на забое НКТ (МПа)</returns>
		public double CalcStaticBottomholePressure(GasFlow topFlow, double bottomholeTemperature)
		{
			SegmentFlows.Clear();

			double length = Length / NumberOfSegments;
			double depth = length;
			Pipe pipe = new Pipe(PipeDiameter, length, depth, PipeRoughness);

			double dT = (bottomholeTemperature - topFlow.TopTemperature) / NumberOfSegments;
			GasFlow gasFlow = topFlow;
			for (int i = 0; i < NumberOfSegments; i++)
			{
				double btmTemp = gasFlow.TopTemperature + dT;
                pipe.CalcStaticBottomPressure(gasFlow, btmTemp);

                SegmentFlows.Add(gasFlow);

				gasFlow = gasFlow.GenerateBottomFlow();
			}

			return SegmentFlows.Last().BottomPressure;
		}


		/// <summary>
		/// Вычисление устьевого давления НКТ остановленной скважины по барометрической формуле (МПа)
		/// Гриценко стр. 117 и 113 (для сухой скважины)
		/// </summary>
		/// <param name="bottomFlow">Поток в самом нижнем сегменте НКТ</param>
		/// <param name="wellheadTemperature">Температура на забое НКТ (K)</param>
		/// <returns>Давление на забое НКТ (МПа)</returns>
		public double CalcStaticWellheadPressure(GasFlow bottomFlow, double wellheadTemperature)
		{
			SegmentFlows.Clear();

			double length = Length / NumberOfSegments;
			double depth = length;
			Pipe pipe = new Pipe(PipeDiameter, length, depth, PipeRoughness);

			double dT = (wellheadTemperature - bottomFlow.BottomTemperature) / NumberOfSegments;
			GasFlow gasFlow = bottomFlow;
			for (int i = 0; i < NumberOfSegments; i++)
			{
				double topTemp = gasFlow.BottomTemperature + dT;
				pipe.CalcStaticTopPressure(gasFlow, topTemp);

				SegmentFlows.Add(gasFlow);

				gasFlow = gasFlow.GenerateTopFlow();
			}

			return SegmentFlows.Last().TopPressure;
		}


		/// <summary>
		/// Вычисление забойного давления НКТ в обводненной скважине по барометрической формуле (МПа)
		/// Гриценко стр. 113, 117, 140
		/// </summary>
		/// <param name="topFlow">Поток, получаемый на устье</param>
		/// <param name="liquidFlow">Жидкостной поток</param>
		/// <param name="bottomholeTemperature">Забойная температура (К)</param>
		/// <returns></returns>
		public double CalcBottomholePressure(GasFlow topFlow, LiquidFlow liquidFlow, double bottomholeTemperature)
		{
			SegmentFlows.Clear();

			double length = Length / NumberOfSegments;
			double depth = length;
			double dT = (bottomholeTemperature - topFlow.TopTemperature) / NumberOfSegments;

			WaterPipe pipe = new WaterPipe(PipeDiameter, length, depth, PipeRoughness);
			GasFlow gasFlow = topFlow;

			for (int i = 0; i < NumberOfSegments; i++)
			{
				double btmTemp = gasFlow.TopTemperature + dT;
				pipe.CalcBottomPressure(gasFlow, liquidFlow, btmTemp);


                SegmentFlows.Add(gasFlow);
				gasFlow = gasFlow.GenerateBottomFlow();
			}

			return SegmentFlows.Last().BottomPressure;
		}


		/// <summary>
		/// Вычисление забойного давления НКТ по барометрической формуле (МПа)
		/// Гриценко стр. 117 и 113 (для сухой скважины)
		/// </summary>
		/// <param name="topFlow">Поток в самом верхнем сегменте НКТ</param>
		/// <param name="bottomholeTemperature">Температура на забое НКТ (K)</param>
		/// <returns>Давление на забое НКТ (МПа)</returns>
		public double CalcBottomholePressure(GasFlow topFlow, double bottomholeTemperature)
		{
            SegmentFlows.Clear();

            double length = Length / NumberOfSegments;
			double depth = length;
			Pipe pipe = new Pipe(PipeDiameter, length, depth, PipeRoughness);

			double dT = (bottomholeTemperature - topFlow.TopTemperature) / NumberOfSegments;
            GasFlow gasFlow = topFlow;
			for (int i = 0; i < NumberOfSegments; i++)
			{
                double btmTemp = gasFlow.TopTemperature + dT;
                pipe.CalcPipePressure(gasFlow, btmTemp);

                SegmentFlows.Add(gasFlow);

				gasFlow = gasFlow.GenerateBottomFlow();
			}

			return SegmentFlows.Last().BottomPressure;
		}

		/// <summary>
		/// Конструктор копирования класса Tubing
		/// </summary>
		public Tubing(Tubing tubing)
		{
			SegmentFlows = new List<GasFlow>();
			PipeDiameter = tubing.PipeDiameter;
			PipeWallThickness = tubing.PipeWallThickness;
			PipeRoughness = tubing.PipeRoughness;
			Length = tubing.Length;
			Depth = tubing.Depth;
			TubingDiameter = tubing.TubingDiameter;
			NumberOfSegments = tubing.NumberOfSegments;

			CoeffWaterAccumulationA = tubing.CoeffWaterAccumulationA;
			CoeffWaterAccumulationB = tubing.CoeffWaterAccumulationB;
			CoeffWaterAccumulationC = tubing.CoeffWaterAccumulationC;
			CoeffGasRateFallingA = tubing.CoeffGasRateFallingA;
		}

        /// <summary>
        /// НКТ
        /// </summary>
        /// <param name="pipeDiameter">Диаметр трубы НКТ (м)</param>
        /// <param name="pipeRoughness">Абсолютная шероховатость НКТ (м)</param>
        /// <param name="length">Длина НКТ (м)</param>
        /// <param name="depth">Глубина НКТ (м)</param>
        /// <param name="numberSegments">Количество секций НКТ (безразмерная)</param>
        public Tubing(double pipeDiameter, double pipeWallThickness, double pipeRoughness, double length, double depth, double tubingDiameter, 
						int numberSegments, double wa = 0, double wb = 0, double wc = 0, double grfa = 0)
        {
			SegmentFlows = new List<GasFlow>();
            PipeDiameter = pipeDiameter;
			PipeWallThickness = pipeWallThickness;
			PipeRoughness = pipeRoughness;
            Length = length;
            Depth = depth;
			TubingDiameter = tubingDiameter;
            if (numberSegments < 1 || numberSegments > 20)
                throw new ArgumentOutOfRangeException("Количество сегментов моделирования НКТ должно быть не более 20 и не менее 1.");
            NumberOfSegments = numberSegments;

			CoeffWaterAccumulationA = wa;
			CoeffWaterAccumulationB = wb;
			CoeffWaterAccumulationC = wc;
			CoeffGasRateFallingA = grfa;
		}

        public Tubing()
        {
        }
    }
}
