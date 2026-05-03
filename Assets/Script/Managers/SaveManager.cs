using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Assets.Script.Entities;

namespace Assets.Script.Managers
{
    /// <summary>
    /// Singleton gérant la sauvegarde, le chargement, l'auto-save,
    /// l'export et l'import de fichiers de sauvegarde.
    /// GDD §16 — Système de Sauvegarde.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        // ==========================
        //  Singleton
        // ==========================

        /// <summary>Instance unique du SaveManager, accessible globalement.</summary>
        public static SaveManager Instance { get; private set; }

        // ==========================
        //  Constantes
        // ==========================

        /// <summary>Version actuelle du format de sauvegarde. GDD §16.5.</summary>
        public const string SAVE_VERSION = "1.0.0";

        /// <summary>Extension custom pour les fichiers exportés. GDD §16.3.</summary>
        public const string EXPORT_EXTENSION = ".idleonesave";

        /// <summary>Nombre maximum de slots de sauvegarde. GDD §16.1.</summary>
        public const int MAX_SLOTS = 3;

        private const string SAVE_FOLDER = "saves";
        private const string SAVE_FILE_PREFIX = "save_slot";
        private const string SAVE_FILE_EXTENSION = ".json";
        private const string ACTIVE_SLOT_PREF_KEY = "ActiveSaveSlot";

        // ==========================
        //  Auto-save
        // ==========================

        /// <summary>
        /// Intervalle de l'auto-save en minutes.
        /// GDD §16.2 — Défaut : 5 min, min : 1, max : 60.
        /// </summary>
        [Range(1f, 60f)]
        public float AutoSaveIntervalMinutes = 5f;

        /// <summary>Active ou désactive l'auto-save. GDD §16.2.</summary>
        public bool AutoSaveEnabled = true;

        private Coroutine _autoSaveCoroutine;

        // ==========================
        //  Events
        // ==========================

        /// <summary>Déclenché après une sauvegarde réussie. Paramètre : numéro du slot.</summary>
        public event Action<int> OnSaveCompleted;

        /// <summary>Déclenché après un chargement réussi. Paramètre : numéro du slot.</summary>
        public event Action<int> OnLoadCompleted;

        /// <summary>Déclenché après chaque auto-save réussie.</summary>
        public event Action OnAutoSave;

        // ==========================
        //  Propriétés
        // ==========================

        /// <summary>Chemin du dossier de sauvegarde.</summary>
        public string SaveFolderPath => Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

        /// <summary>
        /// Slot actif utilisé par l'auto-save.
        /// Stocké dans PlayerPrefs. GDD §16.2.
        /// </summary>
        public int ActiveSlot
        {
            get => PlayerPrefs.GetInt(ACTIVE_SLOT_PREF_KEY, 0);
            set
            {
                int clamped = Mathf.Clamp(value, 0, MAX_SLOTS - 1);
                PlayerPrefs.SetInt(ACTIVE_SLOT_PREF_KEY, clamped);
                PlayerPrefs.Save();
            }
        }

