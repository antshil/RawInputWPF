using System.Collections.Generic;

namespace RawInputWPF.RawInput
{
    public class GamepadEventArgs : EventArgs
    {
        public GamepadEventArgs(List<ushort> buttons, string deviceName)
            : base(deviceName)
        {
            Buttons = buttons;
        }

        public List<ushort> Buttons { get; private set; }
    }
}
