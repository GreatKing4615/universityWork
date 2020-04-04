using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrototypeDryWell.Components;
using PrototypeDryWell.Components.Fluids;

namespace PrototypeDryWell
{
	class Program
	{
		static void Main(string[] args)
		{
            Well well = new Well();
            try
            {
                well.InitTest();
            } catch (ArgumentException)
            {
                Console.WriteLine("Проблемы с составом тестового флюида");
            }
            Console.WriteLine("Q = " + well.Modeling(0.01));
            Console.ReadLine();
		}
	}
}
