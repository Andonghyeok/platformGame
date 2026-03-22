using UnityEngine;

public class PlayerCombo : MonoBehaviour
{
    private Animator anim;
    private int comboStep = 0; // 현재 콤보 단계
    private float lastClickedTime; // 마지막 클릭 시간
    public float maxComboDelay = 1.0f; // 콤보로 인정되는 최대 시간

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {


        // 일정 시간이 지나면 콤보 초기화
        if (Time.time - lastClickedTime > maxComboDelay)
        {
            ResetCombo();
        }
    }

    void Attack()
    {
        lastClickedTime = Time.time;
        comboStep++;

        if (comboStep > 3) comboStep = 1; // 3단 콤보 가정

        anim.SetTrigger("Attack");
        anim.SetInteger("ComboStep", comboStep);
    }

    void ResetCombo()
    {
        comboStep = 0;
        anim.SetInteger("ComboStep", 0);
    }
}
