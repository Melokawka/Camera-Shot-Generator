using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GlobalVars))]
public class GlobalVarsEditor : Editor
{
    private bool showFoldout = false;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GlobalVars gv = (GlobalVars)target;

        // Pomiń oryginalne listy, które renderujemy ręcznie
        DrawDefaultInspectorExcept("environmentsList", "environments", "uncombinableIdleAnimations", "uncombinableIdleAnimationsList");

        EditorGUILayout.Space();
        showFoldout = EditorGUILayout.Foldout(showFoldout, "Uncombinable Idle Animations", true);
        if (showFoldout)
        {
            for (int i = 0; i < gv.uncombinableIdleAnimationsList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                gv.uncombinableIdleAnimationsList[i].include = EditorGUILayout.Toggle(gv.uncombinableIdleAnimationsList[i].include, GUILayout.Width(20));
                gv.uncombinableIdleAnimationsList[i].clip = (AnimationClip)EditorGUILayout.ObjectField(gv.uncombinableIdleAnimationsList[i].clip, typeof(AnimationClip), false);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    gv.uncombinableIdleAnimationsList.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add"))
            {
                gv.uncombinableIdleAnimationsList.Add(new AnimSelection());
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Update (required after every change)"))
            {
                gv.uncombinableIdleAnimations.Clear();
                foreach (var entry in gv.uncombinableIdleAnimationsList)
                {
                    if (entry.include && entry.clip != null)
                        gv.uncombinableIdleAnimations.Add(entry.clip);
                }
            }
        }

        EditorGUILayout.Space();

        GUILayout.Label("Environments", EditorStyles.boldLabel);

        for (int i = 0; i < gv.environmentsList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            gv.environmentsList[i].isEnabled = EditorGUILayout.Toggle(gv.environmentsList[i].isEnabled, GUILayout.Width(20));
            gv.environmentsList[i].environment = (GameObject)EditorGUILayout.ObjectField(gv.environmentsList[i].environment, typeof(GameObject), true);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                gv.environmentsList.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add"))
        {
            gv.environmentsList.Add(new EnvironmentEntry());
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(gv);
    }

    void DrawDefaultInspectorExcept(params string[] exclude)
    {
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        while (prop.NextVisible(enterChildren))
        {
            if (System.Array.Exists(exclude, x => x == prop.name)) continue;
            EditorGUILayout.PropertyField(prop, true);
            enterChildren = false;
        }
    }
}
