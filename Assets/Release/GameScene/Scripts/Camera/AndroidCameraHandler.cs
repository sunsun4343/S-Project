using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidCameraHandler : MonoBehaviour
{

    private MultiPlatformInputSystem inputSystem;
    private RTSCamera cam;

    [SerializeField]
    [Tooltip("For perspective cameras this value denotes the min field of view used for zooming (field of view zoom), or the min distance to the ground (translation zoom). For orthographic cameras it denotes the min camera size.")]
    private float camZoomMin = 4;
    [SerializeField]
    [Tooltip("For perspective cameras this value denotes the max field of view used for zooming (field of view zoom), or the max distance to the ground (translation zoom). For orthographic cameras it denotes the max camera size.")]
    private float camZoomMax = 12;
    [SerializeField]
    [Tooltip("정의 된 경계선 가까이에서 카메라를 드래그하면 사용자가 드래그를 멈 추면 다시 튀어 나옵니다. 이 값은 카메라가 튀어 나오는 경계선까지의 거리를 정의합니다.")]
    private float camOverdragMargin = 5.0f;
    [SerializeField]
    [Tooltip("이 값은 카메라의 스크롤 테두리를 정의합니다. 카메라가 여기서 정의한 것보다 더 크게 스크롤하지 않습니다. 하향식 카메라를 사용하면이 두 값이 X / Z 위치에 적용됩니다.")]
    private Vector2 boundaryMin = new Vector2(-1000, -1000);
    [SerializeField]
    [Tooltip("이 값은 카메라의 스크롤 테두리를 정의합니다. 카메라가 여기서 정의한 것보다 더 크게 스크롤하지 않습니다. 하향식 카메라를 사용하면이 두 값이 X / Z 위치에 적용됩니다.")]
    private Vector2 boundaryMax = new Vector2(1000, 1000);
    [SerializeField]
    [Tooltip("값이 낮을수록 카메라의 속도가 느려집니다. 값이 높을수록 카메라가 움직임 업데이트를보다 직접적으로 수행합니다. 프레임 속도가 터치 입력 업데이트 속도와 동기화되지 않을 때 카메라를 부드럽게 유지하는 데 필요합니다.")]
    private float camFollowFactor = 15.0f;
    [SerializeField]
    [Tooltip("빠르게 드래그하면 카메라는 마지막 방향으로 자동 스크롤을 유지합니다. 자동 스크롤은 서서히 멈출 것입니다. 이 값은 카메라가 얼마나 빨리 멈출지를 정의합니다.")]
    private float autoScrollDamp = 300;
    [SerializeField]
    [Tooltip("이 커브는 자동 스크롤 저항 값을 시간에 따라 변조 할 수 있습니다.")]
    private AnimationCurve autoScrollDampCurve = new AnimationCurve(new Keyframe(0, 1, 0, 0), new Keyframe(0.7f, 0.9f, -0.5f, -0.5f), new Keyframe(1, 0.01f, -0.85f, -0.85f));
    [SerializeField]
    [Tooltip("카메라는 장면의 스크롤 가능한 콘텐츠 (예 : 게임 세계의 바닥)가 하향식 카메라의 경우 y = 0에 있거나 사이드 스크롤링 카메라의 경우 z = 0에 있다고 가정합니다. 이 장면에 대해 유효하지 않은 경우이 속성을 올바른 오프셋으로 조정할 수 있습니다.")]
    private float groundLevelOffset = 0;
    [SerializeField]
    [Tooltip("이 기능을 사용하면 두 손가락 회전 동작을 사용하여 카메라를 회전 할 수 있습니다.")]
    private bool enableRotation = false;
    [SerializeField]
    [Tooltip("경계선 가까이에있을 때 여백이 0보다 크면 카메라가 다시 튀어 나옵니다.이 변수는 스프링 백의 속도를 정의합니다.")]
    private float dragBackSpringFactor = 10;

    //--------------------------------------------


    private Vector3 dragStartCamPos;
    private Vector3 cameraScrollVelocity;

    //private float pinchStartCamZoomSize;
    //private Vector3 pinchStartIntersectionCenter;
    //private Vector3 pinchCenterCurrent;
    //private float pinchDistanceCurrent;
    //private float pinchAngleCurrent = 0;
    //private float pinchDistanceStart;
    //private Vector3 pinchCenterCurrentLerp;
    //private float pinchDistanceCurrentLerp;
    //private float pinchAngleCurrentLerp;
    //private bool isRotationLock = true;
    //private bool isRotationActivated = false;
    //private float pinchAngleLastFrame = 0;
    //private float pinchTiltCurrent = 0;
    //private float pinchTiltAccumulated = 0;
    //private bool isTiltModeEvaluated = false;
    //private float pinchTiltLastFrame;
    //private bool isPinchTiltMode;

    private float timeRealDragStop;

    //public bool IsAutoScrolling { get { return (cameraScrollVelocity.sqrMagnitude > float.Epsilon); } }

    public bool IsPinching { get; private set; }
    public bool IsDragging { get; private set; }

    [SerializeField]
    [Tooltip("설정에 따라 카메라가 정의 된 값을 약간 확대 할 수 있습니다. 줌을 해제하면 카메라가 정의 된 값으로 돌아갑니다. 이 변수는 스프링 백의 속도를 정의합니다.")]
    private float zoomBackSpringFactor = 20;
    [SerializeField]
    [Tooltip("화면을 스와이프하면 카메라가 잠시 멈추기 전에 스크롤됩니다. 이 변수는 자동 스크롤의 최대 속도를 제한합니다.")]
    private float autoScrollVelocityMax = 60;
    [SerializeField]
    [Tooltip("이 값은 자동 스크롤 할 때 카메라가 멈추는 속도를 정의합니다.")]
    private float dampFactorTimeMultiplier = 2;
    [SerializeField]
    [Tooltip("이 플래그를 true로 설정하면 균일 한 camOverdragMargin이 camOverdragMargin2d에 설정된 값으로 대체됩니다.")]
    private bool is2dOverdragMarginEnabled = false;
    [SerializeField]
    [Tooltip("이 필드를 사용하면 수평 오버 드래그를 수직 오버 드래그와 다른 값으로 설정할 수 있습니다.")]
    private Vector2 camOverdragMargin2d = Vector2.one * 5.0f;


    /// <summary>
    /// Start 함수 실행 여부
    /// </summary>
    private bool isStarted = false;

    public Camera Cam { get; private set; }

    private bool IsTranslationZoom { get { return false; } }

    public float CamZoom
    {
        get
        {
            if (Cam.orthographic == true)
            {
                return Cam.orthographicSize;
            }
            else
            {
                if (IsTranslationZoom == true)
                {
                    Vector3 camCenterIntersection = GetIntersectionPoint(GetCamCenterRay());
                    return (Vector3.Distance(camCenterIntersection, cam.Transform.position));
                }
                else
                {
                    return Cam.fieldOfView;
                }
            }
        }
        set
        {
            if (Cam.orthographic == true)
            {
                Cam.orthographicSize = value;
            }
            else
            {
                if (IsTranslationZoom == true)
                {
                    Vector3 camCenterIntersection = GetIntersectionPoint(GetCamCenterRay());
                    cam.Transform.position = camCenterIntersection - cam.Transform.forward * value;
                }
                else
                {
                    Cam.fieldOfView = value;
                }
            }
            ComputeCamBoundaries();
        }
    }

    public float CamZoomMin
    {
        get { return (camZoomMin); }
        set { camZoomMin = value; }
    }
    public float CamZoomMax
    {
        get { return (camZoomMax); }
        set { camZoomMax = value; }
    }
    //public float CamOverzoomMargin
    //{
    //    get { return (camOverzoomMargin); }
    //    set { camOverzoomMargin = value; }
    //}
    //public float CamFollowFactor
    //{
    //    get { return (camFollowFactor); }
    //    set { camFollowFactor = value; }
    //}
    //public float AutoScrollDamp
    //{
    //    get { return autoScrollDamp; }
    //    set { autoScrollDamp = value; }
    //}
    public AnimationCurve AutoScrollDampCurve
    {
        get { return (autoScrollDampCurve); }
        set { autoScrollDampCurve = value; }
    }
    public float GroundLevelOffset
    {
        get { return groundLevelOffset; }
        set { groundLevelOffset = value; }
    }
    //public Vector2 BoundaryMin
    //{
    //    get { return boundaryMin; }
    //    set { boundaryMin = value; }
    //}
    //public Vector2 BoundaryMax
    //{
    //    get { return boundaryMax; }
    //    set { boundaryMax = value; }
    //}
    //public PerspectiveZoomMode PerspectiveZoomMode
    //{
    //    get { return (perspectiveZoomMode); }
    //    set { perspectiveZoomMode = value; }
    //}
    public bool EnableRotation
    {
        get { return enableRotation; }
        set { enableRotation = value; }
    }
    //public bool EnableTilt
    //{
    //    get { return enableTilt; }
    //    set { enableTilt = value; }
    //}
    //public float TiltAngleMin
    //{
    //    get { return tiltAngleMin; }
    //    set { tiltAngleMin = value; }
    //}
    //public float TiltAngleMax
    //{
    //    get { return tiltAngleMax; }
    //    set { tiltAngleMax = value; }
    //}
    //public bool EnableZoomTilt
    //{
    //    get { return enableZoomTilt; }
    //    set { enableZoomTilt = value; }
    //}
    //public float ZoomTiltAngleMin
    //{
    //    get { return zoomTiltAngleMin; }
    //    set { zoomTiltAngleMin = value; }
    //}
    //public float ZoomTiltAngleMax
    //{
    //    get { return zoomTiltAngleMax; }
    //    set { zoomTiltAngleMax = value; }
    //}
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

    private List<Vector3> DragCameraMoveVector { get; set; }
    private const int momentumSamplesCount = 5;

    //private const float pinchDistanceForTiltBreakout = 0.05f;
    //private const float pinchAccumBreakout = 0.025f;

    private Vector3 targetPositionClamped = Vector3.zero;

    public bool IsSmoothingEnabled { get; set; }

    private Vector2 CamPosMin { get; set; }
    private Vector2 CamPosMax { get; set; }

    //public TerrainCollider TerrainCollider
    //{
    //    get { return terrainCollider; }
    //    set { terrainCollider = value; }
    //}

    //public Vector2 CameraScrollVelocity
    //{
    //    get { return cameraScrollVelocity; }
    //    set { cameraScrollVelocity = value; }
    //}

    //private bool showHorizonError = true;

    #region work in progress //Features that are currently being worked on, but not fully polished and documented yet. Use them at your own risk.
    //private bool enableOvertiltSpring = false; //Allows to enable the camera to spring being when being tilted over the limits.
    //private float camOvertiltMargin = 5.0f;
    //private float tiltBackSpringFactor = 30;
    //private float minOvertiltSpringPositionThreshold = 0.1f; //This value is necessary to reposition the camera and do boundary update computations while the auto spring back from overtilt is active and larger than this value.
    private bool useUntransformedCamBoundary = false;
    private float maxHorizonFallbackDistance = 1000; //이 값은 카메라가 기울기가 낮을 때 조정해야 할 수도 있습니다 (수평선 시나리오 위 참조).
    #endregion

#if UNITY_EDITOR
    private Vector3 mousePosLastFrame = Vector3.zero;
#endif

    private void Awake()
    {
        inputSystem = GM.Instance.InputSystem;
        cam = this.GetComponent<RTSCamera>();

        //
        //if (cameraTransform != null)
        //{
        //    cachedTransform = cameraTransform;
        //}
        Cam = GetComponent<Camera>();

        IsSmoothingEnabled = true;
        //touchInputController = GetComponent<TouchInputController>();
        dragStartCamPos = Vector3.zero;
        cameraScrollVelocity = Vector3.zero;
        timeRealDragStop = 0;
        //pinchStartCamZoomSize = 0;
        IsPinching = false;
        IsDragging = false;
        DragCameraMoveVector = new List<Vector3>();
        refPlaneXY = new Plane(new Vector3(0, 0, -1), groundLevelOffset);
        refPlaneXZ = new Plane(new Vector3(0, 1, 0), -groundLevelOffset);
        //if (EnableZoomTilt == true)
        //{
        //    ResetZoomTilt();
        //}
        ComputeCamBoundaries();

        //if (CamZoomMax < CamZoomMin)
        //{
        //    Debug.LogWarning("The defined max camera zoom (" + CamZoomMax + ") is smaller than the defined min (" + CamZoomMin + "). Automatically switching the values.");
        //    float camZoomMinBackup = CamZoomMin;
        //    CamZoomMin = CamZoomMax;
        //    CamZoomMax = camZoomMinBackup;
        //}

        //Errors for certain incorrect settings.
        //string cameraAxesError = CheckCameraAxesErrors();
        //if (string.IsNullOrEmpty(cameraAxesError) == false)
        //{
        //    Debug.LogError(cameraAxesError);
        //}
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
        isStarted = true;
        StartCoroutine(InitCamBoundariesDelayed());
    }

    public void OnDestroy()
    {
        if (isStarted)
        {
            //inputSystem.OnInputClick -= InputControllerOnInputClick;
            inputSystem.OnDragStart -= InputControllerOnDragStart;
            inputSystem.OnDragUpdate -= InputControllerOnDragUpdate;
            inputSystem.OnDragStop -= InputControllerOnDragStop;
            //inputSystem.OnFingerDown -= InputControllerOnFingerDown;
            //inputSystem.OnFingerUp -= InputControllerOnFingerUp;
            //inputSystem.OnPinchStart -= InputControllerOnPinchStart;
            //inputSystem.OnPinchUpdateExtended -= InputControllerOnPinchUpdate;
            //inputSystem.OnPinchStop -= InputControllerOnPinchStop;
        }
    }
    
    //----------------------------------------


    #region public interface
    ///// <summary>
    ///// Method for resetting the camera boundaries. This method may need to be invoked
    ///// when resetting the camera transform (rotation, tilt) by code for example.
    ///// </summary>
    //public void ResetCameraBoundaries()
    //{
    //    ComputeCamBoundaries();
    //}

    ///// <summary>
    ///// This method tilts the camera based on the values
    ///// defined for the zoom tilt mode.
    ///// </summary>
    //public void ResetZoomTilt()
    //{
    //    UpdateTiltForAutoTilt(CamZoom);
    //}

    ///// <summary>
    ///// Helper method for retrieving the world position of the
    ///// finger with id 0. This method may only return a valid value when
    ///// there is at least 1 finger touching the device.
    ///// </summary>
    //public Vector3 GetFinger0PosWorld()
    //{
    //    Vector3 posWorld = Vector3.zero;
    //    if (TouchWrapper.TouchCount > 0)
    //    {
    //        Vector3 fingerPos = TouchWrapper.Touch0.Position;
    //        RaycastGround(Cam.ScreenPointToRay(fingerPos), out posWorld);
    //    }
    //    return (posWorld);
    //}

    ///// <summary>
    ///// Method for performing a raycast against either the refplane, or
    ///// against a terrain-collider in case the collider is set.
    ///// </summary>
    //public bool RaycastGround(Ray ray, out Vector3 hitPoint)
    //{
    //    bool hitSuccess = false;
    //    hitPoint = Vector3.zero;
    //    if (TerrainCollider != null)
    //    {
    //        RaycastHit hitInfo;
    //        hitSuccess = TerrainCollider.Raycast(ray, out hitInfo, Mathf.Infinity);
    //        if (hitSuccess == true)
    //        {
    //            hitPoint = hitInfo.point;
    //        }
    //    }
    //    else
    //    {
    //        float hitDistance = 0;
    //        hitSuccess = RefPlane.Raycast(ray, out hitDistance);
    //        if (hitSuccess == true)
    //        {
    //            hitPoint = ray.GetPoint(hitDistance);
    //        }
    //    }
    //    return hitSuccess;
    //}

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

    ///// <summary>
    ///// Custom planet intersection method that doesn't take into account rays parallel to the plane or rays shooting in the wrong direction and thus never hitting.
    ///// May yield slightly better performance however and should be safe for use when the camera setup is correct (e.g. axes set correctly in this script, and camera actually pointing towards floor).
    ///// </summary>
    //public Vector3 GetIntersectionPointUnsafe(Ray ray)
    //{
    //    float distance = Vector3.Dot(RefPlane.normal, Vector3.zero - ray.origin) / Vector3.Dot(RefPlane.normal, (ray.origin + ray.direction) - ray.origin);
    //    return (ray.origin + ray.direction * distance);
    //}

    ///// <summary>
    ///// Returns whether or not the camera is at the defined boundary.
    ///// </summary>
    //public bool GetIsBoundaryPosition(Vector3 testPosition)
    //{
    //    return GetIsBoundaryPosition(testPosition, Vector2.zero);
    //}

    ///// <summary>
    ///// Returns whether or not the camera is at the defined boundary.
    ///// </summary>
    //public bool GetIsBoundaryPosition(Vector3 testPosition, Vector2 margin)
    //{

    //    bool isBoundaryPosition = false;
    //    switch (cameraAxes)
    //    {
    //        case CameraPlaneAxes.XY_2D_SIDESCROLL:
    //            isBoundaryPosition = testPosition.x <= CamPosMin.x + margin.x;
    //            isBoundaryPosition |= testPosition.x >= CamPosMax.x - margin.x;
    //            isBoundaryPosition |= testPosition.y <= CamPosMin.y + margin.y;
    //            isBoundaryPosition |= testPosition.y >= CamPosMax.y - margin.y;
    //            break;
    //        case CameraPlaneAxes.XZ_TOP_DOWN:
    //            isBoundaryPosition = testPosition.x <= CamPosMin.x + margin.x;
    //            isBoundaryPosition |= testPosition.x >= CamPosMax.x - margin.x;
    //            isBoundaryPosition |= testPosition.z <= CamPosMin.y + margin.y;
    //            isBoundaryPosition |= testPosition.z >= CamPosMax.y - margin.y;
    //            break;
    //    }
    //    return (isBoundaryPosition);
    //}

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

    //public void OnDragSceneObject()
    //{
    //    isDraggingSceneObject = true;
    //}

    //public string CheckCameraAxesErrors()
    //{
    //    string error = "";
    //    if (Transform.forward == Vector3.down && cameraAxes != CameraPlaneAxes.XZ_TOP_DOWN)
    //    {
    //        error = "Camera is pointing down but the cameraAxes is not set to TOP_DOWN. Make sure to set the cameraAxes variable properly.";
    //    }
    //    if (Transform.forward == Vector3.forward && cameraAxes != CameraPlaneAxes.XY_2D_SIDESCROLL)
    //    {
    //        error = "Camera is pointing sidewards but the cameraAxes is not set to 2D_SIDESCROLL. Make sure to set the cameraAxes variable properly.";
    //    }
    //    return (error);
    //}

    ///// <summary>
    ///// Helper method that unprojects the given Vector2 to a Vector3
    ///// according to the camera axes setting.
    ///// </summary>
    //public Vector3 UnprojectVector2(Vector2 v2, float offset = 0)
    //{
    //    if (CameraAxes == CameraPlaneAxes.XY_2D_SIDESCROLL)
    //    {
    //        return new Vector3(v2.x, v2.y, offset);
    //    }
    //    else
    //    {
    //        return new Vector3(v2.x, offset, v2.y);
    //    }
    //}

    //public Vector2 ProjectVector3(Vector3 v3)
    //{
    //    if (CameraAxes == CameraPlaneAxes.XY_2D_SIDESCROLL)
    //    {
    //        return new Vector2(v3.x, v3.y);
    //    }
    //    else
    //    {
    //        return new Vector2(v3.x, v3.z);
    //    }
    //}
    #endregion

    private IEnumerator InitCamBoundariesDelayed()
    {
        yield return null;
        ComputeCamBoundaries();
    }

    /// <summary>
    /// MonoBehaviour method override to assign proper default values depending on
    /// the camera parameters and orientation.
    /// </summary>
    //private void Reset()
    //{
    //    //Compute camera tilt to find out the camera orientation.
    //    Vector3 camForwardOnPlane = Vector3.Cross(Vector3.up, GetTiltRotationAxis());
    //    float tiltAngle = Vector3.Angle(camForwardOnPlane, -Transform.forward);
    //    if (tiltAngle < 45)
    //    {
    //        CameraAxes = CameraPlaneAxes.XY_2D_SIDESCROLL;
    //    }
    //    else
    //    {
    //        CameraAxes = CameraPlaneAxes.XZ_TOP_DOWN;
    //    }

    //    //Compute zoom default values based on the camera type.
    //    Camera cameraComponent = GetComponent<Camera>();
    //    if (cameraComponent.orthographic == true)
    //    {
    //        CamZoomMin = 4;
    //        CamZoomMax = 13;
    //        CamOverzoomMargin = 1;
    //    }
    //    else
    //    {
    //        CamZoomMin = 5;
    //        CamZoomMax = 40;
    //        CamOverzoomMargin = 3;
    //        PerspectiveZoomMode = PerspectiveZoomMode.TRANSLATION;
    //    }
    //}

    ///// <summary>
    ///// Method that does all the computation necessary when the pinch gesture of the user
    ///// has changed.
    ///// </summary>
    //private void UpdatePinch(float deltaTime)
    //{

    //    if (IsPinching == true)
    //    {

    //        if (isTiltModeEvaluated == true)
    //        {

    //            if (isPinchTiltMode == true || isPinchModeExclusive == false)
    //            {

    //                //Tilt
    //                float pinchTiltDelta = pinchTiltLastFrame - pinchTiltCurrent;
    //                UpdateCameraTilt(pinchTiltDelta * pinchTiltSpeed);
    //                pinchTiltLastFrame = pinchTiltCurrent;

    //            }
    //            if (isPinchTiltMode == false || isPinchModeExclusive == false)
    //            {

    //                if (isRotationActivated == true && isRotationLock == true && Mathf.Abs(pinchAngleCurrent) >= rotationLockThreshold)
    //                {
    //                    isRotationLock = false;
    //                }

    //                if (IsSmoothingEnabled == true)
    //                {
    //                    float lerpFactor = Mathf.Clamp01(Time.unscaledDeltaTime * camFollowFactor);
    //                    pinchDistanceCurrentLerp = Mathf.Lerp(pinchDistanceCurrentLerp, pinchDistanceCurrent, lerpFactor);
    //                    pinchCenterCurrentLerp = Vector3.Lerp(pinchCenterCurrentLerp, pinchCenterCurrent, lerpFactor);
    //                    if (isRotationLock == false)
    //                    {
    //                        pinchAngleCurrentLerp = Mathf.Lerp(pinchAngleCurrentLerp, pinchAngleCurrent, lerpFactor);
    //                    }
    //                }
    //                else
    //                {
    //                    pinchDistanceCurrentLerp = pinchDistanceCurrent;
    //                    pinchCenterCurrentLerp = pinchCenterCurrent;
    //                    if (isRotationLock == false)
    //                    {
    //                        pinchAngleCurrentLerp = pinchAngleCurrent;
    //                    }
    //                }

    //                //Rotation
    //                if (isRotationActivated == true && isRotationLock == false)
    //                {
    //                    float pinchAngleDelta = pinchAngleCurrentLerp - pinchAngleLastFrame;
    //                    Vector3 rotationAxis = GetRotationAxis();
    //                    Transform.RotateAround(pinchCenterCurrent, rotationAxis, pinchAngleDelta);
    //                    pinchAngleLastFrame = pinchAngleCurrentLerp;
    //                    ComputeCamBoundaries();
    //                }

    //                //Zoom
    //                float zoomFactor = (pinchDistanceStart / Mathf.Max(((pinchDistanceCurrentLerp - pinchDistanceStart) * customZoomSensitivity) + pinchDistanceStart, 0.0001f));
    //                float cameraSize = pinchStartCamZoomSize * zoomFactor;
    //                cameraSize = Mathf.Clamp(cameraSize, camZoomMin - camOverzoomMargin, camZoomMax + camOverzoomMargin);
    //                if (enableZoomTilt == true)
    //                {
    //                    UpdateTiltForAutoTilt(cameraSize);
    //                }
    //                CamZoom = cameraSize;
    //            }

    //            //Position update.
    //            DoPositionUpdateForTilt(false);
    //        }
    //    }
    //    else
    //    {
    //        //Spring back.
    //        if (EnableTilt == true && enableOvertiltSpring == true)
    //        {
    //            float overtiltSpringValue = ComputeOvertiltSpringBackFactor(camOvertiltMargin);
    //            if (Mathf.Abs(overtiltSpringValue) > minOvertiltSpringPositionThreshold)
    //            {
    //                UpdateCameraTilt(overtiltSpringValue * deltaTime * tiltBackSpringFactor);
    //                DoPositionUpdateForTilt(true);
    //            }
    //        }
    //    }
    //}

    //private void UpdateTiltForAutoTilt(float newCameraSize)
    //{
    //    float zoomProgress = Mathf.Clamp01((newCameraSize - camZoomMin) / (camZoomMax - camZoomMin));
    //    float tiltTarget = Mathf.Lerp(zoomTiltAngleMin, zoomTiltAngleMax, zoomProgress);
    //    float tiltAngleDiff = tiltTarget - GetCurrentTiltAngleDeg(GetTiltRotationAxis());
    //    UpdateCameraTilt(tiltAngleDiff);
    //}

    ///// <summary>
    ///// Method that computes the updated camera position when the user tilts the camera.
    ///// </summary>
    //private void DoPositionUpdateForTilt(bool isSpringBack)
    //{

    //    //Position update.
    //    Vector3 intersectionDragCurrent;
    //    if (isSpringBack == true || (isPinchTiltMode == true && isPinchModeExclusive == true))
    //    {
    //        intersectionDragCurrent = GetIntersectionPoint(GetCamCenterRay()); //In exclusive tilt mode always rotate around the screen center.
    //    }
    //    else
    //    {
    //        intersectionDragCurrent = GetIntersectionPoint(Cam.ScreenPointToRay(pinchCenterCurrentLerp));
    //    }
    //    Vector3 dragUpdateVector = intersectionDragCurrent - pinchStartIntersectionCenter;
    //    if (isSpringBack == true && isPinchModeExclusive == false)
    //    {
    //        dragUpdateVector = Vector3.zero;
    //    }
    //    Vector3 targetPos = GetClampToBoundaries(Transform.position - dragUpdateVector);

    //    Transform.position = targetPos; //Disable smooth follow for the pinch-move update to prevent oscillation during the zoom phase.
    //    SetTargetPosition(targetPos);
    //}

    ///// <summary>
    ///// Helper method for computing the tilt spring back.
    ///// </summary>
    //private float ComputeOvertiltSpringBackFactor(float margin)
    //{

    //    float springBackValue = 0;
    //    Vector3 rotationAxis = GetTiltRotationAxis();
    //    float tiltAngle = GetCurrentTiltAngleDeg(rotationAxis);
    //    if (tiltAngle < tiltAngleMin + margin)
    //    {
    //        springBackValue = (tiltAngleMin + margin) - tiltAngle;
    //    }
    //    else if (tiltAngle > tiltAngleMax - margin)
    //    {
    //        springBackValue = (tiltAngleMax - margin) - tiltAngle;
    //    }
    //    return springBackValue;
    //}

    ///// <summary>
    ///// Method that computes all necessary parameters for a tilt update caused by the user's tilt gesture.
    ///// </summary>
    //private void UpdateCameraTilt(float angle)
    //{
    //    Vector3 rotationAxis = GetTiltRotationAxis();
    //    Vector3 rotationPoint = GetIntersectionPoint(new Ray(Transform.position, Transform.forward));
    //    Transform.RotateAround(rotationPoint, rotationAxis, angle);
    //    ClampCameraTilt(rotationPoint, rotationAxis);
    //    ComputeCamBoundaries();
    //}

    ///// <summary>
    ///// Method that ensures that all limits are met when the user tilts the camera.
    ///// </summary>
    //private void ClampCameraTilt(Vector3 rotationPoint, Vector3 rotationAxis)
    //{

    //    float tiltAngle = GetCurrentTiltAngleDeg(rotationAxis);
    //    if (tiltAngle < tiltAngleMin)
    //    {
    //        float tiltClampDiff = tiltAngleMin - tiltAngle;
    //        Transform.RotateAround(rotationPoint, rotationAxis, tiltClampDiff);
    //    }
    //    else if (tiltAngle > tiltAngleMax)
    //    {
    //        float tiltClampDiff = tiltAngleMax - tiltAngle;
    //        Transform.RotateAround(rotationPoint, rotationAxis, tiltClampDiff);
    //    }
    //}

    ///// <summary>
    ///// Method to get the current tilt angle of the camera.
    ///// </summary>
    //private float GetCurrentTiltAngleDeg(Vector3 rotationAxis)
    //{
    //    Vector3 camForwardOnPlane = Vector3.Cross(RefPlane.normal, rotationAxis);
    //    float tiltAngle = Vector3.Angle(camForwardOnPlane, -Transform.forward);
    //    return (tiltAngle);
    //}

    /// <summary>
    /// Returns the rotation axis of the camera. This purely depends
    /// on the defined camera axis.
    /// </summary>
    private Vector3 GetRotationAxis()
    {
        return (RefPlane.normal);
    }

    /// <summary>
    /// Returns the rotation of the camera.
    /// </summary>
    private float GetRotationDeg()
    {
        return (cam.Transform.rotation.eulerAngles.z);
    }

    ///// <summary>
    ///// Returns the tilt rotation axis.
    ///// </summary>
    //private Vector3 GetTiltRotationAxis()
    //{
    //    Vector3 rotationAxis = Transform.right;
    //    return (rotationAxis);
    //}

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
    /// 원하는 캠 위치를 설정하기위한 내부 도우미 방법.
    /// </summary>
    private void SetTargetPosition(Vector3 newPositionClamped)
    {
        targetPositionClamped = newPositionClamped;
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

    public void Update()
    {

        #region auto scroll code

        if (cameraScrollVelocity.sqrMagnitude > float.Epsilon)
        {
            float timeSinceDragStop = Time.realtimeSinceStartup - timeRealDragStop;
            float dampFactor = Mathf.Clamp01(timeSinceDragStop * dampFactorTimeMultiplier);
            float camScrollVel = cameraScrollVelocity.magnitude;
            float camScrollVelRelative = camScrollVel / autoScrollVelocityMax;
            Vector3 camVelDamp = dampFactor * cameraScrollVelocity.normalized * autoScrollDamp * Time.unscaledDeltaTime;
            camVelDamp *= EvaluateAutoScrollDampCurve(1.0f - camScrollVelRelative);
            if (camVelDamp.sqrMagnitude >= cameraScrollVelocity.sqrMagnitude)
            {
                cameraScrollVelocity = Vector3.zero;
            }
            else
            {
                cameraScrollVelocity -= camVelDamp;
            }
        }

        #endregion

    }

    public void LateUpdate()
    {

        //Pinch.
        //UpdatePinch(Time.unscaledDeltaTime);

        //Translation.
        UpdatePosition(Time.unscaledDeltaTime);

        #region editor codepath
#if UNITY_EDITOR
        //Allow to use the middle mouse wheel in editor to be able to zoom without touch device during development.
        float mouseScrollDelta = Input.GetAxis("Mouse ScrollWheel");
        Vector3 mousePosDelta = mousePosLastFrame - Input.mousePosition;
        bool isEditorInputRotate = false;
        bool isEditorInputTilt = false;

        if (Input.GetMouseButton(1))
        {
            const float mouseRotationFactor = 0.01f;
            mouseScrollDelta = mousePosDelta.x * mouseRotationFactor;
            isEditorInputRotate = true;
        }
        else if (Input.GetMouseButton(2))
        {
            const float mouseTiltFactor = 0.01f;
            mouseScrollDelta = mousePosDelta.y * mouseTiltFactor;
            isEditorInputTilt = true;
        }
        bool anyModifierPressed = Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        if (anyModifierPressed == true)
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                mouseScrollDelta = 0.05f;
            }
            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                mouseScrollDelta = -0.05f;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                mouseScrollDelta = 0.05f;
                isEditorInputRotate = true;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                mouseScrollDelta = -0.05f;
                isEditorInputRotate = true;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                mouseScrollDelta = 0.05f;
                isEditorInputTilt = true;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                mouseScrollDelta = -0.05f;
                isEditorInputTilt = true;
            }
        }

        if (Mathf.Approximately(mouseScrollDelta, 0) == false)
        {
            if (isEditorInputRotate == true)
            {
                if (EnableRotation == true)
                {
                    Vector3 rotationAxis = GetRotationAxis();
                    Vector3 intersectionScreenCenter = GetIntersectionPoint(Cam.ScreenPointToRay(Input.mousePosition));
                    cam.Transform.RotateAround(intersectionScreenCenter, rotationAxis, mouseScrollDelta * 100);
                    ComputeCamBoundaries();
                }
            }
            else if (isEditorInputTilt == true)
            {
                //if (EnableTilt == true)
                //{
                //    UpdateCameraTilt(mouseScrollDelta * 100);
                //}
            }
            else
            {
                float editorZoomFactor = 15;
                if (Cam.orthographic)
                {
                    editorZoomFactor = 15;
                }
                else
                {
                    if (IsTranslationZoom)
                    {
                        editorZoomFactor = 30;
                    }
                    else
                    {
                        editorZoomFactor = 100;
                    }
                }
                float zoomAmount = mouseScrollDelta * editorZoomFactor;
                float camSizeDiff = DoEditorCameraZoom(zoomAmount);
                Vector3 intersectionScreenCenter = GetIntersectionPoint(Cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)));
                Vector3 pinchFocusVector = GetIntersectionPoint(Cam.ScreenPointToRay(Input.mousePosition)) - intersectionScreenCenter;
                float multiplier = (1.0f / CamZoom * camSizeDiff);
                cam.Transform.position += pinchFocusVector * multiplier;
            }
        }
        for (int i = 0; i < 3; ++i)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
            {
                StartCoroutine(ZoomToTargetValueCoroutine(Mathf.Lerp(CamZoomMin, CamZoomMax, (float)i / 2.0f)));
            }
        }
