using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Player : MonoBehaviour
{
    CharacterController characterConttroller;
    Animator animator　= null;

    [SerializeField] string playerName;//名前
    [SerializeField] string aliasName = "イフ";//仮の名前
    [SerializeField] float normalSpeed = 3f; // 通常時の移動速度
    [SerializeField] float sprintSpeed = 5f; // ダッシュ時の移動速度
    [SerializeField] float gravity = 10f;    // 重力の大きさ
    [SerializeField] int HP = 1;//HP
    public bool isDead = false;//死亡判定
    public bool isMove = true;//移動判定
    [SerializeField] public bool isHoldKey = false;

    Vector3 moveDirection = Vector3.zero;//移動方向
    Vector3 startPosition;//プレイヤーの初期位置

    private void Start()
    {
        //コンポーネントの取得
        characterConttroller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        //プレイヤーの初期位置
        startPosition = transform.position;
    }


    private void Update()
    {
        
      if (isDead) return;


        // 移動速度を取得。左Shiftキーを入力している間はダッシュ
        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : normalSpeed;

        // 前後左右の入力から、移動のためのベクトルを計算
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        //移動方向を計算
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // isGrounded は地面にいるかどうかを判定する
        if (characterConttroller.isGrounded)
        {
            moveDirection = move * speed;
        }
        else
        {
            // 重力を効かせる
            moveDirection = move + new Vector3(0, moveDirection.y, 0);
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move は指定したベクトルだけ移動させる命令
        characterConttroller.Move(moveDirection  * Time.deltaTime);

        // 移動のアニメーション
        //animator.SetFloat("MoveSpeed", move.magnitude);

        
    }
}
