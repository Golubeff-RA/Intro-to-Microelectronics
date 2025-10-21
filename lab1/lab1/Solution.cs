using MathNet.Numerics.LinearAlgebra;

namespace lab1
{
    public class Solution
    {
        private readonly int size_x_, size_y_;
        private List<Vector<double>> x_history_;
        private List<Vector<double>> y_history_;
        private List<double> time_history_;

        public Solution(List<Vector<double>> x_history, List<Vector<double>> y_history, List<double> time_history_)
        {
            this.size_x_ = x_history[0].Count;
            this.size_y_ = y_history[0].Count;
            this.x_history_ = x_history;
            this.y_history_ = y_history;
            this.time_history_ = time_history_;
        }

        public Solution(int size_x = 0, int size_y = 0)
        {
            this.size_x_ = size_x > 0 ? size_x : 0;
            this.size_y_ = size_y > 0 ? size_y : 0;
            this.x_history_ = new List<Vector<double>>();
            this.y_history_ = new List<Vector<double>>();
            this.time_history_ = new List<double>();
        }

        public bool AppendStep(Vector<double> new_x, Vector<double> new_y, double timestamp)
        {
            if (new_x.Count != size_x_ || new_y.Count != size_y_) { return false; }
            x_history_.Add(new_x);
            y_history_.Add(new_y);
            time_history_.Add(timestamp);
            return true;
        }
        public List<Vector<double>> XHistory { get { return x_history_; } }
        public List<Vector<double>> YHistory { get { return y_history_; } }
        public List<double> TimeHistory { get { return time_history_; } }
        public int SizeX { get { return size_x_; } }
        public int SizeY { get { return size_y_; } }
    }
}
