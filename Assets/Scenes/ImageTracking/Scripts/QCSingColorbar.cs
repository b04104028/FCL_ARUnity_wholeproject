using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QCSingColorbar : MonoBehaviour
{
    public List<GameObject> colorbarlist = new List<GameObject>();
    [SerializeField] public QCSingColorManager _colormanager;
    //IDictionary<string, double> HouseLoadDict = new Dictionary<string, double>();

    // Start is called before the first frame update
    void Start()
    {
        SetColorbar();
        double maxloadrounded = RoundToNearest(_colormanager.max_load);
        LoadOnText(maxloadrounded);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetColorbar()
    {
        int i = 0;
        Color deepblue = new Color(5f, 11f, 106f);

        foreach (GameObject colorbar in colorbarlist)
        {
            float lerp = i * 0.2f;

            if (lerp <= 1)
            {
                colorbar.GetComponent<Image>().color = Color.Lerp(Color.black, Color.blue, lerp);
            }
            else if (lerp > 1)
            {
                lerp = 0.2f * (i - 5);
                colorbar.GetComponent<Image>().color = Color.Lerp(Color.blue, Color.white, lerp);
            }

            //Debug.Log("lerp = "+lerp + ",  color = " + colorbar.GetComponent<Image>().color);
            i++;
        }
    }
    public void LoadOnText(double maxloadrounded)
    {
        int i = 0;
        foreach (GameObject colorbar in colorbarlist)
        {
            double section_i = ((double)(colorbarlist.Count - i) / (double)colorbarlist.Count) * maxloadrounded;
            // Debug.Log("colorbar number - i = " + ((double)(colorbarlist.Count - i)/(double)colorbarlist.Count).ToString() + ", section i = " + section_i);
            colorbar.GetComponentInChildren<TextMeshProUGUI>().text = Convert.ToInt32(section_i).ToString();
            i++;
        }
    }
    static double RoundToNearest(double num)
    {
        // Find the order of magnitude (power of 10)
        int orderOfMagnitude = (int)Math.Floor(Math.Log10(num));

        // Calculate the nearest rounded number
        double rounded = Math.Round(num / Math.Pow(10, orderOfMagnitude - 1)) * Math.Pow(10, orderOfMagnitude - 1);

        return rounded;
    }
}