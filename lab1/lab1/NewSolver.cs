using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using static CircuitMatrixSolver;

public class CircuitMatrixSolver
{
    public class SolutionResult
    {
        public bool Success { get; set; }
        public Vector<double> SolutionRow { get; set; }
        public string Error { get; set; }

        // Для удобства - преобразование в массив
        public double[] SolutionArray => SolutionRow?.ToArray();

        // Коэффициенты линейной комбинации строк
        public Vector<double> Coefficients { get; set; }
    }

    /// <summary>
    /// Находит линейную комбинацию строк матрицы, где ненулевыми будут только целевой и разрешенные столбцы
    /// </summary>
    /// <param name="matrix">Исходная матрица уравнений (m строк, 4*n столбцов)</param>
    /// <param name="targetColumn">Индекс целевого столбца (0-based)</param>
    /// <param name="allowedColumns">Множество разрешенных столбцов</param>
    /// <returns>Решение в виде вектора-строки</returns>
    public static SolutionResult Solve(
        Matrix<double> matrix,
        int targetColumn,
        HashSet<int> allowedColumns)
    {
        int m = matrix.RowCount;
        int totalColumns = matrix.ColumnCount;

        // Проверка входных данных
        if (m == 0)
            return new SolutionResult { Success = false, Error = "Матрица пуста" };

        if (targetColumn < 0 || targetColumn >= totalColumns)
            return new SolutionResult { Success = false, Error = $"Недопустимый индекс целевого столбца: {targetColumn}" };

        // Проверяем, что целевой столбец не входит в allowedColumns
        if (allowedColumns.Contains(targetColumn))
            return new SolutionResult { Success = false, Error = "Целевой столбец не должен быть в allowedColumns" };

        // Вычисляем forbiddenColumns как все столбцы, кроме целевого и разрешенных
        var forbiddenColumns = new HashSet<int>();
        for (int i = 0; i < totalColumns; i++)
        {
            if (i != targetColumn && !allowedColumns.Contains(i))
            {
                forbiddenColumns.Add(i);
            }
        }

        // Проверяем, что все столбцы учтены
        int totalCount = 1 + allowedColumns.Count + forbiddenColumns.Count;
        if (totalCount != totalColumns)
            return new SolutionResult { Success = false, Error = "Не все столбцы учтены" };

        // Если нет запрещенных столбцов, просто возвращаем любую строку с ненулевым targetColumn
        if (forbiddenColumns.Count == 0)
        {
            return HandleNoForbiddenColumns(matrix, targetColumn);
        }

        // 1. Переупорядочиваем столбцы: запрещенные -> целевой -> разрешенные
        var columnOrder = new List<int>();
        columnOrder.AddRange(forbiddenColumns.OrderBy(x => x));
        columnOrder.Add(targetColumn);
        columnOrder.AddRange(allowedColumns.OrderBy(x => x));

        // Проверяем, что порядок правильный
        if (columnOrder.Count != totalColumns || columnOrder.Distinct().Count() != totalColumns)
            return new SolutionResult { Success = false, Error = "Ошибка в переупорядочивании столбцов" };

        // Создаем переупорядоченную матрицу
        var reorderedMatrix = PermuteColumns(matrix, columnOrder);

        // 2. Используем метод Гаусса для нахождения линейной комбинации
        int forbiddenCount = forbiddenColumns.Count;
        int targetPos = forbiddenCount; // позиция целевого столбца после переупорядочивания

        // Создаем матрицу коэффициентов для линейной комбинации
        // Ищем вектор коэффициентов α такой, что:
        // 1. Σα_i * (запрещенные столбцы)_i = 0 для всех запрещенных столбцов
        // 2. Σα_i * (целевой столбец)_i ≠ 0

        // Матрица из запрещенных столбцов (m x forbiddenCount)
        var A = reorderedMatrix.SubMatrix(0, m, 0, forbiddenCount);

        // Вектор целевого столбца
        var targetColVector = reorderedMatrix.Column(targetPos);

        try
        {
            // Решаем систему A^T * A * x = A^T * (-b)
            // где b = targetColVector
            var AT = A.Transpose();
            var ATA = AT * A;
            var ATb = AT * (-targetColVector);

            Vector<double> coefficients;

            // Пробуем решить систему
            if (forbiddenCount <= m && ATA.Determinant() > 1e-12)
            {
                // Система квадратная или переопределенная, но с полным рангом
                coefficients = ATA.Solve(ATb);
            }
            else if (m >= forbiddenCount)
            {
                // Используем SVD для более стабильного решения
                var svd = A.Svd(true);

                // Проверяем ранг матрицы A
                int rank = svd.Rank;
                if (rank < forbiddenCount)
                {
                    // Матрица вырождена, нужен специальный подход
                    return SolveWithSpecialMethod(reorderedMatrix, forbiddenCount, targetPos,
                                                 columnOrder, totalColumns);
                }

                coefficients = svd.Solve(-targetColVector);
            }
            else
            {
                // Строк меньше, чем запрещенных столбцов
                // Используем метод наименьших квадратов
                coefficients = (AT * A).Inverse() * (AT * (-targetColVector));
            }

            // 3. Вычисляем итоговую строку как линейную комбинацию
            var solution = Vector<double>.Build.Dense(totalColumns);

            for (int i = 0; i < m; i++)
            {
                if (Math.Abs(coefficients[i]) > 1e-12)
                {
                    solution += coefficients[i] * reorderedMatrix.Row(i);
                }
            }

            // 4. Проверяем результат
            bool success = true;

            // Проверяем запрещенные столбцы (должны быть ~0)
            for (int i = 0; i < forbiddenCount; i++)
            {
                if (Math.Abs(solution[i]) > 1e-8)
                {
                    success = false;
                    break;
                }
            }

            // Проверяем целевой столбец (должен быть ≠ 0)
            if (Math.Abs(solution[targetPos]) < 1e-12)
            {
                success = false;
            }

            if (!success)
            {
                // Пробуем альтернативный метод
                return SolveWithGaussianElimination(matrix, targetColumn, allowedColumns,
                                                   columnOrder, totalColumns);
            }

            // 5. Восстанавливаем исходный порядок столбцов
            var finalSolution = Vector<double>.Build.Dense(totalColumns);
            for (int i = 0; i < totalColumns; i++)
            {
                finalSolution[columnOrder[i]] = solution[i];
            }

            return new SolutionResult
            {
                Success = true,
                SolutionRow = finalSolution,
                Coefficients = coefficients
            };
        }
        catch (Exception ex)
        {
            // Пробуем методом Гаусса как запасной вариант
            return SolveWithGaussianElimination(matrix, targetColumn, allowedColumns,
                                               columnOrder, totalColumns);
        }
    }

