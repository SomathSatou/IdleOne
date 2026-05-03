using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using Assets.Script.UI.Tabs;
using Assets.Script.Managers;

namespace Assets.Script.UI.Settings
{
    /// <summary>
    /// Spawner programmatique qui construit le panneau Settings complet en runtime.
    /// Crée un Canvas overlay séparé (sort order supérieur à MainUI),
    /// le TabManager avec 3 sous-onglets (Sauvegarde, Audio, Graphismes),
    /// et tous les contrôles UI associés.
    /// GDD §17 — Paramètres du Jeu.
    /// </summary>
    public class SettingsSpawner : MonoBehaviour
    {
        // ==========================
        //  Références internes
        // ==========================

        private Canvas _settingsCanvas;
        private SettingsUI _settingsUI;

        /// <summary>Référence publique au SettingsUI pour le toggle depuis l'extérieur.</summary>
        public SettingsUI SettingsUI => _settingsUI;

        // ==========================
        //  Couleurs du thème
        // ==========================

        private static readonly Color BgColor = new Color(0.06f, 0.06f, 0.10f, 0.97f);
        private static readonly Color PanelColor = new Color(0.10f, 0.10f, 0.16f, 0.95f);
        private static readonly Color HeaderColor = new Color(0.12f, 0.12f, 0.20f, 1f);
        private static readonly Color ButtonColor = new Color(0.3f, 0.5f, 0.7f, 1f);
        private static readonly Color ButtonHighlight = new Color(0.5f, 0.7f, 0.9f);
        private static readonly Color TabInactive = new Color(0.2f, 0.2f, 0.2f, 1f);
        private static readonly Color SliderBg = new Color(0.15f, 0.15f, 0.20f, 1f);
        private static readonly Color SliderFill = new Color(0.3f, 0.6f, 0.9f, 1f);
        private static readonly Color ToggleBg = new Color(0.15f, 0.15f, 0.20f, 1f);
        private static readonly Color ToggleCheckmark = new Color(0.3f, 0.8f, 0.4f, 1f);
        private static readonly Color SectionColor = new Color(0.08f, 0.08f, 0.14f, 0.8f);

        // ==========================
        //  Construction
        // ==========================

        /// <summary>
        /// Construit le panneau Settings complet. Appelé depuis Start ou un autre spawner.
        /// Le panneau est masqué par défaut.
        /// GDD §17.1.
        /// </summary>
        public void Build()
        {
            EnsureSettingsManager();
            CreateOverlayCanvas();
            BuildSettingsPanel();
            _settingsCanvas.gameObject.SetActive(false);
            Debug.Log("[SettingsSpawner] Panneau Settings construit (masqué par défaut).");
        }

        /// <summary>Ouvre le panneau de paramètres. GDD §17.1.</summary>
        public void Open()
        {
            if (_settingsCanvas != null)
                _settingsCanvas.gameObject.SetActive(true);
            if (_settingsUI != null)
                _settingsUI.Show();
        }

        /// <summary>Ferme le panneau de paramètres. GDD §17.1.</summary>
        public void Close()
        {
            if (_settingsUI != null)
                _settingsUI.Hide();
            if (_settingsCanvas != null)
                _settingsCanvas.gameObject.SetActive(false);
        }

        /// <summary>Bascule la visibilité du panneau. GDD §17.1.</summary>
        public void Toggle()
        {
            if (_settingsCanvas != null && _settingsCanvas.gameObject.activeSelf)
                Close();
            else
                Open();
        }

        // ==========================
        //  Setup
        // ==========================

        /// <summary>Garantit qu'un SettingsManager existe.</summary>
        private void EnsureSettingsManager()
        {
            if (SettingsManager.Instance == null)
            {
                var go = new GameObject("SettingsManager");
                go.AddComponent<SettingsManager>();
            }
        }

