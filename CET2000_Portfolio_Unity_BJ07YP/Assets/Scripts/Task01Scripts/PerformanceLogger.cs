using UnityEngine;

// CET2000 Lab 04 - simple performance logger (on the GameManager object).
// Print a report with the "Performance Report" button in the Game view, by right-clicking this component
// and choosing "Print Report", or with the L key (only works if the project uses the old Input Manager).
public class PerformanceLogger : MonoBehaviour
{
    private float startTime;
    private int reachedCount;
    private int collisions;

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.L)) Report();
#endif
    }

    // Works with any Input setting.
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 170, 30), "Performance Report")) Report();
    }

    [ContextMenu("Print Report")]
    public void Report()
    {
        float elapsed = Time.time - startTime;
        Debug.Log($"Performance Report:\nTarget reached: {reachedCount}\nCollisions: {collisions}\nTime: {elapsed:F2}s");
    }

    public void IncrementReached() => reachedCount++;
    public void IncrementCollisions() => collisions++;

    // TODO 4: also record how long the agent takes to reach the target, run tests with different
    // speeds and vision ranges, and enter the results in Documentation/EvaluationTemplate.xlsx.
}
