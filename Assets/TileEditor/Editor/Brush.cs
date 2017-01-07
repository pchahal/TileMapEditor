using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;



public class Brush:Editor
{
    private List<GameObject> tiles;
    private LayerMask layerMask;
    string tag;

    [System.Flags] public enum TileMask
    {
        None = 0,
        //0000
        Left = 1,
        //0001
        Right = 2,
        //0010
        Top = 4,
        //0100
        Bottom = 8
        //1000
    }


    public TileMask GetTileMask(Vector3 pos, float width, LayerMask layer)
    {
        TileMask mask = 0;
     
        RaycastHit2D leftHit = Utility.GetFrontmostRaycastHit(pos + new Vector3(-width, 0, 0), tag, layer);
        if (leftHit.collider != null)
        {
            mask = mask | TileMask.Left;        
        }

        RaycastHit2D rightHit = Utility.GetFrontmostRaycastHit(pos + new Vector3(width, 0, 0), tag, layer);
        if (rightHit.collider != null)
        {
            mask = mask | TileMask.Right;           
        }

        RaycastHit2D topHit = Utility.GetFrontmostRaycastHit(pos + new Vector3(0, width, 0), tag, layer);
        if (topHit.collider != null)
        {
            mask = mask | TileMask.Top;
        }

        RaycastHit2D bottomHit = Utility.GetFrontmostRaycastHit(pos + new Vector3(0, -width, 0), tag, layer);
        if (bottomHit.collider != null)
        {
            mask = mask | TileMask.Bottom;
        }   
        return mask;
    }

    public List<Collider2D> GetNeighbors(Vector3 position, float width, LayerMask layer)
    {
        List<Collider2D> colliders = new List<Collider2D>();
        colliders.Clear();


        RaycastHit2D leftHit = Utility.GetFrontmostRaycastHit(position + new Vector3(-width, 0, 0), tag, layer);
        if (leftHit.collider != null)
        {
            colliders.Add(leftHit.collider);
        }

        RaycastHit2D rightHit = Utility.GetFrontmostRaycastHit(position + new Vector3(width, 0, 0), tag, layer);
        if (rightHit.collider != null)
        {         
            colliders.Add(rightHit.collider);
        }

        RaycastHit2D topHit = Utility.GetFrontmostRaycastHit(position + new Vector3(0, width, 0), tag, layer);
        if (topHit.collider != null)
        {         
            colliders.Add(topHit.collider);
        }

        RaycastHit2D bottomHit = Utility.GetFrontmostRaycastHit(position + new Vector3(0, -width, 0), tag, layer);
        if (bottomHit.collider != null)
        {         
            colliders.Add(bottomHit.collider);
        }

        return colliders;
    }



