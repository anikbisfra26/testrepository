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
    /// <summary>
    /// This file contains multiple Unit Test that implements and demonstrates newly integrated Serialization Functionality of Spatial Pooler
    /// </summary>
    public class SpatialPoolerSerializeTests
    {
        //Below Inputs can be used Globally for all the test cases
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




        //Setting up Default Parameters for the Spatial Pooler Test cases
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


            return parameters;
        }


        /// <summary>
        /// This test runs Spatial Pooler without trained data and with Certain Input parameters.It Serializes the instance of Spatial Pooler in a JSON file.
        /// Further scopes about Deserialization test and Serialized and Deserialized Data comparison function is written towards the end part of this test but commented out for now since Deserialization method is not complete.
        /// Serialized and Deserialized value comparison can be done once the Deserialization function is fully implemented.
        /// </summary>

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
        

     //        var sp2 = SpatialPooler.Deserialize(file);

            // Further scope of the test
            /* Deserialization Approach 1

          var settings = new JsonSerializerSettings {  TypeNameHandling = TypeNameHandling.Auto };

              var sp2 = JsonConvert.DeserializeObject<SpatialPooler>(s4, settings);


            /*  
             *  
             *  Deserialization Approach 2
             * 
                  var sp2 = SpatialPooler.Deserialize(file);
                   var sp3 = sp2.Serialize();

                   Assert.IsTrue(s4.SequenceEqual(sp3)); // Comparison of Spatial Pooler beofre serialization and after Deserialization


            */
        }


        /// <summary>
        /// This test runs SpatialPooler 64x64 with input of 32x32 . It learns the sequence to stable SDR representation.
        /// in very few steps (2 steps). Test runs 5 iterations and keeps stable SDR encoded sequence.
        /// Further scope of the test(once Deserialization is implemented).
        /// After 5 steps, current instance of learned SpatialPooler (SP1) is serialized to JSON and then
        /// deserialized to second instance SP2.
        /// Second instance SP2 continues learning of the same input. Expectation is that SP2 continues in stable state with same 
        /// set of active columns as SP1.
        /// </summary>
        [TestMethod]
        [TestCategory("LongRunning")]
        public void SerializationTestWithTrainedData()
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
            var s5 = sp1.Serialize();
            string file = "spSerializeTrain.json";
            File.WriteAllText(file, s5);


            /*  Further scope of Deseriazation testing and value comparison with serialized data
              * 
             var sp2 = SpatialPooler.Deserialize(file);

              for (int i = 5; i < 10; i++)
              {
              sp2.compute(inputVector, activeArray, false);

               var activeCols2 = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

               var str2 = Helpers.StringifyVector(activeCols2);

               Debug.WriteLine(str2);
               Assert.IsTrue(str1.SequenceEqual(str2));

             }

                 */



        }
        //Serialization and binding Distal Segments

        [TestMethod]
        [TestCategory("LongRunning")]
        public void SerializationDistalSegmentTest()
        {
            Dictionary<Cell, List<DistalDendrite>> distalSegments = new Dictionary<Cell, List<DistalDendrite>>();
            distalSegments.Add(new Cell(), new List<DistalDendrite>() { new DistalDendrite(new Cell(), 1, 1, 1, 1.1, 100) { } });

            var x = new { DistalSegments = distalSegments };

            HtmSerializer ser = new HtmSerializer();
            ser.Serialize(x, "distalsegment.json");
        }
        /*

            [TestMethod]
            [TestCategory("LongRunning")]
            public void DeserializeTest()
            {
                string file = Path.Combine("TestFiles", "sp.test.serialized.json");

                var sp2 = SpatialPooler.Deserialize(file);
            }
            */

    }





}

