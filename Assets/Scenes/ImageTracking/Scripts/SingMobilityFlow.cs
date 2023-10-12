using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//REMEMBER TO SEPARAPTE NON VHEICLE DATA
public class SingMobilityFlow : MonoBehaviour
{
    List<(double, DateTime, double, double, double)> dataList = new List<(double, DateTime, double, double, double)>();
    private Dictionary<double, Color> uidToColor = new Dictionary<double, Color>();
    int NonVehicleCount = 0;
    private float prefabStayTime = 5f;
    private List<GameObject> instantiatedSpheres = new List<GameObject>();


    [SerializeField] public GameObject spherePrefab;
    private float timeSpeedupFactor = 1000.0f;
    //private static string starttime = "2023-07-31T05:59:37.400";//"2023-07-31T03:59:37.400";
    //note: beware of time zone when using DateTime
    private DateTime startTime;
    private DateTime previousTime;
    private DateTime endTime;

    // Define the scaled latitude and longitude range
    public GameObject unityRefLocation; // new Vector3(1.237F, 0.1F, -0.192F);//F, -17.8F, 0);//(129.6f, 7f, -22.6f);
    public GameObject unityOrigin;// new Vector3(0, 0.1F, 0);//(0,0,20F);//(0,7,0); 
    private double[] modelRef = {  103.89292571082814, 1.39096235621748 };//{lon, lat}
    private double[] modelOrigin = { 103.8937334500298, 1.3899905229618015};//{lon, lat}
    ///[SerializeField] public Transform parentTransform;
    private Vector3 offset = new Vector3(25f, -12f, 0);
    

    //progress bar timeline
    [SerializeField] public TextMeshProUGUI timelineText;
    [SerializeField] public Slider progressBar;

    // Start is called before the first frame update
    void Start()
    {
        string path = Application.persistentDataPath + "/PersistantFilePath/" + "Singapore_Mobility.json";
        LoadJsonFlat(path);
        ShowData(dataList);

        startTime = dataList.Min(item => item.Item2);
        endTime = dataList.Max(item => item.Item2);

        //unityOrigin= spherePrefab.transform.position + new Vector3(0f,7f,0f);
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
                double is_vehicle = feature.properties.is_vehicle;
                if (is_vehicle == 0.0)
                {
                    NonVehicleCount++;
                }
                dataList.Add((uid, time, lat, lon, is_vehicle));
            }

            dataList = dataList.OrderBy(item => item.Item2).ToList();
        }
    }
    //to test load json:
    public void ShowData(List<(double, DateTime, double, double, double)> dataList)
    {
        int testTime = 0;
        foreach (var item in dataList)
        {
            double uid = item.Item1;
            DateTime time = item.Item2;
            double lat = item.Item3;
            double lon = item.Item4;
            double is_vehicle = item.Item5;

            testTime++;
            Debug.Log($"UID: {uid}, Time: {time}, Lat: {lat}, Lon: {lon}, is_vehicle: {is_vehicle}");
            if (testTime > 30) break;
        }
    }

    private void AssignColorsToUIDs()
    {
        foreach (var item in dataList)
        {
            double uid = item.Item1;
            if(item.Item5 == 1.0)
            {
                uid += NonVehicleCount;
            }
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

            // if (Vector3.Distance(position, unityOrigin.transform.localPosition) <= (Vector3.Distance(unityRefLocation.transform.localPosition, unityOrigin.transform.localPosition) * 1.05f))
            // {
            // Instantiate the sphere prefab
            //GameObject sphere = Instantiate(spherePrefab, gameObject.transform);
            GameObject sphere = Instantiate(spherePrefab, transform.position + position, Quaternion.identity);

            //GameObject sphere = Instantiate(spherePrefab, position, Quaternion.identity);
                sphere.transform.parent = gameObject.transform;
                sphere.transform.SetParent(gameObject.transform, false);
                sphere.transform.SetLocalPositionAndRotation(position, Quaternion.identity);
                sphere.transform.localScale = new Vector3(5f, 5f, 5f);
                Debug.Log(sphere.ToString());

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

            //}
            i++;

            previousTime = timestamp;
        }
        Debug.Log("data points within model range: " + instantiatedSpheres.Count);//Ans: 2432   //3556
        Debug.Log("number of all data points in json: " + dataList.Count);//Ans: 25353
    }
    private Vector3 MapCoordinatesToUnitySpace(double lon, double lat)
    {
        //note: longitude經度 =x, latutude緯度 = z {x, z}
        Vector3 position = Vector3.zero;
        position.x = (float)(((lon - modelOrigin[0]) / (modelRef[0] - modelOrigin[0]) * (unityRefLocation.transform.localPosition.x - unityOrigin.transform.localPosition.x) + unityOrigin.transform.localPosition.x));
        position.y = unityRefLocation.transform.localPosition.y;
        position.z = (float)(((lat - modelOrigin[1]) / (modelRef[1] - modelOrigin[1]) * (unityRefLocation.transform.localPosition.z - unityOrigin.transform.localPosition.z) + unityOrigin.transform.localPosition.z));

        //should x and z both multiply by -1

        //position += offset;

        //position = Quaternion.Euler(-90, 0, 0) * position;
        //worked 
        //position = Quaternion.Euler(0,0,45) * position;
        //position = Quaternion.Euler(0, 0, -90) * position;
        //position = Quaternion.Euler(0, 0, 180) * position;
        //position = Quaternion.Euler(0, 0, 45) * position;

        //Debug.Log("mapped(x, y, z) = " + position.x + ", " + position.y + ", " + position.z);


        return position;
    }


    private Vector3 XZORIGINALMapCoordinatesToUnitySpace(double lon, double lat)
    {
        //note: longitude =x, latutude = z {x, z}
        Vector3 position = Vector3.zero;
        position.x = (float)((lon - modelOrigin[0]) / (modelRef[0] - modelOrigin[0]) * unityRefLocation.transform.position.x);
        position.y = unityRefLocation.transform.position.y;
        position.z = (float)((lat - modelOrigin[1]) / (modelRef[1] - modelOrigin[1]) * unityRefLocation.transform.position.z);
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




