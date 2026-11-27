using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class NemesisHUD : MonoBehaviour
{
    public static NemesisHUD Instance { get; private set; }

    private static readonly Color Gold = new Color(0.72f, 0.56f, 0.29f, 1f);
    private static readonly Color PaleGold = new Color(0.91f, 0.81f, 0.61f, 1f);
    private static readonly Color MutedGold = new Color(0.43f, 0.33f, 0.19f, 1f);
    private static readonly Color Ink = new Color(0.025f, 0.028f, 0.034f, 0.97f);
    private static readonly Color Blood = new Color(0.53f, 0.095f, 0.12f, 1f);
    private static readonly Color Teal = new Color(0.18f, 0.43f, 0.42f, 1f);

    private Canvas canvas;
    private Sprite frameSprite;
    private Sprite slotSprite;
    private Font bodyFont;
    private Font displayFont;
    private Sprite portraitSprite;
    private Sprite cathedralSprite;
    private Sprite chaliceSprite;
    private Sprite keySprite;
    private Sprite bookSprite;
    private Sprite rosarySprite;
    private Sprite candleSprite;
    private Sprite specterSprite;
    private Sprite revenantSprite;
    private Sprite angelSprite;
    private Sprite crystalSprite;
    private Sprite minimapSprite;
    private RectTransform mapMarker;

    private Text healthText;
    private Text locationText;
    private Text relicText;
    private Text xpText;
    private Text messageText;
    private Image healthFill;
    private Image xpFill;
    private GameObject menuRoot;
    private GameObject modalRoot;
    private bool inMenu;
    private bool modalFromMenu;
    private float messageUntil;
    private int xp;
    private int relics;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += OnSceneChanged;
        LoadArt();
        Build();
    }

    void Start()
    {
        SetHealth(100, 100);
        UpdateStats();
        ShowMainMenu();
    }

    void Update()
    {
        if (mapMarker != null && NemesisPlayer.Instance != null)
        {
            Vector3 playerPosition = NemesisPlayer.Instance.transform.position;
            mapMarker.anchoredPosition = new Vector2(
                Mathf.Clamp(playerPosition.x / 18f * 68f, -68f, 68f),
                Mathf.Clamp(playerPosition.y / 10f * 68f, -68f, 68f));
        }
        if (messageText != null && Time.unscaledTime > messageUntil && !inMenu && (modalRoot == null || !modalRoot.activeSelf))
            messageText.text = "W A S D  —  движение     ЛКМ / J  —  атака     ПРОБЕЛ  —  уворот     E  —  взаимодействие";

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (modalRoot != null && modalRoot.activeSelf) CloseModal();
            else if (inMenu) ResumeGame();
            else ShowPauseMenu();
            return;
        }

        if (inMenu || (modalRoot != null && modalRoot.activeSelf)) return;

        if (Input.GetKeyDown(KeyCode.I)) OpenInventory();
        else if (Input.GetKeyDown(KeyCode.Tab)) OpenCharacter();
        else if (Input.GetKeyDown(KeyCode.B)) OpenHuntBoard();
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }

    void LoadArt()
    {
        frameSprite = MakeFrameSprite(false);
        slotSprite = MakeFrameSprite(true);
        portraitSprite = Resources.Load<Sprite>("NemesisUI/Portrait_HoodedMonk");
        cathedralSprite = Resources.Load<Sprite>("NemesisUI/Backdrop_Cathedral");
        chaliceSprite = Resources.Load<Sprite>("NemesisUI/Icon_HolyChalice");
        keySprite = Resources.Load<Sprite>("NemesisUI/Icon_SilverKey");
        bookSprite = Resources.Load<Sprite>("NemesisUI/Icon_SacredBook");
        rosarySprite = Resources.Load<Sprite>("NemesisUI/Icon_Rosary");
        candleSprite = Resources.Load<Sprite>("NemesisUI/Icon_VotiveCandles");
        specterSprite = Resources.Load<Sprite>("NemesisUI/Icon_ShadowSpecter");
        revenantSprite = Resources.Load<Sprite>("NemesisUI/Icon_BoneRevenant");
        angelSprite = Resources.Load<Sprite>("NemesisUI/Icon_FallenAngel");
        crystalSprite = Resources.Load<Sprite>("NemesisUI/Icon_MagicCrystal");
        minimapSprite = MakeMinimapSprite();

        bodyFont = ChooseFont(new[] { "Georgia", "Book Antiqua", "Palatino Linotype", "Times New Roman", "Arial" });
        displayFont = ChooseFont(new[] { "Georgia", "Book Antiqua", "Palatino Linotype", "Times New Roman", "Arial" });
    }

    Font ChooseFont(string[] preferences)
    {
        try
        {
            string[] installed = Font.GetOSInstalledFontNames();
            foreach (string wanted in preferences)
                foreach (string available in installed)
                    if (string.Equals(wanted, available, StringComparison.OrdinalIgnoreCase))
                        return Font.CreateDynamicFontFromOSFont(available, 18);
        }
        catch { }
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    void Build()
    {
        GameObject canvasObject = new GameObject("NEMESIS - Gothic Interface");
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();

        BuildHud(canvas.transform);

        menuRoot = MakeRect(canvas.transform, "Menu Overlay", Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero).gameObject;
        RectTransform menuRect = menuRoot.GetComponent<RectTransform>();
        menuRect.anchorMin = Vector2.zero;
        menuRect.anchorMax = Vector2.one;
        menuRect.pivot = new Vector2(0.5f, 0.5f);
        menuRect.anchoredPosition = Vector2.zero;
        menuRect.sizeDelta = Vector2.zero;
        Image overlay = menuRoot.AddComponent<Image>();
        overlay.color = new Color(0.008f, 0.012f, 0.019f, 0.62f);
        menuRoot.SetActive(false);

        modalRoot = MakeRect(canvas.transform, "Modal Overlay", Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero).gameObject;
        RectTransform modalRect = modalRoot.GetComponent<RectTransform>();
        modalRect.anchorMin = Vector2.zero;
        modalRect.anchorMax = Vector2.one;
        modalRect.pivot = new Vector2(0.5f, 0.5f);
        modalRect.anchoredPosition = Vector2.zero;
        modalRect.sizeDelta = Vector2.zero;
        Image modalOverlay = modalRoot.AddComponent<Image>();
        modalOverlay.color = new Color(0.008f, 0.012f, 0.019f, 0.82f);
        modalRoot.SetActive(false);
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        GameObject events = new GameObject("EventSystem");
        events.AddComponent<EventSystem>();
        events.AddComponent<StandaloneInputModule>();
        events.transform.SetParent(transform, false);
    }

    void BuildHud(Transform parent)
    {
        RectTransform hero = MakePanel(parent, "Hero Status", new Vector2(390, 132), new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -24), new Color(0.055f, 0.047f, 0.04f, 0.96f));
        AddImage(hero, "Portrait Frame", frameSprite, new Vector2(112, 112), new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -10), Color.white, true);
        AddImage(hero, "Hero Portrait", portraitSprite, new Vector2(88, 88), new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -22), Color.white, false);

        AddText(hero, "Hero Name", "ПЕПЕЛЬНЫЙ ПАЛОМНИК", 19, PaleGold, TextAnchor.MiddleLeft, new Vector2(245, 25), new Vector2(0, 1), new Vector2(0, 1), new Vector2(132, -14), displayFont, FontStyle.Bold);
        healthText = AddText(hero, "Vitality", "ЖИЗНЬ   100 / 100", 16, PaleGold, TextAnchor.MiddleLeft, new Vector2(235, 24), new Vector2(0, 1), new Vector2(0, 1), new Vector2(132, -43), bodyFont, FontStyle.Normal);
        healthFill = AddBar(hero, "Vitality Bar", new Vector2(230, 12), new Vector2(0, 1), new Vector2(0, 1), new Vector2(132, -72), Blood);
        xpText = AddText(hero, "Experience", "ОПЫТ   0", 13, new Color(0.74f, 0.72f, 0.62f), TextAnchor.MiddleLeft, new Vector2(230, 20), new Vector2(0, 1), new Vector2(0, 1), new Vector2(132, -92), bodyFont, FontStyle.Normal);
        xpFill = AddBar(hero, "Experience Bar", new Vector2(230, 5), new Vector2(0, 1), new Vector2(0, 1), new Vector2(132, -115), Teal);

        RectTransform quest = MakePanel(parent, "Main Quest", new Vector2(360, 122),
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -174),
            new Color(0.045f, 0.038f, 0.032f, 0.94f));
        AddText(quest, "Quest Header", "◇  ОСНОВНОЕ ЗАДАНИЕ", 15, Gold, TextAnchor.MiddleLeft,
            new Vector2(320, 24), new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -12), bodyFont, FontStyle.Bold);
        AddText(quest, "Quest Name", "Путь сквозь пепел", 19, PaleGold, TextAnchor.MiddleLeft,
            new Vector2(320, 28), new Vector2(0, 1), new Vector2(0, 1), new Vector2(34, -42), displayFont, FontStyle.Bold);
        AddText(quest, "Quest Objective", "Найти древний алтарь в руинах", 14, new Color(0.76f, 0.71f, 0.63f), TextAnchor.MiddleLeft,
            new Vector2(320, 30), new Vector2(0, 1), new Vector2(0, 1), new Vector2(34, -77), bodyFont, FontStyle.Normal);

        RectTransform resources = MakePanel(parent, "Relics Panel", new Vector2(278, 94), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -24), new Color(0.055f, 0.047f, 0.04f, 0.96f));
        AddText(resources, "Relic Label", "ПЕПЕЛЬНЫЕ ЗНАКИ", 14, new Color(0.72f, 0.66f, 0.52f), TextAnchor.MiddleCenter, new Vector2(240, 24), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), bodyFont, FontStyle.Bold);
        relicText = AddText(resources, "Relic Count", "◇   0", 30, PaleGold, TextAnchor.MiddleCenter, new Vector2(220, 44), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -37), displayFont, FontStyle.Bold);

        RectTransform mapPanel = MakePanel(parent, "World Minimap", new Vector2(230, 230),
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -130), new Color(0.045f, 0.038f, 0.032f, 0.96f));
        Image mapImage = AddImage(mapPanel, "Map Surface", minimapSprite, new Vector2(164, 164),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Color.white, false);
        mapImage.preserveAspect = true;
        mapMarker = MakeRect(mapImage.transform, "Player Marker", new Vector2(10, 10),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        Image markerImage = mapMarker.gameObject.AddComponent<Image>();
        markerImage.color = new Color(1f, 0.68f, 0.22f, 1f);
        AddText(mapPanel, "Compass North", "N", 15, PaleGold, TextAnchor.MiddleCenter, new Vector2(24, 20),
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -4), displayFont, FontStyle.Bold);
        AddText(mapPanel, "Compass West", "W", 14, PaleGold, TextAnchor.MiddleCenter, new Vector2(20, 20),
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(6, 0), displayFont, FontStyle.Bold);
        AddText(mapPanel, "Compass East", "E", 14, PaleGold, TextAnchor.MiddleCenter, new Vector2(20, 20),
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-6, 0), displayFont, FontStyle.Bold);
        AddText(mapPanel, "Compass South", "S", 14, PaleGold, TextAnchor.MiddleCenter, new Vector2(24, 20),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 4), displayFont, FontStyle.Bold);

        RectTransform location = MakePanel(parent, "Location Banner", new Vector2(420, 58), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -24), new Color(0.045f, 0.04f, 0.035f, 0.88f));
        locationText = AddText(location, "Location", "ПЕПЕЛЬНЫЕ ОКРАИНЫ", 20, PaleGold, TextAnchor.MiddleCenter, new Vector2(390, 34), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, displayFont, FontStyle.Bold);

        RectTransform hotbar = MakePanel(parent, "Quick Access", new Vector2(506, 92), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 18), new Color(0.045f, 0.04f, 0.035f, 0.94f));
        Sprite[] icons = { chaliceSprite, keySprite, bookSprite, crystalSprite, rosarySprite };
        string[] keys = { "1", "2", "3", "4", "5" };
        string[] captions = { "РЕЛИКТ", "КЛЮЧ", "ЗНАНИЕ", "КРИСТАЛЛ", "ОБЕРЕГ" };
        for (int i = 0; i < 5; i++)
        {
            float x = (i - 2) * 94f;
            RectTransform slot = MakePanel(hotbar, "Quick Slot " + (i + 1), new Vector2(72, 72), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 1), new Color(0.12f, 0.105f, 0.085f, 0.96f), slotSprite);
            AddImage(slot, "Slot Item", icons[i], new Vector2(42, 42), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 3), Color.white, false);
            AddText(slot, "Slot Key", keys[i], 13, PaleGold, TextAnchor.MiddleCenter, new Vector2(22, 18), new Vector2(0, 1), new Vector2(0, 1), new Vector2(4, -2), bodyFont, FontStyle.Bold);
            AddText(slot, "Slot Caption", captions[i], 8, new Color(0.7f, 0.64f, 0.52f), TextAnchor.MiddleCenter, new Vector2(72, 14), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 1), bodyFont, FontStyle.Bold);
        }

        RectTransform messagePanel = MakePanel(parent, "Message Panel", new Vector2(570, 66), new Vector2(0, 0), new Vector2(0, 0), new Vector2(24, 24), new Color(0.035f, 0.033f, 0.03f, 0.9f));
        messageText = AddText(messagePanel, "Message", "W A S D  —  движение     ЛКМ / J  —  атака     ПРОБЕЛ  —  уворот     E  —  взаимодействие", 14, PaleGold, TextAnchor.MiddleLeft, new Vector2(540, 50), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, bodyFont, FontStyle.Normal);

        RectTransform hints = MakePanel(parent, "Interface Hints", new Vector2(340, 66), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-24, 24), new Color(0.035f, 0.033f, 0.03f, 0.9f));
        AddText(hints, "Hints", "TAB  —  герой     I  —  сумка     B  —  охота     ESC  —  меню", 13, new Color(0.78f, 0.72f, 0.6f), TextAnchor.MiddleCenter, new Vector2(320, 48), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, bodyFont, FontStyle.Normal);

        CreateHudButton(parent, "ИНВЕНТАРЬ", new Vector2(104, 62), new Vector2(0, 0), new Vector2(0, 0), new Vector2(24, 110), OpenInventory);
        CreateHudButton(parent, "КАРТА", new Vector2(104, 62), new Vector2(0, 0), new Vector2(0, 0), new Vector2(136, 110), OpenMap);
        CreateHudButton(parent, "ЖУРНАЛ", new Vector2(104, 62), new Vector2(0, 0), new Vector2(0, 0), new Vector2(248, 110), OpenJournal);
        CreateHudButton(parent, "НАСТРОЙКИ", new Vector2(112, 62), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-136, 110), OpenSettings);
        CreateHudButton(parent, "МЕНЮ", new Vector2(104, 62), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-24, 110), ShowPauseMenu);
    }

    void OnSceneChanged(Scene previous, Scene next)
    {
        if (locationText == null) return;
        locationText.text = next.name == "AshenDungeon"
            ? "ЗАБЫТЫЕ КАТАКОМБЫ"
            : "ПЕПЕЛЬНЫЕ ОКРАИНЫ";
    }

    void ShowMainMenu()
    {
        BuildMenu("ПЕЧАТЬ БЕЗДНЫ", "ИСТОРИЯ, ЗАПЕЧАТАННАЯ ВО ТЬМЕ", true);
        inMenu = true;
        Time.timeScale = 0f;
        Cursor.visible = true;
    }

    void ShowPauseMenu()
    {
        BuildMenu("ПАУЗА", "ПУТЬ МОЖЕТ ПОДОЖДАТЬ", false);
        inMenu = true;
        Time.timeScale = 0f;
        Cursor.visible = true;
    }

    void BuildMenu(string title, string subtitle, bool titleScreen)
    {
        modalRoot.SetActive(false);
        menuRoot.SetActive(true);
        for (int i = menuRoot.transform.childCount - 1; i >= 0; i--)
            Destroy(menuRoot.transform.GetChild(i).gameObject);

        RectTransform frame = MakePanel(menuRoot.transform, "Menu Frame", new Vector2(820, 850),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
            new Color(0.035f, 0.032f, 0.03f, 0.90f));
        if (cathedralSprite != null)
        {
            Image backdrop = AddImage(frame, "Cathedral Silhouette", cathedralSprite,
                new Vector2(310, 390), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-28, 18), new Color(0.72f, 0.67f, 0.59f, 0.19f), false);
            backdrop.preserveAspect = true;
        }

        AddText(frame, "Menu Title", title, 48, PaleGold, TextAnchor.MiddleCenter,
            new Vector2(720, 92), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -46), displayFont, FontStyle.Bold);
        AddText(frame, "Menu Subtitle", subtitle, 16, new Color(0.68f, 0.55f, 0.34f),
            TextAnchor.MiddleCenter, new Vector2(690, 32), new Vector2(0.5f, 1),
            new Vector2(0.5f, 1), new Vector2(0, -125), bodyFont, FontStyle.Bold);
        AddDivider(frame, new Vector2(500, 2), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -166));

        float startY = 145f;
        float spacing = 68f;
        var buttons = titleScreen ? new (string label, Action action, bool primary)[]
        {
            ("НОВАЯ ИГРА", StartNewGame, true),
            ("ПРОДОЛЖИТЬ", ResumeGame, false),
            ("ИНВЕНТАРЬ И СНАРЯЖЕНИЕ", OpenInventory, false),
            ("ДОСКА ОХОТЫ", OpenHuntBoard, false),
            ("НАСТРОЙКИ", OpenSettings, false),
            ("ВЫХОД", QuitGame, false),
        } : new (string label, Action action, bool primary)[]
        {
            ("ПРОДОЛЖИТЬ", ResumeGame, true),
            ("ИНВЕНТАРЬ И СНАРЯЖЕНИЕ", OpenInventory, false),
            ("ДОСКА ОХОТЫ", OpenHuntBoard, false),
            ("НАСТРОЙКИ", OpenSettings, false),
            ("В ГЛАВНОЕ МЕНЮ", ShowMainMenu, false),
            ("ВЫХОД", QuitGame, false),
        };
        for (int i = 0; i < buttons.Length; i++)
            CreateMenuButton(frame, buttons[i].label, new Vector2(0, startY - spacing * i), buttons[i].action, buttons[i].primary);

        AddDivider(frame, new Vector2(500, 1), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 48));
        AddText(frame, "Footer", "ТЁМНОЕ ФЭНТЕЗИ  •  ИССЛЕДУЙ. ВЫЖИВАЙ. ПОМНИ.",
            12, MutedGold, TextAnchor.MiddleCenter, new Vector2(700, 26), new Vector2(0.5f, 0),
            new Vector2(0.5f, 0), new Vector2(0, 14), bodyFont, FontStyle.Normal);
    }
    Sprite MakeMinimapSprite()
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "Nemesis Ashen Courtyard Minimap";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        Color dark = new Color(0.018f, 0.022f, 0.03f, 1f);
        Color stone = new Color(0.20f, 0.20f, 0.22f, 1f);
        Color wall = new Color(0.38f, 0.37f, 0.35f, 1f);
        Color path = new Color(0.45f, 0.40f, 0.32f, 1f);
        Color lava = new Color(0.48f, 0.055f, 0.045f, 1f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            bool courtyard = x >= 18 && x <= 108 && y >= 16 && y <= 110;
            Color c = courtyard ? stone : dark;
            if (courtyard && (x < 24 || x > 102 || y < 22 || y > 104)) c = wall;
            if (x < 40 && y < 48 && x + y < 72) c = lava;
            if (courtyard && (Mathf.Abs(x - (26 + y / 2)) < 3)) c = path;
            if (courtyard && y > 76 && x > 72 && x < 100 && Mathf.Abs(y - (86 + (x - 72) / 3)) < 3) c = path;
            if ((x % 11 == 0 || y % 13 == 0) && courtyard && c == stone) c = new Color(0.24f, 0.23f, 0.23f, 1f);
            texture.SetPixel(x, y, c);
        }
        texture.Apply(false, true);
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128f);
    }

    void CreateHudButton(Transform parent, string label, Vector2 size, Vector2 anchor, Vector2 pivot, Vector2 position, Action action)
    {
        RectTransform rect = MakeRect(parent, label + " Dock Button", size, anchor, pivot, position);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = frameSprite;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.085f, 0.07f, 0.055f, 0.96f);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.35f, 0.26f, 0.15f, 1f);
        colors.pressedColor = new Color(0.18f, 0.12f, 0.08f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        button.onClick.AddListener(() => action?.Invoke());
        AddText(rect, "Dock Label", label, 12, PaleGold, TextAnchor.MiddleCenter, size - new Vector2(8, 8),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, bodyFont, FontStyle.Bold);
    }
    void CreateMenuButton(Transform parent, string label, Vector2 position, Action action, bool primary)
    {
        RectTransform rect = MakeRect(parent, label, new Vector2(primary ? 500 : 460, 54), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = frameSprite;
        image.type = Image.Type.Sliced;
        image.color = primary ? new Color(0.30f, 0.22f, 0.12f, 0.98f) : new Color(0.12f, 0.105f, 0.085f, 0.96f);

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = primary ? new Color(0.48f, 0.35f, 0.17f, 1f) : new Color(0.28f, 0.22f, 0.14f, 1f);
        colors.pressedColor = new Color(0.19f, 0.13f, 0.08f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        button.onClick.AddListener(() => action?.Invoke());

        AddText(rect, "Button Label", label, 18, primary ? PaleGold : new Color(0.82f, 0.77f, 0.66f), TextAnchor.MiddleCenter, new Vector2(430, 44), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, bodyFont, FontStyle.Bold);
    }

    void OpenCharacter()
    {
        OpenModal("ПАЛОМНИК", "ХАРАКТЕРИСТИКИ И СОСТОЯНИЕ");
        AddImage(modalRoot.transform.GetChild(0), "Character Portrait", portraitSprite, new Vector2(112, 112), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(54, 10), Color.white, false);
        AddModalLine("Жизненная сила", "100 / 100", 90);
        AddModalLine("Сила удара", "28", 40);
        AddModalLine("Уворот", "Короткий рывок", -10);
        AddModalLine("Опыт", xp.ToString(), -60);
        AddModalLine("Пепельные знаки", relics.ToString(), -110);
    }

    void OpenInventory()
    {
        OpenModal("СНАРЯЖЕНИЕ", "РЕЛИКВИИ, КЛЮЧИ И ПРЕДМЕТЫ ПУТНИКА");
        Transform panel = modalRoot.transform.GetChild(0);
        Sprite[] icons = { chaliceSprite, keySprite, bookSprite, crystalSprite, rosarySprite, candleSprite };
        string[] names = { "Священный кубок", "Серебряный ключ", "Книга обрядов", "Пепельный кристалл", "Чётки", "Обетные свечи" };
        for (int i = 0; i < 6; i++)
        {
            Vector2 pos = new Vector2(-220 + (i % 3) * 220, 45 - (i / 3) * 150);
            RectTransform slot = MakePanel(panel, "Inventory Slot " + i, new Vector2(178, 126), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Color(0.09f, 0.08f, 0.07f, 0.97f), slotSprite);
            AddImage(slot, "Item Icon", icons[i], new Vector2(48, 48), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), Color.white, false);
            AddText(slot, "Item Name", names[i], 12, PaleGold, TextAnchor.MiddleCenter, new Vector2(160, 34), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), bodyFont, FontStyle.Normal);
        }
        AddText(panel, "Inventory Note", "Найдено пепельных знаков: " + relics + "     •     Некоторые реликвии пока не раскрыты", 14, new Color(0.72f, 0.66f, 0.54f), TextAnchor.MiddleCenter, new Vector2(630, 28), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 34), bodyFont, FontStyle.Normal);
    }

    void OpenMap()
    {
        OpenModal("КАРТА ПЕПЕЛЬНЫХ ОКРАИН", "ИССЛЕДОВАННЫЕ ДОРОГИ И ОТМЕЧЕННЫЕ УГРОЗЫ");
        Transform panel = modalRoot.transform.GetChild(0);
        Image map = AddImage(panel, "Expanded World Map", minimapSprite, new Vector2(300, 300),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), Color.white, false);
        map.preserveAspect = true;
        AddModalLine("Текущая область", "Пепельные окраины", -220);
        AddModalLine("Главная цель", "Древний алтарь", -260);
    }

    void OpenJournal()
    {
        OpenModal("ЖУРНАЛ ПАЛОМНИКА", "ПУТЬ СКВОЗЬ ПЕПЕЛ");
        AddModalLine("Основное задание", "Найти древний алтарь", 92);
        AddModalLine("Место", "Руины собора", 42);
        AddModalLine("Пепельные знаки", relics.ToString(), -8);
        AddModalLine("Собранный опыт", xp.ToString(), -58);
    }
    void OpenHuntBoard()
    {
        OpenModal("ДОСКА ОХОТЫ", "ОТМЕЧЕННЫЕ ЦЕЛИ ПЕПЕЛЬНЫХ ОКРАИН");
        Transform panel = modalRoot.transform.GetChild(0);
        MakeHuntCard(panel, angelSprite, "ПАДШИЙ АНГЕЛ", "ГЛАВНАЯ ЦЕЛЬ", "Награда: 250 опыта", new Vector2(-208, 42), true);
        MakeHuntCard(panel, revenantSprite, "КОСТЯНОЙ МЕРТВЕЦ", "ОПАСНОСТЬ", "Награда: 25 опыта", new Vector2(208, 42), false);
        MakeHuntCard(panel, specterSprite, "ТЕНЕВОЙ ПРИЗРАК", "ОПАСНОСТЬ", "Награда: 25 опыта", new Vector2(-208, -150), false);
        MakeHuntCard(panel, crystalSprite, "ПЕПЕЛЬНЫЙ СЛЕД", "ИССЛЕДОВАНИЕ", "Найди забытые святилища", new Vector2(208, -150), false);
    }

    void MakeHuntCard(Transform parent, Sprite icon, string name, string type, string reward, Vector2 pos, bool elite)
    {
        RectTransform card = MakePanel(parent, "Hunt Card " + name, new Vector2(350, 142), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Color(0.085f, 0.073f, 0.063f, 0.98f), slotSprite);
        AddImage(card, "Target Portrait", icon, new Vector2(72, 82), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(16, 0), Color.white, false);
        AddText(card, "Target Name", name, 16, elite ? new Color(0.88f, 0.57f, 0.39f) : PaleGold, TextAnchor.MiddleLeft, new Vector2(235, 26), new Vector2(0, 1), new Vector2(0, 1), new Vector2(98, -20), displayFont, FontStyle.Bold);
        AddText(card, "Target Type", type, 11, MutedGold, TextAnchor.MiddleLeft, new Vector2(225, 22), new Vector2(0, 1), new Vector2(0, 1), new Vector2(98, -52), bodyFont, FontStyle.Bold);
        AddText(card, "Target Reward", reward, 12, new Color(0.75f, 0.72f, 0.63f), TextAnchor.MiddleLeft, new Vector2(225, 22), new Vector2(0, 1), new Vector2(0, 1), new Vector2(98, -83), bodyFont, FontStyle.Normal);
    }

    void OpenSettings()
    {
        OpenModal("НАСТРОЙКИ", "ЗВУК И ИЗОБРАЖЕНИЕ");
        Transform panel = modalRoot.transform.GetChild(0);
        AddText(panel, "Volume Label", "ГРОМКОСТЬ", 15, PaleGold, TextAnchor.MiddleLeft, new Vector2(180, 28), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-170, 92), bodyFont, FontStyle.Bold);

        GameObject sliderObject = new GameObject("Master Volume");
        sliderObject.transform.SetParent(panel, false);
        RectTransform sr = sliderObject.AddComponent<RectTransform>();
        sr.anchorMin = sr.anchorMax = new Vector2(0.5f, 0.5f);
        sr.pivot = new Vector2(0.5f, 0.5f);
        sr.anchoredPosition = new Vector2(100, 92);
        sr.sizeDelta = new Vector2(300, 24);
        Slider slider = sliderObject.AddComponent<Slider>();
        Image bg = MakeImageObject(sliderObject.transform, "Background", null, new Vector2(300, 8), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Color(0.12f, 0.10f, 0.08f, 1f), false);
        Image fillArea = MakeImageObject(sliderObject.transform, "Fill", null, new Vector2(300, 8), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, Gold, false);
        Image handle = MakeImageObject(sliderObject.transform, "Handle", null, new Vector2(18, 26), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, PaleGold, false);
        slider.targetGraphic = handle;
        slider.fillRect = fillArea.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = AudioListener.volume;
        slider.onValueChanged.AddListener(value => AudioListener.volume = value);

        CreateMenuButton(panel, Screen.fullScreen ? "ПЕРЕЙТИ В ОКОННЫЙ РЕЖИМ" : "ПОЛНОЭКРАННЫЙ РЕЖИМ", new Vector2(0, -5), () => Screen.fullScreen = !Screen.fullScreen, false);
        AddText(panel, "Settings Note", "Изменения применяются сразу", 12, MutedGold, TextAnchor.MiddleCenter, new Vector2(550, 26), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -75), bodyFont, FontStyle.Normal);
    }

    void OpenModal(string title, string subtitle)
    {
        modalFromMenu = inMenu;
        menuRoot.SetActive(false);
        inMenu = false;
        Time.timeScale = 0f;
        Cursor.visible = true;
        modalRoot.SetActive(true);
        for (int i = modalRoot.transform.childCount - 1; i >= 0; i--)
            Destroy(modalRoot.transform.GetChild(i).gameObject);

        RectTransform panel = MakePanel(modalRoot.transform, "Modal Frame", new Vector2(900, 700), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Color(0.035f, 0.032f, 0.03f, 0.99f));
        AddText(panel, "Modal Title", title, 34, PaleGold, TextAnchor.MiddleCenter, new Vector2(800, 62), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -36), displayFont, FontStyle.Bold);
        AddText(panel, "Modal Subtitle", subtitle, 13, MutedGold, TextAnchor.MiddleCenter, new Vector2(800, 28), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -98), bodyFont, FontStyle.Bold);
        AddDivider(panel, new Vector2(700, 2), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -132));
        CreateMenuButton(panel, "ЗАКРЫТЬ", new Vector2(0, -292), CloseModal, false);
    }

    void AddModalLine(string label, string value, float y)
    {
        Transform panel = modalRoot.transform.GetChild(0);
        AddText(panel, label, label, 17, new Color(0.73f, 0.68f, 0.58f), TextAnchor.MiddleLeft, new Vector2(330, 30), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-20, y), bodyFont, FontStyle.Normal);
        AddText(panel, value, value, 18, PaleGold, TextAnchor.MiddleRight, new Vector2(200, 30), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(280, y), bodyFont, FontStyle.Bold);
    }

    void CloseModal()
    {
        modalRoot.SetActive(false);
        if (modalFromMenu)
        {
            menuRoot.SetActive(true);
            inMenu = true;
            Time.timeScale = 0f;
        }
        else
        {
            inMenu = false;
            Time.timeScale = 1f;
            Cursor.visible = false;
        }
    }

    void StartNewGame()
    {
        xp = 0;
        relics = 0;
        UpdateStats();

        // Close the title menu before changing scenes. The HUD is persistent,
        // so leaving this active used to keep the game paused behind it.
        modalRoot.SetActive(false);
        menuRoot.SetActive(false);
        inMenu = false;
        Time.timeScale = 1f;
        Cursor.visible = false;

        SceneManager.LoadScene("AshenOutskirts");
        Time.timeScale = 1f;
        SetMessage("\u041F\u0423\u0422\u042C \u041D\u0410\u0427\u0418\u041D\u0410\u0415\u0422\u0421\u042F");
    }
    void ResumeGame()
    {
        modalRoot.SetActive(false);
        menuRoot.SetActive(false);
        inMenu = false;
        Time.timeScale = 1f;
        Cursor.visible = false;
        SetMessage("ПУТЬ ПРОДОЛЖАЕТСЯ");
    }

    void TogglePause()
    {
        if (inMenu) ResumeGame();
        else ShowPauseMenu();
    }

    void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    void BuildMenuReturnStub() { }

    Image AddBar(Transform parent, string name, Vector2 size, Vector2 anchor, Vector2 pivot, Vector2 position, Color fillColor)
    {
        RectTransform bgRect = MakeRect(parent, name + " Background", size, anchor, pivot, position);
        Image bg = bgRect.gameObject.AddComponent<Image>();
        bg.sprite = slotSprite;
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.13f, 0.11f, 0.09f, 1f);

        RectTransform fillRect = MakeRect(bgRect, name + " Fill", new Vector2(size.x - 4, size.y - 4), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(2, 0));
        Image fill = fillRect.gameObject.AddComponent<Image>();
        fill.color = fillColor;
        fill.type = Image.Type.Simple;
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.anchorMin = new Vector2(0, 0.5f);
        fillRect.anchorMax = new Vector2(0, 0.5f);
        return fill;
    }

    void AddDivider(Transform parent, Vector2 size, Vector2 anchor, Vector2 pivot, Vector2 position)
    {
        RectTransform line = MakeRect(parent, "Gold Divider", size, anchor, pivot, position);
        Image image = line.gameObject.AddComponent<Image>();
        image.color = new Color(Gold.r, Gold.g, Gold.b, 0.75f);
        RectTransform highlight = MakeRect(line, "Divider Highlight", new Vector2(size.x * 0.42f, 1), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        highlight.gameObject.AddComponent<Image>().color = PaleGold;
    }

    RectTransform MakePanel(Transform parent, string name, Vector2 size, Vector2 anchor, Vector2 pivot, Vector2 position, Color color, Sprite spriteOverride = null)
    {
        RectTransform rect = MakeRect(parent, name, size, anchor, pivot, position);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = spriteOverride != null ? spriteOverride : frameSprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = true;
        return rect;
    }

    Image AddImage(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 anchor, Vector2 pivot, Vector2 position, Color color, bool useFrame)
    {
        Image image = MakeImageObject(parent, name, sprite, size, anchor, pivot, position, color, useFrame);
        return image;
    }

    Image MakeImageObject(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 anchor, Vector2 pivot, Vector2 position, Color color, bool useFrame)
    {
        RectTransform rect = MakeRect(parent, name, size, anchor, pivot, position);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = useFrame ? frameSprite : sprite;
        image.type = useFrame ? Image.Type.Sliced : Image.Type.Simple;
        image.color = color;
        image.preserveAspect = sprite != null && !useFrame;
        image.raycastTarget = false;
        return image;
    }

    Text AddText(Transform parent, string name, string value, int size, Color color, TextAnchor alignment, Vector2 rectSize, Vector2 anchor, Vector2 pivot, Vector2 position, Font font, FontStyle style)
    {
        RectTransform rect = MakeRect(parent, name, rectSize, anchor, pivot, position);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = size;
        text.fontStyle = style;
        text.text = value;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.supportRichText = false;
        text.raycastTarget = false;
        return text;
    }

    RectTransform MakeRect(Transform parent, string name, Vector2 size, Vector2 anchor, Vector2 pivot, Vector2 position)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    Sprite MakeFrameSprite(bool slot)
    {
        const int width = 48;
        const int height = 48;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.name = slot ? "Nemesis UI Slot Frame" : "Nemesis UI Gilded Frame";
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        Color fill = slot ? new Color(0.09f, 0.075f, 0.06f, 0.96f) : new Color(0.045f, 0.039f, 0.034f, 0.98f);
        Color border = slot ? new Color(0.37f, 0.29f, 0.18f, 1f) : new Color(0.58f, 0.43f, 0.23f, 1f);
        Color inner = new Color(0.22f, 0.17f, 0.11f, 1f);
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int edge = Mathf.Min(x, y, width - 1 - x, height - 1 - y);
            Color c = fill;
            if (edge <= 1) c = border;
            else if (edge == 3) c = inner;
            if ((x < 7 || x >= width - 7) && (y < 7 || y >= height - 7) && (x == 4 || x == width - 5 || y == 4 || y == height - 5))
                c = new Color(0.82f, 0.65f, 0.34f, 1f);
            float shade = 0.96f + 0.04f * ((float)y / height);
            c.r *= shade; c.g *= shade; c.b *= shade;
            tex.SetPixel(x, y, c);
        }
        tex.Apply(false, true);
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 48, 0, SpriteMeshType.FullRect, new Vector4(10, 10, 10, 10));
    }

    public void SetHealth(int value, int max)
    {
        if (healthText == null || healthFill == null) return;
        max = Mathf.Max(1, max);
        value = Mathf.Clamp(value, 0, max);
        healthText.text = "ЖИЗНЬ   " + value + " / " + max;
        healthFill.rectTransform.sizeDelta = new Vector2(226f * value / max, healthFill.rectTransform.sizeDelta.y);
    }

    public void SetMessage(string value)
    {
        if (messageText == null) return;
        messageText.text = value;
        messageUntil = Time.unscaledTime + 4.5f;
    }

    public void AddXP(int value)
    {
        xp += Mathf.Max(0, value);
        UpdateStats();
    }

    public void AddRelic()
    {
        relics++;
        UpdateStats();
    }

    void UpdateStats()
    {
        if (xpText != null) xpText.text = "ОПЫТ   " + xp;
        if (relicText != null) relicText.text = "◇   " + relics;
        if (xpFill != null)
        {
            float progress = (xp % 100) / 100f;
            xpFill.rectTransform.sizeDelta = new Vector2(226f * progress, xpFill.rectTransform.sizeDelta.y);
        }
    }
}






