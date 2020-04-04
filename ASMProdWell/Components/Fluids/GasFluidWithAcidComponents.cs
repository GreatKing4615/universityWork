using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMProdWell.Components.Fluids
{
    [NotMapped]
    public sealed class GasFluidWithAcidComponents : GasFluid
    {
		/// <summary>
		/// Молярная доля углекислого газа
		/// </summary>
		private double CO2Fraction;

		/// <summary>
		/// Молярная доля сероводорода
		/// </summary>
        private double H2SFraction;

		/// <summary>
		/// Ошибка eps. Используется для вычисления.
		/// </summary>
        private double eps;

        public override double CriticalTemperature
        {
            get
            {
                if (_criticalTemperature == 0)
                {
                    eps = 14 * Math.Pow(H2SFraction, -0.36) * Math.Pow(CO2Fraction, 0.8) + 0.81 * Math.Pow(H2SFraction, 0.7);
                    _criticalTemperature =  base.CriticalTemperature - eps;
                }
                return _criticalTemperature;
            }
        }
		private double _criticalTemperature;


        public override double CriticalPressure
        {
            get
            {
                if (_criticalPressure == 0)
                {
                    _criticalPressure = base.CriticalPressure * CriticalTemperature / (base.CriticalTemperature + H2SFraction * ((1 - H2SFraction) * eps));
                }
                return _criticalPressure;
            }
        }
		private double _criticalPressure;


		public GasFluidWithAcidComponents(List<GasFluidComponent> components) : base(components)
		 {
            foreach (GasFluidComponent c in components)
            {
                if (c.Name == CompoundName.CO2)
                {
                    CO2Fraction = c.X;
                }
                if (c.Name == CompoundName.H2S)
                {
                    H2SFraction = c.X;
                }
            }

        }
	}
}