#endif
        #endregion

        //When the camera is zoomed in further than the defined normal value, it will snap back to normal using the code below.
        if (IsPinching == false && IsDragging == false)
        {
            float camZoomDeltaToNormal = 0;
            if (CamZoom > camZoomMax)
            {
                camZoomDeltaToNormal = CamZoom - camZoomMax;
            }
            else if (CamZoom < camZoomMin)
            {
                camZoomDeltaToNormal = CamZoom - camZoomMin;
            }

            if (Mathf.Approximately(camZoomDeltaToNormal, 0) == false)
            {
                float cameraSizeCorrection = Mathf.Lerp(0, camZoomDeltaToNormal, zoomBackSpringFactor * Time.unscaledDeltaTime);
                if (Mathf.Abs(cameraSizeCorrection) > Mathf.Abs(camZoomDeltaToNormal))
                {
                    cameraSizeCorrection = camZoomDeltaToNormal;
                }
                CamZoom -= cameraSizeCorrection;
            }
        }
#if UNITY_EDITOR
        mousePosLastFrame = Input.mousePosition;
#endif
    }

    /// <summary>
    /// Editor helper code.
    /// </summary>
    private float DoEditorCameraZoom(float amount)
    {
        float newCamZoom = CamZoom - amount;
        newCamZoom = Mathf.Clamp(newCamZoom, camZoomMin, camZoomMax);
        float camSizeDiff = CamZoom - newCamZoom;
        //if (enableZoomTilt == true)
        //{
        //    UpdateTiltForAutoTilt(newCamZoom);
        //}
        CamZoom = newCamZoom;
        return (camSizeDiff);
    }

    /// <summary>
    /// 자동 스크롤에 사용되는 헬퍼 메소드입니다.
    /// </summary>
    private float EvaluateAutoScrollDampCurve(float t)
    {
        if (autoScrollDampCurve == null || autoScrollDampCurve.length == 0)
        {
            return (1);
        }
        return autoScrollDampCurve.Evaluate(t);
    }

    //private void InputControllerOnFingerDown(Vector3 pos)
    //{
    //    cameraScrollVelocity = Vector3.zero;
    //}

    //private void InputControllerOnFingerUp()
    //{
    //    isDraggingSceneObject = false;
    //}

    private Vector3 GetDragVector(Vector3 dragPosStart, Vector3 dragPosCurrent)
    {
        Vector3 intersectionDragStart = GetIntersectionPoint(Cam.ScreenPointToRay(dragPosStart));
        Vector3 intersectionDragCurrent = GetIntersectionPoint(Cam.ScreenPointToRay(dragPosCurrent));
        return (intersectionDragCurrent - intersectionDragStart);
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

    //    private void InputControllerOnPinchStart(Vector3 pinchCenter, float pinchDistance)
    //    {

    //        if (TerrainCollider != null)
    //        {
    //            UpdateRefPlaneForTerrain(pinchCenter);
    //        }

    //        pinchStartCamZoomSize = CamZoom;
    //        pinchStartIntersectionCenter = GetIntersectionPoint(Cam.ScreenPointToRay(pinchCenter));

    //        pinchCenterCurrent = pinchCenter;
    //        pinchDistanceCurrent = pinchDistance;
    //        pinchDistanceStart = pinchDistance;

    //        pinchCenterCurrentLerp = pinchCenter;
    //        pinchDistanceCurrentLerp = pinchDistance;

    //        SetTargetPosition(Transform.position);
    //        IsPinching = true;
    //        isRotationActivated = false;
    //        ResetPinchRotation(0);

    //        pinchTiltCurrent = 0;
    //        pinchTiltAccumulated = 0;
    //        pinchTiltLastFrame = 0;
    //        isTiltModeEvaluated = false;
    //        isPinchTiltMode = false;

    //        if (EnableTilt == false)
    //        {
    //            isTiltModeEvaluated = true; //Early out of this evaluation in case tilt is not enabled.
    //        }
    //    }

    //    private void InputControllerOnPinchUpdate(PinchUpdateData pinchUpdateData)
    //    {

    //        if (EnableTilt == true)
    //        {
    //            pinchTiltCurrent += pinchUpdateData.pinchTiltDelta;
    //            pinchTiltAccumulated += Mathf.Abs(pinchUpdateData.pinchTiltDelta);

    //            if (isTiltModeEvaluated == false && pinchUpdateData.pinchTotalFingerMovement > pinchModeDetectionMoveTreshold)
    //            {
    //                isPinchTiltMode = Mathf.Abs(pinchTiltCurrent) > pinchTiltModeThreshold;
    //                isTiltModeEvaluated = true;
    //                if (isPinchTiltMode == true && isPinchModeExclusive == true)
    //                {
    //                    pinchStartIntersectionCenter = GetIntersectionPoint(GetCamCenterRay());
    //                }
    //            }
    //        }

    //        if (isTiltModeEvaluated == true)
    //        {
    //#pragma warning disable 162
    //            if (isPinchModeExclusive == true)
    //            {
    //                pinchCenterCurrent = pinchUpdateData.pinchCenter;

    //                if (isPinchTiltMode == true)
    //                {

    //                    //Evaluate a potential break-out from a tilt. Under certain tweak-settings the tilt may trigger prematurely and needs to be overrided.
    //                    if (pinchTiltAccumulated < pinchAccumBreakout)
    //                    {
    //                        bool breakoutZoom = Mathf.Abs(pinchDistanceStart - pinchUpdateData.pinchDistance) > pinchDistanceForTiltBreakout;
    //                        bool breakoutRot = enableRotation == true && Mathf.Abs(pinchAngleCurrent) > rotationLockThreshold;
    //                        if (breakoutZoom == true || breakoutRot == true)
    //                        {
    //                            InputControllerOnPinchStart(pinchUpdateData.pinchCenter, pinchUpdateData.pinchDistance);
    //                            isTiltModeEvaluated = true;
    //                            isPinchTiltMode = false;
    //                        }
    //                    }
    //                }
    //            }
    //#pragma warning restore 162
    //            pinchDistanceCurrent = pinchUpdateData.pinchDistance;

    //            if (enableRotation == true)
    //            {
    //                if (Mathf.Abs(pinchUpdateData.pinchAngleDeltaNormalized) > rotationDetectionDeltaThreshold)
    //                {
    //                    pinchAngleCurrent += pinchUpdateData.pinchAngleDelta;
    //                }

    //                if (pinchDistanceCurrent > rotationMinPinchDistance)
    //                {
    //                    if (isRotationActivated == false)
    //                    {
    //                        ResetPinchRotation(0);
    //                        isRotationActivated = true;
    //                    }
    //                }
    //                else
    //                {
    //                    isRotationActivated = false;
    //                }
    //            }
    //        }
    //    }

    //    private void ResetPinchRotation(float currentPinchRotation)
    //    {
    //        pinchAngleCurrent = currentPinchRotation;
    //        pinchAngleCurrentLerp = currentPinchRotation;
    //        pinchAngleLastFrame = currentPinchRotation;
    //        isRotationLock = true;
    //    }

    //    private void InputControllerOnPinchStop()
    //    {
    //        IsPinching = false;
    //        DragCameraMoveVector.Clear();
    //        isPinchTiltMode = false;
    //        isTiltModeEvaluated = false;
    //    }

    //    private void InputControllerOnInputClick(Vector3 clickPosition, bool isDoubleClick, bool isLongTap)
    //    {

    //        if (isLongTap == true)
    //        {
    //            return;
    //        }

    //        Ray camRay = Cam.ScreenPointToRay(clickPosition);
    //        if (OnPickItem != null || OnPickItemDoubleClick != null)
    //        {
    //            RaycastHit hitInfo;
    //            if (Physics.Raycast(camRay, out hitInfo) == true)
    //            {
    //                if (OnPickItem != null)
    //                {
    //                    OnPickItem.Invoke(hitInfo);
    //                }
    //                if (isDoubleClick == true)
    //                {
    //                    if (OnPickItemDoubleClick != null)
    //                    {
    //                        OnPickItemDoubleClick.Invoke(hitInfo);
    //                    }
    //                }
    //            }
    //        }
    //        if (OnPickItem2D != null || OnPickItem2DDoubleClick != null)
    //        {
    //            RaycastHit2D hitInfo2D = Physics2D.Raycast(camRay.origin, camRay.direction);
    //            if (hitInfo2D == true)
    //            {
    //                if (OnPickItem2D != null)
    //                {
    //                    OnPickItem2D.Invoke(hitInfo2D);
    //                }
    //                if (isDoubleClick == true)
    //                {
    //                    if (OnPickItem2DDoubleClick != null)
    //                    {
    //                        OnPickItem2DDoubleClick.Invoke(hitInfo2D);
    //                    }
    //                }
    //            }
    //        }
    //    }

    private IEnumerator ZoomToTargetValueCoroutine(float target)
    {

        if (Mathf.Approximately(target, CamZoom) == false)
        {
            float startValue = CamZoom;
            const float duration = 0.3f;
            float timeStart = Time.time;
            while (Time.time < timeStart + duration)
            {
                float progress = (Time.time - timeStart) / duration;
                CamZoom = Mathf.Lerp(startValue, target, Mathf.Sin(-Mathf.PI * 0.5f + progress * Mathf.PI) * 0.5f + 0.5f);
                yield return null;
            }
            CamZoom = target;
        }
    }

    private Ray GetCamCenterRay()
    {
        return (Cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)));
    }

    //    private void UpdateRefPlaneForTerrain(Vector3 touchPosition)
    //    {
    //        RaycastHit hitInfo;
    //        var dragRay = Cam.ScreenPointToRay(touchPosition);
    //        if (TerrainCollider.Raycast(dragRay, out hitInfo, float.MaxValue) == true)
    //        {
    //            refPlaneXZ = new Plane(new Vector3(0, 1, 0), -hitInfo.point.y);
    //        }
    //    }

    //    public void OnDrawGizmosSelected()
    //    {
    //        Gizmos.color = Color.yellow;
    //        Vector2 boundaryCenter2d = 0.5f * (boundaryMin + boundaryMax);
    //        Vector2 boundarySize2d = boundaryMax - boundaryMin;
    //        Vector3 boundaryCenter = UnprojectVector2(boundaryCenter2d, groundLevelOffset);
    //        Vector3 boundarySize = UnprojectVector2(boundarySize2d);
    //        Gizmos.DrawWireCube(boundaryCenter, boundarySize);
    //    }

}

