using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using System.Collections.Generic;

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
                if (PetDialogue.petDialogue == null) // If dialogue has not been loaded from JSON yet
                {
                    PetDialogue.LoadDialogueJSON();
                    PetDialogue.SetDialogue();
                }
                if (PetDialogue.petDialogue.TryGetValue(__instance.Name, out Dictionary<string, string> dialogue))
                {
                    __result = dialogue;
                } else // If this pet has not been added yet
                {
                    PetDialogue.SetDialogue();
                    Postfix(__instance, ref __result);
                }
            }
        }

    }

}
