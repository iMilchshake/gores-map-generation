using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class DebugUtils
{
    public static void LogTime(string message)
    {
        Debug.Log($"[{Time.time}] {message}");
    }
}