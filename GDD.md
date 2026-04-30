# Game Design Document — Idle Breeding Game
## Projet Akoia × Prozengan

**Version :** 0.1 — Conception initiale
**Date :** 26/04/2026
**Auteurs :** Somath, Akoia, Prozengan, Libebulle

---

## Table des matières

1. [Vision du Projet](#1-vision-du-projet)
2. [Core Loop](#2-core-loop)
3. [Système de Créatures](#3-système-de-créatures)
4. [Craft & Breeding](#4-craft--breeding)
5. [Expéditions & Zones](#5-expéditions--zones)
6. [Rebirth & Prestige](#6-rebirth--prestige)
7. [Économie & Ressources](#7-économie--ressources)
8. [Progression par Run](#8-progression-par-run)
9. [Stack Technique](#9-stack-technique)
10. [Place de l'IA](#10-place-de-lia)
11. [Questions Ouvertes](#11-questions-ouvertes)
12. [Direction Artistique & Narrative](#12-direction-artistique--narrative)
13. [Bâtiments](#13-bâtiments)

---

## 1. Vision du Projet

### Pitch
Un **jeu idle/incremental de breeding de créatures**, mêlant les mécaniques de Koletera II et Trimm, avec une inspiration Pokémon, dans un univers original.

### Références
- **Pokémon** — breeding, collection, types
- **Shapez** — formes/couleurs comme blocs de construction visuelle
- **Cookie Clicker / PokéClicker** — idle/click mécanique
- **Koletera II / Trimm** — systèmes propres au groupe

### Pilliers de Design
1. **Craft déterministe** pour apprendre, **breed aléatoire** pour optimiser
2. **Progression tangible** : chaque rebirth rend le joueur fondamentalement plus fort
3. **Exploration** : grilles de zones à découvrir case par case
4. **Visualisation claire** : hexagone de stats, prédiction de breeding

---

## 2. Core Loop

```
[Clic/Idle] → Ressources (Formes, Couleurs, Matériaux)
     │
     ├── CRAFT → Créature Gen-0
     │              │
     │              ├── DÉPLOIEMENT → Expédition (grille) → + Ressources
     │              │                                        + Squelettes rares
     │              │                                        + Nouvelles Couleurs
     │              │
     │              └── BREED avec une autre créature
     │                         │
     │                         ▼
     │                  Créature Gen-N (meilleure)
     │                         │
     │                         ├── DÉPLOIEMENT → Meilleurs rewards
     │                         └── BREED → Gen-N+1 ...
     │
     └── UPGRADE bâtiments / taux idle

     ═══════════════════════════════
     REBIRTH : reset → Essence → upgrades permanents
     ═══════════════════════════════
```

### Boucle minute par minute
1. Le joueur **clique** ou **attend** → accumule des ressources
2. Il **craft** ou **breed** une créature
3. Il **envoie** la créature explorer une zone (grille)
4. L'expédition rapporte des **rewards** (ressources, déblocages)
5. Il **améliore** ses bâtiments et ses créatures
6. Retour en 1 avec plus de puissance
:L: Définit les bâtiments : Quelle utilité pour le joueur ? Quel rôle dans le gameplay ? (ex: couveuse pour que nouvelle créature naisse plus vite ou plus de slots pour en faire couver plus ?)
---

## 3. Système de Créatures

### 3.1 Les 6 Stats (Hexagone)

Chaque créature possède exactement 6 stats, affichées en radar chart hexagonal.

```
              Volonté
             /       \
       Force           Chance
        |       ⬡       |
  Constitution        Intelligence
             \       /
              Agilité
```

| Stat | Abrév. | Rôle principal | Rôle en expédition |
|---|---|---|---|
| **Force (FOR)** | STR | Dégâts physiques, capacité de charge | Casse obstacles, cases combat |
| **Agilité (AGI)** | AGI | Vitesse, esquive | Réduit le timer d'exploration par case |
| **Intelligence (INT)** | INT | Dégâts magiques, efficacité spécialisée | Révèle cases adjacentes (+vision) |
| **Chance (CHA)** | LCK | Drops rares, crits, mutations | Meilleurs drops, cases cachées bonus |
| **Constitution (CON)** | CON | PV, endurance | Explore plusieurs cases d'affilée |
| **Volonté (VOL)** | WIL | Résistance, bonus idle | Continue d'explorer offline |

### 3.2 Composants d'une Créature

| Composant | Rôle |
|---|---|
| **Squelette** | Type de base (bipède, quadrupède, volant, serpentin…). Détermine les stats de base et l'apparence. |
| **Forme** | Modifie l'apparence et une stat principale (rond → +CON, triangle → +FOR, etc.) |
| **Couleur** | Modifie l'affinité élémentaire et une stat secondaire (rouge → Feu/+FOR, bleu → Eau/+INT, etc.) |
| **Génération** | Gen-0 = crafté, Gen-1+ = issu du breeding. Potentiel augmente avec la génération. |
:L: limitation du nombre de squelette à 6 pour correspondre aux stats ? ou aucun lien ? Possibilité de traits héirtés négatifs si inbreeding type consanguinité ?

### 3.3 Affinités Formes → Stats

| Forme | Stat boostée | Description |
|---|---|---|
| Rond | +CON | Solide, endurant |
| Triangle | +FOR | Pointu, offensif |
| Carré | +VOL | Stable, résistant |
| Ovale | +AGI | Fluide, rapide |
| Losange | +INT | Complexe, intelligent |
| Étoile | +CHA | Rare, chanceux (mutation uniquement) |

### 3.4 Affinités Couleurs → Stats / Éléments

| Couleur | Élément | Stat boostée |
|---|---|---|
| Rouge | Feu | +FOR |
| Bleu | Eau | +INT |
| Vert | Nature | +CON |
| Jaune | Foudre | +AGI |
| Violet | Ombre | +CHA |
| Orange | Terre | +VOL |
| Doré | Lumière | +CHA (mutation) |
| Arc-en-ciel | Chaos | +ALL (mutation rare) |

---

## 4. Craft & Breeding

### 4.1 Phase 1 — Craft (Gen-0)

```
Créature Gen-0 = Squelette + Forme + Couleur
```
:L: Comment est définie CG-0 ? Roulette parmis possibilité ou même base pour tous? Choix de squelette/forme + random couleur ? autre?
**Résultat 100% déterministe.** Le joueur sait exactement ce qu'il obtient.

**Formule :**
```
Stat[i] = base_squelette[i] + bonus_forme[i] + bonus_couleur[i]
```

### 4.2 Phase 2 — Breeding (Gen-1+)

Deux créatures parentes → un enfant avec traits hérités + potentiel de mutation.

**Formule :**
```
base_min   = min(parentA[stat], parentB[stat])
base_max   = max(parentA[stat], parentB[stat])
variance   = (base_max - base_min) * variance_factor   // variance_factor = 0.25 par défaut
floor      = base_min - variance + breeding_min_bonus (prestige)
ceil       = base_max + variance + breeding_max_bonus (prestige)
Stat_enfant = random(floor, ceil) + bonus_generation + mutation_roll
```

> **Règle de régression :** Un enfant **peut** avoir des stats inférieures à ses parents. Le `variance_factor` (par défaut 0.25) étend la fourchette sous le minimum parental. Cela signifie qu'un breeding n'est jamais garanti — le joueur doit faire plusieurs tentatives pour optimiser. Le `floor` ne peut jamais descendre en dessous de 1 (plancher absolu).

<!-- Traité par Agent Concepteur : ajout du variance_factor pour permettre la régression des stats en breeding, le floor peut descendre sous le min parental -->

### 4.3 Héritage des Composants

| Composant | Règle d'héritage |
|---|---|
| Squelette | 50/50 d'un parent ou de l'autre |
| Forme | Dominance : certaines formes sont dominantes sur d'autres |
| Couleur | Mélange possible (rouge + bleu → violet) ou héritage pur |
:L: loi de mendel sur les formes ? Tableau ?

### 4.4 Mutations

- **Débloquées après le 1er rebirth** (achat prestige)
- **Probabilité :** ~5% par breeding (augmentable via prestige/chance)
- **Effets :** Forme ou couleur inédite (Étoile, Doré, Arc-en-ciel), stat qui dépasse le plafond parental

### 4.5 Prédiction de Breeding (UI)

Affichage avant confirmation :
- Hexagone Parent A (bleu)
- Hexagone Parent B (rouge)
- Zone de prédiction enfant (vert semi-transparent = fourchette min/max)

Le joueur voit la **zone probable** et fait un choix éclairé.

---

## 5. Expéditions & Zones

### 5.1 Structure d'une Zone

Chaque zone est une **grille carrée** avec fog of war.

```
  ░ ?? │ ?? │ ?? │ ?? │ ?? ░
  ░────┼────┼────┼────┼────░
  ░ ?? │ ?? │ 👁 │ ?? │ ?? ░
  ░────┼────┼────┼────┼────░
  ░ ?? │ 👁 │ 🏠 │ 👁 │ ?? ░
  ░────┼────┼────┼────┼────░
  ░ ?? │ ?? │ 👁 │ ?? │ ?? ░
  ░────┼────┼────┼────┼────░
  ░ ?? │ ?? │ ?? │ ?? │ ?? ░

  🏠 = Case de départ (révélée)
  👁 = Cases adjacentes (visibles mais non explorées)
  ?? = Cases masquées (fog of war)
```

### 5.2 Types de Cases

| Type | Icône | Contenu |
|---|---|---|
| **Ressource** | 💎 | Drop de formes, couleurs, matériaux |
| **Combat** | ⚔️ | Affrontement → rewards si victoire (FOR, AGI importants) |
| **Événement** | ❓ | Choix narratif, bonus ou malus aléatoire |
| **Boss** | 💀 | Combat difficile, gros rewards, peut débloquer nouveau squelette |
| **Vide** | ⬜ | Rien de spécial, juste de la progression de carte |
| **Cachée** | 🌟 | Invisible, trouvée uniquement avec haute CHA |

### 5.3 Mécanique d'Exploration

1. Le joueur sélectionne une **créature** et une **case adjacente révélée**
2. Timer idle commence (durée basée sur la difficulté de la case - AGI de la créature)
3. À la fin du timer : case explorée, reward obtenu, cases adjacentes révélées
4. Une créature avec haute CON peut enchaîner plusieurs cases sans repos
5. Une créature avec haute VOL continue d'explorer même quand le joueur est offline
6. Une créature avec haute INT révèle +1 case supplémentaire autour
:L: possiblité d'avoir multi epxloration si le joueur a plusieurs créatures ?

### 5.4 Scaling des Zones

| Run | Nombre de zones | Taille grille | Notes |
|---|---|---|---|
| Run 1 | **8** zones | 5×5 → 7×7 | Tutoriel, 1 boss simple, basique |
| Run 2 | **12** zones | 6×6 → 9×9 | Nouvelles mécaniques de cases |
| Run 3 | **16** zones | 7×7 → 11×11 | Cases spéciales, événements rares |
| Run N | **8 + 4×(N-1)** zones | Scaling | Profondeur croissante |

### 5.5 Fin de Zone

Une zone est complétée quand :
- Le boss est vaincu, OU
- Toutes les cases sont explorées (si pas de boss)

Compléter une zone débloque la suivante.
:L: Comment est définie l'apparition des boss ?
---

## 6. Rebirth & Prestige

### 6.1 Déclenchement

Le joueur peut Rebirth à tout moment, mais le jeu le **suggère** quand la progression stagne (expéditions trop lentes, pas assez de stats pour avancer).

### 6.2 Monnaie de Prestige : Essence ✨

```
Essence gagnée = f(zones_explorées, créatures_créées, gen_max, temps_de_run)
```

Plus le run est long et poussé, plus on gagne d'Essence.

### 6.3 Ce qu'on RESET

- Toutes les créatures
- Toutes les ressources (formes, couleurs, matériaux)
- Progression des zones (fog of war remis)
- Niveaux de bâtiments
:L: niveau de bâtiments mais pas quels batiments construit ? j'ai du mal a comprendre le mécanisme autour des batiments
### 6.4 Ce qu'on GARDE

- Essence accumulée (cumulatif entre rebirth)
- Tous les upgrades achetés dans la boutique prestige
- Squelettes / formes / couleurs débloqués
- Statistiques et achievements

### 6.5 Boutique de Prestige

| Upgrade | Coût (Essence) | Effet | Stackable |
|---|---|---|---|
| **Breeding Min+** | 10 / 25 / 50 / 100 / ... | +1 au minimum de la fourchette de breeding par level | ✅ |
| **Breeding Max+** | 10 / 25 / 50 / 100 / ... | +1 au maximum de la fourchette de breeding par level | ✅ |
| **Stats Craft+** | 15 / 35 / 70 / 140 / ... | +1 à toutes les stats des créatures Gen-0 craftées | ✅ |
| **Slot Créature+** | 20 par slot | +1 slot de créature actif | ✅ |
| **Nouveau Squelette** | 50 / 100 / 200 / ... | Débloque un nouveau type de squelette | Limité |
| **Pack Formes** | 30 | Débloque 2 nouvelles formes | Limité |
| **Pack Couleurs** | 30 | Débloque 2 nouvelles couleurs | Limité |
| **Mutation Unlock** | 75 | Active le système de mutations en breeding | 1× |
| **Mutation Rate+** | 40 / 80 / 160 | +2% chance de mutation | ✅ |
| **Vision+** | 40 | +1 case de vision de base en expédition | ✅ |
| **Idle Speed+** | 20 / 45 / 90 / ... | Production idle de base augmentée | ✅ |

---

## 7. Économie & Ressources

### 7.1 Ressources de Base

| Ressource | Source | Utilisation |
|---|---|---|
| **Fragments** | Clic + idle | Monnaie de base, achats divers |
| **Formes** (items) | Expéditions, drops | Composant de craft |
| **Couleurs** (items) | Expéditions, drops | Composant de craft |
| **Matériaux** | Expéditions, mining | Upgrades de bâtiments |
| **Essence** ✨ | Rebirth uniquement | Boutique prestige |
:L: les squelettes ne sont pas des ressources ?

### 7.2 Coûts (première ébauche — à équilibrer)

| Action | Coût |
|---|---|
| Craft Gen-0 | 1 Squelette + 1 Forme + 1 Couleur + X Fragments |
| Breeding | 2 créatures parentes (immobilisées pendant le timer) + Y Fragments |
| Envoyer en expédition | Gratuit (mais la créature est occupée) |
| Upgrade bâtiment Lv. N | N × base_cost Matériaux |

### 7.3 Timers (première ébauche)

| Action | Durée de base |
|---|---|
| Exploration d'une case | 30s → 5min (selon difficulté) |
| Breeding | 2min → 10min (selon génération) |
| Repos d'une créature | 1min (réduit par CON) |

---

## 8. Progression par Run

### Run 1 — Le Squelette de Run (Tutoriel naturel)

| Étape | Action | Apprentissage |
|---|---|---|
| 1 | Clic → fragments | Mécanique idle/clic |
| 2 | Craft 1ère créature (1 squelette, 3 formes, 3 couleurs) | Système de craft |
| 3 | Envoyer en expédition | Système d'expédition + grille |
| 4 | Crafter 3-5 créatures | Explorer les combinaisons |
| 5 | Premier breeding | Héritage, prédiction hexagone |
| 6 | Explorer les 8 zones | Montée en puissance |
| 7 | Stagnation → le jeu propose le Rebirth | Mécanique prestige |

### Contenu Disponible par Run

| Élément | Run 1 | Run 2 | Run 3+ |
|---|---|---|---|
| Squelettes | 1 | 2 | 3+ |
| Formes | 3 (rond, triangle, carré) | 5 | 7+ |
| Couleurs | 3 (rouge, bleu, vert) | 5 | 7+ |
| Zones | 8 | 12 | 16+ |
| Slots créatures | 5 | 5 + achats | 5 + achats |
| Mutations | ❌ Non | ✅ Oui (si acheté) | ✅ Oui |
:L: donc il n'y a que 3 formes ? même chose pour les couleurs ? ou certaines formes et couleurs ne sont dispos que par random breeding?
---

## 9. Stack Technique

**Décision différée** — dépend du format final (web, desktop, mobile) et du style visuel (2D ou 3D).

| Option | Front | Back | Rendu | Hébergement | Plateformes |
|---|---|---|---|---|---|
| **A — Full Web** | React + TypeScript | Django (Python) | 2D Canvas/SVG | Automia / Vercel | Web |
| **B — Unity 2D** | Unity UI | C# | 2D Sprites/Tilemap | Desktop / Mobile / WebGL | Desktop, Mobile, Web |
| **C — Unity 3D** | Unity UI | C# | 3D | Desktop / Mobile / WebGL | Desktop, Mobile, Web |
| **D — Hybride** | React + TS | Django | Unity WebGL | Automia | Web |

### Critères de choix
- Si web only, prototypage rapide → Option A
- Si 2D avec déploiement multi-plateforme (desktop + mobile + web) → Option B
- Si 3D important → Option C
- Si web + moteur de jeu intégré → Option D (plus complexe)

> **Note :** Unity supporte nativement le rendu 2D (sprites, tilemaps, animations 2D) avec les mêmes avantages multi-plateforme que le 3D. Pour un jeu idle avec hexagones de stats et grilles d'exploration, l'option Unity 2D (B) offre un bon compromis entre richesse visuelle et portabilité.

<!-- Traité par Agent Concepteur : correction du tableau stack — Unity supporte aussi le 2D, ajout de l'option B Unity 2D et colonne Plateformes -->

---

## 10. Place de l'IA

| Phase | IA ? | Justification |
|---|---|---|
| Conception / GDD | ✅ Oui | Accélérateur pour structurer et itérer |
| Code / Développement | ✅ Oui | Génération, refacto, tests |
| Illustration in-game | ❌ Non | Art fait main préféré |
| Équilibrage / Simulation | ❓ À discuter | Simuler des runs pour tester les courbes |

---

## 11. Questions Ouvertes

- [ ] **Lore** : Quel est le monde ? Y a-t-il une histoire ?
- [ ] **Nom du jeu**
- [ ] **Multijoueur** : Solo only ou composante sociale (échanges, classements) ?
- [ ] **Monétisation** : Free-to-play ? Premium ? Cosmétiques ?
- [ ] **Plateforme cible** : Web ? Desktop ? Mobile ? Les trois ?
- [ ] **Nombre total de squelettes** prévu à terme
- [ ] **Système d'évolution** : Les créatures peuvent-elles évoluer (en plus du breeding) ?
- [ ] **PvP / Arène** : Les créatures peuvent-elles s'affronter entre joueurs ?
- [ ] **Saisonnalité** : Événements temporaires, créatures limitées ?


## 12. Direction Artistique & Narrative

### 12.1 Esthétique

**Contraste Kawaii × Dark.** Le jeu adopte une direction artistique à deux faces :

- **Visuels Kawaii** — Créatures mignonnes, couleurs pastel, animations douces. Le joueur s'attache à ses créatures.
- **Univers Dark** — Le monde sous-jacent est sombre et cruel. La survie prime sur la morale. L'exploitation des créatures est une mécanique centrale, pas un accident.

> **Inspiration principale :** *Cult of the Lamb* — un visuel adorable qui cache un gameplay brutal et moralement ambigu.

### 12.2 Narrative : De l'Arche à l'Usine

La base du joueur évolue visuellement au fil de la progression, reflétant la corruption progressive du joueur :

| Phase | Apparence de la base | Ambiance |
|---|---|---|
| **Début (Run 1)** | Arche de Noé — refuge pastoral, bois, nature | Protecteur, bienveillant |
| **Mid-game (Run 2-3)** | Ferme organisée — enclos, structures fonctionnelles | Pragmatique, utilitaire |
| **Late-game (Run 4+)** | Usine agro-alimentaire — tapis roulants, cuves, cheminées | Industriel, oppressant |
| **End-game** | Complexe dystopique — les créatures faibles deviennent des ressources pour les plus fortes | Dark, cynique |

**Mécanique narrative clé :** Les créatures faibles ou obsolètes peuvent être **recyclées** (transformées en nourriture, matériaux, ou boosts) pour alimenter les plus fortes. Chaque palier de prestige ou amélioration majeure transforme visuellement une partie de la base.
:L: si le joueur refuse de recycler les vieux mais accepte de les laisser vivre un span de vie, est-ce qu'une autre voie est possible en terme d'évol ? mais plus dure d'un pdv gameplay ?

> **Exemples de transformation visuelle :**
>
> - La **Pension** (départ : pâturage fleuri) → devient un **Élevage Intensif** (enclos serrés, lumières artificielles)
> - Le **Laboratoire** (départ : tour de mage mystique) → devient une **Usine de Mutations** (cuves, tubes, créatures en stase)
> - Le **Temple** (départ : autel sacré) → devient un **Abattoir Rituel** (sacrifices industrialisés)

<!-- Traité par Agent Concepteur : intégration de la direction artistique Kawaii×Dark et de la progression narrative Arche→Usine, inspirée de Cult of the Lamb -->

---

## 13. Bâtiments
La base du joueur est composée de bâtiments upgradables. Chaque bâtiment a une **fonction gameplay** et une **évolution visuelle** liée à la progression narrative (voir section 12.2).

### 13.1 Liste des Bâtiments

| Bâtiment | Fonction principale | Améliorations | Évolution visuelle |
|---|---|---|---|
| **Pension / Ranch** | Nourrit et soigne les créatures. Régénère l'endurance après expéditions. Augmente les slots de créatures actives. | Capacité d'accueil, vitesse de repos, qualité de nourriture | Pâturage → Ferme → Élevage intensif |
| **Maison du Héros** | Hub central. Contient les coffres de stockage et la table de craft. Accès aux menus principaux. | Taille de stockage, slots de craft simultanés | Cabane → Maison → Manoir |
| **Forge** | Craft et amélioration d'équipements/composants. Transforme les matériaux bruts en composants raffinés. | Recettes débloquées, vitesse de craft, qualité des outputs | Enclume → Atelier → Fonderie |
| **Temple des Sacrifices** | Permet de **recycler** les créatures faibles en ressources (fragments, essence mineure, nourriture). Déblocage de bonus temporaires via offrandes. | Types de sacrifices, rendement, bonus rituel | Autel de pierre → Temple → Abattoir rituel |
| **Laboratoire de Breeding** | Centre de breeding et d'expérimentation. Affiche la prédiction hexagonale. Permet les mutations (si débloquées). | Slots de breeding, réduction timer, bonus mutation | Tour de mage → Labo → Usine de mutations |
| **Mairie** | Gestion de la base : extension du territoire, placement des bâtiments, déblocage de nouvelles zones de construction. | Territoire max, nombre de bâtiments, vitesse de construction | Panneau → Bureau → Hôtel de ville |

### 13.2 Système d'Upgrade des Bâtiments

Chaque bâtiment a **N niveaux** (à équilibrer). Le coût augmente par palier :

```
Coût_upgrade(Lv) = base_cost × Lv × multiplicateur
```

| Ressource utilisée | Bâtiments concernés |
|---|---|
| **Fragments** | Tous (coût de base) |
| **Matériaux** | Forge, Maison, Mairie |
| **Essence de créature** (recyclage) | Temple, Laboratoire |

### 13.3 Déblocage des Bâtiments

| Bâtiment | Disponible | Condition |
|---|---|---|
| Maison du Héros | Run 1, début | Toujours disponible |
| Pension / Ranch | Run 1, après 1er craft | Crafter sa première créature |
| Forge | Run 1, après 1ère expédition | Compléter la zone 1 |
| Temple des Sacrifices | Run 2+ | Achat prestige (Essence) |
| Laboratoire de Breeding | Run 1, après 1er breeding | Réussir un premier breeding |
| Mairie | Run 2+ | Achat prestige (Essence) |

<!-- Traité par Agent Concepteur : création de la section Bâtiments complète avec liste, upgrades, déblocage et lien vers la progression narrative -->

# 14. Core engine
## 14.1 Ressources 

Dans cette partie nous allons détaillé les différentes ressources du jeu et leur utilisation.

### Gold
:S: Détailler les mécaniques d'obtention, les sources de production, les caps éventuels et les sinks de gold.

### Taboo
:S: Détailler les mécaniques de gain de Taboo, les facteurs de multiplicateurs, les décisions morales impactant le Taboo, et les sinks spécifiques.

### Wish
:S: Détailler la mécanique des vœux, les conditions de déblocage, les types de vœux possibles, et l'impact sur le meta-game.

Eventuellement dans une future mise à jour, cette ressource permettra de faire des vœux pour obtenir des objets spéciaux ou de recommencer avec des créatures avec des traits particulier. 

Cette ressource peut être utiliser pour augmenter l'un des 7 arbre de compétences correspondant aux 7 péchés capitaux
## 14.2 Augmentation

Ebauche de l'arbre de compétence pour les augmentations lié aux taboo
### Colère
:S: Détailler l'arbre de compétences Colère : liste des nœuds, coûts en Taboo, effets numériques, synergies avec les créatures de combat.

### Cupidité
:S: Détailler l'arbre Cupidité : bonus de gold, multiplicateurs, passifs actifs, et seuils de déblocage.

### Gourmandise
:S: Détailler l'arbre Gourmandise : cuisine, recettes, buffs nourriture, chaînes de production.

### Orgueil
:S: Détailler l'arbre Orgueil : renommée, visiteurs, bonus de prestige, et capacités de show-off.

### Envie
:S: Détailler l'arbre Envie : taux de reproduction, qualité des enfants, mécaniques de breeding avancées.

### Paresse
:S: Détailler l'arbre Paresse : vitesse AFK, bonus offline, automatisation, et réduction de micro-gestion.
### Luxure

:S: Détailler l'arbre Luxure : charme des créatures, bonus d'attraction, événements spéciaux liés à la reproduction ou au spectacle.

---

*Ce document est vivant. Il sera mis à jour au fur et à mesure de la conception.*
