using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System;

public class EnergyTrade : MonoBehaviour
{
    public GameObject lineRendererPrefab;
    public GameObject AnchorpointXZ_1;
    public GameObject AnchorpointXZ_2;
    public GameObject AnchorpointXY;//height

    private float ArrowWidthScale = 0.4f;//DEFAULT VALUE
    private float height = 0.04F;//DEFAULT VALUE
    private int numberOfBombs = 10;
    private float timeDifference = 0.2f;
    private float prefabStayTime = 1f;
    private LineRenderer[] lineRenderers;
    private Vector3[] trajectoryPoints;

    //progress bar timeline
    [SerializeField] public TextMeshProUGUI timelineText;
    [SerializeField] public Slider progressBar;
    int startTime = 11;//default value for zurich case
    int endTime = 16;//default value for zurich case

    //private GameObject targetGO;

    List<(string, string, float, int)> dataList = new List<(string, string, float, int)>();
    public float maxTrans;

    //THESE POINTS ARE FOR TESTING 
    private Vector3 fromPos = new Vector3(0.04f, 0.02f, -0.16f)- new Vector3(0, 0.02f, 0);
    private Vector3 toPos = new Vector3(0.05f, 0.03f, -0.11f)- new Vector3(0, 0.01f, 0);
    private Vector3 fromPos2 = new Vector3(0.01f, 0.02f, -0.16f) - new Vector3(0, 0.02f, 0);
    private Vector3 toPos2 = new Vector3(0.01f, 0.03f, -0.11f) - new Vector3(0, 0.01f, 0);

    public GameObject targetGO;

    void Start()
    {
        //Debug.Log("start");
        string jsonFilePath = "/Users/asisstenz/Desktop/PersistantDataPath/ZurichEnergyTrade.json";//Application.persistentDataPath + "/PersistantFilePath/" + "EnergyTradeSingapore.json";
        //Application.persistentDataPath + "/PersistantFilePath/" + "ZurichEnergyTrade.json";

        LoadJson(jsonFilePath);        

        maxTrans = dataList.Max(item => item.Item3);
        startTime = dataList.Min(item => item.Item4);
        endTime = dataList.Max(item => item.Item4);

        ArrowWidthScale = Vector3.Distance(AnchorpointXZ_1.transform.position, AnchorpointXZ_2.transform.position);
        height = Vector3.Distance(AnchorpointXZ_2.transform.position, AnchorpointXY.transform.position);
        //Debug.Log("height" + height);
        //StartCoroutine(ContinuousLaunchLineRenderer(fromPos, toPos, 2, targetGO));
        ///StartCoroutine(ContinuousLaunchLineRenderer(fromPos2, toPos2, 11, targetGO));
        StartCoroutine(StartLaunchLineRenderers());
    }

    private IEnumerator StartLaunchLineRenderers()
    {
        foreach (var item in dataList)
        {
            string fromBuildingName = item.Item1;
            string toBuildingName = item.Item2;
            float transmissionValue = item.Item3;
            int T = item.Item4;
            ShowTime(T);

            GameObject toBDgo = GameObject.Find(toBuildingName);

            Transform fromBuilding = GameObject.Find(fromBuildingName).GetComponentInChildren<Canvas>().transform;
            Debug.Log("from building position "+fromBuilding.position);
            Transform toBuilding = GameObject.Find(toBuildingName).GetComponentInChildren<Canvas>().transform;
            Debug.Log("to building position" + toBuilding.position);

            Vector3 fromPosition = fromBuilding.position;// +gameObject.transform.position;
            Vector3 toPosition = toBuilding.position;// +gameObject.transform.position;
            targetGO.GetComponent<Renderer>().material.color = Color.white;

            if (transmissionValue > 0)
            {
                yield return StartCoroutine(LaunchLineRenderer(fromPosition, toPosition, transmissionValue, toBDgo));
                //yield return StartCoroutine(ContinuousLaunchLineRenderer(fromPosition, toPosition, transmissionValue, toBDgo));
            }
        }
    }

    public IEnumerator ContinuousLaunchLineRenderer(Vector3 startPoint, Vector3 endPoint, float transmissionValue, GameObject toBDgo)
    {
        int i = 0;
        while (i<5)//(true)//i < 5)
        {
            StartCoroutine(LaunchLineRenderer(startPoint, endPoint, transmissionValue, toBDgo));

            // Wait for 2 seconds before launching the next set of LineRenderers
            yield return new WaitForSeconds(5.0f);
            i++;
        }
    }

