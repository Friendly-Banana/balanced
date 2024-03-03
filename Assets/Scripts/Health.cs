using UnityEngine;
using Mirror;

public abstract class Health : NetworkBehaviour
{
    [SyncVar] public int player;

    [SerializeField] [SyncVar(hook = nameof(UpdateHealthbar))]
    protected float health;

    [SerializeField] protected float maxHealth = 100;
    [SerializeField] protected FilledBar healthbar;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        health = maxHealth;
    }
#endif

    protected abstract void Die();

    [Server]
    public virtual bool ChangeHealth(float damage)
    {
        var nh = health - damage;
        if (health <= 0)
        {
            health = 0;
            Die();
            return true;
        }

        health = nh;
        return false;
    }

    [Client]
    protected virtual void UpdateHealthbar(float _, float newHealth)
    {
        healthbar.SetValue(newHealth / maxHealth);
    }
}