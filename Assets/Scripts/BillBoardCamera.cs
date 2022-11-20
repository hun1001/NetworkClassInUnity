using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BaseAxis
{
    up, down, left, right, forward, back
};

public class BillBoardCamera : MonoBehaviour
{
    public BaseAxis baseAxis = BaseAxis.up;
    Camera userCamera;
    public bool reverseCamera = false;

    Vector3 GetBaseAxis(BaseAxis refBaseAxis) => refBaseAxis switch
    {
        BaseAxis.up => Vector3.up,
        BaseAxis.down => Vector3.down,
        BaseAxis.left => Vector3.left,
        BaseAxis.right => Vector3.right,
        BaseAxis.forward => Vector3.forward,
        BaseAxis.back => Vector3.back,
        _ => Vector3.up,
    };

    private void Awake()
    {
        if (!userCamera)
        {
            userCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        Vector3 posTarget = transform.position + userCamera.transform.rotation * (reverseCamera ? Vector3.back : Vector3.forward);
        Vector3 targetRotation = userCamera.transform.rotation * GetBaseAxis(baseAxis);
        transform.LookAt(posTarget, targetRotation);
    }
}
