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
    

    public void OpenQuestWindow()
    {
        questCanvas.SetActive(true);
        title.text = quest.title;
        description.text = quest.description;
    }
}
