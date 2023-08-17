//E SE EU BOTAR QUANTITY DENTRO DE ITEM PRA DELETAR O PLAYERITEM!?!?!?!?!?
//E SE EU BOTAR QUANTITY DENTRO DE ITEM PRA DELETAR O PLAYERITEM!?!?!?!?!?
//E SE EU BOTAR QUANTITY DENTRO DE ITEM PRA DELETAR O PLAYERITEM!?!?!?!?!?

using System.Collections.Generic;
using UnityEngine;

public enum AnimationState { Idle, Jump, Attack, Block, Charge, Dead }
public enum TileType { Grass, Water, Stone, Earth }
public enum ActionType { None, MoveSelect, TargetSelect, CharacterMenu, ConfirmationMenu, ActMenu, SkillMenu, ItemMenu, WaitMenu } //used by battle navigation
public enum SkillType { Normal, Projectile, Support, Reaction }
public enum ItemType { Weapon, Support, Helmet, Armor, Boots, Accessory, Usable, Key }
public enum Element { Fire, Water, Wind, Earth, Light, Dark, None }
public enum ConditionRemovalType { All, Positives, Negatives }

public class Player{

    public List<Character> Characters;

    List<PlayerItem> InventoryWeapons;
    List<PlayerItem> InventorySupports;
    List<PlayerItem> InventoryHelmets;
    List<PlayerItem> InventoryArmors;
    List<PlayerItem> InventoryBoots;
    List<PlayerItem> InventoryAccessories;
    List<PlayerItem> InventoryUsableItems;
    List<PlayerItem> InventoryKeyItems;

    int[] CharacterSpecificExperience; //CADA PERSONAGEM TEM SEU XP INDIVIDUAL
    int[] CharacterSharedExperience; //CADA PERSONAGEM TEM SEU XP INDIVIDUAL E UM XP SHARED QUE QUALQUER UM PODE USAR PRA COMPRAR BOND COM ELE

    public List<List<PlayerItem>> Inventory = new List<List<PlayerItem>>();

    public Player(){

        Characters = new List<Character>();
        InventoryWeapons = new List<PlayerItem>();
        InventorySupports = new List<PlayerItem>();
        InventoryHelmets = new List<PlayerItem>();
        InventoryArmors = new List<PlayerItem>();
        InventoryBoots = new List<PlayerItem>();
        InventoryAccessories = new List<PlayerItem>();
        InventoryUsableItems = new List<PlayerItem>();
        InventoryKeyItems = new List<PlayerItem>();

        Inventory.Add(InventoryWeapons);
        Inventory.Add(InventorySupports);
        Inventory.Add(InventoryHelmets);
        Inventory.Add(InventoryArmors);
        Inventory.Add(InventoryBoots);
        Inventory.Add(InventoryAccessories);
        Inventory.Add(InventoryUsableItems);
        Inventory.Add(InventoryKeyItems);
    }

    public void AddItem(PlayerItem Item){

        Inventory[(int)Item.Item.Type].Add(Item);
    }
}

public class PlayerItem{

    public Item Item;
    public int Quantity;

    public PlayerItem(Item Item, int Quantity){

        this.Item = Item;
        this.Quantity = Quantity;
    }
}

public class Attributes{

    public int PhyAttack;
    public int MagAttack;
    public int PhyDefense;
    public int MagDefense;
    public int Move;
    public int Jump;
    public int Speed;
    public int Evade;
    public int Accuracy;
    public int DamageOverTime;

    public Attributes(int PhyAttack = 0, int MagAttack = 0, int PhyDefense = 0, int MagDefense = 0, int Move = 0, int Jump = 0, int Speed = 0, int Evade = 0, int Accuracy = 0, int DamageOverTime = 0){

        this.PhyAttack = PhyAttack;
        this.MagAttack = MagAttack;
        this.PhyDefense = PhyDefense;
        this.MagDefense = MagDefense;
        this.Move = Move;
        this.Jump = Jump;
        this.Speed = Speed;
        this.Evade = Evade;
        this.Accuracy = Accuracy;
        this.DamageOverTime = DamageOverTime;
    }

    public void Sum(Attributes attributes, bool add = true){

        int modifier = add ? 1 : -1;
        PhyAttack += attributes.PhyAttack * modifier;
        MagAttack += attributes.MagAttack * modifier;
        PhyDefense += attributes.PhyDefense * modifier;
        MagDefense += attributes.MagDefense * modifier;
        Move += attributes.Move * modifier;
        Jump += attributes.Jump * modifier;
        Speed += attributes.Speed * modifier;
        Evade += attributes.Evade * modifier;
        Accuracy += attributes.Accuracy * modifier;
        DamageOverTime += attributes.DamageOverTime * modifier;
    }
}

