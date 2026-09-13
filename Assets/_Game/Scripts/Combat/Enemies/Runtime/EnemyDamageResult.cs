/// <summary>
/// Snapshot of one actor-owned damage application after one-hop interception
/// and mitigation. HP and shield loss stay separate; default means no damage.
/// The recipient reference may later be destroyed, but the recorded amounts
/// must not be recomputed from mutable actor health after callbacks.
/// </summary>
public readonly struct EnemyDamageResult
{
    public EnemyActor Recipient { get; }
    public int HealthDamage { get; }
    public int ShieldDamage { get; }
    public bool Applied => HealthDamage > 0 || ShieldDamage > 0;

    internal EnemyDamageResult(
        EnemyActor recipient,
        int healthDamage,
        int shieldDamage)
    {
        Recipient = recipient;
        HealthDamage = healthDamage;
        ShieldDamage = shieldDamage;
    }
}
