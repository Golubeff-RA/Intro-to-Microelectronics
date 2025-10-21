namespace lab1
{
    public class Solver
    {
        public static void Solve(SystemDE system, INumericDESolver num_solver, double t_end, double dt)
        {
            int steps = (int)Math.Ceiling(t_end / dt);
            system.Solution.AppendStep(system.X_start,
                system.C * system.X_start + system.D * system.V, 0);
            for (int i = 0; i < steps; i++)
                num_solver.NextStep(system, dt);
        }
    }
}
