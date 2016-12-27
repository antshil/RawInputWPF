using System;
using System.Linq;
using SharpDX.RawInput;


namespace RawInputWPF.Helpers
{
    public static class DeviceHelper
    {
        public static DeviceInfo SearchDevice(IntPtr devicePtr)
        {
            var devs = Device.GetDevices();
            var device = devs.FirstOrDefault(dev => dev.Handle == devicePtr);

            if (device == null)
            {
                device = new DeviceInfo
                {
                    DeviceName = "",
                    DeviceType = DeviceType.HumanInputDevice,
                    Handle = new IntPtr(0)
                };
            }

            return device;
        }
    }
}
