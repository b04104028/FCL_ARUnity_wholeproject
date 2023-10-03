using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class Mobility : MonoBehaviour
{
    IDictionary<int, List<Dictionary<string, object>>> IDDict = new Dictionary<int, List<Dictionary<string, object>>>();

    [SerializeField]public GameObject spherePrefab;
    public Material[] userMaterials;
    public float timeSpeedupFactor = 100.0f;
    private static string starttime = "2023-07-31T03:59:37.400";
    DateTime previousTime = Convert.ToDateTime(starttime);

    private float currentTime = 0f;
    private int currentSphereIndex = 0;
    private int spheresPerGroup = 4; // Number of spheres per group before destroying
    private int maxSpheresPerUser = 6; // Maximum number of spheres per user before destroying

    private Dictionary<int, int> userSphereCounts = new Dictionary<int, int>();
    private List<GameObject> instantiatedSpheres = new List<GameObject>();
    private Coroutine instantiationCoroutine;

   // Define the scaled latitude and longitude range
    public Vector3 refLocation = new Vector3(129.6f, 7f, -22.6f);
    public double[] modelRef = {  8.47834472749149, 47.38709400665463 };
    public double[] modelOrigin = { 8.47667632699755, 47.387293832076544 };
    

    double minLat, maxLat, minLon, maxLon;
    List<double> Lats_list = new List<double>();
    List<double> Lons_list = new List<double>();

    // Start is called before the first frame update
    void Start()
    {
        string path = Application.persistentDataPath + "/PersistantFilePath/" + "Zurich_Mobility.geojson";
        Debug.Log(path);
        LoadJson(path, IDDict);

        //to test loadjson: 
        //int testtime = 10;
        //ShowMobilityData(testtime, IDDict) ;

        minLat = Lats_list.Min();
        maxLat = Lats_list.Max();
        minLon = Lons_list.Min();
        maxLon = Lons_list.Max();
        Debug.Log("minlat = " + minLat + ", maxlat = " + maxLat);

        //Debug.Log("test lat: " + Lats_list[30]);
        //Debug.Log("min location " + minLocation.ToString());
        //Debug.Log("max location " + maxLocation.ToString());

        //Debug.Log(Map(Lats_list[30], minLat, maxLat, minLocation.z, maxLocation.z));


        //InstantiateSphere(IDDict);
        StartCoroutine(InstantiateSpheresWithTimeDelay());
    }
    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime * timeSpeedupFactor;
    }
    public void testLat(List<double> Lats_list)
    {
        foreach (double _lat in Lats_list)
        {
            Debug.Log(_lat);
        }
    }
    public void LoadJson(string path, IDictionary<int, List<Dictionary<string, object>>> IDDict)
    {
        using (StreamReader r = new StreamReader(path))
        {
            string jsonString = r.ReadToEnd();
            ZurichMobilityJson zrhmobilityjsonroot = JsonUtility.FromJson<ZurichMobilityJson>(jsonString);
            //Debug.Log(zrhmobilityjsonroot.features.ToString());
                        
            foreach (Feature f in zrhmobilityjsonroot.features)
            {
                int this_uid = (int)f.properties.uid;
                string dateTimeString = f.properties.Time;
                DateTime dateTime = DateTime.Parse(dateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                double lat = f.properties.Lat;
                double lon = f.properties.Lon;
                Lats_list.Add(lat);
                Lons_list.Add(lon);
                if (!IDDict.ContainsKey(this_uid))
                {
                    IDDict[this_uid] = new List<Dictionary<string, object>>();                  
                }
                var dataEntry = new Dictionary<string, object>
            {
                { "Time", dateTime },
                { "Lat", lat },
                { "Lon", lon }
            };
                // Add the data entry to the list for the userid
                IDDict[this_uid].Add(dataEntry);
            }            
        }

    }
    public void ShowMobilityData(int testTime , IDictionary<int, List<Dictionary<string, object>>> IDDict)
    {
        foreach (var kvp in IDDict)
        {
            double userid = kvp.Key;
            List<Dictionary<string, object>> userEntries = kvp.Value;

            Debug.Log($"User ID: {userid}");
            int i = 0;
            foreach (var entry in userEntries)
            {
                DateTime time = (DateTime)entry["Time"];
                double lat = (double)entry["Lat"];
                double lon = (double)entry["Lon"];
                Debug.Log($"Time: {time}, Lat: {lat}, Lon: {lon}");
                i++;
                if (i > testTime) break;
            }
        }
    }

    public void InstantiateSphere(IDictionary<int, List<Dictionary<string, object>>> aggregatedData)
    {
        float timeDifferenceThreshold = 5.0f; // Time difference threshold in seconds
        int spheresPerGroup = 4; // Number of spheres per group before destroying


        // Iterate through the aggregated data and create spheres
        foreach (var kvp in aggregatedData)
        {
            double userid = kvp.Key;
            List<Dictionary<string, object>> userEntries = kvp.Value;

            int sphereCount = 0;
            GameObject sphereGroup = null;
            DateTime previousTime = DateTime.MinValue;

            // Assign a color to the user based on userid
            int colorIndex = (int)userid % userMaterials.Length;
            Material userMaterial = userMaterials[colorIndex];

            foreach (var entry in userEntries)
            {
                // Extract time data
                DateTime currentTime = (DateTime)entry["Time"];

                // Check if a new sphere group needs to be created
                if (sphereCount % spheresPerGroup == 0)
                {
                    sphereGroup = new GameObject("SphereGroup");
                }

                // Calculate the time difference between the current entry and the previous one
                double timeDifference = (currentTime - previousTime).TotalSeconds;

                if (timeDifference > timeDifferenceThreshold)
                {
                    // Extract latitude and longitude
                    double lat = (double)entry["Lat"];
                    double lon = (double)entry["Lon"];

                    // Map latitude and longitude to the desired range
                    float x = Map(lon, modelOrigin[0], modelRef[0], refLocation.x);
                    float z = Map(lat, modelOrigin[1], modelRef[1], refLocation.z);
                    //Debug.Log("mapped x = " + x + ", mapped z = " + z);

                    Vector3 position = new Vector3(x, 7f, z); // Adjust the Y-coordinate as needed
                    GameObject sphere = Instantiate(spherePrefab, position, Quaternion.identity, sphereGroup.transform);
                    
                    // Assign the user's color material to the sphere
                    Renderer sphereRenderer = sphere.GetComponent<Renderer>();
                    if (sphereRenderer != null)
                    {
                        sphereRenderer.material = userMaterial;
                    }

                    // Update the previous time
                    previousTime = currentTime;

                    sphereCount++;
                }
            }
        }
    }

   
    private float Map(double lonlat, double modelorigin, double modelref, float refloc)
    {
        return (float)((lonlat - modelorigin) / (modelref - modelorigin) * refloc);
    }

    private IEnumerator InstantiateSpheresWithTimeDelay()
    {
        Debug.Log("function is called");
        foreach (var kvp in IDDict)
        {
            Debug.Log("uid loop is called");
            int userId = kvp.Key;
            List<Dictionary<string, object>> userEntries = kvp.Value;
            userSphereCounts[userId] = 0;

            foreach (var entry in userEntries)
            {
                Debug.Log("entry loop is called");
                DateTime timestamp = (DateTime)entry["Time"];
                

                Debug.Log("timestamp: " + timestamp.ToString());

                // Calculate the time difference between the current timestamp and the currentTime
                double timeDifference = (timestamp - previousTime).TotalSeconds / timeSpeedupFactor;
                Debug.Log("minimum value of datetime? " + previousTime.ToString());
                Debug.Log("time difference calculated: " + timeDifference);

                // Wait for the specified time difference
                yield return new WaitForSeconds((float)timeDifference);
                Debug.Log("after waiting");
                // Check if a new sphere group needs to be created
                if (userSphereCounts[userId] % spheresPerGroup == 0)
                {
                    Debug.Log("in the sphere group creation if");
                    GameObject sphereGroup = new GameObject("SphereGroup");
                }

                // Extract latitude and longitude
                double lat = (double)entry["Lat"];
                double lon = (double)entry["Lon"];

                // Map latitude and longitude to the desired range
                float x = Map(lon, modelOrigin[0], modelRef[0], refLocation.x);
                float z = Map(lat, modelOrigin[1], modelRef[1], refLocation.z);
                Debug.Log("x y mapped: " + x + ", " + z);

                Vector3 position = new Vector3(x, 7f, z); // Adjust the Y-coordinate as needed
                GameObject sphere = Instantiate(spherePrefab, position, Quaternion.identity);
                Debug.Log("sphere instantiation");
                // Assign the user's color material to the sphere
                Renderer sphereRenderer = sphere.GetComponent<Renderer>();
                if (sphereRenderer != null)
                {
                    int colorIndex = userId % userMaterials.Length;
                    sphereRenderer.material = userMaterials[colorIndex];
                }

                // Update userSphereCounts
                userSphereCounts[userId]++;

                // Add the instantiated sphere to the list
                instantiatedSpheres.Add(sphere);

                // Check if the first sphere needs to be destroyed
                if (userSphereCounts[userId] > maxSpheresPerUser)
                {
                    GameObject firstSphere = instantiatedSpheres.First();
                    instantiatedSpheres.Remove(firstSphere);
                    Destroy(firstSphere);
                }
                previousTime = timestamp;
            }
        }

        // Repeat the instantiation process in a loop
        StartCoroutine(InstantiateSpheresWithTimeDelay());
    }

}




