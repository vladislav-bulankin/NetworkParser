using System.Runtime.InteropServices;

namespace NetworkParser.UI.Views.Dialogs;

internal static class Win32FileDialog {
    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetSaveFileName (ref OPENFILENAME ofn);

    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName (ref OPENFILENAME ofn);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct OPENFILENAME {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public string lpstrFile;
        public int nMaxFile;
        public string lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
    }

    public static string? ShowSaveDialog (string title, string filter, string defaultExt, string defaultName) {
        var ofn = new OPENFILENAME();
        ofn.lStructSize = Marshal.SizeOf(ofn);
        ofn.lpstrFilter = filter;
        ofn.lpstrFile = defaultName + new string('\0', 512);
        ofn.nMaxFile = ofn.lpstrFile.Length;
        ofn.lpstrTitle = title;
        ofn.lpstrDefExt = defaultExt;
        ofn.lpstrInitialDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        ofn.Flags = 0x00000002; // OFN_OVERWRITEPROMPT

        return GetSaveFileName(ref ofn) ? ofn.lpstrFile.TrimEnd('\0') : null;
    }

    public static string? ShowOpenDialog (string title, string filter) {
        var ofn = new OPENFILENAME();
        ofn.lStructSize = Marshal.SizeOf(ofn);
        ofn.lpstrFilter = filter;
        ofn.lpstrFile = new string('\0', 512);
        ofn.nMaxFile = ofn.lpstrFile.Length;
        ofn.lpstrTitle = title;
        ofn.lpstrInitialDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        ofn.Flags = 0x00001000; // OFN_FILEMUSTEXIST

        return GetOpenFileName(ref ofn) ? ofn.lpstrFile.TrimEnd('\0') : null;
    }
}
