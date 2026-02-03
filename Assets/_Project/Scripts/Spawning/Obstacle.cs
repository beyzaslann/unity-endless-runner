using UnityEngine;

public enum ObstacleType { Hit, JumpOver, SlideUnder }

public class Obstacle : MonoBehaviour
{
    public ObstacleType type = ObstacleType.Hit;
}
