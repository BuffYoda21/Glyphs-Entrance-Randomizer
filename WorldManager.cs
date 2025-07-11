using System;
using Il2Cpp;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlyphsEntranceRando {
    public class WorldManager : MonoBehaviour {
        public void Update() {
            if (SceneManager.GetActiveScene().name != "Game") return;
            if (!warpParent)
                warpParent = GameObject.Find("Warps")?.transform;
            if (!warpParent) return;
            if (!constructDoorEnter || !constructDoorExit || !r1FlowerEnter || !r1FlowerExit) {
                constructDoorEnter = warpParent.Find($"0x{CONSTRUCT_DOOR_ENTER:X4}")?.GetComponent<DynamicTp>();
                constructDoorExit = warpParent.Find($"0x{CONSTRUCT_DOOR_EXIT:X4}")?.GetComponent<DynamicTp>();
                r1FlowerExit = warpParent.Find($"0x{R1_FLOWER_EXIT:X4}")?.GetComponent<DynamicTp>();
                r1FlowerEnter = warpParent.Find($"0x{R1_FLOWER_ENTER:X4}")?.GetComponent<DynamicTp>();
                if (!constructDoorEnter || !constructDoorExit || !r1FlowerEnter || !r1FlowerExit) return;
            }
            if (!player)
                player = GameObject.Find("Player")?.transform;

            if (player && Input.GetKeyDown(KeyCode.Backspace))
                player.position = new Vector3(0f, 0f, 0f);

            if (!runicConstruct) {
                if (!constructParent)
                    constructParent = GameObject.Find("World/Region1/Runic Construct(R3E)")?.transform;
                if (constructParent)
                    runicConstruct = constructParent.Find("DashBoss")?.gameObject;
                if (runicConstruct) {
                    constructDoorEnter.blocked = true;
                    constructDoorExit.blocked = true;
                } else {
                    constructDoorEnter.blocked = false;
                    constructDoorExit.blocked = false;
                }
            }

            // More logic coming later once flower puzzle is implemented. For now these should always be blocked.
            if (!r1FlowerEnter.blocked)
                r1FlowerEnter.blocked = true;
            if (!r1FlowerExit.blocked)
                r1FlowerExit.blocked = true;
        }

        Transform player;
        Transform warpParent;
        Transform constructParent;
        GameObject runicConstruct;
        DynamicTp constructDoorExit;
        DynamicTp constructDoorEnter;
        DynamicTp r1FlowerExit;
        DynamicTp r1FlowerEnter;

        //these entrances may be blocked under certain conditions
        public const int CONSTRUCT_DOOR_EXIT = 0x0033;
        public const int CONSTRUCT_DOOR_ENTER = 0x0032;
        public const int R1_FLOWER_EXIT = 0x0016;
        public const int R1_FLOWER_ENTER = 0x0028;
    }
}