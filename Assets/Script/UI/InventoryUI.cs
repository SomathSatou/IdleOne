using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Script.Entities;
using Assets.Script.Managers;
using Assets.Script.UI.Tabs;

namespace Assets.Script.UI
{
    /// <summary>
    /// Onglet "Inventaire" — affiche les stocks de formes et couleurs consommables,
    /// ainsi que la liste des squelettes débloqués.
    /// GDD §15.3 — Inventaire (formes, couleurs, squelettes débloqués).
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        // ==========================
        //  Références UI
        // ==========================

        [Header("Sections")]
        [SerializeField] private Transform shapesContent;
        [SerializeField] private Transform colorsContent;
        [SerializeField] private Transform skeletonsContent;

        [Header("Titres")]
        [SerializeField] private TMP_Text shapesTitle;
        [SerializeField] private TMP_Text colorsTitle;
        [SerializeField] private TMP_Text skeletonsTitle;

        [Header("Tab Manager")]
        [SerializeField] private TabManager tabManager;
        [SerializeField] private int myTabIndex;

        // ==========================
        //  État
        // ==========================

        private readonly List<GameObject> _spawnedItems = new List<GameObject>();

        // ==========================
        //  Cycle de vie Unity
        // ==========================

        private void Start()
        {
            if (tabManager != null)
                tabManager.OnTabChanged += OnTabChanged;

            RefreshInventory();
        }

        private void OnDestroy()
        {
            if (tabManager != null)
                tabManager.OnTabChanged -= OnTabChanged;
        }

        // ==========================
        //  Méthodes publiques
        // ==========================

        /// <summary>
        /// Injection des références UI (utilisé par les spawners programmatiques).
        /// </summary>
        public void Initialize(Transform shapes, Transform colors, Transform skeletons,
            TMP_Text sTitle, TMP_Text cTitle, TMP_Text skTitle,
            TabManager manager, int tabIndex)
        {
            shapesContent = shapes;
            colorsContent = colors;
            skeletonsContent = skeletons;
            shapesTitle = sTitle;
            colorsTitle = cTitle;
            skeletonsTitle = skTitle;
            tabManager = manager;
            myTabIndex = tabIndex;

            if (tabManager != null)
                tabManager.OnTabChanged += OnTabChanged;
        }

        /// <summary>
        /// Rafraîchit l'affichage complet de l'inventaire à partir des données du profil joueur.
        /// GDD §7.1 — Formes et Couleurs comme ressources consommables.
        /// GDD §3.2 — Squelettes comme types de base (non consommables).
        /// </summary>
        public void RefreshInventory()
        {
            ClearItems();

            var profile = GameManager.Instance?.CurrentProfile;
            if (profile == null) return;

            var inventory = profile.Inventory;

            // Formes consommables
            if (shapesTitle != null)
                shapesTitle.text = $"Formes ({inventory.ShapeStock.Count} types)";

            foreach (var kvp in inventory.ShapeStock)
                CreateInventoryItem(shapesContent, kvp.Key, kvp.Value, GetShapeColor(kvp.Key));

            // Couleurs consommables
            if (colorsTitle != null)
                colorsTitle.text = $"Couleurs ({inventory.ColorStock.Count} types)";

            foreach (var kvp in inventory.ColorStock)
                CreateInventoryItem(colorsContent, kvp.Key, kvp.Value, GetColorTint(kvp.Key));

            // Squelettes débloqués (non consommables)
            if (skeletonsTitle != null)
                skeletonsTitle.text = $"Squelettes débloqués ({inventory.UnlockedSkeletons.Count})";

            foreach (var skeleton in inventory.UnlockedSkeletons)
                CreateSkeletonItem(skeletonsContent, skeleton);
        }

        // ==========================
        //  Méthodes internes
        // ==========================

        /// <summary>Callback du TabManager : rafraîchit l'inventaire au retour sur cet onglet.</summary>
        private void OnTabChanged(int index)
        {
            if (index == myTabIndex)
                RefreshInventory();
        }

