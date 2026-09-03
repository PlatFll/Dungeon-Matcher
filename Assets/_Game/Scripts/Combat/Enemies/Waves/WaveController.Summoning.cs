using System.Collections.Generic;
using UnityEngine;

public sealed partial class WaveController
{
    public bool HasFreeEnemySlot
    {
        get
        {
            if (!IsWaveActive ||
                enemySlots == null ||
                activeEnemies.Count >=
                    enemySlots.Length)
            {
                return false;
            }

            return FindFreeEnemySlot() != null;
        }
    }

    public bool TrySummonEnemy(
        EnemyDefinition definition,
        out EnemyActor summonedEnemy)
    {
        summonedEnemy = null;

        if (!Application.isPlaying ||
            !IsWaveActive ||
            definition == null ||
            difficultyProfile == null ||
            playerActor == null ||
            boardController == null)
        {
            return false;
        }

        EnemySlotUI freeSlot =
            FindFreeEnemySlot();

        if (freeSlot == null)
        {
            return false;
        }

        GemType assignedGemType =
            ChooseSummonGemType();

        EnemyActor enemy =
            CreateEnemy(
                definition,
                freeSlot,
                assignedGemType
            );

        if (enemy == null)
        {
            return false;
        }

        activeEnemies.Add(enemy);
        summonedEnemy = enemy;

        EnemySpawned?.Invoke(enemy);

        Debug.Log(
            $"Summoned {definition.DisplayName} into {freeSlot.name}. " +
            $"The summon is now an independent member of wave {currentWave}.",
            this
        );

        return true;
    }

    private EnemySlotUI FindFreeEnemySlot()
    {
        if (enemySlots == null)
        {
            return null;
        }

        for (int index = 0;
             index < enemySlots.Length;
             index++)
        {
            EnemySlotUI slot =
                enemySlots[index];

            if (slot != null &&
                !slot.IsOccupied &&
                slot.CurrentEnemy == null)
            {
                return slot;
            }
        }

        return null;
    }

    private GemType ChooseSummonGemType()
    {
        HashSet<GemType> usedGemTypes =
            new HashSet<GemType>();

        for (int index = 0;
             index < activeEnemies.Count;
             index++)
        {
            EnemyActor enemy =
                activeEnemies[index];

            if (enemy == null ||
                !enemy.IsInitialized ||
                enemy.IsDefeated)
            {
                continue;
            }

            usedGemTypes.Add(
                enemy.AssignedGemType
            );
        }

        List<GemType> candidates =
            CreateShuffledGemTypeList();

        for (int index = 0;
             index < candidates.Count;
             index++)
        {
            if (!usedGemTypes.Contains(
                    candidates[index]))
            {
                return candidates[index];
            }
        }

        return candidates.Count > 0
            ? candidates[0]
            : default;
    }
}
