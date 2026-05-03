using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Script.Managers;

namespace Assets.Script.UI.Settings
{
    /// <summary>
    /// Sous-onglet "Sauvegarde" du panneau de paramètres.
    /// Gère le toggle auto-save, le slider d'intervalle, la sauvegarde manuelle,
    /// l'export/import de fichiers de sauvegarde, et l'affichage du dernier auto-save.
    /// GDD §17.2 — Sauvegarde (auto-save toggle, intervalle, export/import).
    /// </summary>
    public class SaveSettingsUI : MonoBehaviour
    {
        // ==========================
        //  Références UI — Auto-save
        // ==========================

        private Toggle _autoSaveToggle;
        private Slider _intervalSlider;
        private TMP_Text _intervalLabel;
        private Button _saveNowButton;

        // ==========================
        //  Références UI — Export
        // ==========================

        private TMP_Dropdown _exportSlotDropdown;
        private Button _exportButton;

        // ==========================
        //  Références UI — Import
        // ==========================

        private Button _importButton;
        private TMP_Dropdown _importSlotDropdown;

        // ==========================
        //  Références UI — Info
        // ==========================

        private TMP_Text _lastAutoSaveLabel;

        // ==========================
        //  Initialisation
        // ==========================

        /// <summary>
        /// Injection manuelle des références UI.
        /// Utilisé quand le panneau est construit programmatiquement.
        /// GDD §17.2.
        /// </summary>
        public void Initialize(
            Toggle autoSaveToggle,
            Slider intervalSlider,
            TMP_Text intervalLabel,
            Button saveNowButton,
            TMP_Dropdown exportSlotDropdown,
            Button exportButton,
            Button importButton,
            TMP_Dropdown importSlotDropdown,
            TMP_Text lastAutoSaveLabel)
        {
            _autoSaveToggle = autoSaveToggle;
            _intervalSlider = intervalSlider;
            _intervalLabel = intervalLabel;
            _saveNowButton = saveNowButton;
            _exportSlotDropdown = exportSlotDropdown;
            _exportButton = exportButton;
            _importButton = importButton;
            _importSlotDropdown = importSlotDropdown;
            _lastAutoSaveLabel = lastAutoSaveLabel;

            // Configurer le slider d'intervalle (1-60 min, pas de 1 min)
            if (_intervalSlider != null)
            {
                _intervalSlider.minValue = 1f;
                _intervalSlider.maxValue = 60f;
                _intervalSlider.wholeNumbers = true;
                _intervalSlider.onValueChanged.AddListener(OnIntervalChanged);
            }

            // Listeners
            if (_autoSaveToggle != null)
                _autoSaveToggle.onValueChanged.AddListener(OnAutoSaveToggled);
            if (_saveNowButton != null)
                _saveNowButton.onClick.AddListener(OnSaveNowClicked);
            if (_exportButton != null)
                _exportButton.onClick.AddListener(OnExportClicked);
            if (_importButton != null)
                _importButton.onClick.AddListener(OnImportClicked);

            // Peupler les dropdowns de slots
            PopulateSlotDropdown(_exportSlotDropdown);
            PopulateSlotDropdown(_importSlotDropdown);

            // S'abonner à l'event auto-save pour mettre à jour le label
            if (SaveManager.Instance != null)
                SaveManager.Instance.OnAutoSave += OnAutoSaveOccurred;

            Refresh();
        }

        // ==========================
        //  Rafraîchissement
        // ==========================

        /// <summary>
        /// Met à jour l'affichage des contrôles avec les valeurs actuelles du SettingsManager.
        /// GDD §17.2.
        /// </summary>
        public void Refresh()
        {
            if (SettingsManager.Instance == null) return;

            if (_autoSaveToggle != null)
                _autoSaveToggle.SetIsOnWithoutNotify(SettingsManager.Instance.AutoSaveEnabled);

            if (_intervalSlider != null)
            {
                _intervalSlider.SetValueWithoutNotify(SettingsManager.Instance.AutoSaveIntervalMinutes);
                UpdateIntervalLabel(SettingsManager.Instance.AutoSaveIntervalMinutes);
            }

            // Activer/désactiver le slider selon l'état de l'auto-save
            if (_intervalSlider != null)
                _intervalSlider.interactable = SettingsManager.Instance.AutoSaveEnabled;

            RefreshLastAutoSaveLabel();
        }

