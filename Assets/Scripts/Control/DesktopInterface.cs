using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;
using Dropdown = TMPro.TMP_Dropdown;
using UnityEngine.SceneManagement;

public class DesktopInterface : MonoBehaviour {
    public static DesktopInterface instance;


    public Transform target;

    [Header("Containers")]
    public LineRenderer laser;
    public Text infoDisplay;
    public Camera screenCamera;
    public GameObject fileLoadMenu;

    public GameObject loadedMeshButtonContainer;
    public GameObject loadedMeshButtonTemplate;


    [Header("FreeLook Controls")]
    [Range(-90, 90)]
    public float headRotationMin = -60.0f;
    [Range(-90, 90)]
    public float headRotationMax = 60.0f;
    public float scrollSensitivity = 1f;
    public float mouseSensitivity = 3f;
    public float moveSensitivity = 0.03f;
    public float smoothFrameCount = 4;

    [Header("Locked Orbit Controls")]
    public float distanceMin = .5f;
    public float distanceMax = 2f;

    public bool dataDragEnabled = true;
    public bool lineDrawEnabled = true;

    // Private
    private float distance = 1.0f;
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private List<float> rotArrayX = new List<float>();
    float rotAverageX = 0F;
    private List<float> rotArrayY = new List<float>();
    float rotAverageY = 0F;
    
    // Drag properties
    private GameObject dragTarget;
    private bool currentlyDragging;
    private Vector3 dragOffset;
    private Quaternion rotationOffset;
    private Vector3 eulerOffset;
    private Vector3 targetScreenPosition;
    
    void Start() {

        instance = this;
        // Make the rigid body not change rotation
        if (screenCamera.GetComponent<Rigidbody>()) {
            screenCamera.GetComponent<Rigidbody>().freezeRotation = true;
        }
        rotationX = screenCamera.transform.localEulerAngles.y;
        rotArrayX.Add(rotationX);
        rotationY = -screenCamera.transform.localEulerAngles.x;
        rotArrayY.Add(rotationY);
        rotAverageX = rotationX;
        rotAverageY = rotationY;
    }

    void Update() {
        CanvasSwitch();
        if(lineDrawEnabled)
            LineDraw();
        if(dataDragEnabled)
            DataDrag();
        ProcessMouseMove();
    }

    public void RefreshLoadedMeshesMenu() {

        // Remove all existing mesh buttons
        for (int deleteCount = 0; deleteCount < loadedMeshButtonContainer.transform.childCount; deleteCount++) {
            Destroy(loadedMeshButtonContainer.transform.GetChild(deleteCount).gameObject);
        }

        // Recreate mesh buttons
        if (DataLoader.instance.loadedData.Count > 0) {
            //fileLoadMenu.SetActive(false);
            foreach (GameObject obj in DataLoader.instance.loadedData) {
                DataObject data = obj.GetComponentInChildren<DataObject>();
                GameObject newButton = Instantiate(loadedMeshButtonTemplate, loadedMeshButtonContainer.transform);
                newButton.SetActive(true);
                newButton.GetComponentInChildren<Text>().text = data.identifier;
                newButton.GetComponent<Button>().onClick.AddListener(() => { SetTarget(obj.GetComponentInChildren<MeshVRControls>().transform); });
            }
            if (target == null) {
                SetTarget(DataLoader.instance.loadedData[0].transform);
            }
        }
        else {
            //fileLoadMenu.SetActive(true);
        }

    }

    // Callbacks
    public void EnableLoadMenu() {
        fileLoadMenu.SetActive(true);
    }
    public void CloseLoadMenu() {
        fileLoadMenu.SetActive(false);
    }

    public void SwapView() {
        if (screenCamera.rect.x != 0) {
            screenCamera.rect = new Rect(0, 0, 1, 1);
        }
        else {
            screenCamera.rect = new Rect(0.75f, 0, 0.25f, 0.25f);
        }
    }

