using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidCameraHandler : MonoBehaviour
{

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
    [Tooltip("경계선 가까이에있을 때 여백이 0보다 크면 카메라가 다시 튀어 나옵니다.이 변수는 스프링 백의 속도를 정의합니다.")]
    private float dragBackSpringFactor = 10;
    [SerializeField]
    [Tooltip("값이 낮을수록 카메라의 속도가 느려집니다. 값이 높을수록 카메라가 움직임 업데이트를보다 직접적으로 수행합니다. 프레임 속도가 터치 입력 업데이트 속도와 동기화되지 않을 때 카메라를 부드럽게 유지하는 데 필요합니다.")]
    private float camFollowFactor = 15.0f;
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

    [SerializeField]
    [Tooltip("이 값은 카메라의 스크롤 테두리를 정의합니다. 카메라가 여기서 정의한 것보다 더 크게 스크롤하지 않습니다. 하향식 카메라를 사용하면이 두 값이 X / Z 위치에 적용됩니다.")]
    private Vector2 boundaryMin = new Vector2(-1000, -1000);
    [SerializeField]
    [Tooltip("이 값은 카메라의 스크롤 테두리를 정의합니다. 카메라가 여기서 정의한 것보다 더 크게 스크롤하지 않습니다. 하향식 카메라를 사용하면이 두 값이 X / Z 위치에 적용됩니다.")]
    private Vector2 boundaryMax = new Vector2(1000, 1000);


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

    public bool IsPinching { get; private set; }
    public bool IsDragging { get; private set; }

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

    private List<Vector3> DragCameraMoveVector { get; set; }
    private const int momentumSamplesCount = 5;

    public bool IsSmoothingEnabled { get; set; }


    private bool useUntransformedCamBoundary = false;
    //이 값은 카메라가 기울기가 낮을 때 조정해야 할 수도 있습니다 (수평선 시나리오 위 참조).
    private float maxHorizonFallbackDistance = 1000;

    private void InputControllerOnDragStart(Vector3 dragPosStart, bool isLongTap)
    {
        Debug.LogFormat("DragStart {0}", dragPosStart);

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
        Debug.LogFormat("DragUpdate {0} {1} {2}", dragPosStart, dragPosCurrent, correctionOffset);

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
        Debug.LogFormat("DragStop {0} {1}", dragStopPos, dragFinalMomentum);


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
        if (IsPinching == true)
        {
            return;
        }

        if (IsDragging == true || IsPinching == true)
        {
            Vector3 posOld = cam.Transform.position;
            if (IsSmoothingEnabled == true)
            {
                cam.Transform.position = Vector3.Lerp(cam.Transform.position, targetPositionClamped, Mathf.Clamp01(Time.unscaledDeltaTime * camFollowFactor));
            }
            else
            {
                cam.Transform.position = targetPositionClamped;
            }
            DragCameraMoveVector.Add((posOld - cam.Transform.position) / Time.unscaledDeltaTime);
            if (DragCameraMoveVector.Count > momentumSamplesCount)
            {
                DragCameraMoveVector.RemoveAt(0);
            }
        }

        Vector2 autoScrollVector = -cameraScrollVelocity * deltaTime;
        Vector3 camPos = cam.Transform.position;
        camPos.x += autoScrollVector.x;
        camPos.y += autoScrollVector.y;

        if (IsDragging == false && IsPinching == false)
        {
            Vector3 overdragSpringVector = ComputeOverdragSpringBackVector(camPos, CamOverdragMargin2d, ref cameraScrollVelocity);
            if (overdragSpringVector.magnitude > float.Epsilon)
            {
                camPos += Time.unscaledDeltaTime * overdragSpringVector * dragBackSpringFactor;
            }
        }

        cam.Transform.position = GetClampToBoundaries(camPos);
    }

    /// <summary>
    /// 사용자가 경계에 가까워지면 카메라 끌기 스프링을 계산합니다.
    /// </summary>
    private Vector3 ComputeOverdragSpringBackVector(Vector3 camPos, Vector2 margin, ref Vector3 currentCamScrollVelocity)
    {
        Vector3 springBackVector = Vector3.zero;
        if (camPos.x < CamPosMin.x + margin.x)
        {
            springBackVector.x = (CamPosMin.x + margin.x) - camPos.x;
            currentCamScrollVelocity.x = 0;
        }
        else if (camPos.x > CamPosMax.x - margin.x)
        {
            springBackVector.x = (CamPosMax.x - margin.x) - camPos.x;
            currentCamScrollVelocity.x = 0;
        }


        if (camPos.z < CamPosMin.y + margin.y)
        {
            springBackVector.z = (CamPosMin.y + margin.y) - camPos.z;
            currentCamScrollVelocity.z = 0;
        }
        else if (camPos.z > CamPosMax.y - margin.y)
        {
            springBackVector.z = (CamPosMax.y - margin.y) - camPos.z;
            currentCamScrollVelocity.z = 0;
        }

        return springBackVector;
    }

    /// <summary>
    /// 카메라의 현재 회전 및 기울기에 사용되는 캠 경계를 계산하는 메서드입니다.
    /// 이 계산은 복잡하기 때문에 카메라를 회전하거나 기울일 때 호출해야합니다.
    /// </summary>
    private void ComputeCamBoundaries()
    {
        if (useUntransformedCamBoundary == true)
        {
            CamPosMin = boundaryMin;
            CamPosMax = boundaryMax;
        }
        else
        {

            float camRotation = GetRotationDeg();

            Vector2 camProjectedMin = Vector2.zero;
            Vector2 camProjectedMax = Vector2.zero;

            Vector2 camProjectedCenter = GetIntersection2d(new Ray(cam.Transform.position, -RefPlane.normal)); //Get camera position projected vertically onto the ref plane. This allows to compute the offset that arises from camera tilt.

            //지상에 투영 된 세계 공간 좌표로 카메라 경계를 가져옵니다.
            Vector2 camRight = GetIntersection2d(Cam.ScreenPointToRay(new Vector3(Screen.width, Screen.height * 0.5f, 0)));
            Vector2 camLeft = GetIntersection2d(Cam.ScreenPointToRay(new Vector3(0, Screen.height * 0.5f, 0)));
            Vector2 camUp = GetIntersection2d(Cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height, 0)));
            Vector2 camDown = GetIntersection2d(Cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, 0, 0)));
            camProjectedMin = GetVector2Min(camRight, camLeft, camUp, camDown);
            camProjectedMax = GetVector2Max(camRight, camLeft, camUp, camDown);

            //boundaryMin / Max에서 회전 된 경계 상자 만들기
            Vector2 computeBoundaryMin, computeBoundaryMax;
            RotateBoundingBox(boundaryMin, boundaryMax, -camRotation, out computeBoundaryMin, out computeBoundaryMax);

            Vector2 projectionCorrectionMin = new Vector2(camProjectedCenter.x - camProjectedMin.x, camProjectedCenter.y - camProjectedMin.y);
            Vector2 projectionCorrectionMax = new Vector2(camProjectedCenter.x - camProjectedMax.x, camProjectedCenter.y - camProjectedMax.y);

            CamPosMin = boundaryMin + projectionCorrectionMin;
            CamPosMax = boundaryMax + projectionCorrectionMax;

            Vector2 margin = CamOverdragMargin2d;
            if (CamPosMax.x - CamPosMin.x < margin.x * 2)
            {
                float midPoint = (CamPosMax.x + CamPosMin.x) * 0.5f;
                CamPosMax = new Vector2(midPoint + margin.x, CamPosMax.y);
                CamPosMin = new Vector2(midPoint - margin.x, CamPosMin.y);
            }

            if (CamPosMax.y - CamPosMin.y < margin.y * 2)
            {
                float midPoint = (CamPosMax.y + CamPosMin.y) * 0.5f;
                CamPosMax = new Vector2(CamPosMax.x, midPoint + margin.y);
                CamPosMin = new Vector2(CamPosMin.x, midPoint - margin.y);
            }
        }
    }

    /// <summary>
    /// Returns the rotation of the camera.
    /// </summary>
    private float GetRotationDeg()
    {
        return (cam.Transform.rotation.eulerAngles.z);
    }

    /// <summary>
    /// 2 차원 공간에서 정의 된 지면과 주어진 광선의 교차점을 검색하는 방법.
    /// </summary>
    private Vector2 GetIntersection2d(Ray ray)
    {
        Vector3 intersection3d = GetIntersectionPoint(ray);
        Vector2 intersection2d = new Vector2(intersection3d.x, 0);
        intersection2d.y = intersection3d.y;
        return (intersection2d);
    }

    private Vector2 GetVector2Min(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        return new Vector2(Mathf.Min(v0.x, v1.x, v2.x, v3.x), Mathf.Min(v0.y, v1.y, v2.y, v3.y));
    }

    private Vector2 GetVector2Max(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        return new Vector2(Mathf.Max(v0.x, v1.x, v2.x, v3.x), Mathf.Max(v0.y, v1.y, v2.y, v3.y));
    }


    /// <summary>
    /// 경계 상자를 회전하는 도우미 메서드입니다.
    /// </summary>
    private void RotateBoundingBox(Vector2 min, Vector2 max, float rotationDegrees, out Vector2 resultMin, out Vector2 resultMax)
    {
        Vector2 v0 = new Vector2(max.x, 0);
        Vector2 v1 = new Vector2(0, max.y);
        Vector2 v2 = new Vector2(min.x, 0);
        Vector2 v3 = new Vector2(0, min.y);
        Vector2 v0Rot = RotateVector2(v0, rotationDegrees);
        Vector2 v1Rot = RotateVector2(v1, rotationDegrees);
        Vector2 v2Rot = RotateVector2(v2, rotationDegrees);
        Vector2 v3Rot = RotateVector2(v3, rotationDegrees);
        resultMin = new Vector2(Mathf.Min(v0Rot.x, v1Rot.x, v2Rot.x, v3Rot.x), Mathf.Min(v0Rot.y, v1Rot.y, v2Rot.y, v3Rot.y));
        resultMax = new Vector2(Mathf.Max(v0Rot.x, v1Rot.x, v2Rot.x, v3Rot.x), Mathf.Max(v0Rot.y, v1Rot.y, v2Rot.y, v3Rot.y));
    }

    /// <summary>
    /// 주어진 각도로 Vector2를 회전합니다.
    /// </summary>
    private Vector2 RotateVector2(Vector2 v, float degrees)
    {

        Vector2 vNormalized = v.normalized;
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = vNormalized.x;
        float ty = vNormalized.y;
        vNormalized.x = (cos * tx) - (sin * ty);
        vNormalized.y = (sin * tx) + (cos * ty);
        return vNormalized * v.magnitude;
    }

}
