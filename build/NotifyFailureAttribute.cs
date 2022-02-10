using Nuke.Common;
using Nuke.Common.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NotifyFailureAttribute : BuildExtensionAttributeBase, IOnBuildFinished
{
    public void OnBuildFinished(NukeBuild build)
    {
        if(!build.IsSuccessful)
        {
            Serilog.Log.Fatal("Build failed: this is the attribute way of handling it but we don't have a great way to await this call :|");
        }
    }
}

