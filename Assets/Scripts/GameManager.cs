using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager manager;

    private void Awake()
    {
        if(manager == null)
        {
            DontDestroyOnLoad(gameObject);
            manager = this;
        }
        else if(manager != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 30), "Manager GUI");
        if(GUI.Button(new Rect(10, 40, 100, 30), "Save"))
        {
            Debug.Log("Save clicked");
            Save();
        }
        if(GUI.Button(new Rect(10, 80, 100, 30), "Load"))
        {
            Debug.Log("Load clicked");
            Load();
        }
        if(GUI.Button(new Rect(10, 120, 100, 30), "Empty Button"))
        {
            Debug.Log("Empty Button Clicked");
        }
    }

    public void Save()
    {
        string path = Application.persistentDataPath + "/save.dat";
        Debug.Log("Saving to " + path);
        FileStream file = File.Create(path);

        SaveData data = new SaveData();
        data.aNumber = 42;
        data.aString = "foo";
        data.aFloatArray = new float[] { 0.2f, 1.45f, -0.4f };
        data.aDictionary = new Dictionary<string, string>();
        data.aDictionary["foo"] = "bar";

        string s = JsonUtility.ToJson(data);
        StreamWriter writer = new StreamWriter(file);
        writer.Write(s);
        writer.Flush();
        file.Close();
    }

    public void Load()
    {
        string path = Application.persistentDataPath + "/save.dat";
        if(File.Exists(path))
        {
            Debug.Log("Loading from " + path);
            FileStream file = File.Open(path, FileMode.Open);
            StreamReader reader = new StreamReader(file);
            string s = reader.ReadToEnd();
            reader.Close();
            file.Close();
            Debug.Log("Read file contents: " + s);
            SaveData data = JsonUtility.FromJson<SaveData>(s);
            Debug.Log("Deserialized aNumber=" + data.aNumber + ", aString=\"" + data.aString + "\", aFloatArray.Length=" + data.aFloatArray.Length);
        }
    }
}

class SaveData
{
    public int aNumber;
    public string aString;
    public float[] aFloatArray;
    public Dictionary<string, string> aDictionary;
}