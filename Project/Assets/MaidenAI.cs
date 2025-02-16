using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class MaidenAI : MonoBehaviour
{
	// Componente
	private SphereCollider AICollider;
	private Animator AIAnimator;
	private Transform AITransform;

	// Random pentru atac
	private Random rand = new Random();

	[Header("Detection & Movement")]
	[SerializeField] private float seeRadius = 7f;        // raza de "văzut" (sfera)
	[SerializeField] private float movingSpeed = 0.6f;    // viteza de deplasare
	[SerializeField] private float engageDistance = 1.5f; // distanța la care se oprește înainte de inamic
	[SerializeField] private float runDistance = 2.5f;    // dacă e mai departe => run
	[SerializeField] private float runawayDistance = 5f;  // distanța minimă față de aliat (nu se înghesuie)

	[Header("Combat & Health")]
	[SerializeField] private float health = 100f;         // viața AI
	[SerializeField] private bool hasDied = false;        // devine true când moare

	// Stări interne
	[SerializeField] private bool isWalking = false;
	[SerializeField] private bool isRunning = false;
	[SerializeField] private bool isEngaged = false;

	// Denumiri animații
	private const string AI_IDLE = "idle1 0";
	private const string AI_WALK = "walk";
	private const string AI_RUN = "run1_fbx";

	// Atacuri posibile (2 animații)
	private string[] AI_ATTACKS = { "attack1_mb", "attack2_mb" };

	// Moarte: alege random death1/death2
	private string[] AI_DEATHS = { "death1", "death2" };

	// Starea curentă de animație
	private string currentState;

	// Dicționar obiecte din trigger:
	// 0 = inamici, 1 = aliați, 2 = altele
	private Dictionary<int, List<GameObject>> _closeTriggers = new Dictionary<int, List<GameObject>>();

	private void Awake()
	{
		// Luăm componente
		AICollider = GetComponent<SphereCollider>();
		AIAnimator = GetComponent<Animator>();
		AITransform = transform;

		// Inițializăm liste
		_closeTriggers[0] = new List<GameObject>(); // inamici
		_closeTriggers[1] = new List<GameObject>(); // aliați
		_closeTriggers[2] = new List<GameObject>(); // diverse
	}

	private void Start()
	{
		// Setăm raza de "vedere"
		if (AICollider != null)
		{
			AICollider.radius = seeRadius;
			AICollider.isTrigger = true;
		}
	}

	private void FixedUpdate()
	{
		// Dacă a murit, nu mai face nimic
		if (hasDied) return;

		// 1) Căutăm inamicii (cheia 0)
		if (_closeTriggers.ContainsKey(0) && _closeTriggers[0].Count > 0)
		{
			isWalking = true;

			// găsim inamicul cel mai apropiat
			GameObject closestEnemy = null;
			float closestDistance = float.MaxValue;
			foreach (var enemy in _closeTriggers[0])
			{
				float dist = Vector3.Distance(AITransform.position, enemy.transform.position);
				if (dist < closestDistance)
				{
					closestDistance = dist;
					closestEnemy = enemy;
				}
			}

			if (closestEnemy == null) return;

			// dacă e mai departe decât runDistance => run
			float distToEnemy = Vector3.Distance(closestEnemy.transform.position, AITransform.position);
			isRunning = (distToEnemy >= runDistance);

			// ne uităm spre inamic
			Vector3 enemyPosSameY = new Vector3(
				closestEnemy.transform.position.x,
				AITransform.position.y,
				closestEnemy.transform.position.z
			);
			// rotire smooth
			AITransform.LookAt(Vector3.Lerp(
				AITransform.position + AITransform.forward,
				enemyPosSameY,
				0.05f
			));

			// ne apropiem până la engageDistance
			AITransform.position = Vector3.MoveTowards(
				AITransform.position,
				enemyPosSameY - AITransform.forward * engageDistance,
				movingSpeed * (isRunning ? 2.2f : 1f) * Time.fixedDeltaTime
			);

			// dacă suntem < 1.0f distanță => engaged
			isEngaged = (Vector3.Distance(AITransform.position, enemyPosSameY) < 1.0f);
		}
		else
		{
			// niciun inamic
			isWalking = false;
			isEngaged = false;
		}

		// 2) Verificăm aliații (cheia 1) => ne ferim să nu ne suprapunem
		if (_closeTriggers.ContainsKey(1) && _closeTriggers[1].Count > 0)
		{
			foreach (var ally in _closeTriggers[1])
			{
				float allyDist = Vector3.Distance(AITransform.position, ally.transform.position);
				if (allyDist < runawayDistance)
				{
					// Ne mișcăm puțin în direcția opusă
					Vector3 allyPosSameY = new Vector3(
						ally.transform.position.x,
						AITransform.position.y,
						ally.transform.position.z
					);
					Vector3 direction = AITransform.position - allyPosSameY;
					direction.y = 0;
					direction.Normalize();

					AITransform.position = Vector3.MoveTowards(
						AITransform.position,
						AITransform.position + direction,
						movingSpeed * Time.fixedDeltaTime
					);
				}
			}
		}
	}

	private void Update()
	{
		if (hasDied) return;

		// dacă HP <= 0 => moare
		if (health <= 0 && !hasDied)
		{
			Die();
			return;
		}

		// animații
		AIAnimationLogic();
	}

	void AIAnimationLogic()
	{
		// Stări de bază
		if (isWalking && !isRunning && !isEngaged)
		{
			ChangeAnimationState(AI_WALK);
		}
		else if (isRunning && !isEngaged)
		{
			ChangeAnimationState(AI_RUN);
		}
		else if (!isWalking && !isRunning && !isEngaged)
		{
			ChangeAnimationState(AI_IDLE);
		}

		// Atac dacă e engaged
		if (isEngaged)
		{
			var stateInfo = AIAnimator.GetCurrentAnimatorStateInfo(0);
			// așteptăm să se termine animația curentă
			if (stateInfo.length <= stateInfo.normalizedTime)
			{
				// Alege random: attack1_mb / attack2_mb
				int idx = rand.Next(0, AI_ATTACKS.Length);
				string chosenAttack = AI_ATTACKS[idx];
				ChangeAnimationState(chosenAttack);
			}
		}
	}

	// =============== DETECTARE TRIGGER ===============
	private void OnTriggerEnter(Collider other)
	{
		// player/maincamera => inamic
		if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
		{
			if (!_closeTriggers[0].Contains(other.gameObject))
				_closeTriggers[0].Add(other.gameObject);
		}
		// ally => aliat
		else if (other.CompareTag("Ally"))
		{
			if (!_closeTriggers[1].Contains(other.gameObject))
				_closeTriggers[1].Add(other.gameObject);
		}
		else
		{
			// alt tag => 2
			if (!_closeTriggers[2].Contains(other.gameObject))
				_closeTriggers[2].Add(other.gameObject);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		// Scoatem obiectul care iese
		foreach (var kvp in _closeTriggers)
		{
			if (kvp.Value.Contains(other.gameObject))
			{
				kvp.Value.Remove(other.gameObject);
				break;
			}
		}
	}

	// =============== DAMAGE & DEATH ===============
	public void TakeDamage(float dmg)
	{
		if (hasDied) return;
		health -= dmg;
		if (health <= 0 && !hasDied)
		{
			Die();
		}
	}

	private void Die()
	{
		hasDied = true;
		// random death1 sau death2
		int idx = rand.Next(0, AI_DEATHS.Length);
		string chosenDeath = AI_DEATHS[idx];

		ChangeAnimationState(chosenDeath);

		// dezactivăm colliderul
		if (AICollider) AICollider.enabled = false;
	}

	// =============== HELPER ANIMAȚII ===============
	void ChangeAnimationState(string newState)
	{
		if (currentState == newState) return;
		AIAnimator.Play(newState);
		currentState = newState;
	}

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!AICollider) AICollider = GetComponent<SphereCollider>();
        if (AICollider)
        {
            AICollider.isTrigger = true;
            AICollider.radius = seeRadius;
        }
    }
#endif
}
