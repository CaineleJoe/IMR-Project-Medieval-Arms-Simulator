using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class SwordController : MonoBehaviour
{
	[Header("Spring-Damper Settings")]
	public float positionStiffness = 300f;
	public float positionDamping = 20f;

	public float rotationStiffness = 50f;
	public float rotationDamping = 5f;

	[Header("Collision Feedback")]
	public AudioSource clashSound;

	private Rigidbody rb;
	private XRGrabInteractable grabInteractable;
	private Transform controllerTransform; // Dynamically assigned at runtime
	private bool isGrabbed = false;
	private bool isColliding = false;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		rb.interpolation = RigidbodyInterpolation.Interpolate;

		grabInteractable = GetComponent<XRGrabInteractable>();

		// Automatically hook into grab/release events
		grabInteractable.selectEntered.AddListener(OnGrab);
		grabInteractable.selectExited.AddListener(OnRelease);
	}

	void FixedUpdate()
	{
		// If not grabbed, do nothing special (just normal physics).
		if (!isGrabbed || controllerTransform == null)
			return;

		// --- Position Spring-Damper ---
		Vector3 currentPos = rb.position;
		Vector3 targetPos = controllerTransform.position;
		Vector3 posError = targetPos - currentPos;

		Vector3 currentVel = rb.velocity;
		Vector3 force = (posError * positionStiffness) - (currentVel * positionDamping);
		rb.AddForce(force, ForceMode.Acceleration);

		// --- Rotation Spring-Damper ---
		if (!isColliding)
		{
			// Full rotation correction
			ApplyRotationCorrection();
		}
		else
		{
			// Reduced rotation correction during collisions
			ApplyRotationCorrection(0.1f);
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

	private void OnGrab(SelectEnterEventArgs args)
	{
		// Set the grab point as the controller transform
		controllerTransform = args.interactorObject.transform;
		isGrabbed = true;
	}

	private void OnRelease(SelectExitEventArgs args)
	{
		// Clear controller reference and stop applying spring-damper forces
		controllerTransform = null;
		isGrabbed = false;
	}
}
