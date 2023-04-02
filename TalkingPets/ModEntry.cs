using StardewModdingAPI;
using StardewModdingAPI.Events;
using HarmonyLib;
using System.Reflection;

namespace TalkingPets
{
    public class ModEntry : Mod
    {

        public override void Entry(IModHelper helper)
        {
            Initialize();
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void Initialize()
        {
            NPCPatches_Dialogue_get.Initialize(Monitor);
            PetDialogue.Initialize(Monitor);
            PetPatches_checkAction.Initialize(Monitor); // TODO: Clean this up, make a global Monitor, maybe just make ModEntry a singleton class(?)
/*            PetPatches_tryToReceiveActiveObject.Initialize(Monitor);
            PetPatches_receiveGift.Initialize(Monitor);
            PetPatches_getGiftTasteForThisItem.Initialize(Monitor);*/
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            PetDialogue.petDialogue?.Clear();
        }

    }

}
