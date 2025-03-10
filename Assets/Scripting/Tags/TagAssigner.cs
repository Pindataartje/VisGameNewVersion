using UnityEngine;

public class TagAssigner : MonoBehaviour
{
    public string tagToAssign = "Untagged"; // Default tag, change in Inspector

    void Start()
    {
        // Check if the tag exists before assigning it
        if (IsTagValid(tagToAssign))
        {
            gameObject.tag = tagToAssign;
            Debug.Log($"Tag '{tagToAssign}' assigned to {gameObject.name}");
        }
        else
        {
            Debug.LogError($"Tag '{tagToAssign}' does not exist. Please add it in the Unity Tag Manager.");
        }
    }

    // Helper method to check if the tag exists in the Tag Manager
    private bool IsTagValid(string tag)
    {
        foreach (string existingTag in UnityEditorInternal.InternalEditorUtility.tags)
        {
            if (existingTag == tag) return true;
        }
        return false;
    }
}
