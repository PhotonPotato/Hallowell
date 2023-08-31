using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CustomLevelDesign : MonoBehaviour
{
    public GameObject parentLevelObject;
    public List<PhysicalBlock> allPysicalBlocks;
    public List<PhysicalRectangle> allRects;
    public Sprite texture;
    public float defaultTextureHeight;
    public float defaultColHeight;
    public float defaultColOffset;
    public bool spawnWithColliders;

    [Space]

    public bool filterResults;
    public ContactFilter2D detailPlacementFilter;
    public float maxYDiffFromClusterOrg = .5f;
    public GameObject[] detailsToUse;
    public GameObject detailsParent;
    public bool randomClusters;
    public int amtClusters;
    public int detailsPerCluster;
    public float clusterDensity = .5f;
    public bool randomlyFlipX;
    public float mapXMin;
    public float mapXMax;

    public void Awake()
    {
        allPysicalBlocks = new List<PhysicalBlock>();
        allRects = new List<PhysicalRectangle>();
    }

    public void generateNewPhysBlock()
    {
        PhysicalBlock newBlock = new PhysicalBlock(new Vector2(0, 0), new Vector2(0, 1), texture, 0, 0, 0, defaultTextureHeight, defaultColHeight, defaultColOffset);
        GameObject obj = new GameObject();
        obj.transform.SetParent(parentLevelObject.transform);
        SpriteRenderer sprite = obj.AddComponent<SpriteRenderer>();
        sprite.sprite = texture;
        sprite.drawMode = SpriteDrawMode.Tiled;
        newBlock.blockObj = obj;

        if (spawnWithColliders)
        {
            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1, 1);
        }

        obj.name = "Ground Piece " + (allPysicalBlocks.Count + 1);
        obj.tag = "Ground";
        obj.layer = 0;

        allPysicalBlocks.Add(newBlock);
    }

    public void generateNewPhysBlock(Vector2 overrideRPt, Vector2 overrideLPt, int type, int id, int indivID)
    {
        PhysicalBlock newBlock = new PhysicalBlock(overrideLPt, overrideRPt, texture, type, id, indivID, defaultTextureHeight, defaultColHeight, defaultColOffset);
        GameObject obj = new GameObject();
        obj.transform.SetParent(parentLevelObject.transform);
        SpriteRenderer sprite = obj.AddComponent<SpriteRenderer>();
        sprite.sprite = texture;
        sprite.drawMode = SpriteDrawMode.Tiled;
        newBlock.blockObj = obj;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1, 1);
        obj.name = "Rect Piece " + indivID + "of " + id;
        obj.tag = "Ground";
        obj.layer = 0;

        allPysicalBlocks.Add(newBlock);
    }

    public void generateRectangleOfBlocks()
    {
        //Generate 4 blocks.
        generateNewPhysBlock(new Vector2(1, 0), new Vector2(1, 1), 1, 0, 0);
        generateNewPhysBlock(new Vector2(1, 1), new Vector2(0, 1), 1, 0, 1);
        generateNewPhysBlock(new Vector2(0, 1), new Vector2(0, 0), 1, 0, 2);
        generateNewPhysBlock(new Vector2(0, 0), new Vector2(1, 0), 1, 0, 3);
    }

    public void deletePhysBlock(int index)
    {
        if (allPysicalBlocks.Count <= index) return;

        DestroyImmediate(allPysicalBlocks[index].blockObj);
        allPysicalBlocks.RemoveAt(index);
    }

    public void generateClusters()
    {
        //Make blobs of details around the map using raycasts.
        int clusterSize = Random.Range(2, 6);
        float mapXSize = mapXMax - mapXMin;

        //float increments = mapXSize /

        for (int i = 0; i < amtClusters; i++)
        {
            if (randomClusters)
            {
                float clustX = Random.Range(mapXMin, mapXMax);
                float clustY;
                if (filterResults)
                {
                    RaycastHit2D[] rayList = new RaycastHit2D[1];
                    Physics2D.Raycast(new Vector2(clustX, 100), Vector2.down, detailPlacementFilter, rayList);
                    clustY = rayList[0].point.y;
                }
                else clustY = Physics2D.Raycast(new Vector2(clustX, 100), Vector2.down).point.y;

                for (int j = 0; j < clusterSize; j++)
                {
                    //Get a random number weighted towards 0 and then mult by density factor.
                    float detOffset = RandomFromDistribution.RandomNormalDistribution(0, 1);// ;
                    //Debug.Log(detOffset);
                    generateDetailFromCluster(clustX + (detOffset * clusterDensity), clustY);
                }
            }
            else
            {

            }
        }
    }

    public void generateDetailFromCluster(float genX, float clusterY)
    {
        RaycastHit2D[] rayList = new RaycastHit2D[1];
        if (filterResults) Physics2D.Raycast(new Vector2(genX, 100), Vector2.down, detailPlacementFilter, rayList);
        else rayList[0] = Physics2D.Raycast(new Vector2(genX, 100), Vector2.down);

        if (rayList.Length == 0) return;

        foreach (RaycastHit2D hit in rayList)
        {
            if (Mathf.Abs(hit.point.y - clusterY) < maxYDiffFromClusterOrg)
            {
                //generateRandomDetailAtPoint(hit.point);
            }
        }
    }

    /// <TH>
    /// THIS IS JUST REMOVED FOR BUILDS
    /// ADD BACK WHEN DONE FOR USE OF CUSTOM EDITOR

    /*
    public void generateRandomDetailAtPoint(Vector2 point)
    {
        //Get random index.
        int index = Random.Range(0, detailsToUse.Length);

        //Weird way of instantiating prefabs.
        GameObject newDetail = PrefabUtility.InstantiatePrefab(detailsToUse[index].gameObject as GameObject) as GameObject;
        SpriteRenderer renderer = newDetail.GetComponent<SpriteRenderer>();

        //Get the resolution and size of the actual display image.
        float spriteResolution = renderer.sprite.pixelsPerUnit;
        float spritePixels = renderer.sprite.rect.height;
        //Now use the res and size to determin the actual height in the world and divide by 2 to get the center.
        float yOffset = spritePixels / spriteResolution / 2 * newDetail.transform.localScale.y;

        //Flip the sprite horizontally randomly
        if (randomlyFlipX) renderer.flipX = (Random.Range(0, 1) == 0);

        //Move the object up based on this value.
        newDetail.transform.position = new Vector3(point.x, point.y + yOffset, 0);
        newDetail.transform.SetParent(detailsParent.transform);
    }*/
}
