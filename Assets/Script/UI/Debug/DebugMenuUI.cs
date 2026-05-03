#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;
using Assets.Script.Managers;
using Assets.Script.UI.Tabs;
using Assets.Script.UI.Settings;
using Assets.Script.Entities;
using Assets.Script.Creatures;

namespace Assets.Script.UI.DebugMenu
{
    /// <summary>
    /// Menu debug overlay activable via F12.
    /// Fournit navigation rapide, cheats et infos runtime.
    /// Compilé conditionnellement : UNITY_EDITOR ou DEVELOPMENT_BUILD uniquement.
    /// GDD §18.4 — Menu Debug.
    /// </summary>
    public class DebugMenuUI : MonoBehaviour
    {
        // ==========================
        //  Références externes
        // ==========================

        private TabManager _tabManager;
        private SettingsSpawner _settingsSpawner;

        // ==========================
        //  Références internes
        // ==========================

        private Canvas _debugCanvas;
        private GameObject _panel;
        private bool _isVisible;

        // Runtime info labels
        private TMP_Text _fpsLabel;
        private TMP_Text _creatureCountLabel;
        private TMP_Text _activeSlotLabel;
        private TMP_Text _lastAutoSaveLabel;

        // Console log
        private TMP_Text _consoleText;
        private ScrollRect _consoleScroll;
        private bool _showConsole;
        private readonly List<string> _logBuffer = new List<string>();
        private const int MAX_LOG_LINES = 100;

        // FPS tracking
        private float _fpsTimer;
        private int _fpsFrameCount;
        private float _currentFPS;

        // ==========================
        //  Couleurs du thème
        // ==========================

        private static readonly Color BgColor = new Color(0.05f, 0.05f, 0.08f, 0.95f);
        private static readonly Color PanelColor = new Color(0.08f, 0.08f, 0.14f, 0.95f);
        private static readonly Color SectionColor = new Color(0.10f, 0.10f, 0.18f, 0.9f);
        private static readonly Color ButtonColor = new Color(0.3f, 0.5f, 0.7f, 1f);
        private static readonly Color CheatColor = new Color(0.5f, 0.3f, 0.7f, 1f);
        private static readonly Color DangerColor = new Color(0.6f, 0.2f, 0.2f, 1f);
        private static readonly Color TextColor = Color.white;
        private static readonly Color TextDimColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        private static readonly Color ConsoleBgColor = new Color(0.03f, 0.03f, 0.05f, 0.95f);

        // ==========================
        //  Initialisation
        // ==========================

        /// <summary>
        /// Initialise le DebugMenuUI avec les références aux systèmes existants.
        /// GDD §18.4.
        /// </summary>
        public void Initialize(TabManager tabManager, SettingsSpawner settingsSpawner)
        {
            _tabManager = tabManager;
            _settingsSpawner = settingsSpawner;

            CreateDebugCanvas();
            BuildDebugPanel();

            _debugCanvas.gameObject.SetActive(false);
            _isVisible = false;

            // S'abonner aux logs Unity
            Application.logMessageReceived += OnLogReceived;

            Debug.Log("[DebugMenuUI] Menu debug initialisé (F12 pour toggle).");
        }

        // ==========================
        //  Cycle de vie Unity
        // ==========================

        private void Update()
        {
            // Toggle via F12
            if (Keyboard.current != null && Keyboard.current[Key.F12].wasPressedThisFrame)
            {
                Toggle();
            }

            if (_isVisible)
            {
                UpdateFPS();
                UpdateRuntimeInfo();
            }
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLogReceived;
        }

        // ==========================
        //  Toggle
        // ==========================

        /// <summary>Bascule la visibilité du menu debug.</summary>
        public void Toggle()
        {
            _isVisible = !_isVisible;
            if (_debugCanvas != null)
                _debugCanvas.gameObject.SetActive(_isVisible);
        }

        /// <summary>Affiche le menu debug.</summary>
        public void Show()
        {
            _isVisible = true;
            if (_debugCanvas != null)
                _debugCanvas.gameObject.SetActive(true);
        }

        /// <summary>Masque le menu debug.</summary>
        public void Hide()
        {
            _isVisible = false;
            if (_debugCanvas != null)
                _debugCanvas.gameObject.SetActive(false);
        }

        // ==========================
        //  Construction UI
        // ==========================

