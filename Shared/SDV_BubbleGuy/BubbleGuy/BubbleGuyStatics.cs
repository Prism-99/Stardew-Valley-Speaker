﻿

using StardewValley;

namespace SDV_Speaker.Speaker
{
   internal static class BubbleGuyStatics
    {
        public static string ModPath;
        public static string BubbleGuyName;
        public static string BubbleGuyPrefix => "BubbleGuy_";
        public static void Initialize(string sModPath)
        {
            ModPath = sModPath;
            BubbleGuyName = $"{BubbleGuyPrefix}{Game1.player.uniqueMultiplayerID}";
        }
    }
}
