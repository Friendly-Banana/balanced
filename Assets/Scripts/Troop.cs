using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Troop : Health
{
    public enum State
    {
        Moving,
        Attacking,
        Freezed
    }

    public float speed = 1;
    public float damage = 5;
    public float weight => health * 0.1f * damage;

    protected Rigidbody2D rb;
    protected Health target;
    protected Transform castle;

    [SerializeField] [SyncVar]
    // TODO hook and animate
    protected State state = State.Moving;

    [Server]
    protected override void Die()
    {
        Destroy(gameObject);
    }

    [Server]
    public override bool ChangeHealth(float damage)
    {
        var died = base.ChangeHealth(damage);
        rb.mass = weight;
        return died;
    }

    [Server]
    public void Attack()
    {
        if (state != State.Attacking) return;
        if (target == null)
        {
            state = State.Moving;
        }
        else
        {
            target.ChangeHealth(damage);
        }
    }

    public override void OnStartServer()
    {
        rb = GetComponent<Rigidbody2D>();

        var gm = GameManager.instance;
        gm.activeTroops.Add(this);
        castle = gm.castles[1-player];
        gameObject.layer = LayerMask.NameToLayer($"Player{player}");
    }

    public override void OnStartClient()
    {
        var gm = GameManager.instance;
        GetComponent<SpriteRenderer>().color *= gm.teamColor[player];
        GetComponent<SpriteRenderer>().flipX = player == 0;
        var go = Instantiate(gm.healthbar, transform);
        healthbar = go.GetComponent<FilledBar>();
        go.transform.localPosition = new Vector3(0, 0.5f, 0);
    }

    [ServerCallback]
    protected virtual void FixedUpdate()
    {
        if (state == State.Moving)
        {
            transform.position = Vector2.MoveTowards(transform.position, castle.position, speed * Time.deltaTime);
        }
    }

    public override void OnStopServer()
    {
        GameManager.instance.activeTroops.Remove(this);
    }
}