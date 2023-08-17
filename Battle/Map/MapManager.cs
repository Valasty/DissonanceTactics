//NUM DA PRA MAPEAR OS TILES AUTOMATICAMENTE USANDO RAYCAST DO TOPO? CRIO UMA FUNÇAO COM A LARGURA DO RAYAST NO MAPA E ELE AUTOMATICAMENTE GERA OS TILE PRA MIM... NUM ROLA?

using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

    public Vector2Int mapSize;
    Tile[,] grid;
    List<Tile> inMoveTiles = new List<Tile>();
    List<Tile> inRangeTiles = new List<Tile>();
    List<Tile> inAreaTiles = new List<Tile>();
        
    public void GenerateGrid() {

        GameObject[] listTiles = GameObject.FindGameObjectsWithTag("Tile");
        grid = new Tile[mapSize.x, mapSize.y];

        int counter = 0;
        for (int y = 0; y < mapSize.y; y++){
            for (int x = 0; x < mapSize.x; x++) {
                grid[x, y] = listTiles[counter].GetComponent<Tile>();
                grid[x, y].gridPosition = new Vector2Int(x, y);
                grid[x, y].height = (int)(grid[x, y].transform.position.y * 5);
                if (grid[x, y].elevatedTile != null){
                    grid[x, y].elevatedTile.gridPosition = grid[x, y].gridPosition;
                    grid[x, y].elevatedTile.height = (int)(grid[x, y].elevatedTile.transform.position.y * 5f);
                }
                counter++;
            }
        }
    }

    public bool CheckTileInRange(Tile tile, ActionType actionType){

        List<Tile> selectedTiles = (actionType == ActionType.MoveSelect) ? inMoveTiles : inRangeTiles;
        return selectedTiles.Contains(tile) ? true : false;
    }

    public Tile GetTile(Vector2Int position){

        return (position.x < 0 || position.x >= mapSize.x || position.y < 0 || position.y >= mapSize.y) ? null : grid[position.x, position.y];
    }

    public Tile GetElevatedTile(Tile currentTile, Vector2Int targetPosition){

        Tile tile = GetTile(targetPosition);
        if (tile.elevatedTile != null && Mathf.Abs(tile.elevatedTile.height - currentTile.height) < Mathf.Abs(tile.height - currentTile.height))
            tile = tile.elevatedTile;
        return tile;
    }

    public List<Tile> GetTiles(ActionType actionType){

        switch (actionType){
            case ActionType.None:
                return inAreaTiles;
            case ActionType.MoveSelect:
                return inMoveTiles;
            case ActionType.TargetSelect:
                return inRangeTiles;
        }

        return null;
    }

    public int GetTileDistance(Tile startTile, Tile targetTile){

        return Mathf.Abs(startTile.gridPosition.x - targetTile.gridPosition.x) + Mathf.Abs(startTile.gridPosition.y - targetTile.gridPosition.y);
    }

    public int GetHeightDifference(int baseTileHeight, int targetTileHeight){

        return targetTileHeight - baseTileHeight;
    }

    public bool HeightInRange(int skillHeight, int baseTileHeight, int targetTileHeight){
        
        if (Mathf.Abs(GetHeightDifference(baseTileHeight, targetTileHeight)) > skillHeight)
            return false;
        return true;
    }
    
    public List<Tile> BuildPath(Tile startTile, Tile endTile){

        List<Tile> path = new List<Tile>();
        while (endTile != startTile){
            path.Add(endTile);
            endTile = endTile.parent;
        }
        path.Reverse();
        return path;
    }

    public void GenerateMoveTilesInRange(BattleCharacter currentCharacter){

        int stepCount = 0;
        List<Tile> inRangeTiles = new List<Tile>();
        List<Tile> tilesToCheck = new List<Tile>();
        List<Tile> nextTilesToCheck = new List<Tile>();
        inRangeTiles.Add(currentCharacter.currentTile);
        tilesToCheck.Add(currentCharacter.currentTile);
        while (stepCount < currentCharacter.Move){
            foreach (Tile tile in tilesToCheck){
                List<Tile> neighbourTiles = GetNeighbourTilesToMove(tile, currentCharacter);
                foreach (Tile neighbour in neighbourTiles){
                    if (!nextTilesToCheck.Contains(neighbour) && !inRangeTiles.Contains(neighbour)){
                        nextTilesToCheck.Add(neighbour);
                        inRangeTiles.Add(neighbour);
                        inRangeTiles[inRangeTiles.Count - 1].parent = tile; //REMOVE THIS IF PATHFIND FUNCTION IS TO BE USED
                    }
                }
            }
            tilesToCheck.Clear();
            tilesToCheck.AddRange(nextTilesToCheck);
            nextTilesToCheck.Clear();
            stepCount++;
        }
        
        //removes all tiles already containing the character
        for (int i = inRangeTiles.Count - 1; i >= 0; i--){
            if (inRangeTiles[i].character != null)
                inRangeTiles.RemoveAt(i);
        }

        inMoveTiles = inRangeTiles;
    }

    List<Tile> GetNeighbourTilesToMove(Tile currentTile, BattleCharacter currentCharacter){

        List<Tile> neighbours = new List<Tile>();
        List<Vector2Int> locationToCheck = new List<Vector2Int> {
            new Vector2Int(currentTile.gridPosition.x + 1, currentTile.gridPosition.y),
            new Vector2Int(currentTile.gridPosition.x - 1, currentTile.gridPosition.y),
            new Vector2Int(currentTile.gridPosition.x, currentTile.gridPosition.y + 1),
            new Vector2Int(currentTile.gridPosition.x, currentTile.gridPosition.y - 1)
        };

        for (int i = 0; i < 4; i++){
            Tile neighbour = GetTile(locationToCheck[i]);
            if (neighbour == null)
                continue;
            while (neighbour != null){

                bool canGoUp = (currentTile.elevatedTile != null && neighbour.height - currentTile.height > 5) ? false : true;
                bool canGoDown = true;
                if (neighbour.elevatedTile != null){
                    int baseTileHeightDistance = Mathf.Abs(currentTile.height - neighbour.height);
                    int elevatedTileHeightDistance = Mathf.Abs(currentTile.height - neighbour.elevatedTile.height);
                    if (baseTileHeightDistance > elevatedTileHeightDistance)
                        canGoDown = false;
                }

                if (!neighbour.blocked
                && canGoUp
                && canGoDown
                && neighbour.height <= currentTile.height + currentCharacter.Jump
                && neighbour.height >= currentTile.height - currentCharacter.Jump - 2
                && (neighbour.character == null || (neighbour.character != null && neighbour.character.tag == currentCharacter.tag)))
                    neighbours.Add(neighbour);

                neighbour = (neighbour.elevatedTile == null) ? null : neighbour.elevatedTile;
            }
        }
        return neighbours;
    }

    public void GenerateRange(Tile startTile, Skill skill){

        if (skill.Type == SkillType.Support){
            inAreaTiles = GenerateTiles(startTile, skill.Height, skill.Range, true);
            inAreaTiles.Remove(startTile);
        }
        else{
            inRangeTiles = GenerateTiles(startTile, skill.Height, skill.Range, true);
            if ((skill.Physical && skill.Damage >= 0) || !skill.Physical && skill.Damage > 0)
                inRangeTiles.Remove(startTile);
        }
    }

    public void GenerateArea(Tile startTile, Skill skill){

        inAreaTiles = GenerateTiles(startTile, skill.Height, skill.Area, false);        
    }

    List<Tile> GenerateTiles (Tile startTile, int skillHeight, int range, bool inRange){

        //DRAWS A DIAMOND SHAPE
        List<Tile> inRangeTiles = new List<Tile>();
        int count = range;
        for (int y = -range; y <= range; y++){
            for (int x = -(range - count); x <= (range - count); x++){
                Tile tile = GetTile(new Vector2Int(startTile.gridPosition.x + x, startTile.gridPosition.y + y));
                if (tile != null){

                    //APPLIES CRITERIAS FOR TARGET AVAILABILITY
                    if (inRange){
                        if (HeightInRange(skillHeight, startTile.height, tile.height))
                            inRangeTiles.Add(tile);
                        if (tile.elevatedTile != null && HeightInRange(skillHeight, startTile.height, tile.elevatedTile.height))
                            inRangeTiles.Add(tile.elevatedTile);
                    }

                    //APPLIES CRITERIAS FOR AREA AVAILABILITY
                    else{
                        if (tile.elevatedTile != null){
                            int baseTileHeightDistance = Mathf.Abs(startTile.height - tile.height);
                            int elevatedTileHeightDistance = Mathf.Abs(startTile.height - tile.elevatedTile.height);
                            inRangeTiles.Add((baseTileHeightDistance <= elevatedTileHeightDistance) ? tile : tile.elevatedTile);
                        }
                        else if(HeightInRange(skillHeight, startTile.height, tile.height))
                            inRangeTiles.Add(tile);
                    }
                }
            }
            count += (y >= 0) ? 1 : -1;
        }
        return inRangeTiles;
    }
    
    /*List<Tile> GetTilesInLineTarget(Tile startTile, int skillHeight, int range){        

        List<Tile> inRangeTiles = new List<Tile>();
        List<Vector2Int> locationsToCheck = new List<Vector2Int>();
        for (int y = -range; y <= range; y++)
            locationsToCheck.Add(new Vector2Int(startTile.gridPosition.x, startTile.gridPosition.y + y));
        for (int x = -range; x <= range; x++)
            locationsToCheck.Add(new Vector2Int(startTile.gridPosition.x + x, startTile.gridPosition.y));
        foreach (Vector2Int location in locationsToCheck){
            Tile tile = GetTileInRange(location, skillHeight, startTile.height);
            if (tile != null)
                inRangeTiles.Add(tile);
        }
        
        return inRangeTiles;
    }*/

    public void ToggleTilesVisibility(ActionType actionType, bool isMove){

        List<Tile> selectedTiles = (isMove) ? inMoveTiles : inRangeTiles;
                
        foreach (Tile tile in selectedTiles) {
            switch (actionType){
                case ActionType.None:
                    tile.mesh.material.color = Color.clear;
                    break;
                case ActionType.MoveSelect:
                    tile.mesh.material.color = Color.blue;
                    break;
                case ActionType.TargetSelect:
                    tile.mesh.material.color = Color.red;
                    break;
            }
        }

        if (actionType == ActionType.None)
            selectedTiles.Clear();
    }

    public void ToggleAreaVisibility(bool on){
        
        foreach (Tile tile in inAreaTiles){
            if (!on)
                tile.mesh.material.color = CheckTileInRange(tile, ActionType.TargetSelect) ? Color.red : Color.clear;
            else
                tile.mesh.material.color = Color.yellow;
        }
    }

    /*List<Tile> Pathfind(Tile startTile, Tile endTile){

        List<Tile> openList = new List<Tile>();
        List<Tile> closedList = new List<Tile>();
        openList.Add(startTile);

        while (openList.Count > 0){
            
            //LOWEST FCOST TILE IN THE OPEN LIST IS ANALYSED THEN SENT TO CLOSED LIST
            openList.Sort((x, y) => x.fCost.CompareTo(y.fCost));
            Tile currentTile = openList[0];
            openList.Remove(currentTile);
            closedList.Add(currentTile);

            //FINISHES PATH
            if (currentTile == endTile)
                return DrawPath(currentTile, endTile);

            //GET NEIGHBOUR TILES
            List<Tile> neighbours = GetNeighbourTiles(currentTile);

            //SETS NEIGHBOUR F AND G COSTS
            foreach (Tile neighbour in neighbours){
                if (closedList.Contains(neighbour))
                    continue;
                neighbour.gCost = GetTileDistance(startTile, neighbour);
                neighbour.hCost = GetTileDistance(endTile, neighbour);
                neighbour.parent = currentTile;
                if (!openList.Contains(neighbour))
                    openList.Add(neighbour);
            }
        }

        return new List<Tile>();
    }*/
}