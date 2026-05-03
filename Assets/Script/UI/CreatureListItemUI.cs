using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Script.Creatures;

namespace Assets.Script.UI
{
    /// <summary>
    /// Élément de liste représentant une créature dans l'onglet Créatures.
    /// Affiche les infos résumées et permet la sélection.
    /// GDD §15.2 — Liste de Créatures (affichage, tri, filtres futurs).
    /// </summary>
    public class CreatureListItemUI : MonoBehaviour
    {
        // ==========================
        //  Références UI
        // ==========================

        [Header("Textes")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text generationText;
        [SerializeField] private TMP_Text componentsText;
        [SerializeField] private TMP_Text totalStatsText;
        [SerializeField] private TMP_Text statusText;

        [Header("Sélection")]
        [SerializeField] private Button selectButton;
        [SerializeField] private Image backgroundImage;

        // ==========================
        //  État
        // ==========================

        private Creature _creature;

        /// <summary>
        /// Événement déclenché quand l'utilisateur sélectionne cette créature.
        /// Paramètre : la créature liée à cet item.
        /// </summary>
        public event Action<Creature> OnSelected;

        // ==========================
        //  Cycle de vie Unity
        // ==========================

        private void Start()
        {
            if (selectButton != null)
                selectButton.onClick.AddListener(OnSelectClicked);
        }

        private void OnDestroy()
        {
            if (selectButton != null)
                selectButton.onClick.RemoveListener(OnSelectClicked);
        }

        // ==========================
        //  Méthodes publiques
        // ==========================

        /// <summary>
        /// Lie cet item UI à une créature et affiche ses informations résumées.
        /// GDD §3.1 — Les 6 Stats, §3.2 — Composants d'une Créature.
        /// </summary>
        public void Bind(Creature creature)
        {
            _creature = creature;
            Refresh();
        }

        /// <summary>
        /// Rafraîchit l'affichage à partir des données de la créature liée.
        /// </summary>
        public void Refresh()
        {
            if (_creature == null) return;

            if (nameText != null)
                nameText.text = _creature.Name;

            if (generationText != null)
                generationText.text = $"Gen-{_creature.Generation}";

            if (componentsText != null)
                componentsText.text = $"{_creature.Skeleton} | {_creature.Shape} | {_creature.Color}";

            if (totalStatsText != null)
                totalStatsText.text = $"Total: {_creature.TotalStats:F1}";

            if (statusText != null)
            {
                if (_creature.IsAlive)
                {
                    statusText.text = "Actif";
                    statusText.color = UnityEngine.Color.green;
                }
                else
                {
                    statusText.text = "Incapacité";
                    statusText.color = UnityEngine.Color.red;
                }
            }
        }

        /// <summary>
        /// Retourne la créature liée à cet item.
        /// </summary>
        public Creature GetCreature() => _creature;

        /// <summary>
        /// Injection manuelle des références UI (utilisé par les spawners programmatiques).
        /// </summary>
        public void Initialize(TMP_Text name, TMP_Text gen, TMP_Text comp,
            TMP_Text stats, TMP_Text status, Button select, Image bg)
        {
            nameText = name;
            generationText = gen;
            componentsText = comp;
            totalStatsText = stats;
            statusText = status;
            selectButton = select;
            backgroundImage = bg;

            if (selectButton != null)
                selectButton.onClick.AddListener(OnSelectClicked);
        }

        /// <summary>
        /// Met en surbrillance cet item (sélection visuelle).
        /// </summary>
        public void SetHighlight(bool highlighted)
        {
            if (backgroundImage != null)
                backgroundImage.color = highlighted
                    ? new Color(0.3f, 0.5f, 0.7f, 0.8f)
                    : new Color(0.15f, 0.15f, 0.15f, 0.6f);
        }

        // ==========================
        //  Méthodes internes
        // ==========================

        private void OnSelectClicked()
        {
            OnSelected?.Invoke(_creature);
        }
    }
}
