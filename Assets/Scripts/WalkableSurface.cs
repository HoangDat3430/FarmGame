using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm
{
    /// <summary>
    /// Add this component to the tilemap that is defining the surface the player is walking on. This is used by the
    /// sound system to determine which sounds to play when walking.
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class WalkableSurface : MonoBehaviour
    {
        private void Awake()
        {
            GameManager.Instance.WalkSurfaceTilemap = GetComponent<Tilemap>();
        }
    }
}
