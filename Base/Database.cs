using System.Collections.Generic;
using UnityEngine;

public class Database : MonoBehaviour{
    
    public Sprite[] Portraits;
    public Texture2D[] CharacterSheets;
    public Sprite[] Elements;
    public Sprite[] DamageTypes;

    public List<Character> Characters = new List<Character>(); //this list is just the initial character stats!!!
    public List<Character> Enemies = new List<Character>();

    public List<List<Skill>> Skills = new List<List<Skill>>();
    List<Skill> CaesarSkills = new List<Skill>();
    List<Skill> AryaSkills = new List<Skill>();
    List<Skill> DariusSkills = new List<Skill>();
    List<Skill> EnemySkills = new List<Skill>();

    public List<List<Item>> Items = new List<List<Item>>();
    List<Item> Weapons = new List<Item>();
    List<Item> Supports = new List<Item>();
    List<Item> Helmets = new List<Item>();
    List<Item> Armors = new List<Item>();
    List<Item> Boots = new List<Item>();
    List<Item> Accessories = new List<Item>();
    List<Item> Usableitems = new List<Item>();
    List<Item> KeyItems = new List<Item>();

    public Player Player; //instantiated on load by main menu

    void Awake(){

        Skills.Add(CaesarSkills);
        Skills.Add(AryaSkills);
        Skills.Add(DariusSkills);
        Skills.Add(EnemySkills);

        Items.Add(Weapons);
        Items.Add(Supports);
        Items.Add(Helmets);
        Items.Add(Armors);
        Items.Add(Boots);
        Items.Add(Accessories);
        Items.Add(Usableitems);
        Items.Add(KeyItems);

        ////////////////////////////////////////////////////////////////////////////// SKILLS //////////////////////////////////////////////////////////////////////////////
        //////////////////////////// CAESAR -- blacksmith
        CaesarSkills.Add(new Skill(0, "Tackle",             SkillType.Normal, 1, 2, 0, 0, true, 100, 1f));                      CaesarSkills[0].Description = "Deals 100% damage and Push 1 (Push interrupts casting)";
        CaesarSkills.Add(new Skill(1, "Earth Hammer",       SkillType.Normal, 1, 2, 0, 0, true, 100, 1.2f, Element.Earth));     CaesarSkills[1].Description = "Deals 120% damage and reduces 20 CP";
        CaesarSkills.Add(new Skill(2, "Rock Bullet",        SkillType.Projectile, 3, 4, 0, 1, false, 100, 0.8f, Element.Earth));CaesarSkills[2].Description = "Deals 80% damage";
        CaesarSkills.Add(new Skill(3, "Poison Scratch",     SkillType.Normal, 1, 2, 0, 0, true, 100, 0.6f, Element.Earth));     CaesarSkills[3].Description = "Deals 60% damage plus 30% each round for 3 rounds";
        CaesarSkills.Add(new Skill(4, "Sand Curtain",       SkillType.Normal, 3, 3, 1, 1, false, 100, 0, Element.Earth));       CaesarSkills[4].Description = "Reduces 40% Accuracy for 3 Rounds";
        CaesarSkills.Add(new Skill(5, "Earth Shield",       SkillType.Normal, 1, 2, 0, 1, false, 999, 0, Element.Earth));       CaesarSkills[5].Description = "Provides 200% Defense and Magic Defense for 1 Round";
        CaesarSkills.Add(new Skill(6, "Nature's Blessing",  SkillType.Normal, 2, 6, 0, 1, false, 999, 0, Element.Earth));       CaesarSkills[6].Description = "Removes all debuffs";
        CaesarSkills.Add(new Skill(7, "Charging Strike",    SkillType.Projectile, 4, 1, 0, 0, true, 100, 1f));                  CaesarSkills[7].Description = "Deals 100% damage";
        CaesarSkills.Add(new Skill(0, "Cover",              SkillType.Support, 1, 2, 0, 0, true, 999, 0));                      CaesarSkills[8].Description = "When an adjacent ally with less HP is targeted by a single target damaging attack, changes places with them";
        //CaesarSkills.Add(new Skill(0, "Enrage",             SkillType.Reaction, 0, 0, 0, 0, true, 999, 0));                     CaesarSkills[9].Description = "When damaged, increases Attack and Magic Attack in 10%, cumulative until the end of the battle";
        CaesarSkills.Add(new Skill(0, "Counter",            SkillType.Reaction, 1, 2, 0, 0, true, 100, 1));                     CaesarSkills[9].Description = "Counters single target attacks within range with 100% damage";
        //warcry

        //////////////////////////// ARYA -- alchemist
        AryaSkills.Add(new Skill(0, "Backstab",         SkillType.Normal, 1, 2, 0, 0, true, 100, 0.5f));                    AryaSkills[0].Description = "Deals 50% damage to the front, 100% to the sides, 150% to the back"; //contains specific logic inside GetDamage()
        AryaSkills.Add(new Skill(1, "Pickpocket",       SkillType.Normal, 1, 2, 0, 0, true, 50, 0));                        AryaSkills[1].Description = "Steals an item from target (expires after battle)"; //////////////////////////////////////////////////////////// COMO VAI FUNCIONAR ISSO????????????
        AryaSkills.Add(new Skill(2, "Flash Cut",        SkillType.Normal, 1, 2, 0, 0, true, 100, 1, Element.Light));        AryaSkills[2].Description = "Deals 100% damage and crosses Arya to the other side (if possible)";
        AryaSkills.Add(new Skill(3, "Shocking Touch",   SkillType.Normal, 1, 2, 0, 1, false, 100, 1f, Element.Light));      AryaSkills[3].Description = "Deals 100% damage and slightly delays target Turn";
        AryaSkills.Add(new Skill(4, "Enlighten",        SkillType.Normal, 3, 8, 0, 1, false, 999, 0, Element.Light));       AryaSkills[4].Description = "Provides 100% Magic Attack and Magic Defense for 3 Rounds";
        AryaSkills.Add(new Skill(5, "Healing Touch",    SkillType.Normal, 1, 2, 0, 2, false, 999, -1.2f, Element.Light));   AryaSkills[5].Description = "Heals 120% HP";
        AryaSkills.Add(new Skill(6, "Provoke",          SkillType.Normal, 5, 8, 0, 0, true, 999, 0));                       AryaSkills[6].Description = "Target turns to you and gets Evasion reduced by 30% for 2 turns";
        AryaSkills.Add(new Skill(0, "Flash",            SkillType.Support, 2, 4, 0, 0, false, 999, 0));                     AryaSkills[7].Description = "When an ally within range is targeted by a single target damaging attack, reduces 40% Accuracy"; //DEVE SER LIGHT!?!?!?!??!
        AryaSkills.Add(new Skill(0, "Backstep",         SkillType.Reaction, 1, 2, 0, 0, true, 999, 0));                     AryaSkills[8].Description = "When damaged, if possible steps away in the attacker's direction and increases Evasion by 50% until your next turn";

        //////////////////////////// DARIUS -- cook
        DariusSkills.Add(new Skill(0, "Power Shot",     SkillType.Normal, 4, 8, 0, 2, true, 100, 2f));                  DariusSkills[0].Description = "Deals 200% damage";
        DariusSkills.Add(new Skill(1, "Quick Shot",     SkillType.Normal, 4, 8, 0, 0, true, 140, 0.6f));                DariusSkills[1].Description = "Deals 60% damage and provides 25 CP to self";
        DariusSkills.Add(new Skill(2, "Piercing Arrow", SkillType.Projectile, 6, 2, 0, 0, true, 100, 0.8f));            DariusSkills[2].Description = "Deals 80% damage to everyone on it's path";
        DariusSkills.Add(new Skill(3, "Wind Shot",      SkillType.Normal, 4, 8, 0, 0, true, 120, 1f, Element.Wind));    DariusSkills[3].Description = "Deals 100% damage";
        DariusSkills.Add(new Skill(4, "Wind Jump",      SkillType.Normal, 3, 8, 0, 0, false, 999, 0, Element.Wind));    DariusSkills[4].Description = "Jumps to the target tile";
        DariusSkills.Add(new Skill(5, "Wind Push",      SkillType.Normal, 1, 2, 0, 0, true, 999, 0, Element.Wind));     DariusSkills[5].Description = "Push 5";
        DariusSkills.Add(new Skill(6, "Hermes Boots",   SkillType.Normal, 1, 2, 0, 1, false, 999, 0, Element.Wind));    DariusSkills[6].Description = "Provides 2 Move and 4 Jump for 3 turns";
        DariusSkills.Add(new Skill(0, "Fire Support",   SkillType.Support, 4, 8, 0, 0, true, 100, 1));                  DariusSkills[7].Description = "When an enemy within range is targeted by a single target damaging attack, deals 100% damage";
        DariusSkills.Add(new Skill(0, "Defend",         SkillType.Reaction, 0, 0, 0, 0, true, 999, 0));                 DariusSkills[8].Description = "When targeted by a single target damaging skill, turns to the attacker and increases Defense and Magic Defense by 100%";
        //deadly shot (bonus damage com low hp)
        //disarming shot

        //////////////////////////// ENEMY ---- TEMPORARY SKILLS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        EnemySkills.Add(new Skill(0, "Tackle",          SkillType.Normal, 1, 2, 0, 0, true, 100, 1f));
        EnemySkills.Add(new Skill(0, "DummySupport",    SkillType.Support, 1, 2, 0, 0, false, 999, 0));
        EnemySkills.Add(new Skill(0, "DummyReact",      SkillType.Reaction, 1, 2, 0, 0, true, 999, 0));
        
        ////////////////////////////////////////////////////////////////////////////// CHARACTERS //////////////////////////////////////////////////////////////////////////////
        Characters.Add(new Character(0, "Caesar",   5, Element.Earth, 200, new Attributes(10, 6, 10, 6, 8, 8, 8), Skills[0]));
        Characters.Add(new Character(1, "Arya",     5, Element.Light, 100, new Attributes(4, 8, 4, 8, 4, 4, 12), Skills[1]));
        Characters.Add(new Character(2, "Darius",   5, Element.Wind,  150, new Attributes(8, 6, 6, 8, 6, 2, 10), Skills[2]));
        Enemies.Add(new Character(3, "Enemy",      5, Element.Water, 100, new Attributes(10, 10, 10, 10, 3, 2, 7), Skills[3]));

        ////////////////////////////////////////////////////////////////////////////// ITEMS //////////////////////////////////////////////////////////////////////////////
        Weapons.Add(new Weapon(0, "Glove",              ItemType.Weapon, new Attributes(PhyAttack: 1), 0));
        Weapons.Add(new Weapon(1, "Dagger",             ItemType.Weapon, new Attributes(PhyAttack: 1), 1));
        Weapons.Add(new Weapon(2, "Bow",                ItemType.Weapon, new Attributes(PhyAttack: 1), 2));

        Supports.Add(new Equipment(0, "Off Glove",      ItemType.Support, new Attributes(PhyAttack: 1), 0));
        Supports.Add(new Equipment(1, "Dagger",         ItemType.Support, new Attributes(MagAttack: 1), 1));
        Supports.Add(new Equipment(2, "Arrow",          ItemType.Support, new Attributes(MagAttack: 1), 2));

        Helmets.Add(new Equipment(1, "Heavy Helmet",    ItemType.Helmet, new Attributes(PhyDefense: 1, MagDefense: 1)));
        Helmets.Add(new Equipment(0, "Light Hat",       ItemType.Helmet, new Attributes(MagDefense: 2)));

        Armors.Add(new Equipment(1, "Heavy Armor",      ItemType.Armor, new Attributes(PhyDefense: 2)));
        Armors.Add(new Equipment(0, "Light Clothes",    ItemType.Armor, new Attributes(PhyDefense: 1, MagDefense: 1)));

        Boots.Add(new Equipment(1, "Heavy Boots",       ItemType.Boots, new Attributes(PhyDefense: 1, MagDefense: 1)));
        Boots.Add(new Equipment(0, "Light Shoe",        ItemType.Boots, new Attributes(Move: 1)));

        Accessories.Add(new Equipment(0, "Ring",        ItemType.Accessory, new Attributes(Evade: 10, Jump: 1)));

        Usableitems.Add(new Usable(0, "Potion",     ItemType.Usable, new Skill(999, "Potion", SkillType.Normal, 3, 6, 0, 0, true, 999, -50))); ((Usable)Usableitems[0]).Skill.Description = "Heals 50 HP";
        Usableitems.Add(new Usable(1, "Shuriken",   ItemType.Usable, new Skill(999, "Shuriken", SkillType.Normal, 3, 6, 0, 0, true, 100, 50))); ((Usable)Usableitems[1]).Skill.Description = "Causes 50 damage";



        //VALORES TEMPORARIOS
        //VALORES TEMPORARIOS
        //VALORES TEMPORARIOS
        Player = new Player();
        Player.Characters = Characters;
        Player.Characters[0].Equip(GetEquipmentByIndex(ItemType.Weapon, 0));
        Player.Characters[1].Equip(GetEquipmentByIndex(ItemType.Weapon, 1));
        Player.Characters[2].Equip(GetEquipmentByIndex(ItemType.Weapon, 2));
        Player.AddItem(new PlayerItem(Items[(int)ItemType.Usable][0], 5));
        Player.AddItem(new PlayerItem(Items[(int)ItemType.Usable][1], 2));
        //if (database.Player.Characters[0].Equipments[0] is Equipment){ } //pode ser usado pra checar se é equipment
        //database.Player.Characters[0].Equipments[0] = database.Items[0][0] as Equipment; //seta null se nao for um equipment

        //VALORES TEMPORARIOS
        //VALORES TEMPORARIOS
        //VALORES TEMPORARIOS
    }

    public Equipment GetEquipmentByIndex(ItemType Type, int Index){               

        return (Equipment)Items[(int)Type][Index];
    }
}