using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;

namespace TalkingPets
{
    // TODO: Instead of modifying the original serializer, create a mod-only serializer that's not considered in regular loading
    /**
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            // TODO: Create process that ensures uninstalling the mod will not remove original animals, aka save the talking animals to the originals every time
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            new XmlSerializer(typeof(SaveGame), new Type[]
            {
                typeof(TalkingDog),
                typeof(TalkingCat)
            }).DeepCloneTo(SaveGame.serializer);
            new XmlSerializer(typeof(GameLocation), new Type[]
            {
                typeof(TalkingDog),
                typeof(TalkingCat)
            }).DeepCloneTo(SaveGame.locationSerializer);
        }

        // Duplicates talking pet data to base pet data, meaning you can uninstall the mod and retain your data
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Farm farm = Game1.getFarm();
            List<Pet> talkingPets = GetAllTalkingPets();
            for (int i = 0; i < talkingPets.Count; i++)
            {
                if (talkingPets[i] is Dog)
                {
                    SaveGame.save
                }
                else if (originalPets[i] is Cat)
                {
                    talkingPets.Add(new TalkingCat());
                }
                originalPets[i].DeepCloneTo(talkingPets[i]);
                // TODO: Replace placeholder dialogue with real dialogue loader akin to regular NPCs
                talkingPets[i].CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3990"), talkingPets[i]));
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            Farm farm = Game1.getFarm();
            Point DogPosPoint = farm.GetPetStartLocation(); // Keeping here in case water bowl is moved day-to-day
            List<Pet> originalPets = GetAllOriginalPets();
            for (int i = 0; i < originalPets.Count; i++)
            {
                Monitor.Log(originalPets[i].Name, LogLevel.Debug);
                Pet talkingPet = new();
                if (originalPets[i] is Dog)
                {
                    talkingPet = originalPets[i].DeepCloneTo(new TalkingDog());
                }
                else if (originalPets[i] is Cat)
                {
                    talkingPet = originalPets[i].DeepCloneTo(new TalkingCat());
                }
                // TODO: Replace placeholder dialogue with real dialogue loader akin to regular NPCs
                //talkingPet.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3990"), talkingPet));
                Game1.removeThisCharacterFromAllLocations(originalPets[i]);
                talkingPet.currentLocation = farm;
                Game1.warpCharacter(talkingPet, farm, DogPosPoint.ToVector2());
            }
        }

        // Will only get base class pets that are currently on the farm
        public static List<Pet> GetAllOriginalPets()
        {
            List<Pet> pets = new List<Pet>();
            foreach (NPC i in Game1.getFarm().characters)
            {
                if (i.GetType() == typeof(Dog) || i.GetType() == typeof(Cat))
                {
                    pets.Add(i as Pet);
                }
            }
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                foreach (NPC j in Utility.getHomeOfFarmer(farmer).characters)
                {
                    if (j.GetType() == typeof(Dog) || j.GetType() == typeof(Cat))
                    {
                        pets.Add(j as Pet);
                    }
                }
            }
            return pets;
        }

        // Will only get new class pets that are currently on the farm
        public static List<Pet> GetAllTalkingPets()
        {
            List<Pet> pets = new List<Pet>();
            foreach (NPC i in Game1.getFarm().characters)
            {
                if (i.GetType() == typeof(TalkingDog) || i.GetType() == typeof(TalkingCat))
                {
                    pets.Add(i as Pet);
                }
            }
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                foreach (NPC j in Utility.getHomeOfFarmer(farmer).characters)
                {
                    if (j.GetType() == typeof(TalkingDog) || j.GetType() == typeof(TalkingCat))
                    {
                        pets.Add(j as Pet);
                    }
                }
            }
            return pets;
        }

    }
    */
}

// Important methods for talking

// NPC.checkAction() [overriden by Pet]
// NPC.canTalk() [overriden by Pet]