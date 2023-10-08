using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using TMPro;
//REMEMBER TO ADD SCROLLBAR TIMELINE!

public class EnergyTrade : MonoBehaviour
{
    public GameObject lineRendererPrefab;
    private float height = 0.02F;
    private int numberOfBombs = 10;
    private float timeDifference = 0.2f;
    private float prefabStayTime = 1f;
    private LineRenderer[] lineRenderers;
    private Vector3[] trajectoryPoints;

    //private GameObject targetGO;

    List<(string, string, float, int)> dataList = new List<(string, string, float, int)>();
    public float maxTrans;

    private Vector3 fromPos = new Vector3(0.04f, 0.02f, -0.16f)- new Vector3(0, 0.02f, 0);

    private Vector3 toPos = new Vector3(0.05f, 0.03f, -0.11f)- new Vector3(0, 0.01f, 0);


    private Vector3 fromPos2 = new Vector3(0.01f, 0.02f, -0.16f) - new Vector3(0, 0.02f, 0);

    private Vector3 toPos2 = new Vector3(0.01f, 0.03f, -0.11f) - new Vector3(0, 0.01f, 0);

    public GameObject targetGO;

    void Start()
    {
        //Debug.Log("start");
        string jsonFilePath = Application.persistentDataPath + "/PersistantFilePath/" + "ZurichEnergyTrade.json";

        LoadJson(jsonFilePath);
        

        maxTrans = dataList.Max(item => item.Item3);

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

            GameObject toBDgo = GameObject.Find(toBuildingName);

            Transform fromBuilding = GameObject.Find(fromBuildingName).GetComponentInChildren<Canvas>().transform;
            Transform toBuilding = GameObject.Find(toBuildingName).GetComponentInChildren<Canvas>().transform;

            Vector3 fromPosition = fromBuilding.position;
            Vector3 toPosition = toBuilding.position;

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

        for (int i = 0; i < numberOfBombs; i++)
        {
            GameObject lineRendererObject = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
            lineRenderers[i] = lineRendererObject.GetComponent<LineRenderer>();
            lineRenderers[i].positionCount = 2;
            lineRenderers[i].SetPosition(0, trajectoryPoints[i]);
            lineRenderers[i].SetPosition(1, trajectoryPoints[i + 1]);
            lineRenderers[i].startWidth = transmissionValue / maxTrans *0.02f;//0.001f;// 
            lineRenderers[i].endWidth = 0f;
            Debug.Log("trajecyory point i = "+ i +", " +trajectoryPoints[i]);
            Debug.Log("trajectory point i+1 = " + trajectoryPoints[i+1]);

            if (i == (numberOfBombs - 1))
            {
                Debug.Log("change color and text ");
                targetGO.GetComponent<Renderer>().material.color = Color.yellow;
                targetGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = "+" + transmissionValue.ToString();

            }

            yield return new WaitForSeconds((float)timeDifference);
            Destroy(lineRenderers[i], prefabStayTime);

        }
        targetGO.GetComponent<Renderer>().material.color = originalTargetColor;
        targetGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = "";

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
