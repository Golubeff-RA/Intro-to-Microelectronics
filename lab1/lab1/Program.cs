using lab1;
using System.Text;


class Program
{
    static void Main(string[] args)
    {
        /*if (args.Length == 0)
        {
            Console.WriteLine("Использование: lab1.exe <input-file>");
            Console.ReadLine();
            return;
        }

        string inputFile = args[0];
        Console.WriteLine($"Входной файл: {inputFile}");*/

        string jsonContent = File.ReadAllText("input_sample.json", Encoding.UTF8);
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
        //Console.WriteLine("\n===Как только посчитаете коэффициенты нажмите Enter, будем вводить матрицы===");
        //Console.ReadLine();

        Console.WriteLine(scheme.CalcBigMatrix().ToMatrixString());
        Console.WriteLine("столбцы выходов (Y) = ");
        foreach (var y_col in scheme.GetNeededCols(scheme.outputs, 0))
            Console.Write(y_col.ToString() +  " ");

        Console.WriteLine("\nстолбцы переменных состояния (X)t = ");
        foreach (var dx_col in scheme.GetNeededCols(scheme.state_vars, 0))
            Console.Write(dx_col.ToString() + " ");

        Console.WriteLine("\nстолбцы переменных состояния\' (dX/dt) = ");
        foreach (var dx_col in scheme.GetNeededCols(scheme.state_vars, scheme.GetAllBranches().Count * 2))
            Console.Write(dx_col.ToString() + " ");

        Console.WriteLine("\nстолбцы источников воздействия = ");
        foreach (var dx_col in scheme.GetSourcesCols())
            Console.Write(dx_col.ToString() + " ");
        Console.ReadLine();
    }

}