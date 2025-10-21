namespace lab1
{
    public interface INumericDESolver
    {
        public abstract void NextStep(SystemDE system, double dt);
    }
}
