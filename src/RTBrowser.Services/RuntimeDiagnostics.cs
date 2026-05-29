namespace RTBrowser.Services
{
    public static class RuntimeDiagnostics
    {
        public static RuntimeTrace Trace(
            string category,
            string operation)
        {
            return new RuntimeTrace(
                category,
                operation);
        }
    }
}
