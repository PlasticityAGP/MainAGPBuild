using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleCelShading : MonoBehaviour
{

    public KeyCode toggleKey = KeyCode.T;

    private void Update()
    {
        if(Input.GetKeyDown(toggleKey))
        {
            SetCelShading(!GetCelShading());
        }
    }

    private void SetCelShading(bool isOn)
    {
        if (isOn)
            Camera.main.renderingPath = RenderingPath.DeferredShading;
        else
            Camera.main.renderingPath = RenderingPath.Forward;
    }

    private bool GetCelShading()
    {
        return Camera.main.renderingPath == RenderingPath.DeferredShading;
    }
}
