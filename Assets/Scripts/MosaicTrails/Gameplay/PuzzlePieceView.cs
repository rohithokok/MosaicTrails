using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class PuzzlePieceView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private PuzzleBoardController _board;
    private RectTransform _rect;
    private Image _frame;
    private Coroutine _hintRoutine;

    public int Column { get; private set; }
    public int Row { get; private set; }
    public Vector2 AnchoredPosition => _rect.anchoredPosition;
    public PuzzleCluster Cluster { get; set; }

    public void Initialize(PuzzleBoardController board, int column, int row, Sprite sprite, Vector2 size)
    {
        _board = board;
        Column = column;
        Row = row;
        _rect = (RectTransform)transform;
        _rect.sizeDelta = size;
        _frame = GetComponent<Image>();

        GameObject artworkObject = new GameObject("Artwork", typeof(RectTransform), typeof(Image));
        RectTransform artworkRect = (RectTransform)artworkObject.transform;
        artworkRect.SetParent(transform, false);
        artworkRect.anchorMin = Vector2.zero;
        artworkRect.anchorMax = Vector2.one;
        artworkRect.offsetMin = new Vector2(4f, 4f);
        artworkRect.offsetMax = new Vector2(-4f, -4f);
        Image artwork = artworkObject.GetComponent<Image>();
        artwork.sprite = sprite;
        artwork.preserveAspect = false;
        artwork.raycastTarget = false;

        Shadow shadow = gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.025f, 0.10f, 0.14f, 0.58f);
        shadow.effectDistance = new Vector2(0f, -7f);
    }

    public void SetPosition(Vector2 position)
    {
        _rect.anchoredPosition = position;
    }

    public void Translate(Vector2 delta)
    {
        _rect.anchoredPosition += delta;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_board != null && eventData.button == PointerEventData.InputButton.Left)
            _board.BeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_board != null)
            _board.Drag(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_board != null)
            _board.EndDrag(this, eventData);
    }

    public void SetHintPulse(bool active)
    {
        if (_frame == null)
            return;

        if (_hintRoutine != null)
            StopCoroutine(_hintRoutine);
        _hintRoutine = active ? StartCoroutine(HintPulse()) : null;
        if (!active)
            _frame.color = Color.white;
    }

    private IEnumerator HintPulse()
    {
        float elapsed = 0f;
        Color warm = new Color(1f, 0.76f, 0.36f);
        while (elapsed < 2.2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float pulse = 0.5f + 0.5f * Mathf.Sin(elapsed * 9f);
            _frame.color = Color.Lerp(Color.white, warm, pulse * 0.72f);
            yield return null;
        }
        _frame.color = Color.white;
        _hintRoutine = null;
    }
}
