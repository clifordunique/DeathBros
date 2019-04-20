﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticStates
{
    public static SCS_Idle idle = new SCS_Idle();
    public static SCS_Walking walking = new SCS_Walking();
    public static SCS_Jumping jumping = new SCS_Jumping();
    public static SCS_Jumpsquat jumpsquat = new SCS_Jumpsquat();
    public static SCS_Landing landing = new SCS_Landing();

    public static SCS_Crouch crouch = new SCS_Crouch(EStateType.Complex);
    public static SCS_DoubleJumpsquat doubleJumpsquat = new SCS_DoubleJumpsquat(EStateType.Complex);
    public static SCS_Dash dash = new SCS_Dash(EStateType.Complex);
    public static SCS_Skid skid = new SCS_Skid(EStateType.Complex);

    public static SCS_Wallsliding wallsliding = new SCS_Wallsliding(EStateType.Complex);
    public static SCS_WalljumpStart walljumpStart = new SCS_WalljumpStart(EStateType.Complex);
    public static SCS_Walljumping walljumping = new SCS_Walljumping(EStateType.Complex);

    public static SCS_Shield shield = new SCS_Shield();
    public static SCS_HitLand hitland = new SCS_HitLand();
    public static SCS_HitLanded hitlanded = new SCS_HitLanded();
    public static SCS_Hitstun hitstun = new SCS_Hitstun();
    public static SCS_Roll roll = new SCS_Roll();
    public static SCS_StandUp standUp = new SCS_StandUp();

    public static SCS_Dead dead = new SCS_Dead();
    public static SCS_Die die = new SCS_Die();

    public static SCS_ThrowItem throwItem = new SCS_ThrowItem();
    public static SCS_AirDodge airDodge = new SCS_AirDodge();
    public static SCS_ShieldHit shieldHit = new SCS_ShieldHit();
    public static SCS_Grab grab = new SCS_Grab();
    public static SCS_GetGrabbed getGrabbed = new SCS_GetGrabbed();
}