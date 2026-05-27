using UnityEngine;

/// <summary>
/// 상점은 보스 처치 후 자동으로 열림 (BossHealth.Die → GameManager.GoToShop)
/// 이 컴포넌트는 플레이어에 그대로 두되, B키는 더 이상 사용하지 않음
/// </summary>
public class Shop : MonoBehaviour
{
    // 상점 진입은 BossHealth.Die()에서 GameManager.GoToShop()으로 처리
    // 이 스크립트는 향후 확장용으로 남겨둠
}
