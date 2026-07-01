using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private bool updateTargets = false;

    [SerializeField] private List<TrackingTarget> trackingTargets = new();

    [SerializeField] private float distanceScale = 1f;
    [SerializeField] private float distanceOffset = 5f;
    [SerializeField] private float verticalOffset = 5f;

    [SerializeField] private float positionSmoothing = 1f;
    [SerializeField] private float lookatSmoothing = 1f;

    private void Start()
    {
        UpdateTrackingTargets();
    }

    private void Update()
    {
        if (updateTargets)
        {
            UpdateTrackingTargets();
            updateTargets = false;
        }

        UpdatePosition(GetIdealPos(trackingTargets, GetWeightedTargetPos(trackingTargets)), GetWeightedTargetPos(trackingTargets));
    }


    public void UpdateTrackingTargets()
    {
        trackingTargets.Clear();

        foreach (TrackingTarget target in FindObjectsByType<TrackingTarget>())
        {
            trackingTargets.Add(target);
        }
    }


    private void UpdatePosition(Vector3 position, Vector3 lookat)
    {
        transform.position = Vector3.Lerp(transform.position, position, positionSmoothing * Time.unscaledDeltaTime);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookat - transform.position, Vector3.up), lookatSmoothing * Time.unscaledDeltaTime);
    }


    private Vector3 GetIdealPos(List<TrackingTarget> targets, Vector3 weightedTargetPos)
    {
        Vector3 toReturn = weightedTargetPos;

        float averageDistance = 0f;
        float weightSum = 0f;

        foreach (TrackingTarget target in targets)
        {
            averageDistance += Vector3.Distance(weightedTargetPos, target.transform.position) * target.Weight;
            weightSum += target.Weight;
        }

        averageDistance /= weightSum;

        toReturn.z = -1 * (averageDistance * distanceScale + distanceOffset);

        toReturn.y += verticalOffset;

        return toReturn;
    }

    private Vector3 GetWeightedTargetPos(List<TrackingTarget> targets)
    {
        Vector3 toReturn = Vector3.zero;
        float weightSum = 0f;

        foreach (TrackingTarget target in targets)
        {
            toReturn += target.transform.position * target.Weight;
            weightSum += target.Weight;
        }

        return toReturn / weightSum;
    }


    public void EventListenerCameraShake(float strength)
    {
        CameraShake(strength, strength);
    }
    
    public void CameraShake(float strength, float duration)
    {
        StartCoroutine(ShakeCoroutine(duration, strength));
    }


    private IEnumerator ShakeCoroutine(float time, float strength)
    {
        float startTime = Time.time;

        while (Time.time - startTime < time)
        {
            float intensity = strength * (1 + (startTime - Time.time) / time);

            transform.position += new Vector3(
                    Random.Range(-intensity, intensity),
                    Random.Range(-intensity, intensity),
                    Random.Range(-intensity, intensity)
                );

            yield return null;
        }
    }
}
