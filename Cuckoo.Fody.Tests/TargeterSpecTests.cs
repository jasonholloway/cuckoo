using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cuckoo.Gather.Monikers;
using Cuckoo.Gather.Targeters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cuckoo.Fody.Test
{
    [TestClass]
    public class TargeterSpecTests
    {
        //[TestMethod]
        //public void ReceivesTargeterSpecsFromBuildVars() {
        //    throw new NotImplementedException();
        //}

        [TestMethod]
        public void ConfigReaderReadsConfig() 
        {
            var el = XElement.Parse(string.Format(@"
                                                <Cuckoo>
                                                    <Targeter FullName='{0}' Assembly='{1}' />
                                                    <Targeter FullName='{2}' Assembly='{3}' />
                                                </Cuckoo>",
                                                typeof(AttributeTargeter).FullName,
                                                typeof(AttributeTargeter).Assembly.FullName, 
                                                typeof(CascadeTargeter).FullName,
                                                typeof(CascadeTargeter).Assembly.FullName
                                                ));

            var targeterTypes = ConfigReader.Read(el)
                                              .ToArray();

            Assert.AreEqual(
                    typeof(AttributeTargeter).AssemblyQualifiedName, 
                    targeterTypes[0].AssemblyQualifiedName );

            Assert.AreEqual(
                    typeof(CascadeTargeter).AssemblyQualifiedName,
                    targeterTypes[1].AssemblyQualifiedName );
        }



        //[TestMethod]
        //public void TypeMonikerGenerationFromAssemblyQualifiedName() 
        //{
        //    var types = new[] { 
        //                    typeof(AttributeTargeter),
        //                    typeof(MonikerGenerator),
        //                    //typeof(int) 
        //                };


        //    var monikerGen = new MonikerGeneratorExt(null);

        //    var monikers = types.Select(t => monikerGen.Type(t.AssemblyQualifiedName));
            
        //    var typesAndMonikers = types.Zip(monikers,
        //                                (t, m) => new {
        //                                            Type = t,
        //                                            Moniker = m
        //                                        });

        //    foreach(var tm in typesAndMonikers) {
        //        Assert.AreEqual(tm.Type.Name, tm.Moniker.Name);
        //        Assert.AreEqual(tm.Type.FullName, tm.Moniker.FullName);
        //        Assert.AreEqual(tm.Type.AssemblyQualifiedName, tm.Moniker.AssemblyQualifiedName);
        //    }
        //}

                
    }
}
