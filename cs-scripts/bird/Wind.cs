using StateMachineCore;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineExtrude))]
[RequireComponent(typeof(MySplineColliderGenerator))]
public class Wind : MonoBehaviour
{
    class Rider
    {
        public IMovable2D movable;
        public Component component;
        public float currentT;
        public float currentSpeed;
    }

    private SplineContainer spline;
    [SerializeField] private float acceleration = 20;
    [SerializeField] private float maxSpeed = 50;
    [SerializeField] private float width = 2;

    private readonly Dictionary<IMovable2D, Rider> riders = new();
    private List<IMovable2D> ridersToRemove  = new List<IMovable2D>();
    private float splineLength;

#if UNITY_EDITOR
    [ContextMenu("Set up winds")]
    private void Reset()
    {
        MySplineColliderGenerator coll = GetComponent<MySplineColliderGenerator>();
        coll.SetWidth(width);
        SplineExtrude shape = GetComponent<SplineExtrude>();
        shape.Radius = width;

        AssetDatabase.Refresh();
        // or
        EditorApplication.isPlaying = false;
        // Defer material assignment until SplineExtrude has completed its first update
        EditorApplication.delayCall += () =>
        {
            if (this == null) return; // Guard in case object was destroyed

            Material wind = EditorHelper.FindMaterialByName("Wind");
            Material arrow = EditorHelper.FindMaterialByName("Arrow");

            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.SetMaterials(new List<Material>() { wind, arrow });
                EditorUtility.SetDirty(mr);
            }
        };

        gameObject.layer = LayerMask.NameToLayer("Wind");
        GetComponent<PolygonCollider2D>().isTrigger = true;
    }
#endif

    public void RideWind(IMovable2D movable, Component movableComponent)
    {
        // Find initial entry point 
        SplineUtility.GetNearestPoint(spline.Spline,
            spline.transform.InverseTransformPoint(movableComponent.transform.position),
            out _, out float startT);

        var rider = new Rider
        {
            movable = movable,
            component = movableComponent,
            currentT = startT,
            currentSpeed = movable.Velocity.magnitude
        };

        riders.Add(movable, rider);
    }

    private void Start()
    {
        spline = GetComponent<SplineContainer>();
        splineLength = spline.Spline.GetLength();   
    }

    public void ExitWind(IMovable2D movable)
    {
        if (riders.ContainsKey(movable))
        {
            riders.Remove(movable);
        }
    }

    private void FixedUpdate()
    {
        foreach (KeyValuePair<IMovable2D, Rider> pair in riders)
        {
            Rider rider = pair.Value;
            IMovable2D movable = pair.Key;

            float addSpeed = acceleration * Time.fixedDeltaTime;
            rider.currentSpeed = Mathf.Clamp(rider.currentSpeed + addSpeed, 0, maxSpeed);

            float tIncrement = (rider.currentSpeed / splineLength) * Time.fixedDeltaTime;
            rider.currentT += tIncrement;

            if (rider.currentT >= 1f)
            {
                if (spline.Spline.Closed)
                {
                    rider.currentT %= 1f; // Loop back to start if it's a closed circle
                }
                else
                {
                    ridersToRemove.Add(movable);
                    continue;
                }
            }

            spline.Evaluate(rider.currentT, out float3 localTargetPos, out float3 tangent, out float3 up);

            Vector3 neededVelocity = (localTargetPos - (float3)rider.component.transform.position) / Time.fixedDeltaTime;

            Vector3 target = new Vector3(
                KeepIfFasterAndSameDirection(movable.Velocity.x, neededVelocity.x),
                KeepIfFasterAndSameDirection(movable.Velocity.y, neededVelocity.y)
            );
            target = Vector3.ClampMagnitude(target, maxSpeed);
            movable.SetVelocityX(target.x);
            movable.SetVelocityY(target.y);
        }

        foreach (IMovable2D rider in ridersToRemove)
        {
            riders.Remove(rider);
        }
        ridersToRemove.Clear();
    }

    float KeepIfFasterAndSameDirection(float current, float needed) =>
        Mathf.Sign(current) == Mathf.Sign(needed) && Mathf.Abs(current) > Mathf.Abs(needed)
        ? current : needed;
}

public class EditorHelper
{
    public static Material FindMaterialByName(string materialName)
    {
        #if UNITY_EDITOR
        // Search for all material assets matching the name
        string[] guids = AssetDatabase.FindAssets($"{materialName} t:Material");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            // Exact name match (case-sensitive)
            if (mat != null && mat.name == materialName)
                return mat;
        }
        return null;

        #endif
        return null;
    }
}