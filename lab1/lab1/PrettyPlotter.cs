using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    public class PrettyPlotter
    {
        public static void SavePlots(SystemDE system, string for_x_ses, string for_y_ses)
        {
            if (for_x_ses != null)
            {
                var plt = new Plot();
                plt.Title("X[i](t)");
                plt.XLabel("t, c");
                plt.YLabel("X[i]");

                double[] time = system.Solution.TimeHistory.ToArray();

                int state_count = system.Solution.SizeX;
                for (int i = 0; i < state_count; i++)
                {
                    double[] state = system.Solution.XHistory.Select(x => x[i]).ToArray();
                    var scat = plt.Add.Scatter(time, state);
                    scat.LegendText = $"Состояние X{i + 1}";
                }

                plt.ShowLegend(Alignment.UpperRight);
                plt.Legend.Orientation = Orientation.Horizontal;
                plt.SavePng(for_x_ses, 1200, 800);
            }

            if (for_y_ses != null)
            {
                var plt = new Plot();
                plt.Title("Y[i](t)");
                plt.XLabel("t, c");
                plt.YLabel("Y[i]");

                double[] time = system.Solution.TimeHistory.ToArray();

                int state_count = system.Solution.SizeY;
                for (int i = 0; i < state_count; i++)
                {
                    double[] state = system.Solution.YHistory.Select(x => x[i]).ToArray();
                    var scat = plt.Add.Scatter(time, state);
                    scat.LegendText = $"Выход Y{i + 1}";
                }

                plt.ShowLegend(Alignment.UpperRight);
                plt.Legend.Orientation = Orientation.Horizontal;
                plt.SavePng(for_y_ses, 1200, 800);
            }
        }
    }
}
