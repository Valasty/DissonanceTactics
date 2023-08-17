using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterDetails : MonoBehaviour{

    public BattleManager battle;
    public Image panelImage;
    public Image charPortrait;
    public Image charElement;
    public RectTransform HPBar;
    public TextMeshProUGUI[] charAttributeTexts;
    public TextMeshProUGUI[] battleAttributeTexts;
    public GameObject battlePreview;
    public GameObject conditionPrefab;
    public Transform conditionCanvas;

    BattleCharacter currentCharacter;

    public void LoadCharacterData(BattleCharacter character) {

        if (character == null) {
            gameObject.SetActive(false);
            return;
        }

        currentCharacter = character;

        panelImage.color = (currentCharacter.tag == "Ally") ? new Color(0, 0, 0.5f, 0.85f) : new Color(0.5f, 0, 0, 0.85f);
        charPortrait.sprite = currentCharacter.Portrait;                
        charElement.sprite = battle.database.Elements[(int)currentCharacter.Element];

        charAttributeTexts[0].text = currentCharacter.name;
        charAttributeTexts[1].text = currentCharacter.Level.ToString();
        charAttributeTexts[2].text = currentCharacter.PhyAttack.ToString();
        charAttributeTexts[3].text = currentCharacter.MagAttack.ToString();
        charAttributeTexts[4].text = currentCharacter.PhyDefense.ToString();
        charAttributeTexts[5].text = currentCharacter.MagDefense.ToString();
        charAttributeTexts[6].text = currentCharacter.Move.ToString();
        charAttributeTexts[7].text = currentCharacter.Jump.ToString();
        charAttributeTexts[8].text = currentCharacter.Speed.ToString();
        charAttributeTexts[9].text = currentCharacter.Evade.ToString();
        charAttributeTexts[10].text = currentCharacter.HP + " / " + currentCharacter.HPMax;
        charAttributeTexts[11].text = currentCharacter.CP.ToString();

        HPBar.localScale = new Vector2((float)currentCharacter.HP / (float)currentCharacter.HPMax, 1);

        //CONDITIONS
        foreach (Transform child in conditionCanvas)
            Destroy(child.gameObject);
        foreach (Condition condition in currentCharacter.conditionsList){
            GameObject conditionObject = Instantiate(conditionPrefab, conditionCanvas);
            conditionObject.GetComponentInChildren<Image>().sprite = condition.Portrait;
            conditionObject.GetComponentInChildren<TextMeshProUGUI>().text = condition.Duration.ToString();
        }

        UpdateBattleDetails();
        gameObject.SetActive(true);
    }

    public void UpdateBattleDetails(){

        if (battle.actionType == ActionType.TargetSelect && battle.map.CheckTileInRange(currentCharacter.currentTile, ActionType.TargetSelect)){
            battleAttributeTexts[0].text = battle.GetSuccessRate(battle.currentSkill, battle.currentCharacter, currentCharacter) + "%";
            battleAttributeTexts[1].text = (battle.currentSkill.Damage != 0) ? battle.GetDamage(battle.currentSkill, battle.currentCharacter, currentCharacter).ToString() : "N/A";
            battlePreview.SetActive(true);
        }
        else
            battlePreview.SetActive(false);
    }
}