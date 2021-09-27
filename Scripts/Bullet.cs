using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damage;
    private int attackerId;
    private bool isMine;

    public Rigidbody rig;


    // Initialize variables
    public void Initialize(int damage, int attackerId, bool isMine)
    {
        this.damage = damage;
        this.attackerId = attackerId;
        this.isMine = isMine;

        Destroy(gameObject, 5.0f);
    }

    // If the bullet hits something
    private void OnTriggerEnter(Collider other)
    {
        // Did we hit a player?
        // If this is the local player's bullet, damage the hit player.
        // We're using the client side for hit detection.
        if(other.CompareTag("Player") && isMine)
        {
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if(player.id != attackerId) // Don't hit self
            {
                player.photonView.RPC("TakeDamage", player.photonPlayer, attackerId, damage);
            }
        }
        Destroy(gameObject);
    }
}
