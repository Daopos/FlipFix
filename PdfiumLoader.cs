using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

public static class PdfiumLoader
{
    [DllImport("kernel32", SetLastError = true)]
    static extern IntPtr LoadLibrary(string lpFileName);

    public static void LoadPdfium()
    {
        string dllPath = Path.Combine(Path.GetTempPath(), "pdfium.dll");

        if (!File.Exists(dllPath))
        {
            using Stream? s = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("FlipFix.x64.pdfium.dll"); // Adjust namespace!
            using FileStream fs = new FileStream(dllPath, FileMode.Create, FileAccess.Write);
            s?.CopyTo(fs);
        }

        LoadLibrary(dllPath);
    }
}
