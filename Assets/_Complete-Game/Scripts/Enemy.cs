using UnityEngine;
using System.Collections;

namespace Completed
{
	//Enemy inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
	public class Enemy : MovingObject
	{
		public int playerDamage; 							//The amount of playerHealth points to subtract from the player when attacking.
		public AudioClip attackSound1;						//First of two audio clips to play when attacking the player.
		public AudioClip attackSound2;                      //Second of two audio clips to play when attacking the player.
		public int hp = 3;

		private SpriteRenderer spriteRenderer;
		private Animator animator;							//Variable of type Animator to store a reference to the enemy's Animator component.
		private Transform target;							//Transform to attempt to move toward each turn.
		private bool skipMove;								//Boolean to determine whether or not enemy should skip a turn or move this turn.
		
		
		//Start overrides the virtual Start function of the base class.
		protected override void Start ()
		{
			//Register this enemy with our instance of GameManager by adding it to a list of Enemy objects. 
			//This allows the GameManager to issue movement commands.
			GameManager.instance.AddEnemyToList (this);
			
			//Get and store a reference to the attached Animator component.
			animator = GetComponent<Animator> ();
			
			//Find the Player GameObject using it's tag and store a reference to its transform component.
			target = GameObject.FindGameObjectWithTag ("Player").transform;

			spriteRenderer = GetComponent<SpriteRenderer> ();
			
			//Call the start function of our base class MovingObject.
			base.Start ();
		}
		
		
		//Override the AttemptMove function of MovingObject to include functionality needed for Enemy to skip turns.
		//See comments in MovingObject for more on how base AttemptMove function works.
		
		protected override bool AttemptMove <T> (int xDir, int yDir)
		{
			//Check if skipMove is true, if so set it to false and skip this turn.
			if(skipMove && !GameManager.instance.enemiesFaster)
			{
				skipMove = false;
				return false;
			}
			
			//Call the AttemptMove function from MovingObject.
			base.AttemptMove <T> (xDir, yDir);
			
			//Now that Enemy has moved, set skipMove to true to skip next move.
			skipMove = true;
			return true;
		}
		


        //MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
        public void MoveEnemy ()
		{
			//Declare variables for X and Y axis move directions, these range from -1 to 1.
			//These values allow us to choose between the cardinal directions: up, down, left and right.
			int xDir = 0;
			int yDir = 0;
			
			// 난이도에 따라 향상된 AI를 Enemy에 적용.
			if (GameManager.instance.enemiesSmarter)
			{
				Debug.Log("향상된 AI 적용됨");
				int xHeading = (int)target.position.x - (int)transform.position.x;
				int yHeading = (int)target.position.y - (int)transform.position.y;
				bool moveOnX = false;

				// 목표와 자신 사이의 x좌표상 거리가 더 큰 경우 moveOnX를 True로 한다.
				if (Mathf.Abs(xHeading) >= Mathf.Abs(yHeading))
				{
					moveOnX = true;
				}
				for (int attempt = 0; attempt < 2; attempt++)
				{
					if (moveOnX == true && xHeading < 0)
					{
						xDir = -1;
						yDir = 0;
					}
					else if (moveOnX == true && xHeading > 0)
					{
						xDir = 1;
						yDir = 0;
					}
                    else if (moveOnX == false && yHeading < 0)
                    {
						xDir = 0;
						yDir = -1;
                    }
                    else if (moveOnX == false && yHeading > 0)
                    {
						xDir = 0;
						yDir = 1;
                    }

					Vector3 start = transform.position;
					Vector3 end = start + new Vector3(xDir, yDir);
					base.boxCollider.enabled = false;
					RaycastHit2D hit = Physics2D.Linecast(start, end, base.blockingLayer);
					base.boxCollider.enabled = true;
					if (hit.transform != null)
					{
						if (hit.transform.gameObject.tag == "Wall" || hit.transform.gameObject.tag == "Chest")
						{
							if (moveOnX == true)
							{
								moveOnX = false;
							}
							else
							{
								moveOnX = true;
							}
						}
					}
					else
					{
						break;
					}
                }
			}
			else
			{
                //If the difference in positions is approximately zero (Epsilon) do the following:
                if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)

                    //If the y coordinate of the target's (player) position is greater than the y coordinate of this enemy's position set y direction 1 (to move up). If not, set it to -1 (to move down).
                    yDir = target.position.y > transform.position.y ? 1 : -1;

                //If the difference in positions is not approximately zero (Epsilon) do the following:
                else
                    //Check if target x position is greater than enemy's x position, if so set x direction to 1 (move right), if not set to -1 (move left).
                    xDir = target.position.x > transform.position.x ? 1 : -1;
            }
			//Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player
			AttemptMove <Player> (xDir, yDir);
		}


        //OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
        //and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
        protected override void OnCantMove<T>(T component)
        {
			

			//Declare hitPlayer and set it to equal the encountered component.
			Player hitPlayer = component as Player;

            // 호출 시 순서 문제로 NullException 오류 발생을 조건문에서 확인 후 반환
			if (component == default(T))
			{
				return;
			}

            //Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
            hitPlayer.LoseFood (playerDamage);
			
			//Set the attack trigger of animator to trigger Enemy attack animation.
			animator.SetTrigger ("enemyAttack");
			
			//Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
			SoundManager.instance.RandomizeSfx (attackSound1, attackSound2);
		}

		// 적의 체력 피해 처리
        public void DamageEnemy(int loss)
        {
            hp -= loss;
            //If hit points are less than or equal to zero:
            if (hp <= 0)
            {
                //Disable the gameObject.
                gameObject.SetActive(false);
            }
        }

        public SpriteRenderer getSpriteRenderer()
		{
			return spriteRenderer;
		}
	}
}
