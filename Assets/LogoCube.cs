using UnityEngine;
using System.Collections;


public class LogoCube : MonoBehaviour {

    MeshRenderer _meshRenderer;
    MeshRenderer meshRenderer() {
        if (_meshRenderer == null) {
            _meshRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
        }
        return _meshRenderer;
    }

    // Logo border (visible) or inner
    public bool inner = false;

    public Quaternion originalRotation = Quaternion.identity;
    public Vector3 originalPosition = Vector3.zero;

    public float originalSize = 1;
    public float size { 
        get { return this.gameObject.transform.localScale.z; }
        set { this.gameObject.transform.localScale = new Vector3 (value, value, value); }
    }

    public Color color {
        get { return meshRenderer().material.color; }
        set { meshRenderer().material.color = value; }  
    }


    public bool isRotating = false;
    public IEnumerator Rotate(float duration, Vector3 angles, float delay = 0)
    {
        if (isRotating) {
            yield break;
        }
        isRotating = true;

        // Execute code after the delay
        yield return new WaitForSeconds(delay);

        Quaternion fromAngle = transform.rotation;
        Quaternion toAngle = Quaternion.Euler(transform.eulerAngles + angles);
        for (float t = 0f ; t < 1f ; t += Time.deltaTime/duration) {
            transform.rotation = Quaternion.Lerp(fromAngle, toAngle, t);
            yield return null ;
        }

        isRotating = false;
        // This is a hack, not an universal solution...
        transform.localRotation = Quaternion.identity;
    }
}