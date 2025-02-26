using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestGiver : MonoBehaviour
{
    public Quest quest;

    public GameObject questCanvas;
    public TMP_Text title;
    public TMP_Text description;
    public Korte_Movement player;// Verander de "korte movement" naar de naar van je player script.


    public void OpenQuestWindow()
    {
        questCanvas.SetActive(true);
        title.text = quest.title;
        description.text = quest.description;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.Confined;
    }



    public void AcceptQuest()
    {
        questCanvas.SetActive(false);
        quest.isActive = true;
        player.quest = quest;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
    }
}
