using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMProdWell.Components.Pipes;

namespace ASMProdWell.Utils
{
	public class TubingBuilder
	{
		public static TubingBuilder One;

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
		public double PipeWallThickness { get; set; }

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
		/// Количество секций НКТ (безразмерная)
		/// </summary>
		public int NumberOfSegments { get; private set; }


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
		/// Коэффициент падения дебита из-за скопления воды A
		/// </summary>
		public double CoeffGasRateFallA;


		public Tubing BuildTubing()
		{
			return new Tubing(PipeDiameter, PipeWallThickness, PipeRoughness, Length, Depth, TubingDiameter, NumberOfSegments,
								CoeffWaterAccumulationA, CoeffWaterAccumulationB, CoeffWaterAccumulationC, CoeffGasRateFallA);
		}

		/// <summary>
		/// Построитель класса Tubing
		/// </summary>
		/// <param name="pipeDiameter">Диаметр трубы НКТ (м)</param>
		/// <param name="pipeRoughness">Абсолютная шероховатость НКТ (м)</param>
		/// <param name="length">Длина НКТ (м)</param>
		/// <param name="depth">Глубина НКТ (м)</param>
		/// <param name="numberSegments">Количество секций НКТ (безразмерная)</param>
		public TubingBuilder(double pipeDiameter, double pipeWallThickness, double pipeRoughness, double length,
								double depth, double tubingDiameter, int numberSegments, double wa = 0, double wb = 0, double wc = 0, double grfa = 0)
		{
			PipeDiameter = pipeDiameter;
			PipeWallThickness = pipeWallThickness;
			PipeRoughness = pipeRoughness;
			Length = length;
			Depth = depth;
			TubingDiameter = tubingDiameter;
			NumberOfSegments = numberSegments;

			CoeffWaterAccumulationA = wa;
			CoeffWaterAccumulationB = wb;
			CoeffWaterAccumulationC = wc;
			CoeffGasRateFallA = grfa;
		}

	}
}
