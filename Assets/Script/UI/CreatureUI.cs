using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Script.Creatures;

namespace Assets.Script.UI
{
    /// <summary>
    /// Panneau UI affichant les informations d'une créature active :
    /// nom, barre de PV, texte PV/Max, statut (Actif/Incapacité), bouton Nourrir.
    /// S'abonne aux événements du CreatureHealthTicker pour réagir à l'état Incapacitée.
    /// </summary>
    public class CreatureUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button feedButton;

        private Creature _creature;
        private CreatureHealthTicker _ticker;

        private void Start()
        {
            if (feedButton != null)
                feedButton.onClick.AddListener(OnFeedClicked);

            Refresh();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();

            if (feedButton != null)
                feedButton.onClick.RemoveListener(OnFeedClicked);
        }

        /// <summary>
        /// Lie ce panneau UI à une créature et à son ticker de santé.
        /// S'abonne à OnHealthChanged (rafraîchit PV/slider) et OnIncapacitated (statut).
        /// </summary>
        public void Bind(Creature creature, CreatureHealthTicker ticker)
        {
            UnsubscribeEvents();

            _creature = creature;
            _ticker = ticker;
            Debug.Log($"[CreatureUI] Bind creature={creature?.Name}, ticker={ticker != null}");

            if (_ticker != null)
            {
                _ticker.OnHealthChanged += Refresh;
                _ticker.OnIncapacitated += OnCreatureIncapacitated;
            }

            Refresh();
        }

        /// <summary>Désabonne tous les events pour éviter les fuites mémoire.</summary>
        private void UnsubscribeEvents()
        {
            if (_ticker != null)
            {
                _ticker.OnHealthChanged -= Refresh;
                _ticker.OnIncapacitated -= OnCreatureIncapacitated;
            }
        }

        /// <summary>
        /// Rafraîchit l'ensemble des champs UI (nom, PV, statut) à partir de la créature liée.
        /// Appelée au Start, après Bind, et après chaque clic sur Nourrir.
        /// </summary>
        public void Refresh()
        {
            if (_creature == null) return;

            Debug.Log($"[CreatureUI] Refresh : {_creature.CurrentHealth}/{_creature.MaxHealth} — sliderValue={(float)_creature.CurrentHealth / _creature.MaxHealth}");

            if (nameText != null)
                nameText.text = _creature.Name;

            if (healthSlider != null)
                healthSlider.value = (float)_creature.CurrentHealth / _creature.MaxHealth;

            if (healthText != null)
                healthText.text = $"{_creature.CurrentHealth} / {_creature.MaxHealth}";

            UpdateStatus();
        }

        /// <summary>
        /// Met à jour le texte de statut : "Actif" (vert) si PV > 0,
        /// "Incapacite - Nourrir pour soigner" (rouge) si PV == 0.
        /// </summary>
        private void UpdateStatus()
        {
            if (statusText == null) return;

            if (_creature.IsAlive)
            {
                statusText.text = "Actif";
                statusText.color = Color.green;
            }
            else
            {
                statusText.text = "Incapacite - Nourrir pour soigner";
                statusText.color = Color.red;
            }
        }

        /// <summary>Callback déclenché par le ticker quand la créature atteint 0 PV.</summary>
        private void OnCreatureIncapacitated()
        {
            UpdateStatus();
        }

        /// <summary>Callback du bouton Nourrir : soigne +10 PV puis rafraîchit l'UI.</summary>
        private void OnFeedClicked()
        {
            if (_creature == null) return;

            Debug.Log($"[CreatureUI] Nourrir clicked — Heal(10) before={_creature.CurrentHealth}");
            _creature.Heal(10f);
            Debug.Log($"[CreatureUI] Nourrir clicked — Heal(10) after={_creature.CurrentHealth}");
            Refresh();
        }
    }
}
