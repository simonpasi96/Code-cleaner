using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CodeCleaner : EditorWindow
{
    string input;
    Vector2 scroll;
    List<CleanerModule> cleaners = new List<CleanerModule>()
    {
        new BracketsCleaner(),
        new PrivatesCleaner(),
        new RegionsRemover(),
        new NewlinesCleaner()
    };
    int activeCleaner = -1;


    void OnGUI()
    {
        // Text.
        scroll = EditorGUILayout.BeginScrollView(scroll);
        GUIStyle areaStyle = new GUIStyle(GUI.skin.textArea)
        {
            richText = true
        };
        if (activeCleaner >= 0)
        {
            areaStyle.normal.background = new Texture2D(0, 0);
            GUILayout.TextArea(cleaners[activeCleaner].GetPreview(), areaStyle, GUILayout.ExpandHeight(true));
        }
        else
            input = GUILayout.TextArea(input, areaStyle, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        // Cleaner interface.
        if (activeCleaner >= 0)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUIStyle moduleTitle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Italic
            };
            GUILayout.Label(cleaners[activeCleaner].GetType().ToTitle(), moduleTitle);
            if (GUILayout.Button("X", GUILayout.MaxWidth(40)) || Event.current.keyCode == KeyCode.Escape)
                activeCleaner = -1;
            GUILayout.EndHorizontal();
            if (activeCleaner >= 0)
                cleaners[activeCleaner].DrawUI();
            GUILayout.EndVertical();
        }

        EditorGUILayout.BeginHorizontal();
        // File drop area.
        Rect openFileArea = DrawDropArea<TextAsset>("  Open file", (asset) => { input = asset.text; });
        GUI.DrawTexture(new Rect(openFileArea.x + openFileArea.width * .5f - 40, openFileArea.position.y + 8, 15, 15), EditorGUIUtility.IconContent("Folder Icon").image);

        // Buttons.
        if (GUILayout.Button("Copy", GUILayout.Height(27)))
            GUIUtility.systemCopyBuffer = input;
        if (GUILayout.Button("Clear", GUILayout.Height(27)))
        {
            Undo.RegisterCompleteObjectUndo(this, "Clear Code cleaner input");
            input = "";
        }
        EditorGUILayout.EndHorizontal();


        // Context menu.
        if (Event.current.button == 1)
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < cleaners.Count; i++)
            {
                int index = i;
                menu.AddItem(new GUIContent(cleaners[i].GetType().ToTitle()), false, () =>
                {
                    activeCleaner = index;
                    cleaners[activeCleaner].Find(input);
                    cleaners[activeCleaner].Finalize = delegate (string cleaningResult)
                    {
                        input = cleaningResult;
                        activeCleaner = -1;
                    };
                });
            }

            menu.AddItem(new GUIContent("Run all cleaners"), false, () =>
            {

            });

            menu.AddSeparator("");
            if (activeCleaner >= 0)
                menu.AddDisabledItem(new GUIContent("Reset example text"));
            else
                menu.AddItem(new GUIContent("Reset example text"), false, () =>
                {
                    input = @"
      
      public void Speak()
      {
         float currentTime = Time.realtimeSinceStartup;


        if (Util.Helper.isEditorMode)
        {
#if UNITY_EDITOR
            Speaker.SpeakNative(Text, Voices.Voice, Rate, Pitch, Volume);
            if (GenerateAudioFile)
            {
                Speaker.Generate(Text, path, Voices.Voice, Rate, Pitch, Volume);
            }
#endif
        }
        else
        {
            uid = Mode == Model.Enum.SpeakMode.Speak
               ? Speaker.Speak(Text, Source, Voices.Voice, true, Rate, Pitch, Volume, path)
               : Speaker.SpeakNative(Text, Voices.Voice, Rate, Pitch, Volume);
        }
    }
         else
         {
            Debug.LogWarning(Speak' called too fast - please slow down, this);
         }
      }

";
                });
            menu.ShowAsContext();

            Event.current.Use();
        }
    }

    [MenuItem("Window/Tools/Code cleaner")]
    public static void Open()
    {
        GetWindow(typeof(CodeCleaner)).titleContent = new GUIContent("Code cleaner");
    }

    Rect DrawDropArea<T>(string boxText, System.Action<T> ObjectAction)
    {
        Event evt = Event.current;
        Rect boxRect = GUILayoutUtility.GetRect(50, 30, GUILayout.ExpandWidth(true));
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter
        };
        if (EditorGUIUtility.isProSkin)
            boxStyle.normal.textColor = Color.white;
        GUI.Box(boxRect, boxText, boxStyle);

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!boxRect.Contains(evt.mousePosition))
                    break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (UnityEngine.Object droppedObject in DragAndDrop.objectReferences)
                    {

                        if (droppedObject is T target)
                            ObjectAction(target);
                    }
                }
                break;
        }

        return boxRect;
    }
}
