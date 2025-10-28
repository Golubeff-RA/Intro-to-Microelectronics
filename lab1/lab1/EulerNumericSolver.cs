using MathNet.Numerics.LinearAlgebra;

namespace lab1
{
    public class EulerNumericSolver : INumericDESolver
    {
        public void NextStep(SystemDE system, double dt)
        {
            Vector<double> dXdt = system.A * system.Solution.XHistory.Last() + system.B * system.V;

            var new_X = system.Solution.XHistory.Last() + dt * dXdt;
            var new_Y = system.C * new_X + system.D * system.V;
            var last_t = system.Solution.TimeHistory.Last();

            system.Solution.AppendStep(new_X, new_Y, last_t + dt);
        }
    }
}

