using UnityEngine;

public class Melee : Troop
{

    private void OnCollisionEnter2D(Collision2D other)
    {
        var health = other.gameObject.GetComponent<Health>();
        if (health != null && health.player != player)
        {
            state = State.Attacking;
            target = health;
        }
    }
}