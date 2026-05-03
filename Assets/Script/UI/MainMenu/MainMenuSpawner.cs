using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using Assets.Script.Managers;
using Assets.Script.UI.Settings;

namespace Assets.Script.UI.MainMenu
{
    /// <summary>
    /// Spawner programmatique qui construit le menu principal complet en runtime.
    /// Crée le Canvas, les boutons de navigation, le panneau "Nouvelle Partie"
    /// et intègre le SaveSlotUI et le SettingsSpawner.
    /// GDD §18.2 — Menu Principal.
    /// </summary>
    public class MainMenuSpawner : MonoBehaviour
    {
        // ==========================
        //  Références internes
        // ==========================

        private Canvas _menuCanvas;
        private SaveSlotUI _saveSlotUI;
        private SettingsSpawner _settingsSpawner;

        private GameObject _mainPanel;
        private GameObject _newGamePanel;
        private TMP_InputField _nameInput;

        // ==========================
        //  Couleurs du thème
        // ==========================

        private static readonly Color BgColor = new Color(0.06f, 0.06f, 0.10f, 1f);
        private static readonly Color PanelColor = new Color(0.10f, 0.10f, 0.16f, 0.95f);
        private static readonly Color HeaderColor = new Color(0.12f, 0.12f, 0.20f, 1f);
        private static readonly Color ButtonColor = new Color(0.3f, 0.5f, 0.7f, 1f);
        private static readonly Color ButtonHighlight = new Color(0.4f, 0.6f, 0.8f, 1f);
        private static readonly Color AccentColor = new Color(0.4f, 0.7f, 1f, 1f);
        private static readonly Color TextColor = Color.white;
        private static readonly Color TextDimColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color InputBgColor = new Color(0.15f, 0.15f, 0.22f, 1f);

        // ==========================
        //  Cycle de vie Unity
        // ==========================

        private void Start()
        {
            EnsureManagers();
            SetupCanvas();
            BuildMainMenu();
            BuildNewGamePanel();
            BuildSaveSlots();
            BuildSettingsSpawner();

            Debug.Log("[MainMenuSpawner] Menu principal construit.");
        }

        // ==========================
        //  Setup
        // ==========================

        /// <summary>Garantit que les managers existent (fallback si Bootstrap n'a pas été chargé).</summary>
        private void EnsureManagers()
        {
            if (GameManager.Instance == null)
            {
                var go = new GameObject("GameManager");
                go.AddComponent<GameManager>();
            }
            if (SaveManager.Instance == null)
            {
                var go = new GameObject("SaveManager");
                go.AddComponent<SaveManager>();
            }
            if (SettingsManager.Instance == null)
            {
                var go = new GameObject("SettingsManager");
                go.AddComponent<SettingsManager>();
            }
            if (GameSceneManager.Instance == null)
            {
                var go = new GameObject("GameSceneManager");
                go.AddComponent<GameSceneManager>();
            }
        }

        /// <summary>Crée le Canvas plein écran pour le menu principal.</summary>
        private void SetupCanvas()
        {
            var canvasGO = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);

            _menuCanvas = canvasGO.GetComponent<Canvas>();
            _menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _menuCanvas.sortingOrder = 0;

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
        //  Construction du menu principal — GDD §18.2
        // ==========================

        /// <summary>Construit la hiérarchie du menu principal.</summary>
        private void BuildMainMenu()
        {
            var root = _menuCanvas.transform;

            // Background
            var bg = CreatePanel(root, "Background", BgColor);
            StretchFull(bg);

            // Panneau central
            _mainPanel = new GameObject("MainPanel", typeof(RectTransform));
            _mainPanel.transform.SetParent(root, false);
            var mainRT = _mainPanel.GetComponent<RectTransform>();
            mainRT.anchorMin = new Vector2(0.25f, 0.10f);
            mainRT.anchorMax = new Vector2(0.75f, 0.90f);
            mainRT.offsetMin = Vector2.zero;
            mainRT.offsetMax = Vector2.zero;

            // Titre du jeu
            CreateText(mainRT, "GameTitle", "IdleOne", 60,
                new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.98f),
                TextAlignmentOptions.Center, AccentColor);

            // Sous-titre
            CreateText(mainRT, "Subtitle", "Idle Breeding Game", 20,
                new Vector2(0.05f, 0.70f), new Vector2(0.95f, 0.78f),
                TextAlignmentOptions.Center, TextDimColor);

