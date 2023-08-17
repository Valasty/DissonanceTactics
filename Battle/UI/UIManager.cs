using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour{

    public TextMeshProUGUI turnText;
    public TextMeshProUGUI heightText;
    public TextMeshProUGUI gridText;
    public TextMeshProUGUI[] elementDetails;

    public UICharacterDetails charDetailsMain;
    public UICharacterDetails charDetailsTarget;

    void Awake(){

        charDetailsTarget = Instantiate(charDetailsMain, gameObject.transform);
        RectTransform rectTransform = charDetailsTarget.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 0);
        rectTransform.anchorMax = new Vector2(1, 0);
        rectTransform.anchoredPosition = new Vector2(-225, 75);
    }

    public void LoadTileDetails(Tile tile){

        //load character data
        charDetailsTarget.LoadCharacterData(tile.character);

        //load grid data
        gridText.text = "X: " + tile.gridPosition.x + " | Y: " + tile.gridPosition.y;
        heightText.text = tile.height.ToString();

        //load element data
        for (int i = 0; i < tile.elements.Length; i++)
            elementDetails[i].text = tile.elements[i].ToString();        
    }

    public void SetTurn(int turn){

        turnText.text = turn.ToString();
    }
}