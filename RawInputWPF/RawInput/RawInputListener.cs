using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Interop;
using RawInputWPF.Helpers;
using SharpDX.Multimedia;
using SharpDX.RawInput;


namespace RawInputWPF.RawInput
{
    public class RawInputListener
    {
        public event EventHandler<GamepadEventArgs> ButtonsChanged;

        public event EventHandler<KeyboardEventArgs> KeyDown;
        public event EventHandler<KeyboardEventArgs> KeyUp;

        private const int WM_INPUT = 0x00FF;
        private HwndSource _hwndSource;

        public void Init(IntPtr hWnd)
        {
            if (_hwndSource != null)
            {
                return;
            }

            _hwndSource = HwndSource.FromHwnd(hWnd);
            if (_hwndSource != null)
                _hwndSource.AddHook(WndProc);

            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericGamepad, DeviceFlags.InputSink, hWnd);
            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, DeviceFlags.InputSink, hWnd);

            Device.RawInput += OnRawInput;
            Device.KeyboardInput += OnKeyboardInput;
        }

        public void Clear()
        {
            Device.RawInput -= OnRawInput;
            Device.KeyboardInput -= OnKeyboardInput;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_INPUT)
            {
                Device.HandleMessage(lParam);
            }

            return IntPtr.Zero;
        }

        private void OnKeyboardInput(object sender, KeyboardInputEventArgs e)
        {
            if (e.State != KeyState.KeyDown && e.State != KeyState.KeyUp)
                return;

            var handler = e.State == KeyState.KeyDown ? KeyDown : KeyUp;
            if (handler == null)
                return;

            var deviceName = "";
            if (e.Device != IntPtr.Zero)
                deviceName = DeviceHelper.SearchDevice(e.Device).DeviceName;

            var key = KeyInterop.KeyFromVirtualKey((int)e.Key);
            var evt = new KeyboardEventArgs(deviceName, key);
            handler(this, evt);
        }

        private void OnRawInput(object sender, RawInputEventArgs e)
        {
            var handler = ButtonsChanged;
            if (handler == null)
                return;

            var hidInput = e as HidInputEventArgs;
            if (hidInput == null)
                return;

            if (e.Device == IntPtr.Zero)
                return;

            var deviceName = DeviceHelper.SearchDevice(e.Device).DeviceName;
            List<ushort> pressedButtons;
            RawInputParser.Parse(hidInput, out pressedButtons);

            handler(this, new GamepadEventArgs(pressedButtons, deviceName));
        }
    }
}
