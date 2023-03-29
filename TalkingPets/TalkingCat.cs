using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using StardewValley;
using StardewValley.Characters;
using StardewValley.BellsAndWhistles;
using Netcode;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;

namespace TalkingPets
{
    public class TalkingCat : Cat
    {

        public override bool canTalk()
        {
            return true;
        }

        // NOTE: Not strictly needed, but may be useful later
        public override bool CanSocialize
        {
            get
            {
                return true;
            }
        }

        public override void tryToReceiveActiveObject(Farmer who)
        {
            who.Halt();
            who.faceGeneralDirection(base.getStandingPosition(), 0, false, false);
            if (who.ActiveObject != null && this.Dialogue.ContainsKey("reject_" + who.ActiveObject.ParentSheetIndex.ToString()))
            {
                this.setNewDialogue(this.Dialogue["reject_" + who.ActiveObject.ParentSheetIndex.ToString()], false, false);
                Game1.drawDialogue(this);
            }
            else if (Game1.NPCGiftTastes.ContainsKey(this.Name))
            {
                foreach (string s in who.activeDialogueEvents.Keys)
                {
                    if (s.Contains("dumped") && this.Dialogue.ContainsKey(s))
                    {
                        base.doEmote(12, true);
                        return;
                    }
                }
                if ((who.friendshipData.ContainsKey(this.displayName) && who.friendshipData[this.displayName].GiftsThisWeek < 2) || this.isBirthday(Game1.currentSeason, Game1.dayOfMonth))
                {
                    if (who.friendshipData[this.displayName].GiftsToday == 1)
                    {
                        Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3981", this.displayName)));
                        return;
                    }
                    this.receiveGift(who.ActiveObject, who, true, 1f, true);
                    who.reduceActiveItemByOne();
                    who.completelyStopAnimatingOrDoingAction();
                    base.faceTowardFarmerForPeriod(4000, 3, false, who);
                }
                else
                {
                    Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3987", this.displayName, 2)));
                }
            }
        }

        public new void receiveGift(StardewValley.Object o, Farmer giver, bool updateGiftLimitInfo = true, float friendshipChangeMultiplier = 1f, bool showResponse = true)
        {
            Game1.NPCGiftTastes.TryGetValue(this.Name, out var value);
            string[] array = value.Split('/');
            float num = 1f;
            switch (o.Quality)
            {
                case 1:
                    num = 1.1f;
                    break;
                case 2:
                    num = 1.25f;
                    break;
                case 4:
                    num = 1.5f;
                    break;
            }

            if (Birthday_Season != null && Game1.currentSeason.Equals(Birthday_Season) && Game1.dayOfMonth == Birthday_Day)
            {
                friendshipChangeMultiplier = 8f;
                string text = ((Manners == 2) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4274") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4275"));
                if (Game1.random.NextDouble() < 0.5)
                {
                    text = ((Manners == 2) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4276") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4277"));
                }

