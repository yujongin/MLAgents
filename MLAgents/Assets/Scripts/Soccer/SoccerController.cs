using UnityEngine;
using Unity.MLAgents;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
public enum Team
{
    Blue = 0,
    Red = 1
}

public class SoccerController : MonoBehaviour
{
    public int maxSteps = 10000;


    public GameObject ball;
    [HideInInspector]
    public Rigidbody ballRb;
    Vector3 ballStartingPos;
    public List<SoccerAgent> agents = new List<SoccerAgent>();
    public float agentSpeed = 2;

    SimpleMultiAgentGroup blueAgentGroup;
    SimpleMultiAgentGroup redAgentGroup;

    int stepCounter;

    int blueScore = 0;
    int redScore = 0;

    public TMP_Text scoreText;

    void Start()
    {
        ballRb = ball.GetComponent<Rigidbody>();    
        ballStartingPos = ball.transform.position;

        blueAgentGroup = new SimpleMultiAgentGroup();
        redAgentGroup = new SimpleMultiAgentGroup();

        foreach(SoccerAgent agent in agents)
        {
            agent.moveSpeed = agentSpeed;
            if(agent.team == Team.Blue)
            {
                blueAgentGroup.RegisterAgent(agent);
            }
            else
            {
                redAgentGroup.RegisterAgent(agent);
            }
        }
        InitEnvironment();
    }
    private void FixedUpdate()
    {
        stepCounter++;

        if(stepCounter>maxSteps && maxSteps > 0)
        {
            //골을 못넣었으면 에피소드 무효
            blueAgentGroup.GroupEpisodeInterrupted();
            redAgentGroup.GroupEpisodeInterrupted();
            InitEnvironment();
        }
    }
    public void GoalScored(Team scoredTeam)
    {
        //골 넣으면 상점 먹히면 벌점 빨리 넣을 수록 상점이 큼
        if(scoredTeam == Team.Blue)
        {
            blueAgentGroup.AddGroupReward(1 - (float)stepCounter / maxSteps);
            redAgentGroup.AddGroupReward(-1);
            blueScore++;
        }
        else
        {
            redAgentGroup.AddGroupReward(1 - (float)stepCounter / maxSteps);
            blueAgentGroup.AddGroupReward(-1);
            redScore++;
        }
        redAgentGroup.EndGroupEpisode();
        blueAgentGroup.EndGroupEpisode();

        scoreText.text = blueScore.ToString() + ":" + redScore.ToString();
        InitEnvironment();
    }

    public void InitEnvironment()
    {
        stepCounter = 0;
        foreach(var agent in agents)
        {
            agent.transform.position = agent.startingPos + new Vector3(0,0,Random.Range(-0.2f,0.2f));
            float rot = (agent.team == Team.Blue) ? Random.Range(80f, 100f): Random.Range(-80f,-100f);
            agent.transform.rotation = Quaternion.Euler(0, rot, 0);
            agent.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            agent.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        float bx = Random.Range(-1f, 1f);
        float by = Random.Range(-1f, 1f);
        ball.transform.position = ballStartingPos + new Vector3(bx, 0, by);
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
    }

}