        // ==========================
        //  Cycle de vie Unity
        // ==========================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureSaveFolderExists();
        }

        private void Start()
        {
            StartAutoSave();
        }

        private void OnDestroy()
        {
            StopAutoSave();
        }

        // ==========================
        //  Sauvegarde — GDD §16.1
        // ==========================

        /// <summary>
        /// Sauvegarde le profil actuel dans le slot spécifié.
        /// Sérialise le PlayerProfile en JSON via JsonUtility et écrit le fichier.
        /// Met à jour LastSavedAt sur le profil.
        /// GDD §16.1, §16.4.
        /// </summary>
        public void SaveGame(int slot)
        {
            if (!IsValidSlot(slot))
            {
                Debug.LogWarning($"[SaveManager] SaveGame : slot {slot} invalide (0-{MAX_SLOTS - 1}).");
                return;
            }

            PlayerProfile profile = GameManager.Instance != null ? GameManager.Instance.CurrentProfile : null;
            if (profile == null)
            {
                Debug.LogWarning("[SaveManager] SaveGame : aucun profil chargé dans GameManager.");
                return;
            }

            try
            {
                // Mettre à jour le timestamp
                profile.LastSavedAt = DateTime.UtcNow.ToString("o");

                // Construire le SaveData avec conversion des Dictionary
                SaveData saveData = SaveData.FromProfile(profile, SAVE_VERSION);

                // Sérialiser en JSON
                string json = JsonUtility.ToJson(saveData, true);

                // Écrire le fichier
                string filePath = GetSlotFilePath(slot);
                EnsureSaveFolderExists();
                File.WriteAllText(filePath, json);

                Debug.Log($"[SaveManager] Sauvegarde slot {slot} réussie → {filePath}");
                OnSaveCompleted?.Invoke(slot);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Erreur lors de la sauvegarde slot {slot} : {e.Message}");
            }
        }

        // ==========================
        //  Chargement — GDD §16.1
        // ==========================

        /// <summary>
        /// Charge un profil depuis le slot spécifié.
        /// Lit le fichier JSON, désérialise, restaure les Dictionary, et retourne le profil.
        /// Retourne null si le slot n'existe pas ou en cas d'erreur.
        /// GDD §16.1, §16.4.
        /// </summary>
        public PlayerProfile LoadGame(int slot)
        {
            if (!IsValidSlot(slot))
            {
                Debug.LogWarning($"[SaveManager] LoadGame : slot {slot} invalide (0-{MAX_SLOTS - 1}).");
                return null;
            }

            if (!SaveExists(slot))
            {
                Debug.LogWarning($"[SaveManager] LoadGame : aucune sauvegarde dans le slot {slot}.");
                return null;
            }

            try
            {
                string filePath = GetSlotFilePath(slot);
                string json = File.ReadAllText(filePath);

                SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                if (saveData == null || saveData.Profile == null)
                {
                    Debug.LogError($"[SaveManager] LoadGame : données corrompues dans le slot {slot}.");
                    return null;
                }

                // Restaurer les Dictionary depuis les listes sérialisées
                saveData.RestoreDictionaries();

                ActiveSlot = slot;

                Debug.Log($"[SaveManager] Chargement slot {slot} réussi — joueur '{saveData.Profile.PlayerName}'");
                OnLoadCompleted?.Invoke(slot);

                return saveData.Profile;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Erreur lors du chargement slot {slot} : {e.Message}");
                return null;
            }
        }

        // ==========================
        //  Vérification & Métadonnées
        // ==========================

        /// <summary>
        /// Vérifie si une sauvegarde existe dans le slot spécifié.
        /// GDD §16.1.
        /// </summary>
        public bool SaveExists(int slot)
        {
            if (!IsValidSlot(slot)) return false;
            return File.Exists(GetSlotFilePath(slot));
        }

        /// <summary>
        /// Retourne les métadonnées d'un slot sans charger tout le profil.
        /// Retourne null si le slot est vide ou en cas d'erreur.
        /// GDD §16.1.
        /// </summary>
        public SaveMetadata GetSaveInfo(int slot)
        {
            if (!IsValidSlot(slot) || !SaveExists(slot))
                return null;

            try
            {
                string filePath = GetSlotFilePath(slot);
                string json = File.ReadAllText(filePath);

                SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                return saveData?.Metadata;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Erreur lecture métadonnées slot {slot} : {e.Message}");
                return null;
            }
        }

        // ==========================
        //  Suppression
        // ==========================

        /// <summary>
        /// Supprime le fichier de sauvegarde du slot spécifié.
        /// GDD §16.1.
        /// </summary>
        public void DeleteSave(int slot)
        {
            if (!IsValidSlot(slot))
            {
                Debug.LogWarning($"[SaveManager] DeleteSave : slot {slot} invalide.");
                return;
            }

            try
            {
                string filePath = GetSlotFilePath(slot);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"[SaveManager] Sauvegarde slot {slot} supprimée.");
                }
                else
                {
                    Debug.LogWarning($"[SaveManager] DeleteSave : aucun fichier dans le slot {slot}.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Erreur suppression slot {slot} : {e.Message}");
            }
        }

        // ==========================
        //  Export / Import — GDD §16.3
        // ==========================

        /// <summary>
        /// Exporte le fichier de sauvegarde d'un slot vers un chemin de destination.
        /// Le fichier exporté utilise l'extension .idleonesave.
        /// GDD §16.3.
        /// </summary>
        public void ExportSave(int slot, string destinationPath)
        {
            if (!IsValidSlot(slot))
            {
                Debug.LogWarning($"[SaveManager] ExportSave : slot {slot} invalide.");
                return;
            }

            if (!SaveExists(slot))
            {
                Debug.LogWarning($"[SaveManager] ExportSave : aucune sauvegarde dans le slot {slot}.");
                return;
            }

            try
            {
                string sourcePath = GetSlotFilePath(slot);

                // Ajouter l'extension custom si absente
                if (!destinationPath.EndsWith(EXPORT_EXTENSION))
                {
                    destinationPath += EXPORT_EXTENSION;
                }

                File.Copy(sourcePath, destinationPath, overwrite: true);
                Debug.Log($"[SaveManager] Export slot {slot} → {destinationPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Erreur export slot {slot} : {e.Message}");
            }
        }

        /// <summary>
        /// Importe un fichier de sauvegarde externe dans le slot spécifié.
        /// Valide que le fichier est un JSON valide avec le champ Version avant d'écraser.
        /// Retourne true si l'import a réussi.
        /// GDD §16.3.
        /// </summary>
        public bool ImportSave(string sourcePath, int slot)
        {
            if (!IsValidSlot(slot))
            {
                Debug.LogWarning($"[SaveManager] ImportSave : slot {slot} invalide.");
                return false;
            }

            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
            {
                Debug.LogWarning($"[SaveManager] ImportSave : fichier source introuvable — {sourcePath}");
                return false;
            }

            try
            {
                string json = File.ReadAllText(sourcePath);

                // Valider que le JSON est un SaveData valide avec un champ Version
                SaveData importedData = JsonUtility.FromJson<SaveData>(json);
                if (importedData == null || string.IsNullOrEmpty(importedData.Version))
                {
                    Debug.LogWarning("[SaveManager] ImportSave : fichier invalide (pas de champ Version).");
                    return false;
                }

                if (importedData.Profile == null)
                {
                    Debug.LogWarning("[SaveManager] ImportSave : fichier invalide (pas de profil).");
                    return false;
                }

                // Copier le fichier dans le slot
                string destinationPath = GetSlotFilePath(slot);
                EnsureSaveFolderExists();
                File.WriteAllText(destinationPath, json);

                Debug.Log($"[SaveManager] Import réussi → slot {slot} (joueur '{importedData.Metadata?.PlayerName}')");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Erreur import vers slot {slot} : {e.Message}");
                return false;
            }
        }

        // ==========================
        //  Auto-save — GDD §16.2
        // ==========================

        /// <summary>
        /// Modifie l'intervalle d'auto-save et redémarre la coroutine.
        /// L'intervalle est clampé entre 1 et 60 minutes.
        /// GDD §16.2.
        /// </summary>
        public void SetAutoSaveInterval(float minutes)
        {
            AutoSaveIntervalMinutes = Mathf.Clamp(minutes, 1f, 60f);
            if (AutoSaveEnabled)
            {
                StopAutoSave();
                StartAutoSave();
            }
        }

        /// <summary>
        /// Démarre la coroutine d'auto-save si elle n'est pas déjà active.
        /// </summary>
        private void StartAutoSave()
        {
            if (!AutoSaveEnabled) return;

            StopAutoSave();
            _autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
            Debug.Log($"[SaveManager] Auto-save démarré (intervalle : {AutoSaveIntervalMinutes} min, slot : {ActiveSlot}).");
        }

        /// <summary>
        /// Arrête la coroutine d'auto-save.
        /// </summary>
        private void StopAutoSave()
        {
            if (_autoSaveCoroutine != null)
            {
                StopCoroutine(_autoSaveCoroutine);
                _autoSaveCoroutine = null;
            }
        }

        /// <summary>
        /// Coroutine d'auto-save. Sauvegarde dans le slot actif à intervalle régulier.
        /// Utilise WaitForSecondsRealtime pour ne pas être affectée par Time.timeScale.
        /// GDD §16.2.
        /// </summary>
        private IEnumerator AutoSaveRoutine()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(AutoSaveIntervalMinutes * 60f);

                if (!AutoSaveEnabled) continue;

                if (GameManager.Instance != null && GameManager.Instance.CurrentProfile != null)
                {
                    SaveGame(ActiveSlot);
                    Debug.Log($"[SaveManager] Auto-save effectué (slot {ActiveSlot}).");
                    OnAutoSave?.Invoke();
                }
            }
        }

        // ==========================
        //  Utilitaires internes
        // ==========================

        /// <summary>
        /// Retourne le chemin complet du fichier de sauvegarde pour un slot donné.
        /// Format : {persistentDataPath}/saves/save_slot{N}.json
        /// </summary>
        private string GetSlotFilePath(int slot)
        {
            return Path.Combine(SaveFolderPath, $"{SAVE_FILE_PREFIX}{slot}{SAVE_FILE_EXTENSION}");
        }

        /// <summary>
        /// Vérifie si le numéro de slot est valide (0 à MAX_SLOTS-1).
        /// </summary>
        private bool IsValidSlot(int slot)
        {
            return slot >= 0 && slot < MAX_SLOTS;
        }

        /// <summary>
        /// Crée le dossier de sauvegarde s'il n'existe pas.
        /// </summary>
        private void EnsureSaveFolderExists()
        {
            try
            {
                if (!Directory.Exists(SaveFolderPath))
                {
                    Directory.CreateDirectory(SaveFolderPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Impossible de créer le dossier de sauvegarde : {e.Message}");
            }
        }
    }
}
