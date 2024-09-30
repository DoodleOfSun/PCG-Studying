using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using System.Collections.Generic;   // 딕셔너리 자료구조 사용
using UnityEngine.EventSystems;
using System.ComponentModel;

namespace Completed
{
	//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
	public class Player : MovingObject
	{
		public float restartLevelDelay = 1f;		//Delay time in seconds to restart level.
		public int pointsPerFood = 10;				//Number of points to add to player playerHealth points when picking up a playerHealth object.
		public int pointsPerSoda = 20;				//Number of points to add to player playerHealth points when picking up a soda object.
		public int wallDamage = 1;					//How much damage a player does to a wall when chopping it.
		public Text foodText;						//UI Text to display current player playerHealth total.
		public AudioClip moveSound1;				//1 of 2 Audio clips to play when player moves.
		public AudioClip moveSound2;				//2 of 2 Audio clips to play when player moves.
		public AudioClip eatSound1;					//1 of 2 Audio clips to play when player collects a playerHealth object.
		public AudioClip eatSound2;					//2 of 2 Audio clips to play when player collects a playerHealth object.
		public AudioClip drinkSound1;				//1 of 2 Audio clips to play when player collects a soda object.
		public AudioClip drinkSound2;				//2 of 2 Audio clips to play when player collects a soda object.
		public AudioClip gameOverSound;             //Audio clip to play when player dies.
		public static Vector2 savingPlayerPosition;             // 플레이어의 현재 위치를 저장
		public bool onWorldBoard;
		public bool dungeonTransition;
        public int playerHealth;                    //Used to store player playerHealth points total during level.


        // 아이템과 상호작용
        public Image gloveImage;
		public Image bootImage;
		public int attackMod, defenseMod = 0;



		private Animator animator;					//Used to store a reference to the Player's animator component.
		private Dictionary<string, Item> inventory;
		
		
		//Start overrides the Start function of MovingObject
		protected override void Start ()
		{
			//Get a component reference to the Player's animator component
			animator = GetComponent<Animator>();

			//Get the current playerHealth point total stored in GameManager.instance between levels.
			//playerHealth = GameManager.instance.playerHealth;
			GameManager.instance.playerHealth = playerHealth;

			//Set the foodText to reflect the current player playerHealth total.
			foodText.text = "Health : " + playerHealth;

			savingPlayerPosition.x = savingPlayerPosition.y = 2;

			onWorldBoard = true;
			dungeonTransition = false;

			inventory = new Dictionary<string, Item>();
			
			
			//Call the Start function of the MovingObject base class.
			base.Start ();
		}
		
		
		//This function is called when the behaviour becomes disabled or inactive.
		private void OnDisable ()
		{
			//When Player object is disabled, store the current local playerHealth total in the GameManager so it can be re-loaded in next level.
			
			// Player 비활성화되면 Null Exception 오류가 발생. 유니티 상 호출 순서의 문제로 보이는데 이를 조건문으로 체크하여 해결함.
			if (GameManager.instance == null)
			{
				return;
			}
            GameManager.instance.playerHealth = playerHealth;
		}
		
		
		private void Update ()
		{
			// 난이도 결정
			AdaptDifficulty();

			//If it's not the player's turn, exit the function.
			if(!GameManager.instance.playersTurn) return;
			
			int horizontal = 0;  	//Used to store the horizontal move direction.
			int vertical = 0;       //Used to store the vertical move direction.

			bool canPlayerMove = false;

			//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
			horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
			
			//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
			vertical = (int) (Input.GetAxisRaw ("Vertical"));

            //Check if moving horizontally, if so set vertical to zero.
            if (horizontal != 0)
			{
				vertical = 0;
			}

			if(horizontal != 0 || vertical != 0)
			{
				if (!dungeonTransition)
				{
                    // 플레이어가 상호작용할 타일 종류를 레이캐스트로 뽑아내 switch문에서 AttemptMove를 올바르게 호출
                    
					if(!dungeonTransition){

						// 레이캐스트 발사하기
						Vector2 start = transform.position;
						Vector2 end = start + new Vector2(horizontal, vertical);
						base.boxCollider.enabled = false;
						RaycastHit2D hit = Physics2D.Linecast(start,end, base.blockingLayer);
						base.boxCollider.enabled = true;

						// 레이캐스트 체크 부분
						if (hit.transform != null)
                        {
                            switch (hit.transform.gameObject.tag)
							{
								case "Wall":
									canPlayerMove = AttemptMove<Wall>(horizontal, vertical);
									break;
                                case "Chest":
									canPlayerMove = AttemptMove<Chest>(horizontal, vertical);
                                    break;
                                case "Enemy":
									canPlayerMove = AttemptMove<Enemy>(horizontal, vertical);
                                    break;
                                case "Blocking":
                                    break;
                            }
						}
						// 레이캐스트 충돌 없는 경우 Wall로 AttemptMove 호출
						else{
							canPlayerMove= AttemptMove<Wall>(horizontal, vertical);
						}
					}

                    if (canPlayerMove && onWorldBoard)
					{
						savingPlayerPosition.x += horizontal;
						savingPlayerPosition.y += vertical;
						GameManager.instance.updateBoard(horizontal, vertical);
					}
					
				}
			}
		}

