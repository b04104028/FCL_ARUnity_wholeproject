// Root myDeserializedClass = JsonConvert.DeserializeObject(myJsonResponse);
///Users/gast/Downloads/arfoundation-samples-main/Assets/Scenes/ImageTracking/Data/DataFromJson.cs
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
[Serializable]
public class House
{
    [JsonProperty("id")] public string id; //{ get; set; }
    [JsonProperty("load")] public string load; //{ get; set; }
}

public class Root
{
    public List<House> houses; //{ get; set; }

}