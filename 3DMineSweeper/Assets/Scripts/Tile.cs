using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType { Blank, Mine, Num }

    public TileType type = TileType.Blank;
    public int mineNeighbors = 0;
    public bool isCovered = true;
    public bool isFlaged = false;
    public Sprite coverSprite;
    public Sprite flagSprite;
    public Sprite hitMineSprite;
    public Sprite missFlagSprite;
    public Sprite highlightSprite;
    private Sprite defaultSprite;

    private AudioManager audioManager;

    private void Start()
    {
        defaultSprite = GetComponent<SpriteRenderer>().sprite;
        GetComponent<SpriteRenderer>().sprite = coverSprite;
        audioManager = FindObjectOfType<AudioManager>();
    }

    /* sets the icon and type of the tile
     * icon is the string name of the icon for the tile in the graphics folder
     * tile is the TileType val
     */
    public void SetTile(string icon, TileType tile)
    {
        defaultSprite = Resources.Load<Sprite>("Graphics/" + icon);
        type = tile;
    }

    //Sets the number of how many tiles are mines
    public void SetMineNeighbors(int mines)
    {
        mineNeighbors = mines;
    }

    //Reveals the tile, setting the sprite to what the tile is (aka mine, empty, or number tile)
    public void RevealTile()
    {
        isCovered = false;
        isFlaged = false;
        GetComponent<SpriteRenderer>().sprite = defaultSprite;

        audioManager.Play("RevealTile");
    }

    //covers a tile
    public void CoverTile()
    {
        isCovered = true;
        isFlaged = false;
        GetComponent<SpriteRenderer>().sprite = coverSprite;
    }

    //changes tiles icon to a flag tile sprite
    public void FlagTile()
    {
        isFlaged = true;
        GetComponent<SpriteRenderer>().sprite = flagSprite;

        audioManager.Play("Flag");
    }

    //changes tiles icon to a covered tile sprite (removed a flag)
    public void UnflagTile()
    {
        isFlaged = false;
        GetComponent<SpriteRenderer>().sprite = coverSprite;

        audioManager.Play("Unflag");
    }

    //changes tiles icon to a hit mine sprite (player revealed a mine)
    public void HitMine()
    {
        GetComponent<SpriteRenderer>().sprite = hitMineSprite;

        audioManager.Play("Defeat");
    }

    //changes tiles icon to a miss flag sprite (flag was placed on a safe tile)
    public void MissFlag()
    {
        GetComponent<SpriteRenderer>().sprite = missFlagSprite;
    }

    //changes tiles icon to a highlighted tile sprite
    public void HighlightTile()
    {
        GetComponent<SpriteRenderer>().sprite = highlightSprite;
    }

    //changes tiles icon to a covered tile sprite
    public void UnHighlightTile()
    {
        GetComponent<SpriteRenderer>().sprite = coverSprite;
    }

}
