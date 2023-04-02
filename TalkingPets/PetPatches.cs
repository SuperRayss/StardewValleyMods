using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using System;

namespace TalkingPets
{
    [HarmonyPatch(typeof(Pet))]
    [HarmonyPatch("canTalk")]
    public class PetPatches_canTalk
    {

        // Prefix patch for canTalk()
        public static bool Prefix(Pet __instance, ref bool __result)
        {
            __result = true;
            return false;
        }

    }

    [HarmonyPatch(typeof(Pet))]
    [HarmonyPatch("checkAction")]
    public class PetPatches_checkAction
    {

        // Prefix patch for checkAction()
        // TODO: Add friendship towards farmer for giving gifts

        private static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static bool Prefix(Pet __instance, Farmer who, GameLocation l, ref bool __result)
        {
            if (__instance.IsInvisible)
            {
                return false;
            }
            if (__instance.isSleeping.Value)
            {
                if (!__instance.isEmoting)
                {
                    __instance.doEmote(24, true);
                }
                __instance.shake(250);
                __result = false;
                return false;
            }
            if (!who.CanMove)
            {
                __result = false;
                return false;
            }
            if (Game1.NPCGiftTastes.ContainsKey(__instance.Name) && !Game1.player.friendshipData.ContainsKey(__instance.Name))
            {
                Game1.player.friendshipData.Add(__instance.Name, new Friendship(0));
            }
            if (who.checkForQuestComplete(__instance, -1, -1, who.ActiveObject, null, -1, 5))
            {
                __instance.faceTowardFarmerForPeriod(6000, 3, false, who);
                return true;
            }
            bool newCurrentDialogue = false;
            if (who.friendshipData.ContainsKey(__instance.Name))
            {
                newCurrentDialogue = __instance.checkForNewCurrentDialogue(who.friendshipData.ContainsKey(__instance.Name) ? (who.friendshipData[__instance.Name].Points / 250) : 0, false);
                if (!newCurrentDialogue)
                {
                    newCurrentDialogue = __instance.checkForNewCurrentDialogue(who.friendshipData.ContainsKey(__instance.Name) ? (who.friendshipData[__instance.Name].Points / 250) : 0, true);
                }
            }
            if (who.IsLocalPlayer && who.friendshipData.ContainsKey(__instance.Name) && (__instance.endOfRouteMessage.Value != null || newCurrentDialogue || (__instance.currentLocation != null && __instance.currentLocation.HasLocationOverrideDialogue(__instance))))
            {
                if (!newCurrentDialogue && __instance.setTemporaryMessages(who))
                {
                    Game1.player.checkForQuestComplete(__instance, -1, -1, null, null, 5, -1);
                    __result = false;
                    return false;
                }
                if (__instance.Sprite.Texture.Bounds.Height > 32)
                {
                    __instance.faceTowardFarmerForPeriod(5000, 4, false, who);
                }
                if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse())
                {
                    __instance.tryToReceiveActiveObject(who);
                    __instance.faceTowardFarmerForPeriod(3000, 4, false, who);
                    return true;
                }
                __instance.grantConversationFriendship(who, 20);
                Game1.drawDialogue(__instance);
                return true;
            }
            else
            {
                if (__instance.canTalk() && __instance.CurrentDialogue.Count > 0)
                {
                    if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse())
                    {
                        if (who.IsLocalPlayer)
                        {
                            __instance.tryToReceiveActiveObject(who);
                        }
                        else
                        {
                            __instance.faceTowardFarmerForPeriod(3000, 4, false, who);
                        }
                        return true;
                    }
                    if (__instance.CurrentDialogue.Count >= 1 || __instance.endOfRouteMessage.Value != null || (__instance.currentLocation != null && __instance.currentLocation.HasLocationOverrideDialogue(__instance)))
                    {
                        if (__instance.setTemporaryMessages(who))
                        {
                            Game1.player.checkForQuestComplete(__instance, -1, -1, null, null, 5, -1);
                            __result = false;
                            return false;
                        }
                        if (__instance.Sprite.Texture.Bounds.Height > 32)
                        {
                            __instance.faceTowardFarmerForPeriod(5000, 4, false, who);
                        }
                        if (who.IsLocalPlayer)
                        {
                            __instance.grantConversationFriendship(who, 20);
                            Game1.drawDialogue(__instance);
                            return true;
                        }
                    }
                    else if (!__instance.doingEndOfRouteAnimation.Value)
                    {
                        try
                        {
                            if (who.friendshipData.ContainsKey(__instance.Name))
                            {
                                __instance.faceTowardFarmerForPeriod(who.friendshipData[__instance.Name].Points / 125 * 1000 + 1000, 4, false, who);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        if (Game1.random.NextDouble() < 0.1)
                        {
                            __instance.doEmote(8, true);
                        }
                    }
                }
                else if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse())
                {
                    __instance.tryToReceiveActiveObject(who);
                    __instance.faceTowardFarmerForPeriod(3000, 4, false, who);
                    return true;
                }
                if (__instance.setTemporaryMessages(who))
                {
                    __result = false;
                    return false;
                }
                if ((__instance.doingEndOfRouteAnimation.Value || !__instance.goingToDoEndOfRouteAnimation.Value) && __instance.endOfRouteMessage.Value != null)
                {
                    Game1.drawDialogue(__instance);
                    return true;
                }
                __result = false;
                return false;
            }
        }

    }

