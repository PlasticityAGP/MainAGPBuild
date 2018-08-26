using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is based on a script written by Hayden Scott-Baron located here  http://wiki.unity3d.com/index.php/FadeObjectInOut

public class SCR_FadeInOut : MonoBehaviour {

    [SerializeField]
    private GameObject Target;
    [SerializeField]
    private float LengthOfFade;

    private float MaximumAlphaValue()
    {
        float MaxAlpha = 0.0f;
        Renderer[] Renderers = Target.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < Renderers.Length; ++i)
        {
            for (int j = 0; j < Renderers[i].materials.Length; ++j)
            {
                MaxAlpha = Mathf.Max(MaxAlpha, Renderers[i].materials[j].color.a);
            }
        }
        return MaxAlpha;
    }

    private float MinimumAlphaValue()
    {
        float MinAlpha = 1.0f;
        Renderer[] Renderers = Target.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < Renderers.Length; ++i)
        {
            for (int j = 0; j < Renderers[i].materials.Length; ++j)
            {
                MinAlpha = Mathf.Min(MinAlpha, Renderers[i].materials[j].color.a);
            }
        }
        return MinAlpha;
    }

    IEnumerator Fade(float FadingTime)
    {
        bool AmFadingIn = (FadingTime >= 0.0f);
        Renderer[] Renderers = Target.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < Renderers.Length; ++i)
        {
            Renderers[i].enabled = true;
        }

        float Alpha;
        if (AmFadingIn) Alpha = MinimumAlphaValue();
        else Alpha = MaximumAlphaValue();
        while ((Alpha >= 0.0f && !AmFadingIn) || (Alpha <= 1.0f && AmFadingIn))
        {  
            Alpha += Time.deltaTime * (1.0f / FadingTime);
            for (int i = 0; i < Renderers.Length; ++i)
            {
                for (int j = 0; j < Renderers[i].materials.Length; ++j)
                {
                    Color NewColor = Renderers[i].materials[j].color;
                    if(!AmFadingIn) NewColor.a = Mathf.Min(NewColor.a, Alpha);
                    else NewColor.a = Mathf.Max(NewColor.a, Alpha);
                    NewColor.a = Mathf.Clamp(NewColor.a, 0.0f, 1.0f);
                    Renderers[i].materials[j].SetColor("_Color", NewColor);
                }
            }
            yield return null;
        }
        if (!AmFadingIn)
        {
            for (int i = 0; i < Renderers.Length; ++i)
            {
                Renderers[i].enabled = false;
            }
        }
    }

    private void FadeIn(float Time)
    {
        StopAllCoroutines();
        StartCoroutine("Fade", Time);
    }

    private void FadeOut(float Time)
    {
        StopAllCoroutines();
        StartCoroutine("Fade", -Time);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Character")
        {
            FadeOut(LengthOfFade);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Character")
        {
            FadeIn(LengthOfFade);
        }
    }
}
