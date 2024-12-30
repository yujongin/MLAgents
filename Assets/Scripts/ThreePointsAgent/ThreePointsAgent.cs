using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
public class ThreePointsAgent : Agent
{
    [SerializeField]
    List<GameObject> points;
    [SerializeField]
    Renderer floorRenderer;
    [SerializeField]
    Material winMaterial;
    [SerializeField]
    Material loseMaterial;

    int nextPoint;
    int interactionPoint;

    int inThisArea = -1;

    bool pressedInteractionInThisFrame = false;

    public override void OnEpisodeBegin()
    {
        nextPoint = 0;
        interactionPoint = -1;
        inThisArea = -1;
        transform.localPosition = new Vector3(0, 0.5f, 0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            pressedInteractionInThisFrame = true;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // �����ڰ� ���� ����� ������ �޸���ƽ ��忡�� ���
        // �ַ� �׽�Ʈ�� �Ǵ� ��� �н��� ���
        // in actionsOut : in�� �� ������ �ٷ� ���� �����ϴ� ������ ����

        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");  // x�� �̵� ����Ű Ȱ��ȭ
        continuousActions[1] = Input.GetAxisRaw("Vertical");    // y�� �̵� ����Ű Ȱ��ȭ

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = pressedInteractionInThisFrame ? 1 : 0;
        // ��ư�� �ԷµǸ� 1, �ƴϸ� 0
        pressedInteractionInThisFrame = false;

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Agent�� ������ �ൿ�� ����, ���� ������Ʈ, ���Ǽҵ� ����

        // 2���� ��ǥ
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        // �̵� �ӵ�
        float moveSpeed = 3f;

        // �̵�
        transform.Translate(new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime);

        // ��ȣ�ۿ�
        int interaction = actions.DiscreteActions[0];

        if (interaction == 1)
        {
            // Discrete action�� 1�̸� / ��ư�� Ȱ��ȭ�Ǹ�
            Interact();
        }

        //AddReward(-0.5f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent���� Vector Observation ������ �����ϴ� �Լ�

        // ����Ʈ�� ���� ������ �߰�
        sensor.AddObservation(points[nextPoint].transform.localPosition - transform.localPosition);

        sensor.AddObservation(inThisArea); // ���ڰ� 1�� = Space Size 1

        // Observe�� 2�� ���õǾ��⿡ Behavior Parameters�� Space Size�� 6�� �Ǿ�� �Ѵ�.
        // ������ 1���� float ���� (x, y, z) 3�� = Space Size 3
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(points[nextPoint].transform.localPosition);
    }

    void Interact()
    {
        if (interactionPoint == nextPoint)
        {
            // ���� ���ͷ����� �ؾ��ϴ� ��Ȳ�̶��
            Collider[] overlapped = Physics.OverlapBox(transform.position, Vector3.one / 2);

            if (overlapped.Length > 0)
            {
                foreach (var collider in overlapped)
                {
                    if (collider.isTrigger)
                    {
                        // ���ϰ� �ִ� �ݶ��̴��� Ʈ������ ��
                        if (collider.gameObject == points[interactionPoint])
                        {
                            // �� Ʈ������ ������Ʈ�� ���� ��ȣ�ۿ��ؾ� �� ����Ʈ�� ��
                            Debug.Log("Great!!");
                            AddReward(4f);
                            nextPoint++; // ���� ����Ʈ ����
                            break;
                        }
                        else
                        {
                            // ���� ��ȣ�ۿ��ؾ��� ����Ʈ�� �ƴ� ����
                            Debug.Log("wrong interact");
                            AddReward(-0.1f);
                            break;
                        }
                    }
                }
            }
            else
            {
                AddReward(-0.1f);
            }

        }
        else
        {
            // �������� �ƴ� ��
            Debug.Log("wrong interact");
            AddReward(-0.1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Point")
        {
            inThisArea = points.IndexOf(other.gameObject);

            // ����Ʈ�� ������ ��
            if (other.gameObject == points[nextPoint])
            {
                if (nextPoint == interactionPoint)
                {
                    // �����ٰ� �ٽ� �湮�� ����Ʈ�� ��
                    Debug.Log("wrong interact");
                    AddReward(-0.5f);
                }
                else
                {
                    // ����
                    Debug.Log("Good!!");
                    AddReward(4f + 4 * nextPoint);
                    interactionPoint = nextPoint; // ��ȣ�ۿ��� ����Ʈ ����

                    if (nextPoint == points.Count - 1)
                    {
                        // ������ ����Ʈ�� ��
                        Debug.Log("Complete!!");
                        AddReward(10f);
                    }

                }
            }
        }
    }
}