# AGENTS.md — IdleOne

## Projet

IdleOne est un jeu idle/incremental de breeding de créatures. Ce fichier référence les trois agents IA qui collaborent sur le projet.

## Agents disponibles

| Agent | Fichier | Rôle |
|---|---|---|
| **Concepteur** | `agents/agent-concepteur.md` | Design, documentation, amélioration des specs |
| **Développeur** | `agents/agent-developpeur.md` | Implémentation des features |
| **Testeur** | `agents/agent-testeur.md` | Tests unitaires et procédures de test |

## Système de balises collaboratives

Les collaborateurs humains peuvent insérer des balises dans les fichiers de documentation pour déclencher une analyse par l'agent Concepteur :

| Balise | Auteur | Type |
|---|---|---|
| `:S:` | **Somath** | Suggestion |
| `:P:` | **Prozengan** | Problème |
| `:A:` | **Akoia** | Amélioration |
| `:L:` | **Libebulle** | Question |
| `:IA:` | **Agent IA** | Amélioration proposée par IA |

**Règle stricte :** Les balises `:S:`, `:P:`, `:A:` et `:L:` sont **réservées aux collaborateurs humains**. Aucun agent IA ne doit jamais les générer. Si un agent veut proposer une amélioration, il utilise exclusivement `:IA:`.

Quand l'agent Concepteur rencontre une balise `:S:`, `:P:`, `:A:`, `:L:` ou `:IA:`, il doit la traiter comme un prompt et proposer des améliorations documentées.

## Workflow général

```
Collaborateur écrit dans la doc (avec balises :S: :P: :A: :L:)
        │
        ▼
  Agent Concepteur → Analyse les balises, améliore la documentation
        │
        ▼
  Agent Développeur → Implémente les features décrites dans la doc
        │
        ▼
  Agent Testeur → Écrit les tests unitaires et les procédures de test
```

## Structure du projet

```
IdleOne/
├── AGENTS.md              ← Ce fichier
├── agents/
│   ├── agent-concepteur.md
│   ├── agent-developpeur.md
│   └── agent-testeur.md
├── GDD.md                 ← Game Design Document
├── README.md
└── ...
```
