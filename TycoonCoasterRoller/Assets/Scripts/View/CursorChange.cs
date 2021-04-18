using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorChange : MonoBehaviour
{
    [SerializeField] Texture2D cursorTexture;
    [SerializeField] CursorMode cursorMode = CursorMode.Auto;
    [SerializeField] Vector2 hotSpot = Vector2.zero;
    [SerializeField] GameObject image;

    private void Start()
    {
        EventManager.instance.onModeChanged += OnMouseEnter;
    }

    private void OnMouseEnter(BuildingSystem.ClickMode obj)
    {
        if (obj == BuildingSystem.ClickMode.Normal)
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
            image.SetActive(false);
        }
        else
        {
            Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
            image.SetActive(true);
        }
    }
}
