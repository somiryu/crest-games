using UnityEngine;

/// <summary>
/// Mouse/touch input used by the minigames. Forwards to UnityEngine.Input; in the editor and in
/// development builds it also accepts simulated presses so automated QA bots can play the games.
/// Release builds compile only the forwarding, so they behave exactly as reading Input directly.
/// </summary>
public static class GameInput
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    static bool pendingDown, pendingUp, held;
    static int downFrame = -1, upFrame = -1;
    static Vector3? simulatedPosition;

    /// <summary>Starts a simulated press at a screen position, seen by GetMouseButtonDown(0) on the next frame.</summary>
    public static void SimulatePress(Vector3 screenPosition)
    {
        simulatedPosition = screenPosition;
        pendingDown = true;
        held = true;
    }

    /// <summary>Moves the simulated pointer, while pressed this is a drag.</summary>
    public static void SimulateMove(Vector3 screenPosition) => simulatedPosition = screenPosition;

    /// <summary>Ends the simulated press, seen by GetMouseButtonUp(0) on the next frame.</summary>
    public static void SimulateRelease()
    {
        pendingUp = true;
        held = false;
    }

    public static void ClearSimulation()
    {
        pendingDown = pendingUp = held = false;
        downFrame = upFrame = -1;
        simulatedPosition = null;
    }

    //Pending presses become active on the first query of a frame, so every script reading input that frame sees them
    static void Resolve()
    {
        if (pendingDown) { downFrame = Time.frameCount; pendingDown = false; }
        if (pendingUp) { upFrame = Time.frameCount; pendingUp = false; }
    }

    public static bool GetMouseButtonDown(int button)
    {
        Resolve();
        return Input.GetMouseButtonDown(button) || (button == 0 && downFrame == Time.frameCount);
    }

    public static bool GetMouseButtonUp(int button)
    {
        Resolve();
        return Input.GetMouseButtonUp(button) || (button == 0 && upFrame == Time.frameCount);
    }

    public static bool GetMouseButton(int button)
    {
        Resolve();
        return Input.GetMouseButton(button) || (button == 0 && (held || downFrame == Time.frameCount));
    }

    public static Vector3 mousePosition => simulatedPosition ?? Input.mousePosition;
#else
    public static bool GetMouseButtonDown(int button) => Input.GetMouseButtonDown(button);
    public static bool GetMouseButtonUp(int button) => Input.GetMouseButtonUp(button);
    public static bool GetMouseButton(int button) => Input.GetMouseButton(button);
    public static Vector3 mousePosition => Input.mousePosition;
#endif
}
