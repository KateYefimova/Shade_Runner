using System.Collections.Generic;
using UnityEngine;

// Маршрут патрулювання: точки задаються дочірніми об'єктами (або списком).
// Один об'єкт PatrolRoute = один маршрут. Охоронці беруть його через GuardAI.
public class PatrolRoute : MonoBehaviour
{
    public enum RouteMode
    {
        Loop,      // 1 → 2 → 3 → 1 → 2 → 3 ...
        PingPong   // 1 → 2 → 3 → 2 → 1 → 2 ...
    }

    [SerializeField] private RouteMode mode = RouteMode.Loop;

    [Tooltip("Точки маршруту по порядку. Якщо порожньо, беруться всі дочірні об'єкти.")]
    [SerializeField] private List<Transform> points = new List<Transform>();

    [SerializeField] private Color gizmoColor = Color.cyan;

    private readonly List<Transform> runtimePoints = new List<Transform>();

    private List<Transform> GetPoints()
    {
        runtimePoints.Clear();

        if (points != null && points.Count > 0)
        {
            foreach (Transform p in points)
                if (p != null)
                    runtimePoints.Add(p);
        }
        else
        {
            foreach (Transform child in transform)
                runtimePoints.Add(child);
        }

        return runtimePoints;
    }

    public int Count => GetPoints().Count;

    public Vector2 GetPosition(int index)
    {
        List<Transform> pts = GetPoints();
        return pts[Mathf.Clamp(index, 0, pts.Count - 1)].position;
    }

    // Повертає індекс наступної точки. direction змінюється лише в режимі PingPong.
    public int GetNextIndex(int current, ref int direction)
    {
        int count = Count;

        if (count <= 1)
            return 0;

        if (mode == RouteMode.Loop)
            return (current + 1) % count;

        int next = current + direction;

        if (next >= count || next < 0)
        {
            direction = -direction;
            next = current + direction;
        }

        return next;
    }

    private void OnDrawGizmos()
    {
        List<Transform> pts = GetPoints();
        Gizmos.color = gizmoColor;

        for (int i = 0; i < pts.Count; i++)
        {
            Vector3 p = pts[i].position;
            Gizmos.DrawWireSphere(p, 0.2f);

            if (i + 1 < pts.Count)
                Gizmos.DrawLine(p, pts[i + 1].position);
            else if (mode == RouteMode.Loop && pts.Count > 2)
                Gizmos.DrawLine(p, pts[0].position);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(p + Vector3.up * 0.3f, (i + 1).ToString());
#endif
        }
    }
}