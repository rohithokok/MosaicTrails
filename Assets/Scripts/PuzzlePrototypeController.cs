using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Runtime-only four-piece picture-block prototype. Uses generated placeholder art
/// and Input System pointer devices, so it needs no extra assets or action bindings.
/// </summary>
public sealed class PuzzlePrototypeController : MonoBehaviour
{
    private const int ImageSize = 256;
    private const int GridSize = 2;
    private const float PixelsPerUnit = 100f;

    // Center-to-center errors up to this amount on either axis are accepted.
    // At a 1.28-unit piece width this permits about 14% of a piece as drop tolerance.
    private const float ConnectionTolerance = 0.18f;

    private readonly List<Cluster> _clusters = new List<Cluster>();
    private Camera _camera;
    private Cluster _draggedCluster;
    private Vector2 _dragOffset;
    private PointerKind _activePointer;
    private int _nextSortingOrder = 10;
    private bool _completed;
    private float _pieceWorldSize;

    private enum PointerKind
    {
        None,
        Mouse,
        Touch
    }

    private sealed class Piece
    {
        public int Column;
        public int Row;
        public Transform Transform;
        public SpriteRenderer Renderer;
        public BoxCollider2D Collider;
        public Cluster Cluster;
    }

    private sealed class Cluster
    {
        public Transform Root;
        public readonly List<Piece> Pieces = new List<Piece>();
    }

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null)
        {
            Debug.LogError("Puzzle prototype needs a scene camera.", this);
            enabled = false;
            return;
        }

        CreatePuzzle();
    }

    private void Update()
    {
        if (_completed)
            return;

        if (_draggedCluster == null)
        {
            if (TryGetPointerDown(out Vector2 screenPosition, out PointerKind pointer))
                BeginDrag(screenPosition, pointer);
            return;
        }

        if (TryGetActivePointer(out Vector2 activePosition, out bool released))
        {
            MoveDraggedCluster(activePosition);
            if (released)
                EndDrag();
        }
        else
        {
            EndDrag();
        }
    }

    private void CreatePuzzle()
    {
        Texture2D image = CreatePlaceholderIllustration();
        image.name = "Runtime Moonlit Greenhouse Placeholder";
        image.filterMode = FilterMode.Point;
        image.wrapMode = TextureWrapMode.Clamp;

        int tilePixels = ImageSize / GridSize;
        _pieceWorldSize = tilePixels / PixelsPerUnit;
        float half = _pieceWorldSize * 0.5f;

        // Deliberately begin apart from the picture's assembled location.
        Vector2[] starts =
        {
            new Vector2(-1.95f, 1.55f),
            new Vector2(1.95f, 1.55f),
            new Vector2(-1.95f, -1.55f),
            new Vector2(1.95f, -1.55f)
        };

        int startIndex = 0;
        for (int row = 0; row < GridSize; row++)
        {
            for (int column = 0; column < GridSize; column++)
            {
                Cluster cluster = new Cluster();
                GameObject root = new GameObject("Piece Cluster " + (startIndex + 1));
                cluster.Root = root.transform;
                root.transform.position = starts[startIndex++];

                Rect rect = new Rect(
                    column * tilePixels,
                    (GridSize - 1 - row) * tilePixels,
                    tilePixels,
                    tilePixels);
                Sprite sprite = Sprite.Create(image, rect, new Vector2(0.5f, 0.5f), PixelsPerUnit);
                sprite.name = "Picture Piece " + column + " " + row;

                GameObject pieceObject = new GameObject("Picture Piece " + column + " " + row);
                pieceObject.transform.SetParent(root.transform, false);

                SpriteRenderer renderer = pieceObject.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = startIndex;

                BoxCollider2D collider = pieceObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(_pieceWorldSize, _pieceWorldSize);

                Piece piece = new Piece
                {
                    Column = column,
                    Row = row,
                    Transform = pieceObject.transform,
                    Renderer = renderer,
                    Collider = collider,
                    Cluster = cluster
                };
                cluster.Pieces.Add(piece);
                _clusters.Add(cluster);
            }
        }
    }

    private void BeginDrag(Vector2 screenPosition, PointerKind pointer)
    {
        Vector2 worldPosition = ScreenToWorld(screenPosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);
        Piece hitPiece = null;
        int highestOrder = int.MinValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Piece candidate = FindPiece(hits[i]);
            if (candidate != null && candidate.Renderer.sortingOrder >= highestOrder)
            {
                hitPiece = candidate;
                highestOrder = candidate.Renderer.sortingOrder;
            }
        }

        if (hitPiece == null)
            return;

        _draggedCluster = hitPiece.Cluster;
        _activePointer = pointer;
        _dragOffset = (Vector2)_draggedCluster.Root.position - worldPosition;
        for (int i = 0; i < _draggedCluster.Pieces.Count; i++)
            _draggedCluster.Pieces[i].Renderer.sortingOrder = _nextSortingOrder++;
    }

    private void MoveDraggedCluster(Vector2 screenPosition)
    {
        Vector2 position = ScreenToWorld(screenPosition) + _dragOffset;
        _draggedCluster.Root.position = new Vector3(position.x, position.y, 0f);
    }

    private void EndDrag()
    {
        Cluster moving = _draggedCluster;
        _draggedCluster = null;
        _activePointer = PointerKind.None;

        if (moving == null)
            return;

        if (TryFindConnection(moving, out Cluster stationary, out Vector2 correction))
        {
            moving.Root.position += (Vector3)correction;
            MergeClusters(moving, stationary);
            if (_clusters.Count == 1)
            {
                _completed = true;
                Debug.Log("Picture puzzle completed.", this);
            }
        }
    }

    private bool TryFindConnection(Cluster moving, out Cluster stationary, out Vector2 correction)
    {
        stationary = null;
        correction = Vector2.zero;
        float bestError = float.MaxValue;

        for (int c = 0; c < _clusters.Count; c++)
        {
            Cluster other = _clusters[c];
            if (other == moving)
                continue;

            for (int i = 0; i < moving.Pieces.Count; i++)
            {
                for (int j = 0; j < other.Pieces.Count; j++)
                {
                    Piece a = moving.Pieces[i];
                    Piece b = other.Pieces[j];
                    int columnDelta = b.Column - a.Column;
                    int rowDelta = b.Row - a.Row;

                    // Only source-image neighbors can form a connection.
                    if (Mathf.Abs(columnDelta) + Mathf.Abs(rowDelta) != 1)
                        continue;

                    Vector2 expected = new Vector2(
                        columnDelta * _pieceWorldSize,
                        -rowDelta * _pieceWorldSize);
                    Vector2 actual = (Vector2)b.Transform.position - (Vector2)a.Transform.position;
                    Vector2 error = actual - expected;

                    // Both the edge gap and its perpendicular alignment must be in tolerance.
                    if (Mathf.Abs(error.x) > ConnectionTolerance || Mathf.Abs(error.y) > ConnectionTolerance)
                        continue;

                    float errorSquared = error.sqrMagnitude;
                    if (errorSquared >= bestError)
                        continue;

                    bestError = errorSquared;
                    stationary = other;
                    correction = error;
                }
            }
        }

        return stationary != null;
    }

    private void MergeClusters(Cluster moving, Cluster stationary)
    {
        for (int i = 0; i < moving.Pieces.Count; i++)
        {
            Piece piece = moving.Pieces[i];
            piece.Transform.SetParent(stationary.Root, true);
            piece.Cluster = stationary;
            piece.Renderer.sortingOrder = _nextSortingOrder++;
            stationary.Pieces.Add(piece);
        }

        moving.Pieces.Clear();
        _clusters.Remove(moving);
        Destroy(moving.Root.gameObject);
    }

    private bool TryGetPointerDown(out Vector2 position, out PointerKind pointer)
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            position = Touchscreen.current.primaryTouch.position.ReadValue();
            pointer = PointerKind.Touch;
            return true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            position = Mouse.current.position.ReadValue();
            pointer = PointerKind.Mouse;
            return true;
        }

        position = Vector2.zero;
        pointer = PointerKind.None;
        return false;
    }

    private bool TryGetActivePointer(out Vector2 position, out bool released)
    {
        if (_activePointer == PointerKind.Touch && Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            position = touch.position.ReadValue();
            released = touch.press.wasReleasedThisFrame;
            return touch.press.isPressed || released;
        }

        if (_activePointer == PointerKind.Mouse && Mouse.current != null)
        {
            position = Mouse.current.position.ReadValue();
            released = Mouse.current.leftButton.wasReleasedThisFrame;
            return Mouse.current.leftButton.isPressed || released;
        }

        position = Vector2.zero;
        released = false;
        return false;
    }

    private Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        float depth = -_camera.transform.position.z;
        Vector3 world = _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
        return new Vector2(world.x, world.y);
    }

    private Piece FindPiece(Collider2D collider)
    {
        for (int i = 0; i < _clusters.Count; i++)
        {
            List<Piece> pieces = _clusters[i].Pieces;
            for (int j = 0; j < pieces.Count; j++)
            {
                if (pieces[j].Collider == collider)
                    return pieces[j];
            }
        }

        return null;
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Max(16, Screen.width / 32),
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.96f, 0.91f, 0.78f) }
        };

        string message = _completed
            ? "NIGHT GARDEN COMPLETE!"
            : "NIGHT GARDEN  •  Drag matching picture edges together";
        GUI.Label(new Rect(8f, 12f, Screen.width - 16f, 48f), message, style);
    }

    private static Texture2D CreatePlaceholderIllustration()
    {
        Color[] pixels = new Color[ImageSize * ImageSize];
        Color top = new Color(0.10f, 0.19f, 0.27f);
        Color bottom = new Color(0.20f, 0.39f, 0.39f);

        for (int y = 0; y < ImageSize; y++)
        {
            Color rowColor = Color.Lerp(bottom, top, y / (float)(ImageSize - 1));
            for (int x = 0; x < ImageSize; x++)
                pixels[y * ImageSize + x] = rowColor;
        }

        // Moon and scattered stars.
        FillCircle(pixels, 196, 194, 25, new Color(0.96f, 0.78f, 0.46f));
        FillCircle(pixels, 205, 203, 22, top);
        FillCircle(pixels, 39, 205, 2, new Color(0.96f, 0.85f, 0.63f));
        FillCircle(pixels, 87, 176, 2, new Color(0.96f, 0.85f, 0.63f));
        FillCircle(pixels, 148, 224, 2, new Color(0.96f, 0.85f, 0.63f));
        FillCircle(pixels, 231, 155, 2, new Color(0.96f, 0.85f, 0.63f));

        // Greenhouse silhouette: peaked glass roof, supports, bench and pots.
        Color frame = new Color(0.78f, 0.66f, 0.44f);
        DrawLine(pixels, 34, 117, 128, 183, 3, frame);
        DrawLine(pixels, 128, 183, 222, 117, 3, frame);
        DrawLine(pixels, 34, 117, 222, 117, 3, frame);
        DrawLine(pixels, 43, 117, 43, 43, 3, frame);
        DrawLine(pixels, 213, 117, 213, 43, 3, frame);
        DrawLine(pixels, 43, 43, 213, 43, 3, frame);
        DrawLine(pixels, 128, 181, 128, 44, 2, frame);
        DrawLine(pixels, 65, 139, 65, 44, 2, frame);
        DrawLine(pixels, 191, 139, 191, 44, 2, frame);
        DrawLine(pixels, 52, 78, 204, 78, 3, frame);
        DrawLine(pixels, 52, 43, 204, 43, 3, frame);

        Color leaf = new Color(0.33f, 0.66f, 0.43f);
        Color lightLeaf = new Color(0.53f, 0.79f, 0.47f);
        DrawLine(pixels, 92, 46, 91, 114, 3, new Color(0.30f, 0.49f, 0.34f));
        FillEllipse(pixels, 78, 95, 15, 7, leaf);
        FillEllipse(pixels, 103, 105, 16, 7, lightLeaf);
        FillEllipse(pixels, 78, 119, 14, 7, lightLeaf);
        FillEllipse(pixels, 103, 127, 15, 7, leaf);
        FillEllipse(pixels, 158, 55, 11, 18, leaf);
        FillEllipse(pixels, 175, 61, 10, 16, lightLeaf);
        FillEllipse(pixels, 146, 68, 10, 14, lightLeaf);

        Color terracotta = new Color(0.80f, 0.39f, 0.27f);
        FillRect(pixels, 79, 43, 104, 52, terracotta);
        FillRect(pixels, 75, 52, 108, 56, new Color(0.91f, 0.56f, 0.35f));

        // Clean tile seams, part of the original artwork itself.
        Color seam = new Color(0.10f, 0.16f, 0.20f);
        FillRect(pixels, 126, 0, 129, ImageSize, seam);
        FillRect(pixels, 0, 126, ImageSize, 129, seam);

        Texture2D texture = new Texture2D(ImageSize, ImageSize, TextureFormat.RGBA32, false);
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        return texture;
    }

    private static void FillRect(Color[] pixels, int xMin, int yMin, int xMax, int yMax, Color color)
    {
        for (int y = Mathf.Max(0, yMin); y < Mathf.Min(ImageSize, yMax); y++)
            for (int x = Mathf.Max(0, xMin); x < Mathf.Min(ImageSize, xMax); x++)
                pixels[y * ImageSize + x] = color;
    }

    private static void FillCircle(Color[] pixels, int centerX, int centerY, int radius, Color color)
    {
        int squaredRadius = radius * radius;
        for (int y = Mathf.Max(0, centerY - radius); y <= Mathf.Min(ImageSize - 1, centerY + radius); y++)
        {
            for (int x = Mathf.Max(0, centerX - radius); x <= Mathf.Min(ImageSize - 1, centerX + radius); x++)
            {
                int dx = x - centerX;
                int dy = y - centerY;
                if (dx * dx + dy * dy <= squaredRadius)
                    pixels[y * ImageSize + x] = color;
            }
        }
    }

    private static void FillEllipse(Color[] pixels, int centerX, int centerY, int radiusX, int radiusY, Color color)
    {
        for (int y = Mathf.Max(0, centerY - radiusY); y <= Mathf.Min(ImageSize - 1, centerY + radiusY); y++)
        {
            for (int x = Mathf.Max(0, centerX - radiusX); x <= Mathf.Min(ImageSize - 1, centerX + radiusX); x++)
            {
                float dx = (x - centerX) / (float)radiusX;
                float dy = (y - centerY) / (float)radiusY;
                if (dx * dx + dy * dy <= 1f)
                    pixels[y * ImageSize + x] = color;
            }
        }
    }

    private static void DrawLine(Color[] pixels, int x0, int y0, int x1, int y1, int width, Color color)
    {
        float dx = x1 - x0;
        float dy = y1 - y0;
        float lengthSquared = dx * dx + dy * dy;
        int radius = width / 2;
        int minX = Mathf.Max(0, Mathf.Min(x0, x1) - radius);
        int maxX = Mathf.Min(ImageSize - 1, Mathf.Max(x0, x1) + radius);
        int minY = Mathf.Max(0, Mathf.Min(y0, y1) - radius);
        int maxY = Mathf.Min(ImageSize - 1, Mathf.Max(y0, y1) + radius);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float t = lengthSquared > 0f
                    ? Mathf.Clamp01(((x - x0) * dx + (y - y0) * dy) / lengthSquared)
                    : 0f;
                float nearestX = x0 + t * dx;
                float nearestY = y0 + t * dy;
                float lineRadius = width * 0.5f;
                float distanceX = x - nearestX;
                float distanceY = y - nearestY;
                if (distanceX * distanceX + distanceY * distanceY <= lineRadius * lineRadius)
                    pixels[y * ImageSize + x] = color;
            }
        }
    }
}
