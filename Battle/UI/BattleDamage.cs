using TMPro;
using UnityEngine;

public class BattleDamage : MonoBehaviour{

    public TextMeshProUGUI battleDamage;
    
    void Update(){

        gameObject.transform.Translate(Vector3.up * Time.deltaTime * 1.6f);
    }

    public void ProcessDamage(string text, Vector3 position){

        transform.position = position;
        if (text.Substring(0, 1) == "-"){
            text = text.Remove(0, 1);
            battleDamage.color = Color.green;
        }

        battleDamage.text = text;
        Destroy(gameObject, 0.6f);
    }
}