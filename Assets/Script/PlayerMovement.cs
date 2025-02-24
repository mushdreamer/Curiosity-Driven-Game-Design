using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    //basic setting
    public CharacterController characterController;
    private Animator animator;
    //camera
    public Camera playerCamera;
    public Transform cameraHolder;
    private Vector3 moveDirection = Vector3.zero;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    private float rotationX = 0;
    //player info
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
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

        // ˮƽ��ת�� CameraHolder ����
        cameraHolder.Rotate(Vector3.up, mouseX);

        // ͨ����������� Y λ��ʵ�����¿� (�����Խ�ɫΪ����)
        float verticalOffset = Mathf.Sin(Mathf.Deg2Rad * rotationX) * 4f; // 4f ��������ɫ�ľ���
        float zOffset = Mathf.Cos(Mathf.Deg2Rad * rotationX) * -4f; // ������������ɫ�Ĺ̶�ֵ

        // ��������� CameraHolder �ڵ����λ�ã�ʵ���Խ�ɫΪ���ĵ������ӽ�
        playerCamera.transform.localPosition = new Vector3(0, 1.5f + verticalOffset, zOffset);

        // �������ʼ�տ����ɫ����
        playerCamera.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }


}
