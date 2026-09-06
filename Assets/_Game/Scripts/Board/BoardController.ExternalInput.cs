using System;
using System.Collections.Generic;

public partial class BoardController
{
    private sealed class ExternalInputBlockToken : IDisposable
    {
        private BoardController board;

        public ExternalInputBlockToken(BoardController owner)
        {
            board = owner;
        }

        public void Dispose()
        {
            if (board == null)
            {
                return;
            }

            board.ReleaseExternalInputBlock(this);
            board = null;
        }
    }

    private readonly HashSet<ExternalInputBlockToken>
        externalInputBlocks = new HashSet<ExternalInputBlockToken>();

    public bool IsExternalInputBlocked => externalInputBlocks.Count > 0;

    public IDisposable AcquireExternalInputBlock()
    {
        ExternalInputBlockToken token =
            new ExternalInputBlockToken(this);

        externalInputBlocks.Add(token);
        pointerStartGem = null;
        ClearSelection();

        return token;
    }

    private void ReleaseExternalInputBlock(ExternalInputBlockToken token)
    {
        if (token != null)
        {
            externalInputBlocks.Remove(token);
        }
    }
}
