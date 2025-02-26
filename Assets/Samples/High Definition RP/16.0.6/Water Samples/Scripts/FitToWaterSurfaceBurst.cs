using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class FitToWaterSurfaceBurst : MonoBehaviour
{
    // Public parameters
    public int count = 50;
    public WaterSurface waterSurface = null;
    public bool includeDeformation = true;
    public float currentSpeedMultiplier = 1.0f;
    public GameObject prefab;

    // List
    private List<GameObject> prefabList;
    private BoxCollider boxCollider;

    // Input job parameters
    private NativeArray<float3> targetPositionBuffer;

    // Output job parameters
    private NativeArray<float> errorBuffer;
    private NativeArray<float3> candidatePositionBuffer;
    private NativeArray<float3> projectedPositionWSBuffer;
    private NativeArray<float3> directionBuffer;
    private NativeArray<int> stepCountBuffer;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        Reset();
    }

    void Reset()
    {
        // Dispose of any existing buffers before reallocating
        DisposeBuffers();

        // Allocate the buffers
        targetPositionBuffer = new NativeArray<float3>(count, Allocator.Persistent);
        errorBuffer = new NativeArray<float>(count, Allocator.Persistent);
        candidatePositionBuffer = new NativeArray<float3>(count, Allocator.Persistent);
        projectedPositionWSBuffer = new NativeArray<float3>(count, Allocator.Persistent);
        stepCountBuffer = new NativeArray<int>(count, Allocator.Persistent);
        directionBuffer = new NativeArray<float3>(count, Allocator.Persistent);

        prefabList = new List<GameObject>();
        prefabList.Clear();

        // Remove existing child objects safely
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            SmartDestroy(transform.GetChild(i).gameObject);
        }

        // Spawn new objects
        for (int x = 0; x < count; ++x)
        {
            GameObject instance = Instantiate(prefab);
            instance.transform.SetParent(transform);
            instance.transform.localPosition = RandomPointInBounds(GetComponent<Collider>().bounds) - transform.position;
            instance.transform.localEulerAngles = new Vector3(-180, UnityEngine.Random.Range(0, 360), 0);
            prefabList.Add(instance);
        }
    }

    void Update()
    {
        if (waterSurface == null || !targetPositionBuffer.IsCreated)
            return;

        // Try to get the simulation data if available
        WaterSimSearchData simData = new WaterSimSearchData();
        if (!waterSurface.FillWaterSearchData(ref simData))
            return;

        // Fill the input positions
        for (int i = 0; i < prefabList.Count; ++i)
        {
            targetPositionBuffer[i] = prefabList[i].transform.position;
        }

        // Prepare the job
        WaterSimulationSearchJob searchJob = new WaterSimulationSearchJob
        {
            simSearchData = simData,
            targetPositionWSBuffer = targetPositionBuffer,
            startPositionWSBuffer = targetPositionBuffer,
            maxIterations = 8,
            error = 0.01f,
            includeDeformation = includeDeformation,
            excludeSimulation = false,
            projectedPositionWSBuffer = projectedPositionWSBuffer,
            errorBuffer = errorBuffer,
            candidateLocationWSBuffer = candidatePositionBuffer,
            directionBuffer = directionBuffer,
            stepCountBuffer = stepCountBuffer
        };

        // Schedule and complete the job
        JobHandle handle = searchJob.Schedule(count, 1);
        handle.Complete();

        // Update positions
        for (int i = 0; i < prefabList.Count; ++i)
        {
            float3 projectedPosition = projectedPositionWSBuffer[i];
            prefabList[i].transform.position = projectedPosition + Time.deltaTime * directionBuffer[i] * currentSpeedMultiplier;
        }
    }

    private Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
            UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    void DisposeBuffers()
    {
        if (targetPositionBuffer.IsCreated) targetPositionBuffer.Dispose();
        if (errorBuffer.IsCreated) errorBuffer.Dispose();
        if (candidatePositionBuffer.IsCreated) candidatePositionBuffer.Dispose();
        if (projectedPositionWSBuffer.IsCreated) projectedPositionWSBuffer.Dispose();
        if (stepCountBuffer.IsCreated) stepCountBuffer.Dispose();
        if (directionBuffer.IsCreated) directionBuffer.Dispose();
    }

    void OnDestroy()
    {
        DisposeBuffers();
    }

    void OnDisable()
    {
        DisposeBuffers();
    }

    public static void SmartDestroy(UnityEngine.Object obj)
    {
        if (obj == null)
            return;

#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            Destroy(obj);
        }
        else
        {
            DestroyImmediate(obj);
        }
#else
        Destroy(obj);
#endif
    }
}
