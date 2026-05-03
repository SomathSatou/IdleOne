using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Script.Managers;
using Assets.Script.Entities;

namespace Assets.Script.UI.MainMenu
{
    /// <summary>
    /// Panneau d'affichage des 3 slots de sauvegarde.
    /// Permet de charger ou supprimer une sauvegarde existante.
    /// GDD §18.3 — Slots de Sauvegarde.
    /// </summary>
    public class SaveSlotUI : MonoBehaviour
    {
        // ==========================
        //  Références UI
        // ==========================

        private GameObject _panel;
        private SlotRow[] _slots;
        private GameObject _confirmPanel;
        private int _pendingDeleteSlot = -1;

        /// <summary>Callback appelé quand un slot est chargé avec succès.</summary>
        public event Action OnSlotLoaded;

        /// <summary>Callback appelé quand l'utilisateur demande à revenir au menu.</summary>
        public event Action OnBackRequested;

        // ==========================
        //  Couleurs du thème
        // ==========================

        private static readonly Color PanelColor = new Color(0.10f, 0.10f, 0.16f, 0.95f);
        private static readonly Color SlotColor = new Color(0.12f, 0.12f, 0.20f, 0.9f);
        private static readonly Color SlotEmptyColor = new Color(0.08f, 0.08f, 0.12f, 0.7f);
        private static readonly Color ButtonColor = new Color(0.3f, 0.5f, 0.7f, 1f);
        private static readonly Color DeleteColor = new Color(0.6f, 0.2f, 0.2f, 1f);
        private static readonly Color ConfirmBgColor = new Color(0.06f, 0.06f, 0.10f, 0.97f);
        private static readonly Color TextColor = Color.white;
        private static readonly Color TextDimColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        // ==========================
        //  Structure interne
        // ==========================

        private class SlotRow
        {
            public RectTransform Root;
            public TMP_Text NameLabel;
            public TMP_Text InfoLabel;
            public TMP_Text DateLabel;
            public Button LoadButton;
            public Button DeleteButton;
        }

        // ==========================
        //  Construction
        // ==========================

        /// <summary>
        /// Construit le panneau des slots de sauvegarde sous le parent donné.
        /// Le panneau est masqué par défaut.
        /// GDD §18.3.
        /// </summary>
        public void Build(RectTransform parent)
        {
            // Panneau principal
            _panel = CreatePanel(parent, "SaveSlotsPanel", PanelColor,
                new Vector2(0.10f, 0.10f), new Vector2(0.90f, 0.90f));

            var panelRT = _panel.GetComponent<RectTransform>();

            // Titre
            CreateText(panelRT, "Title", "Charger une Partie", 28,
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f),
                TextAlignmentOptions.Center, TextColor);

            // 3 slots
            _slots = new SlotRow[SaveManager.MAX_SLOTS];
            for (int i = 0; i < SaveManager.MAX_SLOTS; i++)
            {
                float yMax = 0.85f - (i * 0.27f);
                float yMin = yMax - 0.24f;
                _slots[i] = BuildSlotRow(panelRT, i, yMin, yMax);
            }

            // Bouton Retour
            var backBtn = CreateButton(panelRT, "BackButton", "Retour",
                new Vector2(0.35f, 0.02f), new Vector2(0.65f, 0.09f), ButtonColor);
            backBtn.onClick.AddListener(() => OnBackRequested?.Invoke());

            // Panneau de confirmation de suppression
            BuildConfirmPanel(panelRT);

            _panel.SetActive(false);
        }

        /// <summary>Construit une ligne de slot de sauvegarde.</summary>
        private SlotRow BuildSlotRow(RectTransform parent, int slotIndex, float yMin, float yMax)
        {
            var row = new SlotRow();

            var slotPanel = CreatePanel(parent, $"Slot_{slotIndex}", SlotColor,
                new Vector2(0.05f, yMin), new Vector2(0.95f, yMax));
            row.Root = slotPanel.GetComponent<RectTransform>();

            // Nom du joueur
            row.NameLabel = CreateText(row.Root, "SlotName", $"Slot {slotIndex + 1}", 20,
                new Vector2(0.03f, 0.60f), new Vector2(0.60f, 0.95f),
                TextAlignmentOptions.Left, TextColor);

            // Info (run, temps de jeu)
            row.InfoLabel = CreateText(row.Root, "SlotInfo", "", 14,
                new Vector2(0.03f, 0.25f), new Vector2(0.60f, 0.58f),
                TextAlignmentOptions.Left, TextDimColor);

            // Date dernière save
            row.DateLabel = CreateText(row.Root, "SlotDate", "", 12,
                new Vector2(0.03f, 0.03f), new Vector2(0.60f, 0.25f),
                TextAlignmentOptions.Left, TextDimColor);

            // Bouton Charger
            row.LoadButton = CreateButton(row.Root, "LoadBtn", "Charger",
                new Vector2(0.62f, 0.55f), new Vector2(0.85f, 0.92f), ButtonColor);
            int capturedSlot = slotIndex;
            row.LoadButton.onClick.AddListener(() => OnLoadSlotClicked(capturedSlot));

            // Bouton Supprimer
            row.DeleteButton = CreateButton(row.Root, "DeleteBtn", "Supprimer",
                new Vector2(0.62f, 0.08f), new Vector2(0.85f, 0.45f), DeleteColor);
            row.DeleteButton.onClick.AddListener(() => OnDeleteSlotClicked(capturedSlot));

            return row;
        }

        /// <summary>Construit le panneau de confirmation de suppression.</summary>
        private void BuildConfirmPanel(RectTransform parent)
        {
            _confirmPanel = CreatePanel(parent, "ConfirmDeletePanel", ConfirmBgColor,
                new Vector2(0.20f, 0.30f), new Vector2(0.80f, 0.70f));
            var confirmRT = _confirmPanel.GetComponent<RectTransform>();

            CreateText(confirmRT, "ConfirmText", "Supprimer cette sauvegarde ?", 22,
                new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.90f),
                TextAlignmentOptions.Center, TextColor);

