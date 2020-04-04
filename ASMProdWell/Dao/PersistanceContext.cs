using ASMProdWell.Components;
using ASMProdWell.Components.Fluids;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMProdWell.Components.Equipment.Pumps;
using ASMProdWell.Security;

namespace ASMProdWell.Dao
{
    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    class PersistanceContext : DbContext
    {
        public PersistanceContext() : base("conn")
        {
            Database.SetInitializer<PersistanceContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EfficiencyCoefficient>();
            modelBuilder.Entity<PowerCoefficient>();
            modelBuilder.Entity<HeadCoefficient>();
            modelBuilder.Entity<TorqueCoefficient>();
            modelBuilder.Entity<RateCoefficient>();

            modelBuilder.Entity<ElectricSubmersiblePump>();
            modelBuilder.Entity<ProgressiveCavityPump>();
            //modelBuilder.Entity<ElectricSubmersiblePump>()
            //    .HasMany(p => p.EfficiencyCoefficients)
            //    .WithRequired(ec => ec.Pump)
            //    .HasForeignKey<int>(ec => ec.PumpId);
            //modelBuilder.Entity<ElectricSubmersiblePump>()
            //    .HasMany(p => p.PowerCoefficients)
            //    .WithRequired(ec => ec.Pump)
            //    .HasForeignKey<int>(ec => ec.PumpId);
            //modelBuilder.Entity<ElectricSubmersiblePump>()
            //    .HasMany(p => p.HeadCoefficients)
            //    .WithRequired(ec => ec.Pump)
            //    .HasForeignKey<int>(ec => ec.PumpId);
            base.OnModelCreating(modelBuilder);
            
        }
        public DbSet<Pump> Pumps { get; set; }

        public DbSet<ElectricSubmersiblePump> EspPumps { get; set;}

        public DbSet<ProgressiveCavityPump> PcpPumps { get; set; }

        public DbSet<DryWell> DryWells { get; set; }

        public DbSet<Layer> WateredLayers { get; set; }

        public DbSet<WateredWell> WateredWells { get; set; }

        public DbSet<GasFluid> GasFluids { get; set; }

        public DbSet<Coefficient> Coefficients { get; set; }

        public DbSet<User> Users { get; set; }

    }
}
