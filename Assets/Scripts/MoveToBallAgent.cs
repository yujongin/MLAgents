using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MoveToBallAgent : Agent
{
    [SerializeField] Transform targetTransform;

    [SerializeField] Renderer FloorRenderer;
    [SerializeField] Material winMaterial;
    [SerializeField] Material loseMaterial;

    public override void OnEpisodeBegin()
    {
        do
        {
            transform.localPosition = new Vector3(Random.Range(-3.5f, 3.5f), 0.5f, Random.Range(-3.5f, 3.5f));
            targetTransform.localPosition = new Vector3(Random.Range(-3.5f, 3.5f), 0.5f, Random.Range(-3.5f, 3.5f));
        } while (Vector3.Distance(transform.localPosition, targetTransform.localPosition) < 1.5f);
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        //VectorSensor�� �����͸� �־ ����
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float x = actions.ContinuousActions[0];
        float y = actions.ContinuousActions[1];

        //int x = actions.DiscreteActions[0];
        //int y = actions.DiscreteActions[1];

        float moveSpeed = 2f;

        transform.Translate(new Vector3(x, 0, y) * Time.deltaTime * moveSpeed);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continousAction = actionsOut.ContinuousActions;

        continousAction[0] = Input.GetAxisRaw("Horizontal");
        continousAction[1] = Input.GetAxisRaw("Vertical");
    }

    //reward
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ball")
        {
            Debug.Log("Good");
            SetReward(1f); // ����
            FloorRenderer.material = winMaterial;
            EndEpisode(); // ���Ǽҵ� ���� �� ���ο� ���Ǽҵ�� ���۽�Ų��.
        }
        else if (other.tag == "Wall")
        {
            Debug.Log("Bad");
            SetReward(-1f); // ����
            FloorRenderer.material = loseMaterial;
            EndEpisode(); // ���Ǽҵ� ���� �� ���ο� ���Ǽҵ�� ���۽�Ų��.
        }
    }
}
