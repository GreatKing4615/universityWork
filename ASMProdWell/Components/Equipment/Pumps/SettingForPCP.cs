using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ASMProdWell.Components.Equipment.Pumps
{
	/// <summary>
	/// Настройки для насоса ВШН
	/// </summary>
	[NotMapped]
	public sealed class SettingForPCP : ProgressiveCavityPump
	{
		/// <summary>
		/// Скорость вращения (об/мин)
		/// </summary>
		public double Speed {
			get
			{
				return _speed;
			}
			set
			{
				if (value < MinSpeed)
				{
					_speed = MinSpeed;
				}
				else if (value > MaxSpeed)
				{
					_speed = MaxSpeed;
				}
				else
					_speed = value;

				Rate = CalcRate(Head, Speed);
				Power = CalcPower(Head, Speed);
				Torque = CalcTorque(Head, Speed);
			}
		}
		private double _speed;

		/// <summary>
		/// Напор (м)
		/// </summary>
		public double Head {
			get {
				return _head;
			}
			set
			{
				if (CalcRate(value, Speed) >= 0)
					_head = value;
			}
		}
		private double _head;

		/// <summary>
		/// Расход при заданом напоре Head, скорости вращения Speed (м3/сут)
		/// </summary>
		public double Rate { get; private set; }

		/// <summary>
		/// Мощность при заданом напоре Head, скорости вращения Speed  (кВт)
		/// </summary>
		public double Power { get; private set; }

		/// <summary>
		/// Крутящий момент при заданом напоре Head, скорости вращения Speed  (килограммсилы-метр)
		/// </summary>
		public double Torque { get; private set; }


		/// <summary>
		/// Настройки для насоса ВШН
		/// </summary>
		/// <param name="pcp">ВШН</param>
		/// <param name="speed">Скорость вращения (об/мин)</param>
		/// <param name="H">Напор (м)</param>
		public SettingForPCP(ProgressiveCavityPump pcp, double speed, double H) : base(pcp)
		{
			Head = H;
			Speed = speed;
		}

		/// <summary>
		/// Настройки для насоса ВШН по умолчанию
		/// </summary>
		/// <param name="pcp">Насоса ВШН</param>
		public SettingForPCP(ProgressiveCavityPump pcp) : base(pcp)
		{
			Head = 0;
			Speed = BaseSpeed;
		}
	}
}
