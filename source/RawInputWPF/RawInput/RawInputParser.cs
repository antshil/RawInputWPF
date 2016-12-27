using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SharpDX.RawInput;


namespace RawInputWPF.RawInput
{
    public static class RawInputParser
    {
        public static bool Parse(HidInputEventArgs hidInput, out List<ushort> pressedButtons)
        {
            var preparsedData = IntPtr.Zero;
            pressedButtons = new List<ushort>();

            try
            {
                preparsedData = GetPreparsedData(hidInput.Device);
                if (preparsedData == IntPtr.Zero)
                    return false;

                HIDP_CAPS hidCaps;
                CheckError(HidP_GetCaps(preparsedData, out hidCaps));

                pressedButtons = GetPressedButtons(hidCaps, preparsedData, hidInput.RawData);
            }
            catch (Win32Exception e)
            {
                return false;
            }
            finally
            {
                if (preparsedData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(preparsedData);
                }
            }

            return true;
        }


        #region InternalMethods

        private static void CheckError(int retCode)
        {
            if (retCode != HIDP_STATUS_SUCCESS)
            {
                throw new Win32Exception();
            }
        }

        private static IntPtr GetPreparsedData(IntPtr device)
        {
            uint reqDataSize = 0;
            GetRawInputDeviceInfo(device, RIDI_PREPARSEDDATA, IntPtr.Zero, ref reqDataSize);

            var preparsedData = Marshal.AllocHGlobal((int)reqDataSize);

            GetRawInputDeviceInfo(device, RIDI_PREPARSEDDATA, preparsedData, ref reqDataSize);

            return preparsedData;
        }

        private static List<ushort> GetPressedButtons(HIDP_CAPS hidCaps, IntPtr preparsedData, byte[] rawInputData)
        {
            var buttonCapsLength = hidCaps.NumberInputButtonCaps;
            var buttonCaps = new HIDP_BUTTON_CAPS[buttonCapsLength];
            CheckError(HidP_GetButtonCaps(HIDP_REPORT_TYPE.HidP_Input, buttonCaps, ref buttonCapsLength, preparsedData));

            var usagePages = new HashSet<ushort>();
            foreach (var bc in buttonCaps)
            {
                usagePages.Add(bc.UsagePage);
            }

            var res = new List<ushort>();
            foreach (var usagePage in usagePages)
            {
                int usageListLength = hidCaps.NumberInputButtonCaps;
                var usageList = new ushort[usageListLength];

                CheckError(HidP_GetUsages(HIDP_REPORT_TYPE.HidP_Input, usagePage, 0,
                    usageList, ref usageListLength, preparsedData, rawInputData, rawInputData.Length));

                for (var i = 0; i < usageListLength; ++i)
                {
                    res.Add(usageList[i]);
                }
            }

            return res;
        }

        #endregion InternalMethods


        #region NativeHidApi

        private enum HIDP_REPORT_TYPE
        {
            HidP_Input,
            HidP_Output,
            HidP_Feature,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct HIDP_CAPS
        {
            public ushort Usage;
            public ushort UsagePage;
            public ushort InputReportByteLength;
            public ushort OutputReportByteLength;
            public ushort FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public ushort[] Reserved;
            public ushort NumberLinkCollectionNodes;
            public ushort NumberInputButtonCaps;
            public ushort NumberInputValueCaps;
            public ushort NumberInputDataIndices;
            public ushort NumberOutputButtonCaps;
            public ushort NumberOutputValueCaps;
            public ushort NumberOutputDataIndices;
            public ushort NumberFeatureButtonCaps;
            public ushort NumberFeatureValueCaps;
            public ushort NumberFeatureDataIndices;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct HIDP_BUTTON_CAPS
        {
            [FieldOffset(0)]
            public ushort UsagePage;
            [FieldOffset(2)]
            public byte ReportID;
            [FieldOffset(3)]
            public byte IsAlias;
            [FieldOffset(4)]
            public ushort BitField;
            [FieldOffset(6)]
            public ushort LinkCollection;
            [FieldOffset(8)]
            public ushort LinkUsage;
            [FieldOffset(10)]
            public ushort LinkUsagePage;
            [FieldOffset(12)]
            public byte IsRange;
            [FieldOffset(13)]
            public byte IsStringRange;
            [FieldOffset(14)]
            public byte IsDesignatorRange;
            [FieldOffset(15)]
            public byte IsAbsolute;
            [FieldOffset(56)]
            public ushort Usage;
            [FieldOffset(58)]
            public ushort Reserved1;
            [FieldOffset(60)]
            public ushort StringIndex;
            [FieldOffset(62)]
            public ushort Reserved2;
            [FieldOffset(64)]
            public ushort DesignatorIndex;
            [FieldOffset(66)]
            public ushort Reserved3;
            [FieldOffset(68)]
            public ushort DataIndex;
            [FieldOffset(70)]
            public ushort Reserved4;
        }

        // HID status codes (all codes see in <hidpi.h>).
        private const int HIDP_STATUS_SUCCESS = (0x0 << 28) | (0x11 << 16) | 0;

        // https://msdn.microsoft.com/ru-ru/library/windows/desktop/ms645597(v=vs.85).aspx
        // Commands for GetRawInputDeviceInfo
        private const uint RIDI_PREPARSEDDATA = 0x20000005;

        // https://msdn.microsoft.com/ru-ru/library/windows/desktop/ms645597(v=vs.85).aspx
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputDeviceInfo(IntPtr device, uint command, IntPtr outData, ref uint dataSize);

        // http://msdn.microsoft.com/en-us/library/windows/hardware/ff539715%28v=vs.85%29.aspx
        [DllImport("hid.dll", SetLastError = true)]
        private static extern int HidP_GetCaps(IntPtr preparsedData, out HIDP_CAPS capabilities);

        // http://msdn.microsoft.com/en-us/library/windows/hardware/ff539707%28v=vs.85%29.aspx
        [DllImport("hid.dll", SetLastError = true)]
        private static extern int HidP_GetButtonCaps(HIDP_REPORT_TYPE reportType, [In, Out] HIDP_BUTTON_CAPS[] buttonCaps,
            ref ushort buttonCapsLength, IntPtr preparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern int HidP_GetUsages(HIDP_REPORT_TYPE reportType, ushort usagePage, ushort linkCollection,
            [In, Out] ushort[] usageList, ref int usageLength, IntPtr preparsedData,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 7)] byte[] report, int reportLength);

        #endregion NativeHidApi
    }

}
