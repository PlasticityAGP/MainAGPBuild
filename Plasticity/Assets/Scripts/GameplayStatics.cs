using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameplayStatics {

    public static bool NotEmpty(string[] array)
    {
        if (array.Length == 0) return false;
        else return true;
    }

    public static bool LessThanZero(float input)
    {
        return input > 0.0f;
    }

    public static bool IsNull(GameObject thing)
    {
        try
        {
            return thing.scene.IsValid();
        }
        catch
        {
            return false;
        }
    }
}
