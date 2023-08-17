using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MenuManager : MonoBehaviour{

    public UIManager ui;
    public BattleManager battle;
    public MapManager map;
    public Database database;

    public GameObject mainMenu;
    public GameObject characterMenu;
    public GameObject actMenu;
    public Transform skillMenu;
    public GameObject waitMenu;
    public GameObject confirmationMenu;

    public Button actionMoveButton;
    public Button actionActButton;
    public GameObject skillButtonPrefab;

    public GameObject skillAttributesObject;
    public TextMeshProUGUI[] descriptionTexts;
    public Image[] descriptionImages;
    
    public GameObject lastSelectedGameObject;

    void Start(){

        ///////// INSTANTIATE SKILLS BUTTONS /////////
        for (int i = 1; i < 16; i++)
            Instantiate(skillButtonPrefab, skillMenu);
    }

    void OnEnable(){

        if (battle.actionType == ActionType.ConfirmationMenu){
            mainMenu.SetActive(false);
            confirmationMenu.SetActive(true);
            EventSystem.current.SetSelectedGameObject(confirmationMenu.transform.GetChild(0).gameObject);
        }
        else {
            mainMenu.SetActive(true);
            if (battle.actionType == ActionType.CharacterMenu) //quando começa o turno ou dando back no MOVE
                EventSystem.current.SetSelectedGameObject(battle.currentCharacter.hasMoved ? actionActButton.gameObject : actionMoveButton.gameObject);
            else if ((int)battle.actionType > 4)
                EventSystem.current.SetSelectedGameObject(EventSystem.current.currentSelectedGameObject);
        }
    }

    void Update(){

        if (Input.GetButtonDown("Cancel")) {
            switch (battle.actionType){
                case ActionType.CharacterMenu:
                    battle.currentSkill = null;
                    battle.actionType = ActionType.None;
                    battle.cursor.enabled = true;
                    gameObject.SetActive(false);
                    break;
                case ActionType.ActMenu:
                    battle.actionType = ActionType.CharacterMenu;
                    actMenu.SetActive(false);
                    characterMenu.SetActive(true);
                    EventSystem.current.SetSelectedGameObject(actionActButton.gameObject);
                    break;
                case ActionType.SkillMenu:
                    battle.actionType = ActionType.ActMenu;
                    skillMenu.gameObject.SetActive(false);
                    actMenu.SetActive(true);
                    EventSystem.current.SetSelectedGameObject(actMenu.transform.GetChild(1).gameObject);
                    break;
                /*case ActionType.ItemMenu:
                    break;*/
                case ActionType.ConfirmationMenu:
                    ConfirmButtonClick(false);
                    break;
            }
        }
    }

    public void ConfirmButtonClick(bool yes){

        confirmationMenu.SetActive(false);
        gameObject.SetActive(false);
        if (yes){
            if (battle.currentSkill == null)
                battle.MoveCharacterStart();
            else
                battle.ExecuteActionStart();
        }
        else{
            EventSystem.current.SetSelectedGameObject(lastSelectedGameObject);
            battle.actionType = (battle.currentSkill == null) ? ActionType.MoveSelect : ActionType.TargetSelect;
            battle.cursor.enabled = true;
        }
    }

    public void MoveButtonClick(){

        battle.actionType = ActionType.MoveSelect;
        battle.currentSkill = null;
        map.GenerateMoveTilesInRange(battle.currentCharacter);
        map.ToggleTilesVisibility(battle.actionType, true);
        battle.cursor.enabled = true;
        gameObject.SetActive(false);
    }

    public void ActButtonClick(){

        battle.actionType = ActionType.ActMenu;
        characterMenu.SetActive(false);
        actMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(actMenu.transform.GetChild(0).gameObject);
    }

    public void WaitButtonClick(){

        battle.actionType = ActionType.WaitMenu;
        waitMenu.transform.position = battle.currentCharacter.transform.position;
        waitMenu.SetActive(true);
        gameObject.SetActive(false);
        battle.cursor.gameObject.SetActive(false);
    }

    public void SkillMenuButtonClick(bool skill){

        if (!skill && database.Player.Inventory[(int)ItemType.Usable].Count == 0)
            return;

        battle.actionType = ActionType.SkillMenu;
        actMenu.SetActive(false);
        skillMenu.gameObject.SetActive(true);

        if (skill){
            for (int i = 0; i < skillMenu.transform.childCount; i++){
                if (i < battle.currentCharacter.Skills.Count){
                    skillMenu.GetChild(i).gameObject.SetActive(true);
                    SkillButton skillButton = skillMenu.GetChild(i).GetComponent<SkillButton>();
                    skillButton.currentSkill = battle.currentCharacter.Skills[i];
                    skillMenu.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = skillButton.currentSkill.LocalizedName;
                }
                else
                    skillMenu.GetChild(i).gameObject.SetActive(false);
            }
        }

        else{
            for (int i = 0; i < skillMenu.transform.childCount; i++){
                if (i < database.Player.Inventory[(int)ItemType.Usable].Count){
                    skillMenu.GetChild(i).gameObject.SetActive(true);
                    SkillButton skillButton = skillMenu.GetChild(i).GetComponent<SkillButton>();
                    skillButton.currentSkill = ((Usable)database.Player.Inventory[(int)ItemType.Usable][i].Item).Skill;
                    skillMenu.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = skillButton.currentSkill.LocalizedName + " x" + database.Player.Inventory[(int)ItemType.Usable][i].Quantity;
                }
                else
                    skillMenu.GetChild(i).gameObject.SetActive(false);
            }
        }

        EventSystem.current.SetSelectedGameObject(skillMenu.GetChild(0).gameObject);
    }

    public void ActionButtonClick(){

        battle.actionType = ActionType.TargetSelect;
        lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
        map.GenerateRange(battle.currentCharacter.currentTile, battle.currentSkill);
        map.ToggleTilesVisibility(ActionType.TargetSelect, false);        
        map.GenerateArea(battle.cursor.currentTile, battle.currentSkill);
        map.ToggleAreaVisibility(true);
        battle.cursor.enabled = true;
        ui.charDetailsTarget.UpdateBattleDetails();
        gameObject.SetActive(false);
    }

    public void SkillButtonSelect(bool skill){

        if (skill)
            battle.currentSkill = EventSystem.current.currentSelectedGameObject.GetComponent<SkillButton>().currentSkill;
        else
            battle.currentSkill = battle.currentCharacter.AttackSkill;

        skillAttributesObject.SetActive(true);
        descriptionTexts[0].text = battle.currentSkill.Description;
        switch (battle.currentSkill.Charge){
            case 0:
                descriptionTexts[1].text = "N/A";
                break;
            case 1:
                descriptionTexts[1].text = "Fast (" + battle.GetChargeTime() + ")";
                break;
            case 2:
                descriptionTexts[1].text = "Med (" + battle.GetChargeTime() + ")";
                break;
            case 3:
                descriptionTexts[1].text = "Slow (" + battle.GetChargeTime() + ")";
                break;
        }
        descriptionTexts[2].text = battle.currentSkill.Range.ToString();
        descriptionTexts[3].text = battle.currentSkill.Height.ToString();
        descriptionTexts[4].text = (battle.currentSkill.Area == 0) ? "N/A" : battle.currentSkill.Area.ToString();

        descriptionImages[0].sprite = battle.currentSkill.Physical ? database.DamageTypes[0] : database.DamageTypes[1];
        descriptionImages[1].sprite = database.Elements[(int)battle.currentSkill.Element];
    }

    public void ButtonUnselect(){

        skillAttributesObject.SetActive(false);
        descriptionTexts[0].text = null;
        descriptionTexts[1].text = null;
        descriptionTexts[2].text = null;
        descriptionTexts[3].text = null;
    }
}