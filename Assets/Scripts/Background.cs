using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Background : MonoBehaviour {
    public Tilemap[] tilemap;
    public Tilemap overlay;
    public Tilemap board;
    public float scaleFactor; //assume a 4:3 aspect ratio
    float defaultWidth = 895;
    float defaultHeight = 559;

    public static Background instance = null;

    private void Awake()
    {
        if (instance==null){
            instance = this;
        }
        else if (instance!=this){
            Destroy(gameObject);
        }
        tilemap = (Tilemap[])Tilemap.FindObjectsOfType(typeof(Tilemap));
        board = tilemap[0];
        overlay = tilemap[1];
        float pixelWidth = Screen.width;
        float defaultDpi = 221;
        float deviceDpi = Screen.dpi;
        float defaultSize = 895 / defaultDpi;
        float deviceSize = pixelWidth / deviceDpi;
        scaleFactor = deviceSize / defaultSize;
        Debug.Log("scale factor " + scaleFactor);
    }

    public void scaleTile(GameObject go, float scaleX,float scaleY)
    {
        go.transform.localScale = new Vector3(scaleFactor*scaleX,scaleFactor*scaleY);
    }

    public void changeBoardTileSprite(Vector2 worldPosition,Sprite tileColor)
    {
        //Vector3Int v2Int = new Vector3Int(getCol(screenPoint.x), getRow(screenPoint.y), 0);
        Vector3Int cell = board.WorldToCell(worldPosition);
        Tile newtile = new Tile();
        newtile.sprite = tileColor;
        board.SetTile(cell, newtile);
    }

    public void changeOverlayTileSprite(Vector2 worldPosition, Sprite tileColor)
    {
        //Vector3Int v2Int = new Vector3Int(getCol(screenPoint.x), getRow(screenPoint.y), 0);
        Vector3Int cell = overlay.WorldToCell(worldPosition);
        Tile newtile = new Tile();
        newtile.sprite = tileColor;
        overlay.SetTile(cell, newtile);
    }

    public void revealTiles(Vector2 fromIndustry,Vector2 toIndustry)
    {
        //Vector3Int v2Int = new Vector3Int(getCol(screenPoint.x), getRow(screenPoint.y), 0);
        Vector3Int cellFrom = overlay.WorldToCell(fromIndustry);
        Vector3Int cellTo = overlay.WorldToCell(toIndustry);
        overlay.SetTile(cellFrom, null);
        overlay.SetTile(cellTo, null);
    }

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
