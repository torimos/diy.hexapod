public class HexConfig
{
    public const float coxaJointSize = 0.3f;
    public const float otherJointSize = coxaJointSize * 0.8f;

    public const float bodyHeight = 0.5f;
    public const float bodyOffset = bodyHeight/2 + otherJointSize / 2;

    public const float legsXOffset = 0.54f;
    public const float legsOffsetY = bodyHeight/2  + bodyOffset;
    public const float legsZOffset = 0.93f;
    public const float legsAngleOffset = 59.7f;

    public const float coxaInitAngle = 0;
    public const float coxaOffsetAngle = 0;
    public const float coxaLength = 0.3f;

    public const float femurInitAngle = 0;
    public const float femurOffsetAngle = 90;
    public const float femurLength = 0.55f;

    public const float tibiaInitAngle = -90;
    public const float tibiaOffsetAngle = 90;
    public const float tibiaLength = 1.4f;


    // 5 LF ^ RF 0
    // 4 LM + RM 1
    // 3 LR . RR 2
    public static int[,] ServoMap = new int[6, 3] { //cfr RR RM RF  LR LM LF
        {18,17,16}, {13,12,19}, { 8,15,14},
        { 1, 2, 3}, { 6, 7, 0}, {11, 4, 5}
    };
}
