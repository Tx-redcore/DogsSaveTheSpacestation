using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public AudioClip pickUpSound;
    public PickUpType pickUpType;

    public int healAmount = 10;
    public Sprite healSprite;
    public int speedBoost = 2;
    public Sprite speedSprite;
    public int jumpBoost = 2;
    public Sprite jumpSprite;

    public delegate void OnPickUp(int amount, PickUpType type);
    public static event OnPickUp onPickUp;

    public delegate void OnPlaySound(AudioClip clip);
    public static event OnPlaySound onPlaySound;

    private SpriteRenderer renderer;

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        if (renderer)
        {
            if (pickUpType != PickUpType.NotSet) {
                switch (pickUpType)
                {
                    case PickUpType.Heal:
                        if (healSprite) { renderer.sprite = healSprite; }
                        break;
                    case PickUpType.Speed:
                        if (speedSprite) { renderer.sprite = speedSprite; }
                        break;
                    case PickUpType.Jump:
                        if (jumpSprite) { renderer.sprite = jumpSprite; }
                        break;
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        onPlaySound?.Invoke(pickUpSound);
        switch (pickUpType)
        {
            case PickUpType.Heal:
                onPickUp?.Invoke(healAmount, PickUpType.Heal);
                Destroy(this.gameObject);
                break;
            case PickUpType.Speed:
                onPickUp?.Invoke(speedBoost, PickUpType.Speed);
                Destroy(this.gameObject);
                break;
            case PickUpType.Jump:
                onPickUp?.Invoke(jumpBoost, PickUpType.Jump);
                Destroy(this.gameObject);
                break;
        }       
    }
}

public enum PickUpType
{
    Heal,
    Speed,
    Jump,
    NotSet,
}