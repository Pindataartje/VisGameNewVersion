using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
[System.Serializable]
public class Quest 
{
    public bool isActive;

    public string title;
    public string description;


    public QuestGoal goal;


    public void Complete()
    {
        isActive = false;
        Debug.Log("Quest Completed.");
        goal.currentAmount = 0; // Reset progress
    }
}
