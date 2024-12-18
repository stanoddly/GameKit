using System.Diagnostics;

namespace GameKit.ShaderPack;

public static class SlangCompiler
{
    public static void Compile(params string[] args)
    {
        using (Process p = new Process())
        {
            ProcessStartInfo info = new ProcessStartInfo("/opt/slang/bin/slangc");
            info.Arguments = string.Join(" ", args);
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            p.StartInfo = info;
            p.Start();

            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                throw new NotImplementedException();
            }
            
            string output = p.StandardOutput.ReadToEnd();
        }
    }
}