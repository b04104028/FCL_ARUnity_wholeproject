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
    private float timeDifference = 0.1f;
    //private float prefabStayTime = 2f;//0.8f;
    private LineRenderer[] lineRenderers;
    private List<LineRenderer[]> lineRenderersList = new List<LineRenderer[]>();
    private Vector3[] trajectoryPoints;

    //progress bar timeline
    [SerializeField] public TextMeshProUGUI timelineText;
    [SerializeField] public Slider progressBar;
    int startTime = 11;//default value for zurich case
    int endTime = 16;//default value for zurich case


    //private GameObject targetGO;

    List< List<(string, string, float, int)>> dataList = new List<List<(string, string, float, int)>>();
    public float maxTrans = 0;

    /*//THESE POINTS ARE FOR TESTING 
    private Vector3 fromPos = new Vector3(0.04f, 0.02f, -0.16f)- new Vector3(0, 0.02f, 0);
    private Vector3 toPos = new Vector3(0.05f, 0.03f, -0.11f)- new Vector3(0, 0.01f, 0);
    private Vector3 fromPos2 = new Vector3(0.01f, 0.02f, -0.16f) - new Vector3(0, 0.02f, 0);
    private Vector3 toPos2 = new Vector3(0.01f, 0.03f, -0.11f) - new Vector3(0, 0.01f, 0);
    */
    public GameObject targetGO;

    private Color lightBlue = new Color(0.2f, 0.5f, 1.0f, 1.0f);

    void Start()
    {
        //Debug.Log("start");
        //string jsonFilePath = "/Users/asisstenz/Desktop/PersistantDataPath/ZurichEnergyTrade.json";//Application.persistentDataPath + "/PersistantFilePath/" + "EnergyTradeSingapore.json";
        string jsonFilePath = Application.persistentDataPath + "/PersistantFilePath/" + "ZurichEnergyTrade.json";

        LoadJson(jsonFilePath);                

        ArrowWidthScale = Vector3.Distance(AnchorpointXZ_1.transform.position, AnchorpointXZ_2.transform.position);
        height = Vector3.Distance(AnchorpointXZ_2.transform.position, AnchorpointXY.transform.position);
        //Debug.Log("height" + height);
        //StartCoroutine(ContinuousLaunchLineRenderer(fromPos, toPos, 2, targetGO));
        ///StartCoroutine(ContinuousLaunchLineRenderer(fromPos2, toPos2, 11, targetGO));
        StartCoroutine(StartLaunchLineRenderers());
    }

    public void LoadJson(string path)
    {
        using (StreamReader r = new StreamReader(path))
        {
            string jsonString = r.ReadToEnd();

            rootDataEnergyTrade rootdataEnergyTrade = JsonUtility.FromJson<rootDataEnergyTrade>(jsonString);

            // Group items by T using LINQ
            var groupedData = rootdataEnergyTrade.DataEnergyTrade
                .Select(et => new
                {
                    FromBuilding = RemoveDemandCSV(et.From),
                    ToBuilding = RemoveDemandCSV(et.To),
                    Transmission = et.Transmission,
                    T = et.T
                })
                .GroupBy(item => item.T)
                .ToList();

            // Clear the existing dataList
            dataList.Clear();

            // Iterate through the groups and aggregate the items
            foreach (var group in groupedData)
            {
                // Aggregate the items in each group into a new list
                var aggregatedList = group.Select(item => (item.FromBuilding, item.ToBuilding, item.Transmission, item.T)).ToList();

                // Add the aggregated list to dataList
                dataList.Add(aggregatedList);
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

    public void ShowTime(int T)
    {
        timelineText.text = T.ToString() + "h";
        ///if (T >= 12)
        //{
        //    timelineText.text = (T-12).ToString() + "PM";
        //}
        progressBar.minValue = 0f;
        progressBar.maxValue = (float)(endTime - startTime);
        progressBar.value = (float)(T - startTime);//int to float
        Debug.Log("time now: " + T);
    }

    private IEnumerator StartLaunchLineRenderers()
    {
        foreach (var grouplist in dataList)
        {
            foreach (var item in grouplist)
            {
                string fromBuildingName = item.Item1;
                string toBuildingName = item.Item2;
                float transmissionValue = item.Item3;
                int T = item.Item4;
                ShowTime(T);

                if(maxTrans < grouplist.Max(item => item.Item3))
                {
                    maxTrans = grouplist.Max(item => item.Item3);
                }
                if (startTime > grouplist.Min(item => item.Item4))
                {
                    startTime = grouplist.Min(item => item.Item4);
                }
                if (endTime < grouplist.Max(item => item.Item4))
                {
                    endTime = grouplist.Max(item => item.Item4);
                }

                GameObject toBDgo = GameObject.Find(toBuildingName);
                GameObject fromBDgo = GameObject.Find(fromBuildingName);

                Transform fromBuilding = GameObject.Find(fromBuildingName).GetComponentInChildren<Canvas>().transform;
                Debug.Log("from building position " + fromBuilding.position);
                Transform toBuilding = GameObject.Find(toBuildingName).GetComponentInChildren<Canvas>().transform;
                Debug.Log("to building position" + toBuilding.position);

                Vector3 fromPosition = fromBuilding.position;// +gameObject.transform.position;
                Vector3 toPosition = toBuilding.position;// +gameObject.transform.position;
                targetGO.GetComponent<Renderer>().material.color = Color.white;

                if (transmissionValue > 0)
                {
                    //TEMPChangeBuildingColor(fromPosition, toPosition, transmissionValue, toBDgo, fromBDgo, T);
                    yield return StartCoroutine(TEMPChangeBuildingColor(fromPosition, toPosition, transmissionValue, toBDgo, fromBDgo, T));
                    //yield return StartCoroutine(ContinuousLaunchLineRenderer(fromPosition, toPosition, transmissionValue, toBDgo));
                }
            }

            yield return new WaitForSeconds(2.0f);

            foreach (var item in grouplist) {
                string fromBuildingName = item.Item1;
                string toBuildingName = item.Item2;

                GameObject toBDgo = GameObject.Find(toBuildingName);
                GameObject fromBDgo = GameObject.Find(fromBuildingName);

                // Reset the color and text of targetGO
                toBDgo.GetComponent<Renderer>().material.color = Color.white;
                toBDgo.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = "";

                // Reset the color and text of fromGO
                fromBDgo.GetComponent<Renderer>().material.color = Color.white;
                fromBDgo.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = "";
            }

            foreach (LineRenderer[] lrgroup in lineRenderersList)
            {
                foreach (LineRenderer lr in lrgroup)
                {
                    Destroy(lr);
                }
            }

        }
    }

    public IEnumerator TEMPChangeBuildingColor(Vector3 startPoint, Vector3 endPoint, float transmissionValue, GameObject targetGO, GameObject fromGO, int T)
    {
        lineRenderers = new LineRenderer[numberOfBombs];
        trajectoryPoints = CalculateStraightTrajectory(startPoint, endPoint, height, numberOfBombs + 1);

        //Color originalTargetColor = targetGO.GetComponent<Renderer>().material.color;
        //Color originalFromColor = fromGO.GetComponent<Renderer>().material.color;

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
            Debug.Log("trajectory point i+1 = " + trajectoryPoints[i + 1]);
            //lineRenderers[i].transform.parent = gameObject.transform;
            //lineRenderers[i].SetPosition(0, trajectoryPoints[i] + gameObject.transform.position);
            //lineRenderers[i].SetPosition(1, trajectoryPoints[i + 1] + gameObject.transform.position);

            if(i == 0)
            {
                fromGO.GetComponent<Renderer>().material.color = lightBlue;// Color.blue;
                fromGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = "-" + transmissionValue.ToString() + "kWh";
            }

            if (i == (numberOfBombs - 1))
            {
                // Set the color and text of targetGO
                targetGO.GetComponent<Renderer>().material.color = Color.yellow;
                targetGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = "+" + transmissionValue.ToString() + "kWh";
            }

            yield return new WaitForSeconds((float)timeDifference);
            //Destroy(lineRenderers[i], prefabStayTime);
        }

        lineRenderersList.Add(lineRenderers);
        Debug.Log("end point " + endPoint);

        // Wait for 2 seconds
        //if (currentT != T)
        //{
            //yield return new WaitForSeconds(2.0f);
           // Debug.Log("Wait for next hour");
           // currentT = T;
        //}

        // Reset the color and text of targetGO
        //targetGO.GetComponent<Renderer>().material.color = originalTargetColor;
        //targetGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = "";

        // Reset the color and text of fromGO
        //fromGO.GetComponent<Renderer>().material.color = originalFromColor;
        //fromGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = "";
    }

    Vector3[] CalculateStraightTrajectory(Vector3 startPoint, Vector3 endPoint, float height, int numberOfPoints)
    {
        Vector3[] points = new Vector3[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            double y = ((endPoint.y - startPoint.y) / (float)numberOfBombs * (float)i) + startPoint.y;
            double x = ((endPoint.x - startPoint.x) / (float)numberOfBombs * (float)i) + startPoint.x;
            double z = ((endPoint.z - startPoint.z) / (float)numberOfBombs * (float)i) + startPoint.z;

            points[i] = new Vector3((float)x, (float)y, (float)z);
            Debug.Log(",  i= " + i + ", calculated points: " + points[i]);
        }
        Debug.Log("in trajectory funciton, startpoint: " + startPoint + ", endpoint: " + endPoint + ", out put point i=0 : " + points[0] + ", output point i = 10: " + points[10]);

        return points;
    }

    /*public IEnumerator ContinuousLaunchLineRenderer(Vector3 startPoint, Vector3 endPoint, float transmissionValue, GameObject toBDgo)
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
            double t = i / (numberOfPoints - 1);
            double  u = 1 - t;
            double tt = t * t;
            double uu = u * u;
            double ut = u * t;
            Debug.Log("t = " + t);

            double z = (height * (1 - uu)+ startPoint.z); // -height *2)
            if (i < numberOfPoints / 2)
            {
                z = (height * (1 - tt) + startPoint.z); // -height *2
            }
            // Calculate the Y-coordinate differently for the X-Z plane

            // Calculate the X and Z coordinates normally
            double x = uu * startPoint.x + 2 * ut * endPoint.x + tt * endPoint.x;
            double y = (uu * startPoint.y + 2 * ut * endPoint.y + tt * endPoint.y);// *(-1);

            points[i] = new Vector3((float)x, (float)y, (float)z);
        }

        Debug.Log("in trajectory funciton, startpoint: " + startPoint + ", endpoint: " + endPoint + ", out put point i=0 : " + points[0] + ", output point i = 9: " + points[9]);
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

    public void ARCHIVELoadJson(string path)
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

                //temporarily comment out this to use new loadjson function
                //dataList.Add((FromBuilding, ToBuilding, transmission, T));
            }
        }
    }*/

}
