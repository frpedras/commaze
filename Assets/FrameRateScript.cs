using UnityEngine;

public class FrameRateScript : MonoBehaviour
{
    public int targetFrameRate = 120;
    public bool showFPS = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Application.targetFrameRate = targetFrameRate;
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        if (!showFPS) return;

        float framesPerSecond = Time.unscaledDeltaTime > 0f
            ? 1f / Time.unscaledDeltaTime
            : 0f;

        GUI.Label(
            new Rect(Screen.width - 110f, 8f, 102f, 24f),
            $"FPS: {framesPerSecond:0}",
            new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperRight,
                fontSize = 14
            });
    }
#endif
}
