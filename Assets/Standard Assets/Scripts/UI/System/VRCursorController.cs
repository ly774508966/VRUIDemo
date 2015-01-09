using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class CursorConstraints
{
    public string Name;

    public float XYSpeedScale = 5f;
    public float XZSpeedScale = 5f;

    public float XMin = -10.0f;
    public float XMax = 110.0f;
    public float YMin = -50.0f;
    public float YMax = 100.0f;
    public float ZMin = 0.0f;
    public float ZMax = 150.0f;

    public bool AllowXZPlane = false;

    public Vector3 SnapPositionInsideBounds(
        Vector3 position)
    {
        Vector3 newPosition;

        newPosition.x = Math.Max(Math.Min(position.x, XMax), XMin);
        newPosition.y = Math.Max(Math.Min(position.y, YMax), YMin);
        newPosition.z = Math.Max(Math.Min(position.z, ZMax), ZMin);

        return newPosition;
    }
}

public class VRCursorController : MonoBehaviour 
{
    const string DEFAULT_CURSOR_CONSTRAINTS= "default";
    const int LEFT_MOUSE_BUTTON = 0;

    static Quaternion XZ_ORIENTATION = Quaternion.Euler(180.0f, 0.0f, 0.0f);
    static Quaternion XY_ORIENTATION = Quaternion.Euler(90.0f, 0.0f, 0.0f);

    public enum eCursorPlane
    {
        XYPlane,
        XZPlane
    }

    public float WidgetOffsetHeight = 5.0f;

    public CursorConstraints[] CursorConstraintList = new CursorConstraints[1];

    private static VRCursorController m_instance;

    private Vector3 m_lastCursorPosition;
    private VRCursorController.eCursorPlane m_lastCursorPlane;

    private GameObject m_cursorIcon;
    private MeshRenderer m_cursorIconRenderer;

    private int m_activeCursorConstraintsIndex = -1;
    private int m_defaultCursorConstraintsIndex = -1;

    public static VRCursorController GetInstance()
    {
        return m_instance;
    }

    public eCursorPlane CursorPlane 
    { 
        get; 
        private set; 
    }

    public bool GetCursorMoved()
    {
        Vector3 cursorPosition= GetCursorPosition();
        return
            CursorPlane != m_lastCursorPlane ||
            (cursorPosition - m_lastCursorPosition).sqrMagnitude > 0.001f;
    }

    public bool GetCursorPressed()
    {
        return Input.GetMouseButtonDown(LEFT_MOUSE_BUTTON);
    }

    public bool GetCursorReleased()
    {
        return Input.GetMouseButtonUp(LEFT_MOUSE_BUTTON);
    }

    public float GetCursorScroll()
    {
        return Input.mouseScrollDelta.y;
    }

    public Vector3 GetCursorPosition()
    {
        return m_instance.transform.position;
    }

    public Vector3 GetCursorRaycastPosition(
        float raycastOffset)
    {
        return m_instance.transform.position - GetCursorRaycastDirection() * raycastOffset;
    }

    public Vector3 GetCursorRaycastDirection()
    {
        Vector3 raycastDirection = Vector3.zero;

        switch(CursorPlane)
        {
        case eCursorPlane.XYPlane:
            raycastDirection = Vector3.forward; // (0, 0, 1)
            break;
        case eCursorPlane.XZPlane:
            raycastDirection = Vector3.down; // (0, -1, 0)
            break;
        }

        return raycastDirection;
    }

    private int FindCursorConstraints(
        string constraintsName)
    {
        int constraintsIndex = -1;

        for (int index = 0; index < CursorConstraintList.Length; index++)
        {
            if (CursorConstraintList[index].Name == constraintsName)
            {
                constraintsIndex = index;
                break;
            }
        }

        return constraintsIndex;
    }

    public void SetCursorConstraints(
        string constraintsName)
    {
        int newCursorConstraintIndex = FindCursorConstraints(constraintsName);

        if (newCursorConstraintIndex == -1)
        {
            Debug.LogError(
                string.Format("VRCursor: Unable to find constraints {0}, using {1} instead", 
                constraintsName, DEFAULT_CURSOR_CONSTRAINTS));
            newCursorConstraintIndex = m_defaultCursorConstraintsIndex;
        }

        if (newCursorConstraintIndex != m_activeCursorConstraintsIndex)
        {
            CursorConstraints oldConstraints = CursorConstraintList[m_activeCursorConstraintsIndex];
            CursorConstraints newConstraints = CursorConstraintList[newCursorConstraintIndex];

            // If the cursor is still in the XZ plane and becomes disallowed to be there,
            // snap the cursor to the XY plane and re-orient
            if (CursorPlane == eCursorPlane.XZPlane &&
                oldConstraints.AllowXZPlane && !newConstraints.AllowXZPlane)
            {
                Vector3 snappedCursorPosition = this.transform.position;

                snappedCursorPosition.z = newConstraints.ZMax;

                this.transform.position = snappedCursorPosition;
                this.transform.rotation = XY_ORIENTATION;
                this.CursorPlane = eCursorPlane.XYPlane;
            }

            // Make sure the cursor position is inside the new bounds
            {
                Vector3 desiredCursorPosition = this.transform.position;

                desiredCursorPosition = newConstraints.SnapPositionInsideBounds(desiredCursorPosition);

                this.transform.position = desiredCursorPosition;
            }

            m_activeCursorConstraintsIndex = newCursorConstraintIndex;
        }
    }

