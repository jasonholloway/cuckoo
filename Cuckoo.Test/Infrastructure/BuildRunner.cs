using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Test.Infrastructure
{
    //this class is needed to build a small assembly consisting of the specified classes
    //via the specified buildtask and weavers

    //then an assembly is returned for cecil to inspect and test

    internal class BuildRunner
    {
        Project _project;

        public BuildRunner() {
            var projectCollection = new ProjectCollection();

            _project = new Project();

            

            var projInst = _project.CreateProjectInstance();


            var result = BuildManager.DefaultBuildManager
                            .BuildRequest(new BuildRequestData(projInst, new string[] { "Default" }));

        }


    }
}
