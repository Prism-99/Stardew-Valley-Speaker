﻿//
//  Used for retrieving game graphic assets
//  Textures and sprite sheets
//
//  SDV 1.5.5 switched to use MonoGame which enforces thread access rules
//  more tighly than the base XNA API
//
//  All graphic access must be done on the UI thread or they will fail
//
//  This class leverages existing SMAPI game hooks to ensure graphic
//  loading is done in the required thread.
//

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
using System.Threading;
using StardewValley;


namespace StardewModHelpers
{
    internal static class StardewTextureLoader
    {
        private static List<string> lImagesToLoad = new List<string> { };
        private static Dictionary<string, StardewBitmap> dcImages = new Dictionary<string, StardewBitmap> { };
        private static List<string> lSpriteSheetToLoad = new List<string> { };
        private static Dictionary<string, StardewBitmap> dcSpriteSheets = new Dictionary<string, StardewBitmap> { };
        private static  IModHelper oHelper;
        private static int mainThreadId;
        public static void Initialize(IModHelper helper)
        {
            oHelper = helper;
            // making assumpition being initialized in main thread
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }
        public static StardewBitmap LoadSpriteSheet(string sSheetName)
        {
            StardewBitmap sbResult;
            //
            //  check if the request can be handled in the current
            //  thread or needs to be queued to be handled in the UI
            //  thread
            //
            if (Thread.CurrentThread.ManagedThreadId == mainThreadId)
            {
                sbResult = GetSpriteSheet(sSheetName);
            }
            else
            {
                //
                //  Queue up spritesheet request
                //
                lock (lSpriteSheetToLoad)
                {
                    lSpriteSheetToLoad.Add(sSheetName);
                }
                oHelper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked_LoadSheet;
                //
                //  Wait for request to be handled in another thread
                //
                while (!dcSpriteSheets.ContainsKey(sSheetName))
                {
                    Thread.Sleep(100);
                }
                //
                //  Retrieve results
                //
                lock (dcSpriteSheets)
                {
                    sbResult = dcSpriteSheets[sSheetName];
                    dcSpriteSheets.Remove(sSheetName);
                }
            }

            return sbResult;
        }
        private static StardewBitmap GetSpriteSheet(string sSheetName)
        {
#if TRACE
            StardewLogger.LogTrace("GetSpritesheet", $"sheetname '{sSheetName}'");
#endif
            //
            //  Fetch requested spritesheet
            //
            switch (sSheetName)
            {
                case "emoteSpriteSheet":
                    return new StardewBitmap(Game1.emoteSpriteSheet);
                case "objectSpriteSheet":
                    return new StardewBitmap(Game1.objectSpriteSheet);
                case "bigCraftableSpriteSheet":
                    return new StardewBitmap(Game1.bigCraftableSpriteSheet);
                default:
                    return new StardewBitmap(oHelper.Content.Load<Texture2D>(sSheetName,ContentSource.GameContent));
            }
        }
        public static StardewBitmap LoadImageInUIThread(string sImage)
        {
            StardewBitmap sbResult;
            //
            //  check if the request can be handled in the current
            //  thread or needs to be queued to be handled in the UI
            //  thread
            //
            if (Thread.CurrentThread.ManagedThreadId == mainThreadId)
            {
                sbResult = new StardewBitmap(oHelper.Content.Load<Texture2D>(sImage, ContentSource.GameContent));
            }
            else
            {
                //
                //  Queue up spritesheet request
                //
                lock (lImagesToLoad)
                {
                    lImagesToLoad.Add(sImage);
                    //
                    // use SMAPI to add hook to fetch image in the UI thread
                    //
                    oHelper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked_LoadImage;
                }
                //
                //  Wait for request to be handled in another thread
                //
                while (!dcImages.ContainsKey(sImage))
                {
                    Thread.Sleep(200);
                }
                //
                //  Retrieve results
                //
                lock (dcImages)
                {
                    sbResult = dcImages[sImage];
                    dcImages.Remove(sImage);
                }
            }

            return sbResult;
        }

        private static void GameLoop_UpdateTicked_LoadImage(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            //
            //  Get request image in UI thread
            //
            lock (lImagesToLoad)
            {
                foreach (string sImagePath in lImagesToLoad)
                {
                    try
                    {
                        Texture2D txImage = oHelper.Content.Load<Texture2D>(sImagePath, ContentSource.GameContent);
                        lock (dcImages)
                        {
                            dcImages.Add(sImagePath, new StardewBitmap(txImage));
                        }
                    }
                    catch
                    {
                        dcImages.Add(sImagePath, null);
                    }
                }
                //
                //  clear requesst list
                //
                lImagesToLoad.Clear();
                //
                //  unhook event
                //
                oHelper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked_LoadImage;
            }
        }
        private static void GameLoop_UpdateTicked_LoadSheet(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            lock (lSpriteSheetToLoad)
            {
                foreach (string sImagePath in lSpriteSheetToLoad)
                {
                    lock (dcSpriteSheets)
                    {
                        dcSpriteSheets.Add(sImagePath, GetSpriteSheet(sImagePath));
                    }
                }
                //
                //  clear request list
                //
                lSpriteSheetToLoad.Clear();
                //
                //  unhook event
                //
                oHelper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked_LoadSheet;
            }
        }
    }
}
