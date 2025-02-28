using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    Kill,
    Gather,
    TakeOver
}

[Serializable]
public class Quest
{
    public string questName;
    public string questDescription;
    public QuestType questType;
    public int targetAmount;  // How many enemies to kill or items to gather
    public int currentAmount; // Current progress on quest
    public string targetItemTag; // For Gather quests, using a tag rather than an item name
    public GameObject targetEnemy; // For Kill quests
    public bool isAccepted; // New flag: only update progress when quest is accepted
    public bool isCompleted;

    public void UpdateProgress()
    {
        // Only update progress if the quest is accepted.
        if (!isAccepted)
            return;

        // Update progress depending on the type of quest
        if (questType == QuestType.Kill)
        {
            // Only update progress if the enemy is dead (i.e., targetEnemy is null)
            if (targetEnemy != null)
                return;
            currentAmount++;
        }
        // For Gather quests, progress is now updated via the CollectedItem method.

        // Mark as completed if progress is met
        if (currentAmount >= targetAmount)
        {
            isCompleted = true;
        }
    }

    public void Killed()
    {
        // Only update if the quest is accepted and of type Kill.
        if (!isAccepted || questType != QuestType.Kill)
            return;

        // Increment progress since an enemy has been killed.
        currentAmount++;

        // Mark the quest as complete if the target amount has been reached.
        if (currentAmount >= targetAmount)
        {
            isCompleted = true;
        }
    }

    // New method: Update progress for Gather quests using the item's tag.
    public void CollectedItem(GameObject item)
    {
        // Only update if the quest is accepted and of type Gather.
        if (!isAccepted || questType != QuestType.Gather)
            return;

        // Check if the collected item has the correct tag.
        if (item.CompareTag(targetItemTag))
        {
            currentAmount++;
            if (currentAmount >= targetAmount)
            {
                isCompleted = true;
            }
        }
    }
}
