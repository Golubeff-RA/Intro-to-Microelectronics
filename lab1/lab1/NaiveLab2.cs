using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    public class NaiveLab2
    {
        private static readonly string[] names = {"R", "r_si", "Cn", "Csi", "Czi", "Czs",
            "u_zi", "Up", "Su_zi"};
        private static Dictionary<string, double> val_dict = new Dictionary<string, double>();
        private static void InitializeNameValDict(ElectricScheme scheme)
        {
            val_dict.Add("R", scheme.resistors[0].resistance);
            if (scheme.resistors[0].unique_id != 0)
                throw new Exception("");
            val_dict.Add("r_si", scheme.resistors[1].resistance);
            if (scheme.voltage_sources[1].unique_id != 6)
                throw new Exception("");
            val_dict.Add("u_zi", scheme.voltage_sources[1].voltage);
            val_dict.Add("Up", scheme.voltage_sources[0].voltage);
            val_dict.Add("Su_zi", scheme.current_sources[0].current);
        }
        public static Tuple<Matrix<double>, Matrix<double>, Matrix<double>, Matrix<double>> NaiveSystemCalc(ElectricScheme scheme)
        {
            var A = Matrix<double>.Build.Dense(1, 1);
            var B = Matrix<double>.Build.Dense(1, 3);
            var C = Matrix<double>.Build.Dense(1, 1);
            var D = Matrix<double>.Build.Dense(1, 3);
            InitializeNameValDict(scheme);
            A[0, 0] = -(1 / val_dict["R"] + 1 / val_dict["r_si"]);
            C[0, 0] = 1;
            B[0, 1] = 1 / val_dict["R"]; B[0, 2] = 1;
            return new Tuple<Matrix<double>, Matrix<double>, Matrix<double>, Matrix<double>>(A, B, C, D);
        }
    }
}
