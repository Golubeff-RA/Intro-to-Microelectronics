using lab1;
using MathNet.Numerics.LinearAlgebra;

var A = Matrix<double>.Build.Random(5, 5);
var B = Matrix<double>.Build.Random(5, 3);
var C = Matrix<double>.Build.Random(3, 5);
var D = Matrix<double>.Build.Random(3, 3);
var V = Vector<double>.Build.Random(3);
var X_start = Vector<double>.Build.Random(5);

var system = new SystemDE(A, B, C, D, V, X_start);

var X_step = Vector<double>.Build.Random(5);
var Y_step = Vector<double>.Build.Random(3);

Solver.Solve(system, new EulerNumericSolver(), 10.0, 0.1);
PrettyPrinter.Print(system);


PrettyPlotter.SavePlots(system, "X.png", "Y.png");