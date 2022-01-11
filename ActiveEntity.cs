using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum EntityState
{
    Moving,
    Following,
    Attacking,
    AttackMoving,
    Defending,
    Pursuing
}

public class ActiveEntity : Entity
{
    public float baseDamage;
    public float baseAttackTime;
    public float attackRange;

    public bool canMove;
    public bool canAttack;
    public bool canChop;
    public string[] availableAbilities;
    public string[] builds;

    public Team team;

    public bool checkingForTargets;
    public bool holdingPosition;
    public bool patrolling;
    private Vector3 oldLocation;
    public EntityState currentState;

    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip commandedSound;
    [SerializeField] private AudioClip attackCommandedSound;
    public Sprite icon;
    public HPBar hpBar;
    public CapsuleCollider capsuleCollider;
    protected NavMeshAgent navMeshAgent;

    protected override void Awake()
    {
        base.Awake();

        capsuleCollider = GetComponent<CapsuleCollider>();
        hpBar = Instantiate(Resources.Load<HPBar>("Prefabs/UI/HP Bar Canvas"),
            new Vector3(), Quaternion.identity, transform);
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent)
        {
            navMeshAgent.angularSpeed = 1500;
            navMeshAgent.acceleration = 100;
        }

        Stop();
    }

    public virtual void ResetSate()
    {
        StopAllCoroutines();
        if(navMeshAgent) navMeshAgent.ResetPath();
        checkingForTargets = false;
        holdingPosition = false;
        patrolling = false;
    }

    public void MoveToLocation(Vector3 targetLocation)
    {
        if(isDead) return;
        ResetSate();

        oldLocation = transform.position;
        navMeshAgent.SetDestination(targetLocation);
        currentState = EntityState.Moving;
        StartCoroutine(CheckIfReachedDestinationCoroutine());

        UpdateAnimation();
    }

    public void Follow(Entity target)
    {
        if (isDead) return;
        ResetSate();

        currentState = EntityState.Following;
        StartCoroutine(FollowCoroutine(target));

        //Animation is updated in coroutine
        OnMove();
    }

    public virtual void Stop()
    {
        if (isDead) return;
        ResetSate();

        currentState = EntityState.Defending;
        StartCoroutine(CheckingForTargetsCoroutine());

        UpdateAnimation();
    }

    public void HoldPosition()
    {
        if (isDead) return;
        Stop();
        holdingPosition = true;
    }

    public void Patrol(Vector3 location)
    {
        if (isDead) return;
        ResetSate();

        oldLocation = transform.position;
        patrolling = true;
        AttackMove(location);
    }

    public virtual void AttackMove(Vector3 targetPoint)
    {
        if (isDead) return;
        oldLocation = transform.position;

        if (patrolling)
        {
            StopAllCoroutines();
            checkingForTargets = false;
            holdingPosition = false;
        }
        else ResetSate();

        navMeshAgent.SetDestination(targetPoint);
        currentState = EntityState.AttackMoving;
        StartCoroutine(CheckingForTargetsCoroutine());
        StartCoroutine(CheckIfReachedDestinationCoroutine());

        UpdateAnimation();
    }

    public virtual bool Attack(Entity target)
    {
        if (isDead) return false;

        if (target.isDead)
        {
            Debug.Log("Target is dead.");
            return false;
        }
        if (target is ActiveEntity activeEntity)
        {
            if (activeEntity.team == team)
            {
                Debug.Log("Cannot attack your own team.");
                return false;
            }
        }
        if (!canAttack && !(target is Trees))
        {
            Debug.Log("Unit cannot attack.");
            return false;
        }
        if (!canChop && target is Trees)
        {
            Debug.Log("Unit cannot chop trees.");
            return false;
        }
        if (target.isInvulnerable)
        {
            Debug.Log("Target is invulnerable.");
            return false;
        }

        if (!CheckIfTargetWithinAttackRange(target) && !holdingPosition)
        {
            Debug.Log("Not in range, engaging pursuit");
            ResetSate();

            Pursue(target);
            return true;
        }
        if (!CheckIfTargetWithinAttackRange(target) && holdingPosition)
            return false;

        ResetSate();

        StartCoroutine(AttackingCoroutine(target));
        return true;
    }

    public void Pursue(Entity target)
    {
        if (isDead) return;
        ResetSate();

        currentState = EntityState.Pursuing;
        StartCoroutine(PursuitCoroutine(target));

        UpdateAnimation();
    }

    private IEnumerator TurnTowards(Entity target)
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 30);

        float dot = Vector3.Dot(transform.forward, (target.transform.position - transform.position).normalized);

        while (dot < 0.95f)
        {
            yield return new WaitForSeconds(.05f);
            direction = (target.transform.position - transform.position).normalized;
            lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 30);
            dot = Vector3.Dot(transform.forward, (target.transform.position - transform.position).normalized);
        }
    }

    private IEnumerator CheckIfReachedDestinationCoroutine()
    {
        while (currentState == EntityState.Moving || currentState == EntityState.AttackMoving)
        {
            if (navMeshAgent.destination.x == transform.position.x &&
                navMeshAgent.destination.z == transform.position.z)
            {
                if (patrolling)
                {
                    AttackMove(oldLocation);
                    yield break;
                }
                else
                {
                    Stop();
                }
            }
            else yield return new WaitForSeconds(.1f);
        }
    }

    private IEnumerator CheckingForTargetsCoroutine()
    {
        yield return new WaitForSeconds(.1f);

        checkingForTargets = true;
        while (currentState == EntityState.AttackMoving || currentState == EntityState.Defending || currentState == EntityState.Pursuing)
        {
            foreach (var collider in Physics.OverlapSphere(transform.position, (attackRange + 100)/100))
            {
                if (collider.GetComponent<Entity>() is Entity entity && entity != this)
                {
                    if (Attack(entity))
                    {
                        checkingForTargets = false;
                        yield break;
                    }
                }
            }
            yield return new WaitForSeconds(.5f);
        }

        checkingForTargets = false;
    }

    private IEnumerator PursuitCoroutine(Entity target)
    {
        while (currentState == EntityState.Pursuing)
        {
            if (!CheckIfTargetWithinAttackRange(target))
                navMeshAgent.SetDestination(target.transform.position);
            else
            {
                Attack(target);
                break;
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    private IEnumerator FollowCoroutine(Entity target)
    {
        //Animations updated directly since this move command is continuous and doesn't check for reaching destination
        while (currentState == EntityState.Following)
        {
            if (!CheckIfTargetWithinFollowRange(target))
            {
                navMeshAgent.SetDestination(target.transform.position);
                animator.SetBool("walking", true);
            }
            else
            {
                navMeshAgent.ResetPath();
                animator.SetBool("walking", false);
            }

            yield return new WaitForSeconds(.5f);
        }
    }

    private IEnumerator AttackingCoroutine(Entity target)
    {
        currentState = EntityState.Attacking;
        yield return StartCoroutine(TurnTowards(target));

        UpdateAnimation();
        OnAttackStarted();

        yield return new WaitForSeconds(baseAttackTime);
  
        target.TakeDamage(baseDamage, this);

        if (!target.isDead)
        {
            if (!(this is Peasant peasant) || peasant.lumberCarried < peasant.maxLumberCarried)
                Attack(target);
        }
        else Stop();
    }

    protected bool CheckIfTargetWithinAttackRange(Entity target)
    {
        if (Physics.OverlapSphere(transform.position, attackRange / 100).Any
            (hit => hit.GetComponent<Entity>() == target.GetComponent<Entity>()))
            return true;
        return false;
    }

    protected bool CheckIfTargetWithinFollowRange(Entity target)
    {
        if (Physics.OverlapSphere(transform.position, 3).Any(hit => hit.GetComponent<Entity>() == target.GetComponent<Entity>()))
            return true;
        return false;
    }

    public virtual void OnMove()
    {
        UpdateAnimation();

        if (moveSound != null)
        {
            audioPlayer.PlayAudioClip(moveSound);
        }
    }

    public virtual void OnCommanded()
    {
        holdingPosition = false;
        UpdateAnimation();

        if (commandedSound != null)
        {
            audioPlayer.PlayAudioClip(commandedSound);
        }
    }

    public virtual void OnAttackCommanded()
    {
        holdingPosition = false;

        if (attackCommandedSound != null)
        {
            audioPlayer.PlayAudioClip(attackCommandedSound);
        }
    }

    public virtual void OnAttackStarted()
    {
        //play attack started sound (ranged)
    }

    public override void OnDeath(Entity killer)
    {
        ResetSate();
        if (navMeshAgent)
            navMeshAgent.enabled = false;

        base.OnDeath(killer);
    }

    //Called by target in the TakeDamage method
    public virtual void OnAttackSuccessful(Entity target)
    {
        //play attack landed sound on receiver
    }

    public override void UpdateAnimation()
    {
        if(navMeshAgent == null)
            return;
        
        animator.SetBool("walking", false);

        switch (currentState)
        {
            case EntityState.Defending:
                break;
            case EntityState.Moving:
            case EntityState.Following:
            case EntityState.AttackMoving:
            case EntityState.Pursuing:
                animator.SetBool("walking", true);
                break;
        }
    }
}
