public class DataStructureUtils
{
    /// <summary> Map array size to bucket index based on power-of-2 increments </summary>
    public static int GetQueueIndex(int size) => size switch
    {
        8 => 0, 16 => 1, 32 => 2, 64 => 3, 128 => 4, 256 => 5,
        512 => 6, 1024 => 7, 2048 => 8, 4096 => 9, 8192 => 10,
        16384 => 11, 32768 => 12, 65536 => 13, 131072 => 14,
        262144 => 15, _ => -1
    };

}