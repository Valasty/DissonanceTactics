using UnityEngine;
using UnityEngine.EventSystems;

public class WaitMenu : MonoBehaviour{

    Transform currentSelected;

    public BattleManager battle;

    int upObjectIndex;
    int downObjectIndex;
    int leftObjectIndex;
    int rightObjectIndex;

    int currentDirection;

    void Awake(){
        
        ChangeButtonNavigation();
        currentSelected = transform.GetChild(upObjectIndex);
    }

    void OnEnable(){

        currentDirection = battle.currentCharacter.direction;
        currentSelected.GetComponent<MeshRenderer>().material.color = Color.gray;
        currentSelected = transform.GetChild(battle.currentCharacter.direction);
        currentSelected.GetComponent<MeshRenderer>().material.color = Color.yellow;
    }

    void Update(){

        if (Input.GetButtonDown("Horizontal"))
            ButtonSelect("Horizontal");

        if (Input.GetButtonDown("Vertical"))
            ButtonSelect("Vertical");

        if (Input.GetButtonDown("Submit")){
            SubmitCancelClick();
            battle.EndTurn();
        }
        
        if (Input.GetButtonDown("Cancel")){
            SubmitCancelClick();
            battle.currentCharacter.direction = currentDirection;
            EventSystem.current.SetSelectedGameObject(battle.menu.characterMenu.transform.GetChild(2).gameObject);
        }
    }

    void ButtonSelect(string axis){

        currentSelected.GetComponent<MeshRenderer>().material.color = Color.gray;
        if (axis == "Vertical")
            currentSelected = Input.GetAxisRaw(axis) == 1 ? transform.GetChild(upObjectIndex) : transform.GetChild(downObjectIndex);
        else
            currentSelected = Input.GetAxisRaw(axis) == 1 ? transform.GetChild(rightObjectIndex) : transform.GetChild(leftObjectIndex);
        currentSelected.GetComponent<MeshRenderer>().material.color = Color.yellow;
        battle.currentCharacter.direction = currentSelected.GetSiblingIndex();
    }

    void SubmitCancelClick(){

        battle.actionType = ActionType.CharacterMenu;
        battle.menu.gameObject.SetActive(true);
        battle.cursor.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ChangeButtonNavigation(){

        switch (Camera.main.transform.rotation.eulerAngles.y){
            case 45:
                upObjectIndex = 0;
                downObjectIndex = 1;
                leftObjectIndex = 2;
                rightObjectIndex = 3;
                break;
            case 135:
                upObjectIndex = 3;
                downObjectIndex = 2;
                leftObjectIndex = 0;
                rightObjectIndex = 1;
                break;
            case 225:
                upObjectIndex = 1;
                downObjectIndex = 0;
                leftObjectIndex = 3;
                rightObjectIndex = 2;
                break;
            case 315:
                upObjectIndex = 2;
                downObjectIndex = 3;
                leftObjectIndex = 1;
                rightObjectIndex = 0;
                break;
        }
    }
}