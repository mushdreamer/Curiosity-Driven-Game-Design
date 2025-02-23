using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public Transform cameraHolder; // CameraHolder ��������ӽǶ�������
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

        // ����ˮƽ�����ƶ� (�Ƴ�Y��Ӱ��)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool isWalking = !isRunning && (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0);

        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float curSpeedZ = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float movementDirectionY = moveDirection.y;

        // �����ɫ���ƶ����� (A/D ��ˮƽ�ƶ���W/S ��ǰ���ƶ�)
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

        // ��������
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);

        // ��ɫ�����ƶ����� (����S���鴤��ȷ��A/DΪ�����ƶ�)
        Vector3 flatMoveDirection = new Vector3(moveDirection.x, 0, moveDirection.z);
        if (flatMoveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatMoveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    void LateUpdate()
    {
        // CameraHolder ֱ�Ӹ����ɫλ�ã���ƫ��
        cameraHolder.position = transform.position;

        // ����������ӽǣ�����������ת
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        // ˮƽ��ת�� CameraHolder ���ƣ���ֱ��ת������������
        cameraHolder.Rotate(Vector3.up, mouseX);

        // ��������� CameraHolder �ڲ������λ�ã���֤�Խ�ɫΪԲ��
        playerCamera.transform.localPosition = new Vector3(0, 1.5f, -4f);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        // ȷ�����ʼ�տ����ɫ����
        playerCamera.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }

}
