using UnityEngine;

namespace Borboerue
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private ETileType Type { get; set; }
        [SerializeField] private GameObject TileObject { get; set; }

        public ETileType GetTileType()
        {
            return Type;
        }

        public void SetTileType(ETileType type)
        {
            Type = type;
        }

        public GameObject GetTileObject()
        {
            return TileObject;
        }

        public void SetTileObject(GameObject tileObject)
        {
            TileObject = tileObject;
        }
    }
}