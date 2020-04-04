using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMProdWell.Components.Fluids;

namespace ASMProdWell.Components.Equipment.Pumps
{
    [Table("Pump")]
    public abstract class Pump
    {
        public int Id { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Внешний диаметр насоса (мм)
        /// </summary>
        public double Diameter { get; set; }


        /// <summary>
        /// Номинальная подача (куб.м/сут)
        /// </summary>
        public double NominalRate { get; set; }


        /// <summary>
        /// Коэффициенты полинома характеризующего мощность от напора (индексы соответствуют степени)
        /// </summary>
        [ForeignKey("PumpId")]
        public List<PowerCoefficient> PowerCoefficients { get; set; }

		/// <summary>
		/// Вычисление необходимого напора (м)
		/// </summary>
		/// <param name="bottomholePressure">Забойное давление (МПа)</param>
		/// <param name="wellheadPressure">Устьевое давление (МПа)</param>
		/// <param name="depthWell">Глубина скважины (м)</param>
		/// <param name="waterFluid">Флюид пластовой воды</param>
		/// <returns></returns>
		public static double CalcNeededHeadHeight( double bottomholePressure, double wellheadPressure, double depthWell, WaterFluid waterFluid)
		{
			//Флюид дистилиованной воды
			WaterFluid distilledWater = new WaterFluid(PhysicalConstants.DistilledWaterDensity);
			double Pb = bottomholePressure;
			double Pwh = wellheadPressure;
			double d = depthWell;
			double Pc = waterFluid.CalcColumnPressure(depthWell); // Давление создаваемое столбом дистилированной воды
			double Ptmp = Pc - Pb + Pwh;
			double Hdw = distilledWater.CalcColumnHeight(Ptmp); //Высота столба жидкости, создаваемого такое давление
			return Hdw;
		}
    }
}
