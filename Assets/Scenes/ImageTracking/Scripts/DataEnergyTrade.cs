using System;
using System.Collections.Generic;
using Newtonsoft.Json;

// Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
[Serializable]
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class DataEnergyTrade
{
    [JsonProperty("From")] public string From;// { get; set; }
    [JsonProperty("To")] public string To;// { get; set; }
    [JsonProperty("Transmission")] public float Transmission;// { get; set; }
    [JsonProperty("T")] public int T;// { get; set; }
}
[Serializable]
public class rootDataEnergyTrade
{
    [JsonProperty("DataEnergyTrade")] public List<DataEnergyTrade> DataEnergyTrade; // { get; set; }
}