    public IEnumerator LaunchLineRenderer(Vector3 startPoint, Vector3 endPoint, float transmissionValue, GameObject targetGO)
    {
        lineRenderers = new LineRenderer[numberOfBombs];
        trajectoryPoints = CalculateParabolicTrajectory(startPoint, endPoint, height, numberOfBombs + 1);

        Color originalTargetColor = targetGO.GetComponent<Renderer>().material.color;

        Debug.Log("start point" + startPoint);
        for (int i = 0; i < numberOfBombs; i++)
        {
            //GameObject lineRendererObject = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
            GameObject lineRendererObject = Instantiate(lineRendererPrefab, gameObject.transform.position, Quaternion.identity, gameObject.transform);

            lineRenderers[i] = lineRendererObject.GetComponent<LineRenderer>();
            lineRenderers[i].positionCount = 2;
            lineRenderers[i].SetPosition(0, trajectoryPoints[i]);
            lineRenderers[i].SetPosition(1, trajectoryPoints[i + 1]);
            lineRenderers[i].startWidth = transmissionValue / maxTrans * ArrowWidthScale;
            lineRenderers[i].endWidth = 0f;
            Debug.Log("trajectory point i = " + trajectoryPoints[i]);
            Debug.Log("trajectory point i+1 = " + trajectoryPoints[i+1]);
            //lineRenderers[i].transform.parent = gameObject.transform;
            //lineRenderers[i].SetPosition(0, trajectoryPoints[i] + gameObject.transform.position);
            //lineRenderers[i].SetPosition(1, trajectoryPoints[i + 1] + gameObject.transform.position);
        


            if (i == (numberOfBombs - 1))
            {
                // Set the color and text of targetGO
                targetGO.GetComponent<Renderer>().material.color = Color.yellow;
                targetGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = "+" + transmissionValue.ToString() + "kWh";
            }

            yield return new WaitForSeconds((float)timeDifference);
            Destroy(lineRenderers[i], prefabStayTime);
        }
        Debug.Log("end point " + endPoint);
        
        // Wait for 2 seconds
        yield return new WaitForSeconds(2.0f);

        // Reset the color and text of targetGO
        targetGO.GetComponent<Renderer>().material.color = originalTargetColor;
        targetGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = "";
    }


    public IEnumerator ARCHIVELaunchLineRenderer(Vector3 startPoint, Vector3 endPoint, float transmissionValue, GameObject targetGO)
    {
        lineRenderers = new LineRenderer[numberOfBombs];
        trajectoryPoints = CalculateParabolicTrajectory(startPoint, endPoint, height, numberOfBombs + 1);

        Color originalTargetColor = targetGO.GetComponent<Renderer>().material.color;
        int linesRemaining = numberOfBombs; // Track the number of line renderers remaining


        for (int i = 0; i < numberOfBombs; i++)
        {
            GameObject lineRendererObject = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
            lineRenderers[i] = lineRendererObject.GetComponent<LineRenderer>();
            lineRenderers[i].positionCount = 2;
            lineRenderers[i].SetPosition(0, trajectoryPoints[i]);
            lineRenderers[i].SetPosition(1, trajectoryPoints[i + 1]);
            lineRenderers[i].startWidth = transmissionValue / maxTrans * ArrowWidthScale;// *0.02f;//0.001f;// 
            lineRenderers[i].endWidth = 0f;
            //Debug.Log("trajecyory point i = "+ i +", " +trajectoryPoints[i]);
            //Debug.Log("trajectory point i+1 = " + trajectoryPoints[i+1]);
            lineRenderers[i].transform.parent = gameObject.transform;

            if (i == (numberOfBombs - 1))
            {
                //Debug.Log("change color and text ");
                targetGO.GetComponent<Renderer>().material.color = Color.yellow;
                targetGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = "+" + transmissionValue.ToString();
            }

            StartCoroutine(DestroyLineRenderer(lineRenderers[i], prefabStayTime, () =>
            {
                linesRemaining--; // Decrement lines remaining when a line renderer is destroyed
                if (linesRemaining == 0)
                {
                    // All line renderers are destroyed, reset the color and text
                    targetGO.GetComponent<Renderer>().material.color = originalTargetColor;
                    targetGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = "";
                }
            }));

            yield return new WaitForSeconds((float)timeDifference);
            //Destroy(lineRenderers[i], prefabStayTime);
        }
       // targetGO.GetComponent<Renderer>().material.color = originalTargetColor;
       // targetGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = "";

    }
    private IEnumerator DestroyLineRenderer(LineRenderer lineRenderer, float delay, Action onComplete)
    {
        yield return new WaitForSeconds(delay);
        Destroy(lineRenderer.gameObject);
        onComplete?.Invoke();
    }

