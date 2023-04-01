using StardewModdingAPI;
using StardewModdingAPI.Events;
using HarmonyLib;
using System.Reflection;

namespace TalkingPets
{
    public class ModEntry : Mod
    {

        // TODO: There are currently bugs:
        // 1. Gift tastes seem out of whack for some types, this is an issue with the content patcher setup
        // The best approach will be to patch the gift tastes members the same way as dialogue
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            NPCPatches.Initialize(Monitor);
            PetDialogue.Initialize(Monitor);
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var api = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            api.RegisterToken(this.ModManifest, "PetNames", () =>
            {
                return PetDialogue.petNames; // Will return null if not initialized, which is fine
            });
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            PetDialogue.petNames?.Clear();
            PetDialogue.petDialogue?.Clear();
        }

    }

}
