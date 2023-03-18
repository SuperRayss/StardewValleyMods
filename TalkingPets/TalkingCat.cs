using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StardewValley;
using StardewValley.Characters;

namespace TalkingPets
{
    public class TalkingCat : Cat
    {
        new string Name
        {
            //get { return base.Name.Trim(); }
            get { return "Cat"; }
        }

        public override bool canTalk()
        {
            return true;
        }

        public new Dictionary<string, string> Dialogue
        {
            get
            {
                string dialogue_file = "Characters\\Dialogue\\" + this.Name;
                try
                {
                    return Game1.content.Load<Dictionary<string, string>>(dialogue_file).Select(
                        (KeyValuePair<string, string> pair) => {
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
                        }
                    ).ToDictionary((KeyValuePair<string, string> p) => p.Key, (KeyValuePair<string, string> p) => p.Value);
                }
                catch (ContentLoadException) { }
                return new Dictionary<string, string>();
            }
        }

        public override void tryToReceiveActiveObject(Farmer who)
        {
            who.Halt();
            who.faceGeneralDirection(base.getStandingPosition(), 0, false, false);
            //Game1.drawDialogueNoTyping(this, $"{this.Name} {Game1.NPCGiftTastes.ContainsKey("Penny")} {Game1.NPCGiftTastes.ContainsKey(base.Name.Trim())}");
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
                if ((who.friendshipData.ContainsKey(this.Name) && who.friendshipData[this.Name].GiftsThisWeek < 2) || this.isBirthday(Game1.currentSeason, Game1.dayOfMonth))
                {
                    if (who.friendshipData[this.Name].GiftsToday == 1)
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
                giver.friendshipData[this.Name].GiftsToday++;
                giver.friendshipData[this.Name].GiftsThisWeek++;
                giver.friendshipData[this.Name].LastGiftDate = new WorldDate(Game1.Date);
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

        public new int getGiftTasteForThisItem(Item item)
        {
            int num = 8;
            if (item is StardewValley.Object)
            {
                StardewValley.Object @object = item as StardewValley.Object;
                Game1.NPCGiftTastes.TryGetValue(this.Name, out var value);
                string[] array = value.Split('/');
                int parentSheetIndex = @object.ParentSheetIndex;
                int category = @object.Category;
                string value2 = parentSheetIndex.ToString() ?? "";
                string value3 = category.ToString() ?? "";
                if (Game1.NPCGiftTastes["Universal_Love"].Split(' ').Contains(value3))
                {
                    num = 0;
                }
                else if (Game1.NPCGiftTastes["Universal_Hate"].Split(' ').Contains(value3))
                {
                    num = 6;
                }
                else if (Game1.NPCGiftTastes["Universal_Like"].Split(' ').Contains(value3))
                {
                    num = 2;
                }
                else if (Game1.NPCGiftTastes["Universal_Dislike"].Split(' ').Contains(value3))
                {
                    num = 4;
                }

                if (CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Love"].Split(' ')))
                {
                    num = 0;
                }
                else if (CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Hate"].Split(' ')))
                {
                    num = 6;
                }
                else if (CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Like"].Split(' ')))
                {
                    num = 2;
                }
                else if (CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Dislike"].Split(' ')))
                {
                    num = 4;
                }

                bool flag = false;
                bool flag2 = false;
                if (Game1.NPCGiftTastes["Universal_Love"].Split(' ').Contains(value2))
                {
                    num = 0;
                    flag = true;
                }
                else if (Game1.NPCGiftTastes["Universal_Hate"].Split(' ').Contains(value2))
                {
                    num = 6;
                    flag = true;
                }
                else if (Game1.NPCGiftTastes["Universal_Like"].Split(' ').Contains(value2))
                {
                    num = 2;
                    flag = true;
                }
                else if (Game1.NPCGiftTastes["Universal_Dislike"].Split(' ').Contains(value2))
                {
                    num = 4;
                    flag = true;
                }
                else if (Game1.NPCGiftTastes["Universal_Neutral"].Split(' ').Contains(value2))
                {
                    num = 8;
                    flag = true;
                    flag2 = true;
                }

                if (@object.Type.Contains("Arch"))
                {
                    num = 4;
                    if (this.Name.Equals("Penny") || this.Name.Equals("Dwarf"))
                    {
                        num = 2;
                    }
                }

                if (num == 8 && !flag2)
                {
                    if ((int)@object.Edibility != -300 && (int)@object.Edibility < 0)
                    {
                        num = 6;
                    }
                    else if ((int)@object.Price < 20)
                    {
                        num = 4;
                    }
                }

                if (value != null)
                {
                    List<string[]> list = new();
                    for (int i = 0; i < 10; i += 2)
                    {
                        string[] array2 = array[i + 1].Split(' ');
                        string[] array3 = new string[array2.Length];
                        for (int j = 0; j < array2.Length; j++)
                        {
                            if (array2[j].Length > 0)
                            {
                                array3[j] = array2[j];
                            }
                        }

                        list.Add(array3);
                    }

                    if (list[0].Contains(value2))
                    {
                        return 0;
                    }

                    if (list[3].Contains(value2))
                    {
                        return 6;
                    }

                    if (list[1].Contains(value2))
                    {
                        return 2;
                    }

                    if (list[2].Contains(value2))
                    {
                        return 4;
                    }

                    if (list[4].Contains(value2))
                    {
                        return 8;
                    }

                    if (CheckTasteContextTags(item, list[0]))
                    {
                        return 0;
                    }

                    if (CheckTasteContextTags(item, list[3]))
                    {
                        return 6;
                    }

                    if (CheckTasteContextTags(item, list[1]))
                    {
                        return 2;
                    }

                    if (CheckTasteContextTags(item, list[2]))
                    {
                        return 4;
                    }

                    if (CheckTasteContextTags(item, list[4]))
                    {
                        return 8;
                    }

                    if (!flag)
                    {
                        if (category != 0 && list[0].Contains(value3))
                        {
                            return 0;
                        }

                        if (category != 0 && list[3].Contains(value3))
                        {
                            return 6;
                        }

                        if (category != 0 && list[1].Contains(value3))
                        {
                            return 2;
                        }

                        if (category != 0 && list[2].Contains(value3))
                        {
                            return 4;
                        }

                        if (category != 0 && list[4].Contains(value3))
                        {
                            return 8;
                        }
                    }
                }
            }

            return num;
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
            if (Game1.NPCGiftTastes.ContainsKey(this.Name) && !Game1.player.friendshipData.ContainsKey(this.Name))
            {
                Game1.player.friendshipData.Add(this.Name, new Friendship(0));
            }
            if (who.checkForQuestComplete(this, -1, -1, who.ActiveObject, null, -1, 5))
            {
                base.faceTowardFarmerForPeriod(6000, 3, false, who);
                return base.checkAction(who, l);
            }
            bool newCurrentDialogue = false;
            if (who.friendshipData.ContainsKey(this.Name))
            {
                newCurrentDialogue = this.checkForNewCurrentDialogue(who.friendshipData.ContainsKey(this.Name) ? (who.friendshipData[this.Name].Points / 250) : 0, false);
                if (!newCurrentDialogue)
                {
                    newCurrentDialogue = this.checkForNewCurrentDialogue(who.friendshipData.ContainsKey(this.Name) ? (who.friendshipData[this.Name].Points / 250) : 0, true);
                }
            }
            if (who.IsLocalPlayer && who.friendshipData.ContainsKey(this.Name) && (this.endOfRouteMessage.Value != null || newCurrentDialogue || (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))))
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
                            //Game1.drawDialogueNoTyping(this, "tryToReceiveActiveObject2");
                            this.tryToReceiveActiveObject(who); // This is what's going on currently if they have dialogue left
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
                            if (who.friendshipData.ContainsKey(this.Name))
                            {
                                base.faceTowardFarmerForPeriod(who.friendshipData[this.Name].Points / 125 * 1000 + 1000, 4, false, who);
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

        public new bool checkForNewCurrentDialogue(int heartLevel, bool noPreface = false)
        {
            foreach (string key in Game1.player.activeDialogueEvents.Keys)
            {
                if (Dialogue.ContainsKey(key))
                {
                    string text = key;
                    if (!text.Equals("") && !Game1.player.mailReceived.Contains(this.Name + "_" + text))
                    {
                        CurrentDialogue.Clear();
                        CurrentDialogue.Push(new Dialogue(Dialogue[text], this));
                        if (!key.Contains("dumped"))
                        {
                            Game1.player.mailReceived.Add(this.Name + "_" + text);
                        }

                        return true;
                    }
                }
            }
            string text2 = ((!Game1.currentSeason.Equals("spring") && !noPreface) ? Game1.currentSeason : "");
            if (Dialogue.ContainsKey(string.Concat(text2, Game1.currentLocation.Name, "_", getTileX().ToString(), "_", getTileY().ToString())))
            {
                CurrentDialogue.Push(new Dialogue(Dialogue[string.Concat(text2, Game1.currentLocation.Name, "_", getTileX().ToString(), "_", getTileY().ToString())], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }

            if (Dialogue.ContainsKey(text2 + (string)Game1.currentLocation.Name + "_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
            {
                CurrentDialogue.Push(new Dialogue(Dialogue[text2 + (string)Game1.currentLocation.Name + "_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }

            if (heartLevel >= 10 && Dialogue.ContainsKey(text2 + (string)Game1.currentLocation.Name + "10"))
            {
                CurrentDialogue.Push(new Dialogue(Dialogue[text2 + (string)Game1.currentLocation.Name + "10"], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }

            if (heartLevel >= 8 && Dialogue.ContainsKey(text2 + (string)Game1.currentLocation.Name + "8"))
            {
                CurrentDialogue.Push(new Dialogue(Dialogue[text2 + (string)Game1.currentLocation.Name + "8"], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }

            if (heartLevel >= 6 && Dialogue.ContainsKey(text2 + (string)Game1.currentLocation.Name + "6"))
            {
                CurrentDialogue.Push(new Dialogue(Dialogue[text2 + (string)Game1.currentLocation.Name + "6"], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }

            if (heartLevel >= 4 && Dialogue.ContainsKey(text2 + (string)Game1.currentLocation.Name + "4"))
            {
                CurrentDialogue.Push(new Dialogue(Dialogue[text2 + (string)Game1.currentLocation.Name + "4"], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }

            if (heartLevel >= 2 && Dialogue.ContainsKey(text2 + (string)Game1.currentLocation.Name + "2"))
            {
                CurrentDialogue.Push(new Dialogue(Dialogue[text2 + (string)Game1.currentLocation.Name + "2"], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }

            if (Dialogue.ContainsKey(text2 + (string)Game1.currentLocation.Name))
            {
                CurrentDialogue.Push(new Dialogue(Dialogue[text2 + (string)Game1.currentLocation.Name], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }

            return false;
        }

    }

}
