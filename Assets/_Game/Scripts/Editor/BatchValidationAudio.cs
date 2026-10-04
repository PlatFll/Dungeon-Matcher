using UnityEditor;
using UnityEngine;

/// <summary>Silence automated Editor runs without changing player audio preferences.</summary>
[InitializeOnLoad]
internal static class BatchValidationAudio
{
    static BatchValidationAudio()
    {
        if (Application.isBatchMode) EditorUtility.audioMasterMute = true;
    }
}