        /// <summary>Crée le Canvas overlay pour le panneau Settings (sort order supérieur).</summary>
        private void CreateOverlayCanvas()
        {
            var canvasGO = new GameObject("SettingsCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);

            _settingsCanvas = canvasGO.GetComponent<Canvas>();
            _settingsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _settingsCanvas.sortingOrder = 100;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            if (EventSystem.current == null)
            {
                var esGO = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                esGO.GetComponent<InputSystemUIInputModule>();
            }
        }

        // ==========================
        //  Construction du panneau principal
        // ==========================

        /// <summary>Construit la hiérarchie complète du panneau Settings. GDD §17.1.</summary>
        private void BuildSettingsPanel()
        {
            var root = _settingsCanvas.transform;

            // Background semi-transparent (bloquant les clics)
            var bg = CreatePanel(root, "SettingsBg", BgColor);
            StretchFull(bg);

            // Panneau central
            var mainPanel = CreatePanel(root, "SettingsMainPanel", PanelColor);
            mainPanel.anchorMin = new Vector2(0.10f, 0.05f);
            mainPanel.anchorMax = new Vector2(0.90f, 0.95f);
            mainPanel.offsetMin = Vector2.zero;
            mainPanel.offsetMax = Vector2.zero;

            // SettingsUI component
            _settingsUI = mainPanel.gameObject.AddComponent<SettingsUI>();

            // Header : "PARAMÈTRES" + bouton ✕
            var header = CreatePanel(mainPanel, "Header", HeaderColor);
            header.anchorMin = new Vector2(0f, 0.92f);
            header.anchorMax = new Vector2(1f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            var title = CreateTextDirect(header, "Title", "PARAMÈTRES", 24,
                new Vector2(0.02f, 0.1f), new Vector2(0.85f, 0.9f),
                TextAlignmentOptions.Left, Color.white);

            var closeBtn = CreateButton(header, "CloseButton", "✕",
                new Vector2(0.88f, 0.1f), new Vector2(0.98f, 0.9f));
            closeBtn.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f, 1f);

            // Bouton Réinitialiser (en bas)
            var resetBtn = CreateButton(mainPanel, "ResetButton", "Réinitialiser les paramètres",
                new Vector2(0.30f, 0.01f), new Vector2(0.70f, 0.06f));
            resetBtn.GetComponent<Image>().color = new Color(0.5f, 0.3f, 0.2f, 1f);

            // Barre de sous-onglets
            var tabBar = CreatePanel(mainPanel, "SettingsTabBar", new Color(0.12f, 0.12f, 0.18f, 0.95f));
            tabBar.anchorMin = new Vector2(0f, 0.85f);
            tabBar.anchorMax = new Vector2(1f, 0.92f);
            tabBar.offsetMin = Vector2.zero;
            tabBar.offsetMax = Vector2.zero;

            // TabManager
            var tabManagerGO = new GameObject("SettingsTabManager");
            tabManagerGO.transform.SetParent(mainPanel, false);
            var tabManager = tabManagerGO.AddComponent<TabManager>();

            // 3 boutons d'onglets
            var btnSave = CreateTabButton(tabBar, "TabSave", "Sauvegarde",
                new Vector2(0.02f, 0.1f), new Vector2(0.33f, 0.9f));
            var btnAudio = CreateTabButton(tabBar, "TabAudio", "Audio",
                new Vector2(0.35f, 0.1f), new Vector2(0.65f, 0.9f));
            var btnGraphics = CreateTabButton(tabBar, "TabGraphics", "Graphismes",
                new Vector2(0.67f, 0.1f), new Vector2(0.98f, 0.9f));

            // 3 panneaux de contenu
            var contentSave = CreateContentPanel(mainPanel, "ContentSave");
            var contentAudio = CreateContentPanel(mainPanel, "ContentAudio");
            var contentGraphics = CreateContentPanel(mainPanel, "ContentGraphics");

            // Enregistrer les onglets
            tabManager.AddTab(new TabDefinition { Name = "Sauvegarde", Content = contentSave.gameObject, TabButton = btnSave.GetComponent<Button>() });
            tabManager.AddTab(new TabDefinition { Name = "Audio", Content = contentAudio.gameObject, TabButton = btnAudio.GetComponent<Button>() });
            tabManager.AddTab(new TabDefinition { Name = "Graphismes", Content = contentGraphics.gameObject, TabButton = btnGraphics.GetComponent<Button>() });

            // Construire le contenu de chaque sous-onglet
            var saveUI = BuildSaveTab(contentSave);
            var audioUI = BuildAudioTab(contentAudio);
            var graphicsUI = BuildGraphicsTab(contentGraphics);

            // Initialiser le TabManager
            tabManager.InitializeAfterDynamicSetup();

            // Initialiser le SettingsUI
            _settingsUI.Initialize(
                tabManager,
                closeBtn.GetComponent<Button>(),
                resetBtn.GetComponent<Button>(),
                saveUI,
                audioUI,
                graphicsUI);
        }

        // ==========================
        //  Sous-onglet Sauvegarde — GDD §17.2
        // ==========================

        /// <summary>Construit le contenu de l'onglet Sauvegarde. GDD §17.2.</summary>
        private SaveSettingsUI BuildSaveTab(RectTransform parent)
        {
            // Section Auto-save
            var sectionAuto = CreatePanel(parent, "SectionAutoSave", SectionColor);
            sectionAuto.anchorMin = new Vector2(0.02f, 0.72f);
            sectionAuto.anchorMax = new Vector2(0.98f, 0.98f);
            sectionAuto.offsetMin = Vector2.zero;
            sectionAuto.offsetMax = Vector2.zero;

            CreateTextDirect(sectionAuto, "LabelAutoSave", "Auto-Sauvegarde", 18,
                new Vector2(0.02f, 0.78f), new Vector2(0.50f, 0.98f),
                TextAlignmentOptions.TopLeft, Color.white);

            var autoSaveToggle = CreateToggle(sectionAuto, "AutoSaveToggle",
                new Vector2(0.02f, 0.52f), new Vector2(0.15f, 0.76f));

            CreateTextDirect(sectionAuto, "ToggleLabel", "Activée", 14,
                new Vector2(0.16f, 0.52f), new Vector2(0.50f, 0.76f),
                TextAlignmentOptions.Left, Color.white);

            CreateTextDirect(sectionAuto, "IntervalTitle", "Intervalle :", 14,
                new Vector2(0.02f, 0.26f), new Vector2(0.25f, 0.50f),
                TextAlignmentOptions.Left, Color.white);

            var intervalSlider = CreateSlider(sectionAuto, "IntervalSlider",
                new Vector2(0.26f, 0.28f), new Vector2(0.75f, 0.48f));

            var intervalLabel = CreateTextDirect(sectionAuto, "IntervalValue", "5 min", 14,
                new Vector2(0.77f, 0.26f), new Vector2(0.98f, 0.50f),
                TextAlignmentOptions.Left, Color.white);

            var saveNowBtn = CreateButton(sectionAuto, "SaveNowBtn", "Sauvegarder maintenant",
                new Vector2(0.55f, 0.52f), new Vector2(0.98f, 0.76f));

            var lastSaveLabel = CreateTextDirect(sectionAuto, "LastSaveLabel", "Dernière sauvegarde : —", 12,
                new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.24f),
                TextAlignmentOptions.Left, new Color(0.7f, 0.7f, 0.7f));

            // Section Export
            var sectionExport = CreatePanel(parent, "SectionExport", SectionColor);
            sectionExport.anchorMin = new Vector2(0.02f, 0.40f);
            sectionExport.anchorMax = new Vector2(0.98f, 0.70f);
            sectionExport.offsetMin = Vector2.zero;
            sectionExport.offsetMax = Vector2.zero;

            CreateTextDirect(sectionExport, "LabelExport", "Export", 18,
                new Vector2(0.02f, 0.78f), new Vector2(0.50f, 0.98f),
                TextAlignmentOptions.TopLeft, Color.white);

            CreateTextDirect(sectionExport, "SlotLabel", "Slot à exporter :", 14,
                new Vector2(0.02f, 0.40f), new Vector2(0.30f, 0.70f),
                TextAlignmentOptions.Left, Color.white);

            var exportDropdown = CreateDropdown(sectionExport, "ExportSlotDropdown",
                new Vector2(0.32f, 0.42f), new Vector2(0.60f, 0.68f));

            var exportBtn = CreateButton(sectionExport, "ExportBtn", "Exporter",
                new Vector2(0.65f, 0.42f), new Vector2(0.98f, 0.68f));

            // Section Import
            var sectionImport = CreatePanel(parent, "SectionImport", SectionColor);
            sectionImport.anchorMin = new Vector2(0.02f, 0.08f);
            sectionImport.anchorMax = new Vector2(0.98f, 0.38f);
            sectionImport.offsetMin = Vector2.zero;
            sectionImport.offsetMax = Vector2.zero;

            CreateTextDirect(sectionImport, "LabelImport", "Import", 18,
                new Vector2(0.02f, 0.78f), new Vector2(0.50f, 0.98f),
                TextAlignmentOptions.TopLeft, Color.white);

            CreateTextDirect(sectionImport, "DestLabel", "Slot de destination :", 14,
                new Vector2(0.02f, 0.40f), new Vector2(0.35f, 0.70f),
                TextAlignmentOptions.Left, Color.white);

            var importDropdown = CreateDropdown(sectionImport, "ImportSlotDropdown",
                new Vector2(0.37f, 0.42f), new Vector2(0.60f, 0.68f));

            var importBtn = CreateButton(sectionImport, "ImportBtn", "Importer un fichier",
                new Vector2(0.65f, 0.42f), new Vector2(0.98f, 0.68f));

            // SaveSettingsUI component
            var saveUI = parent.gameObject.AddComponent<SaveSettingsUI>();
            saveUI.Initialize(
                autoSaveToggle,
                intervalSlider,
                intervalLabel,
                saveNowBtn.GetComponent<Button>(),
                exportDropdown,
                exportBtn.GetComponent<Button>(),
                importBtn.GetComponent<Button>(),
                importDropdown,
                lastSaveLabel);

            return saveUI;
        }

