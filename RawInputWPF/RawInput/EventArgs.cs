namespace RawInputWPF.RawInput
{
    public class EventArgs
    {
        public EventArgs(string deviceName)
        {
            DeviceName = deviceName;
        }

        public string DeviceName { get; private set; }
    }
}
