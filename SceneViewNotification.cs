using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
public class SceneViewNotification : Editor
{
    static List<Notification> notifications = new List<Notification>();

    public enum NotificationType
    {
        Info,
        Warning,
        Error
    }

    private class Notification
    {
        public string text;
        public DateTime dateTime;
        public NotificationType type;
    }

    public static void Add(string text, NotificationType type)
    {
        Notification n = new Notification();
        n.dateTime = DateTime.Now;
        n.text = text;
        n.type = type;

        if (notifications.Count >= MaxItems) notifications.RemoveAt(notifications.Count - 1);

        notifications.Insert(0, n);
    }

    public static void Clear()
    {
        notifications.Clear();
    }

    const float width = 300f;
    const float height = 155f;
    const int lineHeight = 20;
    private static float linePos;

    public static int MaxItems
    {
        get { return EditorPrefs.GetInt("SVN_MAX_ITEMS", 30); }
        set { EditorPrefs.SetInt("SVN_MAX_ITEMS", value); }
    }
    public static float MaxLifetime
    {
        get { return EditorPrefs.GetFloat("SVN_LIFETIME", 3f); }
        set { EditorPrefs.SetFloat("SVN_LIFETIME", value); }
    }
    public static float FadeoutDuration
    {
        get { return EditorPrefs.GetFloat("SVN_FADE_DUR", 2f); }
        set { EditorPrefs.SetFloat("SVN_FADE_DUR", value); }
    }

    private static bool mouseOver;

    private static void OnScene(SceneView sceneView)
    {
        Handles.BeginGUI();

        //Get rekt
        Rect highlightRect = new Rect(5, sceneView.camera.pixelHeight - 5 - 100, 100, 150);
        Rect lineRect = new Rect(10, sceneView.camera.pixelHeight - 25, width - 10f, lineHeight);
        Rect iconRect = new Rect(10, sceneView.camera.pixelHeight - 25, width - 10f, lineHeight);
        lineRect.x += 20f; //Make room for icon

        Vector2 mousePos = Event.current.mousePosition;
        mouseOver = highlightRect.Contains(mousePos);

        //Move everything up 20px to make room for the clear button
        lineRect.y = (mouseOver) ? lineRect.y - 20f : lineRect.y;
        iconRect.y = (mouseOver) ? iconRect.y - 20f : iconRect.y;

        //Show button on mouseover
        if (mouseOver) if (GUI.Button(new Rect(0, sceneView.camera.pixelHeight - 18f, 55f, 20f), "Clear log", EditorStyles.toolbarButton)) Clear();

        foreach (Notification n in notifications)
        {
            //Time alive running from 0 and up
            float lifetime = ((float)DateTime.Now.Subtract(n.dateTime).TotalMilliseconds);

            float alpha = 1;
            //When past maximum lifetime, start fadeTime at 0
            if (lifetime >= (MaxLifetime * 1000))
            {
                float fadeTime = lifetime - (MaxLifetime * 1000);
                alpha = 1 - (fadeTime / (FadeoutDuration * 1000));
            }

            Color originalColor = GUI.color;
            Color textColor = GUI.color;
            Texture icon = null;
            switch (n.type)
            {
                case NotificationType.Info:
                    {
                        textColor = Color.white;
                        icon = EditorGUIUtility.IconContent("console.infoicon.sml").image;
                    }
                    break;
                case NotificationType.Warning:
                    {
                        textColor = new Color(252f / 255f, 174f / 255f, 78f / 255f);
                        icon = EditorGUIUtility.IconContent("console.warnicon.sml").image;
                    }
                    break;
                case NotificationType.Error:
                    {
                        textColor = new Color(255f / 255f, 112f / 255f, 112f / 255f);
                        icon = EditorGUIUtility.IconContent("console.erroricon.sml").image;
                    }
                    break;
            }

            GUI.color = new Color(1f, 1f, 1f, mouseOver ? 1f : alpha);
            GUI.Label(iconRect, icon);

            GUI.color = new Color(textColor.r, textColor.g, textColor.b, mouseOver ? 1f : alpha);

            string hourString = ((n.dateTime.Hour <= 9) ? "0" : "") + n.dateTime.Hour;
            string minuteString = ((n.dateTime.Minute <= 9) ? "0" : "") + n.dateTime.Minute;
            string secString = ((n.dateTime.Second <= 9) ? "0" : "") + n.dateTime.Second;
            string timeString = "[" + hourString + ":" + minuteString + ":" + secString + "] ";

            GUI.Label(lineRect, new GUIContent(" " + timeString + "" + n.text + ""), LogText);

            //Decrease height pos of next line
            lineRect.y -= lineHeight;
            iconRect.y -= lineHeight;

            GUI.color = originalColor;
        }

        Handles.EndGUI();
    }

