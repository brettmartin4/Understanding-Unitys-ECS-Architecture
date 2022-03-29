using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class FollowAircraft : MonoBehaviour
{

    private float x, y, z;

    private void Update()
    {
        x = GameManager.main.state.q1;
        y = GameManager.main.state.q5;
        z = GameManager.main.state.q3;

        transform.position = new float3(x, y, z);
    }
}
