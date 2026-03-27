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
    [Slider(LabelLanguageId = "LabelAmountOfTechTypes", TooltipLanguageId = "TooltipAmountOfTechTypes", Min = 10, Max = 250)]
    public int AmountOfTechTypes = 100;
    [Slider(LabelLanguageId = "LabelMultiplier", TooltipLanguageId = "TooltipMultiplier", Min = 1, Max = 100)]
    public int Multiplier = 1;
}