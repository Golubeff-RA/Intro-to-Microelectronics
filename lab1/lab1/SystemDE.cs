using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public Solution Solution { get { return solution_; } } 
        public Matrix<double> A { get { return A_; } }
        public Matrix<double> B { get { return B_; } }  
        public Matrix<double> C { get { return C_; } }
        public Matrix<double> D { get { return D_; } }
        public Vector<double> V { get { return V_; } }
        public Vector<double> X_start { get { return X_start_; } }
    }
}
