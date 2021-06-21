public class ServoState
{
    public int position;
    public int positionDelta;
    public int positionNew;

    public void ProcessData(uint data)
    {
        int moveTime = (int)((data >> 16) & 0xFFFF);
        int pos = (int)((data) & 0xFFFF);

        int ticks = moveTime / 20;
        if (position != pos &&
            position > 0 &&
            pos > 0 &&
            moveTime > 0)
        {
            positionNew = pos;
            positionDelta = (positionNew - position) / ticks;
        }
        else
        {
            position = pos;
            positionNew = 0;
            positionDelta = 0;
        }
    }

    public void Update()
    {
        if (positionDelta != 0)
        {
            position += positionDelta;
            if (positionDelta > 0)
            {
                if (position >= positionNew)
                {
                    position = positionNew;
                    positionDelta = 0;
                }
            }
            else if (positionDelta < 0)
            {
                if (position <= positionNew)
                {
                    position = positionNew;
                    positionDelta = 0;
                }
            }
        }
    }
}