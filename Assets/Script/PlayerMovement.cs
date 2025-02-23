using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public Transform cameraHolder; // CameraHolder 用于相机视角独立控制
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    public CharacterController characterController;

    private bool canMove = true;

    private Animator animator;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 forward = cameraHolder.transform.forward;
        Vector3 right = cameraHolder.transform.right;

        // 仅在水平面上移动 (移除Y轴影响)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool isWalking = !isRunning && (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0);

        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float curSpeedZ = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float movementDirectionY = moveDirection.y;

        // 计算角色的移动方向 (A/D 是水平移动，W/S 是前后移动)
        moveDirection = (forward * curSpeedZ) + (right * curSpeedX);
        moveDirection.y = movementDirectionY;

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.R) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        // 动画控制
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);

        // 角色朝向移动方向 (避免S键抽搐，确保A/D为侧向移动)
        Vector3 flatMoveDirection = new Vector3(moveDirection.x, 0, moveDirection.z);
        if (flatMoveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatMoveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    void LateUpdate()
    {
        // CameraHolder 直接跟随角色位置，无偏移
        cameraHolder.position = transform.position;

        // 鼠标控制相机视角，保持自由旋转
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        // 水平旋转由 CameraHolder 控制
        cameraHolder.Rotate(Vector3.up, mouseX);

        // 通过调整相机的 Y 位置实现上下看 (保持以角色为中心)
        float verticalOffset = Mathf.Sin(Mathf.Deg2Rad * rotationX) * 4f; // 4f 是相机与角色的距离
        float zOffset = Mathf.Cos(Mathf.Deg2Rad * rotationX) * -4f; // 保持相机距离角色的固定值

        // 设置相机在 CameraHolder 内的相对位置，实现以角色为中心的上下视角
        playerCamera.transform.localPosition = new Vector3(0, 1.5f + verticalOffset, zOffset);

        // 保持相机始终看向角色中心
        playerCamera.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }


}
