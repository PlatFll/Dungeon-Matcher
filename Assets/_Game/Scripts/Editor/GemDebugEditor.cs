using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Gem))]
[CanEditMultipleObjects]
public sealed class GemDebugEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(
            "Play Mode Debug",
            EditorStyles.boldLabel
        );

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Make Selected Gem(s) Poison Bomb"))
            {
                foreach (Object selectedTarget in targets)
                {
                    Gem gem = selectedTarget as Gem;

                    if (gem == null)
                    {
                        continue;
                    }

                    Undo.RecordObject(
                        gem,
                        "Make Gem Poison Bomb"
                    );

                    gem.SetSpecialType(
                        GemSpecialType.PoisonBomb
                    );

                    EditorUtility.SetDirty(gem);
                }
            }

            if (GUILayout.Button("Clear Selected Gem Special Type"))
            {
                foreach (Object selectedTarget in targets)
                {
                    Gem gem = selectedTarget as Gem;

                    if (gem == null)
                    {
                        continue;
                    }

                    Undo.RecordObject(
                        gem,
                        "Clear Gem Special Type"
                    );

                    gem.SetSpecialType(
                        GemSpecialType.None
                    );

                    EditorUtility.SetDirty(gem);
                }
            }
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Enter Play Mode, select one or more runtime Gem objects, " +
                "then use the buttons above. Each gem keeps its current " +
                "GemType, so an Emerald gem becomes an Emerald Poison Bomb, " +
                "a Ruby gem becomes a Ruby Poison Bomb, and so on.",
                MessageType.Info
            );
        }
    }
}