        /// <summary>Crée le Canvas overlay pour le debug menu (sort order très élevé).</summary>
        private void CreateDebugCanvas()
        {
            var canvasGO = new GameObject("DebugCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);

            _debugCanvas = canvasGO.GetComponent<Canvas>();
            _debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _debugCanvas.sortingOrder = 200;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            if (EventSystem.current == null)
            {
                var esGO = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                esGO.GetComponent<InputSystemUIInputModule>();
            }
        }

        /// <summary>Construit le panneau debug complet. GDD §18.4.</summary>
        private void BuildDebugPanel()
        {
            var root = _debugCanvas.transform;

            // Background semi-transparent
            var bg = CreatePanel(root, "DebugBg", new Color(0f, 0f, 0f, 0.5f));
            StretchFull(bg);

            // Panneau latéral droit
            _panel = CreatePanel(root, "DebugPanel", PanelColor).gameObject;
            var panelRT = _panel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.70f, 0f);
            panelRT.anchorMax = new Vector2(1f, 1f);
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            // Header
            var header = CreatePanel(panelRT.transform, "Header", new Color(0.12f, 0.12f, 0.20f, 1f));
            header.anchorMin = new Vector2(0f, 0.95f);
            header.anchorMax = new Vector2(1f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            CreateText(header, "Title", "DEBUG MENU (F12)", 20,
                new Vector2(0.05f, 0.1f), new Vector2(0.75f, 0.9f),
                TextAlignmentOptions.Left, TextColor);

            var closeBtn = CreateButton(header, "CloseBtn", "✕",
                new Vector2(0.85f, 0.1f), new Vector2(0.98f, 0.9f), DangerColor);
            closeBtn.onClick.AddListener(Hide);

            // Section Navigation
            BuildNavigationSection(panelRT);

            // Section Cheats
            BuildCheatsSection(panelRT);

            // Section Infos Runtime
            BuildRuntimeInfoSection(panelRT);

            // Section Console
            BuildConsoleSection(panelRT);
        }

        // ==========================
        //  Section Navigation — GDD §18.4
        // ==========================

        /// <summary>Construit la section de navigation rapide.</summary>
        private void BuildNavigationSection(RectTransform parent)
        {
            var section = CreatePanel(parent.transform, "NavSection", SectionColor);
            section.anchorMin = new Vector2(0.02f, 0.78f);
            section.anchorMax = new Vector2(0.98f, 0.94f);
            section.offsetMin = Vector2.zero;
            section.offsetMax = Vector2.zero;

            CreateText(section, "NavTitle", "Navigation", 16,
                new Vector2(0.03f, 0.82f), new Vector2(0.97f, 0.98f),
                TextAlignmentOptions.Left, TextColor);

            // Boutons onglets
            var btnCreatures = CreateButton(section, "NavCreatures", "Créatures",
                new Vector2(0.02f, 0.52f), new Vector2(0.24f, 0.78f), ButtonColor);
            btnCreatures.onClick.AddListener(() => { if (_tabManager != null) _tabManager.SwitchTab(0); });

            var btnBreeding = CreateButton(section, "NavBreeding", "Breeding",
                new Vector2(0.26f, 0.52f), new Vector2(0.48f, 0.78f), ButtonColor);
            btnBreeding.onClick.AddListener(() => { if (_tabManager != null) _tabManager.SwitchTab(1); });

            var btnInventory = CreateButton(section, "NavInventory", "Inventaire",
                new Vector2(0.50f, 0.52f), new Vector2(0.72f, 0.78f), ButtonColor);
            btnInventory.onClick.AddListener(() => { if (_tabManager != null) _tabManager.SwitchTab(2); });

            var btnStats = CreateButton(section, "NavStats", "Stats",
                new Vector2(0.74f, 0.52f), new Vector2(0.98f, 0.78f), ButtonColor);
            btnStats.onClick.AddListener(() => { if (_tabManager != null) _tabManager.SwitchTab(3); });

            // Paramètres & Retour Menu
            var btnSettings = CreateButton(section, "NavSettings", "Paramètres",
                new Vector2(0.02f, 0.05f), new Vector2(0.48f, 0.45f), ButtonColor);
            btnSettings.onClick.AddListener(() => { if (_settingsSpawner != null) _settingsSpawner.Toggle(); });

            var btnMainMenu = CreateButton(section, "NavMainMenu", "Retour Menu",
                new Vector2(0.50f, 0.05f), new Vector2(0.98f, 0.45f), DangerColor);
            btnMainMenu.onClick.AddListener(OnReturnToMainMenu);
        }

        // ==========================
        //  Section Cheats — GDD §18.4
        // ==========================

        /// <summary>Construit la section de cheats debug.</summary>
        private void BuildCheatsSection(RectTransform parent)
        {
            var section = CreatePanel(parent.transform, "CheatSection", SectionColor);
            section.anchorMin = new Vector2(0.02f, 0.48f);
            section.anchorMax = new Vector2(0.98f, 0.76f);
            section.offsetMin = Vector2.zero;
            section.offsetMax = Vector2.zero;

            CreateText(section, "CheatTitle", "Triche Debug", 16,
                new Vector2(0.03f, 0.88f), new Vector2(0.97f, 0.98f),
                TextAlignmentOptions.Left, TextColor);

            // Ligne 1 : Ressources
            var btnFragments = CreateButton(section, "CheatFragments", "+1000 Fragments",
                new Vector2(0.02f, 0.62f), new Vector2(0.32f, 0.85f), CheatColor);
            btnFragments.onClick.AddListener(() => AddResource(ResourceType.Fragments, 1000f));

            var btnEssence = CreateButton(section, "CheatEssence", "+100 Essence",
                new Vector2(0.34f, 0.62f), new Vector2(0.64f, 0.85f), CheatColor);
            btnEssence.onClick.AddListener(() => AddResource(ResourceType.Essence, 100f));

            var btnGold = CreateButton(section, "CheatGold", "+10 Gold",
                new Vector2(0.66f, 0.62f), new Vector2(0.98f, 0.85f), CheatColor);
            btnGold.onClick.AddListener(() => AddResource(ResourceType.Gold, 10f));

            // Ligne 2 : Actions
            var btnSpawn = CreateButton(section, "CheatSpawn", "Spawn Créature",
                new Vector2(0.02f, 0.34f), new Vector2(0.32f, 0.57f), CheatColor);
            btnSpawn.onClick.AddListener(SpawnRandomCreature);

            var btnSave = CreateButton(section, "CheatSave", "Forcer Save",
                new Vector2(0.34f, 0.34f), new Vector2(0.64f, 0.57f), CheatColor);
            btnSave.onClick.AddListener(ForceSave);

            var btnRebirth = CreateButton(section, "CheatRebirth", "Rebirth",
                new Vector2(0.66f, 0.34f), new Vector2(0.98f, 0.57f), DangerColor);
            btnRebirth.onClick.AddListener(ForceRebirth);

            // Toggle Console
            var btnConsole = CreateButton(section, "CheatConsole", "Toggle Console",
                new Vector2(0.02f, 0.05f), new Vector2(0.48f, 0.28f), ButtonColor);
            btnConsole.onClick.AddListener(ToggleConsole);
        }

        // ==========================
        //  Section Infos Runtime — GDD §18.4
        // ==========================

        /// <summary>Construit la section d'informations runtime.</summary>
        private void BuildRuntimeInfoSection(RectTransform parent)
        {
            var section = CreatePanel(parent.transform, "RuntimeSection", SectionColor);
            section.anchorMin = new Vector2(0.02f, 0.32f);
            section.anchorMax = new Vector2(0.98f, 0.46f);
            section.offsetMin = Vector2.zero;
            section.offsetMax = Vector2.zero;

            CreateText(section, "RuntimeTitle", "Infos Runtime", 16,
                new Vector2(0.03f, 0.78f), new Vector2(0.97f, 0.98f),
                TextAlignmentOptions.Left, TextColor);

            _fpsLabel = CreateText(section, "FPS", "FPS: --", 14,
                new Vector2(0.03f, 0.40f), new Vector2(0.48f, 0.72f),
                TextAlignmentOptions.Left, TextDimColor);

            _creatureCountLabel = CreateText(section, "CreatureCount", "Créatures: --", 14,
                new Vector2(0.50f, 0.40f), new Vector2(0.98f, 0.72f),
                TextAlignmentOptions.Left, TextDimColor);

            _activeSlotLabel = CreateText(section, "ActiveSlot", "Slot: --", 14,
                new Vector2(0.03f, 0.05f), new Vector2(0.48f, 0.38f),
                TextAlignmentOptions.Left, TextDimColor);

            _lastAutoSaveLabel = CreateText(section, "LastAutoSave", "Auto-save: --", 14,
                new Vector2(0.50f, 0.05f), new Vector2(0.98f, 0.38f),
                TextAlignmentOptions.Left, TextDimColor);
        }

        // ==========================
        //  Section Console
        // ==========================

        /// <summary>Construit la section console de logs.</summary>
        private void BuildConsoleSection(RectTransform parent)
        {
            var section = CreatePanel(parent.transform, "ConsoleSection", ConsoleBgColor);
            section.anchorMin = new Vector2(0.02f, 0.02f);
            section.anchorMax = new Vector2(0.98f, 0.30f);
            section.offsetMin = Vector2.zero;
            section.offsetMax = Vector2.zero;

            // ScrollRect
            var scrollGO = new GameObject("ConsoleScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGO.transform.SetParent(section.transform, false);
            var scrollRT = scrollGO.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0.02f, 0.02f);
            scrollRT.anchorMax = new Vector2(0.98f, 0.98f);
            scrollRT.offsetMin = Vector2.zero;
            scrollRT.offsetMax = Vector2.zero;
            scrollGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.3f);

            _consoleScroll = scrollGO.GetComponent<ScrollRect>();
            _consoleScroll.horizontal = false;
            _consoleScroll.vertical = true;
            _consoleScroll.movementType = ScrollRect.MovementType.Clamped;

            // Content
            var contentGO = new GameObject("Content", typeof(RectTransform), typeof(ContentSizeFitter));
            contentGO.transform.SetParent(scrollGO.transform, false);
            var contentRT = contentGO.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = new Vector2(1f, 1f);
            contentRT.pivot = new Vector2(0.5f, 1f);
            contentRT.sizeDelta = new Vector2(0f, 0f);

            var fitter = contentGO.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _consoleScroll.content = contentRT;

            // Text
            _consoleText = CreateText(contentRT, "ConsoleText", "[Console prête]", 12,
                Vector2.zero, Vector2.one, TextAlignmentOptions.TopLeft, new Color(0.7f, 0.9f, 0.7f, 1f));
            var textRT = _consoleText.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = new Vector2(1f, 1f);
            textRT.pivot = new Vector2(0.5f, 1f);
            _consoleText.enableWordWrapping = true;
            _consoleText.overflowMode = TextOverflowModes.Overflow;

            // Commencer masquée
            section.gameObject.SetActive(false);
            _showConsole = false;
        }

