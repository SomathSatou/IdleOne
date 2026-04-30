using UnityEngine;
using Assets.Script.Creatures;
using Assets.Script.UI;

namespace Assets.Script
{
    /// <summary>
    /// Spawner de test pour le prototype UI créature.
    /// Crée une créature mockée (CON=5 → 50 PV max), initialise le ticker de dégâts au temps,
    /// et bind l'ensemble au panneau CreatureUI pour la démo.
    /// </summary>
    public class DemoCreatureSpawner : MonoBehaviour
    {
        [SerializeField] private CreatureUI creatureUI;

        /// <summary>
        /// Au démarrage : instancie une Creature en code, lance le ticker (1 PV/s),
        /// et relie le tout au panneau UI pour affichage/interaction.
        /// </summary>
        private void Start()
        {
            var creature = new Creature
            {
                Name = "DemoCreature",
                Constitution = 5,
                Strength = 3,
                Agility = 4,
                Intelligence = 3,
                Luck = 2,
                Willpower = 3
            };

            creature.CurrentHealth = creature.MaxHealth;

            var ticker = gameObject.AddComponent<CreatureHealthTicker>();
            ticker.Initialize(creature);

            if (creatureUI != null)
                creatureUI.Bind(creature, ticker);
        }
    }
}
