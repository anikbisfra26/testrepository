using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using NeoCortexApi.Utility;
using NeoCortex;
using Newtonsoft.Json;

using Newtonsoft.Json.Serialization;

namespace UnitTestsProject
{
    [TestClass]
    public class SpatialPoolerSerializeTests
    {
        //  int[] activeArray = new int[32 * 32];
        /*   int[] inputVector =  {
                                          1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                          0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                          1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                          0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,
                                          1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,
                                          1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                          0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                          1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                          0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,
                                          1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                          0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,
                                          1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,
                                          1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                          0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                          0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                          1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0 }; */

        #region Private Methods
        private static Parameters GetDefaultParams()
        {
            ThreadSafeRandom rnd = new ThreadSafeRandom(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 10);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 80.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.WRAP_AROUND, true);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10);
            parameters.Set(KEY.MAX_BOOST, 1.0);
            parameters.Set(KEY.RANDOM, rnd);
            parameters.Set(KEY.IS_BUMPUP_WEAKCOLUMNS_DISABLED, true);
            //int r = parameters.Get<int>(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA);

            return parameters;
        }
        #endregion

        [TestMethod]
        [TestCategory("LongRunning")]
        public void DeserializeTest()
        {
            string file = Path.Combine("TestFiles", "sp.test.serialized.json");

            var sp2 = SpatialPooler.Deserialize(file);
        }
        

        [TestMethod]
        [TestCategory("LongRunning")]
        public void SerializationDistalSegmentTest()
        {
            Dictionary<Cell, List<DistalDendrite>> distalSegments = new Dictionary<Cell, List<DistalDendrite>>();
            distalSegments.Add(new Cell(), new List<DistalDendrite>() { new DistalDendrite(new Cell(), 1, 1, 1, 1.1, 100) { } });

            var x = new  { DistalSegments = distalSegments };

            HtmSerializer ser = new HtmSerializer();
            ser.Serialize(x, "distalsegment.json");
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        public void SerializationTest1()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 1000 });
            parameters.setColumnDimensions(new int[] { 2048 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 2048);
            parameters.setGlobalInhibition(true);

            var sp1 = new SpatialPooler();

            var mem1 = new Connections();
            parameters.apply(mem1);

            sp1.init(mem1);

            var s4 = sp1.Serialize();

            string file = "spSerialized.json";

           

            File.WriteAllText(file, s4);

           // var settings = new JsonSerializerSettings { ContractResolver = new ContractResolver(), Formatting = Formatting.Indented };

        //    var sp2 = JsonConvert.DeserializeObject<SpatialPooler>(s4);





            /*   JsonTextReader reader = new JsonTextReader(new StringReader(s4));
               StringBuilder sb = new StringBuilder();

               while (reader.Read())
               {
                   if (reader.Value != null)
                   {
                       Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
                   }
                   else
                   {
                       Console.WriteLine("Token: {0}", reader.TokenType);

                   }
               }

               */


                   var sp2 = SpatialPooler.Deserialize(file);



            // TODO
            // Assert.IsTrue(Compare(sp, sp2))



            /*  var settings = new JsonSerializerSettings { ContractResolver = new ContractResolver(), Formatting = Formatting.Indented };

              var jsonMem = JsonConvert.SerializeObject(mem1, settings);
              //Response.Write(jsonData);

              var mem2 = JsonConvert.DeserializeObject<Connections>(jsonMem, settings);

              var jsonSp = JsonConvert.SerializeObject(sp, settings);

              var sp2 = JsonConvert.DeserializeObject<SpatialPooler>(jsonSp, settings);


            sp1.init(mem1);


       */
        }
        /*    [TestMethod]
            [TestCategory("LongRunning")]
            public void DeSerializationTest1()
            {
                SpatialPooler sp2 = new SpatialPooler();
                var mem = new Connections();

                sp2.init(mem);

                sp2.DesererializeConnection("serializeSP2.json");
            }
            */

        /// <summary>
        /// This test runs SpatialPooler 32x32 with input of 16x16. It learns the sequence to stable SDR representation
        /// in very few steps (2 steps). Test runs 10 iterations and keeps stable SDR encoded sequence.
        /// After 10 steps, current instance of learned SpatialPooler (SP1) is serialized to JSON and then
        /// deserialized to second instance SP2.
        /// Second instance SP2 continues learning of the same input. Expectation is that SP2 continues in stable state with same 
        /// set of active columns as SP1.
        /// </summary>
        [TestMethod]
        [TestCategory("LongRunning")]
        public void OutputPersistenceStability1()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 32 * 32 });
            parameters.setColumnDimensions(new int[] { 64 * 64 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 64 * 64);
            parameters.setMinPctOverlapDutyCycles(0.01);

            var mem = new Connections();
            parameters.apply(mem);

            var sp1 = new SpatialPooler();
            sp1.init(mem);


            int[] activeArray = new int[64 * 64];

            int[] inputVector = Helpers.GetRandomVector(32 * 32, parameters.Get<Random>(KEY.RANDOM));
            /*  int [] inputVector =  {
                                             1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                             0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                             1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                             0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,
                                             1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,
                                             1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                             0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                             1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                             0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,
                                             1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                             0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,
                                             1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,
                                             1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                             0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                             0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                             1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0 };

          */
            string str1 = String.Empty;

            for (int i = 0; i < 5; i++)
            {
                sp1.compute(inputVector, activeArray, true);

                var activeCols1 = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                str1 = Helpers.StringifyVector(activeCols1);

                Debug.WriteLine(str1);
            }
            var s5 = sp1.Serialize("SPserializecompare5.json");
            var s6 = JsonConvert.SerializeObject(sp1);
            Assert.IsTrue(s5.SequenceEqual(s6));
            //File.WriteAllText("SPserializecompare5.txt", s5);


            // var s4 = sp1.SerializeConnections();
            // File.WriteAllText("persistence5.json",s4);
            //   sp1.SerializeConnections("serializeSPPersistence5.json");

            /*
                        SpatialPooler sp2 = new SpatialPooler();
                           var mem2 = new Connections();

                            sp2.init(mem2);

                        //        sp2.DesererializeConnection("serializeSPPersistence4.json");

            //sp2 = JsonConvert.DeserializeObject<SpatialPooler>(s4);

                        for (int i = 5; i < 10; i++)
                        {
                            sp2.compute(inputVector, activeArray, false);

                            var activeCols2 = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                            var str2 = Helpers.StringifyVector(activeCols2);

                            Debug.WriteLine(str2);

                           // Assert.IsTrue(str1.SequenceEqual(str2));
                        }

                */



         }

     /*   class ContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Select(p => base.CreateProperty(p, memberSerialization))
                    .Union(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Select(f => base.CreateProperty(f, memberSerialization)))
                    .ToList();
                props.ForEach(p => { p.Writable = true; p.Readable = true; });
                return props;
            }
        }*/

    }


}