    private int GetPrefabHelper(TileMask mask)
    {
        string maskStr = mask.ToString();
        var result = System.Convert.ToString((int)mask, 2).PadLeft(4, '0');

        int index = 0;
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].name == result)
                index = i;
        }      
        return index;
    }


    private int GetTilePrefab(TileMask mask)
    {
        int index = 0;
        TileMask testMask = TileMask.Top | TileMask.Bottom | TileMask.Right;
        if (mask == testMask)
            index = GetPrefabHelper(TileMask.Left);
        
        testMask = TileMask.Top | TileMask.Left | TileMask.Right;
        if (mask == testMask)
            index = GetPrefabHelper(TileMask.Bottom);
        testMask = TileMask.Bottom | TileMask.Left | TileMask.Right;
        if (mask == testMask)
            index = GetPrefabHelper(TileMask.Top);

        testMask = TileMask.Bottom | TileMask.Left | TileMask.Top;
        if (mask == testMask)
            index = GetPrefabHelper(TileMask.Right);

        testMask = TileMask.Right | TileMask.Bottom;
        if (mask == testMask)
            index = GetPrefabHelper(TileMask.Left | TileMask.Top);

        testMask = TileMask.Top | TileMask.Right;           
        if (mask == testMask)
            index = GetPrefabHelper(TileMask.Left | TileMask.Bottom);

        testMask = TileMask.Left | TileMask.Top;
        if (mask == testMask)
            index = GetPrefabHelper(TileMask.Right | TileMask.Bottom);

        testMask = TileMask.Left | TileMask.Bottom;
        if (mask == testMask)
            index = GetPrefabHelper(TileMask.Top | TileMask.Right);

        testMask = TileMask.Left | TileMask.Right;
        if (mask == testMask)
            index = GetPrefabHelper(TileMask.Top | TileMask.Bottom);

        testMask = TileMask.Top | TileMask.Bottom;
        if (mask == testMask)
            index = GetPrefabHelper(TileMask.Right | TileMask.Left);


        if (mask == TileMask.Left)
            index = GetPrefabHelper(TileMask.Top | TileMask.Right | TileMask.Bottom);
        
        if (mask == TileMask.Bottom)
            index = GetPrefabHelper(TileMask.Top | TileMask.Right | TileMask.Left);
        

        if (mask == TileMask.Right)
            index = GetPrefabHelper(TileMask.Top | TileMask.Bottom | TileMask.Left);
        
        if (mask == TileMask.Top)
            index = GetPrefabHelper(TileMask.Right | TileMask.Bottom | TileMask.Left);
        
        return index;
    }


    public void UpdateNeighborTiles(Vector3 position, float width, LayerMask layer, Vector3 pivotOffset)
    {

        List<Collider2D> neighborColliders = GetNeighbors(position, width, layer);

        for (int i = 0; i < neighborColliders.Count; i++)
        {
            
            TileMask mask = GetTileMask(neighborColliders[i].transform.position - pivotOffset, width, layer);
            int index = GetTilePrefab(mask);          

            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(tiles[index]) as GameObject;
            obj.transform.position = neighborColliders[i].transform.position;
            obj.transform.rotation = neighborColliders[i].transform.rotation;
            obj.transform.parent = neighborColliders[i].transform.parent;
            obj.transform.localScale = new Vector3(width, width, width);
            obj.name = mask.ToString();
            if (neighborColliders[i].GetComponent<SpriteRenderer>() != null && obj.GetComponent<SpriteRenderer>() != null)
                obj.GetComponent<SpriteRenderer>().color = neighborColliders[i].GetComponent<SpriteRenderer>().color;



            if (obj.GetComponent<BoxCollider2D>() == null)
                obj.AddComponent<BoxCollider2D>();
            GameObject.DestroyImmediate(neighborColliders[i].gameObject);

        }
            
    }

    public GameObject GetTile(Vector3 position, float width, Color color, GameObject parent)
    {
        if (tiles == null)
            return null;

       
        GameObject newTile;

        TileMask mask = GetTileMask(position, width, layerMask);
        int index = GetTilePrefab(mask);

        RaycastHit2D hit = Utility.GetFrontmostRaycastHit(position, tag, layerMask);
        if (hit.collider != null)
            newTile = hit.collider.gameObject;
        else
            newTile = (GameObject)PrefabUtility.InstantiatePrefab(tiles[index]);
        
        newTile.transform.localScale = new Vector3(width, width, width);
        newTile.transform.position = position;
        newTile.name = mask.ToString();
        newTile.transform.parent = parent.transform;


       
        Vector3 pivotOffset = Vector3.zero;
        if (newTile.GetComponent<SpriteRenderer>() != null)
        {
            //  newTile.GetComponent<SpriteRenderer>().color = color;
            Sprite sprite = newTile.GetComponent<SpriteRenderer>().sprite;
            Vector2 pivot = sprite.pivot;           
            pivotOffset = new Vector3(pivot.x / sprite.rect.width - .5f, pivot.y / sprite.rect.height - .5f, 0);
            position += pivotOffset;
            newTile.transform.position += pivotOffset;           
        }
        if (newTile.GetComponent<BoxCollider2D>() == null)
            newTile.AddComponent<BoxCollider2D>();       

        UpdateNeighborTiles(position, width, layerMask, pivotOffset);
        return newTile;
    }



    public void LoadTileSet(string assetPath)
    {        
        if (assetPath == "")
            return;
        tiles = Utility.GetPrefabsInFolder(assetPath, ".prefab");

        if (tiles[0].layer != null)
        {
            layerMask = 1 << tiles[0].layer;
        }
        else
        {
            layerMask = Physics.DefaultRaycastLayers;
        }
        tag = tiles[0].tag;
    }

  
    public void RemoveExtraCollider()
    {  
        GameObject tiles = GameObject.Find("Tiles");
        foreach (Transform t in tiles.transform)
        {           
            GameObject prefab = PrefabUtility.GetPrefabParent(t.gameObject) as GameObject;

            if (prefab.GetComponent<BoxCollider2D>() == null)
            {
                if (t.GetComponent<BoxCollider2D>() != null)
                    DestroyImmediate(t.GetComponent<BoxCollider2D>());
            }
        }
    }
}
