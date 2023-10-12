using UnityEngine;
//using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using Unity.VisualScripting;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using System;
using System.Globalization;
using TMPro;
using System.Reflection;
using UnityEngine.UIElements;
using System.Linq;

public class EColorManager : MonoBehaviour
{
    public Renderer rend;
    public GameObject this_house;


    //data
    public double load;
    public double max_load;// 17044.088//actual max load = 339000.++++2;
    public double min_load;// = 0.0;

    //List<float> HouseLoadList = new List<float>();
    IDictionary<string, double> HouseLoadDict = new Dictionary<string, double>();

    public void Start()
    {
        string path = Application.persistentDataPath + "/PersistantFilePath/" + "Zurich_E_total.json";//"ExampleJSON.json";
        //Debug.Log("path: " + Application.persistentDataPath);
        //path: /Users/gast/Library/Application Support/Unity Technologies/AR Foundation Samples
        rend = GetComponent<Renderer>();
        LoadJson(path, HouseLoadDict);
        //ShowLoad(HouseLoadDict);
        max_load = GetMaxLoad(HouseLoadDict);
        min_load = GetMinLoad(HouseLoadDict);
        //Debug.Log("MAX LOAD:" + max_load);//MAX LOAD:17044.088
        //Debug.Log("MIN LOAD: " + min_load);//0
        AssignLoad(HouseLoadDict);
        //ShowLoad(HouseLoadDict);
        AssignColor();
        LoadOnText(load);

    }

    public void Update()
    { }
    public void AssignColor()
    {
        //Color[] _colors = new Color[] { Color.blue, Color.cyan, Color.green, Color.yellow, new Color(0.2F, 0.3F, 0.4F), Color.red };
        //float range = (float) 1 / (_colors.Length-1);//why not work?
        float lerp = (float)((load - min_load) / (max_load - min_load));
        float alpha = 0.5f;
        Color transparentWhite = new Color(Color.white.r, Color.white.g, Color.white.b, alpha);
        Color transparentBlue = new Color(Color.blue.r, Color.blue.g, Color.blue.b, alpha);
        //Color deepblue = new Color(5f,11f,106f);
        //Color transparentRed = //new Color(Color.deepblue.r, Color.red.g, Color.red.b, alpha);

        switch (lerp)
        {
            case <= 0.2f:
                rend.material.color = Color.Lerp(Color.white, Color.green, (float)(lerp / 0.2f));
                break;
            case <= 1.0f:
                rend.material.color = Color.Lerp(Color.green, Color.black, (float)((lerp - 0.2f) / 0.8f));
                break;
            case > 1.0f:
                rend.material.color = Color.black;
                break;
        }
        //Debug.Log("house " + gameObject.name + "'s PR = " + lerp.ToString() + ", and its real load= " + load.ToString());
        //rend.material.color = Color.Lerp(colors[index], colors[index + 1], t);

        //rend.material.color = Color.Lerp(new Color32(255,255,0,170), new Color32(254, 73, 0, 255), lerp);  
    }
    //it works wellhttps://stackoverflow.com/questions/36239705/serialize-and-deserialize-json-and-json-array-in-unity
    public void LoadJson(string path, IDictionary<string, double> HouseLoadDict)
    {
        using (StreamReader r = new StreamReader(path))
        {//this path also works: Application.dataPath + "/Scenes/ImageTracking/Prefabs/" + "ExampleJSON.json"
            //Debug.Log("used path to load json: " + path);
            //used path to load json: /Users/gast/Library/Application Support/Unity Technologies/AR Foundation Samples/PersistantFilePath/Zurich_QH_total.json

            string jsonString = r.ReadToEnd();
            //Debug.Log(jsonString);
            //try
            //{
                Root jsonroot = JsonUtility.FromJson<Root>(jsonString);
                //Debug.Log(jsonroot.houses);
                foreach (House h in jsonroot.houses)
                {
                    //Debug.Log(h);
                    //assume json file provided the data in string, so covertion needed.                  
                    NumberFormatInfo provider = new NumberFormatInfo();
                    provider.NumberDecimalSeparator = ".";
                    double load_double = Convert.ToDouble(h.load, provider);
                    //Debug.Log(load_double.ToString());
                    HouseLoadDict.Add(h.id, load_double);
                }
           // }
            //catch (Exception e)
            //{
              //  Debug.Log("cant read json");
           // }
        }
    }
    public double GetMaxLoad(IDictionary<string, double> HouseLoadDict)
    {
        return HouseLoadDict.Aggregate((x, y) => x.Value > y.Value ? x : y).Value;
    }
    public double GetMinLoad(IDictionary<string, double> HouseLoadDict)
    {
        return HouseLoadDict.Aggregate((x, y) => x.Value < y.Value ? x : y).Value;
    }
    public void ShowLoad(IDictionary<string, double> HouseLoadDict)
    {
        foreach (KeyValuePair<string, double> kvp in HouseLoadDict)
        {
            Debug.Log("house " + kvp.Key + "'s load: " + kvp.Value);
        }
    }
    public void AssignLoad(IDictionary<string, double> HouseLoadDict)
    {
        foreach (KeyValuePair<string, double> kvp in HouseLoadDict)
        {
            if (kvp.Key == gameObject.name)
            {
                //Debug.Log("house key "+ kvp.Key + "match the house named " + gameObject.name);
                //load = float.Parse(kvp.Value, CultureInfo.InvariantCulture.NumberFormat);
                load = kvp.Value;
                //Debug.Log("load of " + kvp.Key + "= "+ load.ToString());
            }
        }
        /*
        for (int index = 1001; index < HouseLoadDict.Count; index++)
        {
            string label = "B" + index.ToString();
            if (label == gameObject.name)
            {
                load = HouseLoadDict[index];
            }
        }*/
    }
    public void LoadOnText(double load)
    {
        Canvas _canvas = gameObject.GetComponentInChildren<Canvas>();
        if (_canvas != null)
        {
            _canvas.GetComponentInChildren<TextMeshProUGUI>().text = Convert.ToInt32(load).ToString();
        }
    }

}