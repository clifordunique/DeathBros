﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StaticAttackStates/Special_Projectile")]
public class StaticAttackStateProjectile : StaticAttackStateSO
{
    public NES_Projectile projectile;
    public Vector2 projectileVelocity;

    public override SCS_Attack CreateAttackState()
    {
        SCS_SpecialAttackProjectile projectileAttack = new SCS_SpecialAttackProjectile();
        projectileAttack.animationName = animationName;
        projectileAttack.projectile = projectile;
        projectileAttack.projectileVelocity = projectileVelocity;
        projectileAttack.attackBuff = new AttackBuff();
        return projectileAttack;
    }
}

public class SCS_SpecialAttackProjectile : SCS_SpecialAttack
{
    public NES_Projectile projectile;
    public Vector2 projectileVelocity;

    public override void Enter(Character chr)
    {
        base.Enter(chr);

        chr.ASpawnProjectile += SpawnProjectile;
    }

    public override void Execute(Character chr)
    {
        base.Execute(chr);
    }

    public override void Exit(Character chr)
    {
        base.Exit(chr);

        chr.ASpawnProjectile -= SpawnProjectile;
    }

    protected void SpawnProjectile(Character chr, Vector2 position)
    {
        chr.SCS_SpawnProjetile(projectile, projectileVelocity);
    }
}


public class SCS_SpecialAttack : SCS_Attack
{

    public override void Enter(Character chr)
    {
        base.Enter(chr);
    }

    public override void Execute(Character chr)
    {
        base.Execute(chr);

        chr.FrozenInputX *= 0.95f;

        chr.SetInputs(new Vector2(chr.FrozenInputX, 0));

        //if (chr.Ctr.onLedge)
        //    chr.SetInputs(Vector2.zero);

        if (chr.Anim.animationOver)
        {
            chr.SCS_Idle();
        }
    }
}