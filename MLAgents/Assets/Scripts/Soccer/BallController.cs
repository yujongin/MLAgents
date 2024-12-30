using UnityEngine;

public class BallController : MonoBehaviour
{
    public SoccerController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "BlueGoal")
        {
            controller.GoalScored(Team.Red);
        }
        else if (other.tag == "RedGoal")
        {
            controller.GoalScored(Team.Blue);
        }

    }
}
