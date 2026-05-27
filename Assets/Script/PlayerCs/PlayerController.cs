using UnityEngine;
using UnityEngine.EventSystems; // UI 클릭 감지 기능 (현재 미사용)

// 파일명은 PlayerController.cs지만 클래스 이름은 PlayerMovement — 헷갈리지 말 것
// MonoBehaviour = Unity 오브젝트에 붙일 수 있는 스크립트의 기본 형태
public class PlayerMovement : MonoBehaviour
{
    // Inspector에서 조절 가능한 이동 수치들
    // speed         = 최대 이동 속도
    // acceleration  = 키를 누를 때 속도가 올라가는 빠르기 (클수록 즉각 반응)
    // deceleration  = 키를 뗄 때 속도가 줄어드는 빠르기 (클수록 즉시 멈춤)
    // rotationSpeed = 이동 방향으로 몸을 돌리는 빠르기
    public float speed = 30f, acceleration = 8f, deceleration = 3f, rotationSpeed = 5f;

    // [HideInInspector] = Inspector 창에는 안 보이지만 다른 스크립트에서 접근 가능
    // speedMultiplier = 외부에서 속도를 배율로 조절하는 용도
    //   예) 스킬 SpeedBoost가 이걸 2.0으로 바꾸면 속도 2배
    //   기본값 1f = 배율 없음 (정상 속도)
    [HideInInspector] public float speedMultiplier = 1f;

    // 현재 실제 이동 속도 벡터 (방향 + 크기 포함)
    // 매 프레임 갱신되며 관성/감속 표현에 사용
    Vector3 velocity;

    // Update = 매 프레임(1초에 약 60번) 자동으로 실행되는 함수
    void Update()
    {
        // ── 입력 읽기 ──────────────────────────────────────────────────
        // GetAxisRaw("Horizontal") : A/D 또는 ←/→ 키 → -1, 0, 1 반환
        // GetAxisRaw("Vertical")   : W/S 또는 ↑/↓ 키 → -1, 0, 1 반환
        // y = 0 : 위아래(점프) 없는 평면 이동
        // .normalized : 대각선 이동 시 속도가 빨라지지 않도록 길이를 1로 맞춤
        //   (오른쪽+위 동시 입력 시 실제 이동거리가 루트2배가 되는 걸 방지)
        Vector3 dir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        // ── 가속/감속 계산 ─────────────────────────────────────────────
        // 키를 누르고 있으면 → acceleration, 키를 안 누르면 → deceleration
        // Time.deltaTime을 곱하는 이유 : 프레임레이트가 달라도 같은 속도가 되도록 보정
        //   (60fps든 30fps든 1초에 움직이는 거리가 동일하게)
        float t = (dir != Vector3.zero ? acceleration : deceleration) * Time.deltaTime; 

        // Vector3.Lerp(a, b, t) : a에서 b 방향으로 t만큼 부드럽게 이동
        // 현재 velocity에서 목표 속도(dir * speed * speedMultiplier)로 조금씩 다가감
        // → 키를 누르면 서서히 가속, 떼면 서서히 감속하는 관성 표현
        velocity = Vector3.Lerp(velocity, dir * speed * speedMultiplier, t);

        // ── 몸 회전 ────────────────────────────────────────────────────
        // 이동 입력이 있을 때만 회전 (멈춰 있을 때는 마지막 방향 유지)
        // Quaternion.LookRotation(dir) : dir 방향을 바라보는 회전값 계산
        // Quaternion.Lerp : 현재 회전에서 목표 회전으로 부드럽게 돌아감
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);

        // ── 실제 이동 적용 ─────────────────────────────────────────────
        // transform.Translate : 오브젝트를 velocity 방향으로 이동
        // Space.World : 월드 좌표 기준으로 이동
        //   (오브젝트가 어느 방향을 보든 상관없이 W키 = 항상 화면 위쪽으로 이동)
        transform.Translate(velocity * Time.deltaTime, Space.World);
    }  
}
