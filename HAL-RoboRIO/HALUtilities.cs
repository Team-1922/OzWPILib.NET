//File automatically generated using robotdotnet-tools. Please do not modify.

using System.Runtime.InteropServices;

namespace HAL_RoboRIO
{
    public class HALUtilities
    {

        [DllImport(HAL.LibhalathenaSharedSo, EntryPoint = "delayTicks")]
        public static extern void delayTicks(int ticks);

        [DllImport(HAL.LibhalathenaSharedSo, EntryPoint = "delayMillis")]
        public static extern void delayMillis(double ms);

        [DllImport(HAL.LibhalathenaSharedSo, EntryPoint = "delaySeconds")]
        public static extern void delaySeconds(double s);
    }
}