        // ==========================
        //  Sous-onglet Audio — GDD §17.3
        // ==========================

        /// <summary>Construit le contenu de l'onglet Audio. GDD §17.3.</summary>
        private AudioSettingsUI BuildAudioTab(RectTransform parent)
        {
            // Volume Master
            CreateTextDirect(parent, "MasterTitle", "Volume Master", 16,
                new Vector2(0.02f, 0.85f), new Vector2(0.30f, 0.95f),
                TextAlignmentOptions.Left, Color.white);

            var masterSlider = CreateSlider(parent, "MasterSlider",
                new Vector2(0.32f, 0.86f), new Vector2(0.85f, 0.94f));

            var masterLabel = CreateTextDirect(parent, "MasterValue", "100%", 14,
                new Vector2(0.87f, 0.85f), new Vector2(0.98f, 0.95f),
                TextAlignmentOptions.Left, Color.white);

            // Volume Musique
            CreateTextDirect(parent, "MusicTitle", "Volume Musique", 16,
                new Vector2(0.02f, 0.70f), new Vector2(0.30f, 0.80f),
                TextAlignmentOptions.Left, Color.white);

            var musicSlider = CreateSlider(parent, "MusicSlider",
                new Vector2(0.32f, 0.71f), new Vector2(0.85f, 0.79f));

            var musicLabel = CreateTextDirect(parent, "MusicValue", "70%", 14,
                new Vector2(0.87f, 0.70f), new Vector2(0.98f, 0.80f),
                TextAlignmentOptions.Left, Color.white);

            // Volume SFX
            CreateTextDirect(parent, "SFXTitle", "Volume SFX", 16,
                new Vector2(0.02f, 0.55f), new Vector2(0.30f, 0.65f),
                TextAlignmentOptions.Left, Color.white);

            var sfxSlider = CreateSlider(parent, "SFXSlider",
                new Vector2(0.32f, 0.56f), new Vector2(0.85f, 0.64f));

            var sfxLabel = CreateTextDirect(parent, "SFXValue", "100%", 14,
                new Vector2(0.87f, 0.55f), new Vector2(0.98f, 0.65f),
                TextAlignmentOptions.Left, Color.white);

            // Mute global
            var muteToggle = CreateToggle(parent, "MuteToggle",
                new Vector2(0.02f, 0.38f), new Vector2(0.08f, 0.48f));

            CreateTextDirect(parent, "MuteLabel", "Muet global", 16,
                new Vector2(0.10f, 0.38f), new Vector2(0.40f, 0.48f),
                TextAlignmentOptions.Left, Color.white);

            // AudioSettingsUI component
            var audioUI = parent.gameObject.AddComponent<AudioSettingsUI>();
            audioUI.Initialize(
                masterSlider,
                masterLabel,
                musicSlider,
                musicLabel,
                sfxSlider,
                sfxLabel,
                muteToggle);

            return audioUI;
        }

