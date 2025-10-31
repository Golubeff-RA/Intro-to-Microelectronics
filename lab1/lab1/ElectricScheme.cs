using MathNet.Numerics.LinearAlgebra;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;

namespace lab1
{
    public class JsonElectricScheme
    {
        public List<ResistorBranch> resistors { get; set; } = new List<ResistorBranch>();
        public List<CapacitorBranch> capacitors { get; set; } = new List<CapacitorBranch>();
        public List<InductorBranch> inductors { get; set; } = new List<InductorBranch>();
        public List<CurrentSourceBranch> current_sources { get; set; } = new List<CurrentSourceBranch>();
        public List<VoltageSourceBranch> voltage_sources { get; set; } = new List<VoltageSourceBranch>();

        [JsonPropertyName("state_valiables")]
        public List<JsonStateVariable> state_valiables { get; set; } = new List<JsonStateVariable>();

        public List<JsonStateVariable> outputs { get; set; } = new List<JsonStateVariable>();
    }
    public class JsonStateVariable
    {
        public string annotation { get; set; } = "";

        [JsonConverter(typeof(StateTypesConverter))]
        public STATE_TYPES state_type { get; set; }
        public int? unique_id { get; set; }
    }
    public class ElectricScheme
    {
        public List<CurrentSourceBranch> current_sources { get; set; } = new List<CurrentSourceBranch>();
        public List<VoltageSourceBranch> voltage_sources { get; set; } = new List<VoltageSourceBranch>();
        public List<ResistorBranch> resistors { get; set; } = new List<ResistorBranch>();
        public List<CapacitorBranch> capacitors { get; set; } = new List<CapacitorBranch>();
        public List<InductorBranch> inductors { get; set; } = new List<InductorBranch>();
        public List<StateVariable> state_vars { get; set; } = new List<StateVariable>();
        public List<StateVariable> outputs { get; set; } = new List<StateVariable>();
        public List<Branch> GetBackBoneTree()
        {
            List<Branch> general_edges = new List<Branch>();
            general_edges.InsertRange(general_edges.Count, voltage_sources);
            general_edges.InsertRange(general_edges.Count, capacitors);

            var parent = new Dictionary<string, string>();
            var rank = new Dictionary<string, int>();

            string Find(string node)
            {
                if (!parent.ContainsKey(node))
                {
                    parent[node] = node;
                    rank[node] = 0;
                    return node;
                }

                if (parent[node] != node)
                {
                    parent[node] = Find(parent[node]);
                }
                return parent[node];
            }

            bool Union(string node1, string node2)
            {
                string root1 = Find(node1);
                string root2 = Find(node2);

                if (root1 == root2)
                    return false;

                if (rank[root1] < rank[root2])
                    parent[root1] = root2;
                else if (rank[root2] > rank[root1])
                    parent[root2] = root1;
                else
                {
                    parent[root2] = root1;
                    rank[root1]++;
                }
                return true;
            }

            var backboneTree = new List<Branch>();
            foreach (var branch in general_edges)
            {
                if (Union(branch.input_node, branch.output_node))
                    backboneTree.Add(branch);
            }

            foreach (var branch in resistors)
            {
                if (Union(branch.input_node, branch.output_node))
                    backboneTree.Add(branch);
            }

            return backboneTree;
        }
        public List<Branch> GetAllBranches()
        {
            List<Branch> edges = new List<Branch>();
            edges.InsertRange(edges.Count, voltage_sources);
            edges.InsertRange(edges.Count, capacitors);

            edges.InsertRange(edges.Count, current_sources);
            edges.InsertRange(edges.Count, resistors);

            edges.InsertRange(edges.Count, inductors);
            return edges;
        }
        public List<Branch> GetOtherBranches()
        {
            var back_bone = GetBackBoneTree();
            var all_branches = GetAllBranches();
            var other_branches = new List<Branch>();
            foreach (var elem in resistors)
                if (!back_bone.Contains(elem))
                    other_branches.Add(elem);

            foreach (var elem in inductors)
                other_branches.Add(elem);

            foreach (var elem in current_sources)
                other_branches.Add(elem);

            return other_branches;
        }
        private Tuple<Dictionary<int, int>, Dictionary<int, int>> GetRowColIdxs()
        {
            var other_branches = GetOtherBranches();
            var back_bone = GetBackBoneTree();
            Dictionary<int, int> cols_idxs = new Dictionary<int, int>();
            Dictionary<int, int> rows_idxs = new Dictionary<int, int>();
            for (int i = 0; i < other_branches.Count; i++)
                rows_idxs.Add(other_branches[i].unique_id, i);
            for (int i = 0; i < back_bone.Count; i++)
                cols_idxs.Add(back_bone[i].unique_id, i);

            return new Tuple<Dictionary<int, int>, Dictionary<int, int>>(rows_idxs, cols_idxs);
        }
        public Matrix<double> CalcMMatrix()
        {
            var other_branches = GetOtherBranches();
            var back_bone = GetBackBoneTree();
            var circuts = CycleDecomposition.FindCircutsWithTreeEdges(GetAllBranches(), back_bone);
            var matrix = Matrix<double>.Build.Dense(other_branches.Count, back_bone.Count);

            var res = GetRowColIdxs();
            var rows_idxs = res.Item1;
            var cols_idxs = res.Item2;
           
            foreach (var elem in other_branches)
            {
                // найдём контур, содержащий элемент
                Circut circut_with_elem = null;
                foreach (var circut in circuts)
                    if (circut.GetElementIds().Contains(elem.unique_id))
                        circut_with_elem = circut;

                if (circut_with_elem != null)
                {
                    List<Branch> backbone_branches_in_cir = new List<Branch>();
                    foreach (var branch in circut_with_elem.branches)
                        if (back_bone.Contains(branch))
                            matrix[rows_idxs[elem.unique_id], cols_idxs[branch.unique_id]] =
                                (double)Circut.CheckBranchesConsistency(circut_with_elem.node_order, elem, branch);
                }
            }
            
            return matrix;
        }
        public void PrintScheme()
        {
            Console.WriteLine("===Компоненты схемы===");
            var all_elements = GetAllBranches();
            all_elements.Sort((Branch x, Branch y) => x.unique_id.CompareTo(y.unique_id));
            foreach (var elem in all_elements) Console.WriteLine(elem.unique_id + ") " + elem.ToString());

            Console.WriteLine("\n===Переменные состояния===");
            foreach (var variable in state_vars) Console.WriteLine(variable.ToString());

            Console.WriteLine("\n===Выходные параметры===");
            foreach (var variable in outputs) Console.WriteLine(variable.ToString());
        }
        public void PrintSystemByMMatrix()
        {
            var other_branches = GetOtherBranches();
            var back_bone = GetBackBoneTree();
            var m_matrix = CalcMMatrix();
            var res = GetRowColIdxs();
            var rows_idxs = res.Item1;
            var cols_idxs = res.Item2;

            Console.WriteLine("___");
            foreach (var elem in other_branches)
            { 
                Console.Write($"| U_{elem.unique_id} = ");
                foreach (var bone in back_bone)
                {
                    var coef = m_matrix[rows_idxs[elem.unique_id], cols_idxs[bone.unique_id]];
                    if (coef == 1)
                        Console.Write($"- U_{bone.unique_id} ");
                    else if (coef == -1)
                        Console.Write($"+ U_{bone.unique_id} ");
                    else
                        Console.Write(new string(' ', ($"+ U_{bone.unique_id} ").Length));
                }
                Console.WriteLine();
            }
            Console.WriteLine("|");
            foreach (var bone in back_bone)
            {
                Console.Write($"| I_{bone.unique_id} = ");
                foreach (var elem in other_branches)
                {
                    var coef = m_matrix[rows_idxs[elem.unique_id], cols_idxs[bone.unique_id]];
                    if (coef == 1)
                        Console.Write($"+ I_{elem.unique_id} ");
                    else if (coef == -1)
                        Console.Write($"- I_{elem.unique_id} ");
                    else
                        Console.Write(new string(' ', ($"+ I_{elem.unique_id} ").Length));
                }
                Console.WriteLine();
            }
            Console.WriteLine("---");

        }
        
