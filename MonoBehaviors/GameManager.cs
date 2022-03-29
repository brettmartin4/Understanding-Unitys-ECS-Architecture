using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;


public class GameManager : MonoBehaviour
{
    private BlobAssetStore blobAssetStore;

    public static GameManager main;

    public GameObject planePrefab;

    public float zBound = -1.0f;
    public float throttleMaxBound = 1.0f;
    public float alphaMaxBound = 20.0f;
    public float bankMaxBound = 20.0f;
    public float flapMaxBound = 40.0f;
    public float throttleMinBound = 0.0f;
    public float alphaMinBound = 0.0f;
    public float bankMinBound = -20.0f;
    public float flapMinBound = 0.0f;

    public State state;
    public Properties prop;

    Entity planeEntityPrefab;
    EntityManager manager;

    private void Awake()
    {
        if (main != null && main != this)
        {
            Destroy(gameObject);
            return;
        }

        main = this;

        manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        blobAssetStore = new BlobAssetStore();

        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);
        planeEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(planePrefab, settings);

        SpawnEntity();
    }

    private void OnDestroy()
    {
        blobAssetStore.Dispose();
    }

    void SpawnEntity()
    {
        Entity plane = manager.Instantiate(planeEntityPrefab);

        manager.DestroyEntity(planeEntityPrefab);

        prop = new Properties(
            16.2f,          // wing area
            10.9f,          // wing span
            2.0f,           // tail area
            0.0889f,        // slope of Cl-alpha curve
            0.178f,         // intercept of Cl-alpha curve
            -0.1f,          // post-stall slope of Cl-alpha curve
            3.2f,           // post-stall intercept of Cl-alpha curve
            16.0f,          // alpha when Cl=Clmax
            0.034f,         // parasite drag coefficient
            0.77f,          // induced drag efficiency coefficient
            1114.0f,        // mass
            119310.0f,      // engine power
            40.0f,          // revolutions per second
            1.905f,         // propeller diameter
            1.83f,          // propeller efficiency coefficient
            -1.32f          // propeller efficiency coefficient
            );

        state = new State(
            0.0f,           // time
            0.0f,           // ODE results, x velocity
            0.0f,           // x
            0.0f,           // z velocity
            0.0f,           // z
            0.0f,           // y velocity
            0.0f,           // y
            0.0f,           // roll angle
            4.0f,           // pitch angle
            0.0f,           // throttle percentage
            0.0f            // flap deflection
            );

        float3 position = new float3(state.q1, state.q3, state.q5);

        manager.SetComponentData(plane, new Translation { Value = position });
    }

}
