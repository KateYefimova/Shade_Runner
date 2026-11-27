using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ExitPathGuide : MonoBehaviour
{
    [SerializeField] private Color color = new Color(0.55f, 0.85f, 1f, 0.45f);
    [SerializeField] private float startAlpha = 0.15f;
    [SerializeField] private float arrowSize = 0.3f;
    [SerializeField] private float arrowSpacing = 0.75f;
    [SerializeField] private float firstArrowOffset = 0.6f;
    [SerializeField] private float cellSize = 0.5f;
    [SerializeField] private float clearance = 0.3f;
    [SerializeField] private float refreshInterval = 0.15f;
    [SerializeField] private float rescanInterval = 0.5f;
    [SerializeField] private int sortingOrder = 3;

    private Collider2D exitArea;
    private Transform player;
    private readonly List<SpriteRenderer> arrows = new List<SpriteRenderer>();
    private Sprite arrowSprite;
    private Material arrowMaterial;
    private GameSessionModel model;

    private Bounds area;
    private int columns;
    private int rows;
    private bool[] blocked;
    private bool[] softBlocked;
    private int[] previous;
    private readonly Queue<int> queue = new Queue<int>();
    private readonly List<Vector3> points = new List<Vector3>();
    private readonly Collider2D[] hits = new Collider2D[8];
    private readonly RaycastHit2D[] castHits = new RaycastHit2D[4];
    private readonly Dictionary<Collider2D, bool> movableCache = new Dictionary<Collider2D, bool>();
    private ContactFilter2D filter;
    private float refreshTimer;
    private float rescanTimer;

    private void Start()
    {
        exitArea = GetComponent<Collider2D>();

        PlayerStatus status = FindFirstObjectByType<PlayerStatus>();
        if (status != null) player = status.transform;

        model = GameManager.Instance != null ? GameManager.Instance.Model : null;
        if (model == null || player == null) return;

        int ignored = LayerMask.GetMask("Player", "Guard", "UI", "Ignore Raycast");
        filter = new ContactFilter2D { useTriggers = false };
        filter.SetLayerMask(~ignored);

        SpriteRenderer template = status.GetComponent<SpriteRenderer>();
        arrowMaterial = template != null ? template.sharedMaterial : null;
        arrowSprite = CreateArrowSprite();
        model.PathHintChanged += OnPathHintChanged;
    }

    private void OnDestroy()
    {
        if (model != null) model.PathHintChanged -= OnPathHintChanged;
        foreach (SpriteRenderer arrow in arrows)
            if (arrow != null) Destroy(arrow.gameObject);
    }

    private void OnPathHintChanged(bool active)
    {
        HideArrows(0);
        refreshTimer = 0f;
        rescanTimer = 0f;
        if (active) BuildArea();
    }

    private void Update()
    {
        if (model == null || !model.PathHintActive || !model.IsPlaying) return;

        refreshTimer -= Time.deltaTime;
        if (refreshTimer > 0f) return;

        refreshTimer = refreshInterval;
        rescanTimer -= refreshInterval;
        if (rescanTimer <= 0f)
        {
            rescanTimer = rescanInterval;
            ScanObstacles();
        }
        RefreshPath();
    }

    private void BuildArea()
    {
        Vector3 exit = exitArea.bounds.center;
        area = new Bounds(exit, Vector3.zero);
        area.Encapsulate(player.position);

        GameObject floorObject = GameObject.Find("Floor");
        Tilemap floor = floorObject != null ? floorObject.GetComponent<Tilemap>() : null;
        if (floor != null)
        {
            floor.CompressBounds();
            Bounds local = floor.localBounds;
            area.Encapsulate(floor.transform.TransformPoint(local.min));
            area.Encapsulate(floor.transform.TransformPoint(local.max));
        }
        area.Expand(2f);

        columns = Mathf.CeilToInt(area.size.x / cellSize);
        rows = Mathf.CeilToInt(area.size.y / cellSize);
        blocked = new bool[columns * rows];
        softBlocked = new bool[columns * rows];
        previous = new int[columns * rows];
    }

    private void RefreshPath()
    {
        int start = CellOf(player.position);
        int goal = CellOf(exitArea.bounds.center);

        if (!FindPath(start, goal, true) && !FindPath(start, goal, false))
        {
            HideArrows(0);
            return;
        }

        points.Clear();
        for (int cell = goal; cell != start; cell = previous[cell])
            points.Add(CenterOf(cell));
        points.Add(player.position);
        points.Reverse();
        points[points.Count - 1] = exitArea.bounds.center;

        Simplify();
        PlaceArrows();
    }

    private void PlaceArrows()
    {
        Vector3 last = points[points.Count - 1];
        Vector3 before = points[points.Count - 2];
        float inset = Mathf.Min(exitArea.bounds.extents.x, exitArea.bounds.extents.y);
        points[points.Count - 1] = last - (last - before).normalized * inset;

        float total = 0f;
        for (int i = 1; i < points.Count; i++)
            total += Vector3.Distance(points[i - 1], points[i]);

        int used = 0;
        float distance = firstArrowOffset;
        float travelled = 0f;

        for (int i = 1; i < points.Count && total > 0f; i++)
        {
            Vector3 from = points[i - 1];
            Vector3 to = points[i];
            float length = Vector3.Distance(from, to);
            if (length < 0.001f) continue;

            Vector3 direction = (to - from) / length;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            while (distance <= travelled + length)
            {
                float progress = distance / total;
                SpriteRenderer arrow = GetArrow(used++);
                arrow.transform.position = from + direction * (distance - travelled);
                arrow.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                arrow.color = new Color(color.r, color.g, color.b, Mathf.Lerp(startAlpha, color.a, progress));
                arrow.enabled = true;
                distance += arrowSpacing;
            }
            travelled += length;
        }

        HideArrows(used);
    }

    private SpriteRenderer GetArrow(int index)
    {
        while (arrows.Count <= index)
        {
            SpriteRenderer arrow = new GameObject("ExitPathArrow").AddComponent<SpriteRenderer>();
            arrow.sprite = arrowSprite;
            if (arrowMaterial != null) arrow.sharedMaterial = arrowMaterial;
            arrow.sortingOrder = sortingOrder;
            arrow.transform.localScale = Vector3.one * arrowSize;
            arrows.Add(arrow);
        }
        return arrows[index];
    }

    private void HideArrows(int fromIndex)
    {
        for (int i = fromIndex; i < arrows.Count; i++)
            arrows[i].enabled = false;
    }

    private void ScanObstacles()
    {
        for (int i = 0; i < blocked.Length; i++)
        {
            blocked[i] = false;
            softBlocked[i] = false;

            int count = Physics2D.OverlapCircle(CenterOf(i), clearance, filter, hits);
            for (int h = 0; h < count; h++)
            {
                if (hits[h].transform.IsChildOf(player)) continue;

                if (IsMovable(hits[h])) softBlocked[i] = true;
                else blocked[i] = true;
            }
        }
    }

    private bool IsMovable(Collider2D collider)
    {
        if (!movableCache.TryGetValue(collider, out bool movable))
        {
            movable = collider.GetComponentInParent<DoorInteractable>() != null ||
                      collider.GetComponentInParent<DraggableBox>() != null;
            movableCache[collider] = movable;
        }
        return movable;
    }

    private bool FindPath(int start, int goal, bool respectMovable)
    {
        if (start < 0 || goal < 0) return false;

        for (int i = 0; i < previous.Length; i++) previous[i] = -1;
        previous[start] = start;
        queue.Clear();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            int cell = queue.Dequeue();
            if (cell == goal) return true;

            int x = cell % columns;
            int y = cell / columns;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = x + dx;
                    int ny = y + dy;
                    if (!Walkable(nx, ny, respectMovable, goal)) continue;
                    if (dx != 0 && dy != 0 && (!Walkable(x + dx, y, respectMovable, goal) || !Walkable(x, y + dy, respectMovable, goal)))
                        continue;

                    int next = ny * columns + nx;
                    if (previous[next] != -1) continue;

                    previous[next] = cell;
                    queue.Enqueue(next);
                }
            }
        }
        return false;
    }

    private bool Walkable(int x, int y, bool respectMovable, int goal)
    {
        if (x < 0 || y < 0 || x >= columns || y >= rows) return false;

        int cell = y * columns + x;
        if (cell == goal) return true;
        return !blocked[cell] && !(respectMovable && softBlocked[cell]);
    }

    private void Simplify()
    {
        int anchor = 0;
        var result = new List<Vector3> { points[0] };

        for (int i = 2; i < points.Count; i++)
        {
            if (!Clear(points[anchor], points[i]))
            {
                anchor = i - 1;
                result.Add(points[anchor]);
            }
        }
        result.Add(points[points.Count - 1]);

        points.Clear();
        points.AddRange(result);
    }

    private bool Clear(Vector3 from, Vector3 to)
    {
        Vector2 direction = to - from;
        float distance = direction.magnitude;
        if (distance < 0.01f) return true;

        int count = Physics2D.CircleCast(from, clearance * 0.8f, direction / distance, filter, castHits, distance);
        for (int i = 0; i < count; i++)
        {
            if (!castHits[i].collider.transform.IsChildOf(player)) return false;
        }
        return true;
    }

    private int CellOf(Vector3 position)
    {
        int x = Mathf.FloorToInt((position.x - area.min.x) / cellSize);
        int y = Mathf.FloorToInt((position.y - area.min.y) / cellSize);
        if (x < 0 || y < 0 || x >= columns || y >= rows) return -1;
        return y * columns + x;
    }

    private Vector3 CenterOf(int cell)
    {
        int x = cell % columns;
        int y = cell / columns;
        return new Vector3(area.min.x + (x + 0.5f) * cellSize, area.min.y + (y + 0.5f) * cellSize, 0f);
    }

    private static Sprite CreateArrowSprite()
    {
        const int size = 32;
        const float thickness = 5f;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            float offset = Mathf.Abs(y + 0.5f - size / 2f);
            for (int x = 0; x < size; x++)
            {
                float edge = 24f - offset;
                bool inside = offset <= 13f && x + 0.5f <= edge && x + 0.5f >= edge - thickness * 1.4f;
                pixels[y * size + x] = inside ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
