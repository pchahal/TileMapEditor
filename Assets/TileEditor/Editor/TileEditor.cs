using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(TileSettings))]
public class TileEditor : Editor
{
    TileSettings tileEditorSettings;
    string currentTilePath;
    Ray ray;
    private Brush brush;
    public Vector3 mouseDown, mouseDrag, mouseUp;
    GameObject cursor, selectionPrefab;
    bool currentTileChanged = false;
    float tileWidth;
    TileEditorPreferences preferences;

    delegate void CreateTileDelegate(Vector3 v,GameObject parent);

    


    public void OnEnable()
    {
        mouseDrag = Vector3.zero;
        tileEditorSettings = (TileSettings)target;
        SceneView.onSceneGUIDelegate += SceneUpdate;

        preferences = AssetDatabase.LoadAssetAtPath("Assets/TileEditor/TileEditorPreferences.asset", typeof(TileEditorPreferences))as TileEditorPreferences;

        string cursorPath = "Assets/TileEditor/Prefabs/TileCursor.prefab";
        GameObject cursorPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(cursorPath, typeof(GameObject));     
        if (cursorPrefab)
        {
            cursor = (GameObject)EditorUtility.InstantiatePrefab(cursorPrefab);
        }
        string tileSelectionPath = "Assets/TileEditor/Prefabs/TileSelectionRect.prefab";
        selectionPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(tileSelectionPath, typeof(GameObject));            
       
        brush = new Brush();
        brush.LoadTileSet(preferences.lastBrush);  
        tileWidth = preferences.TileWidth;




      
    }

