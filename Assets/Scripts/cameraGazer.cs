using UnityEngine;

public class cameraGazer : MonoBehaviour {

    SpriteRenderer spriteRenderer;

    float dotValue;
    bool isRight;

    [SerializeField]
    Character Character;

    private void Awake() {

        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    void Update() {

        LookCamera();
        ChangeDirectionSprite();

    }

    void LookCamera() {

        Quaternion q = Camera.main.transform.rotation;
        q.x = 0;
        q.z = 0;
        transform.rotation = q;

    }

    void ChangeDirectionSprite() {

        dotValue = Vector3.Dot(Character.dirVec, Camera.main.transform.right);
        spriteRenderer.flipX = (dotValue >= 0);

    }
}
