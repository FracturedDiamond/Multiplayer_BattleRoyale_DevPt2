using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class PlayerController : MonoBehaviourPun
{
    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;

    [Header("Components")]
    public Rigidbody rig;

    [Header("Photon")]
    public int id;
    public Player photonPlayer;

    public int curHp;
    public int maxHp;
    public int kills;
    public bool dead;
    private bool flashingDamage;
    public MeshRenderer mr;

    private int curAttackerId;
    public PlayerWeapon weapon;


    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;
        GameManager.instance.players[id - 1] = this;

        // is this not our local player?
        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            rig.isKinematic = true;
        }
        else
            GameUI.instance.Initialize(this);
    }


    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine || dead)
            return;

        Move();

        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();
        if (Input.GetMouseButtonDown(0))
            weapon.TryShoot();
    }


    void Move()
    {
        // Get the input axis
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Calculate a direction relative to where we're facing
        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rig.velocity.y;

        // Set that as our velocity
        rig.velocity = dir;
    }


    private void TryJump()
    {
        // Create a ray facing down
        Ray ray = new Ray(transform.position, Vector3.down);

        // Shoot the raycast
        if (Physics.Raycast(ray, 1.5f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }


    [PunRPC]
    public void TakeDamage(int attackerId, int damage)
    {
        if (dead)
            return;

        curHp -= damage;
        curAttackerId = attackerId;

        // flash the player red
        photonView.RPC("DamageFlash", RpcTarget.Others);

        // update the health bar UI
        GameUI.instance.UpdateHealthBar();

        // die if no health left
        if (curHp <= 0)
            photonView.RPC("Die", RpcTarget.All);
    }


    [PunRPC]
    void DamageFlash()
    {
        if (flashingDamage)
            return;

        StartCoroutine(DamageFlashCoRoutine());

        IEnumerator DamageFlashCoRoutine()
        {
            flashingDamage = true;
            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;

            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDamage = false;
        }
    }


    [PunRPC]
    void Die()
    {
        curHp = 0;
        dead = true;

        GameManager.instance.alivePlayers--;
        GameUI.instance.UpdatePlayerInfoText();

        // Host will check win condition
        if (PhotonNetwork.IsMasterClient)
            GameManager.instance.CheckWinCondition();

        // Is this our local player?
        if(photonView.IsMine)
        {
            // Player attacker or force field?
            if(curAttackerId != 0)
                GameManager.instance.GetPlayer(curAttackerId).photonView.RPC("AddKill", RpcTarget.All);

            // Set the cam to spectator view
            GetComponentInChildren<CameraController>().SetAsSpectator();

            // Disable the physics and hide the player
            rig.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);
        }
    }


    [PunRPC]
    public void AddKill()
    {
        kills++;
        GameUI.instance.UpdatePlayerInfoText();
    }


    [PunRPC]
    public void Heal(int amountToHeal)
    {
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);

        // Update the health bar UI
        GameUI.instance.UpdateHealthBar();
    }
}