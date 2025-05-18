namespace FlipFix
{
    internal static class Program
    {
        // Create a unique mutex name for your application
        private static readonly string MutexName = "PdfFlipFixSingleInstanceMutex";
        private static Mutex _mutex = null;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Try to create a new mutex with the specified name
            bool createdNew;
            _mutex = new Mutex(true, MutexName, out createdNew);

            // If the mutex already exists, another instance is running
            if (!createdNew)
            {
                MessageBox.Show("An instance of PdfFlipFix is already running.", "PdfFlipFix",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return; // Exit the application
            }

            try
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                Application.Run(new FixForm());
            }
            finally
            {
                // Release the mutex when the application closes
                _mutex.ReleaseMutex();
            }
        }
    }
}