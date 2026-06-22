using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class WatchPlayer : MonoBehaviour
{
    [HideInInspector] public int enemyNumber;

    [SerializeField] private Animator enemy;
    [SerializeField] private Animator _player;

    [Header("Get Spotted")]
    [SerializeField, Range(0, 2)] private float getSpottedVerticalOffset = 1f;
    [SerializeField, Range(0, 30)] private float getSpottedVerticalMaxDistance = 7f;
    [SerializeField, Range(0, 10)] private float getSpottedHorizontalMaxDistance = 2f;
    [SerializeField, Range(0, 1)] private float getSpottedHorizontalMaxPercentage = 0.75f;
    [SerializeField, Range(0, 1)] private float getSpottedVerticalMaxPercentage = 0.66f;

    [Header("Get Curious")]
    [SerializeField, Range(0, 2)] private float getCuriousVerticalOffset = 0f;
    [SerializeField, Range(5, 50)] private float getCuriousVerticalMaxDistance = 15f;
    [SerializeField, Range(2, 15)] private float getCuriousHorizontalMaxDistance = 5f;
    [SerializeField, Range(0, 1)] private float getCuriousHorizontalMaxPercentage = 0.75f;
    [SerializeField, Range(0, 1)] private float getCuriousVerticalMaxPercentage = 0.66f;

    [Header("Sixth Sense")]
    [SerializeField] private bool sixthSense = true;
    [SerializeField] private bool immediateSense = false;
    [SerializeField, Range(0, 2)] private float sixthSenseVerticalOffset = 0.25f;
    [SerializeField, Range(0, 5)] private float sixthSenseHorizontal = 2f;
    [SerializeField, Range(0, 5)] private float sixthSenseVertical = 1f;
    [SerializeField, Range(0, 1)] private float sixthSenseAnglePercentage = 0.75f;

    [Header("Temp")]
    private Vector2[] getCuriousCoordinates = new Vector2[6];
    private Vector2[] getSpottedCoordinates = new Vector2[6];
    private Vector2[] sixthSenseCoordinates = new Vector2[6];

    bool inRange = false;
    Transform headBone => enemy.GetBoneTransform(HumanBodyBones.Head);

    private void Start()
    {
        if(headBone == null)
        {
            Debug.LogError("Can't find head!");
        }

        for (int i = 0; i < 3; i++)
        {
            Vector3 localSpottedOffset = new();
            Vector3 localCuriousOffset = new();
            Vector3 localSixthOffset = new();

            switch (i)
            {
                case 0:
                    localSpottedOffset = new Vector3(getSpottedHorizontalMaxDistance * getSpottedVerticalMaxPercentage, 0f, getSpottedVerticalOffset);
                    localCuriousOffset = new Vector3(getCuriousHorizontalMaxDistance * getCuriousVerticalMaxPercentage, 0f, getCuriousVerticalOffset);
                    localSixthOffset = new Vector3(sixthSenseHorizontal, 0f, -sixthSenseVerticalOffset);
                    break;
                case 1:
                    localSpottedOffset = new Vector3(getSpottedHorizontalMaxDistance, 0f, getSpottedVerticalMaxDistance * getSpottedHorizontalMaxPercentage + getSpottedVerticalOffset);
                    localCuriousOffset = new Vector3(getCuriousHorizontalMaxDistance, 0f, getCuriousVerticalMaxDistance * getCuriousHorizontalMaxPercentage + getCuriousVerticalOffset);
                    localSixthOffset = new Vector3(sixthSenseHorizontal, 0f, -sixthSenseVertical - sixthSenseVerticalOffset);
                    break;
                case 2:
                    localSpottedOffset = new Vector3(getSpottedHorizontalMaxDistance * getSpottedVerticalMaxPercentage, 0f, getSpottedVerticalMaxDistance + getSpottedVerticalOffset);
                    localCuriousOffset = new Vector3(getCuriousHorizontalMaxDistance * getCuriousVerticalMaxPercentage, 0f, getCuriousVerticalMaxDistance + getCuriousVerticalOffset);
                    localSixthOffset = new Vector3(sixthSenseHorizontal * sixthSenseAnglePercentage, 0f, -sixthSenseVertical - sixthSenseVerticalOffset);
                    break;
            }

            Vector3 inverseSpottedOffset =
                new Vector3(-localSpottedOffset.x, localSpottedOffset.y, localSpottedOffset.z);

            Vector3 inverseCuriousOffset =
                new Vector3(-localCuriousOffset.x, localCuriousOffset.y, localCuriousOffset.z);

            Vector3 inverseSixthOffset =
                new Vector3(-localSixthOffset.x, localSixthOffset.y, localSixthOffset.z);

            getSpottedCoordinates[i] = new Vector2(localSpottedOffset.x, localSpottedOffset.z);
            getSpottedCoordinates[5 - i] = new Vector2(inverseSpottedOffset.x, inverseSpottedOffset.z);

            getCuriousCoordinates[i] = new Vector2(localCuriousOffset.x, localCuriousOffset.z);
            getCuriousCoordinates[5 - i] = new Vector2(inverseCuriousOffset.x, inverseCuriousOffset.z);

            sixthSenseCoordinates[i] = new Vector2(localSixthOffset.x, localSixthOffset.z);
            sixthSenseCoordinates[i + 3] = new Vector2(inverseSixthOffset.x, inverseSixthOffset.z);
        }
    }

    private void Update()
    {
        //runs only 3 times per second
        if (Time.frameCount % (20+enemyNumber) != 0) return;

        //Debug.Log($"running on frame {Time.frameCount}!");

        Vector3 headPosition = headBone.position;
        Quaternion headRotation = headBone.rotation;

        inRange = false;

        //todo (broken) - need to account for head rotation
        bool inHorizontalRange = _player.transform.position.x < headPosition.x + (headRotation * getSpottedCoordinates[0]).x;
            //&& _player.transform.position.x > headPosition.x + (headRotation * getSpottedCoordinates[5]).x;

        //bool inVerticalRange = _player.transform.position.z < headPosition.z + (headRotation * getSpottedCoordinates[2]).y
        //    && _player.transform.position.z > headPosition.z + (headRotation * getSpottedCoordinates[0]).y;

        if (inHorizontalRange)
        {
            inRange = true;
        }
    }

    private void OnDrawGizmos()
    {
        Transform headBone = enemy.GetBoneTransform(HumanBodyBones.Head);
        Vector3 headPosition = headBone.position;
        Quaternion headRotation = headBone.rotation;

        for (int i = 0; i < 3; i++)
        {
            Vector3 localSpottedOffset = new();
            Vector3 localCuriousOffset = new();
            Vector3 localSixthOffset = new();

            switch (i)
            {
                case 0:
                    localSpottedOffset = new Vector3(getSpottedHorizontalMaxDistance * getSpottedVerticalMaxPercentage, 0f, getSpottedVerticalOffset);
                    localCuriousOffset = new Vector3(getCuriousHorizontalMaxDistance * getCuriousVerticalMaxPercentage, 0f, getCuriousVerticalOffset);
                    localSixthOffset = new Vector3(sixthSenseHorizontal, 0f, -sixthSenseVerticalOffset);
                    break;
                case 1:
                    localSpottedOffset = new Vector3(getSpottedHorizontalMaxDistance, 0f, getSpottedVerticalMaxDistance * getSpottedHorizontalMaxPercentage + getSpottedVerticalOffset);
                    localCuriousOffset = new Vector3(getCuriousHorizontalMaxDistance, 0f, getCuriousVerticalMaxDistance * getCuriousHorizontalMaxPercentage + getCuriousVerticalOffset);
                    localSixthOffset = new Vector3(sixthSenseHorizontal, 0f, -sixthSenseVertical - sixthSenseVerticalOffset);
                    break;
                case 2:
                    localSpottedOffset = new Vector3(getSpottedHorizontalMaxDistance * getSpottedVerticalMaxPercentage, 0f, getSpottedVerticalMaxDistance + getSpottedVerticalOffset);
                    localCuriousOffset = new Vector3(getCuriousHorizontalMaxDistance * getCuriousVerticalMaxPercentage, 0f, getCuriousVerticalMaxDistance + getCuriousVerticalOffset);
                    localSixthOffset = new Vector3(sixthSenseHorizontal * sixthSenseAnglePercentage, 0f, -sixthSenseVertical - sixthSenseVerticalOffset);
                    break;
            }

            Vector3 inverseSpottedOffset =
                new Vector3(-localSpottedOffset.x, localSpottedOffset.y, localSpottedOffset.z);

            Vector3 inverseCuriousOffset =
                new Vector3(-localCuriousOffset.x, localCuriousOffset.y, localCuriousOffset.z);

            Vector3 inverseSixthOffset =
                new Vector3(-localSixthOffset.x, localSixthOffset.y, localSixthOffset.z);

            getSpottedCoordinates[i] = new Vector2(localSpottedOffset.x, localSpottedOffset.z);
            getSpottedCoordinates[5 - i] = new Vector2(inverseSpottedOffset.x, inverseSpottedOffset.z);

            getCuriousCoordinates[i] = new Vector2(localCuriousOffset.x, localCuriousOffset.z);
            getCuriousCoordinates[5 - i] = new Vector2(inverseCuriousOffset.x, inverseCuriousOffset.z);

            sixthSenseCoordinates[i] = new Vector2(localSixthOffset.x, localSixthOffset.z);
            sixthSenseCoordinates[i + 3] = new Vector2(inverseSixthOffset.x, inverseSixthOffset.z);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(headPosition + (headRotation * localSpottedOffset), 0.1f);
            Gizmos.DrawSphere(headPosition + (headRotation * inverseSpottedOffset), 0.1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(headPosition + (headRotation * localCuriousOffset), 0.1f);
            Gizmos.DrawSphere(headPosition + (headRotation * inverseCuriousOffset), 0.1f);

            if (sixthSense)
            {
                Gizmos.color = immediateSense ? Color.red : Color.yellow;
                Gizmos.DrawSphere(headPosition + (headRotation * localSixthOffset), 0.1f);
                Gizmos.DrawSphere(headPosition + (headRotation * inverseSixthOffset), 0.1f);
            }
        }

        for (int i = 0; i < 6; i++)
        {
            Quaternion flip90 = headRotation * Quaternion.Euler(90, 0, 0);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(headPosition + (flip90 * getSpottedCoordinates[i]), headPosition + (flip90 * getSpottedCoordinates[(i + 1) % 6]));

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(headPosition + (flip90 * getCuriousCoordinates[i]), headPosition + (flip90 * getCuriousCoordinates[(i + 1) % 6]));
        }

        if (sixthSense)
        {
            Vector3 sixthSenseOffset = headPosition + new Vector3(0f, 0f, -sixthSenseVerticalOffset);

            Quaternion flip90 = headRotation * Quaternion.Euler(90, 0, 0);
            Gizmos.color = immediateSense ? Color.red : Color.yellow;

            Gizmos.DrawLine(sixthSenseOffset, headPosition + (flip90 * sixthSenseCoordinates[0]));
            Gizmos.DrawLine(sixthSenseOffset, headPosition + (flip90 * sixthSenseCoordinates[3]));

            Gizmos.DrawLine(headPosition + (flip90 * sixthSenseCoordinates[2]), sixthSenseOffset);
            Gizmos.DrawLine(headPosition + (flip90 * sixthSenseCoordinates[5]), sixthSenseOffset);

            for (int i = 0; i < 2; i++)
            {
                Gizmos.DrawLine(headPosition + (flip90 * sixthSenseCoordinates[i]), headPosition + (flip90 * sixthSenseCoordinates[i + 1]));
                Gizmos.DrawLine(headPosition + (flip90 * sixthSenseCoordinates[i + 3]), headPosition + (flip90 * sixthSenseCoordinates[3 + i + 1]));
            }
        }

        Gizmos.color = Color.white;

        DebugDrawRangeLines();
    }

    private void DebugDrawRangeLines()
    {
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
