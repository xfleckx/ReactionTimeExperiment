using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;

public class CSVFileWriter : MonoBehaviour
{

    public string fileName = "data.dat";
    public string delimiter = ";";

    private StreamWriter fs;

    // Use this for initialization
    void Start()
    {
        fs = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite));
        
    }

    public void Write(string[] values)
    {
        fs.WriteLine(string.Join(delimiter, values));
        
    }

    public void WriteEverythingToFile()
    {
        fs.Flush();
    }

    public void OnDestroy()
    {
        if (fs != null)
        {
            WriteEverythingToFile();
            fs.Close();
        }
    }
}
