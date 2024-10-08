﻿using UnityEngine;
using System.Collections;

namespace Completed
{
	//The abstract keyword enables you to create classes and class members that are incomplete and must be implemented in a derived class.
	public abstract class MovingObject : MonoBehaviour
	{
		public float moveTime = 0.1f;			//Time it will take object to move, in seconds.
		public LayerMask blockingLayer;			//Layer on which collision will be checked.

		protected BoxCollider2D boxCollider;	//The BoxCollider2D component attached to this object.

		private Rigidbody2D rb2D;				//The Rigidbody2D component attached to this object.
		private float inverseMoveTime;			//Used to make movement more efficient.
		private bool isMoving;					//Is the object currently moving.
		
		
		//Protected, virtual functions can be overridden by inheriting classes.
		protected virtual void Start ()
		{
			//Get a component reference to this object's BoxCollider2D
			boxCollider = GetComponent <BoxCollider2D> ();
			
			//Get a component reference to this object's Rigidbody2D
			rb2D = GetComponent <Rigidbody2D> ();
			
			//By storing the reciprocal of the move time we can use it by multiplying instead of dividing, this is more efficient.
			inverseMoveTime = 1f / moveTime;
		}
		
		
		//Move returns true if it is able to move and false if not. 
		//Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
		protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
		{
			//Store start position to move from, based on objects current transform position.
			Vector2 start = transform.position;
			
			// Calculate end position based on the direction parameters passed in when calling Move.
			Vector2 end = start + new Vector2 (xDir, yDir);
			
			//Disable the boxCollider so that linecast doesn't hit this object's own collider.
			boxCollider.enabled = false;
			
			//Cast a line from start point to end point checking collision on blockingLayer.
			hit = Physics2D.Linecast (start, end, blockingLayer);
			
			//Re-enable boxCollider after linecast
			boxCollider.enabled = true;
			
			//Check if nothing was hit and that the object isn't already moving.
			if(hit.transform == null && !isMoving)
			{
				// 부드러운 움직임
				//StartCoroutine (SmoothMovement (end));
				// 순간이동
				StartCoroutine(HardMovement(end));

				//Return true to say that Move was successful
				return true;
			}
			
			//If something was hit, return false, Move was unsuccesful.
			return false;
		}
		
		
		//Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
		protected IEnumerator SmoothMovement (Vector3 end)
		{
			//The object is now moving.
			isMoving = true;
			
			//Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
			//Square magnitude is used instead of magnitude because it's computationally cheaper.
			float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            
			//While that distance is greater than a very small amount (Epsilon, almost zero):
			while(sqrRemainingDistance > float.Epsilon)
			{
				//Find a new position proportionally closer to the end, based on the moveTime
				Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
				
				//Call MovePosition on attached Rigidbody2D and move it to the calculated position.
				rb2D.MovePosition (newPostion);
				
				//Recalculate the remaining distance after moving.
				sqrRemainingDistance = (transform.position - end).sqrMagnitude;
				
				//Return and loop until sqrRemainingDistance is close enough to zero to end the function
				yield return null;
			}
			
			
			//Make sure the object is exactly at the end of its movement.
			rb2D.MovePosition (end);
			
			//The object is no longer moving.
			isMoving = false;
			

        }

		// SmoothMovement에서 while문 제거후 순간이동하듯이 이동
		protected IEnumerator HardMovement(Vector3 end)
		{

            isMoving = true;
            float sqrRemainingDistance = (transform.position - end).sqrMagnitude;


            Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);


            rb2D.MovePosition(newPostion);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            rb2D.MovePosition(end);

            isMoving = false;

            yield return null;
        }

		// 베이스 AttemptMove. 이 함수의 내용 자체를 호출시 base.AttemptMove... 로 호출
		// 단, 사용하고자 하는 클래스 안에서 오버라이드가 되어있어야 함
		protected virtual bool AttemptMove <T>(int xDir, int yDir)
            where T : Component
        {
			// 플레이어의 방향 전환
			if (xDir == 1)
			{
				transform.eulerAngles = Vector3.zero;
			}
			else if (xDir == -1)
			{
				transform.eulerAngles = new Vector3(0, 180, 0);
			}

			RaycastHit2D hit;

            bool canMove = Move(xDir, yDir, out hit);

			if(hit.transform == null)
			{
				return true;
			}

			else
			{
                T hitComponent = hit.transform.GetComponent<T>();
                OnCantMove(hitComponent);
                return false;
            }

        }

        //The abstract modifier indicates that the thing being modified has a missing or incomplete implementation.
        //OnCantMove will be overriden by functions in the inheriting classes.
        protected abstract void OnCantMove <T> (T component)
			where T : Component;
	}
}
