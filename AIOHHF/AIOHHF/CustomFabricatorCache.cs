using System.Collections.Generic;
using Nautilus.Json;
using Newtonsoft.Json;

namespace AIOHHF;

public class CustomFabricatorCache : JsonFile
{
    /// <summary>
    /// TechType to CustomFabricator Class ID
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public Dictionary<string, string[]> TechTypeToCustomFabricator { get; set; } = new();

    
    [JsonIgnore]
    public override string JsonFilePath { get; } = null;
}