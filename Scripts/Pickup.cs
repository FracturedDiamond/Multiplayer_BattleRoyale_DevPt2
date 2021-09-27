using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum PickupType
{
    Health,
    Ammo
}

public class Pickup : MonoBehaviour
{
    public PickupType type;
    public int value;

    private void OnTriggerEnter(Collider other)
    {
        if(!PhotonNetwork.IsMasterClient)
            return;

        if(other.CompareTag("Player"))
        {
            // Get the player
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if(type == PickupType.Health)
                player.photonView.RPC("Heal", player.photonPlayer, value);
            else if(type == PickupType.Ammo)
                player.photonView.RPC("GiveAmmo", player.photonPlayer, value);

            // Destroy the object
            PhotonNetwork.Destroy(gameObject);
            // If the above throws errors, try this line, along with the commented function, instead:
            // photonView.RPC("DestroyPickup", RpcTarget.AllBuffered);
        }
    }

    // Uncomment this as well as line in OnTriggerEnter() if Photon.Destroy() throws errors.
    //[PunRPC]
    //public void DestroyPickup()
    //{
    //    Destroy(gameObject);
    //} 
}
