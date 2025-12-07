using lab1;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Text;


class Program
{
    static void Lab1Pipeline(string[] args)
    {
        /*if (args.Length == 0)
        {
            Console.WriteLine("Использование: lab1.exe <input-file>");
            Console.ReadLine();
            return;
        }*/

        string inputFile = "lab2_scheme.json"; //args[0];
        Console.WriteLine($"Входной файл: {inputFile}");

        string jsonContent = File.ReadAllText(inputFile, Encoding.UTF8);
        var scheme = ElectricSchemeDeserializer.DeserializeFromJson(jsonContent);
        scheme.PrintScheme();

        Console.WriteLine("\n===Остовное дерево===");
        foreach (var item in scheme.GetBackBoneTree())
            Console.WriteLine(item);

        Console.WriteLine("\n===Поиск и анализ контуров===");
        var cycles = CycleDecomposition.FindCircutsWithTreeEdges(scheme.GetAllBranches(), scheme.GetBackBoneTree());
        foreach (var cyc in cycles)
            Console.WriteLine(cyc);

        Console.WriteLine("\n===М-матрица схемы===");
        Console.WriteLine(scheme.CalcMMatrix());

        Console.WriteLine("\n===Система уравнений для напряжений и токов===");
        scheme.PrintSystemByMMatrix();

        foreach (var annot in scheme.GetAnnotToIndDict().OrderBy(x => x.Value))
            Console.Write(annot.Key + ";");

        Console.WriteLine();
        PrintMatrixFULL(scheme.CalcBigMatrix(new HashSet<int>()));
        Console.WriteLine("столбцы выходов (Y) = ");
        foreach (var y_col in scheme.GetNeededCols(scheme.outputs, 0))
            Console.Write(y_col.ToString() + " ");

        Console.WriteLine("\nстолбцы переменных состояния (X)t = ");
        foreach (var dx_col in scheme.GetNeededCols(scheme.state_vars, 0))
            Console.Write(dx_col.ToString() + " ");

        Console.WriteLine("\nстолбцы переменных состояния\' (dX/dt) = ");
        foreach (var dx_col in scheme.GetNeededCols(scheme.state_vars, scheme.GetAllBranches().Count * 2))
            Console.Write(dx_col.ToString() + " ");

        Console.WriteLine("\nстолбцы источников воздействия = ");
        foreach (var dx_col in scheme.GetSourcesCols())
            Console.Write(dx_col.ToString() + " ");

        var matrix_for_dx = scheme.CalcBigMatrix(new HashSet<int>());
        var matrix_for_y = scheme.CalcBigMatrix(scheme.GetNeededCols(scheme.state_vars, scheme.GetAllBranches().Count * 2));

        var yellow_columns = scheme.GetNeededCols(scheme.state_vars, 0);
        yellow_columns.UnionWith(scheme.GetSourcesCols());

        var processed_matrix_for_dx = V_V_Samokhin_matrix_desintegrator.ProcessMatrix(matrix_for_dx,
            scheme.GetNeededCols(scheme.state_vars, scheme.GetAllBranches().Count * 2), yellow_columns);
        Console.WriteLine("Матрица для вычисления dX/dt\n");
        PrintMatrixFULL(processed_matrix_for_dx);
        
        var processed_matrix_for_y = V_V_Samokhin_matrix_desintegrator.ProcessMatrix(matrix_for_y,
            scheme.GetNeededCols(scheme.outputs, 0), yellow_columns);
        Console.WriteLine("Матрица для вычисления Y\n" + processed_matrix_for_y.ToString());

        var AB = scheme.CalcTwoMatrices(processed_matrix_for_dx, scheme.GetNeededCols(scheme.state_vars, scheme.GetAllBranches().Count * 2),
            scheme.GetNeededCols(scheme.state_vars, 0), scheme.GetSourcesCols());
        var CD = scheme.CalcTwoMatrices(processed_matrix_for_y, scheme.GetNeededCols(scheme.outputs, 0),
            scheme.GetNeededCols(scheme.state_vars, 0), scheme.GetSourcesCols());

        string[] x_annotations = ElectricScheme.GetAnnotationsOf(scheme.state_vars);
        string[] y_annotations = ElectricScheme.GetAnnotationsOf(scheme.outputs);

        var X_0 = InputStartState(x_annotations);
        SystemDE system = new SystemDE(AB.Item1, AB.Item2, CD.Item1, CD.Item2, scheme.GetVVector(), X_0);

        Console.Write("Введите время конца симуляции (double): ");
        var t_end = double.Parse(Console.ReadLine());
        Console.Write("Введите шаг симуляции (double): ");
        var step_size = double.Parse(Console.ReadLine());
        Solver.Solve(system, new EulerNumericSolver(), t_end, step_size);


        ///////////////////////////////////////////////////////////////////////////
        string plots_path = "../../../../../lab1_plots";
        if (!Directory.Exists(plots_path))
        {
            Directory.CreateDirectory(plots_path);
            Console.WriteLine($"Папка создана: {plots_path}");
        }

        string x_file = Path.Combine(plots_path, $"X_at_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.png");
        string y_file = Path.Combine(plots_path, $"Y_at_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.png");

        PrettyPlotter.SavePlots(system, x_file, y_file, x_annotations, y_annotations);
        Console.ReadLine();
    }
    static void Main(string[] args)
    {
        Lab1Pipeline(args);
    }

    static void PrintMatrixFULL(Matrix<double> matrix)
    {
        string format = $"{{0,{6}:F{4}}} ";
        for (int i = 0; i < matrix.RowCount; i++)
        {
            for (int j = 0; j < matrix.ColumnCount; j++)
            {
                Console.Write(string.Format(format, matrix[i, j]));
            }
            Console.WriteLine();
        }
    }

    private static Vector<double> InputStartState(string[] x_annots)
    {
        var answer = Vector<double>.Build.Dense(x_annots.Length);
        Console.WriteLine("Введите начальное состояние системы (double): ");
        for(int i = 0; i < x_annots.Length; i++)
        {
            Console.Write($"{i + 1}) {x_annots[i]} = ");
            answer[i] = double.Parse(Console.ReadLine());
        }

        return answer;
    }
}