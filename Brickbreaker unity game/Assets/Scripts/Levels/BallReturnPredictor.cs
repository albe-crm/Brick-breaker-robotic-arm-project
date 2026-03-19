using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BallReturnCheckpoint : MonoBehaviour
{
    public enum AssistLevel { None = 0, Gentle = 1, Strong = 2 }

    [Header("References")]
    public Rigidbody2D ballRb;
    public Transform paddle;
    public LineRenderer line;

    [Header("Prediction Settings")]
    public LayerMask predictionMask;    // walls + TargetLine layer ONLY
    public int maxReflections = 6;
    public float maxDistancePerStep = 30f;
    public bool drawLine = true;

    [Header("Paddle range (must match PaddleScript)")]
    public PaddleScript paddleScript;   // drag your Paddle here

    [Header("Assist thresholds")]
    [Range(0f, 1f)] public float noAssistThreshold = 0.03f;
    [Range(0f, 1f)] public float gentleAssistThreshold = 0.12f;

    [Header("Checkpoint identity")]
    public string checkpointName = "CP_A";

    [Header("Arduino one-shot sizes (match Arduino values)")]
    public int gentleOneShotStep01 = 2;
    public int strongOneShotStep01 = 6;

    [Header("State / Debug")]
    public bool hasPrediction = false;
    public float predictedPaddleY;
    [Range(0f, 1f)] public float lastErrorPercent;
    public AssistLevel lastAssistLevel;

    int assistCount = 0;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Start()
    {
        if (line != null)
        {
            line.positionCount = 0;
            line.useWorldSpace = true;
        }

        if (paddleScript != null)
            paddle = paddleScript.transform;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ball")) return;
        if (ballRb == null || paddleScript == null) return;
        if (paddle == null) paddle = paddleScript.transform;

        Vector2 v = ballRb.linearVelocity;
        Debug.Log($"[{checkpointName}] Ball entered, vel=({v.x:F2},{v.y:F2})");

        // Paddle on RIGHT => predict only when moving right
        if (v.x <= 0f)
        {
            Debug.Log($"[{checkpointName}] Ball moving away, skip");
            hasPrediction = false;
            return;
        }

        PredictAndSendOnce();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Ball")) return;
        if (line != null) line.positionCount = 0;
    }

    void PredictAndSendOnce()
    {
        float clampMinY = paddleScript.downScreenEdge;
        float clampMaxY = paddleScript.upScreenEdge;

        Vector2 center = ballRb.position;
        Vector2 dir = ballRb.linearVelocity.normalized;

        if (dir.sqrMagnitude < 0.0001f) return;

        float radius = 0.2f;
        Vector2 pos = center + dir * radius;

        if (line != null && drawLine)
        {
            line.positionCount = 1;
            line.SetPosition(0, pos);
        }

        hasPrediction = false;
        predictedPaddleY = Mathf.Clamp(pos.y, clampMinY, clampMaxY);

        for (int i = 0; i < maxReflections; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(pos, dir, maxDistancePerStep, predictionMask);

            if (!hit)
            {
                if (line != null && drawLine)
                {
                    Vector2 end = pos + dir * maxDistancePerStep;
                    line.positionCount++;
                    line.SetPosition(line.positionCount - 1, end);
                }
                break;
            }

            if (line != null && drawLine)
            {
                line.positionCount++;
                line.SetPosition(line.positionCount - 1, hit.point);
            }

            if (hit.collider.CompareTag("TargetLine"))
            {
                predictedPaddleY = Mathf.Clamp(hit.point.y, clampMinY, clampMaxY);
                hasPrediction = true;
                Debug.Log($"[{checkpointName}] TargetLine hit at Y={predictedPaddleY:F2}");
                break;
            }

            dir = Vector2.Reflect(dir, hit.normal).normalized;
            pos = hit.point + dir * 0.01f;
        }

        if (!hasPrediction) return;

        float error = Mathf.Abs(predictedPaddleY - paddle.position.y);
        float range = Mathf.Max(0.0001f, clampMaxY - clampMinY);
        float errorPercent = error / range;

        lastErrorPercent = errorPercent;

        AssistLevel level;
        if (errorPercent < noAssistThreshold) level = AssistLevel.None;
        else if (errorPercent < gentleAssistThreshold) level = AssistLevel.Gentle;
        else level = AssistLevel.Strong;

        lastAssistLevel = level;

        int assistInt = (int)level;

        float normalized = Mathf.InverseLerp(clampMinY, clampMaxY, predictedPaddleY);
        int targetAngle01 = Mathf.RoundToInt(normalized * 100f);

        int step01 = (assistInt == 1) ? gentleOneShotStep01 :
                     (assistInt == 2) ? strongOneShotStep01 : 0;

        assistCount++;

        Debug.Log($"[CP {checkpointName}] #{assistCount} level={level} predY={predictedPaddleY:F2} paddleY={paddle.position.y:F2} target01={targetAngle01} step01={step01}");

        paddleScript.currentAssistLevel = assistInt;
        paddleScript.SendAssistCommand(assistInt, targetAngle01);
    }
}
