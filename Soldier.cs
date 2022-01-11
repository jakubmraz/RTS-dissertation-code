using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : Unit
{
    public override void UpdateAnimation()
    {
        base.UpdateAnimation();
        animator.SetBool("attacking", false);
        animator.SetBool("attacking2", false);
        if (currentState == EntityState.Attacking)
        {
            if (Random.Range(1, 2) == 1)
            {
                animator.SetBool("attacking", true);
                animator.SetFloat("attackSpeed", baseAttackTime/2.267f + 1);
            }
            else
            {
                animator.SetBool("attacking2", true);
                animator.SetFloat("attackSpeed", baseAttackTime/2.4f + 1);
            }
        }
        else
        {
            animator.SetBool("attacking", false);
            animator.SetBool("attacking2", false);
        }
    }
}
