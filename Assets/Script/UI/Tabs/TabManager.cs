using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.UI.Tabs
{
    /// <summary>
    /// Définition d'un onglet : nom affiché, contenu associé et bouton de sélection.
    /// Utilisé par <see cref="TabManager"/> pour gérer la navigation entre onglets.
    /// GDD §15.1 — Architecture d'Onglets.
    /// </summary>
    [Serializable]
    public class TabDefinition
    {
        /// <summary>Nom affiché de l'onglet.</summary>
        public string Name;

        /// <summary>GameObject contenant le contenu de l'onglet (activé/désactivé).</summary>
        public GameObject Content;

        /// <summary>Bouton UI permettant de sélectionner cet onglet.</summary>
        public Button TabButton;
    }

    /// <summary>
    /// Système d'onglets générique et réutilisable.
    /// Gère la navigation entre plusieurs panneaux via des boutons,
    /// avec highlight visuel de l'onglet actif.
    /// GDD §15.1 — Architecture d'Onglets (TabManager, principes de navigation).
    /// </summary>
    public class TabManager : MonoBehaviour
    {
        // ==========================
        //  Configuration
        // ==========================

        /// <summary>Liste des onglets gérés par ce TabManager.</summary>
        [SerializeField] private List<TabDefinition> tabs = new List<TabDefinition>();

        /// <summary>Couleur du bouton de l'onglet actif.</summary>
        [SerializeField] private Color activeColor = new Color(0.3f, 0.6f, 0.9f, 1f);

        /// <summary>Couleur du bouton des onglets inactifs.</summary>
        [SerializeField] private Color inactiveColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        // ==========================
        //  État
        // ==========================

        /// <summary>Index de l'onglet actuellement actif.</summary>
        public int CurrentTabIndex { get; private set; } = -1;

        // ==========================
        //  Events
        // ==========================

        /// <summary>
        /// Événement déclenché quand l'onglet actif change.
        /// Paramètre : index du nouvel onglet actif.
        /// L'UI peut s'abonner pour réagir aux changements d'onglet.
        /// </summary>
        public event Action<int> OnTabChanged;

        // ==========================
        //  Propriétés publiques
        // ==========================

        /// <summary>Nombre total d'onglets enregistrés.</summary>
        public int TabCount => tabs.Count;

        // ==========================
        //  Cycle de vie Unity
        // ==========================

        private void Start()
        {
            RegisterButtonListeners();

            if (tabs.Count > 0)
                SwitchTab(0);
        }

        private void OnDestroy()
        {
            UnregisterButtonListeners();
        }

        // ==========================
        //  Méthodes publiques
        // ==========================

        /// <summary>
        /// Active l'onglet à l'index donné, désactive tous les autres.
        /// Met à jour le highlight visuel des boutons et déclenche l'événement OnTabChanged.
        /// GDD §15.1 — Principes de navigation.
        /// </summary>
        public void SwitchTab(int index)
        {
            if (index < 0 || index >= tabs.Count) return;

            for (int i = 0; i < tabs.Count; i++)
            {
                bool isActive = (i == index);

                if (tabs[i].Content != null)
                    tabs[i].Content.SetActive(isActive);

                UpdateButtonColor(tabs[i].TabButton, isActive);
            }

            CurrentTabIndex = index;
            OnTabChanged?.Invoke(index);
        }

        /// <summary>
        /// Ajoute un onglet dynamiquement au TabManager.
        /// Utilisé par les spawners programmatiques pour construire les onglets en runtime.
        /// </summary>
        public void AddTab(TabDefinition tab)
        {
            if (tab == null) return;

            tabs.Add(tab);
            int index = tabs.Count - 1;

            if (tab.TabButton != null)
                tab.TabButton.onClick.AddListener(() => SwitchTab(index));

            // Désactiver le contenu par défaut (sauf si c'est le seul onglet)
            if (tab.Content != null)
                tab.Content.SetActive(false);
        }

        /// <summary>
        /// Initialise l'affichage après avoir ajouté tous les onglets dynamiquement.
        /// Active le premier onglet par défaut.
        /// </summary>
        public void InitializeAfterDynamicSetup()
        {
            if (tabs.Count > 0)
                SwitchTab(0);
        }

        /// <summary>
        /// Retourne la définition de l'onglet à l'index donné, ou null si hors limites.
        /// </summary>
        public TabDefinition GetTab(int index)
        {
            if (index < 0 || index >= tabs.Count) return null;
            return tabs[index];
        }

        // ==========================
        //  Méthodes internes
        // ==========================

        /// <summary>Enregistre les listeners onClick sur tous les boutons d'onglets sérialisés.</summary>
        private void RegisterButtonListeners()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                int capturedIndex = i;
                if (tabs[i].TabButton != null)
                    tabs[i].TabButton.onClick.AddListener(() => SwitchTab(capturedIndex));
            }
        }

        /// <summary>Désenregistre les listeners onClick pour éviter les fuites mémoire.</summary>
        private void UnregisterButtonListeners()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].TabButton != null)
                    tabs[i].TabButton.onClick.RemoveAllListeners();
            }
        }

        /// <summary>Met à jour la couleur d'un bouton d'onglet selon son état actif/inactif.</summary>
        private void UpdateButtonColor(Button button, bool isActive)
        {
            if (button == null) return;

            var image = button.GetComponent<Image>();
            if (image != null)
                image.color = isActive ? activeColor : inactiveColor;
        }
    }
}
