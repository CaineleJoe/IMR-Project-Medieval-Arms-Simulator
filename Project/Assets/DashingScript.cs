using UnityEngine;
using UnityEngine.InputSystem;     // pentru InputAction
using System.Collections;

public class SideDashMultiTap : MonoBehaviour
{
	[Header("Camera (pentru a determina stânga/dreapta)")]
	[SerializeField] private Transform cameraTransform;
	// Dacă nu asignezi nimic în Inspector, va căuta automat Camera.main

	[Header("Acțiuni de input (dublu‐tap)")]
	[Tooltip("Trage aici acțiunea ce vine de la Left Hand (ex. DashLeft)")]
	public InputActionProperty dashLeftAction;

	[Tooltip("Trage aici acțiunea ce vine de la Right Hand (ex. DashRight)")]
	public InputActionProperty dashRightAction;

	[Header("Parametri Dash")]
	public float dashDistance = 3f;
	public float dashDuration = 0.2f;

	private bool isDashing = false;

	private void OnEnable()
	{
		if (dashLeftAction != null && dashLeftAction.action != null)
		{
			dashLeftAction.action.performed += OnDashLeftPerformed;
			dashLeftAction.action.Enable();
		}
		if (dashRightAction != null && dashRightAction.action != null)
		{
			dashRightAction.action.performed += OnDashRightPerformed;
			dashRightAction.action.Enable();
		}
	}

	private void OnDisable()
	{
		if (dashLeftAction != null && dashLeftAction.action != null)
		{
			dashLeftAction.action.performed -= OnDashLeftPerformed;
			dashLeftAction.action.Disable();
		}
		if (dashRightAction != null && dashRightAction.action != null)
		{
			dashRightAction.action.performed -= OnDashRightPerformed;
			dashRightAction.action.Disable();
		}
	}

	private void Start()
	{
		if (cameraTransform == null && Camera.main != null)
		{
			cameraTransform = Camera.main.transform;
		}
	}

	private void OnDashLeftPerformed(InputAction.CallbackContext ctx)
	{
		if (!isDashing)
		{
			Vector3 dashDir = GetCameraLeft();
			StartCoroutine(PerformDash(dashDir));
		}
	}

	private void OnDashRightPerformed(InputAction.CallbackContext ctx)
	{
		if (!isDashing)
		{
			Vector3 dashDir = GetCameraRight();
			StartCoroutine(PerformDash(dashDir));
		}
	}

	private Vector3 GetCameraLeft()
	{
		Vector3 camRight = cameraTransform.right;
		camRight.y = 0f;           
		camRight.Normalize();
		return -camRight;          
	}

	private Vector3 GetCameraRight()
	{
		Vector3 camRight = cameraTransform.right;
		camRight.y = 0f;
		camRight.Normalize();
		return camRight;
	}

	private IEnumerator PerformDash(Vector3 direction)
	{
		isDashing = true;

		Vector3 startPos = transform.position;
		Vector3 endPos = startPos + direction * dashDistance;

		float elapsed = 0f;
		while (elapsed < dashDuration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / dashDuration);

			transform.position = Vector3.Lerp(startPos, endPos, t);
			yield return null;
		}

		isDashing = false;
	}
}
