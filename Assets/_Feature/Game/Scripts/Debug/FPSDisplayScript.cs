using UnityEngine;
using System.Collections;

public class FPSDisplayScript : MonoBehaviour
{
    float timeA;
    public int fps;
    public int lastFPS;
    public GUIStyle textStyle;
    // Use this for initialization
    void Start()
    {
        textStyle.fontSize = 50;
        textStyle.normal.textColor = Color.green;

        timeA = Time.timeSinceLevelLoad;
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Time.timeSinceLevelLoad+" "+timeA);
        if (Time.timeSinceLevelLoad - timeA <= 1)
        {
            fps++;
        }
        else
        {
            lastFPS = fps + 1;
            timeA = Time.timeSinceLevelLoad;
            fps = 0;
        }
    }
    void OnGUI()
    {
        string text = string.Format("{0} fps\n{1:0.00} ms", lastFPS, 1000.0f / lastFPS);
        GUI.Label(new Rect(5, 5, 100, 200), text, textStyle);
    }
}
