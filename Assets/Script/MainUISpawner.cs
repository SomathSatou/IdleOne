using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using Assets.Script.UI;
using Assets.Script.UI.Tabs;
using Assets.Script.UI.Settings;
using Assets.Script.Managers;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Assets.Script.UI.DebugMenu;
#endif

namespace Assets.Script
{
    /// <summary>
    /// Spawner autonome qui construit la scène MainUI complète en runtime.
    /// Crée le Canvas, le header de ressources, la barre d'onglets (4 tabs)
    /// et les 4 panneaux de contenu (Créatures, Breeding, Inventaire, Stats).
    /// GDD §15 — Interface Utilisateur (UI).
    /// </summary>
    public class MainUISpawner : MonoBehaviour
    {
        [Header("Optional : assign existing canvas, or leave empty to auto-create")]
        [SerializeField] private Canvas targetCanvas;

        private TabManager _tabManager;
        private SettingsSpawner _settingsSpawner;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private DebugMenuUI _debugMenuUI;
#endif

        // Références header pour mise à jour temps réel
        private TMP_Text _headerFragments;
        private TMP_Text _headerEssence;
        private TMP_Text _headerGold;

        private void Start()
        {
            EnsureGameManager();
            SetupCanvas();
            BuildUI();
            Debug.Log("[MainUISpawner] MainUI built successfully.");
        }

        private void Update()
        {
            RefreshHeader();
        }

        // ==========================
        //  Setup
        // ==========================

        /// <summary>Garantit qu'un GameManager existe avec un profil chargé (mode debug).</summary>
        private void EnsureGameManager()
        {
            if (GameManager.Instance == null)
            {
                var managerGO = new GameObject("GameManager");
                managerGO.AddComponent<GameManager>();
            }

            if (GameManager.Instance.CurrentProfile == null)
            {
                GameManager.Instance.NewGame("Debug Player");
                Debug.Log("[MainUISpawner] Profil debug créé : 'Debug Player'");

                // Ajouter quelques créatures de test
                SpawnDebugCreatures();
            }
        }

        /// <summary>Crée des créatures de test pour le mode debug.</summary>
        private void SpawnDebugCreatures()
        {
            var inv = GameManager.Instance.CurrentProfile.Inventory;

            for (int i = 0; i < 5; i++)
            {
                var c = new Creatures.Creature
                {
                    Name = $"Créature-{i + 1}",
                    Generation = Random.Range(0, 3),
                    Skeleton = PickRandom(new[] { "Bipede", "Quadrupede", "Volant", "Serpentin" }),
                    Shape = PickRandom(new[] { "Rond", "Triangle", "Carre", "Ovale", "Losange" }),
                    Color = PickRandom(new[] { "Rouge", "Bleu", "Vert", "Jaune", "Violet", "Orange" }),
                    Strength = Random.Range(3f, 15f),
                    Agility = Random.Range(3f, 15f),
                    Intelligence = Random.Range(3f, 15f),
                    Luck = Random.Range(3f, 15f),
                    Constitution = Random.Range(3f, 15f),
                    Willpower = Random.Range(3f, 15f)
                };
                c.CurrentHealth = c.MaxHealth;
                inv.AddCreature(c);
            }

            // Ajouter des ressources de test
            GameManager.Instance.CurrentProfile.Resources.Add(Entities.ResourceType.Fragments, 500f);
            GameManager.Instance.CurrentProfile.Resources.Add(Entities.ResourceType.Essence, 25f);
            GameManager.Instance.CurrentProfile.Resources.Add(Entities.ResourceType.Gold, 100f);
        }

