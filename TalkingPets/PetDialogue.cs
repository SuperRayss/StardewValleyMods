using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using System.Linq;

namespace TalkingPets
{
    internal class PetDialogue
    {
        private static Dictionary<string, string> catDialogue_;
        private static Dictionary<string, string> dogDialogue_;
        private static Dictionary<string, string> bothDialogue_;

        internal static Dictionary<string, Dictionary<string, string>> petDialogue = null;

        private static IMonitor Monitor;

        internal static void Initialize(IMonitor monitor)
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
            petDialogue = new();
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
                var catCount = catDialogue.Count;
                var dogCount = dogDialogue.Count;
                var bothCount = bothDialogue.Count;
                foreach (Pet pet in pets)
                {
                    string name = pet.Name;
                    if (!petDialogue.ContainsKey(name)) {
                        petDialogue.Add(name, new Dictionary<string, string>());
                        Game1.NPCGiftTastes.Add(name, Game1.NPCGiftTastes["_" + pet.GetType().Name]);
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
                // Stops an infinite loop from happening if no pet can allocate any more dialogue, but more dialogue exists
                // Such as in the case of having only one pet type, so the other pet type never exhausts its dialogue stack
                // Should also prevent conflicts if dialogue_both.json and dialogue_(type).json have the same key
                // There may exist a possible bug with this if there's only one pet where the loop exits before allocating 
                // all dialogue, but I need to test further for that
                if ( 
                    catDialogue.Count == catCount && 
                    dogDialogue.Count == dogCount && 
                    bothDialogue.Count == bothCount
                    )
                {
                    break;
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
                            text = text[..text.IndexOf("¦")];
                        }
                        else
                        {
                            text = text[(text.IndexOf("¦") + 1)..];
                        }
                    }
                    return new KeyValuePair<string, string>(key, text);
                })
                .ToDictionary((KeyValuePair<string, string> p) => p.Key, (KeyValuePair<string, string> p) => p.Value);
        }

    }

}