        // ==========================
        //  Callbacks — Auto-save
        // ==========================

        /// <summary>Appelé quand le toggle auto-save change. GDD §17.2.</summary>
        private void OnAutoSaveToggled(bool isOn)
        {
            if (SettingsManager.Instance == null) return;

            SettingsManager.Instance.AutoSaveEnabled = isOn;
            SettingsManager.Instance.SyncSaveManager();

            // Activer/désactiver le slider
            if (_intervalSlider != null)
                _intervalSlider.interactable = isOn;
        }

        /// <summary>Appelé quand le slider d'intervalle change. GDD §17.2.</summary>
        private void OnIntervalChanged(float value)
        {
            if (SettingsManager.Instance == null) return;

            SettingsManager.Instance.AutoSaveIntervalMinutes = value;
            SettingsManager.Instance.SyncSaveManager();
            UpdateIntervalLabel(value);
        }

        /// <summary>
        /// Sauvegarde immédiate dans le slot actif.
        /// GDD §17.2.
        /// </summary>
        private void OnSaveNowClicked()
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogWarning("[SaveSettingsUI] SaveManager non disponible.");
                return;
            }

            SaveManager.Instance.SaveGame(SaveManager.Instance.ActiveSlot);
            RefreshLastAutoSaveLabel();
            Debug.Log($"[SaveSettingsUI] Sauvegarde manuelle effectuée (slot {SaveManager.Instance.ActiveSlot}).");
        }

        // ==========================
        //  Callbacks — Export
        // ==========================

        /// <summary>
        /// Exporte le slot sélectionné vers Application.persistentDataPath.
        /// En l'absence de EditorUtility.SaveFilePanel (non disponible en build),
        /// utilise un chemin par défaut.
        /// GDD §17.2, §16.3.
        /// </summary>
        private void OnExportClicked()
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogWarning("[SaveSettingsUI] SaveManager non disponible.");
                return;
            }

            int slot = _exportSlotDropdown != null ? _exportSlotDropdown.value : 0;

            if (!SaveManager.Instance.SaveExists(slot))
            {
                Debug.LogWarning($"[SaveSettingsUI] Aucune sauvegarde dans le slot {slot} à exporter.");
                return;
            }

            // Chemin par défaut : persistentDataPath/exports/
            string exportDir = System.IO.Path.Combine(Application.persistentDataPath, "exports");
            if (!System.IO.Directory.Exists(exportDir))
                System.IO.Directory.CreateDirectory(exportDir);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string exportPath = System.IO.Path.Combine(exportDir, $"idleone_slot{slot}_{timestamp}");

            SaveManager.Instance.ExportSave(slot, exportPath);
            Debug.Log($"[SaveSettingsUI] Export slot {slot} → {exportPath}{SaveManager.EXPORT_EXTENSION}");
        }

        // ==========================
        //  Callbacks — Import
        // ==========================

        /// <summary>
        /// Importe un fichier de sauvegarde dans le slot de destination sélectionné.
        /// Cherche le fichier .idleonesave le plus récent dans le dossier exports.
        /// En l'absence de dialogue de fichier natif, utilise le dossier par défaut.
        /// GDD §17.2, §16.3.
        /// </summary>
        private void OnImportClicked()
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogWarning("[SaveSettingsUI] SaveManager non disponible.");
                return;
            }

            int destinationSlot = _importSlotDropdown != null ? _importSlotDropdown.value : 0;

            // Chercher le fichier .idleonesave le plus récent dans exports/
            string exportDir = System.IO.Path.Combine(Application.persistentDataPath, "exports");
            if (!System.IO.Directory.Exists(exportDir))
            {
                Debug.LogWarning("[SaveSettingsUI] Aucun dossier d'export trouvé.");
                return;
            }

            string[] files = System.IO.Directory.GetFiles(exportDir, $"*{SaveManager.EXPORT_EXTENSION}");
            if (files.Length == 0)
            {
                Debug.LogWarning("[SaveSettingsUI] Aucun fichier d'export trouvé.");
                return;
            }

            // Prendre le plus récent
            string mostRecent = files[0];
            DateTime mostRecentTime = System.IO.File.GetLastWriteTime(mostRecent);
            for (int i = 1; i < files.Length; i++)
            {
                DateTime fileTime = System.IO.File.GetLastWriteTime(files[i]);
                if (fileTime > mostRecentTime)
                {
                    mostRecent = files[i];
                    mostRecentTime = fileTime;
                }
            }

            bool success = SaveManager.Instance.ImportSave(mostRecent, destinationSlot);
            if (success)
                Debug.Log($"[SaveSettingsUI] Import réussi → slot {destinationSlot} depuis {mostRecent}");
            else
                Debug.LogWarning($"[SaveSettingsUI] Échec de l'import depuis {mostRecent}");
        }

        // ==========================
        //  Auto-save event
        // ==========================

        /// <summary>Appelé quand un auto-save est effectué, met à jour le label.</summary>
        private void OnAutoSaveOccurred()
        {
            RefreshLastAutoSaveLabel();
        }

        // ==========================
        //  Utilitaires
        // ==========================

        /// <summary>Peuple un dropdown avec les numéros de slots (0, 1, 2).</summary>
        private void PopulateSlotDropdown(TMP_Dropdown dropdown)
        {
            if (dropdown == null) return;

            dropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>();
            for (int i = 0; i < SaveManager.MAX_SLOTS; i++)
            {
                options.Add($"Slot {i}");
            }
            dropdown.AddOptions(options);
        }

        /// <summary>Met à jour le label d'intervalle auto-save.</summary>
        private void UpdateIntervalLabel(float minutes)
        {
            if (_intervalLabel != null)
                _intervalLabel.text = $"{Mathf.RoundToInt(minutes)} min";
        }

        /// <summary>Rafraîchit le label du dernier auto-save.</summary>
        private void RefreshLastAutoSaveLabel()
        {
            if (_lastAutoSaveLabel == null) return;

            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentProfile != null &&
                !string.IsNullOrEmpty(GameManager.Instance.CurrentProfile.LastSavedAt))
            {
                if (DateTime.TryParse(GameManager.Instance.CurrentProfile.LastSavedAt, out DateTime lastSave))
                {
                    _lastAutoSaveLabel.text = $"Dernière sauvegarde : {lastSave.ToLocalTime():dd/MM/yyyy HH:mm:ss}";
                }
                else
                {
                    _lastAutoSaveLabel.text = "Dernière sauvegarde : —";
                }
            }
            else
            {
                _lastAutoSaveLabel.text = "Dernière sauvegarde : —";
            }
        }

        private void OnDestroy()
        {
            if (_autoSaveToggle != null)
                _autoSaveToggle.onValueChanged.RemoveListener(OnAutoSaveToggled);
            if (_intervalSlider != null)
                _intervalSlider.onValueChanged.RemoveListener(OnIntervalChanged);
            if (_saveNowButton != null)
                _saveNowButton.onClick.RemoveListener(OnSaveNowClicked);
            if (_exportButton != null)
                _exportButton.onClick.RemoveListener(OnExportClicked);
            if (_importButton != null)
                _importButton.onClick.RemoveListener(OnImportClicked);

            if (SaveManager.Instance != null)
                SaveManager.Instance.OnAutoSave -= OnAutoSaveOccurred;
        }
    }
}
