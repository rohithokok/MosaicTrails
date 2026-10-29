using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class GameFlowController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private PuzzleLevelDefinition[] levels;

    private readonly List<InputActionReference> _runtimeActionReferences = new List<InputActionReference>();
    private RectTransform _safeArea;
    private RectTransform _screenRoot;
    private PuzzleBoardController _board;
    private PuzzleHudPresenter _hud;
    private int _activeLevelIndex;
    private Rect _lastSafeArea;
    private bool _ready;

    private void Awake()
    {
        if (levels == null)
            levels = new PuzzleLevelDefinition[0];
        Screen.orientation = ScreenOrientation.Portrait;
        Application.targetFrameRate = 60;
        CreateEventSystem();
        CreateCanvas();
    }

    private void Start()
    {
        _ready = true;
        ShowMenu();
    }

    private void Update()
    {
        if (!_ready || Screen.safeArea == _lastSafeArea)
            return;
        ApplySafeArea();
    }

    public void ShowMenu()
    {
        ClearScreen();
        MainMenuPresenter.Build(_screenRoot, levels, ProgressStore.HighestUnlockedLevel(levels.Length), StartLevel);
        _board = null;
        _hud = null;
    }

    public void StartLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length || levels[levelIndex] == null ||
            levelIndex > ProgressStore.HighestUnlockedLevel(levels.Length))
            return;

        _activeLevelIndex = levelIndex;
        ClearScreen();
        PuzzleLevelDefinition level = levels[_activeLevelIndex];
        _hud = PuzzleHudPresenter.Build(_screenRoot, level,
            ShowHint, RestartLevel, ShowMenu);

        _board = _hud.BoardRect.gameObject.AddComponent<PuzzleBoardController>();
        _board.Build(level, _hud.BoardRect, _hud.SetProgress, OnPuzzleCompleted);
    }

    public void RestartLevel()
    {
        if (levels == null || levels.Length == 0)
            return;
        StartLevel(_activeLevelIndex);
    }

    private void ShowHint()
    {
        if (_board == null)
            return;

        bool shown = _board.ShowHint();
        string clue = levels[_activeLevelIndex].hintText;
        _hud?.SetHintStatus(shown ? clue + "  Follow the amber frames." : "Every piece is already connected.");
    }

    private void OnPuzzleCompleted()
    {
        ProgressStore.UnlockNext(_activeLevelIndex, levels.Length);
        int capturedIndex = _activeLevelIndex;
        bool isLast = capturedIndex >= levels.Length - 1;
        RectTransform panel = CompletionPresenter.Build(_screenRoot, levels[capturedIndex], isLast,
            () =>
            {
                if (isLast)
                    ShowMenu();
                else
                    StartLevel(capturedIndex + 1);
            },
            RestartLevel,
            ShowMenu);
        StartCoroutine(AnimateCompletion(panel));
    }

    private IEnumerator AnimateCompletion(RectTransform panel)
    {
        CanvasGroup group = panel.GetComponentInParent<CanvasGroup>();
        float duration = 0.34f;
        float elapsed = 0f;
        while (elapsed < duration && panel != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            if (group != null)
                group.alpha = eased;
            panel.localScale = Vector3.one * Mathf.Lerp(0.88f, 1f, eased);
            yield return null;
        }
        if (group != null)
            group.alpha = 1f;
        if (panel != null)
            panel.localScale = Vector3.one;
    }

    private void CreateEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventObject = new GameObject("Mosaic Trails Event System");
            eventSystem = eventObject.AddComponent<EventSystem>();
            eventSystem.sendNavigationEvents = false;
        }

        InputSystemUIInputModule module = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (module == null)
            module = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        if (inputActions == null)
        {
            Debug.LogError("Mosaic Trails needs the existing InputSystem_Actions asset.", this);
            return;
        }

        module.actionsAsset = inputActions;
        InputActionMap ui = inputActions.FindActionMap("UI", true);
        module.point = MakeActionReference(ui, "Point");
        module.leftClick = MakeActionReference(ui, "Click");
        module.move = MakeActionReference(ui, "Navigate");
        module.submit = MakeActionReference(ui, "Submit");
        module.cancel = MakeActionReference(ui, "Cancel");
    }

    private InputActionReference MakeActionReference(InputActionMap map, string actionName)
    {
        InputActionReference reference = InputActionReference.Create(map.FindAction(actionName, true));
        _runtimeActionReferences.Add(reference);
        return reference;
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Mosaic Trails Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject safeObject = new GameObject("Safe Area", typeof(RectTransform));
        _safeArea = (RectTransform)safeObject.transform;
        _safeArea.SetParent(canvasObject.transform, false);

        GameObject screenObject = new GameObject("Screen", typeof(RectTransform));
        _screenRoot = (RectTransform)screenObject.transform;
        _screenRoot.SetParent(_safeArea, false);
        _screenRoot.anchorMin = Vector2.zero;
        _screenRoot.anchorMax = Vector2.one;
        _screenRoot.offsetMin = Vector2.zero;
        _screenRoot.offsetMax = Vector2.zero;

        ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        Rect safe = Screen.safeArea;
        _lastSafeArea = safe;
        Vector2 min = safe.position;
        Vector2 max = safe.position + safe.size;
        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;
        _safeArea.anchorMin = min;
        _safeArea.anchorMax = max;
        _safeArea.offsetMin = Vector2.zero;
        _safeArea.offsetMax = Vector2.zero;
    }

    private void ClearScreen()
    {
        if (_screenRoot == null)
            return;
        for (int i = _screenRoot.childCount - 1; i >= 0; i--)
            Destroy(_screenRoot.GetChild(i).gameObject);
        _board = null;
        _hud = null;
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _runtimeActionReferences.Count; i++)
            if (_runtimeActionReferences[i] != null)
                Destroy(_runtimeActionReferences[i]);
        _runtimeActionReferences.Clear();
    }
}
