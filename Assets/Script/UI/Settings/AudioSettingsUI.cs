using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Script.Managers;

namespace Assets.Script.UI.Settings
{
    /// <summary>
    /// Sous-onglet "Audio" du panneau de paramètres.
    /// Gère les sliders de volume (Master, Musique, SFX) et le toggle Mute.
    /// Les changements audio s'appliquent en temps réel.
    /// GDD §17.3 — Audio (master, musique, SFX, mute).
    /// </summary>
    public class AudioSettingsUI : MonoBehaviour
    {
        // ==========================
        //  Références UI
        // ==========================

        private Slider _masterSlider;
        private TMP_Text _masterLabel;
        private Slider _musicSlider;
        private TMP_Text _musicLabel;
        private Slider _sfxSlider;
        private TMP_Text _sfxLabel;
        private Toggle _muteToggle;

        // ==========================
        //  Initialisation
        // ==========================

        /// <summary>
        /// Injection manuelle des références UI.
        /// Utilisé quand le panneau est construit programmatiquement.
        /// GDD §17.3.
        /// </summary>
        public void Initialize(
            Slider masterSlider,
            TMP_Text masterLabel,
            Slider musicSlider,
            TMP_Text musicLabel,
            Slider sfxSlider,
            TMP_Text sfxLabel,
            Toggle muteToggle)
        {
            _masterSlider = masterSlider;
            _masterLabel = masterLabel;
            _musicSlider = musicSlider;
            _musicLabel = musicLabel;
            _sfxSlider = sfxSlider;
            _sfxLabel = sfxLabel;
            _muteToggle = muteToggle;

            // Configurer les sliders (0-1, affiché 0-100%)
            SetupSlider(_masterSlider, 0f, 1f);
            SetupSlider(_musicSlider, 0f, 1f);
            SetupSlider(_sfxSlider, 0f, 1f);

            // Listeners
            if (_masterSlider != null)
                _masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (_musicSlider != null)
                _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (_sfxSlider != null)
                _sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            if (_muteToggle != null)
                _muteToggle.onValueChanged.AddListener(OnMuteToggled);

            Refresh();
        }

        // ==========================
        //  Rafraîchissement
        // ==========================

        /// <summary>
        /// Met à jour l'affichage des contrôles avec les valeurs actuelles du SettingsManager.
        /// GDD §17.3.
        /// </summary>
        public void Refresh()
        {
            if (SettingsManager.Instance == null) return;

            if (_masterSlider != null)
            {
                _masterSlider.SetValueWithoutNotify(SettingsManager.Instance.MasterVolume);
                UpdateLabel(_masterLabel, SettingsManager.Instance.MasterVolume);
            }

            if (_musicSlider != null)
            {
                _musicSlider.SetValueWithoutNotify(SettingsManager.Instance.MusicVolume);
                UpdateLabel(_musicLabel, SettingsManager.Instance.MusicVolume);
            }

            if (_sfxSlider != null)
            {
                _sfxSlider.SetValueWithoutNotify(SettingsManager.Instance.SFXVolume);
                UpdateLabel(_sfxLabel, SettingsManager.Instance.SFXVolume);
            }

            if (_muteToggle != null)
                _muteToggle.SetIsOnWithoutNotify(SettingsManager.Instance.MuteAll);
        }

        // ==========================
        //  Callbacks — Temps réel
        // ==========================

        /// <summary>Appelé quand le slider Master change. GDD §17.3.</summary>
        private void OnMasterVolumeChanged(float value)
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.MasterVolume = value;
            SettingsManager.Instance.ApplyAudioSettings();
            SettingsManager.Instance.SaveSettings();
            UpdateLabel(_masterLabel, value);
        }

        /// <summary>Appelé quand le slider Musique change. GDD §17.3.</summary>
        private void OnMusicVolumeChanged(float value)
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.MusicVolume = value;
            SettingsManager.Instance.ApplyAudioSettings();
            SettingsManager.Instance.SaveSettings();
            UpdateLabel(_musicLabel, value);
        }

        /// <summary>Appelé quand le slider SFX change. GDD §17.3.</summary>
        private void OnSFXVolumeChanged(float value)
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.SFXVolume = value;
            SettingsManager.Instance.ApplyAudioSettings();
            SettingsManager.Instance.SaveSettings();
            UpdateLabel(_sfxLabel, value);
        }

        /// <summary>Appelé quand le toggle Mute change. GDD §17.3.</summary>
        private void OnMuteToggled(bool isMuted)
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.MuteAll = isMuted;
            SettingsManager.Instance.ApplyAudioSettings();
            SettingsManager.Instance.SaveSettings();
        }

        // ==========================
        //  Utilitaires
        // ==========================

        /// <summary>Configure un slider avec les bornes min/max.</summary>
        private void SetupSlider(Slider slider, float min, float max)
        {
            if (slider == null) return;
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = false;
        }

        /// <summary>Met à jour le label d'un slider avec le pourcentage affiché.</summary>
        private void UpdateLabel(TMP_Text label, float value01)
        {
            if (label != null)
                label.text = $"{Mathf.RoundToInt(value01 * 100)}%";
        }

        private void OnDestroy()
        {
            if (_masterSlider != null)
                _masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            if (_musicSlider != null)
                _musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (_sfxSlider != null)
                _sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            if (_muteToggle != null)
                _muteToggle.onValueChanged.RemoveListener(OnMuteToggled);
        }
    }
}
