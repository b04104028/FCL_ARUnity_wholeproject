using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MobilityFlow : MonoBehaviour
{
    List<(double, DateTime, double, double)> dataList = new List<(double, DateTime, double, double)>();
    private Dictionary<double, Color> uidToColor = new Dictionary<double, Color>();
    [SerializeField]public float prefabStayTime = 5f;
    private List<GameObject> instantiatedSpheres = new List<GameObject>();


    [SerializeField] public GameObject spherePrefab;
    public float timeSpeedupFactor = 1000.0f;
    //private static string starttime = "2023-07-31T05:59:37.400";//"2023-07-31T03:59:37.400";
    //note: beware of time zone when using DateTime
    private DateTime startTime;
    private DateTime previousTime;
    private DateTime endTime;

    // Define the scaled latitude and longitude range
    public Vector3 refLocation = new Vector3(1.237F, 0.1F, -0.192F);//F, -17.8F, 0);//(129.6f, 7f, -22.6f);
    public Vector3 unityOrigin = new Vector3(0, 0.1F, 0);//(0,0,20F);//(0,7,0); 
    public double[] modelRef = { 8.47834472749149, 47.38709400665463 };
    public double[] modelOrigin = { 8.47667632699755, 47.387293832076544 };
    [SerializeField] public Transform parentTransform;

    //progress bar timeline
    [SerializeField]public TextMeshProUGUI timelineText;
    [SerializeField]public Slider progressBar;

    // Start is called before the first frame update
    void Start()
    {
        
        string path = Application.persistentDataPath + "/PersistantFilePath/" + "Zurich_Mobility.geojson";
        LoadJsonFlat(path);

        startTime = dataList.Min(item => item.Item2);
        endTime = dataList.Max(item => item.Item2);

        unityOrigin = spherePrefab.transform.position + new Vector3(0f,7f,0f);
        // Assign colors to each unique uid
        AssignColorsToUIDs();
        // Start the sphere instantiation process
        StartCoroutine(InstantiateSpheresWithTimeDelay());

   }
    // Update is called once per frame
    void Update()
    {
        //currentTime += Time.deltaTime;
    }
    public void LoadJsonFlat(string path)
    {
        using (StreamReader r = new StreamReader(path))
        {
            string jsonString = r.ReadToEnd();
            ZurichMobilityJson zrhmobilityjsonroot = JsonUtility.FromJson<ZurichMobilityJson>(jsonString);
            //Debug.Log(zrhmobilityjsonroot.features.ToString());

            foreach (Feature feature in zrhmobilityjsonroot.features)
            {
                double uid = feature.properties.uid;
                DateTime time = DateTime.Parse(feature.properties.Time, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                
                double lat = feature.properties.Lat;
                double lon = feature.properties.Lon;

                dataList.Add((uid, time, lat, lon));
            }

            dataList = dataList.OrderBy(item => item.Item2).ToList();
        }

    }
    //to test load json:
    public void ShowData(List<(double, DateTime, double, double)> dataList)
    {
        int testTime = 0;
        foreach (var item in dataList)
        {
            double uid = item.Item1;         
            DateTime time = item.Item2;     
            double lat = item.Item3;         
            double lon = item.Item4;

            testTime++;
            Debug.Log($"UID: {uid}, Time: {time}, Lat: {lat}, Lon: {lon}");
            if (testTime > 30) break;
        }
    }

    private void AssignColorsToUIDs()
    {
        foreach (var item in dataList)
        {
            double uid = item.Item1;
            if (!uidToColor.ContainsKey(uid))
            {
                // Generate a unique color for each uid
                Color color = UnityEngine.Random.ColorHSV();
                uidToColor[uid] = color;
            }
        }
    }

    private IEnumerator InstantiateSpheresWithTimeDelay()
    {
        int i = 0;
        foreach (var item in dataList)
        {
            double uid = item.Item1;
            DateTime timestamp = item.Item2;
            double lat = item.Item3;
            double lon = item.Item4;

            if (i == 0) previousTime = startTime;

            ShowTime(timestamp);

            // Calculate the time difference relative to the startTime
            double timeDifference = (timestamp - previousTime).TotalSeconds / timeSpeedupFactor;
       
            // Wait for the specified time difference
            yield return new WaitForSeconds((float)timeDifference);

            // Map latitude and longitude to Unity space
            Vector3 position = MapCoordinatesToUnitySpace(lon, lat);

            if (Vector3.Distance(position, unityOrigin) <= (Vector3.Distance(refLocation, unityOrigin)+6f))
            {
                // Instantiate the sphere prefab
                GameObject sphere = Instantiate(spherePrefab, position, Quaternion.identity);
                instantiatedSpheres.Add(sphere);        
                // Assign the user's color material to the sphere based on uid
                Renderer sphereRenderer = sphere.GetComponent<Renderer>();
                if (sphereRenderer != null)
                {
                    Color color = uidToColor[uid];
                    sphereRenderer.material.color = color;
                }
                // Destroy the sphere after n seconds
                Destroy(sphere, prefabStayTime); 

            }
            i++;

            previousTime = timestamp;
        }
        Debug.Log("data points within model range: " + instantiatedSpheres.Count);//Ans: 2432   
        Debug.Log("number of all data points in json: " + dataList.Count);//Ans: 25353
    }
    private Vector3 MapCoordinatesToUnitySpace(double lon, double lat)
    {
        //note: longitude =x, latutude = z {x, z}
        Vector3 position = Vector3.zero;
        position.x = (float)((lon - modelOrigin[0]) / (modelRef[0] - modelOrigin[0]) * refLocation.x);
        position.y = refLocation.y;
        position.z = (float)((lat - modelOrigin[1]) / (modelRef[1] - modelOrigin[1]) * refLocation.z);
        //Debug.Log("mapped(x, y, z) = " + position.x + ", " + position.y + ", " + position.z);

        return position;
    }

    public void ShowTime(DateTime timestamp)
    {
        timelineText.text = timestamp.ToString();

        progressBar.minValue = 0f;
        progressBar.maxValue = (float)((endTime - startTime).TotalSeconds);
        progressBar.value = (float)((timestamp - startTime).TotalSeconds);//float
        //Debug.Log("time now: " + timestamp);
    }


}




