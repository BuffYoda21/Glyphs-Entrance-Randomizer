using UnityEngine;
using MelonLoader;
using HarmonyLib;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace GlyphsEntranceRando
{
    [HarmonyPatch]
    public static class WarpManager
    {
        [HarmonyPatch(typeof(SceneManager), "Internal_SceneLoaded")]
        [HarmonyPostfix]
        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.handle == lastSceneHandle)
                return;
            lastSceneHandle = scene.handle;
            if (scene.name == "Game")
            {
                warpParent = new GameObject("Warps");
                if (entrancePairs != null)
                {
                    foreach (RoomShuffler.SerializedEntrancePair pair in entrancePairs)
                    {
                        GameObject warp = new GameObject($"0x{pair.entrance:X4}");
                        warp.transform.SetParent(warpParent.transform);
                        warps.Add(warp);

                    }
                }
            }
        }

        public static int lastSceneHandle = -1;
        public static GameObject warpParent;
        public static List<RoomShuffler.SerializedEntrancePair> entrancePairs;
        public static List<GameObject> warps = new List<GameObject>();
    }
}