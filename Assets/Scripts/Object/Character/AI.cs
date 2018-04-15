using UnityEngine;

public class AI : Character
{
    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            ApplyDamage(transform, 20, Random.Range(0, 3));
        }
    }
}
