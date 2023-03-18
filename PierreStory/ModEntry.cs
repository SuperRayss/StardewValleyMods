using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace PierreStory
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private NPC speaker;
        private int counter = 0;
//        private Dialogue dialogue { get; set; }
//        private const string speech = "Please buy my wares!";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        // NOTE: Despite the misleading event name, this also works if you start a new game
        // Acts as the best point for accessing initialized members from Game1
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            speaker = Game1.getCharacterFromName("Pierre");
            DrawPierreDialogue("Rise and shine, buddy!");
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (
                !Context.IsWorldReady || 
                !Context.IsPlayerFree || 
                (e.Button.TryGetController(out var button) && button.ToString() == "Start") || 
                (e.Button.TryGetKeyboard(out var key) && key.ToString() == "Escape")
            ) return;
            if (counter < 5) {
                DrawPierreDialogue("Please buy my wares, {Game1.player.displayName}!");
                counter++;
            } else DrawPierreDialogue("You simply can't leave until you buy my wares!");
        }

        private void DrawPierreDialogue(string speech) => Game1.drawDialogue(speaker, speech);

    }
}
