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
5. [Expéditions, Zones & Monde Top-Down](#5-expéditions-zones--monde-top-down)
6. [Rebirth & Prestige](#6-rebirth--prestige)
7. [Économie, Ressources & Bâtiments](#7-économie-ressources--bâtiments)
8. [Progression par Run](#8-progression-par-run)
9. [Stack Technique](#9-stack-technique)
10. [Place de l'IA](#10-place-de-lia)
11. [Questions Ouvertes](#11-questions-ouvertes)
12. [Direction Artistique & Narrative](#12-direction-artistique--narrative)
13. [Les 7 Arbres de Péchés](#13-les-7-arbres-de-péchés)
14. [Système de Profil Joueur](#14-système-de-profil-joueur)
15. [Interface Utilisateur (UI)](#15-interface-utilisateur-ui)
16. [Système de Sauvegarde](#16-système-de-sauvegarde)
17. [Paramètres du Jeu](#17-paramètres-du-jeu)
18. [Navigation & Menus](#18-navigation--menus)

---

## 1. Vision du Projet

### Pitch
Un **jeu idle/incremental de breeding de créatures**, mêlant les mécaniques de Koletera II et Trimm, avec une inspiration Pokémon, dans un univers original.

### Références
- **Pokémon** — breeding, collection, types
- **Shapez** — formes/couleurs comme blocs de construction visuelle
- **Cookie Clicker / PokéClicker** — idle/click mécanique
- **Koltera II / Trimps** — systèmes propres au groupe
- **Graveyard Keeper / Stardew Valley** - univers et DA

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

<!-- Traité par Agent Concepteur : Les bâtiments sont définis dans la section 7.5.1 avec leur fonction gameplay, améliorations et évolution visuelle. Voir §7.5 pour le détail complet. -->

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

> **État Incapacité** : Une créature dont les PV atteignent 0 devient **Incapacitée** — elle ne peut plus explorer ni breeder jusqu'à être soignée (nourrie).

### 3.2 Composants d'une Créature

| Composant | Rôle |
|---|---|
| **Squelette** | Type de base (bipède, quadrupède, volant, serpentin…). Détermine les stats de base et l'apparence. |
| **Forme** | Modifie l'apparence et une stat principale (rond → +CON, triangle → +FOR, etc.) |
| **Couleur** | Modifie l'affinité élémentaire et une stat secondaire (rouge → Feu/+FOR, bleu → Eau/+INT, etc.) |
| **Génération** | Gen-0 = crafté, Gen-1+ = issu du breeding. Potentiel augmente avec la génération. |
<!-- Traité par Agent Concepteur : Aucun lien direct entre le nombre de squelettes et les 6 stats. Les squelettes déterminent les stats de base mais il peut y en avoir plus de 6 types (débloqués via prestige). L'inbreeding est traité dans l'arbre Envie (E-7 Lignée Maudite) qui réduit le malus de consanguinité. -->

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
| Vert | Nature | +VOL |
| Jaune | Foudre | +AGI |
| Violet | Ombre | +CHA |
| Orange | Terre | +CON |
| Doré | Lumière | +CHA (mutation) |
| Arc-en-ciel | Chaos | +ALL (mutation rare) |

:Z: réfléchis au limites , combien de types de formes ect pour cadrer l'espace des créatures posibles

:Z: Couleurs -> essence élémentaire

### 3.5 Bestiaires

Abraham va participer a la partie création du bestiaires

L'idée est qu'il nous propose une premières ébauches des 24 créatures de génération 1, sachant que pour l'instant on partirais sur 5 slimes de couleurs différentes en générations 0. 

en résumé ça donne :

- Gen 0 : 5 slimes
- Gen 1 : 24 créatures qui commence a ressembler a des formes
- Gen 2 : 48 limité les créatures par un ensemble de recettes specifique
- Gen 3 : 96 même principe et éventuellement plus recettes pour des légendaire ou spécifique a des péchés capitaux

---

## 4. Craft & Breeding

### 4.1 Phase 1 — Craft (Gen-0)

```
Créature Gen-0 = Squelette + Forme + Couleur
```
<!-- Traité par Agent Concepteur : Le craft Gen-0 est 100% déterministe. Le joueur choisit librement le Squelette (type de base débloqué), la Forme (parmi celles possédées) et la Couleur (parmi celles possédées). Aucun élément aléatoire. Seules les Formes et Couleurs sont des ressources consommables. -->
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

## 5. Expéditions, Zones & Monde Top-Down

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
:S: L'apparition du boss est aléatoire — il occupe une case masqué aléatoirement dans la zone.

### 5.6 Monde Top-Down & Exploration

#### 5.6.1 Principe Général

IdleOne ajoute une couche d'exploration **top-down** au-dessus des écrans de gestion. Le joueur incarne un personnage qui se déplace physiquement dans le hub : un village entourant sa ferme de créatures.

- **Perspective** : vue top-down orthographique ou légèrement isométrique.
- **Échelle** : le joueur est un sprite/avatar simple. Les créatures actives apparaissent dans leurs enclos.
- **Rôle** : les écrans de craft, breeding et expédition sont accessibles en s'approchant des bâtiments et en interagissant.

> **DA** : rendu top-down conservant le contraste Kawaii × Dark (§12.2).

#### 5.6.2 Caméra & Contrôles

| Élément | Détail |
|---|---|
| **Déplacement** | Clavier (ZQSD / WASD / flèches) ou click-to-move. |
| **Sprint** | Touche Shift (cosmétique, sans impact mécanique). |
| **Caméra** | Orthographique top-down, défilement auto aux bords. Zoom molette (limité). |
| **Interaction** | Touche E / Espace quand une bulle d'action apparaît au-dessus d'une entité à portée (1.5 tiles). |

#### 5.6.3 Architecture des Maps & Changement de Map

Le hub est découpé en maps connectées par des zones de transition.

| Map | Description | Contenu |
|---|---|---|
| **Village** | Place centrale, maisons PNJ, marché. | Entrée expéditions, quêteurs. |
| **Ferme Extérieure** | Pâturages, enclos, jardins. | Pension extérieure, créatures actives. |
| **Maison du Héros** | Intérieur du hub central. | Coffres, table de craft. |
| **Forge** | Intérieur atelier. | Enclume, établi de composants. |
| **Laboratoire** | Intérieur labo. | Cuves breeding, prédiction hexagonale. |
| **Temple** | Intérieur temple (Run 2+). | Autel de recyclage, offrandes. |

**Transitions** : portes avec fondu noir. Si possible, même grande map avec zones délimitées.

#### 5.6.4 PNJ & Routines de Déplacement

Les PNJ animent le village et la ferme avec des comportements programmés.

| Type | Rôle | Localisation |
|---|---|---|
| **Villageois** | Lore, rumeurs. | Village, maisons. |
| **Marchand** | Objets cosmétiques, matériaux. | Place centrale (jour). |
| **Quêteur** | Quêtes secondaires. | Change selon la quête. |
| **Visiteur Prestigieux** | Rewards spéciaux (Orgueil O-2). | Mairie, aléatoire. |
| **Soigneur** | Soigne créatures contre Fragments. | Pension. |

**Routines** :
- **Patrouille** : waypoints avec pauses aléatoires (5-15s).
- **Horaires** : jour (06-20h) actifs dans le village ; nuit retour chez eux. PNJ spéciaux (marchand noir) apparaissent la nuit.
- **Contextuels** : panique si créature échappée ; intérêt si créature rare à proximité ; nervosité si haut Taboo.

<!-- Traité par Agent Concepteur : Ajout des routines météo et événements ponctuels. Pluie → PNJ se réfugient dans les bâtiments, créatures errantes moins actives. Festival saisonnier → PNJ spéciaux offrent quêtes limitées et rewards bonus (Formes/Couleurs rares). -->

#### 5.6.5 Système d'Interactions

Touche d'action (E / Espace) quand une bulle (!/...) apparaît au-dessus d'une entité à portée (1.5 tiles).

| Cible | Résultat |
|---|---|
| **Bâtiment** | Entrer / ouvrir UI de gestion. |
| **Machine / Établi** | UI de craft, breeding, etc. |
| **Créature (enclos)** | Inspection rapide (bulle nom/PV/statut). Option Nourrir. |
| **PNJ** | Dialogue texte, déclenche quête. |
| **Objet décor** | Inspection lore ou récupération Fragments. |

#### 5.6.6 Ferme de Créatures & Enclos

La ferme est le cœur spatial. Les créatures actives résident dans des enclos visibles délimités par des clôtures.

- **Capacité** : chaque enclo accueille [N] créatures selon le niveau de la Pension (§7.5.1).
- **Comportements** :
  - **Errance** : random walk dans l'enclos avec rebond sur les bords.
  - **Faim** : immobilisation + bulle "faim" si PV bas.
  - **Sommeil** : allongement périodique.
  - **Panique** : fuite opposée si enclos endommagé.
- **Interaction** : le joueur peut nourrir (+PV) ou caresser (cosmétique, cœurs visuels).
- **Progression visuelle** : clôtures bois → métal → cages industrielles avec l'accumulation de Taboo.

#### 5.6.7 Liens avec les Autres Systèmes

- **Expéditions** : portail dans le village pour accéder à l'interface des expéditions (§5.1).
- **Bâtiments** : chaque bâtiment du §7.5 possède une représentation 2D/3D. Les upgrades modifient visuellement l'apparence.
- **Économie** : marchand du village accepte Fragments et Gold (Run 2+). Ventes de créatures via stand de marché débloqué avec la Mairie.
- **Prestige** : au Rebirth, la map est réinitialisée (bâtiments niveau 1, enclos vides), mais Essence et upgrades permanents persistent.

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

## 7. Économie, Ressources & Bâtiments

### 7.1 Ressources de craft créatures

| Ressource | Source | Utilisation |
|---|---|---|
| **Fragments** | Clic + idle | Monnaie de base, achats divers |
| **Formes** (items) | Expéditions, drops | Composant de craft |
| **Couleurs** (items) | Expéditions, drops | Composant de craft |
| **Matériaux** | Expéditions, mining | Upgrades de bâtiments |
| **Essence** ✨ | Rebirth uniquement | Boutique prestige |
<!-- Traité par Agent Concepteur : Les squelettes ne sont pas des ressources consommables mais des **types de base** choisis au moment du craft Gen-0. Seules les Formes et Couleurs sont des items/ressources consommables. Les Squelettes sont débloqués via la Boutique de Prestige (Essence). -->

### 7.2 Coûts (première ébauche — à équilibrer)

#### 7.2.1 Gold

Le Gold est la monnaie de commerce et de luxe du late-game, distincte des Fragments (monnaie de base d'un run).

**Sources de production**

| Source | Description | Disponibilité |
|---|---|---|
| **Vente de créatures** | Les créatures Gen-2+ peuvent être vendues sur le marché. Le prix dépend de la génération, des stats et des traits rares. | Run 2+ (débloqué via Mairie) |
| **Expéditions — Cases Trésor** | Certaines cases spéciales (rares) rapportent du Gold directement. | Run 2+, haute CHA augmente la fréquence |
| **Bâtiment : Bourse** | Débloqué Run 3+ via prestige. Génère passivement des intérêts sur le Gold stocké. | Run 3+ |
| **Réalisations (Achievements)** | Milestones globaux (créatures créées, runs complétés, etc.) donnent des bonus de Gold. | Permanent |

**Sinks de Gold**

| Sink | Description | Impact |
|---|---|---|
| **Accélérations** | Payer du Gold pour instantanément terminer un timer (craft, breeding, exploration). | Qualité de vie |
| **Upgrades de bâtiments avancés** | Certains niveaux ultimes de bâtiments (niveaux 10+) nécessitent du Gold. | Puissance |
| **Décorations & Skins** | Achat cosmétique pour la base et les créatures. | Personnalisation |
| **Donations à l'Orgueil** | Offrir du Gold dans l'arbre Orgueil pour débloquer des visiteurs prestigieux. | Progression |

**Caps et équilibrage**

- **Soft cap par run** : Les ventes de créatures de même génération subissent des rendements décroissants (diminishing returns). Vendre 10 créatures Gen-5 rapporte moins que vendre 10 créatures de générations variées.
- **Hard cap temporaire** : Le Gold se reset au Rebirth, sauf les upgrades permanents achetés avec Gold et les décorations.
- **Intérêts de la Bourse** : Plafonnés à [X]% du total de Fragments convertis ce run.

#### 7.2.2 Taboo

Le Taboo est la ressource "morale" du jeu, matérialisant la corruption progressive du joueur. Plus le joueur adopte des comportements sombres (sacrifices, exploitation), plus il accumule du Taboo.

**Mécaniques de gain**

| Source | Description | Multiplicateur de base |
|---|---|---|
| **Sacrifices au Temple** | Recycler une créature au Temple des Sacrifices génère du Taboo proportionnel à ses stats et sa génération. | [5-20] Taboo par créature |
| **Décisions morales sombres** | En expédition, certains événements (❓) proposent un choix corrompu (ex: voler des ressources, abandonner une créature blessée). Le choix sombre rapporte du Taboo + rewards matériels. | [10-50] par décision |
| **Recycling massif** | Détruire plus de [N] créatures en [M] minutes déclenche un bonus de "Frenzy Taboo" (multiplicateur ×[1.5-2.0] pendant [T] minutes). | Multiplicateur temporel |
| **Bonus passif par run** | Chaque run complété après le 2ème accorde un bonus permanent de [+X%] de gain de Taboo. | [+5%] par run |

**Facteurs de multiplicateurs**

| Facteur | Effet |
|---|---|
| **Taux de recyclage** | Pourcentage de créatures recyclées vs créées ce run. Plus le ratio est élevé, plus le multiplicateur est fort (jusqu'à un cap de [×3.0]). |
| **Nombre de runs** | Bonus cumulatif de [+5%] par run complété après le 1er. |
| **Ratio morts/créés** | Nombre total de créatures sacrifiées / nombre total créées (across all runs). Influence un multiplicateur global permanent. |
| **Affinité Élément Ombre** | Les créatures de couleur Violet (Ombre) génèrent [+20%] de Taboo lors de leurs sacrifices. |

**Sinks de Taboo**

| Sink | Description |
|---|---|
| **Arbres de compétences (7 péchés)** | Coût principal. Chaque nœud coûte du Taboo. |
| **Offrandes au Temple** | Offrir du Taboo pour des boosts temporaires (ex: +50% vitesse de breeding pendant 1h). |
| **Vœux corrompus** | Consommer du Taboo pour modifier un vœu (Wish) en sa version sombre, plus puissante mais avec un malus. |

**Lore et progression narrative**

Le Taboo influence visuellement la base du joueur :
- **Taboo < 100** : Base normale, ambiance Kawaii dominante.
- **Taboo 100-500** : Légères teintes sombres, créatures légèrement plus agressives.
- **Taboo 500-2000** : Transformation visible — ombres, fissures, créatures avec traits sombres.
- **Taboo > 2000** : Atmosphère dystopique complète, reflétant la corruption maximale du joueur.

Ces effets sont purement cosmétiques et optionnels (peuvent être désactivés dans les options).

#### 7.2.3 Wish

Les vœux sont la ressource de meta-game la plus rare, permettant de personnaliser l'expérience entre les runs et de créer des "builds" de départ uniques.

**Conditions de déblocage**

| Condition | Détail |
|---|---|
| **Run minimum** | Disponible à partir du **3ème Rebirth**. |
| **Taboo total** | Avoir accumulé [500+] Taboo au cours de toutes les runs précédentes. |
| **Déclenchement** | Le premier Wish est offert gratuitement au 3ème Rebirth. Les suivants se gagnent par milestones. |

**Mécanique de gain**

Les points de Wish se gagnent via des milestones globales (across all runs) :

| Milestone | Reward Wish |
|---|---|
| **Zone complétée (multiple de 10)** | +1 Wish |
| **Génération maximale atteinte (multiple de 5)** | +1 Wish |
| **Achievement rare** | +[1-3] Wishes selon la difficulté |
| **Premier run avec Taboo > 1000** | +2 Wishes |

**Types de vœux**

| Type | Description | Impact |
|---|---|---|
| **Vœu de Créature** | Réincarner en début de run avec une créature Gen-0 possédant un trait spécial (forme Étoile, couleur Doré, ou +[X] à une stat). | Build de départ spécialisé. Les traits spéciaux s'ajoutent au pool de traits possibles pour toutes les runs futures. |
| **Vœu d'Objet** | Obtenir un **Artefact** unique (objet permanent, non resetté au Rebirth). Chaque artefact donne un bonus passif spécifique (ex: "+10% vitesse d'exploration", "-20% coût de breeding"). | Accumulable. Maximum [10] artefacts actifs simultanément. |
| **Vœu de Sagesse** | Débloquer gratuitement **1 nœud** dans un arbre de péché de son choix, sans coût en Taboo. | Accélération de la progression des arbres. Limité à [1] par arbre. |
| **Vœu de Renaissance** | Bonus de départ pour la prochaine run uniquement : [+X] Fragments initiaux, [+Y%] stats sur les Gen-0 craftés, ou [+Z] slots de créatures. | One-shot par run. Non cumulable avec un autre Vœu de Renaissance. |

**Mécanique de vœu corrompu**

En dépensant du **Taboo supplémentaire**, le joueur peut corrompre un vœu pour en augmenter l'effet :

| Vœu de base | Version corrompue | Coût supplémentaire (Taboo) |
|---|---|---|
| Vœu de Créature | Créature Gen-0 avec **2 traits** spéciaux + couleur Ombre | [×2] |
| Vœu d'Objet | Artefact avec effet **doublé** mais malus visuel (base plus sombre) | [×1.5] |
| Vœu de Sagesse | **2 nœuds** gratuits dans l'arbre (au lieu de 1) | [×2.5] |
| Vœu de Renaissance | Bonus de départ **triplés**, mais Taboo initial de [+100] | [×1.5] |

**Impact sur le meta-game**

- **Builds de départ** : Les vœux permettent des stratégies de départ très différentes d'un run à l'autre (ex: run axé breeding via Envie + Vœu de Créature, ou run speedrun via Paresse + Vœu de Renaissance).
- **Cohésion avec les péchés** : Chaque arbre de péché possède un **nœud de Synergie Wish** qui améliore l'effet des vœux liés à son domaine.
- **Legacy** : Les artefacts et les traits débloqués par les vœus s'accumulent au fil des runs, créant une progression long terme indépendante du système de prestige Essence.

### 7.3 Coûts (première ébauche — à équilibrer)

| Action | Coût |
|---|---|
| Craft Gen-0 | 1 Squelette + 1 Forme + 1 Couleur + X Fragments |
| Breeding | 2 créatures parentes (immobilisées pendant le timer) + Y Fragments |
| Envoyer en expédition | Gratuit (mais la créature est occupée) |
| Upgrade bâtiment Lv. N | N × base_cost Matériaux |

### 7.4 Timers (première ébauche)

| Action | Durée de base |
|---|---|
| Exploration d'une case | 30s → 5min (selon difficulté) |
| Breeding | 2min → 10min (selon génération) |
| Repos d'une créature | 1min (réduit par CON) |

### 7.5 Bâtiments

La base du joueur est composée de bâtiments upgradables. Chaque bâtiment a une **fonction gameplay** et une **évolution visuelle** liée à la progression narrative (voir section 12.2).

#### 7.5.1 Liste des Bâtiments

| Bâtiment | Fonction principale | Améliorations | Évolution visuelle |
|---|---|---|---|
| **Pension / Ranch** | Nourrit et soigne les créatures. Régénère l'endurance après expéditions. Augmente les slots de créatures actives. | Capacité d'accueil, vitesse de repos, qualité de nourriture | Pâturage → Ferme → Élevage intensif |
| **Maison du Héros** | Hub central. Contient les coffres de stockage et la table de craft. Accès aux menus principaux. | Taille de stockage, slots de craft simultanés | Cabane → Maison → Manoir |
| **Forge** | Craft et amélioration d'équipements/composants. Transforme les matériaux bruts en composants raffinés. | Recettes débloquées, vitesse de craft, qualité des outputs | Enclume → Atelier → Fonderie |
| **Temple des Sacrifices** | Permet de **recycler** les créatures faibles en ressources (fragments, essence mineure, nourriture). Déblocage de bonus temporaires via offrandes. | Types de sacrifices, rendement, bonus rituel | Autel de pierre → Temple → Abattoir rituel |
| **Laboratoire de Breeding** | Centre de breeding et d'expérimentation. Affiche la prédiction hexagonale. Permet les mutations (si débloquées). | Slots de breeding, réduction timer, bonus mutation | Tour de mage → Labo → Usine de mutations |
| **Mairie** | Gestion de la base : extension du territoire, placement des bâtiments, déblocage de nouvelles zones de construction. | Territoire max, nombre de bâtiments, vitesse de construction | Panneau → Bureau → Hôtel de ville |

#### 7.5.2 Système d'Upgrade des Bâtiments

Chaque bâtiment a **N niveaux** (à équilibrer). Le coût augmente par palier :

```
Coût_upgrade(Lv) = base_cost × Lv × multiplicateur
```

| Ressource utilisée | Bâtiments concernés |
|---|---|
| **Fragments** | Tous (coût de base) |
| **Matériaux** | Forge, Maison, Mairie |
| **Essence de créature** (recyclage) | Temple, Laboratoire |

#### 7.5.3 Déblocage des Bâtiments

| Bâtiment | Disponible | Condition |
|---|---|---|
| Maison du Héros | Run 1, début | Toujours disponible |
| Pension / Ranch | Run 1, après 1er craft | Crafter sa première créature |
| Forge | Run 1, après 1ère expédition | Compléter la zone 1 |
| Temple des Sacrifices | Run 2+ | Achat prestige (Essence) |
| Laboratoire de Breeding | Run 1, après 1er breeding | Réussir un premier breeding |
| Mairie | Run 2+ | Achat prestige (Essence) |

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

| Option | Front | Back | Rendu | Hébergement | Plateformes |
|---|---|---|---|---|---|
| **Unity 3D** | Unity UI | C# | 3D | Desktop / Mobile / WebGL | Desktop, Mobile, Web |

On choisit Unity 3D car si on commence en 2D pur pas moyen d'ajouter l'isométrie pour faire un effet 2.5D.

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


## 13. Les 7 Arbres de Péchés

> **Note :** Les ressources avancées (Gold, Taboo, Wish) sont documentées dans la section 7.2 — Économie & Ressources Avancées.


### Structure commune

| Élément | Détail |
|---|---|
| **Nombre de nœuds** | 10 par arbre |
| **Tiers** | 3 tiers : Fondation (nœuds 1-4), Spécialisation (nœuds 5-8), Maîtrise (nœuds 9-10) |
| **Coûts en Taboo** | Tier 1 : [50-100] \| Tier 2 : [200-500] \| Tier 3 : [800-1500] |
| **Progression** | Linéaire avec embranchements : certains nœuds offrent 2-3 choix mutuellement exclusifs. |
| **Déblocage** | Tous les arbres sont visibles dès le 2ème Run, mais chaque arbre nécessite un **seuil de Taboo** pour activer son premier nœud. |

---

### Colère (Wrath)

**Philosophie** : La destruction pure. L'arbre Colère transforme les créatures en machines de guerre, optimisant les dégâts physiques, la vitesse de combat et l'agressivité en expédition.

#### Nœuds de l'arbre Colère

| Nœud | Tier | Nom | Coût (Taboo) | Effet | Synergie |
|---|---|---|---|---|---|
| **C-1** | 1 | Poing de Fer | [50] | +[5-10]% Force (FOR) sur toutes les créatures actives | Stats hexagone |
| **C-2** | 1 | Sang Bouillant | [75] | Les créatures avec FOR > [seuil] ignorent [10-20]% de la défense des obstacles en expédition | Expédition combat |
| **C-3** | 1 | Rage du Recyclé | [100] | +[1-3] FOR pour chaque créature sacrifiée ce run (cumulatif, cap [+20]) | Temple des Sacrifices |
| **C-4** | 1 | Élan Destructeur | [100] | Réduit le timer des cases **Combat** de [10-15]% | Expédition |
| **C-5** | 2 | Brise-Os | [250] | ×[1.5-2.0] dégâts contre les Boss des expéditions. Si le Boss est vaincu en moins de [T] secondes, reward bonus. | Cases Boss |
| **C-6** | 2 | *Branche A* — Carnage / *Branche B* — Fureur | [300] | **A** : Les créatures tuées en expédition ont [20-30]% de chance de dropper un fragment bonus. **B** : +[15-25]% vitesse d'attaque (réduction timer combat). | Mutuellement exclusif |
| **C-7** | 2 | Volonté de Fer | [400] | +[5-10]% Volonté (VOL). Les créatures ne s'arrêtent plus de combattre en cas de HP bas. | Hexagone + survie |
| **C-8** | 2 | Héritage de Guerre | [450] | Les créatures de Gen-[3+] transmettent [+5-8]% de leur FOR à leurs enfants en plus de la variance normale. | Breeding |
| **C-9** | 3 | Avatar de la Colère | [1000] | Débloque la **Forme Étoile Rouge** (uniquement via cet arbre). Cette forme donne +[15-25] FOR et +[5-10]% chance de crit. | Forme exclusive |
| **C-10** | 3 | Cataclysme | [1500] | Une fois par run, peut instantanément vaincre un Boss d'expédition (sans timer). Coût : sacrifier [2-3] créatures de la Pension. | Capacité ultime |

#### Synergies globales de Colère
- Avec **Cupidité** : Le nœud C-5 (Brise-Os) double aussi le Gold drop des Boss.
- Avec **Envie** : Le nœud C-8 (Héritage de Guerre) se cumule avec les bonus de breeding de l'arbre Envie.
- Avec **Taboo** : Plus le Taboo du joueur est élevé, plus les effets de Colère sont visuellement prononcés (auras rouges, animations aggressives).

<!-- Traité par Agent Concepteur : 10 nœuds structurés en 3 tiers, coûts en Taboo avec fourchettes, effets liés à Force/combat/expédition/breeding, forme exclusive Étoile Rouge, capacité ultime Cataclysme, et synergies avec Cupidité/Envie/Taboo -->

### Cupidité (Greed)

**Philosophie** : L'accumulation et la multiplication des richesses. L'arbre Cupidité optimise la production de Gold et de Fragments, transforme les ressources en plus-value, et débloque des mécaniques de commerce avancées.

#### Nœuds de l'arbre Cupidité

| Nœud | Tier | Nom | Coût (Taboo) | Effet | Synergie |
|---|---|---|---|---|---|
| **Cu-1** | 1 | Main d'Or | [50] | +[10-20]% Fragments gagnés par clic et par production idle. | Ressources de base |
| **Cu-2** | 1 | Taxe du Pêché | [75] | Chaque créature vendue génère [5-10]% de Fragments **supplémentaires** (en plus du prix de vente). | Vente créatures |
| **Cu-3** | 1 | Intérêts Compounds | [100] | La Bourse génère [+0.5-1.0]% d'intérêts supplémentaires par niveau. | Bâtiment Bourse |
| **Cu-4** | 1 | Yeux de Lynx | [100] | +[15-25]% chance de découvrir une case **Trésor** en expédition. | Expédition |
| **Cu-5** | 2 | Alchimiste Avare | [250] | Convertit [X%] des Matériaux excédentaires en Gold automatiquement. | Matériaux → Gold |
| **Cu-6** | 2 | *Branche A* — Monopole / *Branche B* — Spéculation | [300] | **A** : Vendre [5+] créatures d'affilée déclenche un multiplicateur de prix ×[1.2-1.5] sur la suivante. **B** : Le prix de vente fluctue aléatoirement entre [0.8×] et [1.5×] — à optimiser. | Mutuellement exclusif |
| **Cu-7** | 2 | Donation Prestigieuse | [400] | Débloque les **donations à l'Orgueil** (lien avec arbre Orgueil). Chaque donation en Gold donne aussi un bonus temporaire de [+5-10]% production. | Orgueil |
| **Cu-8** | 2 | Héritage d'Or | [450] | Les créatures de Gen-[3+] ont [+10-15]% de chance de naître avec une couleur **Doré** (Lumière/+CHA). | Breeding |
| **Cu-9** | 3 | Fontaine de Richesse | [1000] | Débloque le bâtiment **Fontaine de Richesse** (Run 4+). Génère passivement du Gold basé sur le Taboo total du joueur. | Bâtiment exclusif |
| **Cu-10** | 3 | Midas Corrompu | [1500] | Toutes les ressources (Fragments, Matériaux, Formes, Couleurs) peuvent être converties en Gold à un taux de [Y%]. Cap journalier basé sur le run. | Capacité ultime |

#### Synergies globales de Cupidité
- Avec **Orgueil** : Le nœud Cu-7 (Donation Prestigieuse) est le prérequis pour les visiteurs prestigieux de l'arbre Orgueil.
- Avec **Colère** : Les Boss vaincus avec Cataclysme dropperont +[50]% Gold si Cu-5 est actif.
- Avec **Wish** : Chaque artefact de Cupidité augmente la limite de Bourse de [+10%].

<!-- Traité par Agent Concepteur : 10 nœuds en 3 tiers, effets sur Gold/Fragments/intérêts, branche Monopole/Spéculation mutuellement exclusive, bâtiment exclusif Fontaine de Richesse, capacité ultime Midas Corrompu, synergies avec Orgueil/Colère/Wish -->

### Gourmandise (Gluttony)

**Philosophie** : La consommation vorace et l'optimisation par la nourriture. L'arbre Gourmandise introduit un système de cuisine et de buffs nourriture, transformant les créatures recyclées et les ressources en repas puissants.

#### Nœuds de l'arbre Gourmandise

| Nœud | Tier | Nom | Coût (Taboo) | Effet | Synergie |
|---|---|---|---|---|---|
| **G-1** | 1 | Cuisinier de Base | [50] | Débloque le bâtiment **Cuisine** (Run 2+). Permet de transformer des Matériaux en repas simples. | Nouveau bâtiment |
| **G-2** | 1 | Nourriture de Combat | [75] | Les repas peuvent donner un buff temporaire de +[5-10]% FOR pendant [30-60] minutes. | Combat/Expédition |
| **G-3** | 1 | Cannibalisme Rituel | [100] | Recycler une créature au Temple a [20-30]% de chance de générer un **Festin** (repas rare avec buff multiple). | Temple |
| **G-4** | 1 | Appétit Vorace | [100] | Les créatures avec buff nourriture récupèrent leur endurance [15-25]% plus vite après expédition. | Pension |
| **G-5** | 2 | Chaîne Alimentaire | [250] | Débloque les **Recettes Avancées** : combiner 2 repas simples pour un repas supérieur (buff ×[1.5]). | Cuisine |
| **G-6** | 2 | *Branche A* — Chef Étoilé / *Branche B* — Glouton | [300] | **A** : Les repas supérieurs durent [+50%] plus longtemps. **B** : Les repas donnent un buff additionnel aléatoire (+[3-8]% à une stat aléatoire). | Mutuellement exclusif |
| **G-7** | 2 | Festin de Masse | [400] | Un repas peut être appliqué à [2-3] créatures simultanément (au lieu de 1). | Multi-cibles |
| **G-8** | 2 | Héritage du Banquet | [450] | Les créatures nées alors qu'un buff nourriture est actif sur l'un des parents héritent [+2-5]% de la stat boostée. | Breeding |
| **G-9** | 3 | Mangeur d'Âmes | [1000] | Débloque la **Recette Étoile** : consommer un Festin Étoile (créature Gen-[5+] recyclée) donne un buff permanent de +[1] à une stat (jusqu'à un cap de [+5]). | Buff permanent |
| **G-10** | 3 | Famine Éternelle | [1500] | Tous les timers du jeu (craft, breeding, expédition, repos) sont réduits de [10-15]% tant qu'une créature a un buff nourriture actif. | Capacité ultime |

#### Synergies globales de Gourmandise
- Avec **Paresse** : Le nœud G-10 (Famine Éternelle) se cumule avec les réductions de timer de l'arbre Paresse.
- Avec **Envie** : Le nœud G-8 (Héritage du Banquet) amplifie les bonus de breeding de l'arbre Envie.
- Avec **Taboo** : Plus le Taboo est élevé, plus les repas générés par Cannibalisme Rituel sont puissants.

<!-- Traité par Agent Concepteur : 10 nœuds en 3 tiers, système de cuisine avec recettes simples/avancées/étoile, buffs temporaires et permanents, branche Chef/Glouton mutuellement exclusive, synergies avec Paresse/Envie/Taboo -->

### Orgueil (Pride)

**Philosophie** : L'exhibition et la domination sociale. L'arbre Orgueil transforme la base en palais de renommée, attire des visiteurs prestigieux, et offre des bonus de prestige liés au statut.

| Nœud | Tier | Nom | Coût (Taboo) | Effet |
|---|---|---|---|---|
| **O-1** | 1 | Aura de Grandeur | [50] | +[5-10]% à toutes les stats de la meilleure créature de la base. |
| **O-2** | 1 | Porte Ouverte | [75] | Débloque les **Visiteurs** : PNJ avec quêtes/rewards aléatoires. |
| **O-3** | 1 | Don Prestigieux | [100] | Donner du Gold à un visiteur donne buff +[5-10]% production pendant [1-2]h. |
| **O-4** | 1 | Miroir Déformant | [100] | Créatures avec CHA > [seuil] gagnent +[10-15]% rewards en expédition (cases Cachées). |
| **O-5** | 2 | Cour des Miracles | [250] | Extension Mairie : visiteurs prestigieux [+20-30]% plus fréquents. |
| **O-6** | 2 | *Branche A* — Parade / *Branche B* — Tyrannie | [300] | **A** : Quêtes de show-off, rewards Essence. **B** : Quêtes de domination, rewards Taboo. |
| **O-7** | 2 | Héritage des Rois | [400] | Enfants héritent [+3-6]% de la meilleure stat du parent dominant. |
| **O-8** | 2 | Éclat du Prestige | [450] | Runs avec Taboo > [500] donnent +[1-3]% Essence bonus au Rebirth. |
| **O-9** | 3 | Mécène Corrompu | [1000] | Visiteur unique une fois par run offrant un **Vœu** gratuit. |
| **O-10** | 3 | Apothéose | [1500] | Transformation finale visuelle de la base. Visiteurs légendaires avec artefacts uniques. |

**Synergies** : avec Cupidité (O-3 nécessite Cu-7), avec Envie (O-7 cumule avec bonus Envie), avec Wish (O-9 améliorable par artefact Wish).

<!-- Traité par Agent Concepteur : 10 nœuds, visiteurs/renommée, branche Parade/Tyrannie, Mécène offrant Wish gratuit, Apothéose, synergies Cupidité/Envie/Wish -->

### Envie (Envy)

:S: rework sur des bonus de découverte ou de chance d'obtenir des meilleurs butin 

**Philosophie** : La reproduction obsessionnelle et l'amélioration génétique. L'arbre Envie optimise le breeding : taux, qualité des enfants, mutations, et héritage avancé.

| Nœud | Tier | Nom | Coût (Taboo) | Effet |
|---|---|---|---|---|
| **E-1** | 1 | Fertilité Instable | [50] | Réduit le timer de breeding de [10-15]%. |
| **E-2** | 1 | Géniteur Avide | [75] | +[5-10]% au maximum de la fourchette de breeding. |
| **E-3** | 1 | Espion Génétique | [100] | La prédiction hexagonale affiche la **zone la plus probable** (au lieu de min/max). |
| **E-4** | 1 | Jalousie Mutagène | [100] | +[2-5]% chance de mutation par breeding. |
| **E-5** | 2 | Clonage Imparfait | [250] | Débloque le **Clonage** : dupliquer une créature pour [X] Taboo avec -[20-30]% stats. |
| **E-6** | 2 | *Branche A* — Sélection / *Branche B* — Hybridation | [300] | **A** : [+15-25]% chance d'hériter la meilleure stat du parent dominant. **B** : [+10-20]% chance de combiner les deux affinités élémentaires. |
| **E-7** | 2 | Lignée Maudite | [400] | L'inbreeding a un malus de [variance_factor × 0.5] au lieu de [×1.0]. |
| **E-8** | 2 | Héritage d'Envie | [450] | Gen-[4+] ont [+10-15]% de chance de naître avec un **trait spécial** (ex: Immortalité mineure, résistance mutation négative). |
| **E-9** | 3 | Architecte de Vie | [1000] | Débloque le **Designer Génétique** : choisir la forme ou la couleur de l'enfant avant breeding (coût : [Y] Taboo par choix). |
| **E-10** | 3 | Prolifération | [1500] | Une créature peut être utilisée comme parent [3-4] fois simultanément (au lieu de 2). Les enfants ont [+5-10]% stats bonus. |

**Synergies** : avec Colère (E-8 cumule avec C-8), avec Gourmandise (E-6 amplifie G-8), avec Orgueil (E-7 cumule avec O-7).

<!-- Traité par Agent Concepteur : 10 nœuds, optimisation breeding (timer, variance, mutations, clonage), branche Sélection/Hybridation mutuellement exclusive, inbreeding viable, Designer Génétique, Prolifération, synergies Colère/Gourmandise/Orgueil -->

### Paresse (Sloth)

**Philosophie** : L'efficacité passive et la minimisation de l'effort. L'arbre Paresse réduit les timers, automatise les actions répétitives, et maximise les gains offline.

| Nœud | Tier | Nom | Coût (Taboo) | Effet |
|---|---|---|---|---|
| **P-1** | 1 | Sommeil Léger | [50] | +[10-15]% vitesse de production idle (Fragments, Matériaux). |
| **P-2** | 1 | Auto-Collecte | [75] | Les ressources des cases explorées se collectent automatiquement (plus besoin de cliquer). |
| **P-3** | 1 | Veille Nocturne | [100] | Bonus offline augmenté de [+20-30]% (calculé sur les gains normaux hors ligne). |
| **P-4** | 1 | Endurance de Fainéant | [100] | Les créatures récupèrent leur endurance [15-25]% plus vite sans repos actif. |
| **P-5** | 2 | Chaîne de Production | [250] | Débloque l'**auto-craft** : programmer [1-3] crafts de Gen-0 à exécuter automatiquement quand les ressources sont disponibles. |
| **P-6** | 2 | *Branche A* — Dormeur / *Branche B* — Planificateur | [300] | **A** : Les créatures continuent d'explorer hors ligne [+30-50]% plus longtemps (VOL). **B** : Les expéditions peuvent être programmées en file d'attente (jusqu'à [3-5] cases). |
| **P-7** | 2 | Auto-Breeding | [400] | Débloque l'**auto-breeding** : sélectionner des couples de créatures pour breeding automatique quand le timer est prêt. |
| **P-8** | 2 | Héritage du Dormeur | [450] | Les créatures nées héritent [+3-6]% de la stat VOL du parent avec le plus de VOL. |
| **P-9** | 3 | Bureaucratie Infernale | [1000] | Débloque le **Gestionnaire** : interface centralisée pour gérer toutes les créatures, expéditions, et breeding en un seul écran. |
| **P-10** | 3 | Éternité | [1500] | Tous les timers (craft, breeding, exploration, repos) sont réduits de [20-30]% de manière permanente. Se cumule avec tous les autres bonus. |

**Synergies** : avec Gourmandise (P-10 cumule avec G-10), avec Orgueil (P-8 cumule avec O-7), avec Envie (P-7 réduit le timer E-1).

<!-- Traité par Agent Concepteur : 10 nœuds, optimisation AFK/offline/automatisation, branche Dormeur/Planificateur mutuellement exclusive, auto-craft/auto-breeding, Gestionnaire, capacité ultime Éternité, synergies Gourmandise/Orgueil/Envie -->
### Luxure (Lust)

**Philosophie** : L'attraction charnelle et les plaisirs de la chair. L'arbre Luxure améliore le charme des créatures, débloque des événements spéciaux liés à la reproduction et au spectacle, et optimise les interactions sociales entre créatures. 

:S: optimise le breeding : taux, qualité des enfants, mutations, et héritage avancé.


| Nœud | Tier | Nom | Coût (Taboo) | Effet |
|---|---|---|---|---|
| **L-1** | 1 | Parfum de Séduction | [50] | +[5-10]% Chance (CHA) sur toutes les créatures actives. |
| **L-2** | 1 | Danse de Cour | [75] | Les créatures avec CHA > [seuil] réduisent le timer de leurs expéditions de [5-10]%. |
| **L-3** | 1 | Appel de la Chair | [100] | +[10-15]% chance de découvrir une case **Événement** en expédition (❓). |
| **L-4** | 1 | Flamme Éphémère | [100] | Les événements spéciaux liés à Luxure donnent des rewards bonus en Fragments et rarement en Taboo. |
| **L-5** | 2 | Harem Secret | [250] | Débloque [+1-2] slots de breeding simultanés au Laboratoire (au lieu de 1). |
| **L-6** | 2 | *Branche A* — Courtisane / *Branche B* — Dominateur | [300] | **A** : Les créatures femelles (si applicable) donnent [+15-25]% de stats bonus à leurs enfants. **B** : Les créatures mâles (si applicable) transmettent [+10-20]% de leur meilleure stat aux enfants. |
| **L-7** | 2 | Sylphide Nocturne | [400] | Débloque les **Événements de Nuit** : événements spéciaux en expédition uniquement actifs entre [minuit-6h] (timezone joueur), avec des rewards ×[2-3]. |
| **L-8** | 2 | Héritage de la Chair | [450] | Les enfants nés de parents ayant CHA > [seuil] héritent [+5-8]% CHA supplémentaire. |
| **L-9** | 3 | Bacchanales | [1000] | Débloque les **Festivités** : un événement global une fois par run (déclenchable manuellement) où toutes les créatures gagnent un buff de +[10-15]% à toutes les stats pendant [2-4] heures. |
| **L-10** | 3 | Extase Éternelle | [1500] | Les créatures ne perdent plus d'endurance en expédition ( Constitution devient infinie pour l'exploration enchaînée). Les events de breeding ont [+25-35]% de rewards bonus. |

**Synergies** : avec Colère (L-1 cumule avec C-1 sur FOR/CHA), avec Orgueil (L-6 cumule avec O-2 visiteurs), avec Envie (L-8 cumule avec E-8 sur héritage).

<!-- Traité par Agent Concepteur : 10 nœuds, charme/CHA/événements spéciaux, branche Courtisane/Dominateur mutuellement exclusive, événements nocturnes, Festivités globales, capacité ultime Extase Éternelle, synergies Colère/Orgueil/Envie -->

---

## 14. Système de Profil Joueur

> **Implémentation** : `Assets/Script/Entities/Player/` et `Assets/Script/Managers/GameManager.cs`.

### 14.1 Structure du Profil

Le profil joueur (`PlayerProfile`) est le conteneur principal de toutes les données d'une partie. Il regroupe :

| Champ | Type | Description |
|---|---|---|
| **PlayerName** | string | Nom du joueur choisi à la création |
| **RunCount** | int | Numéro du run actuel (commence à 1, incrémenté au rebirth) |
| **PlayTime** | float | Temps de jeu total en secondes (cumulé sur tous les runs) |
| **CreatedAt** | string (ISO 8601) | Date de création du profil |
| **LastSavedAt** | string (ISO 8601) | Date de la dernière sauvegarde |
| **Resources** | PlayerResources | Sous-système de ressources (§14.2) |
| **Inventory** | PlayerInventory | Sous-système d'inventaire (§14.3) |

Toutes les classes sont `[Serializable]` pour la sauvegarde future (§16).

### 14.2 Cycle de vie des Ressources

Les ressources sont centralisées dans `PlayerResources`. Elles se divisent en deux catégories selon leur comportement au rebirth :

#### Ressources reset au Rebirth

| Ressource | Type | Source | Référence GDD |
|---|---|---|---|
| **Fragments** | float | Clic + idle | §7.1 |
| **Gold** | float | Commerce late-game, ventes | §7.2.1 |
| **Materials** | int | Expéditions, mining | §7.1 |

#### Ressources persistantes

| Ressource | Type | Source | Référence GDD |
|---|---|---|---|
| **Essence** | float | Rebirth uniquement | §6.2 |
| **Taboo** | float | Sacrifices, décisions morales | §7.2.2 |
| **Wishes** | int | Milestones globaux | §7.2.3 |

#### Méthodes principales

- `Add(ResourceType, amount)` — Ajoute une quantité à une ressource
- `Spend(ResourceType, amount) → bool` — Dépense si le joueur peut se le permettre
- `CanAfford(ResourceType, amount) → bool` — Vérifie la disponibilité
- `ResetForRebirth()` — Applique le reset selon les règles §6.3/§6.4

Un événement `OnResourceChanged(ResourceType, oldValue, newValue)` permet à l'UI de se rafraîchir automatiquement.

### 14.3 Inventaire & Slots

L'inventaire (`PlayerInventory`) gère les créatures et les composants de craft.

#### Créatures

| Liste | Description | Comportement au Rebirth |
|---|---|---|
| **OwnedCreatures** | Toutes les créatures du joueur | Reset (vidé) |
| **ActiveCreatures** | Créatures déployées dans les slots actifs | Reset (vidé) |

**MaxActiveSlots** : 5 par défaut. Augmentable via la boutique de prestige (§6.5 — Slot Créature+, coût 20 Essence par slot).

#### Types débloqués (persistent au rebirth)

| Liste | Contenu | Source de déblocage |
|---|---|---|
| **UnlockedSkeletons** | Types de squelettes disponibles au craft | Boutique prestige (§6.5) |
| **UnlockedShapes** | Formes disponibles au craft | Boutique prestige + expéditions |
| **UnlockedColors** | Couleurs disponibles au craft | Boutique prestige + expéditions |

#### Stocks consommables (reset au rebirth)

| Stock | Description |
|---|---|
| **ShapeStock** | Quantités de chaque forme (utilisées au craft Gen-0) |
| **ColorStock** | Quantités de chaque couleur (utilisées au craft Gen-0) |

#### Contenu de départ — Run 1 (§8)

- 1 squelette : Bipède
- 3 formes débloquées : Rond, Triangle, Carré (+ stock initial de 3 chacune)
- 3 couleurs débloquées : Rouge, Bleu, Vert (+ stock initial de 3 chacune)
- 5 slots de créatures actives

### 14.4 GameManager

Le `GameManager` est un **singleton MonoBehaviour** (`DontDestroyOnLoad`) qui centralise l'accès au profil joueur.

| Propriété / Méthode | Description |
|---|---|
| `Instance` (static) | Accès global au singleton |
| `CurrentProfile` | Profil du joueur actuellement chargé |
| `NewGame(playerName)` | Crée un profil vierge avec les contenus de départ du Run 1 |
| `Rebirth()` | Reset les ressources/inventaire, incrémente RunCount, calcule l'Essence gagnée (§6.2) |
| `LoadProfile(profile)` | Charge un profil existant (utilisé par le système de sauvegarde) |

**Formule d'Essence au Rebirth (placeholder)** :
```
Essence = (nb_créatures × 0.5) + (gen_max × 2) + (run_count × 1)
Minimum : 1 Essence par rebirth
```

Cette formule sera affinée quand les systèmes d'expédition et de zones seront implémentés (cf. §6.2).

---

## 15. Interface Utilisateur (UI)

> **Implémentation** : `Assets/Script/UI/`, `Assets/Script/UI/Tabs/`, `Assets/Script/MainUISpawner.cs`.

### 15.1 Architecture d'Onglets

Le système d'onglets (`TabManager`) est le composant central de navigation dans l'UI du jeu. Il est **générique et réutilisable** pour différents contextes (UI principale, paramètres, etc.).

| Élément | Description |
|---|---|
| **TabDefinition** | Structure contenant : nom de l'onglet, GameObject du contenu, Button de sélection |
| **SwitchTab(index)** | Désactive tous les contenus, active celui de l'index donné, met à jour les couleurs des boutons |
| **OnTabChanged(index)** | Événement C# déclenché à chaque changement d'onglet, permettant aux panneaux de se rafraîchir |
| **Highlight visuel** | Le bouton de l'onglet actif change de couleur (activeColor vs inactiveColor) |
| **Ajout dynamique** | Méthode `AddTab()` pour construire les onglets en runtime (spawners programmatiques) |

Principes de navigation :
- Un seul onglet est actif à la fois
- Le premier onglet est affiché par défaut au démarrage
- Le contenu des onglets inactifs est désactivé (`SetActive(false)`) pour optimiser les performances
- Le TabManager ne dépend d'aucun système spécifique — il gère uniquement la visibilité et les événements

### 15.2 Liste de Créatures

L'onglet "Créatures" affiche la liste scrollable de toutes les créatures du joueur (`PlayerInventory.OwnedCreatures`).

| Élément | Description |
|---|---|
| **CreatureListUI** | Gère la liste scrollable et le panneau de détails |
| **CreatureListItemUI** | Élément de liste pour chaque créature — affiche nom, génération, composants, total stats, statut |
| **Panneau Détail** | Affiche les informations complètes de la créature sélectionnée |
| **Bouton Activer** | Déploie la créature dans un slot actif (`PlayerInventory.SetActive()`) ou la désactive |
| **Bouton Détails** | Affiche/masque le panneau de détails |
| **Bouton Recycler** | Placeholder pour le système de recyclage (Temple des Sacrifices, §7.5.1) |

Comportements :
- La liste se rafraîchit automatiquement quand l'onglet redevient actif (via `OnTabChanged`)
- Chaque item affiche : nom, Gen-N, squelette/forme/couleur, total stats, statut (Actif/Inactif/Incapacité)
- La sélection d'un item met à jour le panneau de détails et applique un highlight visuel

Filtres et tri futurs (non implémentés) :
- Tri par nom, génération, total stats, statut
- Filtre par squelette, forme, couleur, statut
- Recherche par nom

### 15.3 Inventaire

L'onglet "Inventaire" affiche les stocks de composants pour le craft Gen-0.

| Section | Contenu | Source de données |
|---|---|---|
| **Formes** | Grid d'items (icône couleur + nom + quantité) | `PlayerInventory.ShapeStock` |
| **Couleurs** | Grid d'items (icône couleur + nom + quantité) | `PlayerInventory.ColorStock` |
| **Squelettes débloqués** | Liste non consommable des types disponibles | `PlayerInventory.UnlockedSkeletons` |

Les formes et couleurs sont des **ressources consommables** (§7.1), tandis que les squelettes sont des **types débloqués** permanents (§6.5).

L'inventaire se rafraîchit au retour sur l'onglet via `OnTabChanged`.

### 15.4 Panneau Statistiques Joueur

L'onglet "Stats" affiche les ressources en temps réel et les informations de profil.

#### Ressources affichées

| Ressource | Format | Mise à jour |
|---|---|---|
| Fragments | Entier | Temps réel via `OnResourceChanged` |
| Gold | Entier | Temps réel via `OnResourceChanged` |
| Materials | Entier | Temps réel via `OnResourceChanged` |
| Essence | Décimal (1 chiffre) | Temps réel via `OnResourceChanged` |
| Taboo | Décimal (1 chiffre) | Temps réel via `OnResourceChanged` |
| Wishes | Entier | Temps réel via `OnResourceChanged` |

#### Informations de profil

| Info | Source |
|---|---|
| Nom du joueur | `PlayerProfile.PlayerName` |
| Run actuel | `PlayerProfile.RunCount` |
| Temps de jeu | `PlayerProfile.PlayTime` (formaté HH:MM:SS) |
| Nombre de créatures | `PlayerInventory.OwnedCreatures.Count` |

Le panneau s'abonne à `PlayerResources.OnResourceChanged` pour un rafraîchissement automatique sans polling.

### 15.5 Intégration BreedingUI

L'UI de breeding existante (§4.5) est intégrée telle quelle comme contenu de l'onglet "Breeding".

| Aspect | Détail |
|---|---|
| **Composant** | `BreedingUI` existant, non modifié |
| **Initialisation** | Via `BreedingUI.Initialize()` avec injection des références UI |
| **Contenu** | Parents A/B avec radar charts, prédiction hexagonale, résultat enfant, contrôles (variance, mutation) |
| **Autonomie** | Le BreedingUI gère ses propres parents aléatoires et son propre breeding |

Le spawner `MainUISpawner` reconstruit la même hiérarchie UI que le `DemoBreedingSpawner` à l'intérieur du panneau d'onglet Breeding.

#### Header persistant

Un header en haut de l'écran affiche en permanence les trois ressources principales :
- **Fragments** (couleur dorée)
- **Essence** (couleur violette)
- **Gold** (couleur or)

Ce header est visible sur tous les onglets et se met à jour en temps réel.

---

## 16. Système de Sauvegarde

> **Implémentation** : `Assets/Script/Managers/SaveManager.cs`, `Assets/Script/Entities/Player/SaveData.cs`, `SaveMetadata.cs`, `SerializableKeyValue.cs`.

### 16.1 Slots de Sauvegarde

Le jeu propose **3 slots de sauvegarde** indépendants (numérotés 0, 1, 2).

| Élément | Détail |
|---|---|
| **Format** | JSON (`JsonUtility` Unity natif, aucune dépendance externe) |
| **Emplacement** | `Application.persistentDataPath + "/saves/"` |
| **Nom de fichier** | `save_slot{N}.json` (N = 0, 1, 2) |
| **Métadonnées rapides** | Nom du joueur, numéro de run, temps de jeu, date de dernière sauvegarde, version du jeu — accessibles sans charger tout le profil |

Chaque slot peut être sauvegardé, chargé ou supprimé indépendamment. Le slot actif (utilisé par l'auto-save) est mémorisé dans `PlayerPrefs`.

### 16.2 Auto-Sauvegarde

| Paramètre | Valeur par défaut | Limites |
|---|---|---|
| **Activée** | Oui | Toggle on/off |
| **Intervalle** | 5 minutes | Min : 1 min, Max : 60 min |
| **Slot cible** | Slot actif (PlayerPrefs) | 0, 1 ou 2 |

- L'auto-save fonctionne via une **coroutine** (`WaitForSecondsRealtime`) pour ne pas geler le jeu ni être affecté par `Time.timeScale`.
- L'auto-save ne s'exécute pas s'il n'y a aucun profil chargé.
- L'intervalle peut être modifié à chaud (la coroutine est redémarrée).

### 16.3 Export / Import

#### Export

- Copie le fichier JSON d'un slot vers un chemin choisi par le joueur.
- Le fichier exporté utilise l'extension custom **`.idleonesave`** pour éviter toute confusion avec d'autres fichiers JSON.

#### Import

- Le joueur sélectionne un fichier `.idleonesave` et un slot de destination.
- **Validation avant écrasement** :
  - Le fichier doit être un JSON valide.
  - Le champ `Version` doit être présent (preuve qu'il s'agit d'un fichier IdleOne).
  - Le champ `Profile` doit exister.
- Si la validation échoue, l'import est refusé avec un message explicite.

### 16.4 Données Sauvegardées

Le conteneur de sauvegarde (`SaveData`) encapsule :

| Champ | Type | Description |
|---|---|---|
| **Metadata** | SaveMetadata | Métadonnées rapides (§16.1) |
| **Profile** | PlayerProfile | Profil complet (§14.1) : identité, ressources, inventaire, créatures |
| **Version** | string | Version du format de sauvegarde |
| **SerializedShapeStock** | List\<SerializableKeyValue\> | Stock de formes (conversion depuis Dictionary) |
| **SerializedColorStock** | List\<SerializableKeyValue\> | Stock de couleurs (conversion depuis Dictionary) |

> **Note technique** : `JsonUtility` ne sérialise pas les `Dictionary<string, int>`. Les stocks de formes et couleurs du `PlayerInventory` sont convertis en listes de paires clé-valeur (`SerializableKeyValue`) lors de la sauvegarde, et reconstruits en Dictionary au chargement.

### 16.5 Migration de Sauvegardes

Chaque fichier de sauvegarde contient un champ **`Version`** (format sémantique : `"1.0.0"`).

Ce champ permettra dans le futur :
- De détecter les sauvegardes obsolètes.
- D'appliquer des migrations automatiques (ajout de nouveaux champs, conversion de structures).
- De refuser les fichiers incompatibles avec un message clair.

La version actuelle du format est `1.0.0`.

---

## 17. Paramètres du Jeu

> **Implémentation** : `Assets/Script/Managers/SettingsManager.cs`, `Assets/Script/UI/Settings/SettingsUI.cs`, `SaveSettingsUI.cs`, `AudioSettingsUI.cs`, `GraphicsSettingsUI.cs`, `SettingsSpawner.cs`.

### 17.1 Catégories de Paramètres

Le panneau de paramètres est un **overlay** (Canvas avec sort order supérieur) accessible depuis le menu principal ou le jeu, sans changement de scène. Il utilise le `TabManager` (§15.1) pour organiser les réglages en sous-onglets.

| Catégorie | Description | Application |
|---|---|---|
| **Sauvegarde** | Auto-save, export/import de fichiers | Immédiate via SaveManager |
| **Audio** | Volumes master, musique, SFX, mute | Temps réel |
| **Graphismes** | Qualité, résolution, plein écran | Via bouton "Appliquer" |

Le panneau inclut un bouton "Réinitialiser" pour rétablir tous les paramètres aux valeurs par défaut, et un bouton "Fermer" (✕) pour masquer l'overlay.

### 17.2 Sauvegarde

Le sous-onglet Sauvegarde permet de configurer l'auto-save et de gérer l'export/import.

#### Auto-sauvegarde

| Contrôle | Type | Détail |
|---|---|---|
| **Activée/Désactivée** | Toggle | Synchronisé avec `SaveManager.AutoSaveEnabled` |
| **Intervalle** | Slider (1-60 min) | Pas de 1 minute, synchronisé avec `SaveManager.SetAutoSaveInterval()` |
| **Sauvegarder maintenant** | Bouton | Sauvegarde immédiate dans le slot actif |
| **Dernière sauvegarde** | Label | Date/heure du dernier auto-save ou save manuel |

#### Export

| Contrôle | Type | Détail |
|---|---|---|
| **Slot à exporter** | Dropdown (0, 1, 2) | Sélection du slot source |
| **Exporter** | Bouton | Copie vers `persistentDataPath/exports/` avec timestamp |

#### Import

| Contrôle | Type | Détail |
|---|---|---|
| **Slot de destination** | Dropdown (0, 1, 2) | Sélection du slot cible |
| **Importer un fichier** | Bouton | Importe le fichier `.idleonesave` le plus récent du dossier exports |

### 17.3 Audio

Le sous-onglet Audio permet de régler les volumes du jeu. **Les changements s'appliquent en temps réel** (pas de bouton "Appliquer").

| Contrôle | Type | Plage | Défaut |
|---|---|---|---|
| **Volume Master** | Slider | 0-100% | 100% |
| **Volume Musique** | Slider | 0-100% | 70% |
| **Volume SFX** | Slider | 0-100% | 100% |
| **Muet global** | Toggle | On/Off | Off |

- Le **Volume Master** est appliqué via `AudioListener.volume`.
- Les volumes **Musique** et **SFX** seront routés via les `AudioSource` individuelles ou un `AudioMixer` quand le système audio sera en place.
- Le **Muet global** force `AudioListener.volume = 0` indépendamment des sliders.

### 17.4 Graphismes

Le sous-onglet Graphismes permet de configurer la qualité d'affichage. **Les changements nécessitent un clic "Appliquer"** pour prendre effet.

| Contrôle | Type | Source | Défaut |
|---|---|---|---|
| **Qualité** | Dropdown | `QualitySettings.names` (Low, Medium, High, Ultra…) | Max disponible |
| **Plein écran** | Toggle | `Screen.fullScreen` | Activé |
| **Résolution** | Dropdown | `Screen.resolutions` (liste dynamique) | Résolution courante |

Les paramètres graphiques sont appliqués via :
- `QualitySettings.SetQualityLevel()`
- `Screen.fullScreen`
- `Screen.SetResolution()`

### 17.5 Persistance

Les paramètres du jeu sont stockés dans **`PlayerPrefs`**, indépendamment des fichiers de sauvegarde du profil joueur.

| Aspect | Détail |
|---|---|
| **Stockage** | `PlayerPrefs` (clés préfixées `Settings_`) |
| **Chargement** | Automatique au démarrage (`SettingsManager.Awake()`) |
| **Sauvegarde** | Immédiate à chaque modification |
| **Portée** | Globale — les settings ne sont pas liés à un slot de sauvegarde |
| **Reset** | Bouton "Réinitialiser" dans le panneau Settings |

Cette séparation garantit que les préférences du joueur (volume, qualité, etc.) sont conservées même si les fichiers de sauvegarde sont supprimés ou importés.

### 17.6 Extensibilité

Le système de paramètres est conçu pour être facilement extensible :

| Future catégorie | Contenu envisagé |
|---|---|
| **Contrôles** | Remapping des touches, sensibilité |
| **Accessibilité** | Taille du texte, daltonisme, sous-titres |
| **Notifications** | Alertes de récolte, rappels d'expédition |
| **Gameplay** | Vitesse de jeu, skip des animations |

Pour ajouter une nouvelle catégorie :
1. Créer un nouveau `MonoBehaviour` UI (ex: `ControlsSettingsUI.cs`) dans `Assets/Script/UI/Settings/`
2. Ajouter les clés `PlayerPrefs` dans `SettingsManager`
3. Ajouter un onglet dans le `SettingsSpawner` via `TabManager.AddTab()`

---

## 18. Navigation & Menus

Cette section décrit le flux de navigation entre les scènes, le menu principal, les slots de sauvegarde, le menu debug et l'architecture des scènes.

### 18.1 Flux de Navigation

Le jeu suit un flux linéaire de scènes, géré exclusivement par le `GameSceneManager` (singleton).

```
[Bootstrap]  →  Initialise tous les managers (DontDestroyOnLoad)
     │
     │  Profil chargé ?  ── oui ──→  [MainUI]
     │                     ── non ──→  [MainMenu]
     ▼
[MainMenu]   →  Nouvelle Partie / Charger / Paramètres / Quitter
     │
     ├── Nouvelle Partie → saisie nom → GameManager.NewGame() → [MainUI]
     ├── Charger → SaveSlotUI → SaveManager.LoadGame() → [MainUI]
     ├── Paramètres → SettingsUI (overlay)
     └── Quitter → Application.Quit()
     ▼
[MainUI]     →  Scène principale avec onglets (§15)
     │
     ├── Bouton ⚙ → SettingsUI (overlay)
     ├── F12 / Bouton Debug → DebugMenuUI (overlay)
     └── DebugMenu > "Retour Menu" → sauvegarde + [MainMenu]
```

**Règle** : toutes les transitions de scène passent par `GameSceneManager`. Aucun appel direct à `SceneManager.LoadScene()` n'est autorisé ailleurs.

### 18.2 Menu Principal

La scène `MainMenu` affiche un menu central avec 4 boutons :

| Bouton | Action |
|---|---|
| **Nouvelle Partie** | Ouvre un panneau de saisie du nom du joueur. Crée un profil via `GameManager.NewGame(name)`, sauvegarde dans le premier slot disponible, puis charge MainUI. |
| **Charger Partie** | Ouvre le panneau `SaveSlotUI` affichant les 3 slots de sauvegarde (§18.3). |
| **Paramètres** | Ouvre le panneau `SettingsUI` en overlay (§17). |
| **Quitter** | Appelle `Application.Quit()`. En éditeur, arrête le Play Mode. |

Le menu affiche également :
- Le titre du jeu ("IdleOne")
- Le sous-titre ("Idle Breeding Game")
- La version du jeu en bas à droite (`Application.version`)

**Implémentation** : `MainMenuSpawner.cs` construit toute l'UI en runtime (même pattern que `MainUISpawner`).

### 18.3 Slots de Sauvegarde

Le panneau `SaveSlotUI` affiche les 3 slots de sauvegarde (constante `SaveManager.MAX_SLOTS`).

| Élément | Détail |
|---|---|
| **Slot occupé** | Affiche : nom joueur, numéro de run, temps de jeu formaté (Xh XXm), date dernière sauvegarde (dd/MM/yyyy HH:mm) |
| **Slot vide** | Affiche "Slot vide" grisé, boutons désactivés |
| **Bouton Charger** | `SaveManager.LoadGame(slot)` → `GameManager.LoadProfile(profile)` → `GameSceneManager.LoadMainUI()` |
| **Bouton Supprimer** | Panneau de confirmation → `SaveManager.DeleteSave(slot)` → rafraîchissement |
| **Bouton Retour** | Ferme le panneau, retour au menu principal |

Les métadonnées sont lues via `SaveManager.GetSaveInfo(slot)` sans charger le profil complet.

### 18.4 Menu Debug

Menu overlay réservé au développement, **compilé conditionnellement** avec `#if UNITY_EDITOR || DEVELOPMENT_BUILD`.

| Aspect | Détail |
|---|---|
| **Activation** | Touche `F12` (toggle) ou bouton dans le header de MainUI |
| **Affichage** | Panneau latéral droit, Canvas overlay (sortingOrder 200) |
| **Fermeture** | Bouton ✕, touche F12, ou bouton header |

#### Navigation rapide

| Bouton | Action |
|---|---|
| Créatures | `TabManager.SwitchTab(0)` |
| Breeding | `TabManager.SwitchTab(1)` |
| Inventaire | `TabManager.SwitchTab(2)` |
| Stats | `TabManager.SwitchTab(3)` |
| Paramètres | `SettingsSpawner.Toggle()` |
| Retour Menu | Sauvegarde + `GameSceneManager.LoadMainMenu()` |

#### Triche Debug

| Bouton | Effet |
|---|---|
| +1000 Fragments | `Resources.Add(Fragments, 1000)` |
| +100 Essence | `Resources.Add(Essence, 100)` |
| +10 Gold | `Resources.Add(Gold, 10)` |
| Spawn Créature | Crée une créature aléatoire dans l'inventaire |
| Forcer Save | `SaveManager.SaveGame(activeSlot)` |
| Rebirth | `GameManager.Rebirth()` |
| Toggle Console | Affiche/masque un log scrollable (`Application.logMessageReceived`) |

#### Infos Runtime

| Info | Source |
|---|---|
| FPS | `1 / Time.unscaledDeltaTime` (moyenne 0.5s) |
| Nombre de créatures | `Inventory.OwnedCreatures.Count` |
| Slot actif | `SaveManager.ActiveSlot` |
| Dernière auto-save | `PlayerProfile.LastSavedAt` |

### 18.5 Architecture des Scènes

Le jeu utilise 3 scènes principales :

| Scène | Index Build | Rôle | Contenu |
|---|---|---|---|
| **Bootstrap** | 0 | Point d'entrée unique | `BootstrapLoader` : crée les singletons managers, redirige vers MainMenu ou MainUI |
| **MainMenu** | 1 | Menu principal | `MainMenuSpawner` : construit le menu, intègre SaveSlotUI et SettingsSpawner |
| **MainUI** | 2 | Jeu principal | `MainUISpawner` : 4 onglets, header ressources, bouton Settings, bouton Debug |

#### Managers Singleton (DontDestroyOnLoad)

| Manager | Fichier | Rôle |
|---|---|---|
| `GameManager` | `Managers/GameManager.cs` | Profil joueur, NewGame, Rebirth |
| `SaveManager` | `Managers/SaveManager.cs` | Sauvegarde/chargement, auto-save, export/import |
| `SettingsManager` | `Managers/SettingsManager.cs` | Paramètres audio/graphismes/sauvegarde |
| `GameSceneManager` | `Managers/GameSceneManager.cs` | Transitions de scène |

Tous suivent le pattern singleton : `if (Instance != null && Instance != this) Destroy(gameObject)`.

---

*Ce document est vivant. Il sera mis à jour au fur et à mesure de la conception.*