            // Boutons
            var btnNewGame = CreateMenuButton(mainRT, "BtnNewGame", "Nouvelle Partie",
                new Vector2(0.20f, 0.52f), new Vector2(0.80f, 0.64f));
            btnNewGame.onClick.AddListener(OnNewGameClicked);

            var btnLoadGame = CreateMenuButton(mainRT, "BtnLoadGame", "Charger Partie",
                new Vector2(0.20f, 0.38f), new Vector2(0.80f, 0.50f));
            btnLoadGame.onClick.AddListener(OnLoadGameClicked);

            var btnSettings = CreateMenuButton(mainRT, "BtnSettings", "Paramètres",
                new Vector2(0.20f, 0.24f), new Vector2(0.80f, 0.36f));
            btnSettings.onClick.AddListener(OnSettingsClicked);

            var btnQuit = CreateMenuButton(mainRT, "BtnQuit", "Quitter",
                new Vector2(0.20f, 0.10f), new Vector2(0.80f, 0.22f));
            btnQuit.onClick.AddListener(OnQuitClicked);

            // Version du jeu
            CreateText(mainRT, "VersionLabel", $"v{Application.version}", 14,
                new Vector2(0.60f, 0.01f), new Vector2(0.98f, 0.08f),
                TextAlignmentOptions.BottomRight, TextDimColor);
        }

        // ==========================
        //  Panneau "Nouvelle Partie" — GDD §18.2
        // ==========================

        /// <summary>Construit le panneau de saisie du nom pour une nouvelle partie.</summary>
        private void BuildNewGamePanel()
        {
            _newGamePanel = CreatePanel(_menuCanvas.transform, "NewGamePanel", new Color(0.06f, 0.06f, 0.10f, 0.97f)).gameObject;
            StretchFull(_newGamePanel.GetComponent<RectTransform>());

            var panelRT = _newGamePanel.GetComponent<RectTransform>();

            // Panneau central
            var innerPanel = CreatePanel(panelRT.transform, "InnerPanel", PanelColor);
            var innerRT = innerPanel.GetComponent<RectTransform>();
            innerRT.anchorMin = new Vector2(0.25f, 0.25f);
            innerRT.anchorMax = new Vector2(0.75f, 0.75f);
            innerRT.offsetMin = Vector2.zero;
            innerRT.offsetMax = Vector2.zero;

            CreateText(innerRT, "Title", "Nouvelle Partie", 30,
                new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.95f),
                TextAlignmentOptions.Center, TextColor);

            CreateText(innerRT, "NameLabel", "Nom du joueur :", 18,
                new Vector2(0.10f, 0.55f), new Vector2(0.90f, 0.70f),
                TextAlignmentOptions.Left, TextColor);

            // Champ de saisie
            _nameInput = CreateInputField(innerRT, "NameInput", "Entrez votre nom...",
                new Vector2(0.10f, 0.38f), new Vector2(0.90f, 0.55f));

            // Boutons
            var btnConfirm = CreateMenuButton(innerRT, "BtnConfirm", "Commencer",
                new Vector2(0.10f, 0.10f), new Vector2(0.48f, 0.30f));
            btnConfirm.onClick.AddListener(OnConfirmNewGame);

            var btnCancel = CreateMenuButton(innerRT, "BtnCancel", "Annuler",
                new Vector2(0.52f, 0.10f), new Vector2(0.90f, 0.30f));
            btnCancel.onClick.AddListener(() => _newGamePanel.SetActive(false));

