using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyBillboard : MonoBehaviour
{
    public enum BillboardType { FacingCamera, MatchingCamera };

    public Camera thatCamera;
    public BillboardType billboardType = BillboardType.FacingCamera;
    public float rotationSpeed = 2f;

    // Start is called before the first frame update
    void Start()
    {
        if(thatCamera is null)
        {
            thatCamera = GetComponent<Camera>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch(this.billboardType)
        {
            case BillboardType.FacingCamera:
                // Make the billboard point toward the camera
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(this.transform.position - thatCamera.transform.position), rotationSpeed);
                break;
            case BillboardType.MatchingCamera:
                // Make the billboard point the same direction as the camera
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, thatCamera.transform.rotation, rotationSpeed);
                break;
        }
    }
}