    public void ClearCursorConstraints()
    {
        SetCursorConstraints(DEFAULT_CURSOR_CONSTRAINTS);
    }

    public CursorConstraints GetCursorConstraints()
    {
        return m_activeCursorConstraintsIndex != -1 ? 
            CursorConstraintList[m_activeCursorConstraintsIndex] :
            CursorConstraintList[m_defaultCursorConstraintsIndex];
    }

    public void SetCursorTexture(
        Texture2D texture)
    {
        if (texture != null)
        {
            m_cursorIcon.SetActive(true);
            m_cursorIconRenderer.material.mainTexture = texture;
        }
        else
        {
            m_cursorIcon.SetActive(false);
        }
    }

    void Awake()
    {
        m_instance = this;
        m_defaultCursorConstraintsIndex = FindCursorConstraints(DEFAULT_CURSOR_CONSTRAINTS);

        if (m_defaultCursorConstraintsIndex == -1)
        {
            CursorConstraints[] newConstraintsList = new CursorConstraints[CursorConstraintList.Length+1];

            newConstraintsList[0] = new CursorConstraints();
            newConstraintsList[0].Name = DEFAULT_CURSOR_CONSTRAINTS;

            CursorConstraintList.CopyTo(newConstraintsList, 1);
            CursorConstraintList = newConstraintsList;

            m_defaultCursorConstraintsIndex = 0;
        }

        m_activeCursorConstraintsIndex = m_defaultCursorConstraintsIndex;
    }

	void Start() 
    {
        CursorPlane = eCursorPlane.XYPlane;
        
        m_lastCursorPosition= this.transform.position;
        m_lastCursorPlane = CursorPlane;

        m_cursorIcon = this.transform.FindChild("Icon").gameObject;
        m_cursorIconRenderer = m_cursorIcon.GetComponent<MeshRenderer>();
        SetCursorTexture(null);
	}

    void OnDestroy()
    {
        m_instance = null;
    }
	
	void Update () 
    {
        CursorConstraints constraints= GetCursorConstraints();

        Vector3 currentCursorPosition = this.transform.position;
        Vector3 desiredCursorPosition = currentCursorPosition;
        Quaternion desiredCursorOrientation = this.transform.rotation;

        Vector3 mouseDelta;

        mouseDelta.x = Input.GetAxis("Mouse X");
        mouseDelta.y = Input.GetAxis("Mouse Y");
        mouseDelta.z = 0.0f;

        // Keep the mouse hidden and locked
        Screen.showCursor = false;
        Screen.lockCursor = true;

        // Remember where the cursor was before it moves
        m_lastCursorPlane = CursorPlane;
        m_lastCursorPosition = currentCursorPosition;

        // Move the cursor
        switch (CursorPlane)
        {
            case eCursorPlane.XYPlane:
                {
                    desiredCursorPosition.x = currentCursorPosition.x + mouseDelta.x * constraints.XYSpeedScale;
                    desiredCursorPosition.y = currentCursorPosition.y + mouseDelta.y * constraints.XYSpeedScale;

                    if (desiredCursorPosition.y < constraints.YMin && constraints.AllowXZPlane)
                    {
                        // Fold the remaining downward motion onto the XZ plane
                        desiredCursorPosition.z = constraints.ZMax - (constraints.YMin - desiredCursorPosition.y);
                        desiredCursorPosition.y = constraints.YMin;

                        // Make the cursor perpendicular to the XZ plane
                        desiredCursorOrientation = XZ_ORIENTATION;

                        // Keep track of the cursor plane change
                        CursorPlane = eCursorPlane.XZPlane;
                    }
                } break;

            case eCursorPlane.XZPlane:
                {
                    desiredCursorPosition.x = currentCursorPosition.x + mouseDelta.x * constraints.XZSpeedScale;
                    desiredCursorPosition.z = currentCursorPosition.z + mouseDelta.y * constraints.XZSpeedScale;

                    if (desiredCursorPosition.z > constraints.ZMax && constraints.AllowXZPlane)
                    {
                        // Fold the remaining Z+ motion onto the XY plane
                        desiredCursorPosition.y = constraints.YMin + (desiredCursorPosition.z - constraints.ZMax);
                        desiredCursorPosition.z = constraints.ZMax;

                        // Make the cursor perpendicular to the XZ plane
                        desiredCursorOrientation = XY_ORIENTATION;

                        // Keep track of the cursor plane change
                        CursorPlane = eCursorPlane.XYPlane;
                    }
                } break;
        }

        // Keep cursor position in bounds at all times
        desiredCursorPosition = constraints.SnapPositionInsideBounds(desiredCursorPosition);

        // Update cursor position and orientation
        this.transform.position = desiredCursorPosition;
        this.transform.rotation = desiredCursorOrientation;
	}
}
