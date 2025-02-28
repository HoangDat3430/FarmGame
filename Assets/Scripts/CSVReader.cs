using System.IO;
using UnityEngine;

public abstract class CSVReader
{
    public virtual void ReadFile(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(path))
        {
            string [] lines = File.ReadAllLines(path);
            OnClear(lines.Length-1);
            for (int i = 1; i < lines.Length; i++) // skip header
            {
                InitRowData(i-1, lines[i].Split(','));
            }
        }
        else
        {
            Debug.LogError("CSV file not found in StreamingAssets!");
        }
    }
    public virtual void InitRowData(int line, string[] data) 
    { 
    }
    public virtual void OnClear(int lineCount)
    {
        
    }
}
