using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidCameraHandler : MonoBehaviour {

    private MultiPlatformInputSystem inputSystem;
    private RTSCamera cam;


    private void Awake()
    {
        inputSystem = GM.Instance.InputSystem;
        cam = this.GetComponent<RTSCamera>();


        //
        Cam = GetComponent<Camera>();
        dragStartCamPos = Vector3.zero;
        cameraScrollVelocity = Vector3.zero;
        timeRealDragStop = 0;
        IsDragging = false;
        DragCameraMoveVector = new List<Vector3>();


    }

    private void Start()
    {
        //inputSystem.OnInputClick += InputControllerOnInputClick;
        inputSystem.OnDragStart += InputControllerOnDragStart;
        inputSystem.OnDragUpdate += InputControllerOnDragUpdate;
        inputSystem.OnDragStop += InputControllerOnDragStop;
        //inputSystem.OnFingerDown += InputControllerOnFingerDown;
        //inputSystem.OnFingerUp += InputControllerOnFingerUp;
        //inputSystem.OnPinchStart += InputControllerOnPinchStart;
        //inputSystem.OnPinchUpdateExtended += InputControllerOnPinchUpdate;
        //inputSystem.OnPinchStop += InputControllerOnPinchStop;

    }

    [SerializeField]
    [Tooltip("정의 된 경계선 가까이에서 카메라를 드래그하면 사용자가 드래그를 멈 추면 다시 튀어 나옵니다. 이 값은 카메라가 튀어 나오는 경계선까지의 거리를 정의합니다.")]
    private float camOverdragMargin = 5.0f;
    [SerializeField]
    [Tooltip("화면을 스와이프하면 카메라가 잠시 멈추기 전에 스크롤됩니다. 이 변수는 자동 스크롤의 최대 속도를 제한합니다.")]
    private float autoScrollVelocityMax = 60;
    [SerializeField]
    [Tooltip("이 플래그를 true로 설정하면 균일 한 camOverdragMargin이 camOverdragMargin2d에 설정된 값으로 대체됩니다.")]
    private bool is2dOverdragMarginEnabled = false;
    [SerializeField]
    [Tooltip("이 필드를 사용하면 수평 오버 드래그를 수직 오버 드래그와 다른 값으로 설정할 수 있습니다.")]
    private Vector2 camOverdragMargin2d = Vector2.one * 5.0f;

    public Camera Cam { get; private set; }

    private Vector3 targetPositionClamped = Vector3.zero;

    private Vector2 CamPosMin { get; set; }
    private Vector2 CamPosMax { get; set; }

    public Vector2 CamOverdragMargin2d
    {
        get
        {
            if (is2dOverdragMarginEnabled)
            {
                return camOverdragMargin2d;
            }
            else
            {
                return Vector2.one * camOverdragMargin;
            }
        }
        set
        {
            camOverdragMargin2d = value;
            camOverdragMargin = value.x;
        }
    }

    /// <summary>
    /// 오브젝트 드래깅 상태
    /// </summary>
    private bool isDraggingSceneObject;

    private Plane refPlaneXY = new Plane(new Vector3(0, 0, -1), 0);
    private Plane refPlaneXZ = new Plane(new Vector3(0, 1, 0), 0);
    public Plane RefPlane
    {
        get
        {
            if (false)
            {
                return refPlaneXZ;
            }
            else
            {
                return refPlaneXY;
            }
        }
    }

    private float timeRealDragStop;

    private Vector3 dragStartCamPos;
    private Vector3 cameraScrollVelocity;

    public bool IsDragging { get; private set; }

    private List<Vector3> DragCameraMoveVector { get; set; }


    //이 값은 카메라가 기울기가 낮을 때 조정해야 할 수도 있습니다 (수평선 시나리오 위 참조).
    private float maxHorizonFallbackDistance = 1000;

    private void InputControllerOnDragStart(Vector3 dragPosStart, bool isLongTap)
    {
        if (isDraggingSceneObject == false)
        {
            cameraScrollVelocity = Vector3.zero;
            dragStartCamPos = cam.Transform.position;
            IsDragging = true;
            DragCameraMoveVector.Clear();
            SetTargetPosition(cam.Transform.position);
        }
    }

    private void InputControllerOnDragUpdate(Vector3 dragPosStart, Vector3 dragPosCurrent, Vector3 correctionOffset)
    {
        if (isDraggingSceneObject == false)
        {
            Vector3 dragVector = GetDragVector(dragPosStart, dragPosCurrent + correctionOffset);
            Vector3 posNewClamped = GetClampToBoundaries(dragStartCamPos - dragVector);
            SetTargetPosition(posNewClamped);
        }
        else
        {
            IsDragging = false;
        }
    }

    private void InputControllerOnDragStop(Vector3 dragStopPos, Vector3 dragFinalMomentum)
    {
        if (isDraggingSceneObject == false)
        {
            cameraScrollVelocity = GetVelocityFromMoveHistory();
            if (cameraScrollVelocity.sqrMagnitude >= autoScrollVelocityMax * autoScrollVelocityMax)
            {
                cameraScrollVelocity = cameraScrollVelocity.normalized * autoScrollVelocityMax;
            }
            timeRealDragStop = Time.realtimeSinceStartup;
            DragCameraMoveVector.Clear();
        }
        IsDragging = false;
    }


    private Vector3 GetDragVector(Vector3 dragPosStart, Vector3 dragPosCurrent)
    {
        Vector3 intersectionDragStart = GetIntersectionPoint(Cam.ScreenPointToRay(dragPosStart));
        Vector3 intersectionDragCurrent = GetIntersectionPoint(Cam.ScreenPointToRay(dragPosCurrent));
        return (intersectionDragCurrent - intersectionDragStart);
    }

    /// <summary>
    /// Method for retrieving the intersection-point between the given ray and the ref plane.
    /// 지정된 광선과 참조 평면 사이의 교차점을 검색하는 방법입니다.
    /// </summary>
    public Vector3 GetIntersectionPoint(Ray ray)
    {
        float distance = 0;
        bool success = RefPlane.Raycast(ray, out distance);
        //if (success == false || (Cam.orthographic == false && distance > maxHorizonFallbackDistance))
        //{

        //    //if (showHorizonError == true)
        //    //{
        //    //    Debug.LogError("Failed to compute intersection between camera ray and reference plane. Make sure the camera Axes are set up correctly.");
        //    //    showHorizonError = false;
        //    //}

        //    //Fallback: Compute a sphere-cap on the ground and use the border point at the direction of the ray as maximum point in the distance.
        //    Vector3 rayOriginProjected = UnprojectVector2(ProjectVector3(ray.origin));
        //    Vector3 rayDirProjected = UnprojectVector2(ProjectVector3(ray.direction));
        //    return rayOriginProjected + rayDirProjected.normalized * maxHorizonFallbackDistance;
        //}
        return (ray.origin + ray.direction * distance);
    }

    /// <summary>
    /// Returns a position that is clamped to the defined boundary.
    /// 정의 끝난 경계에 클램프되는 위치를 돌려줍니다.
    /// </summary>
    public Vector3 GetClampToBoundaries(Vector3 newPosition, bool includeSpringBackMargin = false)
    {

        Vector2 margin = Vector2.zero;
        if (includeSpringBackMargin == true)
        {
            margin = CamOverdragMargin2d;
        }

        newPosition.x = Mathf.Clamp(newPosition.x, CamPosMin.x + margin.x, CamPosMax.x - margin.x);
        newPosition.y = Mathf.Clamp(newPosition.y, CamPosMin.y + margin.y, CamPosMax.y - margin.y);

        return (newPosition);
    }

    /// <summary>
    /// Helper method that computes the suggested auto cam velocity from
    /// the last few frames of the user drag.
    /// 사용자 드래그의 마지막 몇 프레임에서 제안 된 자동 카메라 속도를 계산하는 도우미 메서드입니다.
    /// </summary>
    private Vector3 GetVelocityFromMoveHistory()
    {
        Vector3 momentum = Vector3.zero;
        if (DragCameraMoveVector.Count > 0)
        {
            for (int i = 0; i < DragCameraMoveVector.Count; ++i)
            {
                momentum += DragCameraMoveVector[i];
            }
            momentum /= DragCameraMoveVector.Count;
        }
        return (momentum);
    }



    /// <summary>
    /// 원하는 캠 위치를 설정하기위한 내부 도우미 방법.
    /// </summary>
    private void SetTargetPosition(Vector3 newPositionClamped)
    {
        targetPositionClamped = newPositionClamped;
    }


    //----------------------------------------

    /// <summary>
    /// 사용자가 카메라를 움직일 때 필요한 모든 업데이트를 계산하는 메서드입니다.
    /// </summary>
    private void UpdatePosition(float deltaTime)
    {

        if (IsPinching == true && isPinchTiltMode == true)
        {
            return;
        }

        if (IsDragging == true || IsPinching == true)
        {
            Vector3 posOld = Transform.position;
            if (IsSmoothingEnabled == true)
            {
                Transform.position = Vector3.Lerp(Transform.position, targetPositionClamped, Mathf.Clamp01(Time.unscaledDeltaTime * camFollowFactor));
            }
            else
            {
                Transform.position = targetPositionClamped;
            }
            DragCameraMoveVector.Add((posOld - Transform.position) / Time.unscaledDeltaTime);
            if (DragCameraMoveVector.Count > momentumSamplesCount)
            {
                DragCameraMoveVector.RemoveAt(0);
            }
        }

        Vector2 autoScrollVector = -cameraScrollVelocity * deltaTime;
        Vector3 camPos = Transform.position;
        switch (cameraAxes)
        {
            case CameraPlaneAxes.XY_2D_SIDESCROLL:
                camPos.x += autoScrollVector.x;
                camPos.y += autoScrollVector.y;
                break;
            case CameraPlaneAxes.XZ_TOP_DOWN:
                camPos.x += autoScrollVector.x;
                camPos.z += autoScrollVector.y;
                break;
        }

        if (IsDragging == false && IsPinching == false)
        {
            Vector3 overdragSpringVector = ComputeOverdragSpringBackVector(camPos, CamOverdragMargin2d, ref cameraScrollVelocity);
            if (overdragSpringVector.magnitude > float.Epsilon)
            {
                camPos += Time.unscaledDeltaTime * overdragSpringVector * dragBackSpringFactor;
            }
        }

        Transform.position = GetClampToBoundaries(camPos);
    }

}
