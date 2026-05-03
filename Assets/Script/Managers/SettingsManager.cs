using UnityEngine;

namespace Assets.Script.Managers
{
    /// <summary>
    /// Singleton gérant tous les paramètres du jeu (audio, graphismes, sauvegarde).
    /// Persiste les settings via PlayerPrefs, indépendamment des fichiers de save.
    /// GDD §17 — Paramètres du Jeu.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        // ==========================
        //  Singleton
        // ==========================

        /// <summary>Instance unique du SettingsManager, accessible globalement.</summary>
        public static SettingsManager Instance { get; private set; }

        // ==========================
        //  Clés PlayerPrefs
        // ==========================

        private const string KEY_MASTER_VOLUME = "Settings_MasterVolume";
        private const string KEY_MUSIC_VOLUME = "Settings_MusicVolume";
        private const string KEY_SFX_VOLUME = "Settings_SFXVolume";
        private const string KEY_MUTE_ALL = "Settings_MuteAll";
        private const string KEY_QUALITY_LEVEL = "Settings_QualityLevel";
        private const string KEY_FULLSCREEN = "Settings_Fullscreen";
        private const string KEY_RESOLUTION_INDEX = "Settings_ResolutionIndex";
        private const string KEY_AUTOSAVE_ENABLED = "Settings_AutoSaveEnabled";
        private const string KEY_AUTOSAVE_INTERVAL = "Settings_AutoSaveInterval";

        // ==========================
        //  Audio — GDD §17.3
        // ==========================

        /// <summary>Volume principal (0-1, défaut 1). GDD §17.3.</summary>
        public float MasterVolume { get; set; } = 1f;

        /// <summary>Volume musique (0-1, défaut 0.7). GDD §17.3.</summary>
        public float MusicVolume { get; set; } = 0.7f;

        /// <summary>Volume effets sonores (0-1, défaut 1). GDD §17.3.</summary>
        public float SFXVolume { get; set; } = 1f;

        /// <summary>Mute global (défaut false). GDD §17.3.</summary>
        public bool MuteAll { get; set; } = false;

        // ==========================
        //  Graphismes — GDD §17.4
        // ==========================

        /// <summary>Index du niveau de qualité Unity (défaut = max). GDD §17.4.</summary>
        public int QualityLevel { get; set; }

        /// <summary>Mode plein écran (défaut true). GDD §17.4.</summary>
        public bool Fullscreen { get; set; } = true;

        /// <summary>Index de la résolution dans Screen.resolutions. GDD §17.4.</summary>
        public int ResolutionIndex { get; set; }

        // ==========================
        //  Sauvegarde — GDD §17.2
        // ==========================

        /// <summary>Auto-save activée (miroir de SaveManager). GDD §17.2.</summary>
        public bool AutoSaveEnabled { get; set; } = true;

        /// <summary>Intervalle auto-save en minutes (miroir de SaveManager). GDD §17.2.</summary>
        public float AutoSaveIntervalMinutes { get; set; } = 5f;

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

            // Initialiser les défauts graphiques
            QualityLevel = QualitySettings.names.Length - 1;
            ResolutionIndex = GetCurrentResolutionIndex();

            LoadSettings();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // ==========================
        //  Persistance — GDD §17.5
        // ==========================

