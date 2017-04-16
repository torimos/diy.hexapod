namespace ServoLink.Contracts
{
    public interface IBinaryHelper
    {
        byte[] ConvertToByteArray(params object[] data);
    }
}
