using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ATJS;

public class ATJSDemoManager : ATJSManager {

    protected override void Awake()
    {
        base.Awake();

        

        Job job = new Job();
        job.jobAction = RandomPosition;

        AddTopPriorityJob(job);

    }

    public GameObject gameObjectThread;
    public GameObject gameObjectNonTh;


    protected override void LateUpdate()
    {
        base.LateUpdate();

        gameObjectThread.transform.rotation = Quaternion.Euler(DemoGameDB.Instance.position.value);
        //gameObjectNonTh.transform.position = DemoGameDB.Instance.position.value;


    }


    void RandomPosition()
    {
        Vector3 pos = DemoGameDB.Instance.position.value;

        Vector3 endpos = pos;

        for (int i = 0; i < 99999; i++)
        {
            Vector3 dir = new Vector3(0, 0, 1);
            endpos += dir;
        }

        pos = Vector3.Lerp(pos, pos + Vector3.forward, 1);

        DemoGameDB.Instance.position.value = pos;

    }


}
