using UnityEngine;
using Sirenix.OdinInspector;

public class SceneObjectDestructor
{
    [SerializeField, InlineEditor, Required]
    private DestructibleObjectInfo _info;
    [SerializeField]
    private GameObject _destructedObject;

    private GameObject _intactObject;
    private int _currentHit;

    public void Initialize(GameObject intactObject)
    {
        _intactObject = intactObject;
        _currentHit = _info.MaxHit;
    }

    public void ApplyDamage()
    {
        _currentHit--;
        if (_currentHit <= 0)
            OnDestructed();
    }

    private void OnDestructed()
    {
        // Sound & Particle

        _destructedObject.SetActive(true);
        _intactObject.SetActive(false);
    }
}