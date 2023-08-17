using UnityEngine;

public class AnimationManager : MonoBehaviour{

    //LIST OF COMMON ANIMATIONS        
    Sprite[] idleFront = new Sprite[8];
    Sprite[] idleBack = new Sprite[8];
    Sprite[] jumpFront = new Sprite[1];
    Sprite[] jumpBack = new Sprite[1];
    Sprite[] attackFront = new Sprite[3];
    Sprite[] attackBack = new Sprite[3];
    Sprite[] blockFront = new Sprite[3];
    Sprite[] blockBack = new Sprite[3];
    Sprite[] chargeFront = new Sprite[1];
    Sprite[] chargeBack = new Sprite[1];
    Sprite[] dead = new Sprite[1];

    //VARIABLES
    public SpriteRenderer sprite;
    public BattleCharacter character;
    Sprite[] currentAnimationFrames;
    int animIndex;
    
    const float animTimeInterval = 0.15f;
    float lastFrameTime;
    float currentTimeInterval;
    float overflownInterval;

    bool repeat = true;
    
    void Update(){

        currentTimeInterval = Time.time - lastFrameTime;
        if (currentTimeInterval >= animTimeInterval - overflownInterval){
            
            //JUMPS TO NEXT FRAME
            if (animIndex < currentAnimationFrames.Length - 1)
                animIndex++;
            else{
                if (repeat)
                    animIndex = 0;
                else{
                    if (character.HP > 0){
                        repeat = true;
                        Play((character.chargingSkill == null) ? AnimationState.Idle : AnimationState.Charge);
                    }
                    else if (character.tag == "Ally")
                        Play(AnimationState.Dead);
                }
            }

            //PLAYS ANIMATION
            overflownInterval = currentTimeInterval - animTimeInterval;
            lastFrameTime = Time.time;
            sprite.sprite = currentAnimationFrames[animIndex];
        }
    }

    public void Play(AnimationState animation) { //FICAR ESPERTO COM O COVER, QUE TA ENCAVALANDO A ANIMAÇÃO!!!

        bool newIsFront = true;

        //DEFINES FLIP/FRONT BASED ON CAMERA AND CHAR DIRECTION
        switch (Camera.main.transform.rotation.eulerAngles.y) {
            case 45:
                sprite.flipX = (character.direction == 0 || character.direction == 3) ? true : false;
                newIsFront = (character.direction == 1 || character.direction == 3) ? true : false;
                break;
            case 135:
                sprite.flipX = (character.direction == 1 || character.direction == 3) ? true : false;
                newIsFront = (character.direction == 1 || character.direction == 2) ? true : false;
                break;
            case 225:
                sprite.flipX = (character.direction == 1 || character.direction == 2) ? true : false;
                newIsFront = (character.direction == 0 || character.direction == 2) ? true : false;
                break;
            case 315:
                sprite.flipX = (character.direction == 0 || character.direction == 2) ? true : false;
                newIsFront = (character.direction == 0 || character.direction == 3) ? true : false;
                break;
        }

        //SWITCHES THE ANIMATION FRAMES
        switch (animation){
            case AnimationState.Idle: //da pra melhorar a animação da andança se parar de resetar o frame quando só muda de direção!!!
                currentAnimationFrames = newIsFront ? idleFront : idleBack;
                break;
            case AnimationState.Jump:
                currentAnimationFrames = newIsFront ? jumpFront : jumpBack;
                break;
            case AnimationState.Attack:
                currentAnimationFrames = newIsFront ? attackFront : attackBack;
                repeat = false;
                break;
            case AnimationState.Block:
                currentAnimationFrames = newIsFront ? blockFront : blockBack;
                repeat = false;
                break;
            case AnimationState.Charge:
                currentAnimationFrames = newIsFront ? chargeFront : chargeBack;
                break;
            case AnimationState.Dead:
                currentAnimationFrames = dead;
                break;
        }
        animIndex = 0;
        overflownInterval = 0;
        lastFrameTime = Time.time;
        sprite.sprite = currentAnimationFrames[animIndex];
    }

    public void InstantiateAnimations(Texture2D texture){

        Sprite[] sprites = Resources.LoadAll<Sprite>(texture.name);

        idleFront[0] = sprites[11];
        idleFront[1] = sprites[12];
        idleFront[2] = sprites[13];
        idleFront[3] = sprites[12];
        idleFront[4] = sprites[11];
        idleFront[5] = sprites[14];
        idleFront[6] = sprites[15];
        idleFront[7] = sprites[14];
        idleBack[0] = sprites[6];
        idleBack[1] = sprites[7];
        idleBack[2] = sprites[8];
        idleBack[3] = sprites[7];
        idleBack[4] = sprites[6];
        idleBack[5] = sprites[9];
        idleBack[6] = sprites[10];
        idleBack[7] = sprites[9];
        jumpFront[0] = sprites[17];
        jumpBack[0] = sprites[16];
        attackFront[0] = sprites[3];
        attackFront[1] = sprites[4];
        attackFront[2] = sprites[5];
        attackBack[0] = sprites[0];
        attackBack[1] = sprites[1];
        attackBack[2] = sprites[2];
        blockFront[0] = sprites[19];
        blockFront[1] = sprites[19];
        blockFront[2] = sprites[19];
        blockBack[0] = sprites[18];
        blockBack[1] = sprites[18];
        blockBack[2] = sprites[18];
        chargeFront[0] = sprites[21];
        chargeBack[0] = sprites[20];
        dead[0] = sprites[3];
    }
}