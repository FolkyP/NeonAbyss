using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        Health,
        Shield,
        Ammo,
        Crystal
    }

    [Header("PowerUp Settings")]
    public PowerUpType type;
    public int amount = 50;

    [Header("Visual FX")]
    public float rotateSpeed = 90f;
    public float floatAmplitude = 0.2f;
    public float floatFrequency = 2f;
    public AudioClip pick;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position + Vector3.up * 0.2f;


    }

    void Update()
    {
        Rotate();
        Float();
    }

    void Rotate()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
    }

    void Float()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerLife life = other.GetComponentInParent<PlayerLife>();
        

        WeaponManager weaponManager = other.GetComponentInParent<WeaponManager>();

        ApplyPowerUp(life,weaponManager);

        if (pick != null)
        {
            AudioSettings.Instance.PlaySFX(pick);
        }
        Destroy(gameObject);
    }

    void ApplyPowerUp(PlayerLife player,WeaponManager weapon)
    {
        switch (type)
        {
            case PowerUpType.Health:
                player.Heal(amount);
                break;

            case PowerUpType.Shield:
                player.AddShield(amount);
                break;

            case PowerUpType.Ammo:
                {

                    WeaponManager wm = player.GetComponentInChildren<WeaponManager>();
                    if (wm != null)
                        wm.AddAmmoToCurrentWeapon();

                    break;
                }
        }
    }
}
