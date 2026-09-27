using System.Collections;
using UnityEngine;

// Optional presentation. No input, rewards, matching or damage are driven here.
[DisallowMultipleComponent]
public sealed class BoardStateRestoreVFX : MonoBehaviour
{
    private BoardController board;
    private void OnEnable()
    { board = GetComponent<BoardController>(); if (board != null) board.BoardStateRestored += Show; }
    private void Show() { StopAllCoroutines(); StartCoroutine(Pulse()); }
    private IEnumerator Pulse()
    {
        for (float t = 0; t < .3f; t += Time.deltaTime)
        { SetAmount(.55f * (1 - t / .3f)); yield return null; }
        SetAmount(0);
    }
    private void SetAmount(float amount)
    {
        if (board == null) return;
        for (int y = 0; y < board.Height; y++) for (int x = 0; x < board.Width; x++)
            board.GetGem(x,y)?.SetVFXFlashAmount(amount);
    }
    private void OnDisable()
    { if (board != null) board.BoardStateRestored -= Show; StopAllCoroutines(); SetAmount(0); }
}
