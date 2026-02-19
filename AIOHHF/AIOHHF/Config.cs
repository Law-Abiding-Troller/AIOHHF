using Nautilus.Json;
using Nautilus.Options.Attributes;

namespace AIOHHF;

[Menu("All-In-One Hand Held Fabricator")]
public class Config : ConfigFile
{
    [Slider("Fragment spawn rate (requires restart)", Min = -40, Max = 10, DefaultValue = 0, Tooltip = "Increases or decreases the rate at which the fragments spawn. 0, 1, or -1 will change nothing.")] 
    public float SpawnRate = 0;
}