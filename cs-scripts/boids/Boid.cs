using UnityEngine;

public class Boid : MonoBehaviour
{

    [Header("Detection")]
    public float visualRange;
    [SerializeField] private LayerMask whatIsObstacle;
    [SerializeField] private int obstacleDetectionRaycount;
    [SerializeField] private float obstacleDetectionRayLength;
    [SerializeField] private float obstacleAvoidanceFactor;

    [HideInInspector]
    public float minSpeed;
    [HideInInspector]
    public float maxSpeed;
    [HideInInspector]
    public Vector2 velocity;


    [SerializeField] private bool checkObstacles;

    public void Initialize()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        velocity.x = Mathf.Cos(angle) * minSpeed;
        velocity.y = Mathf.Sin(angle) * minSpeed;
    }

    public void UpdateBoid(BoidData data)
    {
        velocity = data.velocity;

        if(checkObstacles)
        {
            Collider2D[] obstaclesInRange = Physics2D.OverlapCircleAll(transform.position, visualRange, whatIsObstacle);
            if(obstaclesInRange.Length > 0) 
            {
                Vector2 avoidanceVelocity = CalculateObstacleAvoidance();
                velocity += avoidanceVelocity;
            }
        }

        // clamp speed 
        float speed = velocity.magnitude;
        if (speed < minSpeed && speed > 0)
            velocity = (velocity / speed) * minSpeed;
        else if (speed > maxSpeed && speed > 0)
            velocity = (velocity / speed) * maxSpeed;

        transform.Translate(velocity.x * Time.deltaTime, velocity.y * Time.deltaTime, 0, Space.World);

        //Face where we're going.
        // Flip the x and y because the triangle is right side up originally.
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private Vector2 CalculateObstacleAvoidance()
    {
        /*
        foreach (var col in obstaclesInRange)
        {
            if (col.gameObject == gameObject) continue; //skip self
            closeDx += transform.position.x - col.transform.position.x;
            closeDy += transform.position.y - col.transform.position.y;
            //This approach is good for thin objects, but not thick ones. We detect their colliders too far out from their centers(col.transform.position)
        }
        */


        //upon detecting an obstacle in range, shoot out raycasts around the agent
        //go to a space where a raycast does not hit.
        // but is also not too far from the direction im currently going.
        // to do this, we add dot product to the score when we don't hit anything, 
        // meaning a completely different direction will add(-1) to the score, and a closer direction would be something like +(0.8)

        float degreeOffset = 360f / obstacleDetectionRaycount;
        Vector2 highestScoringDirection = Vector2.zero;
        float highestScore = float.NegativeInfinity;

        for (int i = 0; i < obstacleDetectionRaycount; i++)
        {
            // Some caveats: we use a ray here to keep things simple, but in the case that there is a small gap in between two obstacles:
            // a ray may be shot in between that thin gap and the boid will not know that it's not supposed to fit through.
            // we can use a boxcast for that or 2-3 rays per direction with a gap in between rays
            // but I don't really need that for this specific scene so it's aight.

            Vector2 direction = Rotate(Vector2.right, degreeOffset * i);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, obstacleDetectionRayLength, whatIsObstacle);
            float score = 0;

            if (hit)
            {
                score = hit.distance;
            }
            else
            {
                score = obstacleDetectionRayLength;
                score += Vector2.Dot(velocity.normalized, direction);
            }

            if (score > highestScore)
            {
                highestScoringDirection = direction;
                highestScore = score;
            }
        }


        return highestScoringDirection * obstacleAvoidanceFactor;
        //Now that we have the highest score, apply it to our velocity.

        // Now depending on the direction we start with ray detection, (clock wise or counter clockwise) this may be biased towards one direction
        // IN the case that both left and right sides are equally free, the ray to be first shot out will be chosen.
        // We can introduce some randomization here but its not really that big a deal.
    }

    public static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }
}