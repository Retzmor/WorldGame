using UnityEngine;

public class AnimalMovement : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Transform[] pointsMovement;
    [SerializeField] float distancePoint;

    int numberRandom;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        numberRandom = Random.Range(0, pointsMovement.Length);
        spriteRenderer = GetComponent<SpriteRenderer>();
        Around();
    }

    private void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, pointsMovement[numberRandom].position, speed * Time.deltaTime);

        if(Vector2.Distance(transform.position, pointsMovement[numberRandom].position) < distancePoint)
        {
            numberRandom = Random.Range(0, pointsMovement.Length);
            Around();
        }
    }

    public void Around()
    {
        if(transform.position.x < pointsMovement[numberRandom].position.x)
        {
            spriteRenderer.flipX = false;
        }

        else
        {
            spriteRenderer.flipX = true;
        }
    }
}
