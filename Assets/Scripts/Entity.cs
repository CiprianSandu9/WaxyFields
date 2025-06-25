using UnityEngine;

public class Entity : MonoBehaviour
{
    public int Health;
    public int MaxHealth = 100;
    public float Stamina;
    public float MaxStamina = 100;
    public bool IsDead => Health <= 0;

    public void Awake()
    {
        Health = MaxHealth;
        Stamina = MaxStamina;
    }

    public virtual void Damage(int damage)
    {
        //if (IsDead) return;
        //Health -= damage;
        //if (IsDead)
        //{
        //    Health = 0;
        //    OnDeath();
        //}
    }

    public virtual void Heal(int amount)
    {
        Health += amount;
        if (Health > MaxHealth) Health = MaxHealth;
    }

    public virtual void OnDeath()
    {

    }
}
