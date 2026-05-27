using System.Collections;
using UnityEngine;

public class FishAndChips : MonoBehaviour
{
    Animator anim;

    void Awake() => anim = GetComponent<Animator>();

    // 구매 시 호출 — 애니메이션 재생 후 마지막 프레임에 고정
    public void PlayPart()
    {
        anim.enabled = true;
        anim.SetBool("Part", true);
        StartCoroutine(FreezeAfterPlay());
    }

    // 씬 복원 시 호출 — 애니메이션 없이 즉시 최종 상태로 고정
    public void SetPartImmediate()
    {
        anim.enabled = true;
        anim.Play("Parts", 0, 1f);
        anim.Update(0f);
        anim.enabled = false;
    }

    IEnumerator FreezeAfterPlay()
    {
        yield return null; // 애니메이션 시작 대기
        while (true)
        {
            var info = anim.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("Parts") && info.normalizedTime >= 1f)
                break;
            yield return null;
        }
        anim.enabled = false; // 마지막 프레임에 고정 → 다른 애니메이션과 충돌 없음
    }
}