        // ==========================
        //  Update Runtime Info
        // ==========================

        /// <summary>Met à jour le compteur FPS.</summary>
        private void UpdateFPS()
        {
            _fpsTimer += Time.unscaledDeltaTime;
            _fpsFrameCount++;

            if (_fpsTimer >= 0.5f)
            {
                _currentFPS = _fpsFrameCount / _fpsTimer;
                _fpsTimer = 0f;
                _fpsFrameCount = 0;
            }
        }

        /// <summary>Met à jour les labels d'informations runtime.</summary>
        private void UpdateRuntimeInfo()
        {
            if (_fpsLabel != null)
                _fpsLabel.text = $"FPS: {_currentFPS:F0}";

            if (_creatureCountLabel != null)
            {
                int count = GameManager.Instance?.CurrentProfile?.Inventory?.OwnedCreatures?.Count ?? 0;
                _creatureCountLabel.text = $"Créatures: {count}";
            }

            if (_activeSlotLabel != null)
            {
                int slot = SaveManager.Instance?.ActiveSlot ?? -1;
                _activeSlotLabel.text = $"Slot: {slot}";
            }

            if (_lastAutoSaveLabel != null)
            {
                string lastSave = GameManager.Instance?.CurrentProfile?.LastSavedAt ?? "--";
                if (lastSave != "--" && lastSave.Length > 16)
                    lastSave = lastSave.Substring(11, 8);
                _lastAutoSaveLabel.text = $"Auto-save: {lastSave}";
            }
        }

