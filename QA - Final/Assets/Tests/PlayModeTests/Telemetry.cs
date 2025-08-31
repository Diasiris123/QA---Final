using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Telemetry
{
    private static readonly List<string> _events = new();
    public static void Log(string evt) => _events.Add($"{Time.realtimeSinceStartup:F2},{evt}");
    public static string SaveToCSV()
    {
        var path = Path.Combine(Application.persistentDataPath, $"ux_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv");
        File.WriteAllLines(path, _events);
        Debug.Log("Saved UX log: " + path);
        return path;
    }
    public static void Clear() => _events.Clear();
}