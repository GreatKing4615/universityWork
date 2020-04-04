using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMProdWell.Components.Equipment.PlungerLift
{
	public class Plunger
	{
		/// <summary>
		/// Давление необходимое для поднятия одной барали (МПа)
		/// </summary>
		public double PressForOneBbl;

		/// <summary>
		/// Давление необходимое для поднятия плунжера (МПа)
		/// </summary>
		public double PressForPlunger;

		/// <summary>
		/// Коэффициент трения газа под плунжером (безразмерная)
		/// </summary>
		public double FrictionUnderPlungerCoeff;

		/// <summary>
		/// Средняя скорость плунжера (м/мин)
		/// </summary>
		public double Speed;


		/// <summary>
		/// Вычисление минимального давление около устья в обсадной колонне в момент, 
		/// когда плунжер и жидкостная пробка приходят к устью (МПа)
		/// Lea Gas Well Deliquification page 551
		/// </summary>
		/// <param name="wellheadPressure">Устьевое давление (МПа)</param>
		/// <param name="lenghtPath">Длина пути плунжера (м)</param>
		/// <param name="waterVolume">Объем водяной пробки (м3)</param>
		/// <returns></returns>
		public double CalcMinCasingPressure(double wellheadPressure, double lenghtPath, double waterVolume)
		{
			double P_plunger = PressForPlunger * 145.03773773000646; // 5 psi на подъём плунжера
			double P_bbl = PressForOneBbl * 145.03773773000646;  // для tubing size = 2.875 inch давления требуемое для одной барели 102 psi 
			double K = FrictionUnderPlungerCoeff; // для tubing size = 2.875 inch K = 45000 
			double Vw = waterVolume * 6.289822438312567; // С коэффициентов для перевода в барелли
			double Pwh = wellheadPressure * 130.34178860721815;// С коэффициентов для перевода в psig
			double L = lenghtPath / 0.3048;// С коэффициентов для перевода в ft

			double Pcmin = (14.7 + P_plunger + Pwh + P_bbl * Vw) * (1 + L / K);
			Pcmin = Pcmin / 145.03773773000646; // переводим psi в МПа
			return Pcmin;
			/// Вычисление минимального давление около устья в обсадной колонне в момент, 
			/// когда плунжер и жидкостная пробка приходят к устью (МПа)
		}

		/// <summary>
		/// Вычисление давление в обсадной колонне около устья в момент, 
		/// когда плунжер на забое (МПа)
		/// Lea Gas Well Deliquification page 553
		/// </summary>
		/// <param name="wellheadPressure">Устьевое давление (МПа)</param>
		/// <param name="lenghtPath">Длина пути плунжера (м)</param>
		/// <param name="waterVolume">Объем водяной пробки (м3)</param>
		/// <param name="annularSquare">Поперечное сечение затрубного пространства (м2)</param>
		/// <param name="pipeSquare">Поперечное сечение трубного пространства (м2)</param>
		/// <returns></returns>
		public double CalcMaxCasingPressure(double wellheadPressure, double lenghtPath, 
							double waterVolume, double annularSquare, double pipeSquare)
		{
			double Pcmin = CalcMinCasingPressure(wellheadPressure, lenghtPath, waterVolume);
			double Sa = annularSquare;
			double Sp = pipeSquare;
			double Pcmax = Pcmin * (Sa + Sp) / Sa;
			return Pcmax;
		}


		/// <summary>
		/// Плунжер
		/// </summary>
		/// <param name="bressForOneBbl">Давление необходимое для поднятия одной барали (МПа)</param>
		/// <param name="pressForPlunger">Давление необходимое для поднятия плунжера (МПа)</param>
		/// <param name="frictionUnderPlungerCoeff">Коэффициент трения газа под плунжером</param>
		/// <param name="speed">Средняя скорость плунжера (м/мин)</param>
		public Plunger(double bressForOneBbl, double pressForPlunger, 
			double frictionUnderPlungerCoeff, double speed)
		{
			PressForOneBbl = bressForOneBbl;
			PressForPlunger = pressForPlunger;
			FrictionUnderPlungerCoeff = frictionUnderPlungerCoeff;
			Speed = speed;
		}

	}
}
