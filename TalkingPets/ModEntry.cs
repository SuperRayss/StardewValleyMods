using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using HarmonyLib;
using System.Reflection;
using System.Xml.Linq;
using System.Linq;

namespace TalkingPets
{
    internal sealed class ModEntry : Mod
    {
        private Dictionary<string, Dictionary<string, string>> petDialogues = null;
        private List<string> petNames = null;

        // TODO: There are currently bugs:
        // 1. Gift tastes seem out of whack for some types
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            NPCPatches.Initialize(Monitor);
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        /// <inheritdoc cref="IContentEvents.AssetRequested"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            
            if (!e.Name.StartsWith("Characters\\Dialogue\\Pet")) { return; }
            string[] split = e.NameWithoutLocale.BaseName.Split("/");
            string characterName = split[split.Length - 1].Trim();
            if (petDialogues == null) {
                LoadPetDialogues();
            }
            //Monitor.Log(characterName, LogLevel.Debug); // DEBUG
            if (petDialogues.TryGetValue(characterName, out Dictionary<string, string> dialogue))
            {
                //Monitor.Log($"Trying to load dialogue for {characterName}", LogLevel.Debug); // DEBUG
                e.LoadFrom(() => dialogue, AssetLoadPriority.Medium);
                //Monitor.Log($"Loaded dialogue for {characterName}", LogLevel.Debug); // DEBUG
            }

        }
        private void LoadPetDialogues()
        {
            //Monitor.Log($"Loading petDialogues", LogLevel.Debug); // DEBUG
            petDialogues = new();
            petNames = new();
            List<Pet> pets = GetPetList();
            Stack<Tuple<string, string>> catDialogues = new(); // TODO: Load in dialogues from assets/src/dialogue_x.json
            catDialogues.Push(new("Mon", "THE FOG IS COMING but I sure do love being a cat on a nice Monday"));
            catDialogues.Push(new("Tue", "THE FOG IS COMING but I sure do love being a cat on a nice Tuesday"));
            catDialogues.Push(new("Wed", "THE FOG IS COMING but I sure do love being a cat on a nice Wednesday"));
            catDialogues.Push(new("Thu", "THE FOG IS COMING but I sure do love being a cat on a nice Thursday"));
            catDialogues.Push(new("Fri", "THE FOG IS COMING but I sure do love being a cat on a nice Friday"));
            catDialogues.Push(new("Sat", "THE FOG IS COMING but I sure do love being a cat on a nice Saturday"));
            catDialogues.Push(new("Sun", "THE FOG IS COMING but I sure do love being a cat on a nice Sunday"));
            Stack<Tuple<string, string>> dogDialogues = new();
            dogDialogues.Push(new("Mon", "THE FOG IS COMING but I sure do love being a dog on a nice Monday"));
            dogDialogues.Push(new("Tue", "THE FOG IS COMING but I sure do love being a dog on a nice Tuesday"));
            dogDialogues.Push(new("Wed", "THE FOG IS COMING but I sure do love being a dog on a nice Wednesday"));
            dogDialogues.Push(new("Thu", "THE FOG IS COMING but I sure do love being a dog on a nice Thursday"));
            dogDialogues.Push(new("Fri", "THE FOG IS COMING but I sure do love being a dog on a nice Friday"));
            dogDialogues.Push(new("Sat", "THE FOG IS COMING but I sure do love being a dog on a nice Saturday"));
            dogDialogues.Push(new("Sun", "THE FOG IS COMING but I sure do love being a dog on a nice Sunday"));
            Stack<Tuple<string, string>> bothDialogues = new();
            bothDialogues.Push(new("Mon", "THE FOG IS COMING... But on a Monday"));
            bothDialogues.Push(new("Tue", "THE FOG IS COMING... But on a Tuesday"));
            bothDialogues.Push(new("Wed", "THE FOG IS COMING... But on a Wednesday"));
            bothDialogues.Push(new("Thu", "THE FOG IS COMING... But on a Thursday"));
            bothDialogues.Push(new("Fri", "THE FOG IS COMING... But on a Friday"));
            bothDialogues.Push(new("Sat", "THE FOG IS COMING... But on a Saturday"));
            bothDialogues.Push(new("Sun", "THE FOG IS COMING... But on a Sunday"));
            while (catDialogues.Count > 0 || dogDialogues.Count > 0 || bothDialogues.Count > 0)
            {
                // TODO: Make sure that repeated key conflicts don't result in a forever hangup
                foreach (Pet pet in pets)
                {
                    string name = pet.Name.Trim();
                    if (!petDialogues.ContainsKey(name)) {
                        petDialogues.Add(name, new Dictionary<string, string>());
                        petNames.Add(pet.Name);
                    }
                    if (pet is Cat && catDialogues.Count > 0)
                    {
                        var entry = catDialogues.Peek();
                        if (petDialogues[name].TryAdd(entry.Item1, entry.Item2))
                            catDialogues.Pop();
                    }
                    else if (pet is Dog && dogDialogues.Count > 0)
                    {
                        var entry = dogDialogues.Peek();
                        if (petDialogues[name].TryAdd(entry.Item1, entry.Item2))
                            dogDialogues.Pop();
                    }
                    else if (bothDialogues.Count > 0)
                    {
                        var entry = bothDialogues.Peek();
                        if (petDialogues[name].TryAdd(entry.Item1, entry.Item2))
                            bothDialogues.Pop();
                    }
                }
            }

            
        }


