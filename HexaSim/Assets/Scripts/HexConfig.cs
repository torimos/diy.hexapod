public class HexConfig
{
    public const int LegsCount = 6;

    public const float coxaJointSize = 0.3f;
    public const float otherJointSize = coxaJointSize * 0.8f;

    public const float bodyHeight = 0.5f;
    public const float bodyOffset = 0f;

    public const float legsXOffset = 0.54f;
    public const float legsOffsetY = 0;
    public const float legsZOffset = 0.93f;
    public const float legsAngleOffset = 59.7f;

    public const float coxaInitAngle = 0;
    public const float coxaOffsetAngle = 0;
    public const float coxaLength = 0.3f;

    public const float femurInitAngle = 0;
    public const float femurOffsetAngle = 90;
    public const float femurLength = 0.55f;

    public const float tibiaInitAngle = 90;
    public const float tibiaOffsetAngle = 90;
    public const float tibiaLength = 1.4f;


    // 5 LF ^ RF 0
    // 4 LM + RM 1
    // 3 LR . RR 2
    public static int[,] ServoMap = new int[LegsCount, 3] {
        { 8, 7, 6 }, 	//RR - tfc
        { 5, 4, 3 }, 	//RM
        { 2, 1, 0 },  	//RF
        { 11,12,13 }, 	//LR
        { 14,15,16 }, 	//LM
        { 17,18,19 },	//LF 
    };
}