using System.Collections.Generic;
using UnityEngine;



public static class Utility
{
    public enum layers
    {
        Ground = 8,
        Player,
        Enemy,
        Obstactle,
    }

    public static bool isEqual(this float a, float b)
    {
        float epsilon = 1f;
        if (a >= b - epsilon && a <= b + epsilon)
            return true;
        else
            return false;
    }


    public static GameObject CleaChildren(string name)
    {
        GameObject parentTile = GameObject.Find(name);
        if (parentTile == null)
        {
            parentTile = new GameObject();
            parentTile.name = name;

        }
        return parentTile;
    }

    public static LayerMask GetLayer(string name)
    {

        LayerMask layer = 1 << LayerMask.NameToLayer(name);
        return layer;
    }

    #if UNITY_EDITOR
    public static List<GameObject> GetPrefabsInFolder(string assetPath, string endsWith = "")
    {        
        List <GameObject> files = new List<GameObject>();


        string absolutpath = UnityEditor.EditorApplication.applicationPath + "/" + assetPath;        
        try
        {
            string[] aFilePaths = System.IO.Directory.GetFiles(assetPath);  
            foreach (string sFilePath in aFilePaths)
            {                        
                GameObject objAsset = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(sFilePath, typeof(Object));

                if (sFilePath.EndsWith(".prefab"))
                {           
                    files.Add(objAsset);
                }               
            }            
            return files;
        }
        catch (System.Exception ex)
        {
            return files;
        }


    }
    #endif

    public static Vector2 AngleToVector(float angle)
    {
        float radians = Mathf.Deg2Rad * angle;
        Vector2 v = new Vector2((float)Mathf.Cos(radians), (float)Mathf.Sin(radians));
        return v.normalized;
    }

    public static RaycastHit2D GetFrontmostRaycastHit(Vector3 clickPosition, string tag = "", int layer = Physics.DefaultRaycastLayers)
    {
        

        SpriteRenderer spriteRenderer;
        RaycastHit2D[] hits = Physics2D.LinecastAll(clickPosition, clickPosition, layer);

        if (hits.Length != 0)
        {
            // A variable that will store the frontmost sorting layer that contains an object that has been clicked on as an int.
            var topSortingLayer = 0;
            // A variable that will store the index of the top sorting layer as an int.
            int indexOfTopSortingLayer;
            // An array that stores the IDs of all the sorting layers that contain a sprite in the path of the linecast.
            var sortingLayerIDArray = new int[hits.Length];
            // An array that stores the sorting orders of each sprite that has been hit by the linecast
            var sortingOrderArray = new int[hits.Length];
            // An array that stores the sorting order number of the frontmost sprite that has been clicked.
            var topSortingOrder = 0;
            // A variable that will store the index in the sortingOrderArray where topSortingOrder is. This index used with the hits array will give us our frontmost clicked sprite.
            int indexOfTopSortingOrder = 0;

            // Loop through the array of raycast hits...
            for (var i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.gameObject.GetComponent<SpriteRenderer>() == null)
                {
                    sortingLayerIDArray[i] = 0;
                    sortingOrderArray[i] = 0;
                }
                else
                {
                    // Get the SpriteRenderer from each game object under the click.
                    spriteRenderer = hits[i].collider.gameObject.GetComponent<SpriteRenderer>();

                    // Access the sortingLayerID through the SpriteRenderer and store it in the sortingLayerIDArray.
                    sortingLayerIDArray[i] = spriteRenderer.sortingLayerID;

                    // Access the sortingOrder through the SpriteRenderer and store it in the sortingOrderArray.
                    sortingOrderArray[i] = spriteRenderer.sortingOrder;
                }
            }

            // Loop through the array of sprite sorting layer IDs...
            for (var j = 0; j < sortingLayerIDArray.Length; j++)
            {
                // If the sortingLayerID is higher that the topSortingLayer...
                if (sortingLayerIDArray[j] >= topSortingLayer)
                {
                    topSortingLayer = sortingLayerIDArray[j];
                    indexOfTopSortingLayer = j;
                }
            }

            // Loop through the array of sprite sorting orders...
            for (var k = 0; k < sortingOrderArray.Length; k++)
            {
                // If the sorting order of the sprite is higher than topSortingOrder AND the sprite is on the top sorting layer...
                if (sortingOrderArray[k] >= topSortingOrder && sortingLayerIDArray[k] == topSortingLayer)
                {
                    topSortingOrder = sortingOrderArray[k];
                    indexOfTopSortingOrder = k;
                }
                else
                {
                    // Do nothing and continue loop.
                }
            }

            // How many sprites with colliders attached are underneath the click?
            //  Debug.Log("How many sprites have been clicked on: " + hits.Length);

            // Which is the sorting layer of the frontmost clicked sprite?
            // Debug.Log("Frontmost sorting layer ID: " + topSortingLayer);

            // Which is the order in that sorting layer of the frontmost clicked sprite?
            //Debug.Log("Frontmost order in layer: " + topSortingOrder);

            // The indexOfTopSortingOrder will also be the index of the frontmost raycast hit in the array "hits". 
            if (hits[indexOfTopSortingOrder].collider.tag == tag || tag == "")
                return hits[indexOfTopSortingOrder];
            else
                return new RaycastHit2D();
        }
        else // If the hits array has a length of 0 then nothing has been clicked...
        {
            //Debug.Log("Nothing clicked.");
            return new RaycastHit2D();
        }
    }




    public static Texture2D CreateTexture(int x, int y, int width, int height, Color[] colors)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture.SetPixels(colors);
        return texture;
    }

    public static Texture2D ReadTexture(Sprite sprite)
    {         
        int x = (int)sprite.textureRect.x;
        int y = (int)sprite.textureRect.y;
        int width = (int)sprite.textureRect.width;
        int height = (int)sprite.textureRect.height;
        Texture2D texture = sprite.texture;
        Color[] colors = new Color[width * height];
        colors = texture.GetPixels(x, y, width, height);       
        Texture2D newTex = CreateTexture(x, y, width, height, colors);
        return newTex;
    }

}


