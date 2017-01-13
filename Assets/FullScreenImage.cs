using UnityEngine;
using System.Collections;

public class FullScreenImage : MonoBehaviour {

    public Camera spriteCamera;
    public GameObject sprite;

	// Initialization
	void Start () {
        ResizeScreen();
	}

    void ResizeScreen() {
        SpriteRenderer renderer = sprite.GetComponent<SpriteRenderer>();

        // Camera.main
        float worldScreenHeight = spriteCamera.orthographicSize * 2;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        sprite.transform.localScale = new Vector3(
            worldScreenWidth / renderer.sprite.bounds.size.x,
            worldScreenHeight / renderer.sprite.bounds.size.y, 1);
    }
}
