using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100F))
            {
                Debug.Log(hit.collider.name);
                hit.rigidbody.AddForce(ray.direction * 10F, ForceMode.Impulse);
            }
        }
    }
}
