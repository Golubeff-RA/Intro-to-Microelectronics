using System.Text.Json;
using System.Text.Json.Serialization;

namespace lab1
{
    public class ElectricSchemeDeserializer
    {
        public static ElectricScheme DeserializeFromJson(string jsonContent)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString,
                    Converters = { new StateTypesConverter() }
                };

                var jsonScheme = JsonSerializer.Deserialize<JsonElectricScheme>(jsonContent, options);
                var electricScheme = new ElectricScheme();

                electricScheme.resistors = jsonScheme?.resistors ?? new List<ResistorBranch>();
                electricScheme.capacitors = jsonScheme?.capacitors ?? new List<CapacitorBranch>();
                electricScheme.inductors = jsonScheme?.inductors ?? new List<InductorBranch>();
                electricScheme.current_sources = jsonScheme?.current_sources ?? new List<CurrentSourceBranch>();
                electricScheme.voltage_sources = jsonScheme?.voltage_sources ?? new List<VoltageSourceBranch>();


                if (jsonScheme?.state_valiables != null)
                {
                    foreach (var jsonStateVar in jsonScheme.state_valiables)
                    {
                        if (jsonStateVar.unique_id.HasValue)
                        {
                            electricScheme.state_vars.Add(new StateVariable
                            {
                                annotation = jsonStateVar.annotation,
                                type = jsonStateVar.state_type,
                                unique_id = jsonStateVar.unique_id.Value
                            });
                        }
                    }
                }


                if (jsonScheme?.outputs != null)
                {
                    foreach (var jsonOutput in jsonScheme.outputs)
                    {
                        if (jsonOutput.unique_id.HasValue)
                        {
                            electricScheme.outputs.Add(new StateVariable
                            {
                                annotation = jsonOutput.annotation,
                                type = jsonOutput.state_type,
                                unique_id = jsonOutput.unique_id.Value
                            });
                        }
                    }
                }

                return electricScheme;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Ошибка десериализации JSON", ex);
            }
        }
    }
}
