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
        #pragma warning restore IDE0060 // Restore unused parameter warning

        private static void PlaceWarp(GameObject warp, RoomShuffler.SerializedEntrancePair pair) {
            warp.AddComponent<DynamicTp>();
            warp.GetComponent<DynamicTp>().id = pair.entrance;
            warp.GetComponent<DynamicTp>().targetId = pair.couple;
            warp.AddComponent<BoxCollider2D>();
            warp.GetComponent<BoxCollider2D>().isTrigger = true;
            switch (pair.entrance) {
                case 0x0000:
                    warp.transform.position = new Vector3(138f, 8.1f, 0f);
                    warp.transform.localScale = new Vector3(4.5f, 0.25f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Bottom; break;
                case 0x0001:
                    warp.transform.position = new Vector3(14.1f, 1.6f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 8.75f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Right; break;
                case 0x0002:
                    warp.transform.position = new Vector3(14.4f, 1.6f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 8.75f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Left; break;
                case 0x0003:
                    warp.transform.position = new Vector3(42.6f, 1.6f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 8.75f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Right; break;
                case 0x0004:
                    warp.transform.position = new Vector3(42.9f, 1.6f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 8.75f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Left; break;
                case 0x0005:
                    warp.transform.position = new Vector3(71.1f, 1.6f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 8.75f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Right; break;
                case 0x0006:
                    warp.transform.position = new Vector3(71.4f, 1.6f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 8.75f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Left; break;
                case 0x0007:
                    warp.transform.position = new Vector3(99.6f, 1.6f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 8.75f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Right; break;
                case 0x0008:
                    warp.transform.position = new Vector3(84.75f, -7.9f, 0f);
                    warp.transform.localScale = new Vector3(18.5f, 0.25f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Bottom; break;
                case 0x0009:
                    warp.transform.position = new Vector3(99.9f, 1.6f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 8.75f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Left; break;
                case 0x000A:
                    warp.transform.position = new Vector3(128.1f, 1.6f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 8.75f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Right; break;
                case 0x000B:
                    warp.transform.position = new Vector3(128.4f, 1.6f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 8.75f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Left; break;
                case 0x000C:
                    warp.transform.position = new Vector3(185.1f, 4.9f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 2.2f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Right; break;
                case 0x000D:
                    warp.transform.position = new Vector3(138f, 7.8f, 0f);
                    warp.transform.localScale = new Vector3(4.5f, 0.25f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Top; break;
                case 0x000E:
                    warp.transform.position = new Vector3(185.4f, 4.9f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 2.2f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Left; break;
                case 0x000F:
                    warp.transform.position = new Vector3(213.6f, 1.5f, 0f);
                    warp.transform.localScale = new Vector3(0.25f, 9f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Right; break;
                case 0x0010:
                    warp.transform.position = new Vector3(190.625f, -7.9f, 0f);
                    warp.transform.localScale = new Vector3(2.25f, 0.25f, 1f);
                    warp.GetComponent<DynamicTp>().type = EntranceType.Bottom; break;
            }
        }

        public static int lastSceneHandle = -1;
        public static GameObject warpParent;
        public static List<RoomShuffler.SerializedEntrancePair> entrancePairs;
        public static List<GameObject> warps = new List<GameObject>();
    }
}
