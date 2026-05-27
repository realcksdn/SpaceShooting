using UnityEngine;

/// <summary>
/// 시작 시 넓은 범위에 별 프리팹을 랜덤으로 뿌림
/// </summary>
public class StarField : MonoBehaviour
{
    public GameObject starPrefab;
    public int   count     = 200;   // 별 개수
    public float rangeX    = 100f;  // 뿌릴 가로 범위
    public float rangeY    = 100f;  // 뿌릴 세로 범위
    public float minSize   = 0.05f;
    public float maxSize   = 0.2f;
    public float zDepth    = 5f;    // 카메라보다 뒤에 위치

    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            // 탑뷰: X-Z 평면에 뿌리고 Y는 카메라보다 아래
            Vector3 pos = new Vector3(
                Random.Range(-rangeX, rangeX),
                -zDepth,
                Random.Range(-rangeY, rangeY)
            );

            GameObject star = Instantiate(starPrefab, pos, Quaternion.Euler(90f, 0f, 0f), transform);
            float size = Random.Range(minSize, maxSize);
            star.transform.localScale = Vector3.one * size;
        }
    }
}
