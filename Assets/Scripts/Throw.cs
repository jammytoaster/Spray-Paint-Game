using Unity.VisualScripting;
using UnityEngine;

public class Throw : MonoBehaviour
{

    [Header ("References")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject throwableObject;

    [Header ("Settings")]
    public int totalThrows;
    public float throwCooldown;

    [Header ("Throwing")]
    public KeyCode aimKey = KeyCode.Mouse1;
    public KeyCode throwKey = KeyCode.Mouse0;
    public float throwForce;
    public float throwUpwardForce;

    public bool readyToThrow;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        readyToThrow = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(throwKey) && Input.GetKey(aimKey) && readyToThrow && totalThrows > 0){
            Throwing();
            Debug.Log("thow");
        }
    }

    private void Throwing(){
        readyToThrow = false;
        
        // Create thrown object
        GameObject projectile = Instantiate(throwableObject, attackPoint.position, cam.rotation);

        // Get rigidbody of the projectile
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        // Calculate throw direction to make throw central
        Vector3 forceDirection = cam.transform.forward;
        RaycastHit hit;

        if(Physics.Raycast(cam.position, cam.forward, out hit, 500f)){
            forceDirection = (hit.point - attackPoint.position).normalized;
        }

        // Force we want to throw object with
        Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);
        totalThrows--;
        Invoke(nameof(ResetThrows), throwCooldown);

        
    }

    private void ResetThrows(){
        readyToThrow = true;
    }
}