        /// <summary>Crée un item d'inventaire consommable (forme ou couleur) avec nom et quantité.</summary>
        private void CreateInventoryItem(Transform parent, string name, int quantity, Color tint)
        {
            if (parent == null) return;

            var go = new GameObject($"Item_{name}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 50);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(tint.r * 0.3f, tint.g * 0.3f, tint.b * 0.3f, 0.7f);

            // Icône couleur
            var iconGO = new GameObject("Icon", typeof(Image));
            iconGO.transform.SetParent(go.transform, false);
            var iconImg = iconGO.GetComponent<Image>();
            iconImg.color = tint;
            var iconRt = iconGO.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.05f, 0.15f);
            iconRt.anchorMax = new Vector2(0.25f, 0.85f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;

            // Nom
            var nameGO = new GameObject("Name", typeof(TextMeshProUGUI));
            nameGO.transform.SetParent(go.transform, false);
            var nameTmp = nameGO.GetComponent<TextMeshProUGUI>();
            nameTmp.text = name;
            nameTmp.fontSize = 12;
            nameTmp.color = Color.white;
            var nameRt = nameGO.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0.30f, 0.50f);
            nameRt.anchorMax = new Vector2(0.95f, 0.95f);
            nameRt.offsetMin = Vector2.zero;
            nameRt.offsetMax = Vector2.zero;

            // Quantité
            var qtyGO = new GameObject("Quantity", typeof(TextMeshProUGUI));
            qtyGO.transform.SetParent(go.transform, false);
            var qtyTmp = qtyGO.GetComponent<TextMeshProUGUI>();
            qtyTmp.text = $"x{quantity}";
            qtyTmp.fontSize = 14;
            qtyTmp.fontStyle = FontStyles.Bold;
            qtyTmp.color = new Color(1f, 0.9f, 0.5f);
            var qtyRt = qtyGO.GetComponent<RectTransform>();
            qtyRt.anchorMin = new Vector2(0.30f, 0.05f);
            qtyRt.anchorMax = new Vector2(0.95f, 0.50f);
            qtyRt.offsetMin = Vector2.zero;
            qtyRt.offsetMax = Vector2.zero;

            _spawnedItems.Add(go);
        }

        /// <summary>Crée un item de squelette débloqué (non consommable).</summary>
        private void CreateSkeletonItem(Transform parent, string skeletonName)
        {
            if (parent == null) return;

            var go = new GameObject($"Skeleton_{skeletonName}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 40);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.3f, 0.7f);

            var nameGO = new GameObject("Name", typeof(TextMeshProUGUI));
            nameGO.transform.SetParent(go.transform, false);
            var nameTmp = nameGO.GetComponent<TextMeshProUGUI>();
            nameTmp.text = skeletonName;
            nameTmp.fontSize = 14;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = Color.white;
            var nameRt = nameGO.GetComponent<RectTransform>();
            nameRt.anchorMin = Vector2.zero;
            nameRt.anchorMax = Vector2.one;
            nameRt.offsetMin = new Vector2(5, 2);
            nameRt.offsetMax = new Vector2(-5, -2);

            _spawnedItems.Add(go);
        }

        /// <summary>Détruit tous les items instanciés.</summary>
        private void ClearItems()
        {
            foreach (var go in _spawnedItems)
            {
                if (go != null)
                    Destroy(go);
            }
            _spawnedItems.Clear();
        }

        /// <summary>Retourne une couleur représentative pour une forme donnée. GDD §3.3.</summary>
        private Color GetShapeColor(string shape)
        {
            switch (shape)
            {
                case "Rond":     return new Color(0.4f, 0.8f, 0.4f);
                case "Triangle": return new Color(0.9f, 0.4f, 0.3f);
                case "Carre":    return new Color(0.5f, 0.5f, 0.9f);
                case "Ovale":    return new Color(0.3f, 0.9f, 0.9f);
                case "Losange":  return new Color(0.8f, 0.5f, 0.9f);
                case "Etoile":   return new Color(1f, 0.9f, 0.3f);
                default:         return Color.gray;
            }
        }

        /// <summary>Retourne une couleur représentative pour une couleur donnée. GDD §3.4.</summary>
        private Color GetColorTint(string colorName)
        {
            switch (colorName)
            {
                case "Rouge":       return Color.red;
                case "Bleu":        return Color.blue;
                case "Vert":        return Color.green;
                case "Jaune":       return Color.yellow;
                case "Violet":      return new Color(0.6f, 0.2f, 0.8f);
                case "Orange":      return new Color(1f, 0.6f, 0.2f);
                case "Dore":        return new Color(1f, 0.85f, 0f);
                case "Arc-en-ciel": return Color.white;
                default:            return Color.gray;
            }
        }
    }
}
