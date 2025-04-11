using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public static class Utilities
{
    

    
    public static IEnumerator WaitAfterCoroutine(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action?.Invoke();
    }

    public static IEnumerator WaitAfterEndOfFrameCoroutine(Action action)
    {
        yield return new WaitForEndOfFrame();
        action?.Invoke();
    }


    public static void WaitAfter(float waitTime, System.Action action)
    {
        CoroutineManager.Instance.StartStaticCoroutine(WaitAfterCoroutine(waitTime, action));
    }

    private static IEnumerator WaitCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    public static void ButtonInteractableAfter(this Button button)
    {
        button.StartCoroutine(PreventMultipleClick(button));
    }

    private static IEnumerator PreventMultipleClick(this Button button)
    {
        button.interactable = false;
        yield return new WaitForSeconds(0.5f);
        button.interactable = true;
    }

    public static bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }


    public static void SetNativeSize(this Image image)
    {
        image.SetNativeSize();
    }


    public static void SetImageSize(this SpriteRenderer image, float width, float height)
    {
        image.size = new Vector2(width, height);
    }


    // public static Vector2 ImagePixelToWorld(this SpriteRenderer image, Vector2 pixel)
    // {
    //     Bounds bounds = image.bounds;
    //
    //     int imgWidth = image.sprite.texture.width;
    //     int imgHeight = image.sprite.texture.height;
    //
    //     // Normalize coordinates (bottom-left origin in Unity)
    //     float normalizedX = pixel.x / imgWidth;
    //     float normalizedY = pixel.y / imgHeight; // No need to invert Y
    //
    //     float worldX = Mathf.Lerp(bounds.min.x, bounds.max.x, normalizedX);
    //     float worldY = Mathf.Lerp(bounds.min.y, bounds.max.y, normalizedY);
    //     Debug.Log("worldX: " + worldX + " worldY: " + worldY + " pixel: " + pixel);
    //     return new Vector2(worldX, -worldY);
    // }
    //
    //
    // public static Vector2 WorldPositionToImagePixel(this SpriteRenderer image, Vector2 worldPosition)
    // {
    //     Bounds bounds = image.bounds;
    //
    //     int imgWidth = image.sprite.texture.width;
    //     int imgHeight = image.sprite.texture.height;
    //
    //     float normalizedX = Mathf.InverseLerp(bounds.min.x, bounds.max.x, worldPosition.x);
    //     float normalizedY = Mathf.InverseLerp(bounds.min.y, bounds.max.y, worldPosition.y);
    //
    //     float pixelX = Mathf.Lerp(0, imgWidth, normalizedX);
    //     float pixelY = Mathf.Lerp(0, imgHeight, normalizedY);
    //
    //     return new Vector2(pixelX, pixelY);
    // }
    
    public static Vector2Int ImagePixelToWorld(this SpriteRenderer image, Vector2 pixel)
    {
        Bounds bounds = image.bounds;

        int imgWidth = image.sprite.texture.width;
        int imgHeight = image.sprite.texture.height;

        // Normalize coordinates (bottom-left origin in Unity)
        float normalizedX = pixel.x / imgWidth;
        float normalizedY = pixel.y / imgHeight; // No need to invert Y

        float worldX = Mathf.Lerp(bounds.min.x, bounds.max.x, normalizedX);
        float worldY = Mathf.Lerp(bounds.min.y, bounds.max.y, normalizedY);
        return Vector2Int.RoundToInt(new Vector2(worldX, worldY));
    }

    public static Vector2Int WorldPositionToImagePixel(this SpriteRenderer image, Vector2 worldPosition)
    {
        Bounds bounds = image.bounds;

        int imgWidth = image.sprite.texture.width;
        int imgHeight = image.sprite.texture.height;

        float normalizedX = Mathf.InverseLerp(bounds.min.x, bounds.max.x, worldPosition.x);
        float normalizedY = Mathf.InverseLerp(bounds.min.y, bounds.max.y, worldPosition.y);

        float pixelX = Mathf.Lerp(0, imgWidth, normalizedX);
        float pixelY = Mathf.Lerp(0, imgHeight, normalizedY);

        return new Vector2Int(Mathf.RoundToInt(pixelX), Mathf.RoundToInt(pixelY));
    }
}


public class CoroutineManager : MonoBehaviour
{
    private static CoroutineManager _instance;

    public static CoroutineManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find an existing instance
                _instance = FindFirstObjectByType<CoroutineManager>();

                // If still null, create a new one
                if (_instance == null)
                {
                    GameObject go = new GameObject("CoroutineManager");
                    _instance = go.AddComponent<CoroutineManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // Ensure only one instance exists
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void StartStaticCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}