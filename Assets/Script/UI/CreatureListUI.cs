using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Script.Creatures;
using Assets.Script.Managers;
using Assets.Script.UI.Tabs;

namespace Assets.Script.UI
{
    /// <summary>
    /// Onglet "Créatures" — affiche la liste scrollable de toutes les créatures du joueur.
    /// Permet la sélection, l'activation et l'affichage des détails.
    /// GDD §15.2 — Liste de Créatures (affichage, tri, filtres futurs).
    /// </summary>
    public class CreatureListUI : MonoBehaviour
    {
        // ==========================
        //  Références UI
        // ==========================

        [Header("Liste")]
        [SerializeField] private Transform listContent;

        [Header("Détails")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private TMP_Text detailName;
        [SerializeField] private TMP_Text detailStats;
        [SerializeField] private TMP_Text detailComponents;
        [SerializeField] private TMP_Text detailStatus;

        [Header("Boutons d'action")]
        [SerializeField] private Button activateButton;
        [SerializeField] private Button detailsButton;
        [SerializeField] private Button recycleButton;

        [Header("Tab Manager")]
        [SerializeField] private TabManager tabManager;
        [SerializeField] private int myTabIndex;

        // ==========================
        //  État
        // ==========================

        private Creature _selectedCreature;
        private readonly List<CreatureListItemUI> _items = new List<CreatureListItemUI>();

        // ==========================
        //  Cycle de vie Unity
        // ==========================

        private void Start()
        {
            if (activateButton != null)
                activateButton.onClick.AddListener(OnActivateClicked);
            if (detailsButton != null)
                detailsButton.onClick.AddListener(OnDetailsClicked);
            if (recycleButton != null)
                recycleButton.onClick.AddListener(OnRecycleClicked);

            if (tabManager != null)
                tabManager.OnTabChanged += OnTabChanged;

            if (detailPanel != null)
                detailPanel.SetActive(false);

            RefreshList();
        }

        private void OnDestroy()
        {
            if (activateButton != null)
                activateButton.onClick.RemoveListener(OnActivateClicked);
            if (detailsButton != null)
                detailsButton.onClick.RemoveListener(OnDetailsClicked);
            if (recycleButton != null)
                recycleButton.onClick.RemoveListener(OnRecycleClicked);

            if (tabManager != null)
                tabManager.OnTabChanged -= OnTabChanged;
        }

        // ==========================
        //  Méthodes publiques
        // ==========================

        /// <summary>
        /// Injection des références UI (utilisé par les spawners programmatiques).
        /// </summary>
        public void Initialize(Transform content, GameObject detail,
            TMP_Text dName, TMP_Text dStats, TMP_Text dComp, TMP_Text dStatus,
            Button btnActivate, Button btnDetails, Button btnRecycle,
            TabManager manager, int tabIndex)
        {
            listContent = content;
            detailPanel = detail;
            detailName = dName;
            detailStats = dStats;
            detailComponents = dComp;
            detailStatus = dStatus;
            activateButton = btnActivate;
            detailsButton = btnDetails;
            recycleButton = btnRecycle;
            tabManager = manager;
            myTabIndex = tabIndex;

            if (activateButton != null)
                activateButton.onClick.AddListener(OnActivateClicked);
            if (detailsButton != null)
                detailsButton.onClick.AddListener(OnDetailsClicked);
            if (recycleButton != null)
                recycleButton.onClick.AddListener(OnRecycleClicked);

            if (tabManager != null)
                tabManager.OnTabChanged += OnTabChanged;

            if (detailPanel != null)
                detailPanel.SetActive(false);
        }

        /// <summary>
        /// Rafraîchit la liste complète des créatures à partir de l'inventaire du joueur.
        /// Vide la liste existante et recrée chaque item.
        /// GDD §14.3 — Inventaire & Slots (OwnedCreatures).
        /// </summary>
        public void RefreshList()
        {
            ClearList();

            var profile = GameManager.Instance?.CurrentProfile;
            if (profile == null) return;

            var creatures = profile.Inventory.OwnedCreatures;
            for (int i = 0; i < creatures.Count; i++)
            {
                var item = CreateListItem(creatures[i]);
                _items.Add(item);
            }

            if (_items.Count > 0 && _selectedCreature == null)
                SelectCreature(_items[0].GetCreature());
        }

        // ==========================
        //  Méthodes internes
        // ==========================

        /// <summary>Callback du TabManager : rafraîchit la liste quand on revient sur cet onglet.</summary>
        private void OnTabChanged(int index)
        {
            if (index == myTabIndex)
                RefreshList();
        }

        /// <summary>Sélectionne une créature et met à jour le panneau détail.</summary>
        private void SelectCreature(Creature creature)
        {
            _selectedCreature = creature;

            // Mise à jour du highlight sur tous les items
            foreach (var item in _items)
                item.SetHighlight(item.GetCreature() == creature);

            ShowDetails(creature);
        }

        /// <summary>Affiche les détails d'une créature dans le panneau latéral.</summary>
        private void ShowDetails(Creature creature)
        {
            if (creature == null)
            {
                if (detailPanel != null)
                    detailPanel.SetActive(false);
                return;
            }

            if (detailPanel != null)
                detailPanel.SetActive(true);

            if (detailName != null)
                detailName.text = $"{creature.Name} (Gen-{creature.Generation})";

            if (detailStats != null)
                detailStats.text = $"FOR {creature.Strength:F1} | AGI {creature.Agility:F1} | INT {creature.Intelligence:F1}\n" +
                                   $"CHA {creature.Luck:F1} | CON {creature.Constitution:F1} | VOL {creature.Willpower:F1}\n" +
                                   $"Total: {creature.TotalStats:F1}";

            if (detailComponents != null)
                detailComponents.text = $"Squelette: {creature.Skeleton}\nForme: {creature.Shape}\nCouleur: {creature.Color}";

            if (detailStatus != null)
            {
                bool isActive = GameManager.Instance?.CurrentProfile?.Inventory.ActiveCreatures.Contains(creature) ?? false;
                if (!creature.IsAlive)
                {
                    detailStatus.text = "Incapacité";
                    detailStatus.color = Color.red;
                }
                else if (isActive)
                {
                    detailStatus.text = "Actif (déployé)";
                    detailStatus.color = Color.cyan;
                }
                else
                {
                    detailStatus.text = "Inactif";
                    detailStatus.color = Color.yellow;
                }
            }
        }

        /// <summary>Crée un item de liste pour une créature (construction programmatique).</summary>
        private CreatureListItemUI CreateListItem(Creature creature)
        {
            var go = new GameObject($"Item_{creature.Name}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(listContent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 60);

            var bgImage = go.GetComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.6f);

            // Bouton de sélection (sur tout l'item)
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.25f, 0.4f, 0.6f, 0.8f);
            btn.colors = colors;
            btn.targetGraphic = bgImage;

            // Nom
            var nameGO = new GameObject("Name", typeof(TextMeshProUGUI));
            nameGO.transform.SetParent(go.transform, false);
            var nameTmp = nameGO.GetComponent<TextMeshProUGUI>();
            nameTmp.fontSize = 16;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.color = Color.white;
            var nameRt = nameGO.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0.02f, 0.55f);
            nameRt.anchorMax = new Vector2(0.30f, 0.95f);
            nameRt.offsetMin = Vector2.zero;
            nameRt.offsetMax = Vector2.zero;

