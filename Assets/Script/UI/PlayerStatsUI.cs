using UnityEngine;
using TMPro;
using Assets.Script.Entities;
using Assets.Script.Managers;
using Assets.Script.UI.Tabs;

namespace Assets.Script.UI
{
    /// <summary>
    /// Onglet "Stats" — affiche les statistiques du joueur et ses ressources en temps réel.
    /// S'abonne à PlayerResources.OnResourceChanged pour un rafraîchissement automatique.
    /// GDD §15.4 — Panneau Statistiques Joueur (ressources temps réel, infos profil).
    /// </summary>
    public class PlayerStatsUI : MonoBehaviour
    {
        // ==========================
        //  Références UI — Ressources
        // ==========================

        [Header("Ressources")]
        [SerializeField] private TMP_Text fragmentsText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text materialsText;
        [SerializeField] private TMP_Text essenceText;
        [SerializeField] private TMP_Text tabooText;
        [SerializeField] private TMP_Text wishesText;

        // ==========================
        //  Références UI — Profil
        // ==========================

        [Header("Infos Profil")]
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text runCountText;
        [SerializeField] private TMP_Text playTimeText;
        [SerializeField] private TMP_Text creatureCountText;

        [Header("Tab Manager")]
        [SerializeField] private TabManager tabManager;
        [SerializeField] private int myTabIndex;

        // ==========================
        //  État
        // ==========================

        private PlayerResources _subscribedResources;
        private bool _isActiveTab;

        // ==========================
        //  Cycle de vie Unity
        // ==========================

        private void OnEnable()
        {
            SubscribeToResources();
        }

        private void OnDisable()
        {
            UnsubscribeFromResources();
        }

        private void Update()
        {
            if (_isActiveTab)
                UpdatePlayTime();
        }

        // ==========================
        //  Méthodes publiques
        // ==========================

        /// <summary>
        /// Injection des références UI (utilisé par les spawners programmatiques).
        /// </summary>
        public void Initialize(
            TMP_Text fragments, TMP_Text gold, TMP_Text materials,
            TMP_Text essence, TMP_Text taboo, TMP_Text wishes,
            TMP_Text playerName, TMP_Text runCount, TMP_Text playTime, TMP_Text creatureCount,
            TabManager manager, int tabIndex)
        {
            fragmentsText = fragments;
            goldText = gold;
            materialsText = materials;
            essenceText = essence;
            tabooText = taboo;
            wishesText = wishes;
            playerNameText = playerName;
            runCountText = runCount;
            playTimeText = playTime;
            creatureCountText = creatureCount;
            tabManager = manager;
            myTabIndex = tabIndex;

            if (tabManager != null)
                tabManager.OnTabChanged += OnTabChanged;

            SubscribeToResources();
            RefreshAll();
        }

        /// <summary>
        /// Rafraîchit l'ensemble de l'affichage (ressources + infos profil).
        /// </summary>
        public void RefreshAll()
        {
            RefreshResources();
            RefreshProfileInfo();
        }

        // ==========================
        //  Méthodes internes
        // ==========================

        /// <summary>Callback du TabManager : active/désactive le suivi temps réel.</summary>
        private void OnTabChanged(int index)
        {
            _isActiveTab = (index == myTabIndex);
            if (_isActiveTab)
                RefreshAll();
        }

        /// <summary>S'abonne à l'événement OnResourceChanged du profil actuel.</summary>
        private void SubscribeToResources()
        {
            var resources = GameManager.Instance?.CurrentProfile?.Resources;
            if (resources == null || resources == _subscribedResources) return;

            UnsubscribeFromResources();
            _subscribedResources = resources;
            _subscribedResources.OnResourceChanged += OnResourceChanged;
        }

        /// <summary>Se désabonne de l'événement OnResourceChanged.</summary>
        private void UnsubscribeFromResources()
        {
            if (_subscribedResources != null)
            {
                _subscribedResources.OnResourceChanged -= OnResourceChanged;
                _subscribedResources = null;
            }
        }

        /// <summary>Callback quand une ressource change de valeur. GDD §14.2.</summary>
        private void OnResourceChanged(ResourceType type, float oldValue, float newValue)
        {
            RefreshResourceText(type, newValue);
        }

        /// <summary>Rafraîchit l'affichage de toutes les ressources.</summary>
        private void RefreshResources()
        {
            var resources = GameManager.Instance?.CurrentProfile?.Resources;
            if (resources == null) return;

            RefreshResourceText(ResourceType.Fragments, resources.Fragments);
            RefreshResourceText(ResourceType.Gold, resources.Gold);
            RefreshResourceText(ResourceType.Materials, resources.Materials);
            RefreshResourceText(ResourceType.Essence, resources.Essence);
            RefreshResourceText(ResourceType.Taboo, resources.Taboo);
            RefreshResourceText(ResourceType.Wishes, resources.Wishes);
        }

        /// <summary>Met à jour le texte d'une ressource spécifique.</summary>
        private void RefreshResourceText(ResourceType type, float value)
        {
            switch (type)
            {
                case ResourceType.Fragments:
                    if (fragmentsText != null) fragmentsText.text = $"Fragments: {value:F0}";
                    break;
                case ResourceType.Gold:
                    if (goldText != null) goldText.text = $"Gold: {value:F0}";
                    break;
                case ResourceType.Materials:
                    if (materialsText != null) materialsText.text = $"Materials: {(int)value}";
                    break;
                case ResourceType.Essence:
                    if (essenceText != null) essenceText.text = $"Essence: {value:F1}";
                    break;
                case ResourceType.Taboo:
                    if (tabooText != null) tabooText.text = $"Taboo: {value:F1}";
                    break;
                case ResourceType.Wishes:
                    if (wishesText != null) wishesText.text = $"Wishes: {(int)value}";
                    break;
            }
        }

        /// <summary>Rafraîchit les informations de profil du joueur.</summary>
        private void RefreshProfileInfo()
        {
            var profile = GameManager.Instance?.CurrentProfile;
            if (profile == null) return;

            if (playerNameText != null)
                playerNameText.text = $"Joueur: {profile.PlayerName}";

            if (runCountText != null)
                runCountText.text = $"Run: {profile.RunCount}";

            if (creatureCountText != null)
                creatureCountText.text = $"Créatures: {profile.Inventory.OwnedCreatures.Count}";

            UpdatePlayTime();
        }

        /// <summary>Met à jour le texte du temps de jeu formaté.</summary>
        private void UpdatePlayTime()
        {
            var profile = GameManager.Instance?.CurrentProfile;
            if (profile == null || playTimeText == null) return;

            int totalSeconds = (int)profile.PlayTime;
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            playTimeText.text = $"Temps de jeu: {hours:D2}h {minutes:D2}m {seconds:D2}s";
        }
    }
}
