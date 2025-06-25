using System.Collections.Generic;
using UnityEngine;

public class Util : MonoBehaviour
{
    public static List<Vector3> SmoothLine(List<Vector3> points, int iterations = 2)
    {
        if (points == null || points.Count < 2)
            return points;

        var newPoints = new List<Vector3>(points);
        for (int it = 0; it < iterations; it++)
        {
            var smoothed = new List<Vector3>();
            smoothed.Add(newPoints[0]);
            for (int i = 0; i < newPoints.Count - 1; i++)
            {
                Vector3 p0 = newPoints[i];
                Vector3 p1 = newPoints[i + 1];
                Vector3 Q = Vector3.Lerp(p0, p1, 0.25f);
                Vector3 R = Vector3.Lerp(p0, p1, 0.75f);
                smoothed.Add(Q);
                smoothed.Add(R);
            }
            smoothed.Add(newPoints[newPoints.Count - 1]);
            newPoints = smoothed;
        }
        return newPoints;
    }
}
