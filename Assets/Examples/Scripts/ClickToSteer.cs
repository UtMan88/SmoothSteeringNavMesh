using UnityEngine;
using UnityEngine.AI;

// Use physics raycast hit from mouse click to set agent destination
[RequireComponent(typeof(NavMeshAgent))]
public class ClickToSteer : MonoBehaviour
{
    NavMeshAgent m_Agent;
    NavMeshPath m_Path;
    int pathIter = 1;
    Vector3 AgentPosition;
    Vector3 destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    Vector3 endDestination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    RaycastHit m_HitInfo = new RaycastHit();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if(m_Path != null && m_Path.corners != null && m_Path.corners.Length > 0)
        {
            var prev = AgentPosition;
            for(int i = pathIter; i < m_Path.corners.Length; ++i)
            {
                Gizmos.DrawLine(prev, m_Path.corners[i]);
                prev = m_Path.corners[i];
            }
        }
    }

    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Agent.isStopped = true;
        m_Path = new NavMeshPath();
    }

    void Update()
    {
        SetAgentPosition();
        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out m_HitInfo))
            {
                //m_Agent.destination = m_HitInfo.point;
                m_Path = new NavMeshPath();
                endDestination = m_HitInfo.point;
                m_Agent.CalculatePath(endDestination, m_Path);
                pathIter = 1;
                m_Agent.isStopped = false;
                
            }
        }

        if (m_Path.corners == null || m_Path.corners.Length == 0)
            return;


        if (pathIter >= m_Path.corners.Length)
        {
            destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            m_Agent.isStopped = true;
            return;
        }
        else
        {
            destination = m_Path.corners[pathIter];
        }

        if (destination.x < float.PositiveInfinity)
        {
            Vector3 direction = destination - AgentPosition;
            var newDir = Vector3.RotateTowards(transform.forward, direction, 50 * Time.deltaTime, 0.0f);
            var newRot = Quaternion.LookRotation(newDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * 2f);
                
            float distance = Vector3.Distance(AgentPosition, destination);

            if (distance > m_Agent.radius + 0.1)
            {
                Vector3 movement = transform.forward * Time.deltaTime * 2f;

                m_Agent.Move(movement);
            }
            else
            {
                ++pathIter;
                if (pathIter >= m_Path.corners.Length)
                {
                    destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                    m_Agent.isStopped = true;
                }
            }
        }
    }

    void SetAgentPosition()
    {
        NavMeshHit hit;
        if(NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            AgentPosition = hit.position;
        }
    }
}