        // ==========================
        //  Sous-onglet Graphismes — GDD §17.4
        // ==========================

        /// <summary>Construit le contenu de l'onglet Graphismes. GDD §17.4.</summary>
        private GraphicsSettingsUI BuildGraphicsTab(RectTransform parent)
        {
            // Qualité
            CreateTextDirect(parent, "QualityTitle", "Qualité graphique", 16,
                new Vector2(0.02f, 0.82f), new Vector2(0.35f, 0.95f),
                TextAlignmentOptions.Left, Color.white);

            var qualityDropdown = CreateDropdown(parent, "QualityDropdown",
                new Vector2(0.37f, 0.84f), new Vector2(0.75f, 0.94f));

            // Plein écran
            var fullscreenToggle = CreateToggle(parent, "FullscreenToggle",
                new Vector2(0.02f, 0.66f), new Vector2(0.08f, 0.76f));

            CreateTextDirect(parent, "FullscreenLabel", "Plein écran", 16,
                new Vector2(0.10f, 0.66f), new Vector2(0.40f, 0.76f),
                TextAlignmentOptions.Left, Color.white);

            // Résolution
            CreateTextDirect(parent, "ResolutionTitle", "Résolution", 16,
                new Vector2(0.02f, 0.50f), new Vector2(0.35f, 0.60f),
                TextAlignmentOptions.Left, Color.white);

            var resolutionDropdown = CreateDropdown(parent, "ResolutionDropdown",
                new Vector2(0.37f, 0.50f), new Vector2(0.85f, 0.60f));

            // Bouton Appliquer
            var applyBtn = CreateButton(parent, "ApplyBtn", "Appliquer",
                new Vector2(0.30f, 0.30f), new Vector2(0.70f, 0.42f));
            applyBtn.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.3f, 1f);

