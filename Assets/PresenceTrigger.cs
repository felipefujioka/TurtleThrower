﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TurtleThrower
{
    [RequireComponent(typeof(Collider2D))]
    public class PresenceTrigger : MonoBehaviour
    {
        public LayerMask layers;
        public Collider2D trigger;
        
        public UnityEvent OnEnter;
        public UnityEvent OnExit;

        public bool Turtle;
        public bool Shell;
        public bool Item;
        public CollectableItem.ItemID ItemId;

        public GameObject DestroyOnOpen;
        
        void Reset()
        {
            layers = LayerMask.NameToLayer("Everything");
        }
        
        private void Awake()
        {
            if (trigger == null)
            {
                trigger = GetComponent<Collider2D>();
            }
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            bool dispatchOnEnter = false;
            
            CharacterController2D turtleController2D = other.GetComponent<CharacterController2D>();
            if (turtleController2D && Turtle)
            {
                if (Item)
                {
                    var inventory = turtleController2D.GetComponent<Inventory>();
                    if (inventory != null && inventory.Contains(ItemId))
                    {
                        dispatchOnEnter = true;
                    }
                }
                else
                {
                    dispatchOnEnter = true;    
                }
                
            }

            ShellController shellController = other.GetComponent<ShellController>();
            if (shellController && Shell)
            {
                dispatchOnEnter = true;
            }
            
            if (dispatchOnEnter)
            {
                ExecuteOnEnter(other);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            bool shouldExit = false;
            
            CharacterController2D turtleController2D = other.GetComponent<CharacterController2D>();
            if (turtleController2D && Turtle)
            {
                if (Item)
                {
                    var inventory = turtleController2D.GetComponent<Inventory>();
                    if (inventory != null && inventory.Contains(ItemId))
                    {
                        shouldExit = true;
                    }
                }
                else
                {
                    shouldExit = true;
                }
                
                shouldExit = true;
            }

            ShellController shellController = other.GetComponent<ShellController>();
            if (shellController && Shell)
            {
                shouldExit = true;
            }

            if (shouldExit)
            {
                ExecuteOnExit(other);
            }
        }
        
        protected virtual void ExecuteOnEnter(Collider2D other)
        {
            OnEnter.Invoke();
            if (DestroyOnOpen != null)
            {
                Destroy(DestroyOnOpen);   
            }
        }

        protected virtual void ExecuteOnExit(Collider2D other)
        {
            OnExit.Invoke();
        }
    }


}
