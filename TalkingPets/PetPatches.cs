using HarmonyLib;
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
        public static bool Prefix(NPC __instance, ref bool __result)
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
        public static bool Prefix(NPC __instance, Farmer who, GameLocation l, ref bool __result)
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
                    else if (!__instance.doingEndOfRouteAnimation)
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
                if ((__instance.doingEndOfRouteAnimation || !__instance.goingToDoEndOfRouteAnimation) && __instance.endOfRouteMessage.Value != null)
                {
                    Game1.drawDialogue(__instance);
                    return true;
                }
                __result = false;
                return false;
            }
        }

    }

}
