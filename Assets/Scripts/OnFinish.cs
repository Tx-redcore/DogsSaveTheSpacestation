using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFinish : MonoBehaviour
{
    public delegate void OnSave(float time);
    public static event OnSave onSave;

    private PlayerController player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.tag == "Player")
        {
            onSave?.Invoke(player.speedRunSeconds);
        }
    }
}
