using UnityEngine;

public class Ranged : Troop
{
    [SerializeField]
    protected float range = 5;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        Vector2 fwd = player == 0 ? transform.right : -transform.right;
        Debug.DrawRay(transform.position, fwd * range, Color.yellow);
        var hit = Physics2D.Raycast(transform.position, fwd, range, GameManager.instance.enemyLayer[player]);
        if (hit.collider != null)
        {
            target = hit.transform.GetComponent<Health>();
            state = State.Attacking;
        }
    }
}