        // ==========================
        //  Callbacks Cheats
        // ==========================

        /// <summary>Ajoute une ressource au profil actuel.</summary>
        private void AddResource(ResourceType type, float amount)
        {
            var resources = GameManager.Instance?.CurrentProfile?.Resources;
            if (resources == null)
            {
                Debug.LogWarning("[DebugMenu] Pas de profil chargé.");
                return;
            }

            resources.Add(type, amount);
            Debug.Log($"[DebugMenu] +{amount} {type}");
        }

        /// <summary>Spawn une créature aléatoire dans l'inventaire.</summary>
        private void SpawnRandomCreature()
        {
            var inv = GameManager.Instance?.CurrentProfile?.Inventory;
            if (inv == null)
            {
                Debug.LogWarning("[DebugMenu] Pas de profil chargé.");
                return;
            }

            var c = new Creature
            {
                Name = $"Debug-{UnityEngine.Random.Range(100, 999)}",
                Generation = UnityEngine.Random.Range(0, 5),
                Skeleton = PickRandom(new[] { "Bipede", "Quadrupede", "Volant", "Serpentin" }),
                Shape = PickRandom(new[] { "Rond", "Triangle", "Carre", "Ovale", "Losange" }),
                Color = PickRandom(new[] { "Rouge", "Bleu", "Vert", "Jaune", "Violet" }),
                Strength = UnityEngine.Random.Range(3f, 20f),
                Agility = UnityEngine.Random.Range(3f, 20f),
                Intelligence = UnityEngine.Random.Range(3f, 20f),
                Luck = UnityEngine.Random.Range(3f, 20f),
                Constitution = UnityEngine.Random.Range(3f, 20f),
                Willpower = UnityEngine.Random.Range(3f, 20f)
            };
            c.CurrentHealth = c.MaxHealth;

            inv.AddCreature(c);
            Debug.Log($"[DebugMenu] Créature '{c.Name}' spawnée (Gen {c.Generation}).");
        }

