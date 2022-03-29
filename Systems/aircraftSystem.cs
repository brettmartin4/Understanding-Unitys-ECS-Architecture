using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.SceneManagement;


public class aircraftSystem : SystemBase
{

    public void update_UI(State state)
    {
        // velocity information
        float vx = state.q0;
        float vy = state.q4;
        float vz = state.q2;
        // position information
        float y = state.q5;

        float heading = Mathf.Atan2(vz, vx) * Mathf.Rad2Deg;
        float vh = Mathf.Sqrt(vx * vx + vz * vz);
        float climb_angle = Mathf.Atan(vy / Mathf.Clamp(vh, 0.001f, 50000.0f)) * Mathf.Rad2Deg;
        float air_speed = Mathf.Sqrt(vx * vx + vy * vy + vz * vz);

        Debug.LogFormat("Throttle: {0}% | Angle of Attack: {1} deg | Bank Angle: {2} deg | Flap Deflection: {3}deg | Heading: {4} deg | Climb Angle: {5} deg | Air Speed: {6} | Climb Rate: {7} | Altitude: {8}", Mathf.Round(state.throttle * 100.0f), state.alpha, state.bank, state.flap, heading, climb_angle, air_speed, vy, y);
    }


    public void reset_scene(ref State state)
    {
        state.time = 0.0f;
        state.q0 = 0.0f;
        state.q1 = 0.0f;
        state.q2 = 0.0f;
        state.q3 = 0.0f;
        state.q4 = 0.0f;
        state.q5 = 0.0f;
        state.bank = 0.0f;
        state.alpha = 4.0f;
        state.throttle = 0.0f;
        state.flap = 0.0f;

        Entities.ForEach((ref Translation translation, ref Rotation rotation, ref AircraftData aircraftData) => {
            translation.Value = new float3(0.0f, 0.0f, 0.0f);
        }).Schedule();
    }


    public void update_entity(ref State state)
    {
        // velocity information
        float vx = state.q0;
        float vy = state.q4;
        float vz = state.q2;
        // position information
        float x = state.q1;
        float y = state.q5;
        float z = state.q3;

        float3 position = new float3(x, y, z);
        float bank = -90.0f + state.bank;
        float alpha = state.alpha;
        float heading = Mathf.Atan2(vz, vx);

        // calculate Quaternion value for new rotation
        float roll = bank * Mathf.Deg2Rad;
        float pitch = heading;
        float yaw = alpha * Mathf.Deg2Rad;

        float croll = Mathf.Cos(roll * 0.5f);
        float cpitch = Mathf.Cos(pitch * 0.5f);
        float cyaw = Mathf.Cos(yaw * 0.5f);

        float sroll = Mathf.Sin(roll * 0.5f);
        float spitch = Mathf.Sin(pitch * 0.5f);
        float syaw = Mathf.Sin(yaw * 0.5f);

        float cyawcpitch = cyaw * cpitch;
        float syawspitch = syaw * spitch;
        float cyawspitch = cyaw * spitch;
        float syawcpitch = syaw * cpitch;

        Quaternion newRot = new Quaternion((cyawcpitch * sroll - syawspitch * croll),
            (cyawspitch * croll + syawcpitch * sroll),
            (syawcpitch * croll - cyawspitch * sroll),
            (cyawcpitch * croll + syawspitch * sroll));

        Entities.ForEach((ref Translation translation, ref Rotation rotation, ref AircraftData aircraftData) => {
            translation.Value = position;
            rotation.Value = newRot;
        }).Schedule();

        if (y < GameManager.main.zBound) { reset_scene(ref state); }
    }

    
    protected override void OnUpdate()
    {
        // Main loop
        float deltaTime = Time.DeltaTime;

        // Check keyboard inputs
        if (Input.GetKeyDown(KeyCode.E)) { 
            GameManager.main.state.throttle = Mathf.Clamp(GameManager.main.state.throttle + 0.1f, GameManager.main.throttleMinBound, GameManager.main.throttleMaxBound); 
        }
        else if (Input.GetKeyDown(KeyCode.D)) { 
            GameManager.main.state.throttle = Mathf.Clamp(GameManager.main.state.throttle - 0.1f, GameManager.main.throttleMinBound, GameManager.main.throttleMaxBound);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) { 
            GameManager.main.state.alpha = Mathf.Clamp(GameManager.main.state.alpha + 1.0f, GameManager.main.alphaMinBound, GameManager.main.alphaMaxBound);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) { 
            GameManager.main.state.alpha = Mathf.Clamp(GameManager.main.state.alpha - 1.0f, GameManager.main.alphaMinBound, GameManager.main.alphaMaxBound);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) { 
            GameManager.main.state.bank = Mathf.Clamp(GameManager.main.state.bank + 1.0f, GameManager.main.bankMinBound, GameManager.main.bankMaxBound);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) { 
            GameManager.main.state.bank = Mathf.Clamp(GameManager.main.state.bank - 1.0f, GameManager.main.bankMinBound, GameManager.main.bankMaxBound);
        }
        else if (Input.GetKeyDown(KeyCode.L)) { 
            GameManager.main.state.flap = Mathf.Clamp(GameManager.main.state.flap - 1.0f, GameManager.main.flapMinBound, GameManager.main.flapMaxBound);
        }
        else if (Input.GetKeyDown(KeyCode.K)) { 
            GameManager.main.state.flap = Mathf.Clamp(GameManager.main.state.flap + 1.0f, GameManager.main.flapMinBound, GameManager.main.flapMaxBound);
        }
        else if (Input.GetKeyDown(KeyCode.Q)) {
            reset_scene(ref GameManager.main.state);
        }

        // Apply the Runge-Kutta EOM algorithm
        EOM.eom(GameManager.main.prop, ref GameManager.main.state, deltaTime);

        // Update entity translation, rotation, and debug log values
        update_entity(ref GameManager.main.state);
        update_UI(GameManager.main.state);
    }

}
