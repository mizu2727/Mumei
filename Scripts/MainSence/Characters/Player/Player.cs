using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Player : MonoBehaviour
{
    CharacterController characterConttroller;
    Animator animator�@= null;

    [SerializeField] string playerName;//���O
    [SerializeField] string aliasName = "�C�t";//���̖��O
    [SerializeField] float normalSpeed = 3f; // �ʏ펞�̈ړ����x
    [SerializeField] float sprintSpeed = 5f; // �_�b�V�����̈ړ����x
    [SerializeField] float gravity = 10f;    // �d�͂̑傫��
    [SerializeField] int HP = 1;//HP
    public bool isDead = false;//���S����
    public bool isMove = true;//�ړ�����
    [SerializeField] public bool isHoldKey = false;

    Vector3 moveDirection = Vector3.zero;//�ړ�����
    Vector3 startPosition;//�v���C���[�̏����ʒu

    private void Start()
    {
        //�R���|�[�l���g�̎擾
        characterConttroller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        //�v���C���[�̏����ʒu
        startPosition = transform.position;
    }


    private void Update()
    {
        
      if (isDead) return;


        // �ړ����x���擾�B��Shift�L�[����͂��Ă���Ԃ̓_�b�V��
        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : normalSpeed;

        // �O�㍶�E�̓��͂���A�ړ��̂��߂̃x�N�g�����v�Z
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        //�ړ��������v�Z
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // isGrounded �͒n�ʂɂ��邩�ǂ����𔻒肷��
        if (characterConttroller.isGrounded)
        {
            moveDirection = move * speed;
        }
        else
        {
            // �d�͂���������
            moveDirection = move + new Vector3(0, moveDirection.y, 0);
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move �͎w�肵���x�N�g�������ړ������閽��
        characterConttroller.Move(moveDirection  * Time.deltaTime);

        // �ړ��̃A�j���[�V����
        //animator.SetFloat("MoveSpeed", move.magnitude);

        
    }
}
