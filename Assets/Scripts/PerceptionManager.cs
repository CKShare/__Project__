using UnityEngine;

public class PerceptionManager : MonoSingleton<PerceptionManager>
{
    [SerializeField]
    private float _perceiveRadius = 5F;

    private Collider[] _colliders = new Collider[5];

    public void SendSignal(Vector3 position)
    {
        int count = Physics.OverlapSphereNonAlloc(position, _perceiveRadius, _colliders);
        for (int i = 0; i < count; i++)
            _colliders[i].GetComponent<IPerceivable>().Perceive();
    }
}
