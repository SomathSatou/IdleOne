using System;
using System.Collections.Generic;

namespace Assets.Script.Entities
{
    /// <summary>
    /// Paire clé-valeur sérialisable pour contourner la limitation de JsonUtility
    /// qui ne supporte pas les Dictionary.
    /// Utilisé pour sérialiser ShapeStock et ColorStock de PlayerInventory.
    /// GDD §16.4 — Données Sauvegardées.
    /// </summary>
    [Serializable]
    public class SerializableKeyValue
    {
        /// <summary>Clé de la paire (ex : nom de forme ou de couleur).</summary>
        public string Key;

        /// <summary>Valeur associée (ex : quantité en stock).</summary>
        public int Value;

        public SerializableKeyValue() { }

        public SerializableKeyValue(string key, int value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Convertit un Dictionary en liste sérialisable.
        /// </summary>
        public static List<SerializableKeyValue> FromDictionary(Dictionary<string, int> dictionary)
        {
            var list = new List<SerializableKeyValue>();
            if (dictionary == null) return list;

            foreach (var kvp in dictionary)
            {
                list.Add(new SerializableKeyValue(kvp.Key, kvp.Value));
            }
            return list;
        }

        /// <summary>
        /// Reconstruit un Dictionary à partir d'une liste sérialisée.
        /// </summary>
        public static Dictionary<string, int> ToDictionary(List<SerializableKeyValue> list)
        {
            var dictionary = new Dictionary<string, int>();
            if (list == null) return dictionary;

            foreach (var item in list)
            {
                if (item != null && item.Key != null)
                {
                    dictionary[item.Key] = item.Value;
                }
            }
            return dictionary;
        }
    }
}
