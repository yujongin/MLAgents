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
        // 개발자가 직접 명령을 내리는 휴리스틱 모드에서 사용
        // 주로 테스트용 또는 모방 학습에 사용
        // in actionsOut : in은 이 변수에 바로 값을 대입하는 행위를 막음

        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");  // x축 이동 단축키 활성화
        continuousActions[1] = Input.GetAxisRaw("Vertical");    // y축 이동 단축키 활성화

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = pressedInteractionInThisFrame ? 1 : 0;
        // 버튼이 입력되면 1, 아니면 0
        pressedInteractionInThisFrame = false;

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Agent가 결정한 행동을 전달, 보상 업데이트, 에피소드 종료

        // 2개의 좌표
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        // 이동 속도
        float moveSpeed = 3f;

        // 이동
        transform.Translate(new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime);

        // 상호작용
        int interaction = actions.DiscreteActions[0];

        if (interaction == 1)
        {
            // Discrete action이 1이면 / 버튼이 활성화되면
            Interact();
        }

        //AddReward(-0.5f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent에게 Vector Observation 정보를 전달하는 함수

        // 포인트로 가는 방향을 추가
        sensor.AddObservation(points[nextPoint].transform.localPosition - transform.localPosition);

        sensor.AddObservation(inThisArea); // 인자가 1개 = Space Size 1

        // Observe가 2개 세팅되었기에 Behavior Parameters의 Space Size는 6이 되어야 한다.
        // 옵저브 1개당 float 인자 (x, y, z) 3개 = Space Size 3
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(points[nextPoint].transform.localPosition);
    }

    void Interact()
    {
        if (interactionPoint == nextPoint)
        {
            // 현재 인터렉션을 해야하는 상황이라면
            Collider[] overlapped = Physics.OverlapBox(transform.position, Vector3.one / 2);

            if (overlapped.Length > 0)
            {
                foreach (var collider in overlapped)
                {
                    if (collider.isTrigger)
                    {
                        // 접하고 있는 콜라이더가 트리거일 때
                        if (collider.gameObject == points[interactionPoint])
                        {
                            // 그 트리거의 오브젝트가 내가 상호작용해야 할 포인트일 때
                            Debug.Log("Great!!");
                            AddReward(4f);
                            nextPoint++; // 다음 포인트 지정
                            break;
                        }
                        else
                        {
                            // 내가 상호작용해야할 포인트가 아닐 때에
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
            // 목적지가 아닐 때
            Debug.Log("wrong interact");
            AddReward(-0.1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Point")
        {
            inThisArea = points.IndexOf(other.gameObject);

            // 포인트를 만났을 때
            if (other.gameObject == points[nextPoint])
            {
                if (nextPoint == interactionPoint)
                {
                    // 나갔다가 다시 방문한 포인트일 때
                    Debug.Log("wrong interact");
                    AddReward(-0.5f);
                }
                else
                {
                    // 정상
                    Debug.Log("Good!!");
                    AddReward(4f + 4 * nextPoint);
                    interactionPoint = nextPoint; // 상호작용할 포인트 지정

                    if (nextPoint == points.Count - 1)
                    {
                        // 마지막 포인트일 때
                        Debug.Log("Complete!!");
                        AddReward(10f);
                    }

                }
            }
        }
    }
}