        // <NPC xsi:type="Cat"><name>GUY </name>
        // <NPC xsi:type="Dog"><name>REAL</name>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var api = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            api.RegisterToken(this.ModManifest, "PetNames", () =>
            {
                if (petNames?.Count > 0)
                {
                    return petNames;
                }
                return null;
            });
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            petNames = null;
            petDialogues = null;
        }

        internal static List<Pet> GetPetList()
        {
            List<Pet> pets = new();
            foreach (NPC i in Game1.getFarm().characters)
            {
                if (i is Pet)
                {
                    // TODO: Create new JSON file with i.Name (overwrite if exists)
                    pets.Add(i as Pet);
                }
            }
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                foreach (NPC j in Utility.getHomeOfFarmer(farmer).characters)
                {
                    if (j is Pet)
                    {
                        // TODO: Create new JSON file with j.Name (overwrite if exists)
                        pets.Add(j as Pet);
                    }
                }
            }
            return pets;
        }
/*
        internal void LoadPetNames()
        {
            petNames = new();
            foreach (NPC i in Game1.getFarm().characters)
            {
                if (i is Pet)
                {
                    // TODO: Create new JSON file with i.Name (overwrite if exists)
                    petNames.Add(i.Name);
                    //Monitor.Log(i.Name, LogLevel.Debug);
                }
            }
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                foreach (NPC j in Utility.getHomeOfFarmer(farmer).characters)
                {
                    if (j is Pet)
                    {
                        // TODO: Create new JSON file with j.Name (overwrite if exists)
                        petNames.Add(j.Name);
                        //Monitor.Log(j.Name, LogLevel.Debug);
                    }
                }
            }
        }
*/
/*        internal List<string> GetPetTypes()
        {
            List<string> petTypes = new();
            foreach (NPC i in Game1.getFarm().characters)
            {
                if (i is Pet)
                {
                    if (i is Cat) petTypes.Add("cat");
                    else petTypes.Add("dog");
                    //Monitor.Log(petTypes[petTypes.Count - 1], LogLevel.Debug);
                }
            }
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                foreach (NPC j in Utility.getHomeOfFarmer(farmer).characters)
                {
                    if (j is Pet)
                    {
                        if (j is Cat) petTypes.Add("cat");
                        else petTypes.Add("dog");
                        //Monitor.Log(petTypes[petTypes.Count - 1], LogLevel.Debug);
                    }
                }
            }
            return petTypes;
        }
*/
    }

    public class DataModel
    {
        //public Dictionary<string, string> Entries = new Dictionary<string, string>();
        public string test = "yes";

    }
}
