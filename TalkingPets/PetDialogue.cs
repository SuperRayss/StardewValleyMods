using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using System.Linq;

namespace TalkingPets
{
    public class PetDialogue
    {
        private static Dictionary<string, string> catDialogue_;
        private static Dictionary<string, string> dogDialogue_;
        private static Dictionary<string, string> bothDialogue_;

        internal static Dictionary<string, Dictionary<string, string>> petDialogue = null;
        internal static List<string> petNames = null;

        private static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        // TODO: Put json content in mod folder so I don't have to use content patcher at all (after gift tastes are migrated too)
        // Should also be able to call this during Initialize if the file exists already, though make sure LoadDialogue doesn't require an xnb
        internal static void LoadDialogueJSON()
        {
            catDialogue_ = LoadDialogue("Characters\\Dialogue\\Pet\\Cat");
            dogDialogue_ = LoadDialogue("Characters\\Dialogue\\Pet\\Dog");
            bothDialogue_ = LoadDialogue("Characters\\Dialogue\\Pet\\Both");
        }

        internal static void SetDialogue()
        {
            //Monitor.Log($"Loading petDialogue", LogLevel.Debug); // DEBUG
            petDialogue = new();
            petNames = new();
            var catDialogue = new Stack<Tuple<string, string>>();
            var dogDialogue = new Stack<Tuple<string, string>>();
            var bothDialogue = new Stack<Tuple<string, string>>();
            foreach (var entry in catDialogue_)
            {
                catDialogue.Push(new(entry.Key, entry.Value));
            }
            foreach (var entry in dogDialogue_)
            {
                dogDialogue.Push(new(entry.Key, entry.Value));
            }
            foreach (var entry in bothDialogue_)
            {
                bothDialogue.Push(new(entry.Key, entry.Value));
            }
            List<Pet> pets = GetPetList();
            while (catDialogue.Count > 0 || dogDialogue.Count > 0 || bothDialogue.Count > 0)
            {
                // TODO: Make sure that repeated key conflicts don't result in an indefinite hangup
                // Ideally just don't write any repeated keys within an animal type (or between the type and both)
                foreach (Pet pet in pets)
                {
                    string name = pet.Name;
                    if (!petDialogue.ContainsKey(name)) {
                        petDialogue.Add(name, new Dictionary<string, string>());
                        petNames.Add(name);
                    }
                    if (pet is Cat && catDialogue.Count > 0)
                    {
                        var entry = catDialogue.Peek();
                        if (petDialogue[name].TryAdd(entry.Item1, entry.Item2))
                            catDialogue.Pop();
                    }
                    else if (pet is Dog && dogDialogue.Count > 0)
                    {
                        var entry = dogDialogue.Peek();
                        if (petDialogue[name].TryAdd(entry.Item1, entry.Item2))
                            dogDialogue.Pop();
                    }
                    else if (bothDialogue.Count > 0)
                    {
                        var entry = bothDialogue.Peek();
                        if (petDialogue[name].TryAdd(entry.Item1, entry.Item2))
                            bothDialogue.Pop();
                    }
                }
            }
        }

        private static List<Pet> GetPetList()
        {
            List<Pet> pets = new();
            foreach (NPC i in Game1.getFarm().characters)
            {
                if (i is Pet)
                {
                    pets.Add(i as Pet);
                }
            }
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                foreach (NPC j in Utility.getHomeOfFarmer(farmer).characters)
                {
                    if (j is Pet)
                    {
                        pets.Add(j as Pet);
                    }
                }
            }
            return pets;
        }

        private static Dictionary<string, string> LoadDialogue(string dialogue_file)
        {
            return Game1.content.Load<Dictionary<string, string>>(dialogue_file)
                .Select((KeyValuePair<string, string> pair) =>
                {
                    string key = pair.Key;
                    string text = pair.Value;
                    if (text.Contains("¦"))
                    {
                        if (Game1.player.IsMale)
                        {
                            text = text.Substring(0, text.IndexOf("¦"));
                        }
                        else
                        {
                            text = text.Substring(text.IndexOf("¦") + 1);
                        }
                    }
                    return new KeyValuePair<string, string>(key, text);
                })
                .ToDictionary((KeyValuePair<string, string> p) => p.Key, (KeyValuePair<string, string> p) => p.Value);
        }
    }

}
