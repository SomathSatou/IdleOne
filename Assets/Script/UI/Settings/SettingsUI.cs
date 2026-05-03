using UnityEngine;
using UnityEngine.UI;
using Assets.Script.UI.Tabs;
using Assets.Script.Managers;

namespace Assets.Script.UI.Settings
{
    /// <summary>
    /// MonoBehaviour principal du panneau de paramètres (overlay).
    /// Gère l'affichage/masquage du panneau et les boutons Fermer/Réinitialiser.
    /// Utilise <see cref="TabManager"/> pour les sous-onglets (Sauvegarde, Audio, Graphismes).
    /// GDD §17.1 — Catégories de Paramètres.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        // ==========================
        //  Références UI
        // ==========================

        /// <summary>TabManager gérant les sous-onglets du panneau Settings.</summary>
        private TabManager _tabManager;

        /// <summary>Bouton pour fermer le panneau Settings.</summary>
        private Button _closeButton;

        /// <summary>Bouton pour réinitialiser tous les paramètres aux défauts.</summary>
        private Button _resetButton;

        /// <summary>Référence au sous-onglet Sauvegarde.</summary>
        private SaveSettingsUI _saveSettingsUI;

        /// <summary>Référence au sous-onglet Audio.</summary>
        private AudioSettingsUI _audioSettingsUI;

        /// <summary>Référence au sous-onglet Graphismes.</summary>
        private GraphicsSettingsUI _graphicsSettingsUI;

        // ==========================
        //  Initialisation
        // ==========================

        /// <summary>
        /// Injection manuelle des références UI.
        /// Utilisé quand le panneau est construit programmatiquement par le SettingsSpawner.
        /// GDD §17.1.
        /// </summary>
        public void Initialize(
            TabManager tabManager,
            Button closeButton,
            Button resetButton,
            SaveSettingsUI saveSettingsUI,
            AudioSettingsUI audioSettingsUI,
            GraphicsSettingsUI graphicsSettingsUI)
        {
            _tabManager = tabManager;
            _closeButton = closeButton;
            _resetButton = resetButton;
            _saveSettingsUI = saveSettingsUI;
            _audioSettingsUI = audioSettingsUI;
            _graphicsSettingsUI = graphicsSettingsUI;

            // Bouton fermer
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);

            // Bouton réinitialiser
            if (_resetButton != null)
                _resetButton.onClick.AddListener(OnResetClicked);
        }

        // ==========================
        //  Affichage / Masquage
        // ==========================

        /// <summary>
        /// Affiche le panneau de paramètres et rafraîchit tous les sous-onglets.
        /// GDD §17.1.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            Refresh();
        }

        /// <summary>
        /// Masque le panneau de paramètres.
        /// GDD §17.1.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Bascule la visibilité du panneau de paramètres.
        /// </summary>
        public void Toggle()
        {
            if (gameObject.activeSelf)
                Hide();
            else
                Show();
        }

        // ==========================
        //  Rafraîchissement
        // ==========================

        /// <summary>
        /// Rafraîchit tous les sous-onglets avec les valeurs actuelles du SettingsManager.
        /// </summary>
        public void Refresh()
        {
            if (_saveSettingsUI != null) _saveSettingsUI.Refresh();
            if (_audioSettingsUI != null) _audioSettingsUI.Refresh();
            if (_graphicsSettingsUI != null) _graphicsSettingsUI.Refresh();
        }

        // ==========================
        //  Callbacks
        // ==========================

        /// <summary>Réinitialise tous les paramètres et rafraîchit l'affichage.</summary>
        private void OnResetClicked()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.ResetToDefaults();
            }

            Refresh();
            Debug.Log("[SettingsUI] Paramètres réinitialisés aux valeurs par défaut.");
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Hide);

            if (_resetButton != null)
                _resetButton.onClick.RemoveListener(OnResetClicked);
        }
    }
}
