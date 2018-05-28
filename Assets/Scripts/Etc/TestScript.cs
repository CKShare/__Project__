using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField]
    private Shader _shader;

    private void OnEnable()
    {
        GetComponent<Camera>().SetReplacementShader(_shader, "XRay");
    }
}