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

        new string Name
        {
            //get { return base.Name.Trim(); }
            get { return "Cat"; }
        }

        public void drawDialogue()
        {
            if (this.CurrentDialogue.Count != 0)
            {
                Game1.activeClickableMenu = new DialogueBox(this.CurrentDialogue.Peek());
                Game1.dialogueUp = true;
                if (!Game1.eventUp)
                {
                    Game1.player.Halt();
                    Game1.player.CanMove = false;
                }
                Game1.currentSpeaker = this;
            }
        }

        public void drawDialogue(string dialogue)
        {
            this.CurrentDialogue.Push(new Dialogue(dialogue, this));
            drawDialogue();
        }

        public void drawDialogue(string dialogue, Texture2D overridePortrait)
        {
            this.CurrentDialogue.Push(new Dialogue(dialogue, this)
            {
                overridePortrait = overridePortrait
            });
            drawDialogue();
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

        public new Stack<Dialogue> CurrentDialogue
        {
            get
            {
                Stack<Dialogue> currentDialogue = null;
                if (Game1.npcDialogues == null)
                {
                    Game1.npcDialogues = new Dictionary<string, Stack<Dialogue>>();
                }
                Game1.npcDialogues.TryGetValue(this.Name, out currentDialogue);
                if (currentDialogue == null)
                {
                    currentDialogue = (Game1.npcDialogues[this.Name] = this.loadCurrentDialogue());
                }
                return currentDialogue;
            }
            set
            {
                if (Game1.npcDialogues != null)
                {
                    Game1.npcDialogues[this.Name] = value;
                }
            }
        }

        public override void tryToReceiveActiveObject(Farmer who)
        {
            who.Halt();
            who.faceGeneralDirection(base.getStandingPosition(), 0, false, false);
            if (who.ActiveObject != null && this.Dialogue.ContainsKey("reject_" + who.ActiveObject.ParentSheetIndex.ToString()))
            {
                this.setNewDialogue(this.Dialogue["reject_" + who.ActiveObject.ParentSheetIndex.ToString()], false, false);
                drawDialogue();
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
                if ((who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].GiftsThisWeek < 2) || this.isBirthday(Game1.currentSeason, Game1.dayOfMonth))
                {
                    if (who.friendshipData[base.Name].GiftsToday == 1)
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
                giver.friendshipData[base.Name].GiftsToday++;
                giver.friendshipData[base.Name].GiftsThisWeek++;
                giver.friendshipData[base.Name].LastGiftDate = new WorldDate(Game1.Date);
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
                    drawDialogue(list[0] + "$h");
                    giver.changeFriendship((int)(80f * friendshipChangeMultiplier * num), this);
                    doEmote(20);
                    faceTowardFarmerForPeriod(15000, 4, faceAway: false, giver);
                    return;
                case 6:
                    if (showResponse)
                        drawDialogue(list[3] + "$s");
                    giver.changeFriendship((int)(-40f * friendshipChangeMultiplier), this);
                    faceTowardFarmerForPeriod(15000, 4, faceAway: true, giver);
                    doEmote(12);
                    return;
                case 2:
                    if (showResponse)
                        drawDialogue(list[1] + "$h");
                    giver.changeFriendship((int)(45f * friendshipChangeMultiplier * num), this);
                    faceTowardFarmerForPeriod(7000, 3, faceAway: true, giver);
                    return;
                case 4:
                    if (showResponse)
                        drawDialogue(list[2] + "$s");
                    giver.changeFriendship((int)(-20f * friendshipChangeMultiplier), this);
                    return;
            }
            if (showResponse)
            {
                drawDialogue(array[8]);
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
            if (Game1.NPCGiftTastes.ContainsKey(this.Name) && !Game1.player.friendshipData.ContainsKey(base.Name))
            {
                Game1.player.friendshipData.Add(base.Name, new Friendship(0));
            }
            if (who.checkForQuestComplete(this, -1, -1, who.ActiveObject, null, -1, 5))
            {
                base.faceTowardFarmerForPeriod(6000, 3, false, who);
                return base.checkAction(who, l);
            }
            bool newCurrentDialogue = false;
            if (who.friendshipData.ContainsKey(base.Name))
            {
                newCurrentDialogue = this.checkForNewCurrentDialogue(who.friendshipData.ContainsKey(base.Name) ? (who.friendshipData[base.Name].Points / 250) : 0, false);
                if (!newCurrentDialogue)
                {
                    newCurrentDialogue = this.checkForNewCurrentDialogue(who.friendshipData.ContainsKey(base.Name) ? (who.friendshipData[base.Name].Points / 250) : 0, true);
                }
            }
            if (who.IsLocalPlayer && who.friendshipData.ContainsKey(base.Name) && (this.endOfRouteMessage.Value != null || newCurrentDialogue || (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))))
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
                drawDialogue();
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
                            drawDialogue();
                            return base.checkAction(who, l);
                        }
                    }
                    else if (!doingEndOfRouteAnimation)
                    {
                        try
                        {
                            if (who.friendshipData.ContainsKey(base.Name))
                            {
                                base.faceTowardFarmerForPeriod(who.friendshipData[base.Name].Points / 125 * 1000 + 1000, 4, false, who);
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
                    drawDialogue();
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
            string preface = ((!Game1.currentSeason.Equals("spring") && !noPreface) ? Game1.currentSeason : "");
            if (this.Dialogue.ContainsKey(string.Concat(new string[]
            {
                preface,
                Game1.currentLocation.name,
                "_",
                base.getTileX().ToString(),
                "_",
                base.getTileY().ToString()
            })))
            {
                this.CurrentDialogue.Push(new Dialogue(this.Dialogue[string.Concat(new string[]
                {
                    preface,
                    Game1.currentLocation.name,
                    "_",
                    base.getTileX().ToString(),
                    "_",
                    base.getTileY().ToString()
                })], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }
            if (this.Dialogue.ContainsKey(preface + Game1.currentLocation.name + "_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
            {
                this.CurrentDialogue.Push(new Dialogue(this.Dialogue[preface + Game1.currentLocation.name + "_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }
            if (heartLevel >= 10 && this.Dialogue.ContainsKey(preface + Game1.currentLocation.name + "10"))
            {
                this.CurrentDialogue.Push(new Dialogue(this.Dialogue[preface + Game1.currentLocation.name + "10"], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }
            if (heartLevel >= 8 && this.Dialogue.ContainsKey(preface + Game1.currentLocation.name + "8"))
            {
                this.CurrentDialogue.Push(new Dialogue(this.Dialogue[preface + Game1.currentLocation.name + "8"], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }
            if (heartLevel >= 6 && this.Dialogue.ContainsKey(preface + Game1.currentLocation.name + "6"))
            {
                this.CurrentDialogue.Push(new Dialogue(this.Dialogue[preface + Game1.currentLocation.name + "6"], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }
            if (heartLevel >= 4 && this.Dialogue.ContainsKey(preface + Game1.currentLocation.name + "4"))
            {
                this.CurrentDialogue.Push(new Dialogue(this.Dialogue[preface + Game1.currentLocation.name + "4"], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }
            if (heartLevel >= 2 && this.Dialogue.ContainsKey(preface + Game1.currentLocation.name + "2"))
            {
                this.CurrentDialogue.Push(new Dialogue(this.Dialogue[preface + Game1.currentLocation.name + "2"], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }
            if (this.Dialogue.ContainsKey(preface + Game1.currentLocation.name))
            {
                this.CurrentDialogue.Push(new Dialogue(this.Dialogue[preface + Game1.currentLocation.name], this)
                {
                    removeOnNextMove = true
                });
                return true;
            }
            return false;
        }

        private Stack<Dialogue> loadCurrentDialogue()
        {
            this.updatedDialogueYet = true;
            Friendship friends;
            int heartLevel = Game1.player.friendshipData.TryGetValue(base.Name, out friends) ? (friends.Points / 250) : 0;
            Stack<Dialogue> currentDialogue = new Stack<Dialogue>();
            NetVector2 defaultPosition = new();
            Random r = new Random((int)(Game1.stats.DaysPlayed * 77U + (uint)((int)Game1.uniqueIDForThisGame / 2) + 2U + (uint)((int)defaultPosition.X * 77) + (uint)((int)defaultPosition.Y * 777)));
            if (r.NextDouble() < 0.025 && heartLevel >= 1)
            {
                Dictionary<string, string> npcDispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
                string disposition;
                if (npcDispositions.TryGetValue(this.Name, out disposition))
                {
                    string[] relatives = disposition.Split('/', StringSplitOptions.None)[9].Split(' ', StringSplitOptions.None);
                    if (relatives.Length > 1)
                    {
                        int index = r.Next(relatives.Length / 2) * 2;
                        string relativeName = relatives[index];
                        string relativeDisplayName = relativeName;
                        if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en && Game1.getCharacterFromName(relativeName, true, false) != null)
                        {
                            relativeDisplayName = Game1.getCharacterFromName(relativeName, true, false).displayName;
                        }
                        string relativeTitle = relatives[index + 1].Replace("'", "").Replace("_", " ");
                        string relativeProps;
                        bool relativeIsMale = npcDispositions.TryGetValue(relativeName, out relativeProps) && relativeProps.Split('/', StringSplitOptions.None)[4].Equals("male");
                        Dictionary<string, string> npcGiftTastes = Game1.content.Load<Dictionary<string, string>>("Data\\NPCGiftTastes");
                        if (npcGiftTastes.ContainsKey(relativeName))
                        {
                            string itemName = null;
                            string nameAndTitle = (relativeTitle.Length > 2 && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ja) ? (relativeIsMale ? Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4079", new object[]
                            {
                                relativeTitle
                            }) : Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4080", new object[]
                            {
                                relativeTitle
                            })) : relativeDisplayName;
                            string message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4083", nameAndTitle);
                            int item;
                            if (r.NextDouble() < 0.5)
                            {
                                string[] loveItems = npcGiftTastes[relativeName].Split('/', StringSplitOptions.None)[1].Split(' ', StringSplitOptions.None);
                                item = Convert.ToInt32(loveItems[r.Next(loveItems.Length)]);
                                string itemDetails;
                                if (Game1.objectInformation.TryGetValue(item, out itemDetails))
                                {
                                    itemName = itemDetails.Split('/', StringSplitOptions.None)[4];
                                    message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4084", itemName);
                                    if (this.Age == 2)
                                    {
                                        message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4086", relativeDisplayName, itemName) + (relativeIsMale ? Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4088") : Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4089"));
                                    }
                                    else
                                    {
                                        switch (r.Next(5))
                                        {
                                            case 0:
                                                message = Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4091", new object[]
                                                {
                                                nameAndTitle,
                                                itemName
                                                });
                                                break;
                                            case 1:
                                                message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4094", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4097", nameAndTitle, itemName));
                                                break;
                                            case 2:
                                                message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4100", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4103", nameAndTitle, itemName));
                                                break;
                                            case 3:
                                                message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4106", nameAndTitle, itemName);
                                                break;
                                        }
                                        if (r.NextDouble() < 0.65)
                                        {
                                            switch (r.Next(5))
                                            {
                                                case 0:
                                                    message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4109") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4111"));
                                                    break;
                                                case 1:
                                                    message += (relativeIsMale ? ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4113") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4114")) : ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4115") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4116")));
                                                    break;
                                                case 2:
                                                    message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4118") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4120"));
                                                    break;
                                                case 3:
                                                    message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4125");
                                                    break;
                                                case 4:
                                                    message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4126") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4128"));
                                                    break;
                                            }
                                            if (relativeName.Equals("Abigail") && r.NextDouble() < 0.5)
                                            {
                                                message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4128", relativeDisplayName, itemName);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    item = Convert.ToInt32(npcGiftTastes[relativeName].Split('/', StringSplitOptions.None)[7].Split(' ', StringSplitOptions.None)[r.Next(npcGiftTastes[relativeName].Split('/', StringSplitOptions.None)[7].Split(' ', StringSplitOptions.None).Length)]);
                                }
                                catch (Exception)
                                {
                                    item = Convert.ToInt32(npcGiftTastes["Universal_Hate"].Split(' ', StringSplitOptions.None)[r.Next(npcGiftTastes["Universal_Hate"].Split(' ', StringSplitOptions.None).Length)]);
                                }
                                if (Game1.objectInformation.ContainsKey(item))
                                {
                                    itemName = Game1.objectInformation[item].Split('/', StringSplitOptions.None)[4];
                                    message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4135", itemName, Lexicon.getRandomNegativeFoodAdjective(null)) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4138", itemName, Lexicon.getRandomNegativeFoodAdjective(null)));
                                    if (this.Age == 2)
                                    {
                                        message = (relativeIsMale ? Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4141", new object[]
                                        {
                                            relativeDisplayName,
                                            itemName
                                        }) : Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4144", new object[]
                                        {
                                            relativeDisplayName,
                                            itemName
                                        }));
                                    }
                                    else
                                    {
                                        switch (r.Next(4))
                                        {
                                            case 0:
                                                message = ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4146") : "") + Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4147", new object[]
                                                {
                                                nameAndTitle,
                                                itemName
                                                });
                                                break;
                                            case 1:
                                                message = (relativeIsMale ? ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4149", new object[]
                                                {
                                                nameAndTitle,
                                                itemName
                                                }) : Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4152", new object[]
                                                {
                                                nameAndTitle,
                                                itemName
                                                })) : ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4153", new object[]
                                                {
                                                nameAndTitle,
                                                itemName
                                                }) : Game1.LoadStringByGender(this.gender, "Strings\\StringsFromCSFiles:NPC.cs.4154", new object[]
                                                {
                                                nameAndTitle,
                                                itemName
                                                })));
                                                break;
                                            case 2:
                                                message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4161", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4164", nameAndTitle, itemName));
                                                break;
                                        }
                                        if (r.NextDouble() < 0.65)
                                        {
                                            switch (r.Next(5))
                                            {
                                                case 0:
                                                    message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4170");
                                                    break;
                                                case 1:
                                                    message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4171");
                                                    break;
                                                case 2:
                                                    message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4172") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4174"));
                                                    break;
                                                case 3:
                                                    message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4176") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4178"));
                                                    break;
                                                case 4:
                                                    message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4180");
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (itemName != null)
                            {
                                if (Game1.getCharacterFromName(relativeName, true, false) != null)
                                {
                                    message = message + "%revealtaste" + relativeName + item.ToString();
                                }
                                currentDialogue.Clear();
                                if (message.Length > 0)
                                {
                                    try
                                    {
                                        message = message.Substring(0, 1).ToUpper() + message.Substring(1, message.Length - 1);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                                currentDialogue.Push(new Dialogue(message, this));
                                return currentDialogue;
                            }
                        }
                    }
                }
            }
            if (this.Dialogue != null && this.Dialogue.Count != 0)
            {
                string currentDialogueStr = "";
                currentDialogue.Clear();
                if (this.idForClones == -1)
                {
                    if (Game1.isRaining && r.NextDouble() < 0.5 && (base.currentLocation == null || base.currentLocation.GetLocationContext() == GameLocation.LocationContext.Default) && !(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri") && !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
                    {
                        try
                        {
                            currentDialogue.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\rainy")[this.GetDialogueSheetName()], this));
                            return currentDialogue;
                        }
                        catch (Exception)
                        {
                        }
                    }
                    Dialogue d = this.tryToRetrieveDialogue(Game1.currentSeason + "_", heartLevel, "");
                    if (d == null)
                    {
                        d = this.tryToRetrieveDialogue("", heartLevel, "");
                    }
                    if (d != null)
                    {
                        currentDialogue.Push(d);
                    }
                    return currentDialogue;
                }
                this.Dialogue.TryGetValue(this.idForClones.ToString() ?? "", out currentDialogueStr);
                currentDialogue.Push(new Dialogue(currentDialogueStr, this));
            }
            return currentDialogue;
        }

        public new Dialogue tryToRetrieveDialogue(string preface, int heartLevel, string appendToEnd = "")
        {
            int year = Game1.year;
            if (Game1.year > 2)
            {
                year = 2;
            }
            string day_name = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
            if (this.Dialogue.ContainsKey(preface + Game1.dayOfMonth.ToString() + appendToEnd) && year == 1)
            {
                return new Dialogue(this.Dialogue[preface + Game1.dayOfMonth.ToString() + appendToEnd], this);
            }
            if (this.Dialogue.ContainsKey(string.Concat(new string[]
            {
                preface,
                Game1.dayOfMonth.ToString(),
                "_",
                year.ToString(),
                appendToEnd
            })))
            {
                return new Dialogue(this.Dialogue[string.Concat(new string[]
                {
                    preface,
                    Game1.dayOfMonth.ToString(),
                    "_",
                    year.ToString(),
                    appendToEnd
                })], this);
            }
            if (heartLevel >= 10 && this.Dialogue.ContainsKey(preface + day_name + "10" + appendToEnd))
            {
                if (!this.Dialogue.ContainsKey(string.Concat(new string[]
                {
                    preface,
                    day_name,
                    "10_",
                    year.ToString(),
                    appendToEnd
                })))
                {
                    return new Dialogue(this.Dialogue[preface + day_name + "10" + appendToEnd], this);
                }
                return new Dialogue(this.Dialogue[string.Concat(new string[]
                {
                    preface,
                    day_name,
                    "10_",
                    year.ToString(),
                    appendToEnd
                })], this);
            }
            else if (heartLevel >= 8 && this.Dialogue.ContainsKey(preface + day_name + "8" + appendToEnd))
            {
                if (!this.Dialogue.ContainsKey(string.Concat(new string[]
                {
                    preface,
                    day_name,
                    "8_",
                    year.ToString(),
                    appendToEnd
                })))
                {
                    return new Dialogue(this.Dialogue[preface + day_name + "8" + appendToEnd], this);
                }
                return new Dialogue(this.Dialogue[string.Concat(new string[]
                {
                    preface,
                    day_name,
                    "8_",
                    year.ToString(),
                    appendToEnd
                })], this);
            }
            else if (heartLevel >= 6 && this.Dialogue.ContainsKey(preface + day_name + "6" + appendToEnd))
            {
                if (!this.Dialogue.ContainsKey(preface + day_name + "6_" + year.ToString()))
                {
                    return new Dialogue(this.Dialogue[preface + day_name + "6" + appendToEnd], this);
                }
                return new Dialogue(this.Dialogue[string.Concat(new string[]
                {
                    preface,
                    day_name,
                    "6_",
                    year.ToString(),
                    appendToEnd
                })], this);
            }
            else if (heartLevel >= 4 && this.Dialogue.ContainsKey(preface + day_name + "4" + appendToEnd))
            {
                if (!this.Dialogue.ContainsKey(preface + day_name + "4_" + year.ToString()))
                {
                    return new Dialogue(this.Dialogue[preface + day_name + "4" + appendToEnd], this);
                }
                return new Dialogue(this.Dialogue[string.Concat(new string[]
                {
                    preface,
                    day_name,
                    "4_",
                    year.ToString(),
                    appendToEnd
                })], this);
            }
            else if (heartLevel >= 2 && this.Dialogue.ContainsKey(preface + day_name + "2" + appendToEnd))
            {
                if (!this.Dialogue.ContainsKey(preface + day_name + "2_" + year.ToString()))
                {
                    return new Dialogue(this.Dialogue[preface + day_name + "2" + appendToEnd], this);
                }
                return new Dialogue(this.Dialogue[string.Concat(new string[]
                {
                    preface,
                    day_name,
                    "2_",
                    year.ToString(),
                    appendToEnd
                })], this);
            }
            else
            {
                if (!this.Dialogue.ContainsKey(preface + day_name + appendToEnd))
                {
                    return null;
                }

                if (!this.Dialogue.ContainsKey(string.Concat(new string[]
                {
                    preface,
                    day_name,
                    "_",
                    year.ToString(),
                    appendToEnd
                })))
                {
                    return new Dialogue(this.Dialogue[preface + day_name + appendToEnd], this);
                }
                return new Dialogue(this.Dialogue[string.Concat(new string[]
                {
                    preface,
                    day_name,
                    "_",
                    year.ToString(),
                    appendToEnd
                })], this);
            }
        }

    }

}