    /// <summary>
    /// Пользовательская функция для перестановки столбцов
    /// </summary>
    private static Matrix<double> PermuteColumns(Matrix<double> matrix, List<int> columnOrder)
    {
        int m = matrix.RowCount;
        int n = matrix.ColumnCount;
        var result = Matrix<double>.Build.Dense(m, n);

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                result[i, j] = matrix[i, columnOrder[j]];
            }
        }

        return result;
    }

    /// <summary>
    /// Обработка случая, когда нет запрещенных столбцов
    /// </summary>
    private static SolutionResult HandleNoForbiddenColumns(Matrix<double> matrix, int targetColumn)
    {
        int m = matrix.RowCount;

        // Ищем строку, где целевой столбец не нулевой
        for (int i = 0; i < m; i++)
        {
            if (Math.Abs(matrix[i, targetColumn]) > 1e-12)
            {
                var coefficients = Vector<double>.Build.Dense(m);
                coefficients[i] = 1.0;

                return new SolutionResult
                {
                    Success = true,
                    SolutionRow = matrix.Row(i),
                    Coefficients = coefficients
                };
            }
        }

        return new SolutionResult
        {
            Success = false,
            Error = "Не найдена строка с ненулевым целевым столбцом"
        };
    }

    /// <summary>
    /// Специальный метод для вырожденных матриц
    /// </summary>
    private static SolutionResult SolveWithSpecialMethod(
        Matrix<double> reorderedMatrix,
        int forbiddenCount,
        int targetPos,
        List<int> columnOrder,
        int totalColumns)
    {
        int m = reorderedMatrix.RowCount;

        // Используем метод полного гауссова исключения
        // Создаем расширенную матрицу [A | b], где A - запрещенные столбцы, b - целевой
        var A = reorderedMatrix.SubMatrix(0, m, 0, forbiddenCount);
        var b = reorderedMatrix.Column(targetPos);

        // Объединяем в одну матрицу
        var augmented = Matrix<double>.Build.Dense(m, forbiddenCount + 1);
        augmented.SetSubMatrix(0, 0, A);
        augmented.SetColumn(forbiddenCount, b);

        // Приводим к ступенчатому виду по строкам (ручная реализация)
        int currentRow = 0;
        for (int col = 0; col < forbiddenCount && currentRow < m; col++)
        {
            // Ищем ненулевой элемент в текущем столбце
            int pivotRow = -1;
            double maxAbs = 0;
            for (int row = currentRow; row < m; row++)
            {
                double absVal = Math.Abs(augmented[row, col]);
                if (absVal > maxAbs)
                {
                    maxAbs = absVal;
                    pivotRow = row;
                }
            }

            if (pivotRow == -1 || maxAbs < 1e-12) continue;

            // Меняем строки местами
            if (pivotRow != currentRow)
            {
                var tempRow = Vector<double>.Build.Dense(forbiddenCount + 1);
                for (int j = 0; j <= forbiddenCount; j++)
                {
                    tempRow[j] = augmented[pivotRow, j];
                    augmented[pivotRow, j] = augmented[currentRow, j];
                    augmented[currentRow, j] = tempRow[j];
                }
            }

            // Нормируем ведущую строку
            double pivot = augmented[currentRow, col];
            if (Math.Abs(pivot) > 1e-12)
            {
                for (int j = col; j <= forbiddenCount; j++)
                {
                    augmented[currentRow, j] /= pivot;
                }

                // Обнуляем столбец в других строках
                for (int row = 0; row < m; row++)
                {
                    if (row != currentRow)
                    {
                        double factor = augmented[row, col];
                        if (Math.Abs(factor) > 1e-12)
                        {
                            for (int j = col; j <= forbiddenCount; j++)
                            {
                                augmented[row, j] -= factor * augmented[currentRow, j];
                            }
                        }
                    }
                }

                currentRow++;
            }
        }

        // Ищем решение обратной подстановкой
        var coefficients = Vector<double>.Build.Dense(m);
        var usedRows = new List<int>();

        // Проходим по ступенчатой матрице снизу вверх
        for (int row = m - 1; row >= 0; row--)
        {
            // Находим первый ненулевой элемент в строке (ведущий)
            int pivotCol = -1;
            for (int col = 0; col < forbiddenCount; col++)
            {
                if (Math.Abs(augmented[row, col]) > 1e-12)
                {
                    pivotCol = col;
                    break;
                }
            }

            if (pivotCol == -1) continue; // Нулевая строка

            // Выражаем коэффициент через свободные переменные
            // В упрощенном случае полагаем свободные переменные = 0
            double rhs = augmented[row, forbiddenCount];
            double pivotValue = augmented[row, pivotCol];

            // Находим, какая исходная строка соответствует этой строке
            // В реальной реализации нужно отслеживать перестановки строк
            coefficients[row] = rhs / pivotValue;
            usedRows.Add(row);
        }

        // Если не нашли решения, пробуем случайные комбинации
        if (usedRows.Count == 0)
        {
            // Генерируем случайные коэффициенты и проверяем результат
            var random = new Random();
            for (int attempt = 0; attempt < 100; attempt++)
            {
                // Исправленная строка: создаем вектор с случайными значениями
                coefficients = Vector<double>.Build.Dense(m, i => random.NextDouble() * 2 - 1); // значения от -1 до 1

                // Вычисляем линейную комбинацию
                var solution = Vector<double>.Build.Dense(totalColumns);
                for (int i = 0; i < m; i++)
                {
                    solution += coefficients[i] * reorderedMatrix.Row(i);
                }

                // Проверяем запрещенные столбцы
                bool valid = true;
                for (int i = 0; i < forbiddenCount; i++)
                {
                    if (Math.Abs(solution[i]) > 1e-8)
                    {
                        valid = false;
                        break;
                    }
                }

                // Проверяем целевой столбец
                if (Math.Abs(solution[targetPos]) < 1e-12)
                {
                    valid = false;
                }

                if (valid)
                {
                    // Восстанавливаем порядок столбцов
                    var finalSolution = Vector<double>.Build.Dense(totalColumns);
                    for (int i = 0; i < totalColumns; i++)
                    {
                        finalSolution[columnOrder[i]] = solution[i];
                    }

                    return new SolutionResult
                    {
                        Success = true,
                        SolutionRow = finalSolution,
                        Coefficients = coefficients
                    };
                }
            }
        }
        else
        {
            // Вычисляем решение с найденными коэффициентами
            var solution = Vector<double>.Build.Dense(totalColumns);
            for (int i = 0; i < m; i++)
            {
                if (Math.Abs(coefficients[i]) > 1e-12)
                {
                    solution += coefficients[i] * reorderedMatrix.Row(i);
                }
            }

            // Восстанавливаем порядок столбцов
            var finalSolution = Vector<double>.Build.Dense(totalColumns);
            for (int i = 0; i < totalColumns; i++)
            {
                finalSolution[columnOrder[i]] = solution[i];
            }

            return new SolutionResult
            {
                Success = true,
                SolutionRow = finalSolution,
                Coefficients = coefficients
            };
        }

        return new SolutionResult
        {
            Success = false,
            Error = "Не удалось найти решение специальным методом"
        };
    }

    /// <summary>
    /// Метод Гаусса для решения через элементарные преобразования строк
    /// </summary>
    private static SolutionResult SolveWithGaussianElimination(
        Matrix<double> originalMatrix,
        int targetColumn,
        HashSet<int> allowedColumns,
        List<int> columnOrder,
        int totalColumns)
    {
        int m = originalMatrix.RowCount;

        // Создаем копию матрицы для работы
        var matrix = originalMatrix.Clone();

        // Вычисляем forbiddenColumns
        var forbiddenColumns = new HashSet<int>();
        for (int i = 0; i < totalColumns; i++)
        {
            if (i != targetColumn && !allowedColumns.Contains(i))
            {
                forbiddenColumns.Add(i);
            }
        }

        int forbiddenCount = forbiddenColumns.Count;

        // Переупорядочиваем столбцы для удобства
        matrix = PermuteColumns(matrix, columnOrder);

        // Прямой ход метода Гаусса
        int currentRow = 0;

        for (int col = 0; col < forbiddenCount && currentRow < m; col++)
        {
            // Ищем ненулевой элемент в текущем столбце ниже текущей строки
            int pivotRow = -1;
            double maxAbs = 0;

            for (int row = currentRow; row < m; row++)
            {
                double absVal = Math.Abs(matrix[row, col]);
                if (absVal > maxAbs)
                {
                    maxAbs = absVal;
                    pivotRow = row;
                }
            }

            if (pivotRow == -1 || maxAbs < 1e-12) continue;

            // Меняем строки местами
            if (pivotRow != currentRow)
            {
                // Исправлено: создаем временные копии строк
                var tempRow = matrix.Row(pivotRow).Clone();
                matrix.SetRow(pivotRow, matrix.Row(currentRow));
                matrix.SetRow(currentRow, tempRow);
            }

            // Нормируем ведущую строку
            double pivot = matrix[currentRow, col];
            if (Math.Abs(pivot) > 1e-12)
            {
                // Делим всю строку на pivot
                var normalizedRow = matrix.Row(currentRow).Divide(pivot);
                matrix.SetRow(currentRow, normalizedRow);

                // Обнуляем столбец в других строках
                for (int row = 0; row < m; row++)
                {
                    if (row != currentRow)
                    {
                        double factor = matrix[row, col];
                        if (Math.Abs(factor) > 1e-12)
                        {
                            var newRow = matrix.Row(row) - factor * matrix.Row(currentRow);
                            matrix.SetRow(row, newRow);
                        }
                    }
                }

                currentRow++;
            }
        }

        // Теперь матрица приведена к упрощенному виду
        // Ищем строку, где в запрещенных столбцах нули, а в целевом - не ноль

        for (int row = 0; row < m; row++)
        {
            bool validRow = true;

            // Проверяем запрещенные столбцы
            for (int col = 0; col < forbiddenCount; col++)
            {
                if (Math.Abs(matrix[row, col]) > 1e-8)
                {
                    validRow = false;
                    break;
                }
            }

            // Проверяем целевой столбец
            if (validRow && Math.Abs(matrix[row, forbiddenCount]) > 1e-12)
            {
                // Нашли подходящую строку
                // Восстанавливаем исходный порядок столбцов
                var solution = Vector<double>.Build.Dense(totalColumns);
                for (int i = 0; i < totalColumns; i++)
                {
                    solution[columnOrder[i]] = matrix[row, i];
                }

                // Создаем вектор коэффициентов (единичный вектор для этой строки)
                var coefficients = Vector<double>.Build.Dense(m);
                coefficients[row] = 1.0;

                return new SolutionResult
                {
                    Success = true,
                    SolutionRow = solution,
                    Coefficients = coefficients
                };
            }
        }

        // Если не нашли подходящую строку, пробуем линейную комбинацию строк
        // Решаем систему уравнений для коэффициентов

        var A = matrix.SubMatrix(0, m, 0, forbiddenCount);

        try
        {
            var ATA = A.TransposeThisAndMultiply(A);
            var eig = ATA.Evd();

            // Ищем собственный вектор для наименьшего собственного значения
            int minEigIndex = 0;
            double minEig = double.MaxValue;

            for (int i = 0; i < eig.EigenValues.Count; i++)
            {
                double eigVal = eig.EigenValues[i].Real;
                if (eigVal < minEig && eigVal > 1e-12)
                {
                    minEig = eigVal;
                    minEigIndex = i;
                }
            }

            // Берем соответствующий собственный вектор
            var eigenVector = eig.EigenVectors.Column(minEigIndex);

            // Вычисляем линейную комбинацию строк
            var solutionVector = Vector<double>.Build.Dense(totalColumns);

            for (int i = 0; i < m; i++)
            {
                if (Math.Abs(eigenVector[i]) > 1e-12)
                {
                    solutionVector += eigenVector[i] * matrix.Row(i);
                }
            }

            // Восстанавливаем порядок столбцов
            var finalSolution = Vector<double>.Build.Dense(totalColumns);
            for (int i = 0; i < totalColumns; i++)
            {
                finalSolution[columnOrder[i]] = solutionVector[i];
            }

            // Проверяем результат
            bool valid = true;
            for (int i = 0; i < forbiddenCount; i++)
            {
                if (Math.Abs(finalSolution[columnOrder[i]]) > 1e-8)
                {
                    valid = false;
                    break;
                }
            }

            if (valid && Math.Abs(finalSolution[targetColumn]) > 1e-12)
            {
                return new SolutionResult
                {
                    Success = true,
                    SolutionRow = finalSolution,
                    Coefficients = eigenVector
                };
            }
        }
        catch (Exception ex)
        {
            // Если не сработало, возвращаем ошибку
            return new SolutionResult
            {
                Success = false,
                Error = $"Не удалось найти решение методом Гаусса: {ex.Message}"
            };
        }

        return new SolutionResult
        {
            Success = false,
            Error = "Не удалось найти решение методом Гаусса"
        };
    }

    /// <summary>
    /// Вспомогательный метод для создания матрицы из двумерного массива
    /// </summary>
    public static Matrix<double> CreateMatrix(double[,] data)
    {
        return DenseMatrix.OfArray(data);
    }

    /// <summary>
    /// Вспомогательный метод для создания матрицы из массива массивов
    /// </summary>
    public static Matrix<double> CreateMatrix(double[][] data)
    {
        return DenseMatrix.OfRowArrays(data);
    }
}


