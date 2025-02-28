using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public List<Quest> activeQuests = new List<Quest>(); // List of currently active quests
    public int maxActiveQuests = 3; // Limit of active quests
    private QuestGiver currentQuestGiver;  // Reference to the current QuestGiver
    private Quest currentQuest;

    // Add a new quest
    public void AddQuest(Quest quest)
    {
        if (activeQuests.Count < maxActiveQuests)
        {
            activeQuests.Add(quest);
            Debug.Log("Quest added: " + quest.questName);
        }
        else
        {
            Debug.Log("You already have the maximum number of active quests.");
        }
    }

    // Remove a quest
    public void RemoveQuest(Quest quest)
    {
        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
            Debug.Log("Quest removed: " + quest.questName);
        }
    }

    // Update progress of all active quests
    void Update()
    {
        // Create a temporary list to store quests that are completed.
        List<Quest> questsToRemove = new List<Quest>();

        foreach (var quest in activeQuests)
        {
            quest.UpdateProgress();
            if (quest.isCompleted)
            {
                questsToRemove.Add(quest); // Mark quest for removal.
                Debug.Log("Quest completed: " + quest.questName);
            }
        }

        // Remove completed quests after iteration.
        foreach (var quest in questsToRemove)
        {
            RemoveQuest(quest);
        }
    }

    // Called when accepting the quest via QuestGiver
    public void AcceptQuest()
    {
        if (currentQuest != null && currentQuestGiver != null)
        {
            currentQuestGiver.AcceptQuest(); // Call the AcceptQuest method in QuestGiver
            Debug.Log("Quest accepted: " + currentQuest.questName);
        }
    }

    // Set the current quest and QuestGiver when the player interacts with an NPC
    public void SetCurrentQuestGiver(QuestGiver questGiver, Quest quest)
    {
        currentQuestGiver = questGiver;
        currentQuest = quest;
    }
}
