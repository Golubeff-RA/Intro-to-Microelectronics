using MathNet.Numerics.LinearAlgebra;

namespace lab1
{
    public class NaiveLab3
    {
        private static readonly Dictionary<string, int> name_id_dict = new Dictionary<string, int>()
            {{"C_si1", 0}, {"C_si2", 1}, {"C_si3", 2}, {"C_zi1", 3}, {"C_zi2", 4}, {"C_zi3", 5}, 
            {"C_zs1", 6}, {"C_zs2", 7}, {"C_n", 8}, {"u_zi1", 9}, {"u_zi2", 10}, {"U_p", 11}, {"S*u_zi1", 12},
            { "S*u_zi2", 13}, {"S*u_Czi3", 14} };
        private static Dictionary<string, double> val_dict = new Dictionary<string, double>();
        private static void InitializeNameValDict(ElectricScheme scheme)
        {
            List<double> values = new List<double>();
            foreach (var cap in scheme.capacitors)
                values.Add(cap.capacity);
            foreach (var volt in scheme.voltage_sources)
                values.Add(volt.voltage);
            foreach (var curr in scheme.current_sources)
                values.Add(curr.current);

            int counter = 0;
            foreach (var pair in name_id_dict)
            {
                val_dict.Add(pair.Key, values[counter]);
                counter++;
            }

            val_dict.Add("S", 1);
        }
        public static Tuple<Matrix<double>, Matrix<double>, Matrix<double>, Matrix<double>> NaiveSystemCalc(ElectricScheme scheme)
        {
            var A = Matrix<double>.Build.Dense(2, 2);
            var B = Matrix<double>.Build.Dense(2, 6);
            var C = Matrix<double>.Build.Dense(1, 2);
            var D = Matrix<double>.Build.Dense(1, 6);
            InitializeNameValDict(scheme);
            double c1 = val_dict["S"] / (3 * val_dict["C_si1"] + 2 * val_dict["C_zi1"] + 2 * val_dict["C_n"]);
            double c2 = val_dict["S"] * (val_dict["C_si1"] + val_dict["C_zi1"] + val_dict["C_n"]) /
                    (val_dict["C_si1"] * (3 * val_dict["C_si1"] + 2 * val_dict["C_zi1"] + 2 * val_dict["C_n"]));
            double c3 = val_dict["S"] * (2 * val_dict["C_si1"] + val_dict["C_zi1"] + val_dict["C_n"]) /
                    (val_dict["C_si1"] * (3 * val_dict["C_si1"] + 2 * val_dict["C_zi1"] + 2 * val_dict["C_n"]));

            double c4 = val_dict["S"] / val_dict["C_si1"];

            Console.WriteLine($"{c1.ToString()}, {c2.ToString()}, {c3.ToString()}, {c4.ToString()}");
            A[0, 0] = -c1; A[0, 1] = -c1;
            A[1, 0] = -c1; A[1, 1] = -c1;

            B[0, 0] = c2 - c4; B[0, 1] = -c3 + c4; B[0, 2] = -c1;
            B[1, 0] = c2; B[1, 1] = -c3; B[1, 2] = -c1;

            C[0, 0] = 1; C[0, 1] = 1;
            return new Tuple<Matrix<double>, Matrix<double>, Matrix<double>, Matrix<double>>(A, B, C, D);
        }
    }
}
