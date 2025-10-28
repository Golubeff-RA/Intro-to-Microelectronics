using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    public class Circut
    {
        public List<Branch> branches { get; set; } = new List<Branch>();
        public string node_order { get; set; } = "";
        public HashSet<int> GetElementIds()
        {
            HashSet<int> ids = new HashSet<int>();
            foreach (var branch in branches)
            {
                ids.Add(branch.unique_id);
            }
            return ids;
        }

        public override string ToString()
        {
            string cir_str = $"Circut: {node_order}\n";
            foreach (var branch in branches)
            {
                cir_str += branch.ToString() + "\n";
            }
            return cir_str;
        }
    }
    public class CycleDecomposition
    {
        public static List<Circut> FindCircutsWithTreeEdges(
            List<Branch> allEdges,
            List<Branch> spanningTree)
        {
            var Circuts = new List<Circut>();
            var usedEdges = new HashSet<int>();

            // Шаг 1: Создаём контуры для параллельных рёбер (каждое параллельное ребро + ребро из дерева)
            CreateParallelCircuts(allEdges, spanningTree, Circuts, usedEdges);

            // Шаг 2: Находим оставшиеся рёбра не из дерева
            var remainingChords = allEdges.Where(edge =>
                !IsTreeEdge(spanningTree, edge) && !usedEdges.Contains(edge.unique_id)).ToList();

            // Шаг 3: Для каждой оставшейся хорды строим фундаментальный цикл
            foreach (var chord in remainingChords)
            {
                if (usedEdges.Contains(chord.unique_id)) continue;

                var Circut = FindFundamentalCircut(spanningTree, chord, usedEdges);
                if (Circut != null && Circut.branches.Count > 0)
                {
                    Circuts.Add(Circut);
                    foreach (var edge in Circut.branches)
                    {
                        usedEdges.Add(edge.unique_id);
                    }
                }
            }

            return Circuts;
        }

        private static void CreateParallelCircuts(
            List<Branch> allEdges,
            List<Branch> spanningTree,
            List<Circut> Circuts,
            HashSet<int> usedEdges)
        {
            // Группируем все рёбра по парам вершин
            var edgesByVertexPair = allEdges
                .GroupBy(edge => NormalizeEdgeKey(edge.input_node, edge.output_node))
                .ToList();

            foreach (var group in edgesByVertexPair)
            {
                var edges = group.ToList();

                // Если только одно ребро - пропускаем (нет параллельных)
                if (edges.Count <= 1) continue;

                // Находим рёбра из дерева в этой группе
                var treeEdgesInGroup = edges.Where(edge => IsTreeEdge(spanningTree, edge)).ToList();

                // Если нет рёбер из дерева - пропускаем
                if (!treeEdgesInGroup.Any()) continue;

                // Берём первое ребро из дерева как основу
                var treeEdge = treeEdgesInGroup[0];

                // Для каждого параллельного ребра (которое не является этим деревом) создаём контур
                foreach (var parallelEdge in edges)
                {
                    if (parallelEdge.unique_id == treeEdge.unique_id) continue;
                    if (usedEdges.Contains(parallelEdge.unique_id)) continue;

                    // Создаём контур из деревянного ребра и параллельного
                    var Circut = new Circut
                    {
                        node_order = $"{treeEdge.input_node}{treeEdge.output_node}",
                        branches = new List<Branch> { treeEdge, parallelEdge }
                    };

                    Circuts.Add(Circut);

                    // Помечаем параллельное ребро как использованное
                    usedEdges.Add(parallelEdge.unique_id);
                }

                // Помечаем деревянное ребро как использованное в параллельных контурах
                usedEdges.Add(treeEdge.unique_id);
            }
        }

        private static Circut FindFundamentalCircut(List<Branch> spanningTree, Branch chord, HashSet<int> usedEdges)
        {
            // Строим граф из ВСЕХ рёбер дерева (даже использованных в параллельных контурах)
            var graph = BuildGraph(spanningTree);

            // Находим путь между вершинами хорды в дереве
            var path = FindPathInTree(graph, chord.input_node, chord.output_node);

            if (path == null || path.Count < 2) return null;

            // Собираем цикл: путь в дереве + хорда
            var CircutBranches = new List<Branch>();

            // Добавляем рёбра пути из дерева
            for (int i = 0; i < path.Count - 1; i++)
            {
                var edge = FindEdge(spanningTree, path[i], path[i + 1]);
                if (edge != null)
                {
                    CircutBranches.Add(edge);
                }
            }

            // Добавляем саму хорду
            CircutBranches.Add(chord);

            // Формируем node_order
            var nodeOrder = BuildNodeOrder(path);

            return new Circut
            {
                node_order = nodeOrder,
                branches = CircutBranches
            };
        }

        private static Dictionary<string, List<string>> BuildGraph(List<Branch> edges)
        {
            var graph = new Dictionary<string, List<string>>();

            foreach (var edge in edges)
            {
                if (!graph.ContainsKey(edge.input_node))
                    graph[edge.input_node] = new List<string>();
                if (!graph.ContainsKey(edge.output_node))
                    graph[edge.output_node] = new List<string>();

                graph[edge.input_node].Add(edge.output_node);
                graph[edge.output_node].Add(edge.input_node);
            }

            return graph;
        }

        private static List<string> FindPathInTree(Dictionary<string, List<string>> graph,
                                                  string start, string end)
        {
            var visited = new HashSet<string>();
            var path = new List<string>();

            if (DFS(graph, start, end, visited, path))
            {
                return path;
            }

            return null;
        }

        private static bool DFS(Dictionary<string, List<string>> graph, string current,
                               string target, HashSet<string> visited, List<string> path)
        {
            visited.Add(current);
            path.Add(current);

            if (current == target)
                return true;

            if (graph.ContainsKey(current))
            {
                foreach (var neighbor in graph[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        if (DFS(graph, neighbor, target, visited, path))
                            return true;
                    }
                }
            }

            path.RemoveAt(path.Count - 1);
            return false;
        }

        private static Branch FindEdge(List<Branch> edges, string u, string v)
        {
            var normalizedKey = NormalizeEdgeKey(u, v);
            return edges.FirstOrDefault(edge =>
                NormalizeEdgeKey(edge.input_node, edge.output_node) == normalizedKey);
        }

        private static string BuildNodeOrder(List<string> path)
        {
            // Убираем дубликаты, сохраняя порядок
            var uniqueNodes = new List<string>();
            foreach (var node in path)
            {
                if (!uniqueNodes.Contains(node))
                    uniqueNodes.Add(node);
            }

            return string.Join("", uniqueNodes);
        }

        private static string NormalizeEdgeKey(string u, string v)
        {
            return string.Compare(u, v) < 0 ? $"{u}|{v}" : $"{v}|{u}";
        }

        private static bool IsTreeEdge(List<Branch> spanningTree, Branch edge)
        {
            return spanningTree.Any(treeEdge => treeEdge.unique_id == edge.unique_id);
        }

        // Вспомогательный метод для проверки валидности контура
        public static bool ValidateCircut(Circut Circut)
        {
            if (Circut.branches.Count == 0) return false;

            // Для контуров длины 2 (параллельные рёбра) - это валидный случай
            if (Circut.branches.Count == 2)
            {
                var edge1 = Circut.branches[0];
                var edge2 = Circut.branches[1];
                return NormalizeEdgeKey(edge1.input_node, edge1.output_node) ==
                       NormalizeEdgeKey(edge2.input_node, edge2.output_node);
            }

            // Для остальных контуров проверяем, что все вершины имеют степень 2
            var degree = new Dictionary<string, int>();
            foreach (var edge in Circut.branches)
            {
                if (!degree.ContainsKey(edge.input_node)) degree[edge.input_node] = 0;
                if (!degree.ContainsKey(edge.output_node)) degree[edge.output_node] = 0;

                degree[edge.input_node]++;
                degree[edge.output_node]++;
            }

            return degree.All(kvp => kvp.Value == 2);
        }
    }

}