        private void SetupCanvas()
        {
            if (targetCanvas != null) return;

            var canvasGO = new GameObject("MainUICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            targetCanvas = canvasGO.GetComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // EventSystem requis pour les clics UI avec le nouveau Input System Package
            if (EventSystem.current == null)
            {
                var esGO = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                esGO.GetComponent<InputSystemUIInputModule>();
            }
        }

        // ==========================
        //  Construction UI
        // ==========================

        private void BuildUI()
        {
            var root = targetCanvas.transform;

            // Background
            var bg = CreatePanel(root, "Background", new Color(0.08f, 0.08f, 0.12f, 1f));
            StretchFull(bg);

            // Header (ressources persistantes)
            BuildHeader(root);

            // Barre d'onglets
            var tabBar = CreatePanel(root, "TabBar", new Color(0.12f, 0.12f, 0.18f, 0.95f));
            tabBar.anchorMin = new Vector2(0f, 0.90f);
            tabBar.anchorMax = new Vector2(1f, 0.95f);
            tabBar.offsetMin = Vector2.zero;
            tabBar.offsetMax = Vector2.zero;

            // TabManager
            var tabManagerGO = new GameObject("TabManager");
            tabManagerGO.transform.SetParent(root, false);
            _tabManager = tabManagerGO.AddComponent<TabManager>();

            // 4 boutons d'onglets
            var btnCreatures = CreateTabButton(tabBar, "TabCreatures", "Créatures", new Vector2(0.01f, 0.1f), new Vector2(0.24f, 0.9f));
            var btnBreeding = CreateTabButton(tabBar, "TabBreeding", "Breeding", new Vector2(0.26f, 0.1f), new Vector2(0.49f, 0.9f));
            var btnInventory = CreateTabButton(tabBar, "TabInventory", "Inventaire", new Vector2(0.51f, 0.1f), new Vector2(0.74f, 0.9f));
            var btnStats = CreateTabButton(tabBar, "TabStats", "Stats", new Vector2(0.76f, 0.1f), new Vector2(0.99f, 0.9f));

            // 4 panneaux de contenu
            var contentCreatures = CreateContentPanel(root, "ContentCreatures");
            var contentBreeding = CreateContentPanel(root, "ContentBreeding");
            var contentInventory = CreateContentPanel(root, "ContentInventory");
            var contentStats = CreateContentPanel(root, "ContentStats");

            // Enregistrer les onglets
            _tabManager.AddTab(new TabDefinition { Name = "Créatures", Content = contentCreatures.gameObject, TabButton = btnCreatures.GetComponent<Button>() });
            _tabManager.AddTab(new TabDefinition { Name = "Breeding", Content = contentBreeding.gameObject, TabButton = btnBreeding.GetComponent<Button>() });
            _tabManager.AddTab(new TabDefinition { Name = "Inventaire", Content = contentInventory.gameObject, TabButton = btnInventory.GetComponent<Button>() });
            _tabManager.AddTab(new TabDefinition { Name = "Stats", Content = contentStats.gameObject, TabButton = btnStats.GetComponent<Button>() });

            // Construire le contenu de chaque onglet
            BuildCreaturesTab(contentCreatures);
            BuildBreedingTab(contentBreeding);
            BuildInventoryTab(contentInventory);
            BuildStatsTab(contentStats);

            // Initialiser : affiche le premier onglet
            _tabManager.InitializeAfterDynamicSetup();

            // Settings overlay
            BuildSettingsSpawner();

            // Debug menu (editor / dev build only)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            BuildDebugMenu();
#endif
        }

        /// <summary>Instancie le SettingsSpawner pour l'accès depuis MainUI. GDD §17.</summary>
        private void BuildSettingsSpawner()
        {
            var settingsGO = new GameObject("SettingsSpawner");
            settingsGO.transform.SetParent(transform, false);
            _settingsSpawner = settingsGO.AddComponent<SettingsSpawner>();
            _settingsSpawner.Build();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>Instancie le DebugMenuUI. GDD §18.4.</summary>
        private void BuildDebugMenu()
        {
            var debugGO = new GameObject("DebugMenuUI");
            debugGO.transform.SetParent(transform, false);
            _debugMenuUI = debugGO.AddComponent<DebugMenuUI>();
            _debugMenuUI.Initialize(_tabManager, _settingsSpawner);
        }
#endif

        // ==========================
        //  Header
        // ==========================

        /// <summary>Construit le header persistant affichant les ressources principales.</summary>
        private void BuildHeader(Transform root)
        {
            var header = CreatePanel(root, "Header", new Color(0.1f, 0.1f, 0.15f, 0.95f));
            header.anchorMin = new Vector2(0f, 0.95f);
            header.anchorMax = new Vector2(1f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            _headerFragments = CreateTextDirect(header, "HeaderFragments", "Fragments: 0", 16,
                new Vector2(0.02f, 0.1f), new Vector2(0.30f, 0.9f), TextAlignmentOptions.Left,
                new Color(0.9f, 0.7f, 0.3f));

            _headerEssence = CreateTextDirect(header, "HeaderEssence", "Essence: 0", 16,
                new Vector2(0.35f, 0.1f), new Vector2(0.63f, 0.9f), TextAlignmentOptions.Center,
                new Color(0.6f, 0.4f, 1f));

            _headerGold = CreateTextDirect(header, "HeaderGold", "Gold: 0", 16,
                new Vector2(0.58f, 0.1f), new Vector2(0.78f, 0.9f), TextAlignmentOptions.Right,
                new Color(1f, 0.85f, 0f));

            // Bouton Paramètres (⚙)
            var btnSettings = CreateButton(header, "BtnSettings", "⚙",
                new Vector2(0.82f, 0.1f), new Vector2(0.90f, 0.9f));
            btnSettings.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.35f, 1f);
            btnSettings.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (_settingsSpawner != null) _settingsSpawner.Toggle();
            });

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Bouton Debug (🔧)
            var btnDebug = CreateButton(header, "BtnDebug", "F12",
                new Vector2(0.92f, 0.1f), new Vector2(0.99f, 0.9f));
            btnDebug.GetComponent<Image>().color = new Color(0.35f, 0.25f, 0.45f, 1f);
            btnDebug.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (_debugMenuUI != null) _debugMenuUI.Toggle();
            });
#endif
        }

        /// <summary>Met à jour le header avec les valeurs actuelles des ressources.</summary>
        private void RefreshHeader()
        {
            var resources = GameManager.Instance?.CurrentProfile?.Resources;
            if (resources == null) return;

            if (_headerFragments != null)
                _headerFragments.text = $"Fragments: {resources.Fragments:F0}";
            if (_headerEssence != null)
                _headerEssence.text = $"Essence: {resources.Essence:F1}";
            if (_headerGold != null)
                _headerGold.text = $"Gold: {resources.Gold:F0}";
        }

        // ==========================
        //  Onglet Créatures
        // ==========================

        /// <summary>Construit le contenu de l'onglet Créatures. GDD §15.2.</summary>
        private void BuildCreaturesTab(RectTransform parent)
        {
            // Liste scrollable (zone gauche)
            var scrollArea = CreateScrollView(parent, "CreatureScroll",
                new Vector2(0.02f, 0.02f), new Vector2(0.58f, 0.98f));

            // Panneau détail (zone droite)
            var detailPanel = CreatePanel(parent, "DetailPanel", new Color(0.12f, 0.12f, 0.18f, 0.9f));
            detailPanel.anchorMin = new Vector2(0.60f, 0.02f);
            detailPanel.anchorMax = new Vector2(0.98f, 0.98f);
            detailPanel.offsetMin = Vector2.zero;
            detailPanel.offsetMax = Vector2.zero;

            var detailTitle = CreateText(detailPanel, "DetailTitle", "Détails", 20, TextAnchor.UpperCenter);
            detailTitle.anchorMin = new Vector2(0.05f, 0.85f);
            detailTitle.anchorMax = new Vector2(0.95f, 0.98f);

            var detailName = CreateText(detailPanel, "DetailName", "---", 18, TextAnchor.UpperLeft);
            detailName.anchorMin = new Vector2(0.05f, 0.72f);
            detailName.anchorMax = new Vector2(0.95f, 0.84f);

            var detailStats = CreateText(detailPanel, "DetailStats", "---", 14, TextAnchor.UpperLeft);
            detailStats.anchorMin = new Vector2(0.05f, 0.50f);
            detailStats.anchorMax = new Vector2(0.95f, 0.71f);

            var detailComp = CreateText(detailPanel, "DetailComp", "---", 14, TextAnchor.UpperLeft);
            detailComp.anchorMin = new Vector2(0.05f, 0.32f);
            detailComp.anchorMax = new Vector2(0.95f, 0.49f);

            var detailStatus = CreateText(detailPanel, "DetailStatus", "---", 14, TextAnchor.UpperLeft);
            detailStatus.anchorMin = new Vector2(0.05f, 0.22f);
            detailStatus.anchorMax = new Vector2(0.95f, 0.31f);

            // Boutons d'action
            var btnActivate = CreateButton(detailPanel, "BtnActivate", "Activer/Désactiver",
                new Vector2(0.05f, 0.12f), new Vector2(0.48f, 0.20f));
            var btnDetails = CreateButton(detailPanel, "BtnDetails", "Détails",
                new Vector2(0.52f, 0.12f), new Vector2(0.95f, 0.20f));
            var btnRecycle = CreateButton(detailPanel, "BtnRecycle", "Recycler",
                new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.10f));

            // Couleur rouge pour recycler
            var recycleImg = btnRecycle.GetComponent<Image>();
            if (recycleImg != null)
                recycleImg.color = new Color(0.6f, 0.2f, 0.2f, 1f);

            // CreatureListUI
            var listUIGO = new GameObject("CreatureListUI");
            listUIGO.transform.SetParent(parent, false);
            var listUI = listUIGO.AddComponent<CreatureListUI>();
            listUI.Initialize(
                scrollArea.GetComponentInChildren<VerticalLayoutGroup>()?.transform ?? scrollArea,
                detailPanel.gameObject,
                detailName.GetComponent<TMP_Text>(),
                detailStats.GetComponent<TMP_Text>(),
                detailComp.GetComponent<TMP_Text>(),
                detailStatus.GetComponent<TMP_Text>(),
                btnActivate.GetComponent<Button>(),
                btnDetails.GetComponent<Button>(),
                btnRecycle.GetComponent<Button>(),
                _tabManager, 0);

            listUI.RefreshList();
        }

        // ==========================
        //  Onglet Breeding
        // ==========================

        /// <summary>
        /// Construit l'onglet Breeding en réutilisant le BreedingUI existant.
        /// GDD §15.5 — Intégration BreedingUI (§4.5) dans le système d'onglets.
        /// </summary>
        private void BuildBreedingTab(RectTransform parent)
        {
            // Panneau Parent A
            var panelA = CreatePanel(parent, "PanelParentA", new Color(0.1f, 0.1f, 0.2f, 0.8f));
            panelA.anchorMin = new Vector2(0.02f, 0.45f);
            panelA.anchorMax = new Vector2(0.32f, 0.98f);
            panelA.offsetMin = Vector2.zero;
            panelA.offsetMax = Vector2.zero;

            var nameA = CreateText(panelA, "NameA", "Parent A", 18, TextAnchor.UpperLeft);
            nameA.anchorMin = new Vector2(0.05f, 0.82f);
            nameA.anchorMax = new Vector2(0.95f, 0.96f);

            var chartA = CreateRadarChart(panelA, "ChartA", Color.blue, new Color(0.2f, 0.5f, 1f, 0.6f));
            chartA.anchorMin = new Vector2(0.05f, 0.35f);
            chartA.anchorMax = new Vector2(0.95f, 0.80f);
            chartA.offsetMin = Vector2.zero;
            chartA.offsetMax = Vector2.zero;

            var statsA = CreateText(panelA, "StatsA", "...", 13, TextAnchor.LowerLeft);
            statsA.anchorMin = new Vector2(0.05f, 0.02f);
            statsA.anchorMax = new Vector2(0.95f, 0.34f);

            // Panneau Parent B
            var panelB = CreatePanel(parent, "PanelParentB", new Color(0.2f, 0.1f, 0.1f, 0.8f));
            panelB.anchorMin = new Vector2(0.36f, 0.45f);
            panelB.anchorMax = new Vector2(0.66f, 0.98f);
            panelB.offsetMin = Vector2.zero;
            panelB.offsetMax = Vector2.zero;

            var nameB = CreateText(panelB, "NameB", "Parent B", 18, TextAnchor.UpperLeft);
            nameB.anchorMin = new Vector2(0.05f, 0.82f);
            nameB.anchorMax = new Vector2(0.95f, 0.96f);

            var chartB = CreateRadarChart(panelB, "ChartB", Color.red, new Color(1f, 0.3f, 0.3f, 0.6f));
            chartB.anchorMin = new Vector2(0.05f, 0.35f);
            chartB.anchorMax = new Vector2(0.95f, 0.80f);
            chartB.offsetMin = Vector2.zero;
            chartB.offsetMax = Vector2.zero;

            var statsB = CreateText(panelB, "StatsB", "...", 13, TextAnchor.LowerLeft);
            statsB.anchorMin = new Vector2(0.05f, 0.02f);
            statsB.anchorMax = new Vector2(0.95f, 0.34f);

            // Panneau Prédiction
            var panelPred = CreatePanel(parent, "PanelPrediction", new Color(0.1f, 0.2f, 0.1f, 0.8f));
            panelPred.anchorMin = new Vector2(0.70f, 0.45f);
            panelPred.anchorMax = new Vector2(0.98f, 0.98f);
            panelPred.offsetMin = Vector2.zero;
            panelPred.offsetMax = Vector2.zero;

            var namePred = CreateText(panelPred, "NamePred", "Prédiction", 18, TextAnchor.UpperLeft);
            namePred.anchorMin = new Vector2(0.05f, 0.82f);
            namePred.anchorMax = new Vector2(0.95f, 0.96f);

            var chartPred = CreateRadarChart(panelPred, "ChartPred", Color.green, new Color(0.2f, 0.8f, 0.2f, 0.3f));
            chartPred.anchorMin = new Vector2(0.05f, 0.35f);
            chartPred.anchorMax = new Vector2(0.95f, 0.80f);
            chartPred.offsetMin = Vector2.zero;
            chartPred.offsetMax = Vector2.zero;

            var statsPred = CreateText(panelPred, "StatsPred", "...", 13, TextAnchor.LowerLeft);
            statsPred.anchorMin = new Vector2(0.05f, 0.02f);
            statsPred.anchorMax = new Vector2(0.95f, 0.34f);

            // Contrôles
            var panelControls = CreatePanel(parent, "PanelControls", new Color(0.15f, 0.15f, 0.15f, 0.9f));
            panelControls.anchorMin = new Vector2(0.02f, 0.20f);
            panelControls.anchorMax = new Vector2(0.66f, 0.42f);
            panelControls.offsetMin = Vector2.zero;
            panelControls.offsetMax = Vector2.zero;

            var btnRandom = CreateButton(panelControls, "BtnRandom", "Randomize Parents", new Vector2(0.02f, 0.55f), new Vector2(0.32f, 0.95f));
            var btnBreed = CreateButton(panelControls, "BtnBreed", "Breed !", new Vector2(0.35f, 0.55f), new Vector2(0.65f, 0.95f));
            var btnQuick = CreateButton(panelControls, "BtnQuick", "Quick x10", new Vector2(0.68f, 0.55f), new Vector2(0.98f, 0.95f));

            var varianceLabel = CreateText(panelControls, "VarianceLabel", "Variance:", 14, TextAnchor.MiddleLeft);
            varianceLabel.anchorMin = new Vector2(0.02f, 0.25f);
            varianceLabel.anchorMax = new Vector2(0.20f, 0.52f);

            var varianceInput = CreateInputField(panelControls, "VarianceInput", "0.25", new Vector2(0.22f, 0.25f), new Vector2(0.40f, 0.52f));

            var mutLabel = CreateText(panelControls, "MutLabel", "Mutation %:", 14, TextAnchor.MiddleLeft);
            mutLabel.anchorMin = new Vector2(0.45f, 0.25f);
            mutLabel.anchorMax = new Vector2(0.63f, 0.52f);

            var mutInput = CreateInputField(panelControls, "MutInput", "0.05", new Vector2(0.65f, 0.25f), new Vector2(0.83f, 0.52f));

            var logText = CreateText(panelControls, "LogText", "Ready.", 14, TextAnchor.MiddleCenter);
            logText.anchorMin = new Vector2(0.02f, 0.02f);
            logText.anchorMax = new Vector2(0.98f, 0.22f);

            // Panneau Enfant
            var panelChild = CreatePanel(parent, "PanelChild", new Color(0.2f, 0.15f, 0.25f, 0.9f));
            panelChild.anchorMin = new Vector2(0.70f, 0.02f);
            panelChild.anchorMax = new Vector2(0.98f, 0.42f);
            panelChild.offsetMin = Vector2.zero;
            panelChild.offsetMax = Vector2.zero;

            var nameChild = CreateText(panelChild, "NameChild", "Enfant (?)", 18, TextAnchor.UpperLeft);
            nameChild.anchorMin = new Vector2(0.05f, 0.82f);
            nameChild.anchorMax = new Vector2(0.95f, 0.96f);

            var chartChild = CreateRadarChart(panelChild, "ChartChild", Color.cyan, new Color(0.3f, 0.8f, 1f, 0.6f));
            chartChild.anchorMin = new Vector2(0.05f, 0.38f);
            chartChild.anchorMax = new Vector2(0.95f, 0.80f);
            chartChild.offsetMin = Vector2.zero;
            chartChild.offsetMax = Vector2.zero;

            var statsChild = CreateText(panelChild, "StatsChild", "---", 13, TextAnchor.LowerLeft);
            statsChild.anchorMin = new Vector2(0.05f, 0.22f);
            statsChild.anchorMax = new Vector2(0.95f, 0.37f);

            var compChild = CreateText(panelChild, "CompChild", "---", 13, TextAnchor.LowerLeft);
            compChild.anchorMin = new Vector2(0.05f, 0.08f);
            compChild.anchorMax = new Vector2(0.95f, 0.22f);

            var mutChild = CreateText(panelChild, "MutChild", "---", 13, TextAnchor.LowerLeft);
            mutChild.anchorMin = new Vector2(0.05f, 0.01f);
            mutChild.anchorMax = new Vector2(0.95f, 0.08f);

            // BreedingUI (existant, non modifié)
            var uiGO = new GameObject("BreedingUI");
            uiGO.transform.SetParent(parent, false);
            var breedingUI = uiGO.AddComponent<BreedingUI>();
            breedingUI.Initialize(
                nameA.GetComponent<TMP_Text>(), chartA.GetComponent<HexRadarChart>(), statsA.GetComponent<TMP_Text>(),
                nameB.GetComponent<TMP_Text>(), chartB.GetComponent<HexRadarChart>(), statsB.GetComponent<TMP_Text>(),
                chartPred.GetComponent<HexRadarChart>(), statsPred.GetComponent<TMP_Text>(),
                chartChild.GetComponent<HexRadarChart>(), nameChild.GetComponent<TMP_Text>(),
                statsChild.GetComponent<TMP_Text>(), compChild.GetComponent<TMP_Text>(), mutChild.GetComponent<TMP_Text>(),
                btnBreed.GetComponent<Button>(), btnRandom.GetComponent<Button>(), btnQuick.GetComponent<Button>(),
                varianceInput.GetComponent<TMP_InputField>(), mutInput.GetComponent<TMP_InputField>(),
                logText.GetComponent<TMP_Text>());
        }

        // ==========================
        //  Onglet Inventaire
        // ==========================

        /// <summary>Construit le contenu de l'onglet Inventaire. GDD §15.3.</summary>
        private void BuildInventoryTab(RectTransform parent)
        {
            // Section Formes
            var shapesTitle = CreateText(parent, "ShapesTitle", "Formes", 20, TextAnchor.UpperLeft);
            shapesTitle.anchorMin = new Vector2(0.02f, 0.85f);
            shapesTitle.anchorMax = new Vector2(0.48f, 0.98f);

            var shapesPanel = CreatePanel(parent, "ShapesPanel", new Color(0.1f, 0.1f, 0.15f, 0.5f));
            shapesPanel.anchorMin = new Vector2(0.02f, 0.55f);
            shapesPanel.anchorMax = new Vector2(0.48f, 0.84f);
            shapesPanel.offsetMin = Vector2.zero;
            shapesPanel.offsetMax = Vector2.zero;

            var shapesGrid = AddGridLayout(shapesPanel.gameObject, new Vector2(120, 50), new Vector2(5, 5));

            // Section Couleurs
            var colorsTitle = CreateText(parent, "ColorsTitle", "Couleurs", 20, TextAnchor.UpperLeft);
            colorsTitle.anchorMin = new Vector2(0.52f, 0.85f);
            colorsTitle.anchorMax = new Vector2(0.98f, 0.98f);

            var colorsPanel = CreatePanel(parent, "ColorsPanel", new Color(0.1f, 0.1f, 0.15f, 0.5f));
            colorsPanel.anchorMin = new Vector2(0.52f, 0.55f);
            colorsPanel.anchorMax = new Vector2(0.98f, 0.84f);
            colorsPanel.offsetMin = Vector2.zero;
            colorsPanel.offsetMax = Vector2.zero;

            var colorsGrid = AddGridLayout(colorsPanel.gameObject, new Vector2(120, 50), new Vector2(5, 5));

            // Section Squelettes débloqués
            var skeletonsTitle = CreateText(parent, "SkeletonsTitle", "Squelettes débloqués", 20, TextAnchor.UpperLeft);
            skeletonsTitle.anchorMin = new Vector2(0.02f, 0.42f);
            skeletonsTitle.anchorMax = new Vector2(0.98f, 0.52f);

            var skeletonsPanel = CreatePanel(parent, "SkeletonsPanel", new Color(0.1f, 0.1f, 0.15f, 0.5f));
            skeletonsPanel.anchorMin = new Vector2(0.02f, 0.02f);
            skeletonsPanel.anchorMax = new Vector2(0.98f, 0.41f);
            skeletonsPanel.offsetMin = Vector2.zero;
            skeletonsPanel.offsetMax = Vector2.zero;

            var skeletonsGrid = AddGridLayout(skeletonsPanel.gameObject, new Vector2(120, 40), new Vector2(5, 5));

            // InventoryUI
            var invUIGO = new GameObject("InventoryUI");
            invUIGO.transform.SetParent(parent, false);
            var invUI = invUIGO.AddComponent<InventoryUI>();
            invUI.Initialize(
                shapesGrid.transform, colorsGrid.transform, skeletonsGrid.transform,
                shapesTitle.GetComponent<TMP_Text>(), colorsTitle.GetComponent<TMP_Text>(), skeletonsTitle.GetComponent<TMP_Text>(),
                _tabManager, 2);

            invUI.RefreshInventory();
        }

        // ==========================
        //  Onglet Stats
        // ==========================

        /// <summary>Construit le contenu de l'onglet Stats. GDD §15.4.</summary>
        private void BuildStatsTab(RectTransform parent)
        {
            // Titre
            var title = CreateText(parent, "StatsTitle", "Statistiques du Joueur", 24, TextAnchor.UpperCenter);
            title.anchorMin = new Vector2(0.05f, 0.88f);
            title.anchorMax = new Vector2(0.95f, 0.98f);

            // Section Ressources
            var resLabel = CreateText(parent, "ResLabel", "<b>Ressources</b>", 18, TextAnchor.UpperLeft);
            resLabel.anchorMin = new Vector2(0.05f, 0.78f);
            resLabel.anchorMax = new Vector2(0.45f, 0.87f);

            var fragments = CreateText(parent, "Fragments", "Fragments: 0", 16, TextAnchor.UpperLeft);
            fragments.anchorMin = new Vector2(0.05f, 0.70f);
            fragments.anchorMax = new Vector2(0.45f, 0.78f);

            var gold = CreateText(parent, "Gold", "Gold: 0", 16, TextAnchor.UpperLeft);
            gold.anchorMin = new Vector2(0.05f, 0.62f);
            gold.anchorMax = new Vector2(0.45f, 0.70f);

            var materials = CreateText(parent, "Materials", "Materials: 0", 16, TextAnchor.UpperLeft);
            materials.anchorMin = new Vector2(0.05f, 0.54f);
            materials.anchorMax = new Vector2(0.45f, 0.62f);

            var essence = CreateText(parent, "Essence", "Essence: 0", 16, TextAnchor.UpperLeft);
            essence.anchorMin = new Vector2(0.55f, 0.70f);
            essence.anchorMax = new Vector2(0.95f, 0.78f);

            var taboo = CreateText(parent, "Taboo", "Taboo: 0", 16, TextAnchor.UpperLeft);
            taboo.anchorMin = new Vector2(0.55f, 0.62f);
            taboo.anchorMax = new Vector2(0.95f, 0.70f);

            var wishes = CreateText(parent, "Wishes", "Wishes: 0", 16, TextAnchor.UpperLeft);
            wishes.anchorMin = new Vector2(0.55f, 0.54f);
            wishes.anchorMax = new Vector2(0.95f, 0.62f);

            // Section Profil
            var profileLabel = CreateText(parent, "ProfileLabel", "<b>Profil</b>", 18, TextAnchor.UpperLeft);
            profileLabel.anchorMin = new Vector2(0.05f, 0.42f);
            profileLabel.anchorMax = new Vector2(0.45f, 0.51f);

            var playerName = CreateText(parent, "PlayerName", "Joueur: ---", 16, TextAnchor.UpperLeft);
            playerName.anchorMin = new Vector2(0.05f, 0.34f);
            playerName.anchorMax = new Vector2(0.45f, 0.42f);

            var runCount = CreateText(parent, "RunCount", "Run: 1", 16, TextAnchor.UpperLeft);
            runCount.anchorMin = new Vector2(0.05f, 0.26f);
            runCount.anchorMax = new Vector2(0.45f, 0.34f);

            var playTime = CreateText(parent, "PlayTime", "Temps de jeu: 00h 00m 00s", 16, TextAnchor.UpperLeft);
            playTime.anchorMin = new Vector2(0.55f, 0.34f);
            playTime.anchorMax = new Vector2(0.95f, 0.42f);

            var creatureCount = CreateText(parent, "CreatureCount", "Créatures: 0", 16, TextAnchor.UpperLeft);
            creatureCount.anchorMin = new Vector2(0.55f, 0.26f);
            creatureCount.anchorMax = new Vector2(0.95f, 0.34f);

            // PlayerStatsUI
            var statsUIGO = new GameObject("PlayerStatsUI");
            statsUIGO.transform.SetParent(parent, false);
            var statsUI = statsUIGO.AddComponent<PlayerStatsUI>();
            statsUI.Initialize(
                fragments.GetComponent<TMP_Text>(),
                gold.GetComponent<TMP_Text>(),
                materials.GetComponent<TMP_Text>(),
                essence.GetComponent<TMP_Text>(),
                taboo.GetComponent<TMP_Text>(),
                wishes.GetComponent<TMP_Text>(),
                playerName.GetComponent<TMP_Text>(),
                runCount.GetComponent<TMP_Text>(),
                playTime.GetComponent<TMP_Text>(),
                creatureCount.GetComponent<TMP_Text>(),
                _tabManager, 3);
        }

        // ========================================================
        //  UI Factory Methods
        // ========================================================

        private RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        private RectTransform CreateContentPanel(Transform parent, string name)
        {
            var rt = CreatePanel(parent, name, new Color(0f, 0f, 0f, 0f));
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0.90f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        private RectTransform CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.alignment = ToTMP(alignment);
            tmp.color = Color.white;
            tmp.richText = true;
            return go.GetComponent<RectTransform>();
        }

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

        private TextAlignmentOptions ToTMP(TextAnchor a)
        {
            switch (a)
            {
                case TextAnchor.UpperLeft:    return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter:  return TextAlignmentOptions.Top;
                case TextAnchor.LowerLeft:    return TextAlignmentOptions.BottomLeft;
                case TextAnchor.MiddleLeft:   return TextAlignmentOptions.Left;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                default: return TextAlignmentOptions.Center;
            }
        }

        private RectTransform CreateRadarChart(Transform parent, string name, Color outline, Color fill)
        {
            var go = new GameObject(name, typeof(HexRadarChart));
            go.transform.SetParent(parent, false);
            var chart = go.GetComponent<HexRadarChart>();
            chart.SetColors(fill, outline);
            chart.maxStatValue = 20f;
            chart.radius = 80f;
            return go.GetComponent<RectTransform>();
        }

        private RectTransform CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.3f, 0.5f, 0.7f, 1f);

            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.5f, 0.7f, 0.9f);
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

        private RectTransform CreateTabButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rt = CreateButton(parent, name, label, anchorMin, anchorMax);
            var img = rt.GetComponent<Image>();
            if (img != null)
                img.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            var tmp = rt.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
                tmp.fontSize = 18;

            return rt;
        }

        private RectTransform CreateInputField(Transform parent, string name, string defaultText, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(Image), typeof(TMP_InputField));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            var textGO = new GameObject("Text", typeof(TextMeshProUGUI));
            textGO.transform.SetParent(go.transform, false);
            var tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var input = go.GetComponent<TMP_InputField>();
            input.textComponent = tmp;
            input.text = defaultText;
            input.contentType = TMP_InputField.ContentType.DecimalNumber;

            var textRt = textGO.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(4, 2);
            textRt.offsetMax = new Vector2(-4, -2);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        private RectTransform CreateScrollView(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            // ScrollRect container
            var go = new GameObject(name, typeof(Image), typeof(ScrollRect));
            go.transform.SetParent(parent, false);

            var bgImg = go.GetComponent<Image>();
            bgImg.color = new Color(0.08f, 0.08f, 0.12f, 0.5f);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Viewport
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(go.transform, false);
            var vpImg = viewport.GetComponent<Image>();
            vpImg.color = new Color(1, 1, 1, 0.01f);
            var vpMask = viewport.GetComponent<Mask>();
            vpMask.showMaskGraphic = false;
            var vpRt = viewport.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;

            // Content
            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.padding = new RectOffset(5, 5, 5, 5);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Wire ScrollRect
            var scroll = go.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = vpRt;
            scroll.horizontal = false;
            scroll.vertical = true;

            return rt;
        }

        private GridLayoutGroup AddGridLayout(GameObject target, Vector2 cellSize, Vector2 spacing)
        {
            var grid = target.AddComponent<GridLayoutGroup>();
            grid.cellSize = cellSize;
            grid.spacing = spacing;
            grid.padding = new RectOffset(10, 10, 10, 10);
            grid.constraint = GridLayoutGroup.Constraint.Flexible;
            return grid;
        }

        private void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private string PickRandom(string[] arr) => arr[Random.Range(0, arr.Length)];
    }
}
