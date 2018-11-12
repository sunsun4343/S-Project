using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTS2DCamera : MonoBehaviour {

    private Camera cam;

    [SerializeField]
    private float camZoom = 7;
    [SerializeField]
    private float camZoomMin = 4;
    [SerializeField]
    private float camZoomMax = 13;
    [SerializeField]
    private float camOverzoomMargin = 1;

    public float CamZoom
    {
        get
        {
            return cam.orthographicSize;
        }
        set
        {
            cam.orthographicSize = value;
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
    public float CamOverzoomMargin
    {
        get { return (camOverzoomMargin); }
        set { camOverzoomMargin = value; }
    }

    #region Mono Function

    private void Reset()
    {
        InitCameraParamater();

    }

    private void Awake()
    {
        InitCameraParamater();

    }

    private void Update()
    {
        
    }

    #endregion

    private void InitCameraParamater()
    {
        cam = this.GetComponent<Camera>();
        cam.orthographic = true;

        CamZoomMin = 4;
        CamZoomMax = 13;
        CamZoom = 7;
        CamOverzoomMargin = 1;
        ResetCamPosition(20);
    }

    private void ResetCamPosition(float distance)
    {
        this.transform.position = new Vector3(0, 0, -distance);
        ComputeCamBoundaries();
    }

    private void ComputeCamBoundaries()
    {
        //float camRotation = 0;

        //Vector2 camProjectedMin = Vector2.zero;
        //Vector2 camProjectedMax = Vector2.zero;

        //Vector2 camProjectedCenter = GetIntersection2d(new Ray(Transform.position, -RefPlane.normal)); //Get camera position projected vertically onto the ref plane. This allows to compute the offset that arises from camera tilt.

        ////Fetch camera boundary as world-space coordinates projected to the ground.
        //Vector2 camRight = GetIntersection2d(Cam.ScreenPointToRay(new Vector3(Screen.width, Screen.height * 0.5f, 0)));
        //Vector2 camLeft = GetIntersection2d(Cam.ScreenPointToRay(new Vector3(0, Screen.height * 0.5f, 0)));
        //Vector2 camUp = GetIntersection2d(Cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height, 0)));
        //Vector2 camDown = GetIntersection2d(Cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, 0, 0)));
        //camProjectedMin = GetVector2Min(camRight, camLeft, camUp, camDown);
        //camProjectedMax = GetVector2Max(camRight, camLeft, camUp, camDown);

        ////Create rotated bounding box from boundaryMin/Max
        //Vector2 computeBoundaryMin, computeBoundaryMax;
        //RotateBoundingBox(boundaryMin, boundaryMax, -camRotation, out computeBoundaryMin, out computeBoundaryMax);

        //Vector2 projectionCorrectionMin = new Vector2(camProjectedCenter.x - camProjectedMin.x, camProjectedCenter.y - camProjectedMin.y);
        //Vector2 projectionCorrectionMax = new Vector2(camProjectedCenter.x - camProjectedMax.x, camProjectedCenter.y - camProjectedMax.y);

        //CamPosMin = boundaryMin + projectionCorrectionMin;
        //CamPosMax = boundaryMax + projectionCorrectionMax;

        //Vector2 margin = CamOverdragMargin2d;
        //if (CamPosMax.x - CamPosMin.x < margin.x * 2)
        //{
        //    float midPoint = (CamPosMax.x + CamPosMin.x) * 0.5f;
        //    CamPosMax = new Vector2(midPoint + margin.x, CamPosMax.y);
        //    CamPosMin = new Vector2(midPoint - margin.x, CamPosMin.y);
        //}

        //if (CamPosMax.y - CamPosMin.y < margin.y * 2)
        //{
        //    float midPoint = (CamPosMax.y + CamPosMin.y) * 0.5f;
        //    CamPosMax = new Vector2(CamPosMax.x, midPoint + margin.y);
        //    CamPosMin = new Vector2(CamPosMin.x, midPoint - margin.y);
        //}
    }



}