            // GraphicsSettingsUI component
            var graphicsUI = parent.gameObject.AddComponent<GraphicsSettingsUI>();
            graphicsUI.Initialize(
                qualityDropdown,
                fullscreenToggle,
                resolutionDropdown,
                applyBtn.GetComponent<Button>());

            return graphicsUI;
        }

        // ==========================
        //  Helpers UI (même pattern que MainUISpawner)
        // ==========================

        /// <summary>Crée un panneau UI rectangulaire avec Image.</summary>
        private RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<RectTransform>();
        }

        /// <summary>Crée un panneau de contenu pour un onglet (zone sous les tabs).</summary>
        private RectTransform CreateContentPanel(Transform parent, string name)
        {
            var rt = CreatePanel(parent, name, new Color(0.08f, 0.08f, 0.12f, 0.95f));
            rt.anchorMin = new Vector2(0.01f, 0.07f);
            rt.anchorMax = new Vector2(0.99f, 0.84f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        /// <summary>Crée un TMP_Text positionné directement par ancres.</summary>
        private TMP_Text CreateTextDirect(Transform parent, string name, string content, int fontSize,
            Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions alignment, Color color)
        {
            var go = new GameObject(name, typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.richText = true;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return tmp;
        }

        /// <summary>Crée un bouton UI standard avec label TMPro.</summary>
        private RectTransform CreateButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = ButtonColor;

            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = ButtonHighlight;
            btn.colors = colors;

            var labelGO = new GameObject("Label", typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(go.transform, false);
            var tmp = labelGO.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var labelRt = labelGO.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        /// <summary>Crée un bouton d'onglet (même style que TabManager attend).</summary>
        private RectTransform CreateTabButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var rt = CreateButton(parent, name, label, anchorMin, anchorMax);
            var img = rt.GetComponent<Image>();
            if (img != null)
                img.color = TabInactive;

            var tmp = rt.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
                tmp.fontSize = 18;

            return rt;
        }

        /// <summary>
        /// Crée un Slider UI complet (Background + Fill + Handle).
        /// Retourne le composant Slider.
        /// </summary>
        private Slider CreateSlider(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(Slider));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Background
            var bgGO = new GameObject("Background", typeof(Image));
            bgGO.transform.SetParent(go.transform, false);
            bgGO.GetComponent<Image>().color = SliderBg;
            var bgRt = bgGO.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.25f);
            bgRt.anchorMax = new Vector2(1f, 0.75f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            // Fill Area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRt = fillArea.GetComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRt.offsetMin = new Vector2(5f, 0f);
            fillAreaRt.offsetMax = new Vector2(-5f, 0f);

            var fill = new GameObject("Fill", typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            fill.GetComponent<Image>().color = SliderFill;
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            // Handle Slide Area
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var handleAreaRt = handleArea.GetComponent<RectTransform>();
            handleAreaRt.anchorMin = new Vector2(0f, 0f);
            handleAreaRt.anchorMax = new Vector2(1f, 1f);
            handleAreaRt.offsetMin = new Vector2(10f, 0f);
            handleAreaRt.offsetMax = new Vector2(-10f, 0f);

            var handle = new GameObject("Handle", typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            handle.GetComponent<Image>().color = Color.white;
            var handleRt = handle.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(20f, 0f);

            // Wire Slider
            var slider = go.GetComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handle.GetComponent<Image>();

            return slider;
        }

        /// <summary>
        /// Crée un Toggle UI complet (Background + Checkmark).
        /// Retourne le composant Toggle.
        /// </summary>
        private Toggle CreateToggle(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(Toggle));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Background
            var bgGO = new GameObject("Background", typeof(Image));
            bgGO.transform.SetParent(go.transform, false);
            bgGO.GetComponent<Image>().color = ToggleBg;
            var bgRt = bgGO.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            // Checkmark
            var checkGO = new GameObject("Checkmark", typeof(Image));
            checkGO.transform.SetParent(bgGO.transform, false);
            checkGO.GetComponent<Image>().color = ToggleCheckmark;
            var checkRt = checkGO.GetComponent<RectTransform>();
            checkRt.anchorMin = new Vector2(0.15f, 0.15f);
            checkRt.anchorMax = new Vector2(0.85f, 0.85f);
            checkRt.offsetMin = Vector2.zero;
            checkRt.offsetMax = Vector2.zero;

            // Wire Toggle
            var toggle = go.GetComponent<Toggle>();
            toggle.targetGraphic = bgGO.GetComponent<Image>();
            toggle.graphic = checkGO.GetComponent<Image>();

            return toggle;
        }

        /// <summary>
        /// Crée un TMP_Dropdown UI complet.
        /// Retourne le composant TMP_Dropdown.
        /// </summary>
        private TMP_Dropdown CreateDropdown(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(Image), typeof(TMP_Dropdown));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.22f, 1f);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Caption Text
            var captionGO = new GameObject("Label", typeof(TextMeshProUGUI));
            captionGO.transform.SetParent(go.transform, false);
            var captionTMP = captionGO.GetComponent<TextMeshProUGUI>();
            captionTMP.fontSize = 14;
            captionTMP.alignment = TextAlignmentOptions.Left;
            captionTMP.color = Color.white;
            var captionRt = captionGO.GetComponent<RectTransform>();
            captionRt.anchorMin = new Vector2(0.05f, 0f);
            captionRt.anchorMax = new Vector2(0.85f, 1f);
            captionRt.offsetMin = Vector2.zero;
            captionRt.offsetMax = Vector2.zero;

            // Arrow indicator
            var arrowGO = new GameObject("Arrow", typeof(Image));
            arrowGO.transform.SetParent(go.transform, false);
            arrowGO.GetComponent<Image>().color = Color.white;
            var arrowRt = arrowGO.GetComponent<RectTransform>();
            arrowRt.anchorMin = new Vector2(0.88f, 0.25f);
            arrowRt.anchorMax = new Vector2(0.96f, 0.75f);
            arrowRt.offsetMin = Vector2.zero;
            arrowRt.offsetMax = Vector2.zero;

            // Template (dropdown list)
            var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            template.transform.SetParent(go.transform, false);
            template.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.18f, 1f);
            var templateRt = template.GetComponent<RectTransform>();
            templateRt.anchorMin = new Vector2(0f, 0f);
            templateRt.anchorMax = new Vector2(1f, 0f);
            templateRt.pivot = new Vector2(0.5f, 1f);
            templateRt.sizeDelta = new Vector2(0f, 150f);

            // Viewport
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(template.transform, false);
            viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;
            var vpRt = viewport.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;

            // Content
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = new Vector2(0f, 28f);

            // Item template
            var item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
            item.transform.SetParent(content.transform, false);
            var itemRt = item.GetComponent<RectTransform>();
            itemRt.anchorMin = new Vector2(0f, 0.5f);
            itemRt.anchorMax = new Vector2(1f, 0.5f);
            itemRt.sizeDelta = new Vector2(0f, 28f);

            // Item background
            var itemBg = new GameObject("Item Background", typeof(Image));
            itemBg.transform.SetParent(item.transform, false);
            itemBg.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.22f, 1f);
            var itemBgRt = itemBg.GetComponent<RectTransform>();
            itemBgRt.anchorMin = Vector2.zero;
            itemBgRt.anchorMax = Vector2.one;
            itemBgRt.offsetMin = Vector2.zero;
            itemBgRt.offsetMax = Vector2.zero;

            // Item checkmark
            var itemCheck = new GameObject("Item Checkmark", typeof(Image));
            itemCheck.transform.SetParent(item.transform, false);
            itemCheck.GetComponent<Image>().color = ToggleCheckmark;
            var itemCheckRt = itemCheck.GetComponent<RectTransform>();
            itemCheckRt.anchorMin = new Vector2(0f, 0.2f);
            itemCheckRt.anchorMax = new Vector2(0.08f, 0.8f);
            itemCheckRt.offsetMin = new Vector2(2f, 0f);
            itemCheckRt.offsetMax = Vector2.zero;

            // Item label
            var itemLabel = new GameObject("Item Label", typeof(TextMeshProUGUI));
            itemLabel.transform.SetParent(item.transform, false);
            var itemTMP = itemLabel.GetComponent<TextMeshProUGUI>();
            itemTMP.fontSize = 14;
            itemTMP.alignment = TextAlignmentOptions.Left;
            itemTMP.color = Color.white;
            var itemLabelRt = itemLabel.GetComponent<RectTransform>();
            itemLabelRt.anchorMin = new Vector2(0.1f, 0f);
            itemLabelRt.anchorMax = Vector2.one;
            itemLabelRt.offsetMin = Vector2.zero;
            itemLabelRt.offsetMax = Vector2.zero;

            // Wire toggle
            var itemToggle = item.GetComponent<Toggle>();
            itemToggle.targetGraphic = itemBg.GetComponent<Image>();
            itemToggle.graphic = itemCheck.GetComponent<Image>();

            // Wire ScrollRect
            var scrollRect = template.GetComponent<ScrollRect>();
            scrollRect.content = contentRt;
            scrollRect.viewport = vpRt;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // Wire Dropdown
            var dropdown = go.GetComponent<TMP_Dropdown>();
            dropdown.captionText = captionTMP;
            dropdown.itemText = itemTMP;
            dropdown.template = templateRt;

            // Désactiver le template par défaut
            template.SetActive(false);

            return dropdown;
        }

        /// <summary>Étire un RectTransform pour remplir tout son parent.</summary>
        private void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
