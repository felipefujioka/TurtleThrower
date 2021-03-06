﻿using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TurtleThrower
{

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class ShellController : MonoBehaviour
    {
	    public Rigidbody2D rb;
	    public Collider2D collider;

	    private bool physicEnabled = false;

	    public float Velocity;
	    private Vector3 lastPosition;

	    private int groundLayer;
	
	    private void Awake()
	    {
		    groundLayer = LayerMask.NameToLayer("Ground");
		    
		    if (rb == null)
		    {
			    rb = GetComponent<Rigidbody2D>();    
		    }

		    if (collider == null)
		    {
			    collider = GetComponent<PolygonCollider2D>();    
		    }
		    
		    var interactable = GetComponent<Interactable>();
		    if (interactable == null)
		    {
			    interactable = GetComponentInChildren<Interactable>();
		    }
		    
	    }

	    private void FixedUpdate()
	    {
			if (!physicEnabled)
			{
				return;
			}
	    }

	    
	    
	    public void ThrowShell(Vector2 direction, float force)
	    {
		    physicEnabled = true;

		    transform.parent = null;
		    
		    rb.bodyType = RigidbodyType2D.Dynamic;
		    rb.simulated = true;
		    rb.velocity = direction * force;
		    rb.AddTorque(-0.01f, ForceMode2D.Impulse);
	    }

	    /// <summary>
	    /// When the shell is attached to turtle, rigidbody, colliders and physics must be disabled.
	    /// </summary>
	    public void SetAttachedToTurtle(Transform parentPivot, TweenCallback finishEquipCallback)
	    {
		    physicEnabled = false;
		    
		    transform.SetParent(parentPivot);

		    transform.localScale = Vector3.one;

		    transform.DOLocalMove(Vector3.zero, 0.25f);

		    rb.bodyType = RigidbodyType2D.Kinematic;
		    rb.simulated = false;

		    var reverseRotation = transform.DOLocalRotate(Vector3.zero, 0.3f);
			reverseRotation.onComplete += finishEquipCallback;		
			reverseRotation.Play();

		    GetComponent<SpriteRenderer>().sortingOrder = 2;
	    }
	    
	    
	    
    }

}

