using lab1;
using System.Text;

string jsonContent = File.ReadAllText("input_sample.json", Encoding.UTF8);

var scheme = ElectricSchemeDeserializer.DeserializeFromJson(jsonContent);

Console.WriteLine($"Резисторы: {scheme.resistors.Count}");
foreach (var item in scheme.resistors)
    Console.WriteLine(item);
Console.WriteLine($"Конденсаторы: {scheme.capacitors.Count}");
foreach (var item in scheme.capacitors)
    Console.WriteLine(item);
Console.WriteLine($"Катушки: {scheme.inductors.Count}");
foreach (var item in scheme.inductors)
    Console.WriteLine(item);
Console.WriteLine($"Источники тока: {scheme.current_sources.Count}");
foreach (var item in scheme.current_sources)
    Console.WriteLine(item);
Console.WriteLine($"Источники напряжения: {scheme.voltage_sources.Count}");
foreach (var item in scheme.voltage_sources)
    Console.WriteLine(item);

Console.WriteLine("\nOutputs:");
foreach (var output in scheme.outputs)
    Console.WriteLine(output);


Console.WriteLine("\nState variables:");
foreach (var output in scheme.state_vars)
    Console.WriteLine(output);

var tree = scheme.GetBackBoneTree();
Console.WriteLine("\n\nBranches for backbone tree:");
foreach (var item in tree)
    Console.WriteLine(item);

var edges = scheme.GetAllBranches();
var cycles = CycleDecomposition.FindCircutsWithTreeEdges(edges, tree);
foreach(var cyc in cycles)
    Console.WriteLine(cyc);