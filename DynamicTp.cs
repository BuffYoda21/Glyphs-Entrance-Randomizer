using UnityEngine;
using Il2Cpp;
using System.Collections.Generic;

namespace GlyphsEntranceRando {
    public class DynamicTp : MonoBehaviour {
        public void OnTriggerEnter2D(Collider2D other) {
            if (!other.GetComponent<PlayerController>() || !targetExit.GetComponent<DynamicTp>()) return;
            switch (type) {
                case EntranceType.Left: other.transform.localPosition = new Vector3(targetExit.position.x - 1f, targetExit.position.y + ((other.transform.position.y - transform.position.y) / transform.localScale.y * targetExit.localScale.y), targetExit.position.z); break;
                case EntranceType.Right: other.transform.localPosition = new Vector3(targetExit.position.x + 1f, targetExit.position.y + ((other.transform.position.y - transform.position.y) / transform.localScale.y * targetExit.localScale.y), targetExit.position.z); break;
                case EntranceType.Top: other.transform.localPosition = new Vector3(targetExit.position.x + ((other.transform.position.x - transform.position.x) / transform.localScale.x * targetExit.localScale.x), targetExit.position.y + 1f, targetExit.position.z); break;
                case EntranceType.Bottom: other.transform.localPosition = new Vector3(targetExit.position.x + ((other.transform.position.x - transform.position.x) / transform.localScale.x * targetExit.localScale.x), targetExit.position.y - 1f, targetExit.position.z); break;
            }
        }
        public void RegisterTargetFromId(List<GameObject> warps) {
            GameObject warp = warps.Find(warp => warp != this.gameObject && warp.GetComponent<DynamicTp>()?.id == targetId);
            targetExit = warp.transform;
        }

        public Transform targetExit;
        public int id;
        public int targetId;
        public EntranceType type;
    }
}
