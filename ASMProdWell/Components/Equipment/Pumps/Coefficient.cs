using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMProdWell.Components.Equipment.Pumps
{
    [Table("Coefficient")]
    public class Coefficient
    {
        public int Id { get; set; }

        public int Order { get; set; }

        [NotMapped]
        public double Value {
            get
            {
                return double.Parse(string_value);
            }
            set
            {
                string_value = value.ToString();
            }
        }

        [Column("Value")]
        public string string_value {get; set; } //костыль

        public int PumpId { get; set; }

        public Coefficient() {}

        public Coefficient(int order, double value)
        { 
            Order = order;
            Value = value;
        }
    }
    public class EfficiencyCoefficient : Coefficient
    {
        public EfficiencyCoefficient() : base() { }
        public EfficiencyCoefficient(int order, double value) : base(order, value) { }

    }

    public class HeadCoefficient : Coefficient
    {
        public HeadCoefficient() : base() { }
        public HeadCoefficient(int order, double value) : base(order, value) { }
    }

	public class RateCoefficient : Coefficient
	{
		public RateCoefficient() : base() { }
		public RateCoefficient(int order, double value) : base(order, value) { }
	}

	public class TorqueCoefficient : Coefficient
	{
		public TorqueCoefficient() : base() { }
		public TorqueCoefficient(int order, double value) : base(order, value) { }
	}
	
		

	public class PowerCoefficient : Coefficient
    {
        public PowerCoefficient() : base() { }
        public PowerCoefficient(int order, double value) : base(order, value) { }
    }
}
