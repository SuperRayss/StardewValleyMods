using HarmonyLib;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TalkingPets
{
    [HarmonyPatch(typeof(NPC))]
    [HarmonyPatch("Dialogue", MethodType.Getter)]
    public class NPCPatches
    {
        private static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        // Patches Dialogue_get to accept pets as having dialogue
        public static void Postfix(NPC __instance, ref Dictionary<string, string> __result)
        {
            if (__instance is Pet)
            {
#if DEBUG
                //Monitor.Log($"Patching Dialogue_get:\n", LogLevel.Debug); // DEBUG
#endif
                string dialogue_file = "Characters\\Dialogue\\" + __instance.GetDialogueSheetName();
                if (NPC.invalidDialogueFiles.Contains(dialogue_file))
                {
                    __result = new Dictionary<string, string>();
                }
                try
                {
                    __result = LoadDialogue(dialogue_file);
                }
                catch (ContentLoadException ex)
                {
#if DEBUG
                    //Monitor.Log($"Failed in Dialogue_get:\n{ex}", LogLevel.Error); // DEBUG
#endif
                    NPC.invalidDialogueFiles.Add(dialogue_file);
                    if (__instance is Dog) __result = LoadDialogue("Characters\\Dialogue\\DogFallback");
                    else if (__instance is Cat) __result = LoadDialogue("Characters\\Dialogue\\CatFallback");
                }
            }
        }

        public static Dictionary<string, string> LoadDialogue(string dialogue_file)
        {
            return Game1.content.Load<Dictionary<string, string>>(dialogue_file).Select(delegate (KeyValuePair<string, string> pair)
            {
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
            }).ToDictionary((KeyValuePair<string, string> p) => p.Key, (KeyValuePair<string, string> p) => p.Value);
        }
    }

}
