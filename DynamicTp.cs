using UnityEngine;
using Il2Cpp;
using System.Collections.Generic;

namespace GlyphsEntranceRando {
    public class DynamicTp : MonoBehaviour {
        public void OnTriggerEnter2D(Collider2D other) {
            if (other?.GetComponent<PlayerController>() == null || targetExit?.GetComponent<DynamicTp>() == null ||
                other.GetComponent<Rigidbody2D>().linearVelocityX >= 0 && type == EntranceType.Left ||
                other.GetComponent<Rigidbody2D>().linearVelocityX <= 0 && type == EntranceType.Right ||
                other.GetComponent<Rigidbody2D>().linearVelocityY >= 0 && type == EntranceType.Bottom ||
                other.GetComponent<Rigidbody2D>().linearVelocityY <= 0 && type == EntranceType.Top) return;
            if (!targetExit.GetComponent<DynamicTp>().blocked) {
                switch (type) {
                    case EntranceType.Left: other.transform.position = new Vector3(targetExit.position.x - .5f, targetExit.position.y + ((other.transform.position.y - transform.position.y) / transform.localScale.y * targetExit.localScale.y), targetExit.position.z); break;
                    case EntranceType.Right: other.transform.position = new Vector3(targetExit.position.x + .5f, targetExit.position.y + ((other.transform.position.y - transform.position.y) / transform.localScale.y * targetExit.localScale.y), targetExit.position.z); break;
                    case EntranceType.Top: other.transform.position = new Vector3(targetExit.position.x + ((other.transform.position.x - transform.position.x) / transform.localScale.x * targetExit.localScale.x), targetExit.position.y + .5f, targetExit.position.z); break;
                    case EntranceType.Bottom: other.transform.position = new Vector3(targetExit.position.x + ((other.transform.position.x - transform.position.x) / transform.localScale.x * targetExit.localScale.x), targetExit.position.y - .5f, targetExit.position.z); break;
                }
            } else {
                switch (type) {
                    case EntranceType.Left: other.transform.position = new Vector3(other.transform.position.x + 1f, other.transform.position.y, other.transform.position.z); break;
                    case EntranceType.Right: other.transform.position = new Vector3(other.transform.position.x - 1f, other.transform.position.y, other.transform.position.z); break;
                    case EntranceType.Top: other.transform.position = new Vector3(other.transform.position.x, other.transform.position.y - 1f, other.transform.position.z); break;
                    case EntranceType.Bottom: other.transform.position = new Vector3(other.transform.position.x, other.transform.position.y + 1f, other.transform.position.z); break;
                }
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
        public bool blocked = false;
    }
}
