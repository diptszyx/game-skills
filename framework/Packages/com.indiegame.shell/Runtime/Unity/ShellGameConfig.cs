using System;
using System.Collections.Generic;
using UnityEngine;

namespace IndieGame.Shell.Unity
{
    [Serializable]
    public sealed class BoosterCount
    {
        public string id = "";
        public int count;
    }

    [Serializable]
    public sealed class ShellGameConfig
    {
        public string gameId = "";
        public int initialGold = 100;
        public int maxHearts = 5;
        public int initialHearts = 5;
        public int heartSeconds = 1800;
        public int checkInGold = 25;
        public int questGold = 10;
        public int winGold = 20;
        public int reviveGold = 50;
        public int shopGold = 50;
        public int shopPrice = 30;
        public List<BoosterCount> initialBoosters = new List<BoosterCount>();
        public string[] languages = { "en", "vi" };

        public static ShellGameConfig Parse(string json)
        {
            if(string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("Invalid ShellGameConfig json: expected an object");
            // Overwrite a constructed instance so omitted fields retain their defaults.
            var game = new ShellGameConfig();
            try { JsonUtility.FromJsonOverwrite(json, game); }
            catch(ArgumentException ex) { throw new InvalidOperationException("Invalid ShellGameConfig json: " + ex.Message, ex); }
            game.Validate();
            return game;
        }

        internal IReadOnlyDictionary<string,int> InitialBoosterCounts()
        {
            var counts = new Dictionary<string,int>(StringComparer.Ordinal);
            foreach(var entry in initialBoosters) counts[entry.id] = entry.count;
            return counts;
        }

        internal void Validate()
        {
            var bad = new List<string>();
            if(string.IsNullOrWhiteSpace(gameId)) bad.Add(nameof(gameId));
            if(initialGold < 0) bad.Add(nameof(initialGold));
            if(maxHearts < 1) bad.Add(nameof(maxHearts));
            if(initialHearts < 0 || initialHearts > maxHearts) bad.Add(nameof(initialHearts));
            if(heartSeconds < 1) bad.Add(nameof(heartSeconds));
            if(checkInGold < 0) bad.Add(nameof(checkInGold));
            if(questGold < 0) bad.Add(nameof(questGold));
            if(winGold < 0) bad.Add(nameof(winGold));
            if(reviveGold < 1) bad.Add(nameof(reviveGold));
            if(shopGold < 0) bad.Add(nameof(shopGold));
            if(shopPrice < 1) bad.Add(nameof(shopPrice));
            if(initialBoosters == null) bad.Add(nameof(initialBoosters));
            else
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach(var entry in initialBoosters)
                    if(entry == null || string.IsNullOrWhiteSpace(entry.id) || entry.count < 0 || !seen.Add(entry.id)) { bad.Add(nameof(initialBoosters)); break; }
            }
            if(languages == null || languages.Length == 0) bad.Add(nameof(languages));
            else foreach(var language in languages)
                if(string.IsNullOrWhiteSpace(language)) { bad.Add(nameof(languages)); break; }
            if(bad.Count > 0)
                throw new InvalidOperationException("Invalid ShellGameConfig fields: " + string.Join(", ", bad));
        }
    }

    public static class ShellDefaults
    {
        public const string RevivePlacement = "revive";
        public const string DoublePlacement = "double";
    }
}
