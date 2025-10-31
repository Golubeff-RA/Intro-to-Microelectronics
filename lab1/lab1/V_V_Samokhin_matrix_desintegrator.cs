using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace lab1
{
    public class V_V_Samokhin_matrix_desintegrator
    {
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
            
            return matrix;
        }
    }
}