            // Génération
            var genGO = new GameObject("Generation", typeof(TextMeshProUGUI));
            genGO.transform.SetParent(go.transform, false);
            var genTmp = genGO.GetComponent<TextMeshProUGUI>();
            genTmp.fontSize = 14;
            genTmp.color = new Color(0.7f, 0.8f, 1f);
            var genRt = genGO.GetComponent<RectTransform>();
            genRt.anchorMin = new Vector2(0.30f, 0.55f);
            genRt.anchorMax = new Vector2(0.42f, 0.95f);
            genRt.offsetMin = Vector2.zero;
            genRt.offsetMax = Vector2.zero;

            // Composants
            var compGO = new GameObject("Components", typeof(TextMeshProUGUI));
            compGO.transform.SetParent(go.transform, false);
            var compTmp = compGO.GetComponent<TextMeshProUGUI>();
            compTmp.fontSize = 12;
            compTmp.color = new Color(0.8f, 0.8f, 0.8f);
            var compRt = compGO.GetComponent<RectTransform>();
            compRt.anchorMin = new Vector2(0.02f, 0.05f);
            compRt.anchorMax = new Vector2(0.60f, 0.50f);
            compRt.offsetMin = Vector2.zero;
            compRt.offsetMax = Vector2.zero;

            // Total stats
            var statsGO = new GameObject("TotalStats", typeof(TextMeshProUGUI));
            statsGO.transform.SetParent(go.transform, false);
            var statsTmp = statsGO.GetComponent<TextMeshProUGUI>();
            statsTmp.fontSize = 14;
            statsTmp.alignment = TextAlignmentOptions.Right;
            statsTmp.color = new Color(1f, 0.9f, 0.5f);
            var statsRt = statsGO.GetComponent<RectTransform>();
            statsRt.anchorMin = new Vector2(0.60f, 0.55f);
            statsRt.anchorMax = new Vector2(0.85f, 0.95f);
            statsRt.offsetMin = Vector2.zero;
            statsRt.offsetMax = Vector2.zero;

