using UnityEngine;

public class ChainBreakOnImpact : MonoBehaviour
{
    [SerializeField] private float breakForceThreshold = 10f;
    [SerializeField] private string breakerTag = "Breaker"; 

    private HingeJoint hingeJoint;
    private Rigidbody rb;

    void Start()
    {
        hingeJoint = GetComponent<HingeJoint>();
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(breakerTag))
            return; 

        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce >= breakForceThreshold)
        {
            if (hingeJoint != null)
            {
                Destroy(hingeJoint);
            }
        }
    }
}
