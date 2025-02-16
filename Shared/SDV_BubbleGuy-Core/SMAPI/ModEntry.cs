﻿using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using SDV_Speaker.Speaker;
using System.IO;
using SDV_BubbleGuy.SMAPIInt;


namespace SDV_Speaker.SMAPIInt
{
    internal class ModEntry : Mod
    {
        private BubbleGuyManager oManager;
        public override void Entry(IModHelper helper)
        {
            //
            //  check for Stardew Web, if installed
            //  do not load this mod
            //
            if (helper.ModRegistry.IsLoaded("prism99.stardewweb"))
            {
                Monitor.Log("Stardew Web is installed, this mod is not needed an will not be loaded.", LogLevel.Info);
            }
            else
            {
                SMAPIHelpers.Initialize(helper, Monitor);
                oManager = new BubbleGuyManager(Path.Combine(helper.DirectoryPath, "saves"), Path.Combine(helper.DirectoryPath, "Sprites"), helper, Monitor, true);
                oManager.StartBubbleChat(ModManifest.UniqueID);
                helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            }
         }
        public override object GetApi()
        {
            return new BubbleGuyAPI(oManager);
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {

            BubbleGuyStatics.Initialize(Path.Combine(SMAPIHelpers.helper.DirectoryPath, "Sprites"));
             
#if DEBUG
            Monitor.Log($"BubbleGuy name: '{BubbleGuyStatics.BubbleGuyName}'", LogLevel.Info);
#endif
        }
    }

}