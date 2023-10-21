/*ARCHIVE FOR NO USE*/

using UnityEngine;
using System.Collections;

public class EnergyTradeLineRenderer : MonoBehaviour
{
    public GameObject lineRendererPrefab;
    public Transform startPoint { get; set; }
    public Transform endPoint { get; set; }

    public GameObject targetGO { get; set; }
    private float height = 0.15f;
    private int numberOfBombs = 10;
    public float launchDuration = 2.0f;
    private float timeDifference = 0.2f;
    private float prefabStayTime = 1.5f;

    private LineRenderer[] lineRenderers;
    private Vector3[] trajectoryPoints;



    void Start()
    {
        //StartCoroutine(ContinuousLaunchLineRenderer(startPoint, endPoint));
    }

    public IEnumerator ContinuousLaunchLineRenderer(Transform startPoint, Transform endPoint)
    {
        while (true)
        {
            Debug.Log("call continuous launch line renderer");
            StartCoroutine(LaunchLineRenderer(startPoint, endPoint));

            // Wait for 2 seconds before launching the next set of LineRenderers
            yield return new WaitForSeconds(3.0f);
        }
    }
    public IEnumerator LaunchLineRenderer(Transform startPoint, Transform endPoint)
    {
        Debug.Log("in lauch line renderer");
        lineRenderers = new LineRenderer[numberOfBombs];
        trajectoryPoints = CalculateParabolicTrajectory(startPoint.position, endPoint.position, height, numberOfBombs + 1);

        Color originalTargetColor = targetGO.GetComponent<Renderer>().material.color;

        for (int i = 0; i < numberOfBombs; i++)
            {
                GameObject lineRendererObject = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
                lineRenderers[i] = lineRendererObject.GetComponent<LineRenderer>();
                lineRenderers[i].positionCount = 2;
                lineRenderers[i].SetPosition(0, trajectoryPoints[i]);
                lineRenderers[i].SetPosition(1, trajectoryPoints[i + 1]);

                Debug.Log("trajectory point i = " + trajectoryPoints[i]);
                Debug.Log("trajectory point i+1 = " + trajectoryPoints[i + 1]);
            //lineRenderers[i].SetPositions(new Vector3[] { trajectoryPoints[i], trajectoryPoints[i] });
            //Debug.Log("line renderer object: " + lineRendererObject.ToString());
            //Debug.Log("line renderer position: " + lineRenderers[i].transform.position.ToString());

            if (i ==  (numberOfBombs-1))
            {
                targetGO.GetComponent<Renderer>().material.color = Color.yellow;
            }

            // Wait for the specified time difference
            yield return new WaitForSeconds((float)timeDifference);

            Destroy(lineRenderers[i], prefabStayTime);
            }
        targetGO.GetComponent<Renderer>().material.color = originalTargetColor;

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

            float x = uu * startPoint.x + 2 * ut * height + tt * endPoint.x;
            float y = uu * startPoint.y + 2 * ut * height + tt * endPoint.y;
            float z = uu * startPoint.z + 2 * ut * height + tt * endPoint.z;

            points[i] = new Vector3(x, y, z);
            //Debug.Log("interpolate point: " + points[i]);
        }

        return points;
    }
}