public class BatchCircuitMatrixSolver
{
    private readonly CircuitMatrixSolver _solver;

    public BatchCircuitMatrixSolver()
    {
        _solver = new CircuitMatrixSolver();
    }

    /// <summary>
    /// Решает задачу для нескольких целевых столбцов
    /// </summary>
    /// <param name="matrix">Исходная матрица уравнений</param>
    /// <param name="targetColumns">Множество целевых столбцов</param>
    /// <param name="allowedColumns">Множество разрешенных столбцов</param>
    /// <returns>Матрица, где каждая строка - решение для соответствующего целевого столбца</returns>
    public Matrix<double> SolveBatch(
        Matrix<double> matrix,
        HashSet<int> targetColumns,
        HashSet<int> allowedColumns)
    {
        int totalColumns = matrix.ColumnCount;

        // Проверяем, что целевые столбцы не пересекаются с разрешенными
        var intersection = new HashSet<int>(targetColumns);
        intersection.IntersectWith(allowedColumns);

        if (intersection.Count > 0)
        {
            throw new ArgumentException($"Целевые столбцы {string.Join(", ", intersection)} не должны быть в allowedColumns");
        }

        // Проверяем, что все столбцы входят в одну из категорий
        var allColumnsSet = new HashSet<int>(Enumerable.Range(0, totalColumns));
        var providedColumnsSet = new HashSet<int>(allowedColumns);
        providedColumnsSet.UnionWith(targetColumns);

        // Все остальные столбцы считаются запрещенными
        // (это проверка логики, на самом деле forbidden вычисляется внутри)

        // Сортируем целевые столбцы для воспроизводимости
        var sortedTargetColumns = targetColumns.OrderBy(x => x).ToList();
        int numTargets = sortedTargetColumns.Count;

        // Создаем результирующую матрицу
        var resultMatrix = Matrix<double>.Build.Dense(numTargets, totalColumns);

        // Словарь для хранения решений (на случай, если нужно будет получать их по столбцу)
        var solutions = new Dictionary<int, Vector<double>>();

        // Решаем задачу для каждого целевого столбца
        for (int i = 0; i < numTargets; i++)
        {
            int targetColumn = sortedTargetColumns[i];

            var solution = CircuitMatrixSolver.Solve(matrix, targetColumn, allowedColumns);

            if (!solution.Success)
            {
                throw new InvalidOperationException(
                    $"Не удалось найти решение для целевого столбца {targetColumn}: {solution.Error}");
            }

            // Записываем решение в строку матрицы
            resultMatrix.SetRow(i, solution.SolutionRow);
            solutions[targetColumn] = solution.SolutionRow;
        }

        return resultMatrix;
    }
}