using MathNet.Numerics.LinearAlgebra;

namespace lab1
{
    public class SystemDE
    {
        private readonly Matrix<double> A_, B_, C_, D_;
        private readonly Vector<double> V_, X_start_;
        private Solution solution_;

        public SystemDE(Matrix<double> A, Matrix<double> B, Matrix<double> C, Matrix<double> D, Vector<double> V, Vector<double> X_start)
        {
            // dim checking ^_^
            if (A.ColumnCount != A.RowCount) { throw new ArgumentException("A isn't square matrix"); }
            if (B.RowCount != A.RowCount) { throw new ArgumentException("B row count != A row count"); }
            if (C.ColumnCount != A.ColumnCount) { throw new ArgumentException("C col count != A col count"); }
            if (D.ColumnCount != B.ColumnCount) { throw new ArgumentException("D col count != B col count"); }
            if (V.Count != B.ColumnCount) { throw new ArgumentException("V size doesn't match B size"); }
            if (X_start.Count != A.ColumnCount) { throw new ArgumentException("X_start size doesn't match A size"); }
            A_ = A; B_ = B; C_ = C; D_ = D; V_ = V; X_start_ = X_start;
            solution_ = new Solution(A_.ColumnCount, C_.RowCount);
        }

        public SystemDE(double C, double L, double R1, double R2, Vector<double> J, Vector<double> X_start)
        {
            if (C <= 0) { throw new ArgumentException("C can't be <= 0"); }
            if (L <= 0) { throw new ArgumentException("L can't be <= 0"); }
            if (R2 <= 0) { throw new ArgumentException("R2 can't be <= 0"); }
            if (J.Count != 1) { throw new ArgumentException("V size doesn't match B size"); }
            if (X_start.Count != 2) { throw new ArgumentException("X_start size doesn't match A size"); }

            A_ = Matrix<double>.Build.DenseOfArray(new double[,] {
                    {-1 / (C * R2), -1 / C},
                    {1 / L, -R1 / L} });
            B_ = Matrix<double>.Build.DenseOfArray(new double[,] {
                    {1 / C},
                    {0 } });
            C_ = Matrix<double>.Build.DenseOfArray(new double[,] {
                    {1 / R2, 0},
                    {-1 / R2, -1}});
            D_ = Matrix<double>.Build.DenseOfArray(new double[,] {
                    { 0 },
                    { 1 } });
            V_ = J;
            X_start_ = X_start;
            solution_ = new Solution(A_.ColumnCount, C_.RowCount);
        }
        public Solution Solution { get { return solution_; } }
        public Matrix<double> A { get { return A_; } }
        public Matrix<double> B { get { return B_; } }
        public Matrix<double> C { get { return C_; } }
        public Matrix<double> D { get { return D_; } }
        public Vector<double> V { get { return V_; } }
        public Vector<double> X_start { get { return X_start_; } }
    }
}