    static string[] infoMessages = new string[] { "Ding dong!", "So informative!", "<i>So important!</i>", "Something happened!" };
    static string[] warningMessages = new string[] { "Oops!", "Careful", "Warning!", "Thin ice" };
    static string[] errorMessages = new string[] { "<b>Error!</b>", "Dun goofed!", "Made a boo boo!" };

#if UNITY_2019_1_OR_NEWER
    [SettingsProvider]
    public static SettingsProvider SceneViewNotificationSettings()
    {
        var provider = new SettingsProvider("Editor/Scene Notifications", SettingsScope.Project)
        {
            label = "Scene Notifications",
            guiHandler = (searchContent) =>
            {
                MaxLifetime = EditorGUILayout.Slider("Life time", MaxLifetime, 0.1f, 10f);
                FadeoutDuration = EditorGUILayout.Slider("Fade time", FadeoutDuration, 0.1f, 10f);
                MaxItems = EditorGUILayout.IntField("Maximum lines", MaxItems);

                EditorGUILayout.Space();

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel(" ");
                    if (GUILayout.Button("Test message"))
                    {
                        NotificationType type = (NotificationType)(int)UnityEngine.Random.Range(0, 3);

                        string[] messages = new string[4];
                        if (type == NotificationType.Info) messages = infoMessages;
                        if (type == NotificationType.Warning) messages = warningMessages;
                        if (type == NotificationType.Error) messages = errorMessages;

                        string text = messages[UnityEngine.Random.Range(0, messages.Length)];

                        Add(text, type);
                    }
                }
            },

            keywords = new HashSet<string>(new[] { "Scene", "Notifications", "Log" })
        };

        return provider;
    }
#else
    [PreferenceItem("Scene Notifications")]
    public static void PreferencesGUI()
    {
        MaxLifetime = EditorGUILayout.Slider("Life time", MaxLifetime, 0.1f, 10f);
        FadeoutDuration = EditorGUILayout.Slider("Fade time", FadeoutDuration, 0.1f, 10f);
        MaxItems = EditorGUILayout.IntField("Maximum lines", MaxItems);

        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button("Test message"))
            {
                NotificationType type = (NotificationType)(int)UnityEngine.Random.Range(0, 3);

                string[] messages = new string[4];
                if (type == NotificationType.Info) messages = infoMessages;
                if (type == NotificationType.Warning) messages = warningMessages;
                if (type == NotificationType.Error) messages = errorMessages;

                string text = messages[UnityEngine.Random.Range(0, messages.Length)];

                Add(text, type);
            }
        }
    }
#endif

    [InitializeOnLoad]
    sealed class InitializeOnLoad : Editor
    {
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            if (EditorApplication.isPlaying) return;

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnScene;
#else
            SceneView.onSceneGUIDelegate += OnScene;
#endif
        }
    }

    private static GUIStyle _LogText;
    public static GUIStyle LogText
    {
        get
        {
            if (_LogText == null)
            {
                _LogText = new GUIStyle(UnityEngine.GUI.skin.label)
                {
                    richText = true,
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = false,
                    fontSize = 12,
                    stretchWidth = true,
                    font = (Font)EditorGUIUtility.LoadRequired("Fonts/Lucida Grande.ttf"),
                    //fontStyle = FontStyle.Bold,
                    padding = new RectOffset()
                    {
                        left = 0,
                        right = 0,
                        top = 0,
                        bottom = 0
                    },
                    clipping = TextClipping.Overflow
                };
            }

            return _LogText;
        }
    }
}
#endif