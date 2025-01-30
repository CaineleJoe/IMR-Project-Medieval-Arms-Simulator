using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwordControllerV2 : MonoBehaviour
{
	[Header("VR Controller Reference")]
	public Transform controllerTransform;

	[Header("Spring-Damper Settings")]
	public float positionStiffness = 300f;
	public float positionDamping = 20f;

	public float rotationStiffness = 50f;
	public float rotationDamping = 5f;

	[Header("Collision Feedback")]
	public AudioSource clashSound;

	private Rigidbody rb;
	private bool isColliding = false;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		rb.interpolation = RigidbodyInterpolation.Interpolate;
	}

	void FixedUpdate()
	{
		if (controllerTransform == null)
			return;

		// --- Position Spring-Damper ---
		Vector3 currentPos = rb.position;
		Vector3 targetPos = controllerTransform.position;
		Vector3 posError = targetPos - currentPos;

		Vector3 currentVel = rb.velocity;
		Vector3 force = (posError * positionStiffness) - (currentVel * positionDamping);
		rb.AddForce(force, ForceMode.Acceleration);

		// --- Rotation Spring-Damper ---
		// Only apply rotation correction if not colliding, or apply it less aggressively when colliding.
		if (!isColliding)
		{
			// Full rotation correction
			ApplyRotationCorrection();
		}
		else
		{
			// During collision, we either skip rotation correction or apply very gentle correction
			// This prevents the sword from flipping or rotating awkwardly.
			// Option 1: Skip entirely:
			// (Do nothing here.)

			// Option 2: Apply weaker correction:
			// ApplyRotationCorrection(0.1f); // Pass a factor to reduce stiffness
		}
	}

	private void ApplyRotationCorrection(float stiffnessFactor = 1f)
	{
		Quaternion currentRot = rb.rotation;
		Quaternion targetRot = controllerTransform.rotation;

		Quaternion deltaRot = targetRot * Quaternion.Inverse(currentRot);
		deltaRot.ToAngleAxis(out float angle, out Vector3 axis);

		if (angle > 180f) angle -= 360f;
		float angleRad = angle * Mathf.Deg2Rad;

		Vector3 angularVelocity = rb.angularVelocity;
		Vector3 torque = (axis * angleRad * rotationStiffness * stiffnessFactor) - (angularVelocity * rotationDamping * stiffnessFactor);
		rb.AddTorque(torque, ForceMode.Acceleration);
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.CompareTag("Sword"))
		{
			isColliding = true;
			if (clashSound != null)
				clashSound.Play();
		}
	}

	void OnCollisionExit(Collision collision)
	{
		if (collision.collider.CompareTag("Sword"))
		{
			isColliding = false;
		}
	}
}