    public void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= SceneUpdate;	
        DestroyImmediate(cursor);
    }

    void UpdateCursorPosition(Vector3 mousePos)
    {        
        cursor.transform.localScale = new Vector3(tileWidth, tileWidth, tileWidth);
        Vector3 currentTile = new Vector3(GetTilePosition(mousePos.x), GetTilePosition(mousePos.y), 0);
        if (Vector3.Distance(currentTile, cursor.transform.position) > .1f)
            cursor.transform.position = currentTile;              
    }

    void SceneUpdate(SceneView sceneview)
    {        
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        Event e = Event.current;

        ray = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, -e.mousePosition.y + Camera.current.pixelHeight));
        Vector3 mousePos = ray.origin;
     
        //ADD tiles
        if (e.type == EventType.MouseDown && e.button == 0 && e.isMouse)
        {            
            GUIUtility.hotControl = controlID;
            mouseDown = new Vector3(GetTilePosition(mousePos.x), GetTilePosition(mousePos.y), 0); 
            e.Use();
        }

        //Fill add Bunch of Tiles in area
        else if (e.type == EventType.MouseUp && e.isMouse)
        {
            GameObject tileSelection = GameObject.Find("TileSelection");
            if (tileSelection)
                DestroyImmediate(tileSelection);            
            mouseUp = new Vector3(GetTilePosition(mousePos.x), GetTilePosition(mousePos.y), 0);
           
            if (e.button == 0)
            {
                GUIUtility.hotControl = controlID;

                if (preferences.lastBrush.Contains("Brushes"))
                {
                    if (mouseDown != mouseUp && e.shift)
                    {                    
                        FillTiles(mouseDown, mouseUp, CreateTile, GameObject.Find("Tiles"), e.alt);
                    }
                    else
                    {
                        GameObject parent = GameObject.Find("Tiles");
                        CreateTile(mouseUp, parent);                
                    }
                }
                else if (preferences.lastBrush.Contains("Doodads"))
                {
                    CreateDoodad(mouseUp, e.alt, e.shift);
                }
                else if (preferences.lastBrush.Contains("SpriteSheets"))
                {
                    CreateTileFromSpriteSheet(mouseUp);
                }
            }
            else if (e.button == 1 && mouseDown != mouseUp && e.shift)
            {                       
                FillTiles(mouseDown, mouseUp, DeleteTile, null, false);

            }                

            GUIUtility.hotControl = 0;               
        }            
        //Draw Tile Fill Selectino Rect
        else if (e.type == EventType.MouseDrag && e.isMouse)
        {       
            if (e.button == 0)
            {                                                                                       
                if (e.shift)
                {
                    GameObject parent = DestroyChildTiles("TileSelection");
                    FillTiles(mouseDown, cursor.transform.position, CreateTileSelection, parent, e.alt);
                }
                else
                {
                    GameObject parent = GameObject.Find("Tiles");
                    CreateTile(cursor.transform.position, parent);                
                }

            }
            else if (e.button == 1 && e.shift)
            {
                GameObject parent = DestroyChildTiles("TileSelection");
                FillTiles(mouseDown, cursor.transform.position, CreateTileSelection, parent, e.alt);
            }
            else if (e.button == 1)
            {
                DeleteTile(new Vector3(GetTilePosition(mousePos.x), GetTilePosition(mousePos.y), 0), null);
            }
        }

        //DELETE tiles
        else if (e.type == EventType.MouseDown && e.button == 1 && e.isMouse)
        {         
            GUIUtility.hotControl = controlID;
            mouseDown = new Vector3(GetTilePosition(mousePos.x), GetTilePosition(mousePos.y), 0);
            DeleteTile(mouseDown, null);                  
        }
       
        UpdateCursorPosition(mousePos);
        DrawGrid(tileWidth, tileWidth, preferences.GridColor);
    }

    void DeleteTile(Vector3 mousePos, GameObject parent)
    {              
        RaycastHit2D hit = Utility.GetFrontmostRaycastHit(mousePos);
        if (hit.collider != null)
        {            
            Undo.RevertAllInCurrentGroup();
            Undo.RegisterSceneUndo("Delete Selected Objects");
            LayerMask layer = Physics.DefaultRaycastLayers;
            if (hit.collider.gameObject != null)
            {
                layer = hit.collider.gameObject.layer;
                DestroyImmediate(hit.collider.gameObject);

            }
            brush.UpdateNeighborTiles(mousePos, tileWidth, layer, Vector3.zero);
        }
       
    }

    private void CreateTile(Vector3 mousePos, GameObject parent)
    {      
        GameObject obj;                            
        Undo.IncrementCurrentGroup();
        Vector3 aligned = new Vector3(GetTilePosition(mousePos.x), GetTilePosition(mousePos.y), 0.0f);
        obj = brush.GetTile(aligned, tileWidth, preferences.tint, parent);

        if (obj)
        {          
            Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);                
        }
    }

    private void CreateTileSelection(Vector3 mousePos, GameObject parent)
    {
        
        if (selectionPrefab)
        {                 
            GameObject newTile = (GameObject)PrefabUtility.InstantiatePrefab(selectionPrefab);
            newTile.transform.localScale = new Vector3(tileWidth, tileWidth, tileWidth);
            Vector3 aligned = new Vector3(GetTilePosition(mousePos.x), GetTilePosition(mousePos.y), 0.0f);
            newTile.transform.position = aligned;
            newTile.transform.parent = parent.transform;
       
        }        
    }

    private void FillTiles(Vector3 start, Vector3 end, CreateTileDelegate Create, GameObject parent, bool fill)
    {        

        float xMin = start.x < end.x ? start.x : end.x;
        float yMin = start.y < end.y ? start.y : end.y;
        float xMax = start.x > end.x ? start.x : end.x;
        float yMax = start.y > end.y ? start.y : end.y;

        int cols = (int)(Mathf.Abs(start.x - end.x) / tileWidth);
        int rows = (int)(Mathf.Abs(start.y - end.y) / tileWidth);
        if (!fill)
        {          
            for (int i = 0; i <= rows; i++)
                for (int j = 0; j <= cols; j++)
                {                
                    Create(new Vector3(xMin + j * tileWidth, yMin + i * tileWidth, 0), parent);
                }
        }
        else
        {            
            for (int i = 0; i <= rows; i++)
                Create(new Vector3(xMin, yMin + i * tileWidth, 0), parent);
            for (int i = 0; i <= rows; i++)
                Create(new Vector3(xMax, yMin + i * tileWidth, 0), parent);
            
            for (int i = 1; i < cols; i++)
                Create(new Vector3(xMin + i * tileWidth, yMin, 0), parent);
            for (int i = 1; i < cols; i++)
                Create(new Vector3(xMin + i * tileWidth, yMax, 0), parent);
        }
    }

    public float GetTilePosition(float pos)
    {    
       
        float sign = Mathf.Sign(pos);
        pos = Mathf.Abs(pos);

        int FullTiles = (int)(pos / tileWidth);
        int partTiles = 0;

        if (pos % tileWidth > .1)
            partTiles = 1;                       

        float Offset = FullTiles * tileWidth + tileWidth / 2;
        Offset *= sign;
        return Offset;
    }


    private void CreateDoodad(Vector3 mousePos, bool flipStairs, bool addAsChild)
    {
        GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(preferences.lastBrush, typeof(GameObject));
        if (prefab == null)
            return;
        GameObject doodad = (GameObject)EditorUtility.InstantiatePrefab(prefab);
      

        if (addAsChild)
        {    
            RaycastHit2D hit = Utility.GetFrontmostRaycastHit(mousePos);
            if (hit.collider != null && hit.collider.gameObject != null)
            {                        
                Debug.Log(hit.transform.name);
                doodad.transform.parent = hit.collider.transform;
                doodad.transform.localPosition = prefab.transform.localPosition;
            }
        }
        else
        {
            doodad.transform.position = new Vector3(mousePos.x, mousePos.y - tileWidth / 2, mousePos.z);
            if (flipStairs)
                doodad.transform.localScale = new Vector3(-1, 1, 1);
            GameObject parent = null;
            string name = "Doodads";
            if (doodad.name.Contains("Enemy"))
                name = "Enemy";
            parent = GameObject.Find(name);
            if (parent == null)
            {
                parent = new GameObject();
                parent.name = name;
            }
            doodad.transform.parent = parent.transform;
        }
        Undo.RegisterCreatedObjectUndo(doodad, "Create " + doodad.name); 

    }

    private void CreateTileFromSpriteSheet(Vector3 mousePos)
    {
        /* TEMP replace this by loading all sprites in spritesheetsFolder, drawing thumbnails in inpsector,add setting for sortinlayer and sortorder,
        GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(preferences.lastBrush, typeof(GameObject));
        if (prefab == null)
            return;
        GameObject tile = (GameObject)EditorUtility.InstantiatePrefab(prefab);
               
        tile.transform.position = new Vector3(mousePos.x, mousePos.y - tileWidth / 2, mousePos.z);          
        GameObject parent = null;
        parent = GameObject.Find("Tiles");
        if (parent == null)
        {
            parent = new GameObject();
            parent.name = "Tiles";
        }
        tile.transform.parent = parent.transform;       
        Undo.RegisterCreatedObjectUndo(tile, "Create " + tile.name); 
        */
    }


    private GameObject DestroyChildTiles(string name)
    {
        GameObject parent = GameObject.Find(name);
        if (parent)
            DestroyImmediate(parent);
        parent = new GameObject();
        parent.name = name;
        return parent;
    }

    public override void OnInspectorGUI()
    {	
  
       
        string[] assetsPaths = AssetDatabase.GetSubFolders(preferences.brushFolder);

        preferences.brushFoldout = EditorGUILayout.Foldout(preferences.brushFoldout, "Brushes");      
        if (preferences.brushFoldout)
        {
            for (int i = 0; i < assetsPaths.Length; i++)
            {
                string[] tileFileName = assetsPaths[i].Split('/', '.');
                string tileType = tileFileName[tileFileName.Length - 1];      

                if (GUILayout.Button(tileType, GUILayout.Width(100), GUILayout.Height(20)))
                {                   
                    brush.LoadTileSet(assetsPaths[i]);   
                    preferences.lastBrush = assetsPaths[i];

                }
            }
        }
        preferences.doodadsFoldout = EditorGUILayout.Foldout(preferences.doodadsFoldout, "Doodads");
        if (preferences.doodadsFoldout)
        {
         
            List<GameObject> prefabs = Utility.GetPrefabsInFolder(preferences.DoodadsFolder, ".prefab");
            foreach (var prefab in prefabs)
            {
                if (GUILayout.Button(prefab.name, GUILayout.Width(100), GUILayout.Height(20)))
                {                   
                   
                    preferences.lastBrush = preferences.doodadsFolder + prefab.name + ".prefab";                        

                   
                }            
            }
        }

        preferences.spritesheetsFoldout = EditorGUILayout.Foldout(preferences.spritesheetsFoldout, "SpriteSheets");
        if (preferences.spritesheetsFoldout)
        {

            List<GameObject> prefabs = Utility.GetPrefabsInFolder(preferences.spritesheetsFolder, ".prefab");
            foreach (var prefab in prefabs)
            {
                if (GUILayout.Button(prefab.name, GUILayout.Width(100), GUILayout.Height(20)))
                {                   

                    preferences.lastBrush = preferences.spritesheetsFolder + prefab.name + ".prefab";                        


                }            
            }
        }

        preferences.settingsFoldout = EditorGUILayout.Foldout(preferences.settingsFoldout, "Settings");      
        if (preferences.settingsFoldout)
        {
            
            preferences.TileWidth = Mathf.Clamp(EditorGUILayout.FloatField("Tile Width", preferences.TileWidth, GUILayout.MinWidth(100)), .1f, 1000);
            preferences.tint = EditorGUILayout.ColorField("Tile Tint", preferences.tint, GUILayout.MinWidth(100));
            preferences.GridColor = EditorGUILayout.ColorField("Grid Color", preferences.GridColor, GUILayout.MinWidth(100));

        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("RESET", GUILayout.MinWidth(100), GUILayout.Height(35)))
        {           
            DestroyChildTiles("Tiles");
            DestroyChildTiles("Doodads");
        }

        if (GUILayout.Button("SAVE", GUILayout.MinWidth(100), GUILayout.Height(35)))
        {
            brush.RemoveExtraCollider();
        }           
        tileWidth = preferences.TileWidth;
        GUILayout.EndHorizontal();
        SceneView.RepaintAll();
       
    }


    void DrawGrid(float width, float height, Color color)
    {
        Vector3 camPos = Camera.current.transform.position;
        Handles.color = color;

        for (float y = camPos.y - 1000.0f; y < camPos.y + 1000.0f; y += height)
        {
            Handles.DrawLine(new Vector3(-1000.0f, Mathf.Floor(y / height) * height, 0.0f),
                new Vector3(1000.0f, Mathf.Floor(y / height) * height, 0.0f));
        }

        for (float x = camPos.x - 1000.0f; x < camPos.x + 1000.0f; x += width)
        {
            Handles.DrawLine(new Vector3(Mathf.Floor(x / width) * width, -1000.0f, 0.0f),
                new Vector3(Mathf.Floor(x / width) * width, 1000.0f, 0.0f));
        }
    }




 
}
