﻿

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : MonoBehaviour
{
    public Vector2 input;
    public Vector2 knockback;
    public bool slowDown;
    [Space]
    public Vector2 velocity;
    public float jumpVelocity;
    public bool jump;
    [Space]
    public bool applyGravity = true;
    public bool resetVelocity = false;
    public Vector2 forceMovement = Vector2.zero;
    public Vector2 addMovement = Vector2.zero;
    [Space]
    public float angleX, angleY;
    public float oldAngleX;
    [Space]
    public bool grounded;
    public bool oldGrounded;
    public bool onWall;
    public bool oldOnWall;
    public bool onPlatform;
    public bool oldOnPlatform;
    public bool insidePlatform;
    public int wallDirection;
    [Space]
    public bool fallThroughPlatform;
    public int fallThroughPlatformDuration = 5;
    private int fallThroughPlatformTimer = 0;
    [Space]
    public Vector2 hitDirection;
    public float gamma;
    public float alpha;
    [Space]
    public LayerMask collisionMask;
    public LayerMask platformMask;
    public string movingPlatformTag = "MovingPlatform";
    public BoxCollider2D Col { get; protected set; }
    public PlatformController currentPlatform;
    [Space]
    public float gravity = -1f;
    public float movespeed = 10f;
    public float wallSlideSpeed = 3f;
    //public float accelerationTimeAerial = 0.1f;
    [Space]
    public float maxNormalFallSpeed = -20f;
    public float maxFastFallSpeed = -30f;
    public bool fastFall = false;
    [Space]
    public float maxSlopeDownAngle = 45;
    public float maxSlopeUpAngle = 45;
    [Space]
    public float velocityXSmoothingAerial = 0.5f;
    protected float moveAngle;

    protected Bounds bounds;
    protected Bounds fullBounds;

    protected static float skin = 0.01f;
    protected float diag = Mathf.Sqrt(2) * skin;

    protected Vector2 gizmoVector;
    protected Vector2 gizmoSlopeDown;

    void Start()
    {
        velocity = Vector2.zero;
        Col = GetComponent<BoxCollider2D>();

    }

    private void Update()
    {
        //input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));


    }

    public void ManualFixedUpdate()
    {
        Move();

        transform.Translate(velocity);
    }

    protected RaycastHit2D RCXY(Vector2 direction, float distance, bool includePlatforms = false)
    {
        moveAngle = Vector2.Angle(velocity, Vector2.right * Mathf.Sign(velocity.x));

        gizmoVector = velocity.normalized;
        gizmoVector *= (velocity.magnitude);

        LayerMask collisionMasks = collisionMask;

        if (includePlatforms)
            collisionMasks += platformMask;

        float skinDistance = skin;
        float cos = Mathf.Cos(moveAngle * Mathf.Deg2Rad);
        if (cos > skin && cos != 0)
            skinDistance = skin / cos;

        return Physics2D.BoxCast(bounds.center, bounds.size, 0, direction, distance + skinDistance, collisionMasks);
    }

    protected RaycastHit2D RCXY(bool includePlatforms = false)
    {
        return RCXY(velocity, velocity.magnitude, includePlatforms);
    }

    private void Move()
    {
        oldGrounded = grounded;
        oldAngleX = angleX;
        oldOnWall = onWall;
        oldOnPlatform = onPlatform;

        angleX = angleY = 0;
        //wallDirection = 0;

        grounded = false;
        onWall = false;
        onPlatform = false;
        insidePlatform = false;

        //update bounds
        bounds = fullBounds = Col.bounds;
        bounds.Expand(-2 * skin);

        //set often used variables
        float dirX = Mathf.Sign(input.x);



        //reset moving platform vector
        Vector2 movingPlatform = Vector2.zero;

        if (input.y < 0)
        {
            fallThroughPlatform = true;
            fallThroughPlatformTimer = fallThroughPlatformDuration;
        }

        if (fallThroughPlatform)
        {
            fallThroughPlatformTimer--;
        }

        if (fallThroughPlatformTimer <= 0)
        {
            fallThroughPlatform = false;
        }

        bool jump = false;

        //set velocity
        if (resetVelocity)
        {
            velocity = Vector2.zero;
        }

        if (oldGrounded)
        {
            velocity.x = input.x / 60 * movespeed;

            velocity.y = gravity / 6;
        }
        else
        {
            float targetVelocityX = input.x / 60 * movespeed;

            //float smoothedVelocityX = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothingAerial, accelerationTimeAerial / 60);

            //velocity.x += smoothedVelocityX;
            float oldVel = velocity.x;
            velocity.x -= Mathf.Sign(velocity.x) * velocityXSmoothingAerial / 60;

            if (Mathf.Sign(oldVel) != Mathf.Sign(velocity.x))
            {
                velocity.x = 0;
            }

            velocity.x += targetVelocityX;

            velocity.x = Mathf.Clamp(velocity.x, -movespeed / 60, movespeed / 60);
            velocity.y += gravity / 60;

            if (!fastFall)
            {
                if (velocity.y < maxNormalFallSpeed / 60)
                {
                    velocity.y = maxNormalFallSpeed / 60;
                }
            }
            else if (velocity.y < 0)
            {
                velocity.y = maxFastFallSpeed / 60;
            }
            else
            {
                fastFall = false;
            }
        }


        if (jumpVelocity != 0)
        {
            velocity.y = jumpVelocity / 60;
            jumpVelocity = 0;
            jump = true;
        }

        velocity += addMovement;

        if (forceMovement.x != 0) velocity.x = forceMovement.x;
        if (forceMovement.y != 0) velocity.y = forceMovement.y;

        //check if inside platform
        if (!fallThroughPlatform && !jump)
        {
            RaycastHit2D platformCheck = Physics2D.BoxCast(Col.bounds.center, Col.bounds.size, 0, velocity, velocity.magnitude, platformMask);

            if (platformCheck)
            {
                if (platformCheck.transform.tag == movingPlatformTag)
                {
                    if (currentPlatform == platformCheck.transform.GetComponentInParent<PlatformController>())
                    {
                        movingPlatform = currentPlatform.Movement;
                        velocity = movingPlatform;

                        onPlatform = true;
                        grounded = true;

                        //push upwards if inside of platform
                        if (platformCheck.distance == 0)
                        {
                            velocity.y -= gravity / 30;
                        }
                    }
                }
                else if (platformCheck.distance == 0)
                {
                    //check again for standing platforms
                    platformCheck = Physics2D.BoxCast(bounds.center, bounds.size, 0, new Vector2(velocity.x, 0), Mathf.Abs(velocity.x) + skin, platformMask);

                    if (platformCheck)
                    {
                        onPlatform = false;
                        //grounded = false;
                        //insidePlatform = true;
                        //fallThroughPlatform = true;
                    }
                }
            }
        }




        //CHECK IF GROUNDED//////////////////////////////////////////////////////////
        LayerMask groundMask = collisionMask;


        //only groundcheck when falling down
        if (velocity.y <= 0)
        {
            //check only for platforms as ground when not pressing down
            if (!fallThroughPlatform)
            {
                groundMask += platformMask;
            }

            RaycastHit2D groundCheck;

            moveAngle = Vector2.Angle(velocity, Vector2.right * dirX);

            float skinDistance = skin;
            float cos = Mathf.Cos(moveAngle * Mathf.Deg2Rad);
            if (cos > skin && cos != 0)
                skinDistance = skin / cos;

            groundCheck = Physics2D.BoxCast((Vector2)bounds.center - new Vector2(0, bounds.extents.y),
                new Vector2(bounds.size.x, skin), 0, velocity, velocity.magnitude + skinDistance, groundMask);

            float wallAngle = Vector2.Angle(groundCheck.normal, Vector2.up);

            if (groundCheck)// && groundCheck.distance != 0)// && wallAngle != 90) //not grounded if inside the collider (duteance == 0) for example in platforms
            {
                if (wallAngle >= 90) //if on wall check for found only inm downwards direction, otherwise grounded on wall
                {
                    groundCheck = Physics2D.BoxCast((Vector2)bounds.center - new Vector2(0, bounds.extents.y),
                new Vector2(bounds.size.x, skin), 0, Vector2.up, velocity.y + -skin, groundMask);

                }

                if (groundCheck)
                {
                    grounded = true;

                    if (groundCheck.collider.gameObject.layer == 9)
                    {
                        onPlatform = true;
                    }

                    //triangle calculations
                    hitDirection = (Vector2)bounds.center - groundCheck.point;
                    hitDirection = new Vector2(Mathf.Sign(hitDirection.x), Mathf.Sign(hitDirection.y));

                    gamma = Mathf.Abs(90 - Vector2.Angle(groundCheck.normal, hitDirection));
                    alpha = Mathf.Abs(90 - Vector2.Angle(groundCheck.normal, velocity));

                    float distance = skin;

                    float sin = Mathf.Sin(alpha * Mathf.Deg2Rad);
                    if (sin != 0)
                        distance = diag * Mathf.Sin(gamma * Mathf.Deg2Rad) / sin;

                    float moveDistance = (groundCheck.distance - distance);

                    //reduce velocity
                    velocity.Normalize();
                    velocity *= (moveDistance);

                    if (velocity.y > 0) //prevent the player from moving up platforms when standing inside of them
                    {
                        velocity.y = 0;
                    }

                    //add platform velocity if on platform
                    if (groundCheck.transform.tag == movingPlatformTag)
                    {
                        currentPlatform = groundCheck.transform.GetComponentInParent<PlatformController>();

                        movingPlatform = currentPlatform.Movement;

                        if (!oldOnPlatform)
                            velocity.y -= movingPlatform.y;

                        velocity += movingPlatform;

                        onPlatform = true;
                    }
                }
            }
        }

        //normalize gravity, set higher before to cope with changing slopeAngles
        if (!grounded && oldGrounded && !jump)
        {
            velocity.y = gravity / 60;
        }

        //if grounded, try again to move on x axis
        if (oldGrounded)
        {
            velocity.x += input.x / 60 * movespeed;
        }

        //SLOPES/////////////////////////////////////////////////////////////////////////

        if (velocity.x != 0 && oldGrounded && !jump)
        {
            LayerMask slopeMask = collisionMask;

            if (!insidePlatform)
            {
                slopeMask += platformMask;
            }

            //ASCEND SLOPES////////////////////////////////////////////////////////////////////////////
            Vector2 raycastXOrigin = (Vector2)bounds.center + new Vector2(bounds.extents.x * dirX, -bounds.extents.y - skin); // added skin here to prevent getting stuck on ending slopes

            RaycastHit2D hitX = Physics2D.Raycast(raycastXOrigin, Vector2.right * dirX, Mathf.Abs(velocity.x) + skin, slopeMask);

            if (hitX)
            {
                float angle = Vector2.Angle(Vector2.up, hitX.normal);

                if (angle < 90)
                    angleX = angle;

                //move up slope only when at the slope; don't check when there is no wall (90)
                if (angleX <= maxSlopeUpAngle && angleX != 90)
                {
                    //always move up  when on platforms, otherwise you can't climb them when running on the ground, since you're insidePlatform
                    if (angleX == oldAngleX || hitX.collider.gameObject.layer == 9)
                    {
                        velocity = new Vector2(dirX * Mathf.Cos(Mathf.Deg2Rad * angleX),
                            Mathf.Sin(Mathf.Deg2Rad * angleX)).normalized / 60 * movespeed * Mathf.Abs(input.x);
                    }

                    hitX = Physics2D.Raycast(raycastXOrigin, velocity, velocity.magnitude + skin, slopeMask);

                    if (hitX)
                    {
                        angle = Vector2.Angle(Vector2.up, hitX.normal);

                        if (angle < 90)
                            angleX = angle;
                    }
                }
            }


            //DESCEND SLOPES////////////////////////////////////////////////////////////////////////////
            gizmoSlopeDown = new Vector2(velocity.x, 0) + (Vector2)bounds.center - new Vector2(bounds.extents.x * dirX, bounds.extents.y);

            RaycastHit2D slopeDown = Physics2D.Raycast(gizmoSlopeDown, Vector2.down, 1, slopeMask);

            if (slopeDown)
            {
                angleY = Vector2.Angle(Vector2.up, slopeDown.normal);

                float angleD = Vector2.Angle(slopeDown.normal, new Vector2(input.x, 0));


                //push player down to slope //&& angleY > 0 removed
                if (velocity.y < 0 && input.x != 0 && slopeDown && angleY <= maxSlopeDownAngle && angleD <= 90)
                {
                    grounded = true;

                    float skinDistance = diag;

                    alpha = angleY;
                    gamma = 90 + Vector2.Angle(slopeDown.normal, new Vector2(dirX, 1));

                    float sin = Mathf.Sin(Mathf.Deg2Rad * alpha);

                    if (sin != 0)
                        skinDistance = diag * (Mathf.Sin(Mathf.Deg2Rad * gamma) / Mathf.Sin(Mathf.Deg2Rad * alpha));

                    velocity.y = -(slopeDown.distance - skinDistance);

                    //normalize the velocity
                    velocity.Normalize();
                    velocity *= Mathf.Abs(input.x) * movespeed / 60;
                }
            }

        }

        //APPLY KNOCKBACK ??????????????????????????????????????????????????????????????
        if (knockback != Vector2.zero)
        {
            //velocity.x += knockback.x / 60;
            //velocity.y = knockback.y / 60;
            velocity = knockback / 60;

            grounded = false;
        }

        knockback = Vector2.zero;

        if (slowDown)
        {
            velocity *= 0.1f;
        }

        //CHECK FOR COLLISIONS//////////////////////////////////////////////////////////
        if (oldOnWall)
        {
            velocity.x = input.x * movespeed / 60;
        }

        RaycastHit2D collisionCheck = RCXY();

        //try to stop collision by just moving on x axis
        if (collisionCheck)
        //if (collisonCheck)
        {
            RaycastHit2D wallDistanceCheck = RCXY(Vector2.right, velocity.x);

            if (wallDistanceCheck)
            {
                velocity.x = dirX * (wallDistanceCheck.distance * skin);
            }

            collisionCheck = RCXY();


            if (!collisionCheck && !grounded)
            {
                onWall = true;
                wallDirection = (int)dirX;

                if (velocity.y <= 0)
                {
                    velocity.y = -wallSlideSpeed / 60;
                }
            }

        }

        if (collisionCheck)
        {
            //triangle calculations
            hitDirection = (Vector2)bounds.center - collisionCheck.point;
            hitDirection = new Vector2(Mathf.Sign(hitDirection.x), Mathf.Sign(hitDirection.y));

            gamma = Mathf.Abs(90 - Vector2.Angle(collisionCheck.normal, hitDirection));
            alpha = Mathf.Abs(90 - Vector2.Angle(collisionCheck.normal, velocity));

            float distance = skin;

            float sin = Mathf.Sin(alpha * Mathf.Deg2Rad);
            if (sin != 0)
                distance = diag * Mathf.Sin(gamma * Mathf.Deg2Rad) / sin;

            float moveDistance = (collisionCheck.distance - distance);

            //reduce velocity
            velocity.Normalize();
            velocity *= (moveDistance);


            //don't stick on roof
            if (!grounded) //!!!!!!!!! causes slipping through ground
            {
                //velocity.y += gravity / 60;
            }


            currentPlatform = null;

        }

        if (!grounded)
        {
            currentPlatform = null;
        }
        else
        {
            fastFall = false;
        }

        forceMovement = Vector2.zero;
        addMovement = Vector2.zero;
        applyGravity = true;
        resetVelocity = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, bounds.size);

        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(transform.position + (Vector3)gizmoVector, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, bounds.size);

        Gizmos.matrix = Matrix4x4.TRS((Vector3)gizmoSlopeDown, transform.rotation, Vector3.one);
        Gizmos.DrawRay(Vector3.zero, Vector2.down);
    }
}