            _newGamePanel.SetActive(false);
        }

        // ==========================
        //  Save Slots — GDD §18.3
        // ==========================

        /// <summary>Construit le panneau de slots de sauvegarde.</summary>
        private void BuildSaveSlots()
        {
            var saveSlotGO = new GameObject("SaveSlotUI");
            saveSlotGO.transform.SetParent(_menuCanvas.transform, false);
            _saveSlotUI = saveSlotGO.AddComponent<SaveSlotUI>();

            var canvasRT = _menuCanvas.GetComponent<RectTransform>();
            _saveSlotUI.Build(canvasRT);

            _saveSlotUI.OnSlotLoaded += () =>
            {
                GameSceneManager.Instance.LoadMainUI();
            };

            _saveSlotUI.OnBackRequested += () =>
            {
                _saveSlotUI.Hide();
            };
        }

        // ==========================
        //  Settings — GDD §17
        // ==========================

        /// <summary>Instancie le SettingsSpawner pour le menu principal.</summary>
        private void BuildSettingsSpawner()
        {
            var settingsGO = new GameObject("SettingsSpawner");
            settingsGO.transform.SetParent(transform, false);
            _settingsSpawner = settingsGO.AddComponent<SettingsSpawner>();
            _settingsSpawner.Build();
        }

        // ==========================
        //  Callbacks — GDD §18.2
        // ==========================

        /// <summary>Ouvre le panneau de nouvelle partie.</summary>
        private void OnNewGameClicked()
        {
            _newGamePanel.SetActive(true);
            if (_nameInput != null)
            {
                _nameInput.text = "";
                _nameInput.ActivateInputField();
            }
        }

        /// <summary>Ouvre le panneau de chargement des slots.</summary>
        private void OnLoadGameClicked()
        {
            if (_saveSlotUI != null)
                _saveSlotUI.Show();
        }

        /// <summary>Ouvre le panneau de paramètres.</summary>
        private void OnSettingsClicked()
        {
            if (_settingsSpawner != null)
                _settingsSpawner.Open();
        }

        /// <summary>Quitte l'application. GDD §18.2.</summary>
        private void OnQuitClicked()
        {
            Debug.Log("[MainMenuSpawner] Quitter le jeu.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>Confirme la création d'une nouvelle partie.</summary>
        private void OnConfirmNewGame()
        {
            string playerName = _nameInput != null ? _nameInput.text.Trim() : "";
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Joueur";
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.NewGame(playerName);

                // Sauvegarder dans le premier slot disponible
                if (SaveManager.Instance != null)
                {
                    int slot = FindFirstEmptySlot();
                    SaveManager.Instance.ActiveSlot = slot;
                    SaveManager.Instance.SaveGame(slot);
                }

                Debug.Log($"[MainMenuSpawner] Nouvelle partie créée pour '{playerName}' → MainUI.");
                GameSceneManager.Instance.LoadMainUI();
            }
        }

        /// <summary>Trouve le premier slot de sauvegarde vide (ou 0 par défaut).</summary>
        private int FindFirstEmptySlot()
        {
            if (SaveManager.Instance == null) return 0;

            for (int i = 0; i < SaveManager.MAX_SLOTS; i++)
            {
                if (!SaveManager.Instance.SaveExists(i))
                    return i;
            }
            return 0;
        }

        // ==========================
        //  Helpers UI
        // ==========================

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

        private Button CreateMenuButton(RectTransform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            go.GetComponent<Image>().color = ButtonColor;

            // Hover colors
            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = ButtonHighlight;
            colors.pressedColor = new Color(0.2f, 0.4f, 0.6f, 1f);
            btn.colors = colors;

            // Label
            var textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(go.transform, false);

            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            var tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btn;
        }

        private TMP_InputField CreateInputField(RectTransform parent, string name, string placeholder,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            go.GetComponent<Image>().color = InputBgColor;

            // Text Area
            var textArea = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            textArea.transform.SetParent(go.transform, false);
            var areaRT = textArea.GetComponent<RectTransform>();
            areaRT.anchorMin = new Vector2(0.02f, 0.1f);
            areaRT.anchorMax = new Vector2(0.98f, 0.9f);
            areaRT.offsetMin = Vector2.zero;
            areaRT.offsetMax = Vector2.zero;

            // Placeholder
            var placeholderGO = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
            placeholderGO.transform.SetParent(textArea.transform, false);
            var phRT = placeholderGO.GetComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero;
            phRT.anchorMax = Vector2.one;
            phRT.offsetMin = Vector2.zero;
            phRT.offsetMax = Vector2.zero;
            var phTMP = placeholderGO.GetComponent<TextMeshProUGUI>();
            phTMP.text = placeholder;
            phTMP.fontSize = 18;
            phTMP.fontStyle = FontStyles.Italic;
            phTMP.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            phTMP.alignment = TextAlignmentOptions.Left;

            // Input text
            var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(textArea.transform, false);
            var txtRT = textGO.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;
            var txtTMP = textGO.GetComponent<TextMeshProUGUI>();
            txtTMP.fontSize = 18;
            txtTMP.color = Color.white;
            txtTMP.alignment = TextAlignmentOptions.Left;

            // Wire up TMP_InputField
            var inputField = go.GetComponent<TMP_InputField>();
            inputField.textViewport = areaRT;
            inputField.textComponent = txtTMP;
            inputField.placeholder = phTMP;
            inputField.characterLimit = 20;

            return inputField;
        }
    }
}