/*    [HarmonyPatch(typeof(NPC))]
    [HarmonyPatch("tryToReceiveActiveObject")]
    public class PetPatches_tryToReceiveActiveObject
    {

        private static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        // Prefix patch for tryToReceiveActiveObject()
        // TODO: Figure out how to apply only to Pet objects natively
        public static bool Prefix(NPC __instance, Farmer who)
        {
            if (__instance is not Pet) return true;
            Monitor.Log($"PetPatches_tryToReceiveActiveObject", LogLevel.Debug); // DEBUG
            who.Halt();
            who.faceGeneralDirection(__instance.getStandingPosition(), 0, false, false);
            if (who.ActiveObject != null && __instance.Dialogue.ContainsKey("reject_" + who.ActiveObject.ParentSheetIndex.ToString()))
            {
                __instance.setNewDialogue(__instance.Dialogue["reject_" + who.ActiveObject.ParentSheetIndex.ToString()], false, false);
                Game1.drawDialogue(__instance);
                return false;
            }
            string petType = "_" + __instance.GetType().Name;
            if (Game1.NPCGiftTastes.ContainsKey(petType))
            {
                foreach (string s in who.activeDialogueEvents.Keys)
                {
                    if (s.Contains("dumped") && __instance.Dialogue.ContainsKey(s))
                    {
                        __instance.doEmote(12, true);
                        return false;
                    }
                }
                who.completeQuest(25);
                if ((who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].GiftsThisWeek < 2) || (who.spouse != null && who.spouse.Equals(__instance.Name)) || __instance is Child || __instance.isBirthday(Game1.currentSeason, Game1.dayOfMonth))
                {
                    if (who.friendshipData[__instance.Name].GiftsToday == 1)
                    {
                        Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3981", __instance.displayName)));
                        return false;
                    }
                    __instance.receiveGift(who.ActiveObject, who, true, 1f, true);
                    who.reduceActiveItemByOne();
                    who.completelyStopAnimatingOrDoingAction();
                    __instance.faceTowardFarmerForPeriod(4000, 3, false, who);
                }
                else
                {
                    Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3987", __instance.displayName, 2)));
                }
            }
            return false;
        }

    }

    [HarmonyPatch(typeof(NPC))]
    [HarmonyPatch("receiveGift")]
    public class PetPatches_receiveGift
    {

        private static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        // Prefix patch for receiveGift()
        // TODO: Figure out how to apply only to Pet objects natively
        public static bool Prefix(NPC __instance, StardewValley.Object o, Farmer giver, bool updateGiftLimitInfo = true, float friendshipChangeMultiplier = 1f, bool showResponse = true)
        {
            if (__instance is not Pet) return true;
            Monitor.Log($"PetPatches_receiveGift", LogLevel.Debug); // DEBUG
            string petType = "_" + __instance.GetType().Name;
            Game1.NPCGiftTastes.TryGetValue(petType, out string NPCLikes);
            string[] split = NPCLikes.Split('/', StringSplitOptions.None);
            float qualityChangeMultipler = 1f;
            switch (o.Quality)
            {
                case 1:
                    qualityChangeMultipler = 1.1f;
                    break;
                case 2:
                    qualityChangeMultipler = 1.25f;
                    break;
                case 4:
                    qualityChangeMultipler = 1.5f;
                    break;
            }
            if (__instance.Birthday_Season != null && Game1.currentSeason.Equals(__instance.Birthday_Season) && Game1.dayOfMonth == __instance.Birthday_Day)
            {
                friendshipChangeMultiplier = 8f;
                string positiveBirthdayMessage = (__instance.Manners == 2) ? Game1.LoadStringByGender(__instance.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4274") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4275");
                if (Game1.random.NextDouble() < 0.5)
                {
                    positiveBirthdayMessage = ((__instance.Manners == 2) ? Game1.LoadStringByGender(__instance.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4276") : Game1.LoadStringByGender(__instance.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4277"));
                }
                string negativeBirthdayMessage = (__instance.Manners == 2) ? Game1.LoadStringByGender(__instance.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4278") : Game1.LoadStringByGender(__instance.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4279");
                split[0] = positiveBirthdayMessage;
                split[2] = positiveBirthdayMessage;
                split[4] = negativeBirthdayMessage;
                split[6] = negativeBirthdayMessage;
                split[8] = ((__instance.Manners == 2) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4280") : Game1.LoadStringByGender(__instance.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4281"));
            }
            if (giver != null)
            {
                giver.onGiftGiven(__instance, o);
            }
            if (NPCLikes != null)
            {
                Stats stats = Game1.stats;
                uint giftsGiven = stats.GiftsGiven;
                stats.GiftsGiven = giftsGiven + 1U;
                giver.currentLocation.localSound("give_gift");
                if (updateGiftLimitInfo)
                {
                    Friendship friendship = giver.friendshipData[__instance.Name];
                    int num = friendship.GiftsToday;
                    friendship.GiftsToday = num + 1;
                    Friendship friendship2 = giver.friendshipData[__instance.Name];
                    num = friendship2.GiftsThisWeek;
                    friendship2.GiftsThisWeek = num + 1;
                    giver.friendshipData[__instance.Name].LastGiftDate = new WorldDate(Game1.Date);
                }
                int tasteForItem = __instance.getGiftTasteForThisItem(o);
                switch (giver.FacingDirection)
                {
                    case 0:
                        ((FarmerSprite)giver.Sprite).animateBackwardsOnce(80, 50f);
                        break;
                    case 1:
                        ((FarmerSprite)giver.Sprite).animateBackwardsOnce(72, 50f);
                        break;
                    case 2:
                        ((FarmerSprite)giver.Sprite).animateBackwardsOnce(64, 50f);
                        break;
                    case 3:
                        ((FarmerSprite)giver.Sprite).animateBackwardsOnce(88, 50f);
                        break;
                }
                List<string> reactions = new List<string>();
                for (int i = 0; i < 8; i += 2)
                {
                    reactions.Add(split[i]);
                }
                if (tasteForItem == 0)
                {
                    if (showResponse)
                    {
                        Game1.drawDialogue(__instance, reactions[0] + "$h");
                    }
                    giver.changeFriendship((int)(80f * friendshipChangeMultiplier * qualityChangeMultipler), __instance);
                    __instance.doEmote(20, true);
                    __instance.faceTowardFarmerForPeriod(15000, 4, false, giver);
                    return false;
                }
                if (tasteForItem == 6)
                {
                    if (showResponse)
                    {
                        Game1.drawDialogue(__instance, reactions[3] + "$s");
                    }
                    giver.changeFriendship((int)(-40f * friendshipChangeMultiplier), __instance);
                    __instance.faceTowardFarmerForPeriod(15000, 4, true, giver);
                    __instance.doEmote(12, true);
                    return false;
                }
                if (tasteForItem == 2)
                {
                    if (showResponse)
                    {
                        Game1.drawDialogue(__instance, reactions[1] + "$h");
                    }
                    giver.changeFriendship((int)(45f * friendshipChangeMultiplier * qualityChangeMultipler), __instance);
                    __instance.faceTowardFarmerForPeriod(7000, 3, true, giver);
                    return false;
                }
                if (tasteForItem == 4)
                {
                    if (showResponse)
                    {
                        Game1.drawDialogue(__instance, reactions[2] + "$s");
                    }
                    giver.changeFriendship((int)(-20f * friendshipChangeMultiplier), __instance);
                    return false;
                }
                if (showResponse)
                {
                    Game1.drawDialogue(__instance, split[8]);
                }
                giver.changeFriendship((int)(20f * friendshipChangeMultiplier), __instance);
            }
            return false;
        }

    }


    [HarmonyPatch(typeof(NPC))]
    [HarmonyPatch("getGiftTasteForThisItem")]
    public class PetPatches_getGiftTasteForThisItem
    {

        private static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        // Prefix patch for getGiftTasteForThisItem()
        public static bool Prefix(NPC __instance, Item item, ref int __result)
        {
            if (__instance is not Pet) return true;
            Monitor.Log($"PetPatches_getGiftTasteForThisItem", LogLevel.Debug); // DEBUG
            int tasteForItem = 8;
            if (item is StardewValley.Object)
            {
                StardewValley.Object o = item as StardewValley.Object;
                Game1.NPCGiftTastes.TryGetValue(__instance.Name, out string NPCLikes);
                Monitor.Log($"\'{ __instance.Name}\'", LogLevel.Debug);
                *//*                
                Monitor.Log(Game1.NPCGiftTastes.TryGetValue("Pet_Jon Boy ", out string test1).ToString(), LogLevel.Debug);
                Monitor.Log(Game1.NPCGiftTastes.TryGetValue("Pet_Jon Boy", out string test2).ToString(), LogLevel.Debug);
                Monitor.Log(Game1.NPCGiftTastes.TryGetValue("Pet_Jon", out string test3).ToString(), LogLevel.Debug);
                Monitor.Log(Game1.NPCGiftTastes.TryGetValue("Jon Boy", out string test4).ToString(), LogLevel.Debug);
                Monitor.Log(Game1.NPCGiftTastes.TryGetValue(__instance.Name, out string test5).ToString(), LogLevel.Debug);
                Monitor.Log(Game1.NPCGiftTastes.Keys.Join(null, " "), LogLevel.Debug);
                *//*
                string[] split = NPCLikes.Split('/', StringSplitOptions.None);
                int itemNumber = o.ParentSheetIndex;
                int categoryNumber = o.Category;
                string itemNumberString = itemNumber.ToString() ?? "";
                string categoryNumberString = categoryNumber.ToString() ?? "";
                if (Game1.NPCGiftTastes["Universal_Love"].Split(' ', StringSplitOptions.None).Contains(categoryNumberString))
                {
                    tasteForItem = 0;
                }
                else if (Game1.NPCGiftTastes["Universal_Hate"].Split(' ', StringSplitOptions.None).Contains(categoryNumberString))
                {
                    tasteForItem = 6;
                }
                else if (Game1.NPCGiftTastes["Universal_Like"].Split(' ', StringSplitOptions.None).Contains(categoryNumberString))
                {
                    tasteForItem = 2;
                }
                else if (Game1.NPCGiftTastes["Universal_Dislike"].Split(' ', StringSplitOptions.None).Contains(categoryNumberString))
                {
                    tasteForItem = 4;
                }
                if (__instance.CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Love"].Split(' ', StringSplitOptions.None)))
                {
                    tasteForItem = 0;
                }
                else if (__instance.CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Hate"].Split(' ', StringSplitOptions.None)))
                {
                    tasteForItem = 6;
                }
                else if (__instance.CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Like"].Split(' ', StringSplitOptions.None)))
                {
                    tasteForItem = 2;
                }
                else if (__instance.CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Dislike"].Split(' ', StringSplitOptions.None)))
                {
                    tasteForItem = 4;
                }
                bool wasIndividualUniversal = false;
                bool skipDefaultValueRules = false;
                if (Game1.NPCGiftTastes["Universal_Love"].Split(' ', StringSplitOptions.None).Contains(itemNumberString))
                {
                    tasteForItem = 0;
                    wasIndividualUniversal = true;
                }
                else if (Game1.NPCGiftTastes["Universal_Hate"].Split(' ', StringSplitOptions.None).Contains(itemNumberString))
                {
                    tasteForItem = 6;
                    wasIndividualUniversal = true;
                }
                else if (Game1.NPCGiftTastes["Universal_Like"].Split(' ', StringSplitOptions.None).Contains(itemNumberString))
                {
                    tasteForItem = 2;
                    wasIndividualUniversal = true;
                }
                else if (Game1.NPCGiftTastes["Universal_Dislike"].Split(' ', StringSplitOptions.None).Contains(itemNumberString))
                {
                    tasteForItem = 4;
                    wasIndividualUniversal = true;
                }
                else if (Game1.NPCGiftTastes["Universal_Neutral"].Split(' ', StringSplitOptions.None).Contains(itemNumberString))
                {
                    tasteForItem = 8;
                    wasIndividualUniversal = true;
                    skipDefaultValueRules = true;
                }
                if (o.Type.Contains("Arch"))
                {
                    tasteForItem = 4;
                }
                if (tasteForItem == 8 && !skipDefaultValueRules)
                {
                    if (o.Edibility != -300 && o.Edibility < 0)
                    {
                        tasteForItem = 6;
                    }
                    else if (o.Price < 20)
                    {
                        tasteForItem = 4;
                    }
                }
                if (NPCLikes != null)
                {
                    List<string[]> items = new();
                    for (int i = 0; i < 10; i += 2)
                    {
                        string[] splitItems = split[i + 1].Split(' ', StringSplitOptions.None);
                        string[] thisItems = new string[splitItems.Length];
                        for (int j = 0; j < splitItems.Length; j++)
                        {
                            if (splitItems[j].Length > 0)
                            {
                                thisItems[j] = splitItems[j];
                            }
                        }
                        items.Add(thisItems);
                    }
                    if (items[0].Contains(itemNumberString))
                    {
                        tasteForItem = 0;
                    } else if (items[3].Contains(itemNumberString))
                    {
                        tasteForItem = 6;
                    } else if (items[1].Contains(itemNumberString))
                    {
                        tasteForItem = 2;
                    } else if (items[2].Contains(itemNumberString))
                    {
                        tasteForItem = 4;
                    } else if (items[4].Contains(itemNumberString))
                    {
                        tasteForItem = 8;
                    } else if (__instance.CheckTasteContextTags(item, items[0]))
                    {
                        tasteForItem = 0;
                    } else if (__instance.CheckTasteContextTags(item, items[3]))
                    {
                        tasteForItem = 6;
                    } else if (__instance.CheckTasteContextTags(item, items[1]))
                    {
                        tasteForItem = 2;
                    } else if (__instance.CheckTasteContextTags(item, items[2]))
                    {
                        tasteForItem = 4;
                    } else if (__instance.CheckTasteContextTags(item, items[4]))
                    {
                        tasteForItem = 8;
                    } else if (!wasIndividualUniversal)
                    {
                        if (categoryNumber != 0 && items[0].Contains(categoryNumberString))
                        {
                            tasteForItem = 0;
                        } else if (categoryNumber != 0 && items[3].Contains(categoryNumberString))
                        {
                            tasteForItem = 6;
                        } else if (categoryNumber != 0 && items[1].Contains(categoryNumberString))
                        {
                            tasteForItem = 2;
                        } else if (categoryNumber != 0 && items[2].Contains(categoryNumberString))
                        {
                            tasteForItem = 4;
                        } else if (categoryNumber != 0 && items[4].Contains(categoryNumberString))
                        {
                            tasteForItem = 8;
                        }
                    }
                }
            }
            __result = tasteForItem;
            return false;
        }

    }*/

}
