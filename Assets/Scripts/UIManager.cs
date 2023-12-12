using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public RectTransform healthBar;
    public Transform gameOverScreen;

    public Sprite[] healthSprites; //5 sprites

    private PlayerController player;
    private GameController game;

    [TextArea(3,10)]
    public string restartText = "Game Over you Died\r\nPress R to Restart";
    private string speedrunText;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        game = GameObject.FindGameObjectWithTag("Game").GetComponent<GameController>();

        PlayerController.onHealthChange += UpdateHealthBar;
        PlayerController.died += DiedScreen;
        GameController.onRestart += OnRestart;
        OnFinish.onSave += OnSave;
    }

    void UpdateHealthBar(int health)
    {
        if (healthBar)
        {
            if(healthSprites.Length >= 5)
            {
                if (health >= 80)
                {
                    healthBar.GetComponent<Image>().sprite = healthSprites[0];
                }
                else if (health >= 60)
                {
                    healthBar.GetComponent<Image>().sprite = healthSprites[1];
                }
                else if (health >= 40)
                {
                    healthBar.GetComponent<Image>().sprite = healthSprites[2];
                }
                else if (health >= 20)
                {
                    healthBar.GetComponent<Image>().sprite = healthSprites[3];
                }
                else
                {
                    healthBar.GetComponent<Image>().sprite = healthSprites[4];
                }
            }
            //healthBar.sizeDelta = new Vector2(health, healthBar.sizeDelta.y);
        }
    }

    void DiedScreen()
    {
        if (player.isDead)
        {
            Debug.Log("Player Died");
            //spawn game over screen
            ChangeScreen(1f, restartText);
        }
        else
        {
            Debug.LogError("Threw death even without player dead");
        }
       
    }

    void OnRestart()
    {
        ChangeScreen(0f, restartText);
    }

    void OnSave(float currentTime)
    {
        float current = currentTime;
        StartCoroutine(wait());
    }

    void ChangeScreen(float alpha, string text)
    {
        var gameOverColor = gameOverScreen.GetComponent<Image>().color;
        var textColor = gameOverScreen.GetComponentInChildren<TMP_Text>().color;

        var tempColor = gameOverColor;
        var tempTextColor = textColor;
        tempColor.a = alpha;
        tempTextColor.a = alpha;
        gameOverScreen.GetComponent<Image>().color = tempColor;
        gameOverScreen.GetComponentInChildren<TMP_Text>().color = tempTextColor;
        gameOverScreen.GetComponentInChildren<TMP_Text>().text = text;
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(0.1f);
        speedrunText = game.GetSpeedRuns();
        ChangeScreen(1f, speedrunText);
        yield return true;
    }
}
