/*ARCHIVE FOR REFERENCE, IT'S THE PREVIOUS PROGRESS OF MOBILITYFLOW.CS, IT AGGREGATE THE MOBILITY DATA 
 BY UID, AND CAN NAVIGATE THROUGH THE DATA USING KEY OF UID.
BUT WE NEED TO INSTANTIATE THE SPHERE PREFABS ACCORDING TO TIME, SO THIS FILE IS DISCARD*/

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
    private static string starttime = "2023-07-31T05:59:37.400";//"2023-07-31T03:59:37.400";
    //note: beware of time zone when using DateTime
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
        LoadJson(path, IDDict);

        //InstantiateSphere(IDDict);
        StartCoroutine(InstantiateSpheresWithTimeDelay());
    }
    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime * timeSpeedupFactor;
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

    public void ArchiveInstantiateSphere(IDictionary<int, List<Dictionary<string, object>>> aggregatedData)
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

    private IEnumerator ArchiveInstantiateSpheresWithTimeDelay()
    {
        foreach (var kvp in IDDict)
        {
            int userId = kvp.Key;
            List<Dictionary<string, object>> userEntries = kvp.Value;
            userSphereCounts[userId] = 0;

            //for each userid 
            previousTime = (DateTime)userEntries[0]["Time"];
            foreach (var entry in userEntries)
            {
                DateTime timestamp = (DateTime)entry["Time"];
                
                Debug.Log("timestamp: " + timestamp.ToString());

                // Calculate the time difference between the current timestamp and the currentTime
                double timeDifference = (timestamp - previousTime).TotalSeconds / timeSpeedupFactor;
                Debug.Log("previous time? " + previousTime.ToString());
                Debug.Log("time difference calculated: " + timeDifference);

                // Wait for the specified time difference
                yield return new WaitForSeconds((float)timeDifference);
                
                // Check if a new sphere group needs to be created
                if (userSphereCounts[userId] % spheresPerGroup == 0)
                {                    
                    GameObject sphereGroup = new GameObject("SphereGroup");
                }

                // Extract latitude and longitude
                double lat = (double)entry["Lat"];
                double lon = (double)entry["Lon"];

                // Map latitude and longitude to the desired range
                float x = Map(lon, modelOrigin[0], modelRef[0], refLocation.x);
                float z = Map(lat, modelOrigin[1], modelRef[1], refLocation.z);
                //Debug.Log("x y mapped: " + x + ", " + z);

                Vector3 position = new Vector3(x, 7f, z); // Adjust the Y-coordinate as needed
                GameObject sphere = Instantiate(spherePrefab, position, Quaternion.identity);
                
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

    private DateTime? FindOldestTime()
    {
        DateTime? oldestTime = null; // Use nullable DateTime to represent the first encountered timestamp

        foreach (var kvp in IDDict)
        {
            List<Dictionary<string, object>> userEntries = kvp.Value;

            foreach (var entry in userEntries)
            {
                DateTime timestamp = (DateTime)entry["Time"];

                // Initialize oldestTime with the first timestamp encountered
                if (!oldestTime.HasValue)
                {
                    oldestTime = timestamp;
                }
                else if (timestamp < oldestTime)
                {
                    oldestTime = timestamp; // Update oldestTime if a new earliest timestamp is found
                }
            }
        }

        return oldestTime;
    }

    private IEnumerator ArchiveNotWorkInstantiateSpheresWithTimeDelay()
    {
        DateTime? oldestTime = FindOldestTime(); // Call the FindOldestTime function to get the oldest timestamp
        Debug.Log("oldest time: " + oldestTime.ToString());

        foreach (var kvp in IDDict)
        {
            Debug.Log("you are in the IDDict foreach");
            int userId = kvp.Key;
            List<Dictionary<string, object>> userEntries = kvp.Value;
            userSphereCounts[userId] = 0;

            foreach (var entry in userEntries)
            {
                DateTime timestamp = (DateTime)entry["Time"];
                Debug.Log("time stamp"+timestamp.ToString());

                // Calculate the time difference based on the oldest timestamp
                double timeDifference = (timestamp - oldestTime.Value).TotalSeconds / timeSpeedupFactor;

                // Wait for the specified time difference
                yield return new WaitForSeconds((float)timeDifference);

                // Check if a new sphere group needs to be created
                if (userSphereCounts[userId] % spheresPerGroup == 0)
                {
                    GameObject sphereGroup = new GameObject("SphereGroup");
                }

                // Extract latitude and longitude
                double lat = (double)entry["Lat"];
                double lon = (double)entry["Lon"];

                // Map latitude and longitude to the desired range
                float x = Map(lon, modelOrigin[0], modelRef[0], refLocation.x);
                float z = Map(lat, modelOrigin[1], modelRef[1], refLocation.z);

                Vector3 position = new Vector3(x, 7f, z); // Adjust the Y-coordinate as needed
                GameObject sphere = Instantiate(spherePrefab, position, Quaternion.identity);

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
            }
        }

        // Repeat the instantiation process in a loop
        StartCoroutine(InstantiateSpheresWithTimeDelay());
    }

    public void InstantiateSphere(int userId, double lat, double lon, DateTime timestamp)
    {
        // Calculate the time difference between the current timestamp and the currentTime
        double timeDifference = (timestamp - previousTime).TotalSeconds / timeSpeedupFactor;

        // Wait for the specified time difference
        WaitForSeconds wait = new WaitForSeconds((float)timeDifference);
        StartCoroutine(InstantiateSphereDelayed(userId, lat, lon, wait));
    }

    private IEnumerator InstantiateSphereDelayed(int userId, double lat, double lon, WaitForSeconds wait)
    {
        yield return wait;

        // Map latitude and longitude to the desired range
        float x = Map(lon, modelOrigin[0], modelRef[0], refLocation.x);
        float z = Map(lat, modelOrigin[1], modelRef[1], refLocation.z);

        Vector3 position = new Vector3(x, 7f, z); // Adjust the Y-coordinate as needed
        GameObject sphere = Instantiate(spherePrefab, position, Quaternion.identity);

        // Assign the user's color material to the sphere
        int colorIndex = userId % userMaterials.Length;
        Renderer sphereRenderer = sphere.GetComponent<Renderer>();
        if (sphereRenderer != null)
        {
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
    }

    private IEnumerator InstantiateSpheresWithTimeDelay()
    {
        DateTime oldestTime = DateTime.MaxValue;

        foreach (var kvp in IDDict)
        {
            int userId = kvp.Key;
            List<Dictionary<string, object>> userEntries = kvp.Value;
            userSphereCounts[userId] = 0;

            foreach (var entry in userEntries)
            {
                DateTime timestamp = (DateTime)entry["Time"];
                InstantiateSphere(userId, (double)entry["Lat"], (double)entry["Lon"], timestamp);

                // Find the oldest timestamp among all entries
                if (timestamp < oldestTime)
                {
                    oldestTime = timestamp;
                }
            }
        }

        // Update the previousTime to the oldest timestamp
        previousTime = oldestTime;

        // Repeat the instantiation process in a loop
        StartCoroutine(InstantiateSpheresWithTimeDelay());

        // Add the following line to resolve the error
        yield return null;
    }

}




