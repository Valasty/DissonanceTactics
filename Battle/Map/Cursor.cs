//TA DANDO PRA CLICAR EM CONFIRM ANTES DA CAMERA TERMINAR DE CHEGAR NO ALVO!!!

using UnityEngine;
using System.Collections;

public class Cursor : MonoBehaviour{

    public BattleManager battle;
    public WaitMenu waitMenu;
    public Tile currentTile;
    Vector3 targetCameraPosition;
    bool rotatingCamera = false;

    void Update(){

        if (Input.GetButtonDown("Submit")){
            if (battle.actionType == ActionType.None && currentTile.character == battle.currentCharacter){   //when you select the turn character during free move
                battle.actionType = ActionType.CharacterMenu;
                enabled = false;
                battle.menu.gameObject.SetActive(true);
            }
            else if (battle.map.CheckTileInRange(currentTile, battle.actionType)){  //when you try to submit a movement or target... requires action type filter or clean inrangetiles
                battle.actionType = ActionType.ConfirmationMenu;
                enabled = false;
                battle.menu.gameObject.SetActive(true);
            }
        }

        if (Input.GetButtonDown("Cancel")) {
            if (battle.actionType != ActionType.None){
                if (battle.actionType == ActionType.TargetSelect){
                    battle.map.ToggleAreaVisibility(false);
                    battle.map.ToggleTilesVisibility(ActionType.None, false);
                }
                else
                    battle.map.ToggleTilesVisibility(ActionType.None, true);
            }
            StartCoroutine(ResetPlayerTurn());
        }

        if (Input.GetButtonDown("Vertical")){
            int directionInput = (Camera.main.transform.rotation.eulerAngles.y == 225 || Camera.main.transform.rotation.eulerAngles.y == 135) ?
                -(int)Input.GetAxisRaw("Vertical") : (int)Input.GetAxisRaw("Vertical");
            Vector2Int inputVector;            

            if (Camera.main.transform.rotation.eulerAngles.y == 45 || Camera.main.transform.rotation.eulerAngles.y == 225){
                inputVector = new Vector2Int(currentTile.gridPosition.x + directionInput, currentTile.gridPosition.y);
                if (inputVector.x >= battle.map.mapSize.x || inputVector.x < 0)
                    return;
            }
            else{
                inputVector = new Vector2Int(currentTile.gridPosition.x, currentTile.gridPosition.y + directionInput);
                if (inputVector.y >= battle.map.mapSize.y || inputVector.y < 0)
                    return;
            }

            MoveTo(inputVector);
            DisplayArea();
        }

        if (Input.GetButtonDown("Horizontal")){
            int directionInput = (Camera.main.transform.rotation.eulerAngles.y == 225 || Camera.main.transform.rotation.eulerAngles.y == 315) ?
                (int)Input.GetAxisRaw("Horizontal") : -(int)Input.GetAxisRaw("Horizontal");
            Vector2Int inputVector;            

            if (Camera.main.transform.rotation.eulerAngles.y == 45 || Camera.main.transform.rotation.eulerAngles.y == 225){
                inputVector = new Vector2Int(currentTile.gridPosition.x, currentTile.gridPosition.y + directionInput);
                if (inputVector.y >= battle.map.mapSize.y || inputVector.y < 0)
                    return;
            }
            else{
                inputVector = new Vector2Int(currentTile.gridPosition.x + directionInput, currentTile.gridPosition.y);
                if (inputVector.x >= battle.map.mapSize.x || inputVector.x < 0)
                    return;
            }

            MoveTo(inputVector);
            DisplayArea();
        }

        if (Input.GetButtonDown("R3")){
            if (currentTile.elevatedTile != null){
                currentTile = currentTile.elevatedTile;
                UpdateTilePosition();
                DisplayArea();
            }
            else if (currentTile.tag == "Untagged"){
                currentTile = battle.map.GetTile(currentTile.gridPosition);
                UpdateTilePosition();
                DisplayArea();
            }
        }

        if (rotatingCamera)
            return;

        if (Input.GetButtonDown("R1"))
            StartCoroutine(RotateCamera(1));

        if (Input.GetButtonDown("L1"))
            StartCoroutine(RotateCamera(-1));

        if (battle.battleCamera.position != targetCameraPosition)
            battle.battleCamera.position = Vector3.MoveTowards(battle.battleCamera.position, targetCameraPosition, Time.deltaTime * 8);
    }

    public void Return(){

        currentTile = battle.currentCharacter.currentTile;
        UpdateTilePosition();
        enabled = false;
    }

    public void MoveTo(Vector2Int targetPosition){

        Tile newTile = battle.map.GetElevatedTile(currentTile, targetPosition);
        currentTile = newTile;
        UpdateTilePosition();
    }

    void UpdateTilePosition(){

        transform.position = currentTile.transform.position;
        targetCameraPosition = transform.position;
        battle.ui.LoadTileDetails(currentTile);
    }

    void DisplayArea(){

        if (battle.currentSkill != null){
            battle.map.ToggleAreaVisibility(false);
            battle.map.GenerateArea(currentTile, battle.currentSkill);
            battle.map.ToggleAreaVisibility(true);
        }
    }

    IEnumerator ResetPlayerTurn(){

        yield return StartCoroutine(battle.ResetCamera());
        if (battle.currentSkill != null){
            if (battle.currentSkill.Name == "Attack")
                battle.actionType = ActionType.ActMenu;
            else
                battle.actionType = ActionType.SkillMenu;
        }
        else
            battle.actionType = ActionType.CharacterMenu;
        battle.menu.gameObject.SetActive(true);
    }

    public IEnumerator RotateCamera(int positive) {

        rotatingCamera = true;
        float startAngle = Camera.main.transform.rotation.eulerAngles.y;
        float count = 0;
        while (count <= 90) {
            float newY = startAngle + (count * positive);
            Camera.main.transform.rotation = Quaternion.Euler(30, newY, 0);
            for (int i = 0; i < battle.actionQueue.Count; i++)
                battle.actionQueue[i].sprite.transform.rotation = Quaternion.Euler(30, newY, 0);
            count++;
            yield return null;
        }

        for (int i = 0; i < battle.actionQueue.Count; i++)
            battle.actionQueue[i].animator.Play(AnimationState.Idle);

        battle.worldSpaceCanvas.rotation = Quaternion.Euler(30, Camera.main.transform.rotation.eulerAngles.y, 0);

        waitMenu.ChangeButtonNavigation();

        yield return new WaitForSeconds(0.1f);
        rotatingCamera = false;
    }
}