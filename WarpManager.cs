using UnityEngine;
using HarmonyLib;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace GlyphsEntranceRando {
    [HarmonyPatch]
    public static class WarpManager {
        [HarmonyPatch(typeof(SceneManager), "Internal_SceneLoaded")]
        [HarmonyPostfix]
        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (scene.handle == lastSceneHandle)
                return;
            lastSceneHandle = scene.handle;
            if (scene.name != "Game") return;
            warpParent = new GameObject("Warps");
            if (entrancePairs == null) return;

            foreach (RoomShuffler.SerializedEntrancePair pair in entrancePairs) {
                GameObject warp = new GameObject($"0x{pair.entrance:X4}");
                warp.transform.SetParent(warpParent.transform);
                PlaceWarp(warp, pair);
                warps.Add(warp);
            }
            foreach (GameObject warp in warps) {
                warp.GetComponent<DynamicTp>()?.RegisterTargetFromId(warps);
            }
        }

        private static void PlaceWarp(GameObject warp, RoomShuffler.SerializedEntrancePair pair) {
            warp.AddComponent<DynamicTp>();
            warp.GetComponent<DynamicTp>().id = pair.entrance;
            warp.GetComponent<DynamicTp>().targetId = pair.couple;
            warp.AddComponent<BoxCollider2D>();
            warp.GetComponent<BoxCollider2D>().isTrigger = true;
            switch (pair.entrance) {
                case 0x0001:
                    warp.transform.position = new Vector3(14.1f, 1.6f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 8.75f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Right; break;
                case 0x000E:
                    warp.transform.position = new Vector3(185.4f, 4.825f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 2.4f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Left; break;
            }
        }

        public static int lastSceneHandle = -1;
        public static GameObject warpParent;
        public static List<RoomShuffler.SerializedEntrancePair> entrancePairs;
        public static List<GameObject> warps = new List<GameObject>();
    }
}