		// 새로운 AttemptMove
		protected override bool AttemptMove<T>(int xDir, int yDir)
		{
			bool hit = base.AttemptMove<T>(xDir, yDir);

			GameManager.instance.playersTurn = false;

			return hit;
		}


        //OnCantMove overrides the abstract function OnCantMove in MovingObject.
        //It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
        protected override void OnCantMove<T>(T component)
        {
			if (typeof(T) == typeof(Wall))
			{
                Wall blockingObj = component as Wall;
				blockingObj.DamageWall(wallDamage);
			}
			else if (typeof(T) == typeof(Chest))
			{
				Chest blockingObj = component as Chest;
				blockingObj.Open();
			}
			else if (typeof(T) == typeof(Enemy))
			{
				Enemy blockingObj = component as Enemy;
				blockingObj.DamageEnemy(wallDamage);
			}

			animator.SetTrigger("playerChop");
		}
		
		private void AdaptDifficulty()
		{
			if (wallDamage >= 10)
			{
				GameManager.instance.enemiesSmarter = true;
			}
			if (wallDamage >= 15)
			{
				GameManager.instance.enemiesFaster = true;
			}
			if (wallDamage >= 20)
			{
				GameManager.instance.enemySpawnRatio = 5;
			}
		}
		
		// 충돌 시 이벤트 발생
		private void OnTriggerEnter2D (Collider2D other)
		{
			if(other.tag == "Exit")
			{
				dungeonTransition = true;
				Invoke("GoDungeonPortal", 0.5f);
				Destroy(other.gameObject);
			}
			
			else if(other.tag == "Food" || other.tag == "Soda")
			{
				UpdateHealth(other);
				Destroy(other.gameObject);
			}

			else if (other.tag == "Item")
			{
				UpdateInventory(other);
				Destroy(other.gameObject);
			}
		}
		
		//Restart reloads the scene when called.
		private void Restart ()
		{
			//Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
            //and not load all the scene object in the current scene.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
		}
		
		
		//LoseFood is called when an enemy attacks the player.
		//It takes a parameter loss which specifies how many points to lose.
		public void LoseFood (int loss)
		{
			//Set the trigger for the player animator to transition to the playerHit animation.
			animator.SetTrigger ("playerHit");
			
			//Subtract lost playerHealth points from the players total.
			playerHealth -= loss;
			
			//Update the playerHealth display with the new total.
			foodText.text = "-"+ loss + " Health : " + playerHealth;
			
			//Check to see if game has ended.
			CheckIfGameOver ();
		}
		
		
		//CheckIfGameOver checks if the player is out of playerHealth points and if so, ends the game.
		private void CheckIfGameOver ()
		{
			//Check if playerHealth point total is less than or equal to zero.
			if (playerHealth <= 0) 
			{
				//Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
				SoundManager.instance.PlaySingle (gameOverSound);
				
				//Stop the background music.
				SoundManager.instance.musicSource.Stop();
				
				//Call the GameOver function of GameManager.
				GameManager.instance.GameOver ();

				// 게임 오버될때 자꾸 오류나서 추가 안되면 다시 점검
				gameObject.SetActive(false);
			}
		}

		private void UpdateHealth(Collider2D item)
		{
			if (playerHealth < 100)
			{
				if (item.tag == "Food")
				{
					playerHealth += Random.Range(1, 4);
				}
				else
				{
					playerHealth += Random.Range(4, 11);
				}

				GameManager.instance.playerHealth = playerHealth;
				foodText.text = "Health : " + playerHealth;
			}
		}

		private void UpdateInventory(Collider2D item)
		{
			Item itemData = item.GetComponent<Item>();
			switch (itemData.equipmentType)
			{
				case itemType.glove:

					if (!inventory.ContainsKey("glove"))
					{
						inventory.Add("glove", itemData);
					}

					else
					{
						inventory["glove"] = itemData;
					}
					gloveImage.color = itemData.color;
					break;

				case itemType.boot:
                    if (!inventory.ContainsKey("boot"))
                    {
						inventory.Add("boot", itemData);
                    }

                    else
                    {
						inventory["boot"] = itemData;
                    }
                    bootImage.color = itemData.color;
                    break;
			}

			attackMod = 0;
			defenseMod = 0;

			foreach (KeyValuePair<string, Item> gear in inventory)
			{
				attackMod += gear.Value.attackMod;
				defenseMod += gear.Value.defenseMod;

			}
		}

		private void GoDungeonPortal()
		{
			if (onWorldBoard)
			{
				onWorldBoard = false;
				GameManager.instance.enterDungeon();
				transform.position = DungeonManager.startPos;
			}
			else
			{
				onWorldBoard = true;
				GameManager.instance.exitDungeon();
				transform.position = savingPlayerPosition;
			}
		}
	}
}

