using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    public class GhostScript : MonoBehaviour
    {
        private Animator Anim;
        private CharacterController Ctrl;
        private Vector3 MoveDirection = Vector3.zero;
        // Cache hash values
        private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
        private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
        private static readonly int SurprisedState = Animator.StringToHash("Base Layer.surprised");
        private static readonly int AttackState = Animator.StringToHash("Base Layer.attack_shift");
        private static readonly int DissolveState = Animator.StringToHash("Base Layer.dissolve");
        private static readonly int AttackTag = Animator.StringToHash("Attack");

        // dissolve
        [SerializeField] private SkinnedMeshRenderer[] MeshR;
        private float Dissolve_value = 1;
        private bool DissolveFlg = false;
        private const int maxHP = 3;
        private int HP = maxHP;
        private Text HP_text;

        // moving speed
        [SerializeField] private float Speed = 4;

        // Player reference
        private Transform player;

        // Distance variables
        public float chaseDistance = 10f;  // Distance to start chasing the player
        public float attackDistance = 2f;  // Distance to "touch" and "kill" the player

        void Start()
        {
            Anim = this.GetComponent<Animator>();
            Ctrl = this.GetComponent<CharacterController>();
            HP_text = GameObject.Find("Canvas/HP").GetComponent<Text>();
            HP_text.text = "HP " + HP.ToString();

            // Get the player's transform (make sure player has the correct tag)
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        void Update()
        {
            STATUS();
            GRAVITY();
            Respawn();

            // If player is not dead, attempt to follow and kill them
            if (player != null)
            {
                Vector3 directionToPlayer = player.position - transform.position;
                directionToPlayer.y = 0; // Ensure we only move horizontally

                // Follow the player if they are within the chase distance
                if (directionToPlayer.magnitude <= chaseDistance)
                {
                    MoveTowardsPlayer(directionToPlayer);
                }

                // If close enough, attempt to "touch" the player (kill them)
                if (directionToPlayer.magnitude <= attackDistance)
                {
                    AttackPlayer();
                }

                // Handle the various statuses of the player (such as dissolve or attack)
                if (!PlayerStatus.ContainsValue(true))
                {
                    MOVE();
                    PlayerAttack();
                }
                else
                {
                    // Here, we directly check and handle player status
                    int status_name = 0;
                    foreach (var i in PlayerStatus)
                    {
                        if (i.Value == true)
                        {
                            status_name = i.Key;
                            break;
                        }
                    }

                    // Handle specific player statuses
                    if (status_name == Dissolve)
                    {
                        PlayerDissolve();
                    }
                    else if (status_name == Attack)
                    {
                        PlayerAttack();
                    }
                    else if (status_name == Surprised)
                    {
                        // Do nothing, the player is surprised
                    }
                }
            }

            // Dissolve logic if the ghost is defeated
            if (HP <= 0 && !DissolveFlg)
            {
                Anim.CrossFade(DissolveState, 0.1f, 0, 0);
                DissolveFlg = true;
            }
            else if (HP == maxHP && DissolveFlg)
            {
                DissolveFlg = false;
            }
        }

        //---------------------------------------------------------------------
        // character status handling
        //---------------------------------------------------------------------
        private const int Dissolve = 1;
        private const int Attack = 2;
        private const int Surprised = 3;
        private Dictionary<int, bool> PlayerStatus = new Dictionary<int, bool>
        {
            {Dissolve, false },
            {Attack, false },
            {Surprised, false },
        };

        private void STATUS()
        {
            // during dissolve
            if (DissolveFlg && HP <= 0)
            {
                PlayerStatus[Dissolve] = true;
            }
            else if (!DissolveFlg)
            {
                PlayerStatus[Dissolve] = false;
            }
            // during attacking
            if (Anim.GetCurrentAnimatorStateInfo(0).tagHash == AttackTag)
            {
                PlayerStatus[Attack] = true;
            }
            else if (Anim.GetCurrentAnimatorStateInfo(0).tagHash != AttackTag)
            {
                PlayerStatus[Attack] = false;
            }
            // during surprised (when damaged or stunned)
            if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash == SurprisedState)
            {
                PlayerStatus[Surprised] = true;
            }
            else if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash != SurprisedState)
            {
                PlayerStatus[Surprised] = false;
            }
        }

        //---------------------------------------------------------------------
        // Gravity for the ghost falling
        //---------------------------------------------------------------------
        private void GRAVITY()
        {
            if (Ctrl.enabled)
            {
                if (CheckGrounded())
                {
                    if (MoveDirection.y < -0.1f)
                    {
                        MoveDirection.y = -0.1f;
                    }
                }
                MoveDirection.y -= 0.1f;
                Ctrl.Move(MoveDirection * Time.deltaTime);
            }
        }

        //---------------------------------------------------------------------
        // Check if the ghost is grounded
        //---------------------------------------------------------------------
        private bool CheckGrounded()
        {
            if (Ctrl.isGrounded && Ctrl.enabled)
            {
                return true;
            }
            Ray ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);
            float range = 0.2f;
            return Physics.Raycast(ray, range);
        }

        //---------------------------------------------------------------------
        // Move the ghost towards the player
        //---------------------------------------------------------------------
        private void MoveTowardsPlayer(Vector3 directionToPlayer)
        {
            // Move the ghost towards the player
            MoveDirection = directionToPlayer.normalized * Speed;
            Ctrl.Move(MoveDirection * Time.deltaTime);
            Anim.CrossFade(MoveState, 0.1f, 0, 0);
        }

        //---------------------------------------------------------------------
        // Attack the player when close enough
        //---------------------------------------------------------------------
        private void AttackPlayer()
        {
            Anim.CrossFade(AttackState, 0.1f, 0, 0);
            // Here you can add any logic for the ghost attacking the player, such as damage or death
            // For now, we just play an attack animation.
        }

        //---------------------------------------------------------------------
        // Handle respawn logic if necessary
        //---------------------------------------------------------------------
        private void Respawn()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                HP = maxHP;
                Ctrl.enabled = false;
                this.transform.position = Vector3.zero; // Reset position
                this.transform.rotation = Quaternion.Euler(Vector3.zero); // Reset rotation
                Ctrl.enabled = true;

                Dissolve_value = 1;
                for (int i = 0; i < MeshR.Length; i++)
                {
                    MeshR[i].material.SetFloat("_Dissolve", Dissolve_value);
                }

                Anim.CrossFade(IdleState, 0.1f, 0, 0);
            }
        }

        //---------------------------------------------------------------------
        // Movement when user presses arrow keys
        //---------------------------------------------------------------------
        private void MOVE()
        {
            // Handle ghost movement with arrow keys, same as the previous logic
            if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash == MoveState)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    MOVE_Velocity(new Vector3(0, 0, -Speed), new Vector3(0, 180, 0));
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    MOVE_Velocity(new Vector3(0, 0, Speed), new Vector3(0, 0, 0));
                }
                else if (Input.GetKey(KeyCode.LeftArrow))
                {
                    MOVE_Velocity(new Vector3(Speed, 0, 0), new Vector3(0, 90, 0));
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    MOVE_Velocity(new Vector3(-Speed, 0, 0), new Vector3(0, 270, 0));
                }
            }
        }

        private void MOVE_Velocity(Vector3 velocity, Vector3 rot)
        {
            MoveDirection = new Vector3(velocity.x, MoveDirection.y, velocity.z);
            if (Ctrl.enabled)
            {
                Ctrl.Move(MoveDirection * Time.deltaTime);
            }
            this.transform.rotation = Quaternion.Euler(rot);
        }

        // Player dissolve handling
        private void PlayerDissolve()
        {
            Dissolve_value -= Time.deltaTime;
            for (int i = 0; i < MeshR.Length; i++)
            {
                MeshR[i].material.SetFloat("_Dissolve", Dissolve_value);
            }

            if (Dissolve_value <= 0)
            {
                HP = 0;  // The ghost has dissolved (or died)
            }
        }

        // Handle player's attack status
        private void PlayerAttack()
        {
            // Logic when the player attacks (e.g., ghost gets stunned or hit back)
            Anim.CrossFade(SurprisedState, 0.1f, 0, 0);
        }
    }
}
