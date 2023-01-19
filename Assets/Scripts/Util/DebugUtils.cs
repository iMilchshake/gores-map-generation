using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Util
{
    public abstract class DebugUtils
    {
        public static void LogTime(string message)
        {
            Debug.Log($"[{Time.time}] {message}");
        }
    }
}