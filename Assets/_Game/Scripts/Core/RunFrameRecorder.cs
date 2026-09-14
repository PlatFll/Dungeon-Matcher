using UnityEngine;

// Opens the frame before input/actor updates; snapshots close it in LateUpdate.
[DefaultExecutionOrder(-10000)]
public sealed class RunFrameRecorder : MonoBehaviour
{
    private void Update() => GetComponent<RunContinuation>()?.BeginFrame();
}
