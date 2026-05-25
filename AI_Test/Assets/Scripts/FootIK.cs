using UnityEngine;
using UnityEngine.Animations;

public class FootIK : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Space(10)]
    [SerializeField] private float raycastLength;

    private void Update()
    {
        DrawRaycast(HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, "left leg");
        DrawRaycast(HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, "right leg");
    }

    private void DrawRaycast(HumanBodyBones legBone, HumanBodyBones footBone, string debugString)
    {
        Transform leg = animator.GetBoneTransform(legBone);
        Transform foot = animator.GetBoneTransform(footBone);

        Ray ray = new Ray(leg.position, Vector3.down);

        bool rayHit = Physics.Raycast(ray, out RaycastHit hit, raycastLength);

        if (rayHit)
        {
            Quaternion footRotation = Quaternion.LookRotation(transform.forward, hit.normal);

            animator.SetIKPosition(AvatarIKGoal.LeftFoot, Vector3.zero);
        }
    }

    private void OnDrawGizmos()
    {
        Transform leftLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        Transform rightLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(leftLeg.position, leftLeg.position + (Vector3.down * raycastLength));
        Gizmos.DrawLine(rightLeg.position, rightLeg.position + (Vector3.down * raycastLength));
    }
}
