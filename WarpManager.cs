using UnityEngine;
using HarmonyLib;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace GlyphsEntranceRando {
    [HarmonyPatch]
    public static class WarpManager {
#pragma warning disable IDE0060 // Remove unused parameter warning
        [HarmonyPatch(typeof(SceneManager), "Internal_SceneLoaded")]
        [HarmonyPostfix]
        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (scene.handle == lastSceneHandle) return;
            lastSceneHandle = scene.handle;
            if (scene.name != "Game" || entrancePairs == null) return;
            warpParent = new GameObject("Warps");
            warps.Clear();

            foreach (SerializedEntrancePair pair in entrancePairs) {
                GameObject warp = new GameObject($"0x{pair.entrance:X4}");
                warp.transform.SetParent(warpParent.transform);
                PlaceWarp(warp, pair);
                warps.Add(warp);
            }
            foreach (GameObject warp in warps) {
                warp.GetComponent<DynamicTp>()?.RegisterTargetFromId(warps);
            }

            // Remove cutscene triggers
            GameObject.Find("World/Region1/(R4A)/SaveConditional")?.SetActive(false);
            GameObject.Find("World/Region1/(R3D)(sword)/SaveConditional/")?.SetActive(false);
        }
#pragma warning restore IDE0060 // Restore unused parameter warning

        private static void PlaceWarp(GameObject warp, SerializedEntrancePair pair) {
            int entranceId = pair.entrance, targetId = pair.couple;
            warp.AddComponent<DynamicTp>();
            DynamicTp tp = warp.GetComponent<DynamicTp>();
            tp.id = entranceId;
            tp.targetId = targetId;
            Entrance entrance = Resources.Entrances.Contents[entranceId];
            tp.type = entrance.type;
            warp.transform.position = entrance.position;
            warp.transform.localScale = entrance.scale;

            warp.AddComponent<BoxCollider2D>();
            warp.GetComponent<BoxCollider2D>().isTrigger = true;
        }

        public static int lastSceneHandle = -1;
        public static GameObject warpParent;
        public static List<SerializedEntrancePair> entrancePairs;
        public static List<GameObject> warps = new List<GameObject>();
    }
}
