using UnityEngine;
using Memori.Scenes;

namespace TJ
{
    // Registers the Steam Workshop -> local Mods folder sync against SceneHandler's generic
    // pre-load hook, so it runs and completes automatically before the game's first scene
    // transition - and therefore before TabletopTavernData.Instance is ever touched, since that
    // only happens once MainMenu/Tavern content scenes have loaded. SceneHandler lives in the
    // separate Memori.Scenes assembly and has no knowledge of this project's modding system;
    // this is the main-assembly side of that hook.
    public static class SteamWorkshopBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneHandler.OnBeforeFirstLoad += SteamWorkshopModSync.SyncSubscribedItemsToModsFolderAsync;
        }
    }
}
