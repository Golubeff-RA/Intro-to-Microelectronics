using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace lab1
{
    public enum STATE_TYPES
    {
        [Description("voltage")]
        VOLTAGE,

        [Description("current")]
        CURRENT
    }

    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString().ToLower();
        }

        public static T GetEnumFromDescription<T>(string description) where T : Enum
        {
            var fields = typeof(T).GetFields();
            foreach (var field in fields)
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
            }

            // Если не нашли по Description, пробуем парсить по имени enum
            if (Enum.TryParse(typeof(T), description, true, out var result))
                return (T)result;

            throw new ArgumentException($"No enum value found for description: {description}");
        }
    }

    public class StateTypesConverter : JsonConverter<STATE_TYPES>
    {
        public override STATE_TYPES Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string description = reader.GetString();
            return EnumExtensions.GetEnumFromDescription<STATE_TYPES>(description);
        }

        public override void Write(Utf8JsonWriter writer, STATE_TYPES value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.GetDescription());
        }
    }

    public class StateVariable
    {
        public string annotation { get; set; } = "";
        public STATE_TYPES type { get; set; }
        public int unique_id { get; set; }

        public override string ToString()
        {
            return $"State variable {annotation}: element_id = {unique_id}, type = {type}";
        }
    }

    public class Branch
    {
        public int unique_id { get; set; }
        public string input_node { get; set; }
        public string output_node { get; set; }
        public Branch()
        {
            unique_id = 0;
            input_node = "";
            output_node = "";
        }

        public override string ToString() => $" branch: element_id = {unique_id}, input_node = {input_node}, output_node = {output_node} ";

    }
    public class ResistorBranch : Branch
    {
        public double resistance { get; set; }
        public override string ToString() => "Resistor" + base.ToString() + $"resistance = {resistance}";
    }

    public class CapacitorBranch : Branch
    {
        public double capacity { get; set; }
        public override string ToString() => "Capacitor" + base.ToString() + $"capacity = {capacity}";
    }

    public class InductorBranch : Branch
    {
        public double inductivity { get; set; }
        public override string ToString() => "Inductor" + base.ToString() + $"inductivity = {inductivity}";
    }

    public class CurrentSourceBranch : Branch
    {
        public double current { get; set; }
        public override string ToString() => "Current source" + base.ToString() + $"current = {current}";
    }

    public class VoltageSourceBranch : Branch
    {
        public double voltage { get; set; }
        public override string ToString() => "Voltage source" + base.ToString() + $"voltage = {voltage}";
    }
}
