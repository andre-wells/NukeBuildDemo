namespace NukeBuildDemo
{
    public class BinaryHelper
    {
        public static string GetBinaryString(int num)
        {
            
            return Convert.ToString(num, 2).PadLeft(8,'0');
        }
    }
}