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

        public void Init(ETileType type, Vector3 position, GameObject tileObject)
        {
            Type = type;
            CreateTile(position, tileObject);
        }

        public void CreateTile(Vector3 position, GameObject tileObject)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            var mf = gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>();
            gameObject.AddComponent<BoxCollider>();

            mf.mesh = cube.GetComponent<MeshFilter>().mesh;
            Destroy(cube);

            gameObject.transform.position = position;
            gameObject.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            gameObject.transform.parent = tileObject.transform;

            TileObject = tileObject;

            GetTileObject().GetComponent<Tile>().SetTileType(Type);
            GetTileObject().GetComponent<Tile>().SetTileObject(TileObject);
        }
    }
}