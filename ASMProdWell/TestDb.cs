using ASMProdWell.Components;
using ASMProdWell.Components.Equipment.Pumps;
using ASMProdWell.Components.Fluids;
using ASMProdWell.Dao;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMProdWell
{
    class TestDb
    {
        
        public static void Run()
        {
            

            using (PersistanceContext db = new PersistanceContext())
            {
                db.Database.CreateIfNotExists();
                Console.Write("GasFluids: ");

                db.GasFluids.Include("GasFluidComponents").Include("GasFluidComponents.ChemicalCompound").ToList().ForEach(delegate (GasFluid fluid)
                {
                    Console.WriteLine(fluid.Id + " " + fluid.GasFluidComponents[0].Name + " " + fluid.GasFluidComponents[0].X);
                });


                Console.WriteLine("Pumps: ");
                db.EspPumps.Include("PowerCoefficients").Include("EfficiencyCoefficients").Include("HeadCoefficients").ToList().ForEach(p =>
               {
                   Console.WriteLine("EsnPump " + p.Id);
                   Console.WriteLine("N:" + p.Name);
                   Console.WriteLine("Diameter: " + p.Diameter);
                   Console.WriteLine("CD: " + p.ConditionalDimension);
                   Console.WriteLine("BF: " + p.BaseFrequency);
                   Console.WriteLine("ND: " + p.NominalRate);
                   Console.Write("E: ");
                   foreach(EfficiencyCoefficient ec in p.EfficiencyCoefficients)
                   {
                       Console.Write(string.Format("{0} - {1};", ec.Order, ec.Value));
                   }
                   Console.WriteLine();
                   Console.Write("P: ");
                   foreach (PowerCoefficient pc in p.PowerCoefficients)
                   {
                       Console.Write(string.Format("{0} - {1};", pc.Order, pc.Value));
                   }
                   Console.WriteLine();
                   Console.Write("H: ");
                   foreach (HeadCoefficient ec in p.HeadCoefficients)
                   {
                       Console.Write(string.Format("{0} - {1};", ec.Order, ec.Value + 1));
                   }
                   Console.WriteLine();
               });
                db.PcpPumps.Include("PowerCoefficients").Include("RateCoefficients").Include("TorqueCoefficients").ToList().ForEach(p =>
                {
                    Console.WriteLine("PcpPump " + p.Id);
                    Console.WriteLine("N:" + p.Name);
                    Console.WriteLine("Diameter: " + p.Diameter);
                    Console.WriteLine("NS:" + p.BaseSpeed);
                    Console.Write("D: ");
                    foreach (RateCoefficient dc in p.RateCoefficients)
                    {
                        Console.Write(string.Format("{0} - {1};", dc.Order, dc.Value));
                    }
                    Console.WriteLine();
                    Console.Write("P: ");
                    foreach (PowerCoefficient pc in p.PowerCoefficients)
                    {
                        Console.Write(string.Format("{0} - {1};", pc.Order, pc.Value));
                    }
                    Console.WriteLine();
                    Console.Write("H: ");
                    foreach (TorqueCoefficient tc in p.TorqueCoefficients)
                    {
                        Console.Write(string.Format("{0} - {1};", tc.Order, tc.Value + 1));
                    }
                    Console.WriteLine();
                }
                );
            }
            //using (WateredWellContext db = new WateredWellContext())
            //{
            //    db.WateredWells.ToList().ForEach(delegate (WateredWell well)
            //    {
            //        Console.Write(well.Id);
            //    });
            //}
            //using (DryWellContext db = new DryWellContext())
            //{
            //    DryLayerContext db1 = new DryLayerContext();
            //    db1.Database.CreateIfNotExists();

            //    db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
            //    db1.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);


            //    db.Database.CreateIfNotExists();
            //    DryWell dryWell = new DryWell();
            //    dryWell.InitTest(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            //    db.dryWells.Add(dryWell);
            //    db.SaveChanges();
            //    db.dryWells.Include("Layer").ToList().ForEach(delegate (DryWell well)
            //    {
            //        Console.WriteLine(well.Id + " " + well.Layer.Id);
            //    });

            //}

            //using (DryLayerContext db = new DryLayerContext())
            //{
            //    db.Database.CreateIfNotExists();
            //    db.dryLayers.ToList().ForEach(delegate (DryLayer layer)
            //    {
            //        Console.Write(layer.Id);
            //    });
            //}

        }
    }

}
