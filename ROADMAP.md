# Roadmap IdleOne — v0.1 → v1.0

Ce document trace les étapes d'implémentation progressive du jeu, du prototype breeding minimal jusqu'à la version complète avec tous les systèmes documentés dans le [GDD](GDD.md).

---

## État Actuel (Baseline)

- **Modèle créature** : `Creature.cs` avec 6 stats, composants en `string` (Skeleton, Shape, Color), stats dérivées (PV, expédition).
- **Prototype UI** : `CreatureUI.cs` + `CreatureHealthTicker.cs` + `DemoCreatureSpawner.cs` — affichage PV, barre de santé, bouton "Nourrir", statut Actif/Incapacité.
- **Non implémenté** : craft, breeding, expéditions, économie, prestige, bâtiments, mutations.

---

## v0.1 — Écran Breeding Minimal

Objectif : un écran fonctionnel avec 3 panneaux et un résultat de breeding.

- **UI** : 3 panneaux — Parent 1 (gauche), Parent 2 (droite), Résultat (centre).
- **Data** : mock de 2 créatures avec affichage nom + 6 stats textuelles.
- **Mécanique** : bouton "Breed" générant un enfant Gen-1 via formule simplifiée (moyenne des parents + variance aléatoire).
- **Visualisation** : stats affichées en texte (pas d'hexagone graphique).

---

## v0.2 — Formules de Breeding & Craft Gen-0

Objectif : implémenter les formules du GDD et le système de craft déterministe.

- **Breeding** : formule complète GDD §4.2 avec `variance_factor`, floor/ceil, `breeding_min/max_bonus`.
- **Craft Gen-0** : `Squelette + Forme + Couleur → Créature` (formule déterministe §4.1).
- **Refactor** : classes `Skeleton`, `Shape`, `Color` (remplacer les `string` du modèle actuel).
- **UI** : écran de sélection des composants pour le craft.

---

## v0.3 — Visualisation Créature & Hexagone Stats

Objectif : rendre les stats visuellement lisibles via un radar chart.

- **Radar chart** : hexagone affichant les 6 stats (Force, Agilité, Intelligence, Chance, Constitution, Volonté).
- **Carte créature** : affichage nom, PV max, statut, génération, composants.
- **Refactor** : `Creature.cs` utilisant les classes composants de v0.2.

---

## v0.4 — Expéditions & Zones

Objectif : envoyer une créature explorer une grille et rapporter des ressources.

- **Grille** : zones carrées avec fog of war (§5.1).
- **Cases** : Ressource, Combat, Événement, Boss, Vide, Cachée (§5.2).
- **Exploration** : sélection créature + case adjacente, timer idle, récompenses.
- **Stats d'expédition** : vision (INT), vitesse (AGI), endurance (CON), offline (VOL), drops (CHA), combat (FOR).
- **Progression** : compléter une zone débloque la suivante.

---

## v0.5 — Économie de Base & Bâtiments

Objectif : clic/idle, ressources, et premiers bâtiments fonctionnels.

- **Ressources** : Fragments (clic/idle), Formes, Couleurs, Matériaux (expéditions).
- **Bâtiments Run 1** : Maison du Héros (hub), Pension/Ranch (soin/repos), Forge (craft/composants), Laboratoire de Breeding.
- **Upgrades** : coût matériaux, effets de niveau (§13.2).
- **Timers** : exploration, breeding, repos.

---

## v0.6 — Rebirth & Prestige (Essence)

Objectif : introduire la boucle de reset/progression permanente.

- **Essence** : calcul basé sur zones explorées, créatures créées, gen max, temps de run.
- **Reset** : créatures, ressources, zones, niveaux bâtiments.
- **Garde** : Essence cumulée, upgrades prestige, squelettes/formes/couleurs débloqués, achievements.
- **Boutique prestige** : Breeding Min+/Max+, Stats Craft+, Slot Créature+, Mutation Unlock, Idle Speed+, etc. (§6.5).

---

## v0.7 — Mutations & Prédiction de Breeding

Objectif : rendre le breeding plus profond avec des surprises et une UI informative.

- **Mutations** : ~5% par breeding, débloqué après 1er rebirth. Formes/couleurs rares (Étoile, Doré, Arc-en-ciel).
- **Régression** : un enfant peut être moins bon que ses parents (§4.2).
- **Prédiction UI** : hexagone Parent A (bleu), Parent B (rouge), zone probable enfant (vert semi-transparent = fourchette min/max).

---

## v0.8 — Ressources Avancées (Gold, Taboo, Wish)

Objectif : ajouter les ressources de late-game et leurs mécaniques.

- **Gold** : vente de créatures Gen-2+, cases Trésor, bourse (Run 3+). Sinks : accélérations, upgrades avancés, cosmétiques.
- **Taboo** : sacrifices au Temple, décisions sombres en expédition, recycling massif. Sinks : arbres de péchés, offrandes, vœux corrompus.
- **Wish** : débloqué Run 3+, gagné par milestones. 4 types (Créature, Objet, Sagesse, Renaissance) + versions corrompues via Taboo.
- **Bâtiment** : Temple des Sacrifices (Run 2+).

---

## v0.9 — Les 7 Arbres de Péchés

Objectif : système d'augmentation profond alimenté par le Taboo.

- **7 arbres** : Colère, Cupidité, Gourmandise, Orgueil, Envie, Paresse, Luxure.
- **Structure** : 10 nœuds / arbre, 3 tiers (Fondation/Spécialisation/Maîtrise), coûts en Taboo.
- **Branches mutuellement exclusives** : certains nœuds offrent 2-3 choix (ex: Carnage vs Fureur).
- **Synergies** : effets cumulatifs entre arbres (ex: Colère + Cupidité = +Gold sur Boss).

---

## v1.0 — Version Complète

Objectif : jeu jouable de bout en bout, équilibré et poli.

- **Tous les systèmes** intégrés : craft, breeding, expéditions, économie, prestige, bâtiments, mutations, 7 péchés, Gold, Taboo, Wish.
- **Progression narrative visuelle** : base qui évolole d'Arche → Ferme → Usine → Complexe dystopique (§12.2).
- **Équilibrage** : coûts, timers, courbes de progression, diminishing returns.
- **Polish** : UI/UX, animations, feedback, achievements, statistiques globales.
- **Documentation** : builds, déploiement, guide de démarrage.

---

## Dépendances Entre Versions

```text
v0.1 (UI breeding) ──► v0.2 (formules + craft) ──► v0.3 (hexagone + refactor)
     │                                                   │
     └───────────────────────────────────────────────────┘
                           │
                           ▼
                    v0.4 (expéditions) ──► v0.5 (économie + bâtiments)
                                                   │
                                                   ▼
                                           v0.6 (prestige Essence)
                                                   │
                                                   ▼
                                           v0.7 (mutations)
                                                   │
                                                   ▼
                                           v0.8 (Gold/Taboo/Wish)
                                                   │
                                                   ▼
                                           v0.9 (7 péchés)
                                                   │
                                                   ▼
                                           v1.0 (complète)
```

---

Dernière mise à jour : Mai 2026
