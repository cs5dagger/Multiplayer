using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public Transform ViewPoint;
    public float MouseSensitivity = 1f;
    private float VerticalRotStore;
    private Vector2 MouseInput;
    public bool InvertLook;
        
    public float MoveSpeed = 5f, RunSpeed = 8f;
    private float ActiveMoveSpeed; 
    private Vector3 MoveDirection, /*this variable to move in camera direction*/Movement;

    public CharacterController characterController;
    private Camera camera;
    public float JumpForce = 12f, GravityMod = 2.5f;
    public Transform GroundCheckPoint;
    private bool IsGrounded;
    public LayerMask GroundLayers;

    // gunshot time and shots fired
    public GameObject BulletImpact;
    private float ShotCounter;
    public float MuzzleDisplayTime;
    private float MuzzleCounter;

    //overheat weapon
    public float MaxHeat = 10, CoolRate = 4f, OverHeatCoolRate = 5f;
    private float HeatCounter;
    private bool OverHeated;

    // gun switching
    public Gun[] AllGuns;
    private int SelectedGun;

    public GameObject PlayerHitImpact;

    public int MaxHealth = 200;
    private int CurrentHealth;
    public Animator PlayerAnimator;
    public GameObject PlayerModel;
    public Transform ModelGunPoint, GunHolder;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        camera = Camera.main;
        UIController.instance.WeaponTemperatureSlider.maxValue = MaxHeat;
        //SwitchGun();
        CurrentHealth = MaxHealth;
        photonView.RPC("SetGun", RpcTarget.All, SelectedGun);
        // removed spawn here as we want to handle this through player spawner
        /// if in first person view, disable player model locally not on network
        /// it will be visible on network to other players
        if(photonView.IsMine)
        {
            PlayerModel.SetActive(false);
            UIController.instance.HealthSlider.maxValue = MaxHealth;
            UIController.instance.HealthSlider.value = CurrentHealth;
        }
        else
        {
            GunHolder.parent = ModelGunPoint;
            GunHolder.localPosition = Vector3.zero;
            GunHolder.localRotation = Quaternion.identity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        /// If current player is owner than only update
        if (photonView.IsMine)
        {
            MouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * MouseSensitivity;

            /// rotate player camera left and right
            this.transform.rotation = Quaternion.Euler(this.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y + MouseInput.x, this.transform.rotation.eulerAngles.z);

            /// rotate viewpoint camera up and down
            /// Directly using clamp causes issues with quaternion
            VerticalRotStore += MouseInput.y;
            VerticalRotStore = Mathf.Clamp(VerticalRotStore, -60, 60);
            if (InvertLook)
            {
                ViewPoint.rotation = Quaternion.Euler(VerticalRotStore, ViewPoint.rotation.eulerAngles.y, ViewPoint.rotation.eulerAngles.z);
            }
            else
            {
                ViewPoint.rotation = Quaternion.Euler(-VerticalRotStore, ViewPoint.rotation.eulerAngles.y, ViewPoint.rotation.eulerAngles.z);
            }

            MoveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (Input.GetKey(KeyCode.LeftShift))
            {
                ActiveMoveSpeed = RunSpeed;
            }
            else
            {
                ActiveMoveSpeed = MoveSpeed;
            }

            float yVelocity = Movement.y;
            /// multiply by movedirection.z to follow camera in z transform
            Movement = ((transform.forward * MoveDirection.z) + (transform.right * MoveDirection.x)).normalized * ActiveMoveSpeed/*whole value, since diagonl movement is faster*/;
            Movement.y = yVelocity;
            if (characterController.isGrounded)
            {
                Movement.y = 0;
            }

            /// check if ground is below or not using raycast
            IsGrounded = Physics.Raycast(GroundCheckPoint.position, Vector3.down, 0.25f, GroundLayers);
            if (Input.GetButtonDown("Jump") && IsGrounded)
            {
                Movement.y = JumpForce;
            }
            Movement.y += Physics.gravity.y * Time.deltaTime * GravityMod;
            characterController.Move(Movement * Time.deltaTime);

            // handle shooting and muzzle flash
            if (AllGuns[SelectedGun].MuzzleFlash.activeInHierarchy)
            {
                MuzzleCounter -= Time.deltaTime;
                if (MuzzleCounter <= 0)
                {
                    AllGuns[SelectedGun].MuzzleFlash.SetActive(false);
                }
            }
            if (!OverHeated)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }
                // allow firing weapon if left mouse down and isautomatic allowed
                if (Input.GetMouseButton(0) && AllGuns[SelectedGun].IsAutomatic)
                {
                    ShotCounter -= Time.deltaTime;
                    if (ShotCounter <= 0)
                    {
                        Shoot();
                    }
                }
                HeatCounter -= CoolRate * Time.deltaTime;
            }
            else
            {
                HeatCounter -= OverHeatCoolRate * Time.deltaTime;
                if (HeatCounter <= 0)
                {
                    OverHeated = false;
                    UIController.instance.OverheatedMessage.gameObject.SetActive(false);
                }
            }
            if (HeatCounter < 0)
            {
                HeatCounter = 0;
            }

            UIController.instance.WeaponTemperatureSlider.value = HeatCounter;

            // switch between weapon using scroll wheel
            // check in forward direction
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                SelectedGun++;
                if (SelectedGun >= AllGuns.Length)
                {
                    SelectedGun = 0;
                }
                photonView.RPC("SetGun", RpcTarget.All, SelectedGun);
            }
            // check in opposite direction
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                SelectedGun--;
                if (SelectedGun < 0)
                {
                    SelectedGun = AllGuns.Length - 1;
                }
                photonView.RPC("SetGun", RpcTarget.All, SelectedGun);
            }

            // switch weapon using num keys
            for (int i = 0; i < AllGuns.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    SelectedGun = i;
                    photonView.RPC("SetGun", RpcTarget.All, SelectedGun);
                }
            }

            PlayerAnimator.SetBool("grounded", IsGrounded);
            /// magnitude represents the distance being covered
            PlayerAnimator.SetFloat("speed", MoveDirection.magnitude);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }

    private void Shoot()
    {
        /// center of the screen
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        ray.origin = camera.transform.position;

        /// get information of what the ray hit
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            /// if we hit player
            if(hit.collider.gameObject.tag.Equals("Player"))
            {
                //Debug.Log("Hit " + hit.collider.gameObject.GetPhotonView().Owner.NickName);
                PhotonNetwork.Instantiate(PlayerHitImpact.name, hit.point, Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, AllGuns[SelectedGun].ShotDamage);
            }
            else
            {
                /// effect should rotate according to the surface, face is looking
                GameObject bulletImpactObject = Instantiate(BulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));
                Destroy(bulletImpactObject, 1f);
            }
        }
        ShotCounter = AllGuns[SelectedGun].TimeBetweenShots;
        HeatCounter += AllGuns[SelectedGun].HeatPerShot;
        if(HeatCounter >= MaxHeat)
        {
            HeatCounter = MaxHeat;
            OverHeated = true;
            UIController.instance.OverheatedMessage.gameObject.SetActive(true);
        }
        AllGuns[SelectedGun].MuzzleFlash.SetActive(true);
        MuzzleCounter = MuzzleDisplayTime;
    }

    /// <summary>
    /// When bullet deal damage
    /// </summary>
    [PunRPC]
    public void DealDamage(string killer, int damageAmount)
    {
        TakeDamage(killer, damageAmount);
    }

    /// <summary>
    /// Sequence to perform when damage is taken or bullet hits player
    /// </summary>
    /// <param name="damager"></param>
    public void TakeDamage(string killer, int damageAmount)
    {
        if(photonView.IsMine)
        {
            //Debug.Log(photonView.Owner.NickName + " been hit by " + killer);
            CurrentHealth -= damageAmount;
            if(CurrentHealth <= 0 )
            {
                CurrentHealth = 0;
                PlayerSpawner.instance.Die(killer);
            }
            UIController.instance.HealthSlider.value = CurrentHealth;
        }
    }

    private void LateUpdate()
    {
        /// If current player is owner than only update the camera
        if(photonView.IsMine)
        {
            camera.transform.position = ViewPoint.position;
            camera.transform.rotation = ViewPoint.rotation;
        }
    }

    /// <summary>
    /// Swtich between guns
    /// </summary>
    public void SwitchGun()
    {
        foreach(Gun gun in AllGuns)
        {
            gun.gameObject.SetActive(false);
        }
        AllGuns[SelectedGun].gameObject.SetActive(true);
        AllGuns[SelectedGun].MuzzleFlash.SetActive(false);
    }

    /// <summary>
    /// Set gun for all characters to be visible
    /// </summary>
    /// <param name="GunToSwitchTo"></param>
    [PunRPC]
    public void SetGun(int GunToSwitchTo)
    {
        if(GunToSwitchTo < AllGuns.Length)
        {
            SelectedGun = GunToSwitchTo;
            SwitchGun();
        }
    }
}