            // Statut
            var statusGO = new GameObject("Status", typeof(TextMeshProUGUI));
            statusGO.transform.SetParent(go.transform, false);
            var statusTmp = statusGO.GetComponent<TextMeshProUGUI>();
            statusTmp.fontSize = 12;
            statusTmp.alignment = TextAlignmentOptions.Right;
            var statusRt = statusGO.GetComponent<RectTransform>();
            statusRt.anchorMin = new Vector2(0.60f, 0.05f);
            statusRt.anchorMax = new Vector2(0.98f, 0.50f);
            statusRt.offsetMin = Vector2.zero;
            statusRt.offsetMax = Vector2.zero;

            // Composant CreatureListItemUI
            var itemUI = go.AddComponent<CreatureListItemUI>();
            itemUI.Initialize(nameTmp, genTmp, compTmp, statsTmp, statusTmp, btn, bgImage);
            itemUI.Bind(creature);
            itemUI.OnSelected += SelectCreature;

            return itemUI;
        }

        /// <summary>Vide la liste de tous les items existants.</summary>
        private void ClearList()
        {
            foreach (var item in _items)
            {
                if (item != null)
                {
                    item.OnSelected -= SelectCreature;
                    Destroy(item.gameObject);
                }
            }
            _items.Clear();
            _selectedCreature = null;
        }

        // ==========================
        //  Callbacks boutons d'action
        // ==========================

        /// <summary>Active la créature sélectionnée (la déploie dans un slot actif).</summary>
        private void OnActivateClicked()
        {
            if (_selectedCreature == null) return;

            var inventory = GameManager.Instance?.CurrentProfile?.Inventory;
            if (inventory == null) return;

            if (inventory.ActiveCreatures.Contains(_selectedCreature))
            {
                inventory.SetInactive(_selectedCreature);
                Debug.Log($"[CreatureListUI] Créature '{_selectedCreature.Name}' désactivée.");
            }
            else
            {
                bool success = inventory.SetActive(_selectedCreature);
                Debug.Log(success
                    ? $"[CreatureListUI] Créature '{_selectedCreature.Name}' activée."
                    : $"[CreatureListUI] Impossible d'activer '{_selectedCreature.Name}' — slots pleins.");
            }

            ShowDetails(_selectedCreature);
        }

        /// <summary>Affiche/masque le panneau détails.</summary>
        private void OnDetailsClicked()
        {
            if (detailPanel != null)
                detailPanel.SetActive(!detailPanel.activeSelf);
        }

        /// <summary>Placeholder pour le recyclage de créature (non implémenté).</summary>
        private void OnRecycleClicked()
        {
            if (_selectedCreature == null) return;
            Debug.Log($"[CreatureListUI] Recycler '{_selectedCreature.Name}' — placeholder, non implémenté.");
        }
    }
}
