﻿using System;
using System.Collections.Generic;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;

namespace TalkingPets
{
    internal sealed class ModEntry : Mod
    {
        private List<Pet> originalPets;
        private List<Pet> talkingPets;
        private List<Tuple<GameLocation, Vector2>> talkingPetLocations;
        private Farm farm;

        private Stack<string> dialogues = new();

        public override void Entry(IModHelper helper)
        {
            dialogues.Push("DEBUGGING!");
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            for (int i = 0; i < originalPets.Count; i++)
            {
                talkingPetLocations[i] = new Tuple<GameLocation, Vector2>(
                    talkingPets[i].currentLocation, talkingPets[i].getTileLocation()
                );
                //originalPets[i].currentLocation = farm;
                //Game1.warpCharacter(originalPets[i], farm, new Vector2(0, 0));
                originalPets[i].currentLocation = talkingPetLocations[i].Item1;
                Game1.removeThisCharacterFromAllLocations(talkingPets[i]);
                Game1.warpCharacter(originalPets[i], talkingPetLocations[i].Item1, talkingPetLocations[i].Item2);
                originalPets[i].friendshipTowardFarmer = talkingPets[i].friendshipTowardFarmer;
                originalPets[i].grantedFriendshipForPet = talkingPets[i].grantedFriendshipForPet;
                originalPets[i].lastPetDay = talkingPets[i].lastPetDay;
            }
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            talkingPetLocations = new List<Tuple<GameLocation, Vector2>>();
            farm = Game1.getFarm();
            // TODO: Figure out how to handle when pets are added during the game
            // Currently works fine with the standard animals, it just won't turn them to talking animals (it won't consider them at all in originalPets)
            originalPets = GetAllOriginalPets();
            talkingPets = new List<Pet>();
            for (int i = 0; i < originalPets.Count; i++)
            {
                if (originalPets[i] is Dog)
                {
                    talkingPets.Add(new TalkingDog());
                }
                else if (originalPets[i] is Cat)
                {
                    talkingPets.Add(new TalkingCat());
                }
                originalPets[i].DeepCloneTo(talkingPets[i]);
                talkingPetLocations.Add(new Tuple<GameLocation, Vector2>(
                    originalPets[i].currentLocation, originalPets[i].getTileLocation()
                ));
                //Monitor.Log(talkingPetLocations[i].Item2.ToString(), LogLevel.Debug);
            }
            if (talkingPets.Count > 0) 
                AddDialogue();
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // TODO: Add a system for removing dogs, currently doesn't work since originalPets is never updated
            // Look into using a hashMap system instead
            for (int i = 0; i < originalPets.Count; i++)
            {
                Game1.removeThisCharacterFromAllLocations(originalPets[i]);
                //Monitor.Log(talkingPetLocations[i].Item1.Name, LogLevel.Debug);
                talkingPets[i].currentLocation = talkingPetLocations[i].Item1;
                Game1.warpCharacter(talkingPets[i], talkingPetLocations[i].Item1, talkingPetLocations[i].Item2);
            }
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            // TODO: Garbage collection(?)
        }

        private void AddDialogue()
        {
            int counter = 0;
            while (dialogues.Count > 0)
            {
                talkingPets[counter].CurrentDialogue.Push(new Dialogue(dialogues.Pop(), talkingPets[counter])); // DEBUG
                counter++;
                if (counter == talkingPets.Count)
                    counter = 0;
            }
        }

        internal List<Pet> GetAllOriginalPets()
        {
            List<Pet> pets = new();
            foreach (NPC i in farm.characters)
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


    }
}

// Important methods for talking

// NPC.checkAction() [overriden by Pet]
// NPC.canTalk() [overriden by Pet]