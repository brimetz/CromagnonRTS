using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class ExtensionMethods
{
    public static bool HasReached(this NavMeshAgent navMeshAgent)
    {
        if (!navMeshAgent.pathPending)
        {
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static void Finish(this NavMeshAgent navMeshAgent)
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
    }

    public static ETeam GetEnemyTeam(this ETeam team)
    {
        return team == ETeam.Blue ? ETeam.Red : ETeam.Blue;
    }

    public static Vector2 XZ(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static void Random(this ref Vector3 myVector, Vector3 min, Vector3 max)
    {
        myVector = new Vector3(UnityEngine.Random.Range(min.x, max.x), UnityEngine.Random.Range(min.y, max.y), UnityEngine.Random.Range(min.z, max.z));
    }
}
