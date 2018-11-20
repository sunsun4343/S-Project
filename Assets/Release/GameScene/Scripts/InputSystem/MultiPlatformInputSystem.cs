using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MultiPlatformInputSystem : MonoBehaviour {

    /// <summary>
    /// 손가락이 움직이지 않고 적어도 이 지속 기간 동안 항목에 유지되면 제스처는 긴 탭으로 인식됩니다.
    /// </summary>
    [SerializeField]
    [Tooltip("손가락이 움직이지 않고 적어도 이 지속 기간 동안 항목에 유지되면 제스처는 긴 탭으로 인식됩니다.")]
    private float clickDurationThreshold = 0.7f;

    /// <summary>
    /// 더블 클릭 제스처는 두 개의 연속 탭 사이의 시간이 이 시간보다 짧은 경우 인식됩니다.
    /// </summary>
    [SerializeField]
    [Tooltip("더블 클릭 제스처는 두 개의 연속 탭 사이의 시간이 이 시간보다 짧은 경우 인식됩니다.")]
    private float doubleclickDurationThreshold = 0.5f;

    /// <summary>
    /// 드래그는 사용자가 이 값보다 긴 거리로 손가락을 움직이자 마자 시작됩니다. 값은 정규화 된 값으로 정의됩니다.
    /// </summary>
    [SerializeField]
    [Tooltip("드래그는 사용자가 이 값보다 긴 거리로 손가락을 움직이자 마자 시작됩니다. 값은 정규화 된 값으로 정의됩니다.")]
    private float dragStartDistanceThresholdRelative = 0.05f;
    /// <summary>
    /// 활성화되면 드래그 시작 이벤트는 긴 탭 시간이 완료되면 즉시 호출됩니다.
    /// </summary>
    [SerializeField]
    [Tooltip("활성화되면 드래그 시작 이벤트는 긴 탭 시간이 완료되면 즉시 호출됩니다.")]
    private bool longTapStartsDrag = true;


    /// <summary>
    /// 드래그를 시작하기 위해 터치 시작 후 경과해야 되는 최소 시간
    /// </summary>
    private const float dragDurationThreshold = 0.01f;


    //콜백 이벤트

    public delegate void PinchStartDelegate(Vector3 pinchCenter, float pinchDistance);
    public event PinchStartDelegate OnPinchStart;

    public delegate void PinchUpdateDelegate(Vector3 pinchCenter, float pinchDistance, float pinchStartDistance);
    public event PinchUpdateDelegate OnPinchUpdate;

    public delegate void PinchUpdateExtendedDelegate(PinchUpdateData pinchUpdateData);
    public event PinchUpdateExtendedDelegate OnPinchUpdateExtended;

    public event System.Action OnPinchStop;

    public delegate void InputLongTapProgress(float progress);
    public event InputLongTapProgress OnLongTapProgress;

    public delegate void InputDragStartDelegate(Vector3 pos, bool isLongTap);
    public event InputDragStartDelegate OnDragStart;

    public delegate void DragUpdateDelegate(Vector3 dragPosStart, Vector3 dragPosCurrent, Vector3 correctionOffset);
    public event DragUpdateDelegate OnDragUpdate;

    public delegate void DragStopDelegate(Vector3 dragStopPos, Vector3 dragFinalMomentum);
    public event DragStopDelegate OnDragStop;

    public delegate void Input1PositionDelegate(Vector3 pos);
    public event Input1PositionDelegate OnFingerDown;

    public event System.Action OnFingerUp;

    public delegate void InputClickDelegate(Vector3 clickPosition, bool isDoubleClick, bool isLongTap);
    public event InputClickDelegate OnInputClick;

    //공통
    private bool isInputOnLockedArea = false;
    
    /// <summary>
    /// 화면 터치 잠금 관리
    /// </summary>
    public bool IsInputOnLockedArea
    {
        get { return isInputOnLockedArea; }
        set { isInputOnLockedArea = value; }
    }

    /// <summary>
    /// 터치 시작 리얼타임
    /// </summary>
    private float lastFingerDownTimeReal;
    /// <summary>
    /// 이전 프레임 Down 상태
    /// </summary>
    private bool wasFingerDownLastFrame;
    private Vector3 lastFinger0DownPos;
    /// <summary>
    /// 내부 터치 상태
    /// </summary>
    private bool isFingerDown;

    //피칭관련
    private bool isPinching;
    private float pinchStartDistance;
    private List<Vector3> pinchStartPositions;
    private List<Vector3> touchPositionLastFrame;
    private bool wasPinchingLastFrame;
    private Vector3 pinchRotationVectorStart = Vector3.zero;
    private Vector3 pinchVectorLastFrame = Vector3.zero;
    private float totalFingerMovement;

    //클릭관련

    /// <summary>
    /// 
    /// </summary>
    private float lastClickTimeReal;
    /// <summary>
    /// 클릭 이벤트가 방지됨.
    /// </summary>
    private bool isClickPrevented;


    //드래그 관련

    /// <summary>
    /// 이전 프레임 드래그 상태
    /// </summary>
    private bool wasDraggingLastFrame;
    private bool isDragging;
    private Vector3 dragStartPos;
    private Vector3 dragStartOffset;
    /// <summary>
    /// 드래그가 시작된 후 경과한 시간
    /// </summary>
    private float timeSinceDragStart = 0;
    private const int momentumSamplesCount = 5;
    private List<Vector3> DragFinalMomentumVector { get; set; }


    //----------------------------------------------------------------------------------------------

    public void Awake()
    {
        lastFingerDownTimeReal = 0;
        lastClickTimeReal = 0;
        lastFinger0DownPos = Vector3.zero;
        dragStartPos = Vector3.zero;
        isDragging = false;
        wasFingerDownLastFrame = false;
        DragFinalMomentumVector = new List<Vector3>();
        pinchStartPositions = new List<Vector3>() { Vector3.zero, Vector3.zero };
        touchPositionLastFrame = new List<Vector3>() { Vector3.zero, Vector3.zero };
        pinchStartDistance = 1;
        isPinching = false;
        isClickPrevented = false;
    }

    public void OnEventTriggerPointerDown(BaseEventData baseEventData)
    {
        isInputOnLockedArea = true;
    }

    public void Update()
    {
        //터치가 없을 때는 화면을 잠금할 필요가 없음.
        if (TouchWrapper.IsFingerDown == false)
        {
            isInputOnLockedArea = false;
        }

        bool pinchToDragCurrentFrame = false;

        //화면 잠금이 아닌경우 동작
        if (isInputOnLockedArea == false)
        {
            TouchActionUpdate(ref pinchToDragCurrentFrame);
        }

        if (isDragging && TouchWrapper.IsFingerDown && pinchToDragCurrentFrame == false)
        {
            DragFinalMomentumVector.Add(TouchWrapper.Touch0.Position - lastFinger0DownPos);
            if (DragFinalMomentumVector.Count > momentumSamplesCount)
            {
                DragFinalMomentumVector.RemoveAt(0);
            }
        }

        //화면 잠금이 아닌경우 동작
        if (isInputOnLockedArea == false)
        {
            //전 프레임 터치 상태 저장
            wasFingerDownLastFrame = TouchWrapper.IsFingerDown;
        }
        //전 프레임이 터치 상태라면
        if (wasFingerDownLastFrame == true)
        {
            //터치 위치 저장
            lastFinger0DownPos = TouchWrapper.Touch0.Position;
        }

        wasDraggingLastFrame = isDragging;
        wasPinchingLastFrame = isPinching;

        if (TouchWrapper.TouchCount == 0)
        {
            isClickPrevented = false;
            if (isFingerDown == true)
            {
                FingerUp();
            }
        }

    }

    void TouchActionUpdate(ref bool pinchToDragCurrentFrame)
    {
        #region pinch
        if (isPinching == false)
        {
            if (TouchWrapper.TouchCount == 2)
            {
                StartPinch();
                isPinching = true;
            }
        }
        else
        {
            if (TouchWrapper.TouchCount < 2)
            {
                StopPinch();
                isPinching = false;
            }
            else if (TouchWrapper.TouchCount == 2)
            {
                UpdatePinch();
            }
        }
        #endregion

        #region drag
        if (isPinching == false)
        {
            if (wasPinchingLastFrame == false)
            {
                //전 프레임이 터치상태이고, 현재 프레임에 터치 중 일때
                if (wasFingerDownLastFrame == true && TouchWrapper.IsFingerDown)
                {
                    if (isDragging == false)
                    {
                        float dragDistance = GetRelativeDragDistance(TouchWrapper.Touch0.Position, dragStartPos);
                        float dragTime = Time.realtimeSinceStartup - lastFingerDownTimeReal;

                        bool isLongTap = dragTime > clickDurationThreshold;
                        if (OnLongTapProgress != null)
                        {
                            float longTapProgress = 0;
                            if (Mathf.Approximately(clickDurationThreshold, 0) == false)
                            {
                                longTapProgress = Mathf.Clamp01(dragTime / clickDurationThreshold);
                            }
                            //탭하는 동안 지속적으로 호출 (진행도)
                            OnLongTapProgress(longTapProgress);
                        }

                        //일정 거리, 일정 시간 이상 움직이거나
                        if ((dragDistance >= dragStartDistanceThresholdRelative && dragTime >= dragDurationThreshold)
                          //롱 탭이거나
                          || (longTapStartsDrag == true && isLongTap == true))
                        {
                            isDragging = true;
                            dragStartOffset = lastFinger0DownPos - dragStartPos;
                            dragStartPos = lastFinger0DownPos;
                            DragStart(dragStartPos, isLongTap, true);
                        }
                    }
                }
            }
            else
            {
                if (TouchWrapper.IsFingerDown == true)
                {
                    isDragging = true;
                    dragStartPos = TouchWrapper.Touch0.Position;
                    DragStart(dragStartPos, false, false);
                    pinchToDragCurrentFrame = true;
                }
            }

            if (isDragging == true && TouchWrapper.IsFingerDown == true)
            {
                DragUpdate(TouchWrapper.Touch0.Position);
            }

            if (isDragging == true && TouchWrapper.IsFingerDown == false)
            {
                isDragging = false;
                DragStop(lastFinger0DownPos);
            }
        }
        #endregion

        #region click
        if (isPinching == false && isDragging == false && wasPinchingLastFrame == false && wasDraggingLastFrame == false && isClickPrevented == false)
        {
            //최초 터치 처리
            if (wasFingerDownLastFrame == false && TouchWrapper.IsFingerDown)
            {
                lastFingerDownTimeReal = Time.realtimeSinceStartup;
                dragStartPos = TouchWrapper.Touch0.Position;
                FingerDown(TouchWrapper.AverageTouchPos);
            }

            //마지막 터치 처리
            if (wasFingerDownLastFrame == true && TouchWrapper.IsFingerDown == false)
            {
                float fingerDownUpDuration = Time.realtimeSinceStartup - lastFingerDownTimeReal;

                if (wasDraggingLastFrame == false)
                {
                    float clickDuration = Time.realtimeSinceStartup - lastClickTimeReal;

                    bool isDoubleClick = clickDuration < doubleclickDurationThreshold;
                    bool isLongTap = fingerDownUpDuration > clickDurationThreshold;

                    if (OnInputClick != null)
                    {
                        OnInputClick.Invoke(lastFinger0DownPos, isDoubleClick, isLongTap);
                    }

                    lastClickTimeReal = Time.realtimeSinceStartup;
                }
            }
        }
        #endregion

    }

    //----------------------------------------------------------------------------------------------

    private void StartPinch()
    {
        pinchStartPositions[0] = touchPositionLastFrame[0] = TouchWrapper.Touches[0].Position;
        pinchStartPositions[1] = touchPositionLastFrame[1] = TouchWrapper.Touches[1].Position;

        pinchStartDistance = GetPinchDistance(pinchStartPositions[0], pinchStartPositions[1]);
        if (OnPinchStart != null)
        {
            OnPinchStart.Invoke((pinchStartPositions[0] + pinchStartPositions[1]) * 0.5f, pinchStartDistance);
        }
        isClickPrevented = true;
        pinchRotationVectorStart = TouchWrapper.Touches[1].Position - TouchWrapper.Touches[0].Position;
        pinchVectorLastFrame = pinchRotationVectorStart;
        totalFingerMovement = 0;

    }

    private void UpdatePinch()
    {
        float pinchDistance = GetPinchDistance(TouchWrapper.Touches[0].Position, TouchWrapper.Touches[1].Position);
        Vector3 pinchVector = TouchWrapper.Touches[1].Position - TouchWrapper.Touches[0].Position;
        float pinchAngleSign = Vector3.Cross(pinchVectorLastFrame, pinchVector).z < 0 ? -1 : 1;
        float pinchAngleDelta = 0;
        if (Mathf.Approximately(Vector3.Distance(pinchVectorLastFrame, pinchVector), 0) == false)
        {
            pinchAngleDelta = Vector3.Angle(pinchVectorLastFrame, pinchVector) * pinchAngleSign;
        }
        float pinchVectorDeltaMag = Mathf.Abs(pinchVectorLastFrame.magnitude - pinchVector.magnitude);
        float pinchAngleDeltaNormalized = 0;
        if (Mathf.Approximately(pinchVectorDeltaMag, 0) == false)
        {
            pinchAngleDeltaNormalized = pinchAngleDelta / pinchVectorDeltaMag;
        }
        Vector3 pinchCenter = (TouchWrapper.Touches[0].Position + TouchWrapper.Touches[1].Position) * 0.5f;

        #region tilting gesture
        float pinchTiltDelta = 0;
        //Vector3 touch0DeltaRelative = GetTouchPositionRelative(TouchWrapper.Touches[0].Position - touchPositionLastFrame[0]);
        //Vector3 touch1DeltaRelative = GetTouchPositionRelative(TouchWrapper.Touches[1].Position - touchPositionLastFrame[1]);
        //float touch0DotUp = Vector2.Dot(touch0DeltaRelative.normalized, Vector2.up);
        //float touch1DotUp = Vector2.Dot(touch1DeltaRelative.normalized, Vector2.up);
        //float pinchVectorDotHorizontal = Vector3.Dot(pinchVector.normalized, Vector3.right);
        //if (Mathf.Sign(touch0DotUp) == Mathf.Sign(touch1DotUp))
        //{
        //    if (Mathf.Abs(touch0DotUp) > tiltMoveDotTreshold && Mathf.Abs(touch1DotUp) > tiltMoveDotTreshold)
        //    {
        //        if (Mathf.Abs(pinchVectorDotHorizontal) >= tiltHorizontalDotThreshold)
        //        {
        //            pinchTiltDelta = 0.5f * (touch0DeltaRelative.y + touch1DeltaRelative.y);
        //        }
        //    }
        //}
        //totalFingerMovement += touch0DeltaRelative.magnitude + touch1DeltaRelative.magnitude;
        #endregion

        if (OnPinchUpdate != null)
        {
            OnPinchUpdate.Invoke(pinchCenter, pinchDistance, pinchStartDistance);
        }
        if (OnPinchUpdateExtended != null)
        {
            OnPinchUpdateExtended(new PinchUpdateData() { pinchCenter = pinchCenter, pinchDistance = pinchDistance, pinchStartDistance = pinchStartDistance, pinchAngleDelta = pinchAngleDelta, pinchAngleDeltaNormalized = pinchAngleDeltaNormalized, pinchTiltDelta = pinchTiltDelta, pinchTotalFingerMovement = totalFingerMovement });
        }

        pinchVectorLastFrame = pinchVector;
        touchPositionLastFrame[0] = TouchWrapper.Touches[0].Position;
        touchPositionLastFrame[1] = TouchWrapper.Touches[1].Position;

    }


    private float GetPinchDistance(Vector3 pos0, Vector3 pos1)
    {
        float distanceX = Mathf.Abs(pos0.x - pos1.x) / Screen.width;
        float distanceY = Mathf.Abs(pos0.y - pos1.y) / Screen.height;
        return (Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY));
    }

    private void StopPinch()
    {
        dragStartOffset = Vector3.zero;
        if (OnPinchStop != null)
        {
            OnPinchStop.Invoke();
        }
    }

    //----------------------------------------------------------------------------------------------

    private void DragStart(Vector3 pos, bool isLongTap, bool isInitialDrag)
    {
        if (OnDragStart != null)
        {
            OnDragStart(pos, isLongTap);
        }
        isClickPrevented = true;
        timeSinceDragStart = 0;
        DragFinalMomentumVector.Clear();
    }

    private void DragUpdate(Vector3 pos)
    {
        if (OnDragUpdate != null)
        {
            timeSinceDragStart += Time.unscaledDeltaTime;
            Vector3 offset = Vector3.Lerp(Vector3.zero, dragStartOffset, Mathf.Clamp01(timeSinceDragStart * 10.0f));
            OnDragUpdate(dragStartPos, pos, offset);
        }
    }

    private void DragStop(Vector3 pos)
    {
        if (OnDragStop != null)
        {
            Vector3 momentum = Vector3.zero;
            if (DragFinalMomentumVector.Count > 0)
            {
                for (int i = 0; i < DragFinalMomentumVector.Count; ++i)
                {
                    momentum += DragFinalMomentumVector[i];
                }
                momentum /= DragFinalMomentumVector.Count;
            }
            OnDragStop(pos, momentum);
        }

        DragFinalMomentumVector.Clear();
    }

    private void FingerDown(Vector3 pos)
    {
        isFingerDown = true;
        if (OnFingerDown != null)
        {
            OnFingerDown(pos);
        }
    }

    private void FingerUp()
    {
        isFingerDown = false;
        if (OnFingerUp != null)
        {
            OnFingerUp();
        }
    }

    //----------------------------------------------------------------------------------------------




    /// <summary>
    /// 정규화된 거리
    /// </summary>
    private float GetRelativeDragDistance(Vector3 pos0, Vector3 pos1)
    {
        Vector2 dragVector = pos0 - pos1;
        float dragDistance = new Vector2(dragVector.x / Screen.width, dragVector.y / Screen.height).magnitude;
        return dragDistance;
    }
}
