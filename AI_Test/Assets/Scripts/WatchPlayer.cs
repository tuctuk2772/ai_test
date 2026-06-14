using UnityEngine;

public class WatchPlayer : MonoBehaviour
{
    [SerializeField] private Animator enemy;
    [SerializeField] private Animator _player;
    [Header("Variables")]
    [SerializeField, Range(0, 10)] private float lookAheadLength = 3f;

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(enemy.GetBoneTransform(HumanBodyBones.Head).position, enemy.GetBoneTransform(HumanBodyBones.Head).forward * lookAheadLength);


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
