using System;
using UnityEngine;

public class JoystickControlInput
{
    //name of the virtual axe X defined on Input Manager
    private String _axisXName;
    //name of the virtual axe Y defined on Input Manager
    private String _axisYName;

    public JoystickControlInput(String axisXName, String axisYName)
    {
        _axisXName = axisXName;
        _axisYName = axisYName;
    }

    private float GetAxisX()
    {
        if(CFInput.GetAxis(_axisXName) != 0)
        {
            return CFInput.GetAxis(_axisXName);
        }

        return Input.GetAxis(_axisXName);
    }

    private float GetAxisY()
    {
        if(CFInput.GetAxis(_axisYName) != 0)
        {
            return CFInput.GetAxis(_axisYName);
        }

        return Input.GetAxis(_axisYName);
    }

    public bool Pressed()
    {
        return (GetAxisX() != 0) || (GetAxisY() != 0);
    }

    public Vector2 GetVec()
    {
        return new Vector2(GetAxisX(), GetAxisY());
    }

    public float GetAngle()
    {
        float angle = Vector2.Angle(Vector2.up, GetVec());
        Vector3 cross = Vector3.Cross(Vector2.up, GetVec());
        if(cross.z > 0)
        {
            angle = 360 - angle;
        }
        return angle;
    }
}

