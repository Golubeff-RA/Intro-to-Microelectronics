using MathNet.Numerics.LinearAlgebra;
using ScottPlot.TickGenerators.TimeUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace lab1
{
    public class V_V_Samokhin_matrix_desintegrator
    {
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
            for(int i = 0; i < matrix.ColumnCount; ++i)
                if (!yellow_columns.Contains(i) && i != col && (Math.Abs(matrix[row, i]) > 1e-10))
                    return i;
            return -1;
        }
        private static int GetNotNullRow(Matrix<double> matrix, int row, int col, HashSet<int> used_rows)
        {
            for (int i = 0; i < matrix.RowCount; i++)
                if (!used_rows.Contains(i) && i != row && (Math.Abs(matrix[i, col]) > 1e-10))
                    return i;
            return -1;
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
                int needed_row = -1;
                for (int i = 0; i < copy.RowCount; i++)
                    if (Math.Abs(copy[i, column]) > 1e-10)
                    { 
                       needed_row = i;
                       break; 
                    }
                if (needed_row == -1)
                    throw new Exception("Blue col contains only zeros");

                var used_rows = new HashSet<int>();
                int not_null_col = 0;
                do
                {
                    // получили индекс не желтого столбца с ненулевым кэфом
                    not_null_col = CheckRow(copy, needed_row, column, yellow_columns);
                    if (not_null_col != -1)
                    {
                        // получили строку тож с ненулевым кэфом в этом столбце
                        int row_to_substrate = GetNotNullRow(copy, needed_row, not_null_col, used_rows);
                        if (row_to_substrate == -1)
                        {
                            throw new Exception($"no row to substrate! {not_null_col}");
                        }
                        used_rows.Add(row_to_substrate);
                        double coeff = copy[needed_row, not_null_col] / copy[row_to_substrate, not_null_col];
                        GaussSubtraction(ref copy, needed_row, row_to_substrate, coeff);
                    }

                } while (not_null_col != -1 && used_rows.Count < copy.RowCount - 1);
                


                // Нормализуем опорную строку (делаем 1 в голубом столбце)
                double coef = copy[needed_row, column];
                NormalizeRow(ref copy, needed_row, coef);
                // Проверяем, что в опорной строке ненулевые элементы только в желтых столбцах и текущем голубом
                // Если нет - нужно дополнительное преобразование
                for (int j = 0; j < result.ColumnCount; j++)
                {
                    result[current_row, j] = copy[needed_row, j];
                }
                current_row++;
            }

            return result;
        }
    }
}
