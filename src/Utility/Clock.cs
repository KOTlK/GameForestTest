public static class Clock {
    public static float Delta;
    public static float RealTimeDelta;
    public static float Time;
    public static float RealTime;
    public static float Scale = 1f;
    public static int   FrameCount;
    
    public static void Update(float dt) {
        RealTimeDelta  = dt;
        Delta          = dt * Scale;
        Time          += Delta;
        RealTime      += dt;
        FrameCount++;
    }
}