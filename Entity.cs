using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public string entityName;
    public bool isInvulnerable;
    public float maxHP;

    [HideInInspector] public bool isDead;
    [HideInInspector] public float currentHP;

    protected AudioPlayer audioPlayer;
    protected Animator animator;

    [SerializeField] private AudioClip selectSound;

    protected virtual void Awake()
    {
        currentHP = maxHP;
        audioPlayer = FindObjectOfType<AudioPlayer>();
        animator = GetComponent<Animator>();
    }

    public virtual void OnDeath(Entity killer)
    {
        //give killer xp if hero
        isDead = true;
        
        StartCoroutine(PlayDyingAnimation());
    }

    public virtual void TakeDamage(float damageAmount, ActiveEntity damageDealer)
    {
        if (damageDealer is Peasant peasant)
            peasant.lastTreeLocation = transform.position;

        currentHP -= damageAmount;
        OnDamageTaken();
        damageDealer.OnAttackSuccessful(this);

        if (currentHP <= 0)
        {
            currentHP = 0;
            OnDeath(damageDealer);
        }
    }

    public virtual void OnDamageTaken()
    {
        //show particles
        //play sound
    }

    public virtual void OnSelected()
    {
        if (selectSound != null)
        {
            audioPlayer.PlayAudioClip(selectSound);
        }
    }

    protected virtual IEnumerator PlayDyingAnimation()
    {
        for (int i = 0; i < 90; i++)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, i);
            yield return new WaitForSeconds(.005f);
        }
    }

    public virtual void UpdateAnimation()
    {
    }
}
