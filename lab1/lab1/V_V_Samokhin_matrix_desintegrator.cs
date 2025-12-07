using MathNet.Numerics.LinearAlgebra;
using ScottPlot.MultiplotLayouts;
using ScottPlot.TickGenerators.TimeUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;


namespace lab1
{
    public class V_V_Samokhin_matrix_desintegrator
    {
        private static Random random = new Random(124);
        private static void GaussSubtraction(ref Matrix<double> matrix, int row1, int row2, double coeff)
        {
            for (int i = 0; i < matrix.ColumnCount; ++i)
                matrix[row1, i] -= coeff * matrix[row2, i];
        }
        private static void NormalizeRow(ref Matrix<double> matrix, int row, double coeff)
        {
            for (int i = 0; i < matrix.ColumnCount; ++i)
                matrix[row, i] /= coeff;
        }
        private static int CheckRow(Matrix<double> matrix, int row, int col, HashSet<int> yellow_columns)
        {
            List<int> cols = new List<int>();
            for(int i = 0; i < matrix.ColumnCount; ++i)
                if (!yellow_columns.Contains(i) && i != col && (Math.Abs(matrix[row, i]) > 1e-5))
                    cols.Add(i);
            if (cols.Count == 0) return -1;

            int index = random.Next(0, cols.Count);
            return cols[index];
        }
        private static bool CheckRowBadColumns(Matrix<double> matrix, HashSet<int> yellow_columns, int row, int targer_col, int col)
        {
            for (int i = 0; i < matrix.ColumnCount; ++i)
            {
                if (yellow_columns.Contains(i) || i == targer_col || i == col)
                    continue;
                if (Math.Abs(matrix[row, i]) < 1e-10)
                    continue;

                int non_zero_cnt = 0;
                for (int j = 0; j < matrix.RowCount; ++j)
                {
                    if (Math.Abs(matrix[j, i]) > 1e-10)
                    {
                        non_zero_cnt++;
                        if (non_zero_cnt > 1)
                            break;
                    }
                }

                if (non_zero_cnt == 1)
                    return false;
            }

            return true;
        }
        private static int GetNotNullRow(Matrix<double> matrix, int row, int col, HashSet<int> used_rows, HashSet<int> yellow, int target)
        {
            List<int> rows = new List<int>();
            for (int i = 0; i < matrix.RowCount; i++)
                if (!used_rows.Contains(i) && i != row && (Math.Abs(matrix[i, col]) > 1e-10)
                    && CheckRowBadColumns(matrix, yellow, i, target, col))
                    rows.Add(i);
            if (rows.Count == 0) return -1;
            
            int index = random.Next(0, rows.Count);
            return rows[index];
        }
        // Нужно сделать так, чтобы в каждом голубом столбце была лишь 1 единица в строке i (остальные - нули)
        // При этом в этой iй строке ненулевые коэффиценты должны быть только в желтых столбцах и данном голубом столбце
        // Можно домножать строки на константу != 0 и складывать строки
        // 
        // Как можно потестить: в Program.cs парсится схема в объект scheme. У него можно позвать метод CalcBigMatrix() ->matrix
        // Чтобы получить голубые столбцы: для dX/dt: scheme.GetNeededCols(scheme.state_vars, scheme.GetAllBranches().Count * 2)
        //                                 для Y    : scheme.GetNeededCols(scheme.outputs, 0)
        // ЖЕЛТЫЕ = scheme.GetNeededCols(scheme.state_vars, 0) + scheme.GetSourcesCols() (P.S. это источники воздейтсвия (V))
        public static Matrix<double> ProcessMatrix(Matrix<double> matrix, HashSet<int> blue_columns, HashSet<int> yellow_columns)
        {
            Matrix<double> result = Matrix<double>.Build.Dense(blue_columns.Count, matrix.ColumnCount);
            int current_row = 0;
            // Сортируем голубые столбцы для последовательной обработки
            foreach (var column in blue_columns.OrderBy(c => c))
            {
                Matrix<double> copy = matrix.Clone();
                // ищем строчку с ненулевым элементом в голубом столбце
                List<int> needed_rows = new List<int>();
                for (int i = 0; i < copy.RowCount; i++)
                    if (Math.Abs(copy[i, column]) > 1e-10)
                    { 
                       needed_rows.Add(i);
                    }
                if (needed_rows.Count == 0)
                    throw new Exception("Blue col contains only zeros");

                int idx = 0;
                int not_null_col = 0;
                HashSet<int> used_rows = new HashSet<int>();
                int counter = 0;
                do
                {
                    counter++;
                    // получили индекс не желтого столбца с ненулевым кэфом
                    not_null_col = CheckRow(copy, needed_rows[idx], column, yellow_columns);
                    if (not_null_col != -1)
                    {
                        // получили строку тож с ненулевым кэфом в этом столбце
                        int row_to_substrate = GetNotNullRow(copy, needed_rows[idx], not_null_col, new HashSet<int>(), yellow_columns, column);
                        if (row_to_substrate == -1 || counter > 1000)
                        {
                            idx += 1;
                            Console.WriteLine($"No row to substrate to zero col: {not_null_col}, row_idx: {needed_rows[idx-1]}");
                            if (idx >= needed_rows.Count)
                            {
                                throw new Exception($"no row to substrate row = {needed_rows[idx - 1]}");
                            }
                            used_rows = new HashSet<int>();
                            counter = 0;
                            continue;
                        }
                        
                        used_rows.Add(row_to_substrate);
                        double coeff = copy[needed_rows[idx], not_null_col] / (copy[row_to_substrate, not_null_col]);
                        if (Math.Abs(coeff * copy[row_to_substrate, column] - copy[needed_rows[idx], column]) < 1e-10)
                            continue;
                        //Console.WriteLine($"coeff = {coeff} nom = {copy[needed_rows[idx], not_null_col]} denom = {copy[row_to_substrate, not_null_col]} target = {copy[needed_rows[idx], column]}");
                        string format = $"{{0,{5}:F{2}}}";
                        /*Console.WriteLine();
                        for (int i = 0; i < matrix.ColumnCount; i++)
                            if (Math.Abs(copy[needed_rows[idx], i]) > 1e-10)
                                Console.Write(string.Format(format, copy[needed_rows[idx], i]) + " ");
                            else
                                Console.Write("     ");*/

                        GaussSubtraction(ref copy, needed_rows[idx], row_to_substrate, coeff);
                        //Console.WriteLine($"coeff = {coeff} nom = {copy[needed_rows[idx], not_null_col]} denom = {copy[row_to_substrate, not_null_col]} target = {copy[needed_rows[idx], column]}");
                        NormalizeRow(ref copy, needed_rows[idx], copy[needed_rows[idx], column]);
                    }

                } while (not_null_col != -1 && used_rows.Count < copy.RowCount - 1);
                


                // Нормализуем опорную строку (делаем 1 в голубом столбце)
                double coef = copy[needed_rows[idx], column];
                NormalizeRow(ref copy, needed_rows[idx], coef);
                // Проверяем, что в опорной строке ненулевые элементы только в желтых столбцах и текущем голубом
                // Если нет - нужно дополнительное преобразование
                for (int j = 0; j < result.ColumnCount; j++)
                {
                    result[current_row, j] = copy[needed_rows[idx], j];
                }
                current_row++;
            }

            return result;
        }
    }
}
