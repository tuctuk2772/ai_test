using UnityEngine;

public class AutoCover : MonoBehaviour
{
    private Animator _animator;
    private bool _hasAnimator;

    private const float COVER_DETECTION_DISTANCE = 0.3f;
    private const float COVER_DETECTION_RADIUS = 0.15f;

    [SerializeField] private LayerMask debugLayer;

    private Vector3 coverNormal = Vector3.zero;
    private bool inCover = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _hasAnimator = TryGetComponent(out _animator);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        _hasAnimator = TryGetComponent(out _animator);

        if (!_hasAnimator)
        {
            return;
        }

        RaycastHit hit;

        if (Physics.SphereCast(_animator.GetBoneTransform(HumanBodyBones.UpperChest).position, COVER_DETECTION_RADIUS, transform.forward, out hit, COVER_DETECTION_DISTANCE, debugLayer))
        {
            _animator.GetBoneTransform(HumanBodyBones.Head).transform.position =
                _animator.GetBoneTransform(HumanBodyBones.Head).transform.position + (Vector3.up * 100f);
            inCover = true;
            coverNormal = hit.normal;
        }
        else
        {
            inCover = false;
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (inCover)
        {
            _animator.bodyRotation = Quaternion.Euler(coverNormal);
        }

    }
}