    private void ProcessMouseMove() { 

        // Move toward and away from the camera
        Vector3 relativeTranslation = Vector3.zero;
        Vector3 absoluteTranslation = Vector3.zero;
        if (Input.GetAxis("Vertical") != 0) {
            relativeTranslation.z += Input.GetAxis("Vertical") * moveSensitivity;
            target = null;
        }
        // Strafe the camera
        if (Input.GetAxis("Horizontal") != 0) {
            relativeTranslation.x += Input.GetAxis("Horizontal") * moveSensitivity;
            target = null;
        }
        // Elevate/drop the camera
        if (Input.GetAxis("Jump") != 0) {
            absoluteTranslation.y += Input.GetAxis("Jump") * moveSensitivity;
            target = null;
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            if (target != null)
                distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity, distanceMin, distanceMax);
            else
                distance = 1;
            if((target != null && distance > distanceMin && distance < distanceMax) || target == null) {
                relativeTranslation.z += Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
            }
        }
        // Middle click panning (invert values for natural effect)
        if(Input.GetMouseButton(2)) {
            relativeTranslation.x += -Input.GetAxis("Mouse X") * moveSensitivity * 4;
            relativeTranslation.y += -Input.GetAxis("Mouse Y") * moveSensitivity * 4;
            distance = 1;
            target = null;
        }

        screenCamera.transform.localPosition += screenCamera.transform.localRotation * relativeTranslation + absoluteTranslation;

        if (Input.GetMouseButton(1)) {
            //Lock cursor
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;

            // Read the mouse input axis
            rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
            rotationY += Input.GetAxis("Mouse Y") * mouseSensitivity;

            rotArrayX.Add(rotationX);
            rotArrayY.Add(rotationY);

            if (rotArrayX.Count >= smoothFrameCount) {
                rotArrayX.RemoveRange(0, rotArrayX.Count + 1 - (int) smoothFrameCount);
            }
            if (rotArrayY.Count >= smoothFrameCount) {
                rotArrayY.RemoveRange(0, rotArrayY.Count + 1 - (int)smoothFrameCount);
            }

            for (int i = 0; i < rotArrayX.Count; i++) {
                rotAverageX += rotArrayX[i];
            }
            for (int j = 0; j < rotArrayY.Count; j++) {
                rotAverageY += rotArrayY[j];
            }

            rotAverageX /= rotArrayX.Count;
            rotAverageY /= rotArrayY.Count;

            Adjust360andClamp();
        }
        else {
            //Reset the cursor if right click is let go
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        // Joystick controls for right stick
        if (Input.GetAxis("RightStickVert") != 0 || Input.GetAxis("RightStickHoriz") != 0) {
            // Read the mouse input axis
            rotationX += Input.GetAxis("RightStickHoriz") * mouseSensitivity;
            rotationY += Input.GetAxis("RightStickVert") * mouseSensitivity;

            rotArrayX.Add(rotationX);
            rotArrayY.Add(rotationY);

            if (rotArrayX.Count >= smoothFrameCount) {
                rotArrayX.RemoveAt(0);
            }
            if (rotArrayY.Count >= smoothFrameCount) {
                rotArrayY.RemoveAt(0);
            }

            for (int i = 0; i < rotArrayX.Count; i++) {
                rotAverageX += rotArrayX[i];
            }
            for (int j = 0; j < rotArrayY.Count; j++) {
                rotAverageY += rotArrayY[j];
            }
            rotAverageX /= rotArrayX.Count;
            rotAverageY /= rotArrayY.Count;

            Adjust360andClamp();
        }

        screenCamera.transform.localRotation = Quaternion.AngleAxis(rotAverageX, Vector3.up);
        screenCamera.transform.localRotation *= Quaternion.AngleAxis(rotAverageY, Vector3.left);
        if (target != null) {
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = screenCamera.transform.localRotation * negDistance + target.position;
            screenCamera.transform.position = position;
        }
    }