public class Character {

    public int Id;
    public string Name;
    public int Level;
    public Element Element;
    public int HPMax;
    public Attributes Attributes = new Attributes();
    public List<Skill> Skills;

    public Equipment[] Equipments = new Equipment[7];

    public Character(int Id, string Name, int Level, Element Element, int HPMax, Attributes Attributes, List<Skill> Skills){

        this.Id = Id;
        this.Name = Name;
        this.Level = Level;
        this.Element = Element;
        this.HPMax = HPMax;
        this.Attributes.Sum(Attributes);
        this.Skills = Skills;
    }

    //USED BY SAVE FUNCTION
    /*public int GetEquipmentId(Item equipment) {
        if (equipment != null)
            return equipment.Id;
        return 9999;
    }*/

    //USED TO AVOID NULL ISSUES
    public Attributes GetEquipmentsAttributes() {

        Attributes attributes = new Attributes();
        foreach (Equipment equipment in Equipments){
            if (equipment == null)
                continue;
            attributes.Sum(equipment.Attributes);
        }
        return attributes;
    }

    public void Equip(Item equipment){

        Equipment equip = (Equipment)equipment;
        Equipments[(int)equip.Type] = equip;
    }
}

public class Skill {

    public int Id;
    public string Name;
    public SkillType Type;
    public int Range;
    public int Height;
    public int Area;
    public int Charge; //0 - no charge // 1 - fast // 2 - medium // 3 - long
    //public bool Dead;

    public bool Physical;
    public int Accuracy;
    public float Damage;
    public Element Element;

    public string LocalizedName;
    public string Description;
    public BattleCharacter Character; //used by battle for support skills

    public Skill(int Id, string Name, SkillType Type, int Range, int Height, int Area, int Charge, bool Physical, int Accuracy, float Damage, Element Element = Element.None) {

        this.Id = Id;
        this.Name = Name;
        this.Type = Type;
        this.Range = Range;
        this.Height = Height;
        this.Area = Area;
        this.Charge = Charge;
        this.Physical = Physical;
        this.Accuracy = Accuracy;
        this.Damage = Damage;
        this.Element = Element;
        LocalizedName = Name;
    }
}

public class Item {

    public int Id;
    public string Name;
    public ItemType Type;

    public string LocalizedName;
    public string Description;

    public Item(int Id, string Name, ItemType Type){

        this.Id = Id;
        this.Name = Name;
        this.Type = Type;
    }
}

public class Usable : Item {

    public Skill Skill;

    public Usable(int Id, string Name, ItemType Type, Skill Skill) : base(Id, Name, Type){

        this.Skill = Skill;
    }
}

public class Equipment : Item {

    public Attributes Attributes;
    public int CharId;

    public Equipment(int Id, string Name, ItemType Type, Attributes Attributes, int CharId = 999) : base(Id, Name, Type){

        this.Attributes = Attributes;
        this.CharId = CharId;
    }
}

public class Weapon : Equipment {

    public Skill Skill;

    public Weapon(int Id, string Name, ItemType Type, Attributes Attributes, int CharId) : base(Id, Name, Type, Attributes, CharId){

        switch (CharId){
            case 0:
                Skill = new Skill(0, "Attack", SkillType.Normal, 1, 2, 0, 0, true, 100, 1);
                goto default;
            case 1:
                Skill = new Skill(0, "Attack", SkillType.Normal, 1, 2, 0, 0, true, 100, 1);
                goto default;
            case 2:
                Skill = new Skill(0, "Attack", SkillType.Normal, 4, 8, 0, 0, true, 100, 1);
                goto default;
            default:
                Skill.Description = "A simple attack based on the character's weapon. Provides 100% chance of Support action activation.";
                break;
        }
    }
}

public class Condition{

    public string Name;
    public Attributes Attributes;
    public bool Positive;
    public int Duration;
    public Sprite Portrait;

    public Condition(string Name, Attributes Attributes, bool Positive, int Duration, Sprite Portrait){

        this.Name = Name;
        this.Attributes = Attributes;
        this.Positive = Positive;
        this.Duration = Duration;
        this.Portrait = Portrait;
    }
}