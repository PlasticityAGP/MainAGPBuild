using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A reference to the parent game object of the main character.
/// </summary>
public class SCR_StaticCharacterReference : MonoBehaviour {

    // Singleton reference
    private static SCR_StaticCharacterReference instance;

    public static GameObject GetCharacterReference()
    {
        return instance.gameObject;
    }

    private void Start()
    {
        if(instance)
        {
            Debug.LogError("Tried to have 2 Main characters in the scene. Lol it's not a multiplayer game yet. Unless it is... in which case this class needs to be changed to reference just the client player.");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

}