            var btnYes = CreateButton(confirmRT, "BtnYes", "Confirmer",
                new Vector2(0.10f, 0.10f), new Vector2(0.45f, 0.45f), DeleteColor);
            btnYes.onClick.AddListener(ConfirmDelete);

            var btnNo = CreateButton(confirmRT, "BtnNo", "Annuler",
                new Vector2(0.55f, 0.10f), new Vector2(0.90f, 0.45f), ButtonColor);
            btnNo.onClick.AddListener(CancelDelete);

            _confirmPanel.SetActive(false);
        }

        // ==========================
        //  Affichage / Masquage
        // ==========================

        /// <summary>Affiche le panneau et rafraîchit les slots.</summary>
        public void Show()
        {
            if (_panel != null)
            {
                _panel.SetActive(true);
                RefreshSlots();
            }
        }

        /// <summary>Masque le panneau.</summary>
        public void Hide()
        {
            if (_panel != null)
                _panel.SetActive(false);
        }

        /// <summary>Indique si le panneau est visible.</summary>
        public bool IsVisible => _panel != null && _panel.activeSelf;

        // ==========================
        //  Rafraîchissement — GDD §18.3
        // ==========================

        /// <summary>Rafraîchit l'affichage de tous les slots avec les données du SaveManager.</summary>
        public void RefreshSlots()
        {
            if (SaveManager.Instance == null) return;

            for (int i = 0; i < SaveManager.MAX_SLOTS; i++)
            {
                RefreshSlot(i);
            }
        }

        /// <summary>Rafraîchit un slot individuel.</summary>
        private void RefreshSlot(int slot)
        {
            if (_slots == null || slot < 0 || slot >= _slots.Length) return;

            var row = _slots[slot];
            SaveMetadata meta = SaveManager.Instance.GetSaveInfo(slot);

            if (meta != null)
            {
                // Slot occupé
                row.Root.GetComponent<Image>().color = SlotColor;
                row.NameLabel.text = !string.IsNullOrEmpty(meta.PlayerName) ? meta.PlayerName : $"Slot {slot + 1}";
                row.InfoLabel.text = $"Run {meta.RunCount} — {FormatPlayTime(meta.PlayTime)}";
                row.DateLabel.text = FormatDate(meta.LastSavedAt);
                row.LoadButton.interactable = true;
                row.DeleteButton.interactable = true;
            }
            else
            {
                // Slot vide
                row.Root.GetComponent<Image>().color = SlotEmptyColor;
                row.NameLabel.text = "Slot vide";
                row.InfoLabel.text = "";
                row.DateLabel.text = "";
                row.LoadButton.interactable = false;
                row.DeleteButton.interactable = false;
            }
        }

        // ==========================
        //  Callbacks
        // ==========================

        /// <summary>Charge le profil du slot sélectionné. GDD §18.3.</summary>
        private void OnLoadSlotClicked(int slot)
        {
            if (SaveManager.Instance == null || GameManager.Instance == null) return;

            var profile = SaveManager.Instance.LoadGame(slot);
            if (profile != null)
            {
                GameManager.Instance.LoadProfile(profile);
                Debug.Log($"[SaveSlotUI] Slot {slot} chargé → passage à MainUI.");
                OnSlotLoaded?.Invoke();
            }
            else
            {
                Debug.LogWarning($"[SaveSlotUI] Échec du chargement du slot {slot}.");
            }
        }

        /// <summary>Ouvre la confirmation de suppression.</summary>
        private void OnDeleteSlotClicked(int slot)
        {
            _pendingDeleteSlot = slot;
            if (_confirmPanel != null)
                _confirmPanel.SetActive(true);
        }

        /// <summary>Confirme la suppression du slot.</summary>
        private void ConfirmDelete()
        {
            if (_pendingDeleteSlot >= 0 && SaveManager.Instance != null)
            {
                SaveManager.Instance.DeleteSave(_pendingDeleteSlot);
                Debug.Log($"[SaveSlotUI] Slot {_pendingDeleteSlot} supprimé.");
                RefreshSlots();
            }

            _pendingDeleteSlot = -1;
            if (_confirmPanel != null)
                _confirmPanel.SetActive(false);
        }

        /// <summary>Annule la suppression.</summary>
        private void CancelDelete()
        {
            _pendingDeleteSlot = -1;
            if (_confirmPanel != null)
                _confirmPanel.SetActive(false);
        }

        // ==========================
        //  Formatage
        // ==========================

        /// <summary>Formate le temps de jeu en heures et minutes.</summary>
        private string FormatPlayTime(float totalSeconds)
        {
            TimeSpan ts = TimeSpan.FromSeconds(totalSeconds);
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours}h {ts.Minutes:D2}m";
            return $"{ts.Minutes}m {ts.Seconds:D2}s";
        }

        /// <summary>Formate une date ISO 8601 en format lisible.</summary>
        private string FormatDate(string isoDate)
        {
            if (string.IsNullOrEmpty(isoDate)) return "";
            try
            {
                DateTime dt = DateTime.Parse(isoDate);
                return dt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            }
            catch
            {
                return isoDate;
            }
        }

        // ==========================
        //  Helpers UI (style MainUISpawner)
        // ==========================

        private GameObject CreatePanel(RectTransform parent, string name, Color color,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            go.GetComponent<Image>().color = color;
            return go;
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
            tmp.overflowMode = TextOverflowModes.Ellipsis;

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
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return go.GetComponent<Button>();
        }
    }
}
