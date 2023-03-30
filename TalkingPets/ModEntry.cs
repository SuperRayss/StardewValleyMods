using System;
using System.Collections.Generic;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Threading;

namespace TalkingPets
{
    internal sealed class ModEntry : Mod
    {
        private Dictionary<string, string> catDialogues; // TODO: Load in dialogues from assets/dialogue_x.json
        private Dictionary<string, string> dogDialogues;
        private Dictionary<string, string> bothDialogues;
        private List<string> petNames;
/*        private List<string> petTypes;
*/


        // TODO: There are currently bugs:
        // 1. Gift tastes seem out of whack for some types
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            NPCPatches.Initialize(Monitor);
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            /*            IContentPack temp = this.Helper.ContentPacks.CreateTemporary(
                           directoryPath: Path.Combine(this.Helper.DirectoryPath, "content-pack"),
                           id: Guid.NewGuid().ToString("N"),
                           name: "temporary Talking Pets Content Pack",
                           description: "BARK BARK RUFF RUFF MEEEEOOOOWWWW content",
                           author: "SuperRayss",
                           version: new SemanticVersion(1, 0, 0)
                        );
                        foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
                        {
                            if (contentPack.Manifest.UniqueID == "SuperRayss.TalkingPetsContentPack")
                            {
                                contentPack.DirectoryPath
                                YourDataModel data = contentPack.ReadJsonFile<YourDataFile>("content.json");
                                return;
                            }
                        }*/
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var api = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            api.RegisterToken(this.ModManifest, "PetNames", () =>
            {
                if (petNames?.Count > 0) {
                    return petNames;
                }
                return null;
            });
            // TODO: Address this problem where repeated values are ignored
            // This is why it was saying there's no entries for 3 and 4, because it was 1 cat and 3 dogs, so it just saw 1 cat and 1 dog
            // We need to use a custom token that uses key-value storage for name-type
            // Actually we can fix this problem by going back to what I wanted to do originally, create a custom JSON for each indivual animal
            // So long as I create the JSON here, it doesn't need to be loaded as a content pack, just referred to by token
/*            api.RegisterToken(this.ModManifest, "PetTypes", () =>
            {
*//*                if (petTypes?.Count > 0 && petTypes.Count == petNames.Count) {
                    Monitor.Log(String.Join(", ", petTypes), LogLevel.Debug);
                    return petTypes;
                }*//*

                return new string[] { "dog", "dog", "dog", "dog" };

            });*/
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            
            petNames = GetPetNames();
/*            petTypes = GetPetTypes();
*//*            List<Pet> pets = BuildPetList();
            if (pets.Count > 0)
                CreateNamedJSON(pets);*/
        }

        // TODO: Change this to feed in from JSON files
        // Rotate the assignment of dialogue
        // Build out new JSON files with the Character.Name of each pet to be loaded in via content pack API
        private void CreateNamedJSON(List<Pet> pets)
        {
            // TODO: Optimize this terrible way of iterating through the dialogues
            for (int i = 0, counter = 0; i < catDialogues.Count; i++) // For each dialogue, we're apending to a named JSON file for this cat
            {
                if (pets[counter] is Cat)
                {
                    // TODO: Append catDialogues[i] to pets[counter].Name JSON
                }
                counter++;
                if (counter == pets.Count)
                    counter = 0;
            }
            for (int i = 0, counter = 0; i < dogDialogues.Count; i++)
            {
                if (pets[counter] is Dog)
                {
                    // TODO: Append dogDialogues[i] to the pets[counter].Name JSON
                }
                counter++;
                if (counter == pets.Count)
                    counter = 0;
            }
            for (int i = 0, counter = 0; i < bothDialogues.Count; i++)
            {
                // TODO: Append bothDialogues[i] to pets[counter].Name JSON
                counter++;
                if (counter == pets.Count)
                    counter = 0;
            }
        }

        internal static List<Pet> BuildPetList()
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

        internal List<string> GetPetNames()
        {
            List<string> petNames = new();
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
            return petNames;
        }

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
}
