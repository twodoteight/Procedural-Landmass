using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    public const float maxViewDistance = 300;
    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.effectiveChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }
    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordinatesX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordinatesY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoordinates = new Vector2(currentChunkCoordinatesX + xOffset, currentChunkCoordinatesY + yOffset);
                if (terrainChunkDictionary.ContainsKey(viewedChunkCoordinates))
                {
                    TerrainChunk chunk = terrainChunkDictionary[viewedChunkCoordinates];
                    chunk.UpdateTerrainChunk();
                    if (chunk.IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(chunk);
                    }
                }
                else
                {
                    TerrainChunk newChunk = new TerrainChunk(viewedChunkCoordinates, chunkSize, transform, mapMaterial);
                    terrainChunkDictionary.Add(viewedChunkCoordinates, newChunk);
                }
            }
        }
    }
    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MapData mapData;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        public TerrainChunk(Vector2 coordinates, int size, Transform parent, Material material)
        {
            position = coordinates * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 position3D = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            
            meshRenderer.material = material;
            meshObject.transform.position = position3D;
            meshObject.transform.parent = parent;
            SetVisible(false);

            mapGenerator.RequestMapData(OnMapDataReceieved);
        }

        void OnMapDataReceieved(MapData mapData)
        {
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceieved);
        }

        void OnMeshDataReceieved(MeshData meshData)
        {
            //print("OnMeshDataReceieved");
            meshFilter.mesh = meshData.CreateMesh();        
        }
        
        public void UpdateTerrainChunk()
        {
            float viewerDistanceToNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistanceToNearestEdge <= maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
}