                string text2 = ((Manners == 2) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4278") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4279"));
                array[0] = text;
                array[2] = text;
                array[4] = text2;
                array[6] = text2;
                array[8] = ((Manners == 2) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4280") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4281"));
            }

            giver?.onGiftGiven(this, o);
            if (value == null)
            {
                return;
            }

            Game1.stats.GiftsGiven++;
            giver.currentLocation.localSound("give_gift");
            if (updateGiftLimitInfo)
            {
                giver.friendshipData[this.displayName].GiftsToday++;
                giver.friendshipData[this.displayName].GiftsThisWeek++;
                giver.friendshipData[this.displayName].LastGiftDate = new WorldDate(Game1.Date);
            }

            int giftTasteForThisItem = getGiftTasteForThisItem(o);
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

            List<string> list = new();
            for (int i = 0; i < 8; i += 2)
            {
                list.Add(array[i]);
            }

            switch (giftTasteForThisItem)
            {
                case 0:
                    Game1.drawDialogue(this, list[0] + "$h");
                    giver.changeFriendship((int)(80f * friendshipChangeMultiplier * num), this);
                    doEmote(20);
                    faceTowardFarmerForPeriod(15000, 4, faceAway: false, giver);
                    return;
                case 6:
                    if (showResponse)
                        Game1.drawDialogue(this, list[3] + "$s");
                    giver.changeFriendship((int)(-40f * friendshipChangeMultiplier), this);
                    faceTowardFarmerForPeriod(15000, 4, faceAway: true, giver);
                    doEmote(12);
                    return;
                case 2:
                    if (showResponse)
                        Game1.drawDialogue(this, list[1] + "$h");
                    giver.changeFriendship((int)(45f * friendshipChangeMultiplier * num), this);
                    faceTowardFarmerForPeriod(7000, 3, faceAway: true, giver);
                    return;
                case 4:
                    if (showResponse)
                        Game1.drawDialogue(this, list[2] + "$s");
                    giver.changeFriendship((int)(-20f * friendshipChangeMultiplier), this);
                    return;
            }
            if (showResponse)
            {
                Game1.drawDialogue(this, array[8]);
            }
            giver.changeFriendship((int)(20f * friendshipChangeMultiplier), this);
        }

        public override bool checkAction(Farmer who, GameLocation l)
        {
            if (this.IsInvisible)
            {
                return false;
            }
            if (this.isSleeping.Value)
            {
                if (!this.isEmoting)
                {
                    base.doEmote(24, true);
                }
                this.shake(250);
                return false;
            }
            if (!who.CanMove)
            {
                return false;
            }
            if (Game1.NPCGiftTastes.ContainsKey(this.Name) && !Game1.player.friendshipData.ContainsKey(this.displayName))
            {
                Game1.player.friendshipData.Add(this.displayName, new Friendship(0));
            }
            if (who.checkForQuestComplete(this, -1, -1, who.ActiveObject, null, -1, 5))
            {
                base.faceTowardFarmerForPeriod(6000, 3, false, who);
                return base.checkAction(who, l);
            }
            bool newCurrentDialogue = false;
            if (who.friendshipData.ContainsKey(this.displayName))
            {
                newCurrentDialogue = this.checkForNewCurrentDialogue(who.friendshipData.ContainsKey(this.displayName) ? (who.friendshipData[this.displayName].Points / 250) : 0, false);
                if (!newCurrentDialogue)
                {
                    newCurrentDialogue = this.checkForNewCurrentDialogue(who.friendshipData.ContainsKey(this.displayName) ? (who.friendshipData[this.displayName].Points / 250) : 0, true);
                }
            }
            if (who.IsLocalPlayer && who.friendshipData.ContainsKey(this.displayName) && (this.endOfRouteMessage.Value != null || newCurrentDialogue || (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))))
            {
                if (!newCurrentDialogue && this.setTemporaryMessages(who))
                {
                    Game1.player.checkForQuestComplete(this, -1, -1, null, null, 5, -1);
                    return false;
                }
                if (this.Sprite.Texture.Bounds.Height > 32)
                {
                    base.faceTowardFarmerForPeriod(5000, 4, false, who);
                }
                if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse())
                {
                    this.tryToReceiveActiveObject(who);
                    base.faceTowardFarmerForPeriod(3000, 4, false, who);
                    return base.checkAction(who, l);
                }
                this.grantConversationFriendship(who, 20);
                Game1.drawDialogue(this);
                return base.checkAction(who, l);
            }
            else
            {
                if (this.canTalk() && this.CurrentDialogue.Count > 0)
                {
                    if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse())
                    {
                        if (who.IsLocalPlayer)
                        {
                            this.tryToReceiveActiveObject(who);
                        }
                        else
                        {
                            base.faceTowardFarmerForPeriod(3000, 4, false, who);
                        }
                        return base.checkAction(who, l);
                    }
                    if (this.CurrentDialogue.Count >= 1 || this.endOfRouteMessage.Value != null || (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this)))
                    {
                        if (this.setTemporaryMessages(who))
                        {
                            Game1.player.checkForQuestComplete(this, -1, -1, null, null, 5, -1);
                            return false;
                        }
                        if (this.Sprite.Texture.Bounds.Height > 32)
                        {
                            base.faceTowardFarmerForPeriod(5000, 4, false, who);
                        }
                        if (who.IsLocalPlayer)
                        {
                            this.grantConversationFriendship(who, 20);
                            Game1.drawDialogue(this);
                            return base.checkAction(who, l);
                        }
                    }
                    else if (!doingEndOfRouteAnimation)
                    {
                        try
                        {
                            if (who.friendshipData.ContainsKey(this.displayName))
                            {
                                base.faceTowardFarmerForPeriod(who.friendshipData[this.displayName].Points / 125 * 1000 + 1000, 4, false, who);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        if (Game1.random.NextDouble() < 0.1)
                        {
                            base.doEmote(8, true);
                        }
                    }
                }
                else if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse())
                {
                    this.tryToReceiveActiveObject(who);
                    base.faceTowardFarmerForPeriod(3000, 4, false, who);
                    return base.checkAction(who, l);
                }
                if (this.setTemporaryMessages(who))
                {
                    return false;
                }
                if ((doingEndOfRouteAnimation || !goingToDoEndOfRouteAnimation) && this.endOfRouteMessage.Value != null)
                {
                    Game1.drawDialogue(this);
                    return base.checkAction(who, l);
                }
                return false;
            }
        }


    }

}
