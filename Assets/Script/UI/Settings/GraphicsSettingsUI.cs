using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Assets.Script.Managers;

namespace Assets.Script.UI.Settings
{
    /// <summary>
    /// Sous-onglet "Graphismes" du panneau de paramètres.
    /// Gère les dropdowns de qualité et résolution, le toggle plein écran,
    /// et le bouton "Appliquer" (les changements graphiques ne sont pas en temps réel).
    /// GDD §17.4 — Graphismes (qualité, résolution, plein écran).
    /// </summary>
    public class GraphicsSettingsUI : MonoBehaviour
    {
        // ==========================
        //  Références UI
        // ==========================

        private TMP_Dropdown _qualityDropdown;
        private Toggle _fullscreenToggle;
        private TMP_Dropdown _resolutionDropdown;
        private Button _applyButton;

        // ==========================
        //  État interne
        // ==========================

        /// <summary>Résolutions disponibles, mises en cache au Refresh.</summary>
        private Resolution[] _cachedResolutions;

        // Valeurs en attente (appliquées au clic "Appliquer")
        private int _pendingQualityLevel;
        private bool _pendingFullscreen;
        private int _pendingResolutionIndex;

        // ==========================
        //  Initialisation
        // ==========================

        /// <summary>
        /// Injection manuelle des références UI.
        /// Utilisé quand le panneau est construit programmatiquement.
        /// GDD §17.4.
        /// </summary>
        public void Initialize(
            TMP_Dropdown qualityDropdown,
            Toggle fullscreenToggle,
            TMP_Dropdown resolutionDropdown,
            Button applyButton)
        {
            _qualityDropdown = qualityDropdown;
            _fullscreenToggle = fullscreenToggle;
            _resolutionDropdown = resolutionDropdown;
            _applyButton = applyButton;

            // Listeners (les changements modifient les valeurs en attente, pas le rendu)
            if (_qualityDropdown != null)
                _qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            if (_fullscreenToggle != null)
                _fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            if (_resolutionDropdown != null)
                _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            if (_applyButton != null)
                _applyButton.onClick.AddListener(OnApplyClicked);

            Refresh();
        }

        // ==========================
        //  Rafraîchissement
        // ==========================

        /// <summary>
        /// Met à jour l'affichage des contrôles avec les valeurs actuelles du SettingsManager.
        /// Reconstruit les listes de qualité et résolution.
        /// GDD §17.4.
        /// </summary>
        public void Refresh()
        {
            if (SettingsManager.Instance == null) return;

            PopulateQualityDropdown();
            PopulateResolutionDropdown();

            _pendingQualityLevel = SettingsManager.Instance.QualityLevel;
            _pendingFullscreen = SettingsManager.Instance.Fullscreen;
            _pendingResolutionIndex = SettingsManager.Instance.ResolutionIndex;

            if (_qualityDropdown != null)
                _qualityDropdown.SetValueWithoutNotify(_pendingQualityLevel);
            if (_fullscreenToggle != null)
                _fullscreenToggle.SetIsOnWithoutNotify(_pendingFullscreen);
            if (_resolutionDropdown != null)
                _resolutionDropdown.SetValueWithoutNotify(_pendingResolutionIndex);
        }

        // ==========================
        //  Peuplement des dropdowns
        // ==========================

        /// <summary>Remplit le dropdown de qualité avec les niveaux Unity (QualitySettings.names).</summary>
        private void PopulateQualityDropdown()
        {
            if (_qualityDropdown == null) return;

            _qualityDropdown.ClearOptions();
            var options = new List<string>(QualitySettings.names);
            _qualityDropdown.AddOptions(options);
        }

        /// <summary>Remplit le dropdown de résolution avec les résolutions disponibles (Screen.resolutions).</summary>
        private void PopulateResolutionDropdown()
        {
            if (_resolutionDropdown == null) return;

            _cachedResolutions = Screen.resolutions;
            _resolutionDropdown.ClearOptions();

            var options = new List<string>();
            for (int i = 0; i < _cachedResolutions.Length; i++)
            {
                Resolution res = _cachedResolutions[i];
                options.Add($"{res.width} x {res.height} @ {res.refreshRateRatio.value:F0}Hz");
            }

            _resolutionDropdown.AddOptions(options);
        }

        // ==========================
        //  Callbacks
        // ==========================

        /// <summary>Stocke le niveau de qualité sélectionné (en attente d'application). GDD §17.4.</summary>
        private void OnQualityChanged(int index)
        {
            _pendingQualityLevel = index;
        }

        /// <summary>Stocke le choix plein écran (en attente d'application). GDD §17.4.</summary>
        private void OnFullscreenChanged(bool isOn)
        {
            _pendingFullscreen = isOn;
        }

        /// <summary>Stocke la résolution sélectionnée (en attente d'application). GDD §17.4.</summary>
        private void OnResolutionChanged(int index)
        {
            _pendingResolutionIndex = index;
        }

        /// <summary>
        /// Applique les changements graphiques en attente.
        /// Les paramètres ne prennent effet qu'au clic sur ce bouton.
        /// GDD §17.4.
        /// </summary>
        private void OnApplyClicked()
        {
            if (SettingsManager.Instance == null) return;

            SettingsManager.Instance.QualityLevel = _pendingQualityLevel;
            SettingsManager.Instance.Fullscreen = _pendingFullscreen;
            SettingsManager.Instance.ResolutionIndex = _pendingResolutionIndex;
            SettingsManager.Instance.ApplyGraphicsSettings();

            Debug.Log("[GraphicsSettingsUI] Paramètres graphiques appliqués.");
        }

        private void OnDestroy()
        {
            if (_qualityDropdown != null)
                _qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
            if (_fullscreenToggle != null)
                _fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
            if (_resolutionDropdown != null)
                _resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
            if (_applyButton != null)
                _applyButton.onClick.RemoveListener(OnApplyClicked);
        }
    }
}