        /// <summary>
        /// Charge tous les paramètres depuis PlayerPrefs.
        /// Applique immédiatement les settings audio.
        /// GDD §17.5 — Persistance (PlayerPrefs).
        /// </summary>
        public void LoadSettings()
        {
            // Audio
            MasterVolume = PlayerPrefs.GetFloat(KEY_MASTER_VOLUME, 1f);
            MusicVolume = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 0.7f);
            SFXVolume = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f);
            MuteAll = PlayerPrefs.GetInt(KEY_MUTE_ALL, 0) == 1;

            // Graphismes
            QualityLevel = PlayerPrefs.GetInt(KEY_QUALITY_LEVEL, QualitySettings.names.Length - 1);
            Fullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1;
            ResolutionIndex = PlayerPrefs.GetInt(KEY_RESOLUTION_INDEX, GetCurrentResolutionIndex());

            // Sauvegarde
            AutoSaveEnabled = PlayerPrefs.GetInt(KEY_AUTOSAVE_ENABLED, 1) == 1;
            AutoSaveIntervalMinutes = PlayerPrefs.GetFloat(KEY_AUTOSAVE_INTERVAL, 5f);

            // Appliquer immédiatement l'audio
            ApplyAudioSettings();

            Debug.Log("[SettingsManager] Settings chargés depuis PlayerPrefs.");
        }

        /// <summary>
        /// Sauvegarde tous les paramètres dans PlayerPrefs.
        /// GDD §17.5 — Persistance (PlayerPrefs).
        /// </summary>
        public void SaveSettings()
        {
            // Audio
            PlayerPrefs.SetFloat(KEY_MASTER_VOLUME, MasterVolume);
            PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, MusicVolume);
            PlayerPrefs.SetFloat(KEY_SFX_VOLUME, SFXVolume);
            PlayerPrefs.SetInt(KEY_MUTE_ALL, MuteAll ? 1 : 0);

            // Graphismes
            PlayerPrefs.SetInt(KEY_QUALITY_LEVEL, QualityLevel);
            PlayerPrefs.SetInt(KEY_FULLSCREEN, Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(KEY_RESOLUTION_INDEX, ResolutionIndex);

            // Sauvegarde
            PlayerPrefs.SetInt(KEY_AUTOSAVE_ENABLED, AutoSaveEnabled ? 1 : 0);
            PlayerPrefs.SetFloat(KEY_AUTOSAVE_INTERVAL, AutoSaveIntervalMinutes);

            PlayerPrefs.Save();
            Debug.Log("[SettingsManager] Settings sauvegardés dans PlayerPrefs.");
        }

        /// <summary>
        /// Réinitialise tous les paramètres à leurs valeurs par défaut et les sauvegarde.
        /// GDD §17.1 — Catégories de Paramètres.
        /// </summary>
        public void ResetToDefaults()
        {
            // Audio
            MasterVolume = 1f;
            MusicVolume = 0.7f;
            SFXVolume = 1f;
            MuteAll = false;

            // Graphismes
            QualityLevel = QualitySettings.names.Length - 1;
            Fullscreen = true;
            ResolutionIndex = GetCurrentResolutionIndex();

            // Sauvegarde
            AutoSaveEnabled = true;
            AutoSaveIntervalMinutes = 5f;

            ApplyAudioSettings();
            ApplyGraphicsSettings();
            SyncSaveManager();
            SaveSettings();

            Debug.Log("[SettingsManager] Settings réinitialisés aux valeurs par défaut.");
        }

        // ==========================
        //  Application Audio — GDD §17.3
        // ==========================

        /// <summary>
        /// Applique les paramètres audio en temps réel.
        /// Utilise AudioListener.volume pour le volume global.
        /// GDD §17.3 — Audio (master, musique, SFX, mute).
        /// </summary>
        public void ApplyAudioSettings()
        {
            if (MuteAll)
            {
                AudioListener.volume = 0f;
            }
            else
            {
                AudioListener.volume = MasterVolume;
            }

            // Note : MusicVolume et SFXVolume seront utilisés par les AudioSource
            // individuelles ou un AudioMixer quand le système audio sera en place.
        }

        // ==========================
        //  Application Graphismes — GDD §17.4
        // ==========================

        /// <summary>
        /// Applique les paramètres graphiques.
        /// Nécessite un clic "Appliquer" depuis l'UI (pas en temps réel).
        /// GDD §17.4 — Graphismes (qualité, résolution, plein écran).
        /// </summary>
        public void ApplyGraphicsSettings()
        {
            // Qualité
            QualitySettings.SetQualityLevel(QualityLevel, true);

            // Plein écran
            Screen.fullScreen = Fullscreen;

            // Résolution
            Resolution[] resolutions = Screen.resolutions;
            if (ResolutionIndex >= 0 && ResolutionIndex < resolutions.Length)
            {
                Resolution res = resolutions[ResolutionIndex];
                Screen.SetResolution(res.width, res.height, Fullscreen);
            }

            SaveSettings();
            Debug.Log($"[SettingsManager] Graphismes appliqués — Qualité: {QualityLevel}, " +
                      $"Plein écran: {Fullscreen}, Résolution index: {ResolutionIndex}");
        }

        // ==========================
        //  Synchronisation SaveManager — GDD §17.2
        // ==========================

        /// <summary>
        /// Synchronise les paramètres de sauvegarde avec le SaveManager.
        /// Appelé quand les settings de sauvegarde changent.
        /// GDD §17.2 — Sauvegarde (auto-save toggle, intervalle).
        /// </summary>
        public void SyncSaveManager()
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogWarning("[SettingsManager] SyncSaveManager : SaveManager non disponible.");
                return;
            }

            SaveManager.Instance.AutoSaveEnabled = AutoSaveEnabled;
            SaveManager.Instance.SetAutoSaveInterval(AutoSaveIntervalMinutes);

            SaveSettings();
            Debug.Log($"[SettingsManager] SaveManager synchronisé — AutoSave: {AutoSaveEnabled}, " +
                      $"Intervalle: {AutoSaveIntervalMinutes} min");
        }

        // ==========================
        //  Utilitaires
        // ==========================

        /// <summary>Retourne l'index de la résolution actuelle dans Screen.resolutions.</summary>
        private int GetCurrentResolutionIndex()
        {
            Resolution[] resolutions = Screen.resolutions;
            Resolution current = Screen.currentResolution;

            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == current.width &&
                    resolutions[i].height == current.height)
                {
                    return i;
                }
            }

            return resolutions.Length > 0 ? resolutions.Length - 1 : 0;
        }
    }
}
