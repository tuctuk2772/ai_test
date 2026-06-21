using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class RangePoints
{
    public Vector2 coordinates;
    public bool breakLine = false;
}

public class WatchPlayer : MonoBehaviour
{
    [SerializeField] private Animator enemy;
    [SerializeField] private Animator _player;
    [Header("Get Spotted")]
    [SerializeField, Range(0, 30)] private float getSpottedVerticalMaxDistance = 7f;
    [SerializeField, Range(0, 10)] private float getSpottedHorizontalMaxDistance = 2f;
    [SerializeField, Range(0, 1)] private float getSpottedHorizontalMaxPercentage = 0.75f;
    [SerializeField, Range(0, 1)] private float getSpottedVerticalMaxPercentage = 0.66f;

    [Header("Temp")]
    private List<RangePoints> getCuriousRange = new();
    private List<RangePoints> getSpottedRange = new();

    bool inRange = false;

    private void Update()
    {

    }

    private void OnDrawGizmos()
    {
        Transform headBone = enemy.GetBoneTransform(HumanBodyBones.Head);
        Vector3 headPosition = headBone.position;
        Quaternion headRotation = headBone.rotation;

        Gizmos.color = Color.red;

        for (int i = 0; i < 3; i++)
        {
            Vector3 localOffset = new Vector3();

            switch (i)
            {
                case 0:
                    localOffset = new Vector3(getSpottedHorizontalMaxDistance * getSpottedVerticalMaxPercentage, 0f, 0f);
                    break;
                case 1:
                    localOffset = new Vector3(getSpottedHorizontalMaxDistance, 0f, getSpottedVerticalMaxDistance * getSpottedHorizontalMaxPercentage);
                    break;
                case 2:
                    localOffset = new Vector3(getSpottedHorizontalMaxDistance * getSpottedVerticalMaxPercentage, 0f, getSpottedVerticalMaxDistance);
                    break;
            }

            Vector3 inverseOffset = new Vector3(localOffset.x * -1f, localOffset.y, localOffset.z);

            Gizmos.DrawSphere(headPosition + (headRotation * localOffset), 0.1f);
            Gizmos.DrawSphere(headPosition + (headRotation * inverseOffset), 0.1f);
        }

        for (int i = 0; i < getCuriousRange.Count; i++)
        {

            //Vector3 localOffset = new Vector3(getCuriousRange[i].coordinates.x, 0, getCuriousRange[i].coordinates.y);
            //Vector3 nextLocalOffset = new Vector3(getCuriousRange[(i + 1) % getCuriousRange.Count].coordinates.x, 0, getCuriousRange[(i + 1) % getCuriousRange.Count].coordinates.y);

            //Vector3 worldOffset = headRotation * localOffset;

            //Gizmos.color = Color.red;
            //Gizmos.DrawSphere(headPosition + worldOffset, 0.1f);
            //if (!getCuriousRange[i].breakLine)
            //{
            //    Gizmos.DrawLine(headPosition + worldOffset, headPosition + (headRotation * nextLocalOffset));
            //}
        }

        Gizmos.color = Color.white;
        if (inRange)
        {
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.Head).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.LeftUpperArm).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.LeftLowerArm).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.RightUpperArm).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.RightLowerArm).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.Chest).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.RightUpperLeg).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.RightLowerLeg).position);
        }
    }
}
