using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class TileEditorPreferences : ScriptableObject
{
    public float TileWidth = 1;
    public Color tint = Color.white;
    public Color GridColor = Color.white;
    public string lastBrush;


    public bool brushFoldout = false;
    public bool doodadsFoldout = false;
    public bool settingsFoldout = false;
    public bool spritesheetsFoldout = false;
    public string brushFolder = "Assets/TileEditor/Brushes";
    public string doodadsFolder = "Assets/TileEditor/Doodads";
    public string spritesheetsFolder = "Assets/TileEditor/SpriteSheets";


    public string DoodadsFolder
    {
        get
        { 
            if (doodadsFolder.EndsWith("/") == false)
                doodadsFolder += "/";             
            return doodadsFolder; 
        }
        set { }
    }

  
}
