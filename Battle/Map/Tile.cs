using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour{

    //main attributes
    public MeshRenderer mesh;
    public Vector2Int gridPosition;
    public int height;
    public TileType type;
    public bool blocked;
    public Tile elevatedTile;

    //used by pathfinding
    /*public int hCost;
    public int gCost;
    public int fCost { get { return hCost + gCost; } }*/
    public Tile parent;

    public BattleCharacter character; //THIS SHOULD ALWAYS BE SET USING SetCharacterOnTile() OR IT WON'T UPDATE AURAS
    public int[] elements;
    public List<Skill> aurasList = new List<Skill>();

    void Start(){

        switch (type){
            case TileType.Grass:
                elements = new int[6] { -1, 1, 0, 0, 1, -1 };
                break;
            case TileType.Water:
                elements = new int[6] { -1, 1, 1, -1, 0, 0 };
                break;
            case TileType.Stone:
                elements = new int[6] { -1, -1, 1, 1, 0, 0 };
                break;
            case TileType.Earth:
                elements = new int[6] { 0, 0, -1, 1, -1, 1 };
                break;
        }
    }
}