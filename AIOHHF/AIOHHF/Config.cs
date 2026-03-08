using Nautilus.Json;
using Nautilus.Options.Attributes;

namespace AIOHHF;

[Menu("All-In-One Hand Held Fabricator")]
public class Config : ConfigFile
{
    [Slider(LabelLanguageId = "LabelSpawnRate", Min = -40, Max = 10, DefaultValue = 0, TooltipLanguageId = "TooltipSpawnRate")] 
    public float SpawnRate = 0;
    [Toggle(LabelLanguageId = "LabelDebugMode", TooltipLanguageId = "TooltipDebugMode")]
    public bool DebugMode = false;
}