using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    public class PrettyPrinter
    {
        public static void Print(SystemDE system)
        {
            Console.WriteLine("Система дифференциальных уравнений ");
            Console.WriteLine("{ dX/dt = AX + BV");
            Console.WriteLine("{   Y   = CX + DV");
            Console.WriteLine("A = "); Console.WriteLine(system.A);
            Console.WriteLine("B = "); Console.WriteLine(system.B);
            Console.WriteLine("C = "); Console.WriteLine(system.C);
            Console.WriteLine("D = "); Console.WriteLine(system.D);
            Console.WriteLine("V = "); Console.WriteLine(system.V);
            Console.WriteLine("X_0 = "); Console.WriteLine(system.X_start);
            for (int i = 0; i < system.Solution.TimeHistory.Count; i++)
            {
                Console.WriteLine("=====================================");
                Console.Write("t = "); Console.WriteLine(system.Solution.TimeHistory[i]);
                Console.Write("X = "); Console.WriteLine(system.Solution.XHistory[i]);
                Console.Write("Y = "); Console.WriteLine(system.Solution.YHistory[i]);
            }
        }
    }
}
