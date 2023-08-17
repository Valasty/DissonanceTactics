//TEM QUE ADICIONAR A FORMULA DE HEIGHT DIFFERENCE NO GETDAMAGE()
//TA DANDO DAMAGE NEGATIVO CONTRA TARGET EM HIGH GROUND

//MAKE CHARACTER TURN TO THE ATTACKER????
//ADD ACCELERATION TO JUMP/FALL ANIMATIONS?
//IMPLEMENTAR PROJECTILES!!!

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleManager : MonoBehaviour{

    public MapManager map;
    public UIManager ui;
    public MenuManager menu;
    public Database database;
    public SkillManager skill;

    public Transform battleCamera;
    //public CameraManager battleCamera;

    public Cursor cursor;
    public GameObject characterPrefab;
    public GameObject battleTextPrefab;
    public GameObject battleSpeechPrefab;
    public Transform worldSpaceCanvas;

    public BattleCharacter currentCharacter;
    public Skill currentSkill;
    public ActionType actionType;
    public List<BattleCharacter> actionQueue = new List<BattleCharacter>();
    bool chargeCasting = false;
    int turnCount;

    const float moveSpeed = 3.5f;
    const int jumpHeight = 4;

    public class EnemyPlay{

        public Vector2Int movePosition;
        public Vector2Int targetPosition;
        public int effectiveness;

        public EnemyPlay(Vector2Int movePosition, Vector2Int targetPosition, int effectiveness){

            this.movePosition = movePosition;
            this.targetPosition = targetPosition;
            this.effectiveness = effectiveness;
        }        
    }

    void Start(){

        map.GenerateGrid();
        SpawnCharacters();
        
        actionQueue.Sort((x, y) => x.timeToTurn.CompareTo(y.timeToTurn)); ////É A MESMA COISA QUE FAZ NO ENDTURN, PRO PRIMEIRO A AGIR TER 100 DE CP
        while (actionQueue[0].CP < 100){
            foreach (BattleCharacter character in actionQueue)
                character.CP += character.Speed;
        }

        currentCharacter = actionQueue[0];
        cursor.currentTile = currentCharacter.currentTile;
        StartCoroutine(StartTurn());
    }

    void SpawnCharacters(){

        InstantiateCharacter(database.Player.Characters[0], new Vector2Int(5, 2), "Ally");
        InstantiateCharacter(database.Player.Characters[1], new Vector2Int(5, 4), "Ally");
        InstantiateCharacter(database.Player.Characters[2], new Vector2Int(4, 2), "Ally");
        InstantiateCharacter(database.Enemies[0], new Vector2Int(3, 4), "Enemy");
        //InstantiateCharacter(database.Enemies[0], new Vector2Int(3, 3), "Enemy");
        //InstantiateCharacter(database.Enemies[0], new Vector2Int(4, 3), "Enemy");
    }

    void InstantiateCharacter(Character character, Vector2Int position, string tag){

        BattleCharacter battleCharacter = Instantiate(characterPrefab).GetComponent<BattleCharacter>();
        battleCharacter.InstantiateCharacter(character, tag);
        battleCharacter.currentTile = map.GetTile(position);  //sets char with tile
        battleCharacter.currentTile.character = battleCharacter; //sets tile with char
        UpdateAuras(battleCharacter, true);
        battleCharacter.transform.position = battleCharacter.currentTile.transform.position;
        actionQueue.Add(battleCharacter);
    }
        
    IEnumerator StartTurn(){

        turnCount++;
        ui.SetTurn(turnCount);
        currentSkill = null;
        currentCharacter = actionQueue[0];

        //SKILL CHARGE LOGIC
        if (currentCharacter.chargingSkill != null){
            chargeCasting = true;
            currentSkill = currentCharacter.chargingSkill;
            currentCharacter.chargingSkill = null; //removing this will bug the animation
            cursor.MoveTo(currentCharacter.chargingTargetPos); //ISSO VAI BUGAR PRA TILE ELEVADOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO
            map.GenerateArea(cursor.currentTile, currentSkill);
            StartCoroutine(ExecuteAction(currentSkill, currentCharacter, map.GetTiles(ActionType.None)));
        }

        //REGULAR LOGIC
        else{
            yield return StartCoroutine(ResetCamera());

            //PROCESSES CONDITIONS
            for (int i = currentCharacter.conditionsList.Count - 1; i >= 0; i--) {
                if (currentCharacter.conditionsList[i].Attributes.DamageOverTime != 0) {
                    ApplyDamage(currentCharacter.conditionsList[i].Attributes.DamageOverTime, currentCharacter, false);
                    yield return new WaitForSeconds(1);
                }                
                currentCharacter.conditionsList[i].Duration--;
                if (currentCharacter.conditionsList[i].Duration == 0)
                    currentCharacter.RemoveCondition(currentCharacter.conditionsList[i]);
            }

            //START PLAYS
            actionType = ActionType.CharacterMenu;
            if (currentCharacter.tag == "Enemy")
                StartCoroutine(StartEnemyPlay());
            else
                menu.gameObject.SetActive(true);            
        }
    }

    IEnumerator StartEnemyPlay(){

        /*currentSkill = currentCharacter.Skills[0];
        EnemyPlay enemyPlay = GenerateEnemyPlay();

        yield return new WaitForSeconds(0.5f);

        if (enemyPlay.movePosition != currentCharacter.currentTile.gridPosition){
            cursor.MoveTo(enemyPlay.movePosition); //ISSO VAI BUGAR PRA TILE ELEVADOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO
            yield return StartCoroutine(MoveCharacter(currentCharacter, cursor.currentTile));
            yield return new WaitForSeconds(0.5f);
        }

        if (enemyPlay.targetPosition != -Vector2Int.one){
            cursor.MoveTo(enemyPlay.targetPosition); //ISSO VAI BUGAR PRA TILE ELEVADOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO
            map.GenerateSkillTilesInRange(cursor.currentTile, currentSkill, true);
            StartCoroutine(ExecuteAction(currentSkill, currentCharacter, map.GetTiles(ActionType.None)));
        }
        else*/
            EndTurn();yield return null;
    }
    
    public void MoveCharacterStart(){

        //THIS FUNCTION IS NEEDED BECAUSE WE DISABLE THE MENU ON CONFIRMATION = YES, SO IT STOPS THE COROUTINE
        map.ToggleTilesVisibility(ActionType.None, true);
        StartCoroutine(MoveCharacter(currentCharacter, cursor.currentTile));
    }

    IEnumerator MoveCharacter(BattleCharacter character, Tile targetTile){

        //List<Tile> path = Pathfind(character.currentTile, CursorToTile());  //RE-ADD THIS IF PATHFIND FUNCTION IS TO BE USED        
        List<Tile> path = map.BuildPath(character.currentTile, targetTile); ;  //REMOVE THIS IF PATHFIND FUNCTION IS TO BE USED

        cursor.gameObject.SetActive(false);
        yield return StartCoroutine(MoveCameraToTarget(character.transform.position));
        battleCamera.SetParent(character.transform);

        Tile currentTile = character.currentTile;
        while (path.Count > 0){
            TurnToTarget(character, currentTile.gridPosition, path[0].gridPosition);

            if (path[0].height != currentTile.height){
                //JUMP UP
                if (path[0].height > currentTile.height + jumpHeight)
                    yield return StartCoroutine(JumpAnimationUp(character, currentTile.transform.position, path[0].transform.position));
                //VAULT UP
                else if (path[0].height > currentTile.height){
                    Vector3 edgePosition = new Vector3((currentTile.gridPosition.x + path[0].gridPosition.x) / 2f, path[0].transform.position.y, (currentTile.gridPosition.y + path[0].gridPosition.y) / 2f);
                    yield return StartCoroutine(MoveAnimation(character, edgePosition, 1.5f));
                }
                else {
                    //VAULT DOWN
                    Vector3 edgePosition = new Vector3((currentTile.gridPosition.x + path[0].gridPosition.x) / 2f, currentTile.transform.position.y, (currentTile.gridPosition.y + path[0].gridPosition.y) / 2f);
                    yield return StartCoroutine(MoveAnimation(character, edgePosition, 1.5f));
                    //JUMP DOWN
                    if (path[0].height + jumpHeight < currentTile.height)
                        yield return StartCoroutine(JumpAnimationDown(character, currentTile.transform.position, path[0].transform.position));
                }
            }

            //MOVE TO DESTINATION TILE
            yield return StartCoroutine(MoveAnimation(character, path[0].transform.position));

            currentTile = path[0];
            path.RemoveAt(0);
        }

        SetCharacterOnTile(targetTile, character);
        cursor.gameObject.SetActive(true);
        battleCamera.SetParent(null);
        SetCharacterFlag(character, "Move", true);
        character.CP -= 20;
        if (character.tag == "Ally"){
            if (character.hasActed)
                menu.WaitButtonClick();
            else{
                actionType = ActionType.CharacterMenu;
                ui.charDetailsMain.LoadCharacterData(character);
                menu.gameObject.SetActive(true);
            }
        }
    }

    public IEnumerator MoveAnimation(BattleCharacter character, Vector3 targetPosition, float speedOverride = 1f){

        while (character.transform.position != targetPosition){
            character.transform.position = Vector3.MoveTowards(character.transform.position, targetPosition, Time.deltaTime * moveSpeed * speedOverride);
            yield return null;
        }
    }

    IEnumerator JumpAnimationUp(BattleCharacter character, Vector3 currentTilePosition, Vector3 targetTilePosition){

        character.animator.Play(AnimationState.Jump);
        yield return new WaitForSeconds(0.25f);

        Vector3 jumpPosition = new Vector3(currentTilePosition.x, targetTilePosition.y, currentTilePosition.z);        
        yield return StartCoroutine(MoveAnimation(character, jumpPosition, 3));

        Vector3 edgePosition = new Vector3((currentTilePosition.x + targetTilePosition.x) / 2f, targetTilePosition.y, (currentTilePosition.z + targetTilePosition.z) / 2f);
        Vector3 startPosition = character.transform.position;
        int angle = 0;
        while (angle < 180){
            float sin = Mathf.Sin(Mathf.PI * 2 * angle / 360) * 0.5f;
            float time = angle / 180f;
            Vector3 targetPosition = Vector3.Lerp(startPosition, edgePosition, time);
            targetPosition = new Vector3(targetPosition.x, targetPosition.y + sin, targetPosition.z);
            character.transform.position = targetPosition;
            angle += 2;
            yield return null;
        }

        character.animator.Play(AnimationState.Idle);
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator JumpAnimationDown(BattleCharacter character, Vector3 currentTilePosition, Vector3 targetTilePosition){

        character.animator.Play(AnimationState.Jump);
        yield return new WaitForSeconds(0.25f);
        yield return StartCoroutine(MoveAnimation(character, targetTilePosition, 3));
        character.animator.Play(AnimationState.Idle);
        yield return new WaitForSeconds(0.1f);
    }

    public void ExecuteActionStart(){

        //THIS FUNCTION IS NEEDED BECAUSE WE DISABLE THE MENU ON CONFIRMATION = YES, SO IT STOPS THE COROUTINE
        map.ToggleTilesVisibility(ActionType.None, false);
        map.ToggleAreaVisibility(false);
        StartCoroutine(ExecuteAction(currentSkill, currentCharacter, map.GetTiles(ActionType.None)));
    }

    public IEnumerator ExecuteAction(Skill actionSkill, BattleCharacter user, List<Tile> targetTiles, bool counterAction = false){

        //Vector2Int targetGridPosition = currentCharacter.currentTile.gridPosition; ////////////////////////NAO LEMBRO PORQUE MAS ACHO QUE TEM QUE SER CURRENTCHARACTER AQUI POR CAUSA DE ALGUMA SKILL
        if (!counterAction){
            ui.charDetailsMain.LoadCharacterData(null);
            ui.charDetailsTarget.LoadCharacterData(null);
            cursor.gameObject.SetActive(false);

            Vector3 targetCameraPosition = Vector3.Lerp(user.transform.position, cursor.transform.position, 0.5f);
            yield return StartCoroutine(MoveCameraToTarget(targetCameraPosition));
            yield return new WaitForSeconds(0.5f);
            //targetGridPosition = map.GetTile(cursor).gridPosition;
        }

        Vector2Int targetGridPosition = targetTiles[0].gridPosition;
        TurnToTarget(user, user.currentTile.gridPosition, targetGridPosition);
        DisplayText(true, user, actionSkill.LocalizedName);
        yield return new WaitForSeconds(0.5f);

        //SKILL CHARGE LOGIC
        if (!chargeCasting && actionSkill.Charge > 0){
            user.animator.Play(AnimationState.Charge);
            user.chargingSkill = actionSkill;
            user.chargingTargetPos = targetGridPosition;
            user.hasMoved = true;
        }

        //REGULAR LOGIC        
        else{
            if (!counterAction){
                skill.SetSupportSkills(targetTiles[0]);
                yield return StartCoroutine(skill.ActivateNonActiveSkill(true));
            }

            //PROCESS ACTION FOR EACH TARGET
            user.animator.Play(AnimationState.Attack);            
            foreach (Tile tile in targetTiles){
                if (tile.character == null || tile.character.HP == 0)
                    continue;
                BattleCharacter characterToReact = tile.character; //necessary in case the character is moved during enemy action (like TACKLE)

                //CHECKS IF ATTACK HITS
                if (GetSuccessRate(actionSkill, user, tile.character) < Random.Range(0, 100))
                    DisplayText(false, tile.character, "Miss");
                
                //USES MAIN SKILL
                else
                    yield return StartCoroutine(skill.UseMainSkill(actionSkill, user, tile.character));

                if (!counterAction)
                    skill.SetReactionSkills(characterToReact);
            }

            //ACTIVATES SUPPORT AND REACTION SKILLS
            if (!counterAction)
                yield return StartCoroutine(skill.ActivateNonActiveSkill(false));            
        }

        if (counterAction)
            yield break;

        yield return new WaitForSeconds(1);

        menu.characterMenu.SetActive(true);
        menu.actMenu.SetActive(false);
        menu.skillMenu.gameObject.SetActive(false);
        cursor.gameObject.SetActive(true);
        SetCharacterFlag(user, "Act", true);
        user.CP -= 20; //////////////// LEMBRANDO QUE ADICIONEI O CURRENT CHARACTER HEIN!!!

        //SKILL CHARGE LOGIC
        if (chargeCasting || user.tag == "Enemy")
            EndTurn();

        //REGULAR LOGIC
        else{
            if (user.hasMoved)
                EndTurn();
            else{
                currentSkill = null;
                actionType = ActionType.CharacterMenu;
                yield return StartCoroutine(ResetCamera());
                ui.charDetailsMain.LoadCharacterData(user);
                menu.gameObject.SetActive(true);
            }
        }        
    }

    public int GetSuccessRate(Skill skill, BattleCharacter attacker, BattleCharacter defender){

        if (skill.Accuracy == 999 || defender.chargingSkill != null)
            return 100;
        else{
            int hitChance = skill.Accuracy + attacker.Accuracy - defender.Evade;
            string attackDirection = GetAttackDirection(attacker, defender);
            if (attackDirection == "Back")
                hitChance += 20;
            else if (attackDirection == "Side")
                hitChance += 10;
            return hitChance;
        }
    }

    public int GetDamage(Skill skill, BattleCharacter attacker, BattleCharacter defender){

        //ATTACKER CALCULATION
        float damage = 1;
        if (Mathf.Abs(skill.Damage) < 10) //damages over 10 are used by ITEMS, so damage will default to 1
            damage = skill.Physical ? attacker.PhyAttack : attacker.MagAttack /* + Random.Range(attacker.Level / 2, attacker.Level)*/;
        float damageModifiers = 1;
        float skillDamage = skill.Damage;

        if (skillDamage != 0){
            if (skillDamage > 0){

                //PHYSICAL ATTACKS CALCULATION (20% per height difference, 40% for back attack, 20% for side attack)
                if (skill.Physical){

                    //back/side attack calculation
                    string attackDirection = GetAttackDirection(attacker, defender);
                    if (attackDirection == "Back"){
                        damageModifiers += 0.4f;
                        if (skill.Name == "Backstab")
                            skillDamage *= 3;
                    }
                    else if (attackDirection == "Side"){
                        damageModifiers += 0.2f;
                        if (skill.Name == "Backstab")
                            skillDamage *= 2;
                    }

                    //height calculation
                    damageModifiers += (attacker.currentTile.height - defender.currentTile.height) * 0.2f;
                }

                //DEFENDER CALCULATIONS
                float defense = skill.Physical ? defender.PhyDefense : defender.MagDefense /* + Random.Range(defender.Level / 2, defender.Level)*/;
                damage *= 100 / (100 + defense);

                //DAMAGE BONUS TO CHARGING TARGETS (20%)
                if (defender.chargingSkill != null)
                    damageModifiers += 0.2f;
            }
            damage *= skillDamage;
        }

        //ELEMENTAL CALCULATION (15%/30% per element for physical/magical, 50% for strength/weakness)
        if (skill.Element != Element.None)
            damageModifiers += GetElementalModifier(skill, attacker, defender);

        damage *= damageModifiers;        
        damage = (damage > 0) ? Mathf.Clamp(damage, 1, 999) : Mathf.Clamp(damage, -999, -1);
        return (int)damage;
    }

    public float GetElementalModifier(Skill skill, BattleCharacter attacker, BattleCharacter defender, bool debuff = false){

        float modifier = 0;

        //SUMS THE ELEMENTS OF THE MAP, CASTER AND TARGET TILES
        float baseMultiplier = skill.Physical ? 0.15f : 0.3f;
        modifier += (attacker.currentTile.elements[(int)skill.Element] + defender.currentTile.elements[(int)skill.Element]) * baseMultiplier;

        //INCREASES EFFECT FOR OPPOSING ELEMENT FOR DAMAGE AND DEBUFFS
        if (skill.Damage > 0 || debuff){
            int opposingElementIndex = (int)skill.Element % 2 == 0 ? (int)skill.Element + 1 : (int)skill.Element - 1;
            if ((int)defender.Element == opposingElementIndex)
                modifier = modifier + 0.5f;
            else if (defender.Element == skill.Element)
                modifier = modifier + 0.5f;
        }

        return modifier;
    }

    public void ApplyDamage(int damage, BattleCharacter target, bool fatal = true){

        if (!fatal && damage >= target.HP)
            target.HP = 1;
        else
            target.HP -= damage;

        if (damage > 0) //no need to block healing skills
            target.animator.Play(AnimationState.Block);
        DisplayText(false, target, damage.ToString());
        if (target.HP == 0)
            Kill(target);
    }
        
    void Kill(BattleCharacter target){

        actionQueue.Remove(target);

        if (target.tag == "Ally"){
            target.chargingSkill = null;
            target.RemoveConditions(ConditionRemovalType.All);
        }
        else
            StartCoroutine(KillEnemy(target));

        ///////// CHECKS TO END BATTLE /////////
        /*if (currentSkill.EnemyTarget == 1 && charactersInBattle[1].Count == 0)
            StartCoroutine(EndBattle());
        else if (currentSkill.EnemyTarget == 0 && charactersInBattle[0].FindAll(x => x.HP == 0).Count == charactersInBattle[0].Count)
            StartCoroutine(EndBattle(true));*/ //GAMEOVER!!!!!!!!!
    }

    IEnumerator KillEnemy(BattleCharacter target){

        yield return new WaitForSeconds(0.2f);
        float time = 0.8f;
        while (target.sprite.color.a > 0){
            time -= Time.deltaTime;
            target.sprite.color = new Color(1, 1, 1, time);
            yield return null;
        }
        Destroy(target.gameObject);
    }

    public void EndTurn(){

        //PROCESSES SPEED FOR ALL OTHER CHARACTERS
        while (actionQueue[1].CP < 100){
            foreach (BattleCharacter character in actionQueue)
                character.CP += character.Speed;
        }

        //RESETS CURRENT CHARACTER VARIABLES
        if (chargeCasting)
            chargeCasting = false;
        currentCharacter.CP -= 60;
        SetCharacterFlag(currentCharacter, null, false);

        //FINAL PROCESSES
        ReorderQueue(currentCharacter, currentCharacter.chargingSkill);
        StartCoroutine(StartTurn());
    }

    public void ReorderQueue(BattleCharacter character, Skill chargingSkill){        

        //REGULAR LOGIC - //chargingSkill needs to be added on ExecuteAction so that this function can be used by other sources to just delay characters turns
        if (chargingSkill == null){
            actionQueue.Remove(character);
            actionQueue.Add(character);
            for (int i = actionQueue.Count - 2; i >= 0; i--){
                if (character.timeToTurn >= actionQueue[i].timeToTurn)
                    break;
                else{
                    actionQueue[i + 1] = actionQueue[i];
                    actionQueue[i] = character;
                }
            }
        }

        //SKILL CHARGE LOGIC
        else{
            int chargeTime = GetChargeTime();
            if (chargeTime >= actionQueue.Count)
                chargeTime--;
            character.timeToTurn = actionQueue[chargeTime].timeToTurn;
            actionQueue.Remove(character);
            actionQueue.Insert(chargeTime, character);
        }
    }

    public void DisplayText(bool isName, BattleCharacter target, string text){

        if (isName){
            GameObject battleSpeech = Instantiate(battleSpeechPrefab, worldSpaceCanvas);
            battleSpeech.transform.position = target.transform.position;
            battleSpeech.GetComponentInChildren<TextMeshProUGUI>().text = text;
            Destroy(battleSpeech, 1);
        }
        else{
            BattleDamage battleDamage = Instantiate(battleTextPrefab, worldSpaceCanvas).GetComponent<BattleDamage>();
            battleDamage.ProcessDamage(text, target.transform.position);
        }
    }

    public int GetChargeTime(){

        float modifier = 1;
        if (currentSkill.Charge == 1)
            modifier = 0.34f;
        else if (currentSkill.Charge == 2)
            modifier = 0.68f;

        return Mathf.Clamp((int)(actionQueue.Count * modifier), 1, 99);
    }
        
    public string GetAttackDirection(BattleCharacter attacker, BattleCharacter defender){

        int defenderPositionBack = defender.currentTile.gridPosition.y;
        int defenderPositionSide = defender.currentTile.gridPosition.x;
        int attackerPositionBack = attacker.currentTile.gridPosition.y;
        int attackerPositionSide = attacker.currentTile.gridPosition.x;

        if (defender.direction == 2 || defender.direction == 3){
            defenderPositionBack = defender.currentTile.gridPosition.x;
            defenderPositionSide = defender.currentTile.gridPosition.y;
            attackerPositionBack = attacker.currentTile.gridPosition.x;
            attackerPositionSide = attacker.currentTile.gridPosition.y;
        }

        int positionDifferenceBack = defenderPositionSide - attackerPositionSide;
        int positionDifferenceSide = defenderPositionBack - attackerPositionBack;

        if (defender.direction == 0 || defender.direction == 2){
            positionDifferenceBack *= -1;
            positionDifferenceSide *= -1;
        }

        if (defenderPositionBack + positionDifferenceBack <= attackerPositionBack && defenderPositionBack - positionDifferenceBack >= attackerPositionBack)
            return "Back";
        else if (
            (defenderPositionSide + positionDifferenceSide >= attackerPositionSide && defenderPositionSide - positionDifferenceSide <= attackerPositionSide) ||
            (defenderPositionSide - positionDifferenceSide >= attackerPositionSide && defenderPositionSide + positionDifferenceSide <= attackerPositionSide))
            return "Side";
        return null;
    }

    public void TurnToTarget(BattleCharacter characterToTurn, Vector2Int currentGridPosition, Vector2Int targetGridPosition){

        int xDif = targetGridPosition.x - currentGridPosition.x;
        int yDif = targetGridPosition.y - currentGridPosition.y;

        bool front = (characterToTurn.direction == 1 || characterToTurn.direction == 3) ? true : false;
        if (xDif + yDif != 0)
            front = (xDif < yDif && Mathf.Abs(xDif) > Mathf.Abs(yDif)) || (xDif > yDif && Mathf.Abs(xDif) < Mathf.Abs(yDif)) || (xDif == yDif && xDif < 0) ? true : false;

        bool flipX = characterToTurn.sprite.flipX;
        if (xDif != yDif)
            flipX = (xDif < yDif) ? false : true;

        if (front && flipX)
            characterToTurn.direction = 3; //right
        else if (front && !flipX)
            characterToTurn.direction = 1; //down
        else if (!front && flipX)
            characterToTurn.direction = 0; //up
        else if (!front && !flipX)
            characterToTurn.direction = 2; //left
    }
    
    public void SetCharacterOnTile(Tile tileToBeSet, BattleCharacter characterToBeSet){

        //if (characterToBeSet.currentTile.character == characterToBeSet) //this will avoid problems with character swap
        characterToBeSet.currentTile.character = null; //removes character from current tile
        UpdateAuras(characterToBeSet, false); //removes auras from character's current tile
        tileToBeSet.character = characterToBeSet; //sets tile with new char
        characterToBeSet.currentTile = tileToBeSet; //sets character to new tile
        UpdateAuras(characterToBeSet, true); //adds auras from character into new tiles
    }

    void UpdateAuras(BattleCharacter character, bool add){

        map.GenerateRange(character.currentTile, character.SupportSkill);
        foreach (Tile tile in map.GetTiles(ActionType.None)){
            if (add)
                tile.aurasList.Add(character.SupportSkill);
            else
                tile.aurasList.Remove(character.SupportSkill);
        }         
    }
    
    void SetCharacterFlag(BattleCharacter character, string type, bool flag){

        if (type == "Move"){
            character.hasMoved = flag;
            menu.actionMoveButton.interactable = !flag;
        }
        else if (type == "Act"){
            character.hasActed = flag;
            menu.actionActButton.interactable = !flag;
        }
        else{
            character.hasMoved = flag;
            character.hasActed = flag;
            menu.actionMoveButton.interactable = !flag;
            menu.actionActButton.interactable = !flag;
        }
    }
    
    public IEnumerator ResetCamera(){

        cursor.Return();        
        yield return StartCoroutine(MoveCameraToTarget(currentCharacter.transform.position));
        ui.charDetailsMain.LoadCharacterData(currentCharacter);
    }
    
    IEnumerator MoveCameraToTarget(Vector3 targetCameraPosition, float speedOverride = 10){

        while (battleCamera.position != targetCameraPosition){
            battleCamera.position = Vector3.MoveTowards(battleCamera.position, targetCameraPosition, Time.deltaTime * speedOverride);
            yield return null;
        }
    }

    EnemyPlay GenerateEnemyPlay(){
                
        map.GenerateMoveTilesInRange(currentCharacter);
        
        List<Tile> inMoveTiles = map.GetTiles(ActionType.MoveSelect);
        inMoveTiles.Add(currentCharacter.currentTile);
        List<EnemyPlay> enemyPlays = new List<EnemyPlay>();
        Tile closestEnemyTile = GetClosestEnemyTile(currentCharacter);
        bool inRange = (map.GetTileDistance(currentCharacter.currentTile, closestEnemyTile) > currentCharacter.Move + currentCharacter.Skills[0].Range + currentCharacter.Skills[0].Area) ?
            false : true;

        foreach (Tile moveTile in inMoveTiles){ //finds all tiles the character can move
            enemyPlays.Add(new EnemyPlay(moveTile.gridPosition, -Vector2Int.one, -map.GetTileDistance(moveTile, closestEnemyTile) * 10)); //removes effectiveness the longer it is
            if (!inRange) //if enemies are too far, doesn't even check skills range
                continue;

            map.GenerateRange(moveTile, currentCharacter.Skills[0]);
            foreach (Tile targetTile in map.GetTiles(ActionType.TargetSelect)){ //finds all targets he can hit in each tile for a particular skill

                /*map.GenerateSkillTilesInRange(targetTile, currentSkill, true);
                foreach(Tile areaTile in map.GetTiles(ActionType.None)){*/

                if (targetTile.character != null && targetTile.character.HP > 0 && targetTile.character.tag != currentCharacter.tag){ //calculate the effectiveness of the play and put it on priority
                    int effectiveness = 200; //if a character is found, it's already a better play than just moving
                    effectiveness -= map.GetTileDistance(currentCharacter.currentTile, moveTile) * 10; //removes effectiveness the more it needs to move
                    effectiveness += map.GetTileDistance(moveTile, targetTile) * 10; //adds effectiveness the longer is the position to attack
                    effectiveness -= 100 * targetTile.character.HP / targetTile.character.HPMax; //adds effectiveness the less hp the target has
                    enemyPlays[enemyPlays.Count - 1].targetPosition = targetTile.gridPosition;
                    enemyPlays[enemyPlays.Count - 1].effectiveness = effectiveness;
                }
            }
        }

        /*enemyPlays.Sort((x, y) => y.effectiveness.CompareTo(x.effectiveness)); //DELETAAAAAAAAAAARRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR
        foreach (EnemyPlay play in enemyPlays)
            print("Tile to move: " + play.movePosition + " ||| Target char: " + play.targetPosition + " ||| Weight: " + play.effectiveness);*/

        return GetBestEnemyPlay(enemyPlays);
    }

    Tile GetClosestEnemyTile(BattleCharacter centerCharacter){

        BattleCharacter closestEnemy = null;
        int closestDistance = 99;
        foreach (BattleCharacter character in actionQueue){
            if (character.tag != centerCharacter.tag){
                int distance = map.GetTileDistance(centerCharacter.currentTile, character.currentTile);
                if (distance < closestDistance){
                    closestDistance = distance;
                    closestEnemy = character;
                }
            }
        }
        return closestEnemy.currentTile;
    }    

    EnemyPlay GetBestEnemyPlay(List<EnemyPlay> listOfPlays){

        EnemyPlay enemyPlay = new EnemyPlay(-Vector2Int.one, -Vector2Int.one, -999);

        for (int i = 0; i < listOfPlays.Count; i++){
            if (listOfPlays[i].effectiveness > enemyPlay.effectiveness)
                enemyPlay = listOfPlays[i];
        }
        return enemyPlay;
    }
}