    Vector3[] CalculateParabolicTrajectory(Vector3 startPoint, Vector3 endPoint, float height, int numberOfPoints)
    {
        Vector3[] points = new Vector3[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i / (float)(numberOfPoints - 1);
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float ut = u * t;

            float z = (height * (1 - uu)+ startPoint.z); // -height *2)
            if (i < numberOfPoints / 2)
            {
                z = (height * (1 - tt) + startPoint.z); // -height *2
            }
            // Calculate the Y-coordinate differently for the X-Z plane

            // Calculate the X and Z coordinates normally
            float x = uu * startPoint.x + 2 * ut * endPoint.x + tt * endPoint.x;
            float y = (uu * startPoint.y + 2 * ut * endPoint.y + tt * endPoint.y);// *(-1);

            points[i] = new Vector3(x, y, z);
        }

        return points;
    }

    Vector3[] YZORIGINALCalculateParabolicTrajectory(Vector3 startPoint, Vector3 endPoint, float height, int numberOfPoints)
    {
        Vector3[] points = new Vector3[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i / (float)(numberOfPoints - 1);
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float ut = u * t;

            float y = height * (1 - tt) + startPoint.y;
            if (i < numberOfPoints / 2)
            {
                y = height * (1 - uu) + startPoint.y;
            } 
                // Calculate the Y-coordinate differently for the X-Z plane        
            
            // Calculate the X and Z coordinates normally
            float x = uu * startPoint.x + 2 * ut * endPoint.x + tt * endPoint.x;
            float z = uu * startPoint.z + 2 * ut * endPoint.z + tt * endPoint.z;

            points[i] = new Vector3(x, y, z);
        }

        return points;
    }

    public void ShowTime(int T)
    {
        timelineText.text = T.ToString() + "h";
        ///if (T >= 12)
        //{
        //    timelineText.text = (T-12).ToString() + "PM";
        //}
        progressBar.minValue = 0f;
        progressBar.maxValue = (float)(endTime - startTime);
        progressBar.value = (float)(T- startTime);//int to float
        Debug.Log("time now: " + T);
    }


    Vector3[] ARCHIVECalculateParabolicTrajectory(Vector3 startPoint, Vector3 endPoint, float height, int numberOfPoints)
    {
        Vector3[] points = new Vector3[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i / (float)(numberOfPoints - 1);
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float ut = u * t;

            float x = uu * startPoint.x + 2 * ut * height + tt * endPoint.x;
            float y = uu * startPoint.y + 2 * ut * height + tt * endPoint.y;
            float z = uu * startPoint.z + 2 * ut * height + tt * endPoint.z;

            points[i] = new Vector3(x, y, z);
        }

        return points;
    }

    public void LoadJson(string path)
    {
        using (StreamReader r = new StreamReader(path))
        {
            string jsonString = r.ReadToEnd();

            rootDataEnergyTrade rootdataEnergyTrade = JsonUtility.FromJson<rootDataEnergyTrade>(jsonString);

            foreach (DataEnergyTrade et in rootdataEnergyTrade.DataEnergyTrade)
            {
                string FromBuilding = RemoveDemandCSV(et.From);
                string ToBuilding = RemoveDemandCSV(et.To);
                float transmission = et.Transmission;
                int T = et.T;
                
                    dataList.Add((FromBuilding, ToBuilding, transmission, T));
            }
        }
    }

    private string RemoveDemandCSV(string text)
    {
        if (text.EndsWith("_demand.csv"))
        {
            return text.Substring(0, text.Length - "_demand.csv".Length);
        }
        return text;
    }

}
