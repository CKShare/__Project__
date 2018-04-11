using UnityEngine;
using Sirenix.OdinInspector;

public class SceneObject : SerializedMonoBehaviour, IDamageable
{
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField, InlineEditor, Required]
    private SceneObjectInfo _info;
    [SerializeField]
    private SceneObjectDestructor _destructor;

    void Awake()
    {
        _destructor?.Initialize(gameObject);
    }

    public virtual void ApplyDamage(int damage, int reactionID = -1)
    {
        _destructor?.ApplyDamage();
    }

    public SceneObjectInfo Info => _info;
}