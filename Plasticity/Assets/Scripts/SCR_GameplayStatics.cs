using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_GameplayStatics : MonoBehaviour
{

    public IEnumerator Timer(float time, System.Action callBack)
    {
        yield return new WaitForSeconds(time);
        callBack();
    }

    public IEnumerator Timer(float time, float value, System.Action<float> callBack)
    {
        yield return new WaitForSeconds(time);
        callBack(value);
    }

    public static bool NotEmpty (string[] array)
    {
        if (array.Length == 0) return false;
        else return true;
    }

    public static bool NotEmpty(AnimationCurve[] array)
    {
        if (array.Length == 0) return false;
        else return true;
    }

    public static bool NotEmpty(GameObject[] array)
    {
        if (array.Length == 0) return false;
        else return true;
    }

    public static bool NotEmpty(KeyCode[] array)
    {
        if (array.Length == 0) return false;
        else return true;
    }

    public static bool LessThanZero(float input)
    {
        return input > 0.0f;
    }

    public static bool LessThanZero(int input)
    {
        return input > 0;
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

    public static bool GreaterThanOrEqualZero(float input)
    {
        return input >= 0.0f;
    }

    public static bool GreaterThanOrEqualZero(int input)
    {
        return input >= 0;
    }

    public static bool GreaterThanZero(float input)
    {
        return input > 0.0f;
    }

    public static bool GreaterThanZero(int input)
    {
        return input > 0;
    }
}
