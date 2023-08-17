using System.Collections.Generic;
using UnityEngine;

public class BattleCharacter : MonoBehaviour{

    public SpriteRenderer sprite;
    public AnimationManager animator;
    public Sprite Portrait;
    
    public int Level;
    public Element Element;

    public int HPMax;
    int HPCurrent;
    public int HP { get { return HPCurrent; } set { HPCurrent = Mathf.Clamp(value, 0, HPMax); } }

    public int Id;
    public int PhyAttack { get { return BaseStats.PhyAttack + BonusStats.PhyAttack; } set { BaseStats.PhyAttack = value; } }
    public int MagAttack { get { return BaseStats.MagAttack + BonusStats.MagAttack; } set { BaseStats.MagAttack = value; } }
    public int PhyDefense { get { return BaseStats.PhyDefense + BonusStats.PhyDefense; } set { BaseStats.PhyDefense = value; } }
    public int MagDefense { get { return BaseStats.MagDefense + BonusStats.MagDefense; } set { BaseStats.MagDefense = value; } }
    public int Move { get { return BaseStats.Move + BonusStats.Move; } set { BaseStats.Move = value; } }
    public int Jump { get { return BaseStats.Jump + BonusStats.Jump; } set { BaseStats.Jump = value; } }
    public int Speed { get { return BaseStats.Speed + BonusStats.Speed; } set { BaseStats.Speed = value; } }
    public int Evade { get { return BaseStats.Evade + BonusStats.Evade; } set { BaseStats.Evade = value; } }
    public int Accuracy { get { return BaseStats.Accuracy + BonusStats.Accuracy; } set { BaseStats.Accuracy = value; } } //PRECISA DE BASE ACCURACY???
    public int DamageOverTime { get { return BaseStats.DamageOverTime + BonusStats.DamageOverTime; } set { BaseStats.DamageOverTime = value; } } //PRECISA DE BASE DAMAGEOVERTIME???

    Attributes BaseStats = new Attributes();
    Attributes BonusStats = new Attributes();

    public Skill AttackSkill;
    public List<Skill> Skills;
    public Skill SupportSkill;
    public Skill ReactionSkill;

    //battle variables
    public Tile currentTile;
    public int CP;
    public int timeToTurn { get { return (100 - CP) / Speed; } set { CP = 100 - (Speed * value); } }
    int Direction;
    public int direction { get { return Direction; } set { if (Direction != value) { Direction = value; animator.Play(AnimationState.Idle); } } }
    public bool hasMoved = false;
    public bool hasActed = false;
    public Vector2Int chargingTargetPos;

    public Skill chargingSkill;
    public List<Condition> conditionsList = new List<Condition>();

    public void InstantiateCharacter(Character character, string charTag){

        Database database = FindObjectOfType<Database>();

        tag = charTag;
        Portrait = database.Portraits[character.Id];
        animator.InstantiateAnimations(database.CharacterSheets[character.Id]);
        name = character.Name;
        Level = character.Level;
        Element = character.Element;
        HPMax = character.HPMax;
        HPCurrent = HPMax;
        Id = character.Id;
        BaseStats.Sum(character.Attributes);
        
        Skills = character.Skills.FindAll(x => x.Type != SkillType.Support && x.Type != SkillType.Reaction);
        SupportSkill = character.Skills[character.Skills.Count - 2];
        SupportSkill.Character = this;
        ReactionSkill = character.Skills[character.Skills.Count - 1];
        ReactionSkill.Character = this;

        if (tag == "Ally"){
            BaseStats.Sum(character.GetEquipmentsAttributes());
            AttackSkill = ((Weapon)character.Equipments[(int)ItemType.Weapon]).Skill;
        }        
        
        direction = 1;
        CP = Speed * 4;
    }

    public void ApplyCondition(Condition condition){

        Condition existingCondition = conditionsList.Find(x => x.Name == condition.Name);
        if (existingCondition == null){
            conditionsList.Add(condition);
            //if (condition.Attributes != null)
                BonusStats.Sum(condition.Attributes);
        }
        else
            existingCondition.Duration = condition.Duration;
    }

    public void RemoveCondition(Condition condition){

        //if (condition.Attributes != null)
            BonusStats.Sum(condition.Attributes, false);
        conditionsList.Remove(condition);
    }

    public void RemoveConditions(ConditionRemovalType type){

        for (int i = conditionsList.Count - 1; i >= 0; i--){
            switch (type){
                case ConditionRemovalType.Positives:
                    if (conditionsList[i].Positive)
                        goto default;
                    break;
                case ConditionRemovalType.Negatives:
                    if (!conditionsList[i].Positive)
                        goto default;
                    break;
                default:
                    RemoveCondition(conditionsList[i]);
                    break;
            }            
        }
    }
}