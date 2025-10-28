using ScottPlot;
using ScottPlot.Triangulation;
using System.Collections.Generic;
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
    }




}