        public Dictionary<string, int> GetAnnotToIndDict()
        {
            var answer = new Dictionary<string, int>();
            var all_components = GetAllBranches();
            foreach (var component in all_components) {
                answer.Add($"U_{component.unique_id}", component.unique_id);
                answer.Add($"I_{component.unique_id}", component.unique_id + all_components.Count());
                answer.Add($"dU_{component.unique_id}/dt", component.unique_id + all_components.Count() * 2);
                answer.Add($"dI_{component.unique_id}/dt", component.unique_id + all_components.Count() * 3);
            }
            return answer;
        }
        public Matrix<double> CalcBigMatrix()
        {
            var annot_to_idx = GetAnnotToIndDict();
            var idx_to_annot = annot_to_idx.ToDictionary(x => x.Value, x => x.Key);
            var m_matrix = CalcMMatrix();
            int total_row_count = m_matrix.RowCount + m_matrix.ColumnCount + 
                resistors.Count + capacitors.Count + inductors.Count;

            Matrix<double> answer = Matrix<double>.Build.Dense(total_row_count, annot_to_idx.Count);
            int current_row = 0;

            var other_branches = GetOtherBranches();
            var back_bone = GetBackBoneTree();

            for (int i = 0; i < other_branches.Count; i++)
            {
                answer[current_row, annot_to_idx[$"U_{other_branches[i].unique_id}"]] = -1;
                for (int j = 0; j < back_bone.Count; j++)
                {
                    if (m_matrix.At(i, j) != 0)
                        answer[current_row, annot_to_idx[$"U_{back_bone[j].unique_id}"]] = -m_matrix.At(i, j);
                }
                current_row++;
            }

            for (int i = 0; i < back_bone.Count; i++)
            {
                answer[current_row, annot_to_idx[$"I_{back_bone[i].unique_id}"]] = -1;
                for (int j = 0; j < other_branches.Count; j++)
                {
                    if (m_matrix.At(j, i) != 0)
                        answer[current_row, annot_to_idx[$"I_{other_branches[j].unique_id}"]] = m_matrix.At(j, i);
                }
                current_row++;
            }

            foreach (var resistor in resistors)
            {
                answer[current_row, annot_to_idx[$"U_{resistor.unique_id}"]] = -1;
                answer[current_row, annot_to_idx[$"I_{resistor.unique_id}"]] = resistor.resistance;
                current_row++;
            }

            foreach (var capacitor in capacitors)
            {
                answer[current_row, annot_to_idx[$"I_{capacitor.unique_id}"]] = -1;
                answer[current_row, annot_to_idx[$"dU_{capacitor.unique_id}/dt"]] = capacitor.capacity;
                current_row++;
            }

            foreach (var inductor in inductors)
            {
                answer[current_row, annot_to_idx[$"U_{inductor.unique_id}"]] = -1;
                answer[current_row, annot_to_idx[$"dI_{inductor.unique_id}/dt"]] = inductor.inductivity;
                current_row++;
            }

            return answer;
        }

        public HashSet<int> GetNeededCols(List<StateVariable> vars, int padding)
        {
            var cols = new HashSet<int>();

            foreach (var output in vars)
                if (output.type == STATE_TYPES.VOLTAGE)
                    cols.Add(output.unique_id + padding);
                else
                    cols.Add(output.unique_id + GetAllBranches().Count + padding);

            return cols;
        }
        public HashSet<int> GetSourcesCols()
        {
            var cols = new HashSet<int>();
            foreach (var cur in current_sources)
                cols.Add(cur.unique_id + GetAllBranches().Count);

            foreach (var volt in voltage_sources)
                cols.Add(volt.unique_id);
            return cols;
        }


    }
}