    public void Adjust360andClamp() {
        // Don't let our X go beyond 360 degrees + or -
        if (rotationX < -360) {
            rotationX += 360;
        }
        else if (rotationX > 360) {
            rotationX -= 360;
        }

        // Don't let our Y go beyond 360 degrees + or -
        if (rotationY < -360) {
            rotationY += 360;
        }
        else if (rotationY > 360) {
            rotationY -= 360;
        }

        // Clamp our angles to the min and max set in the Inspector
        rotationY = Mathf.Clamp(rotationY, headRotationMin, headRotationMax);
    }

    public void SetTarget(Transform newTarget) {
        target = newTarget;
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = screenCamera.transform.localRotation * negDistance + target.position;

        this.distance = Vector3.Distance(screenCamera.transform.position, target.position);
        screenCamera.transform.localRotation = Quaternion.Euler(80, 0, 80);
        screenCamera.transform.position = position;
    }

    void CanvasSwitch() {
        RaycastHit hitInfo;
        if (Physics.Raycast(screenCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, 5)) {
            Canvas currentCanvas = hitInfo.collider.GetComponentInParent<Canvas>();
            if (currentCanvas != null && currentCanvas.worldCamera != screenCamera) {
                currentCanvas.worldCamera = screenCamera;
            }
        }
    }

    void LineDraw() {
        // Display info
        RaycastHit hitInfo;
        if (Physics.Raycast(screenCamera.ScreenPointToRay(Input.mousePosition),  out hitInfo, 5, ~LayerMask.GetMask("Ignore Raycast", "Grab"))) {
            if (hitInfo.collider.tag == "Data") {
                laser.enabled = true;
                laser.SetPosition(0, laser.transform.position);
                laser.SetPosition(1, hitInfo.point);
                //TooltipBehavior.instance.DisplayHitInfo((MeshCollider)hitInfo.collider, hitInfo.triangleIndex, hitInfo.point, infoDisplay);
            }
            else {
                laser.enabled = false;
                infoDisplay.text = "";
            }
        }
        else {
            laser.enabled = false;
        }
    }


    void DataDrag() {
        // Mouse Button Press Down
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hitInfo;
            if(Physics.Raycast(screenCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, LayerMask.GetMask("Grab"))) {
                if(hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Grab"))
                    dragTarget = hitInfo.collider.GetComponentInParent<MeshVRControls>().gameObject;
            }
            if (dragTarget != null) {
                currentlyDragging = true;
                target = null;
                //Converting world position to screen position.
                targetScreenPosition = screenCamera.WorldToScreenPoint(dragTarget.transform.position);
                dragOffset = dragTarget.transform.position - screenCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, targetScreenPosition.z));
                eulerOffset = dragTarget.transform.localEulerAngles - screenCamera.transform.localEulerAngles;
            }
        }

        // Mouse Button Up
        if (Input.GetMouseButtonUp(0)) {
            if (currentlyDragging && dragTarget != null) {
                BoxClipControls controls = dragTarget.GetComponent<BoxClipControls>();
                if (controls != null)
                    controls.ResetBox();
                dragTarget = null;
            }
            currentlyDragging = false;
        }

        // Check for drag object
        if (currentlyDragging) {
            Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, targetScreenPosition.z);
            Vector3 currentPosition = screenCamera.ScreenToWorldPoint(currentScreenSpace) + dragOffset;
            Quaternion currentRotation = screenCamera.transform.localRotation * rotationOffset * dragTarget.transform.localRotation ;
            dragTarget.transform.position = currentPosition;
            //dragTarget.transform.localRotation = currentRotation;
            dragTarget.transform.localEulerAngles = screenCamera.transform.localEulerAngles + eulerOffset;
        }
    }

    public void Reset() {
        Scene scene = SceneManager.GetActiveScene(); 
        SceneManager.LoadScene(scene.name);
    }
}