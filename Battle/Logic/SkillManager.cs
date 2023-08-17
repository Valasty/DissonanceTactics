using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour{

    public BattleManager battle;

    Skill actionSkill;
    BattleCharacter actionCharacter;
    BattleCharacter actionTarget;

    List<IEnumerator> activateBeforeAttack = new List<IEnumerator>();
    List<IEnumerator> activateAfterAttack = new List<IEnumerator>();
    
    public IEnumerator UseMainSkill(Skill skill, BattleCharacter user, BattleCharacter target){

        actionSkill = skill;
        actionCharacter = user;
        actionTarget = target;

        int bonus;
        Tile targetTile;

        switch (skill.Name){

            /************************************** CAESAR **************************************/
            case "Tackle":
                targetTile = GetTileBehind(actionCharacter, actionTarget);
                float damage = battle.GetDamage(actionSkill, actionCharacter, actionTarget);

                if (targetTile == null)
                    battle.ApplyDamage((int)damage, actionTarget);

                else {
                    int heightDifference = battle.map.GetHeightDifference(actionTarget.currentTile.height, targetTile.height);

                    //deals 20% damage to the character in the target tile if there is one
                    if (targetTile.character != null && Mathf.Abs(heightDifference) < 3) {
                        battle.ApplyDamage((int)damage, actionTarget);
                        yield return StartCoroutine(PushCharacterToTile(actionTarget, targetTile, "Rebound"));
                        if (targetTile.character.HP > 0)
                            battle.ApplyDamage((int)((damage * 0.2f) + 0.5f), targetTile.character); //0.5 is to round up for at least 1 damage
                    }

                    //deal 20% bonus damage to target if there's a step/wall behind it (wall = more than 2 heights above)
                    else if (targetTile.blocked || heightDifference > 2) {
                        battle.ApplyDamage((int)(damage * 1.2f), actionTarget);
                        yield return StartCoroutine(PushCharacterToTile(actionTarget, targetTile, "Rebound"));
                    }

                    //deals 10% fall damage per height difference to the target
                    else if (heightDifference < -2) {
                        actionTarget.chargingSkill = null;
                        battle.ApplyDamage((int)damage, actionTarget);
                        yield return StartCoroutine(PushCharacterToTile(actionTarget, targetTile, "Fall")); //MAKE IT FALL ANIMATION!!!!!
                        battle.ApplyDamage((int)(damage * (Mathf.Abs(heightDifference) * 0.05f)), actionTarget);
                    }

                    //no blockers or height differences
                    else{
                        actionTarget.chargingSkill = null;
                        battle.ApplyDamage((int)damage, actionTarget);
                        yield return StartCoroutine(PushCharacterToTile(actionTarget, targetTile, null));
                    }
                }
                break;

            case "Earth Hammer":
                actionTarget.CP -= GetBonus(20, true);
                break;
            
            case "Sand Curtain":
                actionTarget.ApplyCondition(new Condition(actionSkill.Name, new Attributes(Accuracy: GetBonus(-40, true)), false, 3, battle.database.DamageTypes[0]));
                break;

            case "Earth Shield":
                bonus = GetBonus(actionCharacter.MagAttack * 2);
                actionTarget.ApplyCondition(new Condition(actionSkill.Name, new Attributes(PhyDefense: bonus, MagDefense: bonus), true, 1, battle.database.DamageTypes[0]));
                break;

            case "Nature's Blessing":
                actionTarget.RemoveConditions(ConditionRemovalType.Negatives);
                break;

            case "Poison Scratch":
                actionTarget.ApplyCondition(new Condition(actionSkill.Name, new Attributes(DamageOverTime: GetBonus(actionCharacter.PhyAttack * 0.3f, true)), false, 3, battle.database.DamageTypes[0]));
                goto default;

            case "Cover":
                yield return new WaitForSeconds(0.25f); //necessary to not bug the animation!!!
                Tile targetNewTile = actionCharacter.currentTile;
                yield return StartCoroutine(PushCharacterToTile(actionCharacter, actionTarget.currentTile, null));
                yield return StartCoroutine(PushCharacterToTile(actionTarget, targetNewTile, null));
                actionCharacter.currentTile.character = actionCharacter; //this avoids a problem during character position swap                
                battle.TurnToTarget(actionCharacter, actionCharacter.currentTile.gridPosition, battle.currentCharacter.currentTile.gridPosition);
                break;

            /************************************** ARYA **************************************/
            case "Flash Cut":
                targetTile = GetTileBehind(actionCharacter, actionTarget);
                string checkRebound = null;
                if (targetTile != null){
                    if (targetTile.blocked || targetTile.character != null || !battle.map.HeightInRange(skill.Height, actionCharacter.currentTile.height, targetTile.height))
                        checkRebound = "Rebound";
                    StartCoroutine(PushCharacterToTile(actionCharacter, targetTile, checkRebound));
                }
                goto default;
                
            case "Shocking Touch":
                actionTarget.CP -= GetBonus(20, true);
                battle.ReorderQueue(actionTarget, null);
                goto default;

            case "Enlighten":
                bonus = GetBonus(actionCharacter.MagAttack);
                actionTarget.ApplyCondition(new Condition(actionSkill.Name, new Attributes(MagAttack: bonus, MagDefense: bonus), true, 3, battle.database.DamageTypes[0]));
                break;

            case "Provoke":
                battle.TurnToTarget(actionTarget, actionTarget.currentTile.gridPosition, actionCharacter.currentTile.gridPosition);
                actionTarget.ApplyCondition(new Condition(actionSkill.Name, new Attributes(Evade: -30), false, 2, battle.database.DamageTypes[0]));
                break;

            case "Flash":
                actionTarget.ApplyCondition(new Condition(actionSkill.Name, new Attributes(Accuracy: -40), false, 1, battle.database.DamageTypes[0]));
                break;

            /************************************** DARIUS **************************************/
            case "Quick Shot":
                actionCharacter.CP += 25;
                break;

            case "Wind Jump":
                break;

            case "Wind Push":
                break;

            case "Hermes Boots":
                actionTarget.ApplyCondition(new Condition(actionSkill.Name, new Attributes(Move: 2, Jump: 4), true, 3, battle.database.DamageTypes[0]));
                break;

            /************************************** DEFAULT **************************************/
            default:
                battle.ApplyDamage(battle.GetDamage(skill, actionCharacter, actionTarget), actionTarget);
                break;
        }
    }

    public IEnumerator ActivateNonActiveSkill(bool beforeAttack){

        List<IEnumerator> skillsToActivate = beforeAttack ? activateBeforeAttack : activateAfterAttack;
        foreach (IEnumerator action in skillsToActivate){
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(action);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void SetSupportSkills(Tile targetTile){

        //this method uses "current" variables from battle because its only called on NOT on COUNTER

        //clears actions before executing
        activateBeforeAttack.Clear();
        activateAfterAttack.Clear();
        
        //cancels action if not valid
        if (battle.currentSkill.Area > 0 || battle.currentSkill.Damage <= 0 || targetTile.character == null || targetTile.character.HP == 0)
            return;

        foreach (Skill skill in targetTile.aurasList){
            if (battle.currentCharacter == skill.Character || skill.Character.chargingSkill != null)
                continue;

            //checks if skill meets the criterias
            switch (skill.Name){

                case "Flash":
                    if (targetTile.character.tag == skill.Character.tag)
                        activateBeforeAttack.Add(battle.ExecuteAction(skill, skill.Character, new List<Tile> { battle.currentCharacter.currentTile }, true));
                    break;

                case "Cover":
                    if (targetTile.character.tag == skill.Character.tag && skill.Character.HP > targetTile.character.HP)
                        activateBeforeAttack.Add(battle.ExecuteAction(skill, skill.Character, new List<Tile> { targetTile }, true));
                    break;

                case "Fire Support":
                    if (targetTile.character.tag != skill.Character.tag)
                        activateAfterAttack.Add(battle.ExecuteAction(skill, skill.Character, new List<Tile> { targetTile }, true));
                    break;
            }
        }
    }

    public void SetReactionSkills(BattleCharacter target){

        if (battle.currentCharacter == target
        || target.chargingSkill != null
        || target.ReactionSkill.Range < battle.map.GetTileDistance(target.currentTile, battle.currentCharacter.currentTile)
        || !battle.map.HeightInRange(target.ReactionSkill.Height, target.currentTile.height, battle.currentCharacter.currentTile.height))
            return;

        //checks if skill meets the criterias
        switch (target.ReactionSkill.Name){

            case "Enrage":

                break;

            case "Counter":
                activateAfterAttack.Add(battle.ExecuteAction(target.ReactionSkill, target, new List<Tile> { battle.currentCharacter.currentTile }, true));
                break;
        }        
    }

    int GetBonus(float modifier, bool debuff = false){ //used to facilitate calculations related to elemental modifiers

        return (int)(modifier * (1 + battle.GetElementalModifier(actionSkill, actionCharacter, actionTarget, debuff)));
    }

    IEnumerator PushCharacterToTile(BattleCharacter targetCharacter, Tile targetTile, string pushType){

        List<Vector3> path = new List<Vector3>();

        if (pushType == "Rebound"){ //adds step to bounce back to
            path.Add(new Vector3(
                (targetCharacter.currentTile.transform.position.x + targetTile.transform.position.x) / 2,
                targetCharacter.currentTile.transform.position.y,
                (targetCharacter.currentTile.transform.position.z + targetTile.transform.position.z) / 2));
            path.Add(targetCharacter.currentTile.transform.position);
        }
        else{
            if (pushType == "Fall"){ //adds step to angle the fall
                path.Add(new Vector3(
                    targetTile.transform.position.x,
                    targetCharacter.currentTile.transform.position.y,
                    targetTile.transform.position.z));
            }
            path.Add(targetTile.transform.position);
            battle.SetCharacterOnTile(targetTile, targetCharacter);
        }

        while (path.Count > 0){
            yield return StartCoroutine(battle.MoveAnimation(targetCharacter, path[0], 4));
            if (path.Count > 1 && path[1].y + (0.2f * 2) < path[0].y) //0.2f because it's the current height unit size
                yield return new WaitForSeconds(0.1f); ////////////////////// NEED TO UPDATE IT TO HAVE NO DOWNTIME BUT ADDING ACCELERATION
            path.RemoveAt(0);
        }
    }    

    Tile GetTileBehind(BattleCharacter user, BattleCharacter target){

        Vector2Int targetTilePosition = target.currentTile.gridPosition;
        switch (user.direction){
            case 0: //up
                targetTilePosition += Vector2Int.right;
                break;
            case 1: //down
                targetTilePosition += Vector2Int.left;
                break;
            case 2: //left
                targetTilePosition += Vector2Int.up;
                break;
            case 3: //right
                targetTilePosition += Vector2Int.down;
                break;
        }
        return battle.map.GetElevatedTile(target.currentTile, targetTilePosition);        
    }
}