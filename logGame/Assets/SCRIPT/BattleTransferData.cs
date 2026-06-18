using UnityEngine;

// 씬 간에 전투 관련 데이터(위치, 스탯, 승리여부)를 안전하게 넘겨주기 위한 static 클래스
public static class BattleTransferData
{
    // 필드 적 정보 (이전 코드 유지 및 확장)
    public static string enemyName = "";
    public static int enemyAttack = 0;
    public static int enemyMaxHealth = 0;
    public static int enemyDefense = 0;
    public static int enemyCurrentHealth = 0;

    // ★ 필드 복귀용 데이터 추가
    public static Vector3 playerFieldPosition; // 적과 부딪혔을 때 플레이어 위치
    public static int encounteredEnemyInstanceID; // 부딪힌 적 오브젝트의 고유 ID
    public static bool isBattleWon = false; // true = 이김, false = 도망침 또는 패배

    // ★ [NPC 전투용 핵심 데이터 추가]
    public static bool isNPCBattle = false;          // true면 NPC와의 대화 도중 발생한 전투로 인식
    public static int encounteredNPCInstanceID;     // 전투를 건 NPC의 고유 ID
}