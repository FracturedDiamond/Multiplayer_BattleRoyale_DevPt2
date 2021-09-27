using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Stats")]
    public int damage;
    public int curAmmo;
    public int maxAmmo;
    public float bulletSpeed;
    public float shootRate;

    private float lastShootTime;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPos;

    private PlayerController player;


    private void Awake()
    {
        // Get required components
        player = GetComponent<PlayerController>();
    }


    public void TryShoot()
    {
        // Can we shoot?
        if(curAmmo <= 0 || Time.time - lastShootTime < shootRate)
            return;

        curAmmo--;
        lastShootTime = Time.time;

        // Update the Ammo UI
        GameUI.instance.UpdateAmmoText();


        // Spawn the bullet
        player.photonView.RPC("SpawnBullet", RpcTarget.All, bulletSpawnPos.transform.position, Camera.main.transform.forward);    
    }

    [PunRPC]
    void SpawnBullet(Vector3 pos, Vector3 dir)
    {
        // Spawn and orient bullet
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;

        // Get bullet script
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();

        // Initialize it and set the velocity
        bulletScript.Initialize(damage, player.id, player.photonView.IsMine);
        bulletScript.rig.velocity = dir * bulletSpeed;
    }

    [PunRPC]
    public void GiveAmmo(int ammoToGive)
    {
        curAmmo = Mathf.Clamp(curAmmo + ammoToGive, 0, maxAmmo);

        // Update the Ammo Text
        GameUI.instance.UpdateAmmoText();
    }
}
