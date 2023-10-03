// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class Feature
{
    [JsonProperty("type")] public string type; //{ get; set; }
    [JsonProperty("properties")] public Properties properties;// { get; set; }
    [JsonProperty("geometry")] public Geometry geometry;// { get; set; }
}
[Serializable]
public class Geometry
{
    [JsonProperty("type")] public string type;// { get; set; }
    [JsonProperty("coordinates")] public List<double> coordinates;// { get; set; }
}

[Serializable]
public class Properties
{
    [JsonProperty("uid")] public double uid; //{ get; set; }
    [JsonProperty("Time")] public string Time; //{ get; set; }
    [JsonProperty("Lat")] public double Lat; //{ get; set; }
    [JsonProperty("Lon")] public double Lon; //{ get; set; }
    [JsonProperty("is_vehicle")] public double is_vehicle; //{ get; set; }
}

public class ZurichMobilityJson
{
    [JsonProperty("type")] public string type; //{ get; set; }
    [JsonProperty("features")] public List<Feature> features; //{ get; set; }
}