        /// <summary>Force une sauvegarde immédiate.</summary>
        private void ForceSave()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame(SaveManager.Instance.ActiveSlot);
                Debug.Log("[DebugMenu] Sauvegarde forcée.");
            }
        }

        /// <summary>Force un rebirth.</summary>
        private void ForceRebirth()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Rebirth();
                Debug.Log("[DebugMenu] Rebirth forcé.");
            }
        }

        /// <summary>Retour au menu principal avec sauvegarde.</summary>
        private void OnReturnToMainMenu()
        {
            if (SaveManager.Instance != null && GameManager.Instance?.CurrentProfile != null)
            {
                SaveManager.Instance.SaveGame(SaveManager.Instance.ActiveSlot);
            }

            Hide();

            if (GameSceneManager.Instance != null)
                GameSceneManager.Instance.LoadMainMenu();
        }

        /// <summary>Toggle l'affichage de la console de logs.</summary>
        private void ToggleConsole()
        {
            _showConsole = !_showConsole;
            var consoleSection = _panel.transform.Find("ConsoleSection");
            if (consoleSection != null)
                consoleSection.gameObject.SetActive(_showConsole);
        }

        // ==========================
        //  Log Receiver
        // ==========================

        /// <summary>Reçoit les messages Debug.Log pour la console intégrée.</summary>
        private void OnLogReceived(string logString, string stackTrace, LogType type)
        {
            string prefix = type switch
            {
                LogType.Error => "<color=#FF4444>[ERR]</color>",
                LogType.Warning => "<color=#FFAA44>[WRN]</color>",
                LogType.Exception => "<color=#FF0000>[EXC]</color>",
                _ => "<color=#88FF88>[LOG]</color>"
            };

            _logBuffer.Add($"{prefix} {logString}");
            while (_logBuffer.Count > MAX_LOG_LINES)
                _logBuffer.RemoveAt(0);

            if (_consoleText != null && _showConsole)
            {
                _consoleText.text = string.Join("\n", _logBuffer);
            }
        }

        // ==========================
        //  Helpers
        // ==========================

        private string PickRandom(string[] array)
        {
            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        private RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<RectTransform>();
        }

        private void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private TMP_Text CreateText(RectTransform parent, string name, string text, int fontSize,
            Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.enableAutoSizing = false;

            return tmp;
        }

        private Button CreateButton(RectTransform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Color bgColor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            go.GetComponent<Image>().color = bgColor;

            var textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(go.transform, false);

            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            var tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 13;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return go.GetComponent<Button>();
        }
    }
}
#endif
