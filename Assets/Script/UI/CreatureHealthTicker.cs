using System;
using System.Collections;
using UnityEngine;
using Assets.Script.Creatures;

namespace Assets.Script.UI
{
    /// <summary>
    /// Applique une dégradation continue de santé sur une créature (1 PV par seconde).
    /// Déclenche un événement quand la créature atteint 0 PV (état Incapacitée).
    /// </summary>
    public class CreatureHealthTicker : MonoBehaviour
    {
        private Creature _creature;
        private bool _wasIncapacitated;
        private Coroutine _tickCoroutine;

        /// <summary>Event déclenché à chaque tick où les PV changent (perte de 1 PV).</summary>
        public event Action OnHealthChanged;

        /// <summary>Event déclenché une seule fois quand la créature passe à 0 PV.</summary>
        public event Action OnIncapacitated;

        /// <summary>
        /// Lie le ticker à une créature cible et démarre la coroutine de tick.
        /// </summary>
        public void Initialize(Creature target)
        {
            _creature = target;
            _wasIncapacitated = false;
            Debug.Log($"[CreatureHealthTicker] Initialize : {_creature.Name} — PV={_creature.CurrentHealth}/{_creature.MaxHealth}");

            if (_tickCoroutine != null)
                StopCoroutine(_tickCoroutine);

            _tickCoroutine = StartCoroutine(TickRoutine());
        }

        /// <summary>Retourne la créature actuellement suivie.</summary>
        public Creature GetCreature() => _creature;

        private void OnDestroy()
        {
            if (_tickCoroutine != null)
                StopCoroutine(_tickCoroutine);
        }

        /// <summary>
        /// Coroutine infinie : inflige TakeDamage(1f) toutes les secondes tant que la créature est en vie.
        /// Déclenche OnIncapacitated au premier tick où CurrentHealth atteint 0.
        /// </summary>
        private IEnumerator TickRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                Debug.Log($"[CreatureHealthTicker] Tick — IsAlive={_creature?.IsAlive}, PV={_creature?.CurrentHealth}/{_creature?.MaxHealth}");

                if (_creature == null)
                    continue;

                if (_creature.IsAlive)
                {
                    _creature.TakeDamage(1f);
                    Debug.Log($"[CreatureHealthTicker] TakeDamage(1) → PV={_creature.CurrentHealth}/{_creature.MaxHealth}");
                    OnHealthChanged?.Invoke();

                    if (!_creature.IsAlive && !_wasIncapacitated)
                    {
                        _wasIncapacitated = true;
                        Debug.Log($"[CreatureHealthTicker] INCAPACITATED event invoked");
                        OnIncapacitated?.Invoke();
                    }
                }
                else
                {
                    if (_wasIncapacitated)
                    {
                        // La créature est déjà en état incapacité, rien à faire
                    }
                }
            }
        }
    }
}
