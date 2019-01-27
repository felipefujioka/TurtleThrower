﻿using System.Collections.Generic;
using DefaultNamespace;
using DG.Tweening;
using TurtleThrower;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class CharacterController2D : MonoBehaviour
{
	[Header("Pivots")]
	public Transform DefaultShellPivot;
	public Transform ThrowShellPivot;
	
	[Header("Throw shell Config")]
	public Vector3 DefaultThrowDirection;
	public float DefaultThrowForce;

	[Header("Animation")] 
	public Animator Animator;
	
	private List<Interactable> interactables;
	private ShellController shellController;
	
	public float WalkVelocity = 1f;
	public float JumpHeight = 5f;
	public float Gravity = 3f;
	public float MovementAcceleration = 5f;

	public Transform Shell;
	
	public Transform Foot;
	public Rigidbody2D rigidBody;

	private bool lifting;

	public LayerMask groundMask;
	private RaycastHit2D hit;

	private float footRayDistance = 0f;

	private bool facingRight = true;

	private RaycastHit2D[] results;

	private bool grounded;

	private bool shellEquipped;
	
	Vector2 m_PreviousPosition;
	Vector2 m_CurrentPosition;
	Vector2 m_NextMovement;

	private Vector2 m_Movement;
	
	public Vector2 Velocity { get; protected set; }

	public Inventory inventory;

	private bool isDead = false;

	public bool IsDead()
	{
		return isDead;
	}

	public void Move(float value)
	{
		var scaledVelocity = value * WalkVelocity * (shellEquipped ? 0.5f : 1f);
		
		Animator.SetFloat("Velocity", Mathf.Abs(scaledVelocity));

		m_Movement.x = Mathf.MoveTowards(m_Movement.x, scaledVelocity, MovementAcceleration * Time.deltaTime);	
		
		var direction = value > 0;
		if (Mathf.Abs(value) > 0.05)
		{
			facingRight = direction;
			Flip(facingRight);
		}
	}
	
	/// <summary>
	/// Apply movement vector to next physic step.
	/// </summary>
	/// <param name="movement"></param>
	public void Move(Vector2 movement)
	{
		m_NextMovement += movement * Time.deltaTime;
	}
	
	public void Move2(Vector2 movement)
	{
		m_NextMovement += movement;
	}
	
	public bool IsLifting()
	{
		return lifting;
	}

	public bool ShellIsEquipped()
	{
		return shellEquipped;
	}
	
	public bool IsGrounded()
	{
		var resultsCount = Physics2D.RaycastNonAlloc(transform.position, Vector3.down, results, footRayDistance, groundMask);
		return resultsCount > 0;
	}

	public void Jump()
	{
		if (IsGrounded())
		{
			m_Movement.y = JumpHeight;
			Animator.SetTrigger("Jump");
		}
	}

	public void FinishThrowAnimation()
	{
		var throwDirection = DefaultThrowDirection;
		throwDirection.x = facingRight ? throwDirection.x : throwDirection.x * -1;
		shellController.ThrowShell(throwDirection, DefaultThrowForce);
		shellController = null;
		Lift(false);
		shellEquipped = false;

		Shell.GetComponent<SpriteRenderer>().sortingOrder  = - 2;
		
		SoundManager.Instance.PlaySound("throw");
	}
	
	/// <summary>
	/// Interact with nearable object. Can bem the shell, interrupt, item
	/// </summary>
	public void Interact()
	{	
		// Check proximity.
		foreach (var interactable in interactables)
		{

			var scInteractable = interactable.GetComponentInParent<ShellController>();
			if (scInteractable)
			{
				scInteractable.SetAttachedToTurtle(DefaultShellPivot, FinishEquipShell);
				this.shellController = scInteractable;
			}

			var collectableItem = interactable.GetComponent<CollectableItem>();
			if (collectableItem)
			{
				inventory.Add(collectableItem);
			}
			
			interactable.Interact();
		}
	}

	public void Lift(bool lift)
	{
		Animator.SetBool("Lifting", lift);
		Animator.ResetTrigger("Throw");
		lifting = lift;
	}

	public void Throw()
	{
		Animator.SetTrigger("Throw");
	}

	private void Flip(bool towardRight)
	{
		var currentScale = transform.localScale;
		var isFacingRight = currentScale.x < 0;
		currentScale.x = isFacingRight != towardRight ? -1 * currentScale.x : currentScale.x;
		transform.localScale = currentScale;
	}
	
	private void Awake()
	{
		footRayDistance = Foot.localPosition.magnitude * transform.localScale.y;

		results = new RaycastHit2D[1];
		
		SoundManager.Instance.PlayBGM("game_bgm");

		groundMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("MovableScenery");
	}
	
	private void Start()
	{
		interactables = new List<Interactable>();

		if (inventory == null)
		{
			inventory = GetComponent<Inventory>();
		}
		
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		Interactable interactableEnter = other.GetComponent<Interactable>();
		if (interactableEnter != null)
		{
			if (interactableEnter.Automatic)
			{
				var collectableItem = interactableEnter.GetComponent<CollectableItem>();
				if (collectableItem)
				{
					inventory.Add(collectableItem);
					NewItemScreen.Instance.ShowNewItem(collectableItem);
					SoundManager.Instance.PlaySound("item_collect");
				}
				interactableEnter.Interact();
				return;
			}
			
			if (interactables.Contains(interactableEnter))
			{
				return;
			}
			interactables.Add(interactableEnter);
			Debug.Log(string.Format("[{0}] interactable near {1}", typeof(CharacterController2D), interactableEnter.DebugInfo()));
		}
		
		if (!shellEquipped && other.gameObject.tag.Equals("Deadly"))
		{
			Die();
		}
	}
	
	private void FinishEquipShell()
	{
		shellEquipped = true;
	}

	private void OnCollisionStay2D(Collision2D other)
	{
		if (other.gameObject.tag.Equals("Deadly"))
		{
			Die();
		}
	}

	/// <summary>
	/// Called by other objects.
	/// </summary>
	public void Kill()
	{
		Die();
	}

	private void Die()
	{
		if (shellEquipped)
		{
			return;
		}
		
		Animator.SetTrigger("Die");

		SoundManager.Instance.PlaySound("die");

		transform.DOScale(0, 1f);
		
		Invoke("Respawn", 1.5f);

		isDead = true;
	}

	private void Respawn()
	{
		Animator.SetTrigger("Respawn");
		
		transform.DOMove(Shell.position, 1.0f).onComplete += FinishRespawn;
	}

	private void FinishRespawn()
	{
		isDead = false;

		transform.DOScale(0.1f, 0.5f);
		
		var shellController = Shell.GetComponent<ShellController>();
		shellController.SetAttachedToTurtle(DefaultShellPivot, FinishEquipShell);
		this.shellController = shellController;
	}

	void OnTriggerExit2D(Collider2D other)
	{
		Interactable interactableExit = other.GetComponent<Interactable>();
		if (interactableExit != null)
		{
			interactables.Remove(interactableExit);
			
			Debug.Log(string.Format("[{0}] interactable far {1}", typeof(CharacterController2D), interactableExit.DebugInfo()));
		}
	}

	private void ApplyGravity()
	{
		if (IsGrounded())
		{
			return;
		}

		if (IsDead())
		{
			m_Movement = Vector2.zero;
			return;
		}
		var increment = Gravity * Time.fixedDeltaTime;
		m_Movement += Vector2.down * increment;
	}
	
	private void FixedUpdate()
	{
		var nowGrounded = IsGrounded();
		if (grounded != nowGrounded)
		{
			grounded = nowGrounded;
			Animator.SetBool("Grounded", grounded);
			Animator.ResetTrigger("Jump");
		}
		
		ApplyGravity();
		
		Move(m_Movement);
		
		m_PreviousPosition = rigidBody.position;
		m_CurrentPosition = m_PreviousPosition + m_NextMovement;
		Velocity = (m_CurrentPosition - m_PreviousPosition) / Time.fixedDeltaTime;

		rigidBody.MovePosition(m_CurrentPosition);
		m_NextMovement = Vector2.zero;
	}


	public bool CheckVictory()
	{
		if (shellController == null)
		{
			return false;
		}

		List<CollectableItem.ItemID> requiredList = new List<CollectableItem.ItemID>()
		{
			CollectableItem.ItemID.CollectableBottle,
			CollectableItem.ItemID.CollectableCrocs,
			CollectableItem.ItemID.CollectableFrame,
			CollectableItem.ItemID.CollectableHat,
			CollectableItem.ItemID.CollectableVhs
		};

		
		foreach (var itemId in requiredList)
		{
			if (inventory.Contains(itemId) == false)
			{
				return false;
			}
		}

		return true;
	}
}
