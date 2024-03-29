﻿
using AkkaSb.Net;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.DistributedCompute;
using NeoCortexApi.DistributedComputeLib;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestsProject
{
    [TestClass]
    public class ActorSbTblStoragePersistenceTests
    {
        private string storageConnStr = "DefaultEndpointsProtocol=https;AccountName=azfunctionsamples;AccountKey=NEjFcvFNL/G7Ugq9RSW59+PonNgql/yLq8qfaVZPhanV9aJUnQi2b6Oy3csvPZPGVJreD+RgVUJJFFTZdUBhAA==;EndpointSuffix=core.windows.net";

        private List<object> list = new List<object>();

        public class CounterActor : ActorBase
        {
            /// <summary>
            /// Not serialized.
            /// </summary>
            private long privProp { get; set; } = 1;

            /// <summary>
            /// Not serialized.
            /// </summary>
            private long privField { get; set; } = 1;

            public long PubField { get; set; } = 1;

            public long Counter { get; set; }

            public CounterActor(ActorId id) : base(id)
            {
                Receive<long>((long num) =>
                {
                    Counter += num;
                    this.Perist().Wait();
                    return Counter;
                });
            }
        }

        public class ColumnActor : ActorBase
        {
            public Column Col { get; set; }

            public ColumnActor(ActorId id) : base(id)
            {
                Col = new Column(30, 1, 0.5, 32);
            }
        }

        [TestMethod]
        public void SerializeActorTest()
        {
            CounterActor act1 = new CounterActor(new ActorId(1));
            act1.Counter = 42;

            var json = TableStoragePersistenceProvider.SerializeActor(act1);

            CounterActor act2 = TableStoragePersistenceProvider.DeserializeActor<CounterActor>(json);

            Assert.IsTrue(act2.Counter == act1.Counter);
        }



        [TestMethod]
        public void SerializeColumnActorTest()
        {
            ColumnActor col1 = new ColumnActor(1);

            var json = TableStoragePersistenceProvider.SerializeActor(col1);

            ColumnActor col2 = TableStoragePersistenceProvider.DeserializeActor<ColumnActor>(json);

            Assert.IsTrue(col2.Col.Index == col1.Col.Index);
        }

        /// <summary>
        /// How to execute this test?
        /// 1. Run RunStatePersistenceTest1
        /// 2. Copy 'instanceName' of the test to clipboard.
        /// 3. Comment out RunStatePersistenceTest1 and comment in RunStatePersistenceTest2.
        /// 4. Set  instanceName to value in clopboard.
        /// </summary>
        [TestMethod]
        [TestCategory("SbActorTests")]
        public void TblStatePersistenceTest()
        {
            string instanceName = "instance1603690222";
            // Runs actor counter and persis its state after every increment.
            //RunStatePersistenceTest1(instanceName);

            // Runs actor counter by loading its state.
            RunStatePersistenceTest2(instanceName);
        }

        private void RunStatePersistenceTest1(string instanceName)
        {
            Debug.WriteLine($"Start of {nameof(RunStatePersistenceTest1)}");

            TableStoragePersistenceProvider prov = new TableStoragePersistenceProvider();
            prov.InitializeAsync(instanceName, new Dictionary<string, object>() { { "StorageConnectionString", storageConnStr } }, purgeOnStart: false).Wait();

            var cfg = SbAkkaTest.GetLocaSysConfig();
            ActorSystem sysLocal = new ActorSystem($"{nameof(TblStatePersistenceTest)}/local", cfg);
            ActorSystem sysRemote = new ActorSystem($"{nameof(TblStatePersistenceTest)}/remote", SbAkkaTest.GetRemoteSysConfig(), persistenceProvider: prov);

            CancellationTokenSource src = new CancellationTokenSource();

            var task = Task.Run(() =>
            {
                sysRemote.Start(src.Token);
            });

            ActorReference actorRef1 = sysLocal.CreateActor<CounterActor>(1);

            var response = actorRef1.Ask<long>((long)42).Result;

            Assert.IsTrue(response == 42);

            response = actorRef1.Ask<long>((long)7).Result;

            Assert.IsTrue(response == 49);

            src.Cancel();

            Debug.WriteLine($"End of {nameof(RunStatePersistenceTest1)}");
        }

        private void RunStatePersistenceTest2(string instanceName)
        {
            Debug.WriteLine($"Start of {nameof(RunStatePersistenceTest2)}");

            TableStoragePersistenceProvider prov = new TableStoragePersistenceProvider();
            prov.InitializeAsync(instanceName, new Dictionary<string, object>() { { "StorageConnectionString", storageConnStr } }, purgeOnStart: false).Wait();

            var cfg = SbAkkaTest.GetLocaSysConfig();
            ActorSystem sysLocal = new ActorSystem($"{nameof(TblStatePersistenceTest)}/local", cfg);
            ActorSystem sysRemote = new ActorSystem($"{nameof(TblStatePersistenceTest)}/remote", SbAkkaTest.GetRemoteSysConfig(), persistenceProvider: prov);

            CancellationTokenSource src = new CancellationTokenSource();

            var task = Task.Run(() =>
            {
                sysRemote.Start(src.Token);

                prov.Purge().Wait();
            });

            ActorReference actorRef1 = sysLocal.CreateActor<CounterActor>(1);

            var response = actorRef1.Ask<long>((long)1).Result;

            Assert.IsTrue(response == 50);

            src.Cancel();

            Debug.WriteLine($"End of {nameof(RunStatePersistenceTest2)}");
        }
    }
}




