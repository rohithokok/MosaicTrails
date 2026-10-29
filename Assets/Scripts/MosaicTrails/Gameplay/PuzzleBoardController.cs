using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class PuzzleBoardController : MonoBehaviour
{
    private const float SnapToleranceFraction = 0.18f;
    private const float SnapDuration = 0.18f;

    private readonly List<PuzzleCluster> _clusters = new List<PuzzleCluster>();
    private readonly List<PuzzlePieceView> _pieces = new List<PuzzlePieceView>();
    private readonly List<Sprite> _sprites = new List<Sprite>();
    private Texture2D _artwork;
    private RectTransform _boardRect;
    private int _pieceCount;
    private int _activePointerId = int.MinValue;
    private Vector2 _lastPointerPosition;
    private float _tileWidth;
    private float _tileHeight;
    private bool _busy;
    private Action<int, int> _progressChanged;
    private Action _completed;

    public int PieceCount => _pieceCount;

    public void Build(PuzzleLevelDefinition level, RectTransform boardRect, Action<int, int> progressChanged, Action completed)
    {
        _boardRect = boardRect;
        _progressChanged = progressChanged;
        _completed = completed;
        ClearBoard();

        int columns = Mathf.Max(2, level.columns);
        int rows = Mathf.Max(2, level.rows);
        _pieceCount = columns * rows;
        _artwork = PuzzleArtworkGenerator.Create(level.artworkTheme);

        Rect area = _boardRect.rect;
        float imageSide = Mathf.Min(area.width * 0.76f, area.height * 0.76f);
        _tileWidth = imageSide / columns;
        _tileHeight = imageSide / rows;
        Vector2 tileSize = new Vector2(_tileWidth, _tileHeight);

        List<Vector2> scatterSlots = CreateScatterSlots(columns, rows, area);
        int slotIndex = 0;
        Shuffle(scatterSlots, UnityEngine.Random.Range(1, int.MaxValue));

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int xMin = column * _artwork.width / columns;
                int xMax = (column + 1) * _artwork.width / columns;
                int yMin = (rows - 1 - row) * _artwork.height / rows;
                int yMax = (rows - row) * _artwork.height / rows;
                Rect spriteRect = new Rect(
                    xMin, yMin, xMax - xMin, yMax - yMin);
                Sprite sprite = Sprite.Create(_artwork, spriteRect, new Vector2(0.5f, 0.5f), 100f);
                sprite.name = level.levelTitle + " - " + column + " " + row;
                _sprites.Add(sprite);

                GameObject pieceObject = new GameObject(
                    "Piece " + (row * columns + column + 1),
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(PuzzlePieceView));
                RectTransform pieceRect = (RectTransform)pieceObject.transform;
                pieceRect.SetParent(_boardRect, false);
                pieceRect.anchorMin = pieceRect.anchorMax = new Vector2(0.5f, 0.5f);
                pieceRect.pivot = new Vector2(0.5f, 0.5f);

                Image frame = pieceObject.GetComponent<Image>();
                frame.sprite = PuzzleUIFactory.CardSprite;
                frame.type = Image.Type.Sliced;
                frame.color = Color.white;
                frame.raycastTarget = true;

                PuzzlePieceView view = pieceObject.GetComponent<PuzzlePieceView>();
                view.Initialize(this, column, row, sprite, tileSize);
                view.SetPosition(scatterSlots[slotIndex++]);

                PuzzleCluster cluster = new PuzzleCluster();
                cluster.Pieces.Add(view);
                view.Cluster = cluster;
                _clusters.Add(cluster);
                _pieces.Add(view);
            }
        }

        _progressChanged?.Invoke(0, _pieceCount);
    }

    public void BeginDrag(PuzzlePieceView piece, PointerEventData eventData)
    {
        if (_busy || piece == null || piece.Cluster == null ||
            (_activePointerId != int.MinValue && _activePointerId != eventData.pointerId))
            return;

        _activePointerId = eventData.pointerId;
        if (TryGetBoardPoint(eventData.position, out _lastPointerPosition))
        {
            for (int i = 0; i < piece.Cluster.Pieces.Count; i++)
                piece.Cluster.Pieces[i].transform.SetAsLastSibling();
        }
    }

    public void Drag(PuzzlePieceView piece, PointerEventData eventData)
    {
        if (_busy || _activePointerId != eventData.pointerId || piece == null || piece.Cluster == null)
            return;
        if (!TryGetBoardPoint(eventData.position, out Vector2 point))
            return;

        Vector2 delta = point - _lastPointerPosition;
        TranslateCluster(piece.Cluster, delta);
        _lastPointerPosition = point;
    }

    public void EndDrag(PuzzlePieceView piece, PointerEventData eventData)
    {
        if (_activePointerId != eventData.pointerId)
            return;
        _activePointerId = int.MinValue;

        if (!_busy && piece != null && piece.Cluster != null && FindConnection(piece.Cluster, out PuzzleCluster stationary, out Vector2 correction))
        {
            _busy = true;
            StartCoroutine(SnapAndMerge(piece.Cluster, stationary, correction));
        }
    }

    public bool ShowHint()
    {
        PuzzlePieceView bestA = null;
        PuzzlePieceView bestB = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < _pieces.Count; i++)
        {
            PuzzlePieceView a = _pieces[i];
            for (int j = i + 1; j < _pieces.Count; j++)
            {
                PuzzlePieceView b = _pieces[j];
                if (a.Cluster == b.Cluster || !PuzzleConnectionRules.AreNeighbors(a, b))
                    continue;

                Vector2 expected = PuzzleConnectionRules.ExpectedOffset(a, b, _tileWidth, _tileHeight);
                Vector2 error = (b.AnchoredPosition - a.AnchoredPosition) - expected;
                if (error.sqrMagnitude >= bestDistance)
                    continue;

                bestDistance = error.sqrMagnitude;
                bestA = a;
                bestB = b;
            }
        }

        if (bestA == null)
            return false;

        bestA.SetHintPulse(true);
        bestB.SetHintPulse(true);
        return true;
    }

    private bool FindConnection(PuzzleCluster moving, out PuzzleCluster stationary, out Vector2 correction)
    {
        stationary = null;
        correction = Vector2.zero;
        float bestError = float.MaxValue;

        for (int i = 0; i < moving.Pieces.Count; i++)
        {
            PuzzlePieceView a = moving.Pieces[i];
            for (int c = 0; c < _clusters.Count; c++)
            {
                PuzzleCluster other = _clusters[c];
                if (other == moving)
                    continue;

                for (int j = 0; j < other.Pieces.Count; j++)
                {
                    PuzzlePieceView b = other.Pieces[j];
                    if (!PuzzleConnectionRules.TryGetCorrection(a, b, _tileWidth, _tileHeight, SnapToleranceFraction,
                            out Vector2 candidateCorrection, out float errorSquared) || errorSquared >= bestError)
                        continue;

                    bestError = errorSquared;
                    stationary = other;
                    correction = candidateCorrection;
                }
            }
        }

        return stationary != null;
    }

    private IEnumerator SnapAndMerge(PuzzleCluster moving, PuzzleCluster stationary, Vector2 correction)
    {
        float elapsed = 0f;
        float previousEase = 0f;
        while (elapsed < SnapDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / SnapDuration);
            float eased = t * t * (3f - 2f * t);
            TranslateCluster(moving, correction * (eased - previousEase));
            previousEase = eased;
            yield return null;
        }
        TranslateCluster(moving, correction * (1f - previousEase));

        for (int i = 0; i < moving.Pieces.Count; i++)
        {
            PuzzlePieceView piece = moving.Pieces[i];
            piece.Cluster = stationary;
            stationary.Pieces.Add(piece);
        }
        moving.Pieces.Clear();
        _clusters.Remove(moving);
        _busy = false;

        int joined = 0;
        for (int i = 0; i < _clusters.Count; i++)
            if (_clusters[i].Pieces.Count > 1)
                joined += _clusters[i].Pieces.Count;
        _progressChanged?.Invoke(joined, _pieceCount);

        if (_clusters.Count == 1)
            _completed?.Invoke();
    }

    private bool TryGetBoardPoint(Vector2 screenPoint, out Vector2 boardPoint)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(_boardRect, screenPoint, null, out boardPoint);
    }

    private static void TranslateCluster(PuzzleCluster cluster, Vector2 delta)
    {
        for (int i = 0; i < cluster.Pieces.Count; i++)
            cluster.Pieces[i].Translate(delta);
    }

    private static List<Vector2> CreateScatterSlots(int columns, int rows, Rect area)
    {
        List<Vector2> slots = new List<Vector2>(columns * rows);
        float cellWidth = area.width / columns;
        float cellHeight = area.height / rows;
        float left = -area.width * 0.5f + cellWidth * 0.5f;
        float bottom = -area.height * 0.5f + cellHeight * 0.5f;
        for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
                slots.Add(new Vector2(left + column * cellWidth, bottom + row * cellHeight));
        return slots;
    }

    private static void Shuffle<T>(IList<T> list, int seed)
    {
        System.Random random = new System.Random(seed);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private void ClearBoard()
    {
        for (int i = 0; i < _pieces.Count; i++)
            if (_pieces[i] != null)
                Destroy(_pieces[i].gameObject);
        _pieces.Clear();
        _clusters.Clear();
        for (int i = 0; i < _sprites.Count; i++)
            if (_sprites[i] != null)
                Destroy(_sprites[i]);
        _sprites.Clear();
        if (_artwork != null)
        {
            Destroy(_artwork);
            _artwork = null;
        }
        _activePointerId = int.MinValue;
        _busy = false;
    }

    private void OnDestroy()
    {
        ClearBoard();
    }
}
