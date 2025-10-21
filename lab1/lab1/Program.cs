using HarfBuzzSharp;
using lab1;
using MathNet.Numerics.LinearAlgebra;


double C = 0.5;
double L = 1;
double R1 = 5;
double R2 = 7;

var X_start = Vector<double>.Build.DenseOfArray(new double[] { 0, 0 });
var J = Vector<double>.Build.DenseOfArray(new double[] { 1 });

var system_de = new SystemDE(C, L, R1, R2, J, X_start);
Solver.Solve(system_de, new EulerNumericSolver(), 10, 0.01);

string plots_path = "../../../../../lab1_plots";

if (!Directory.Exists(plots_path))
{
    Directory.CreateDirectory(plots_path);
    Console.WriteLine($"Папка создана: {plots_path}");
}
string[] x_annotations = ["U(c)", "I(L)" ];
string[] y_annotations = ["I(2)", "I(3)" ];
string x_file = Path.Combine(plots_path, $"X_at_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.png");
string y_file = Path.Combine(plots_path, $"Y_at_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.png");

PrettyPlotter.SavePlots(system_de, x_file, y_file, x_annotations, y_annotations);