using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using EventAggregator.Fmaciasruano.Components;
using EventAggregatorAbstract.Fmaciasruano.Components;
using FifoTaskQueue.Fmaciasruano.Components;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using NLog;
using NUnit.Framework;
using EA = EventAggregator.Fmaciasruano.Components.EventAggregator;

namespace FifoTaskQueueNC5_0.Test.Fmaciasruano.Components
{
    [TestFixture()]
    public class FifoTaskQueueTests
    {
        private IEventAggregator eventAggregator;
        private FifoTaskQueueAbstract.Fmaciasruano.Components.ITaskQueue fifoTaskQueue;

        [SetUp]
        public void ResetEventsOutput()
        {
            eventAggregator = EA.Create(
                ProcessEventFactory.Instance,
                ProcessEventSubscriptorFactory.Instance,
                UIEventSubscriptorFactory.Instance);

            fifoTaskQueue = FifoTaskQueue.Fmaciasruano.Components.FifoTaskQueue.Create(
                TaskScheduler.Current,
                TasksProvider.Create(LogManager.GetCurrentClassLogger()),
                LogManager.GetCurrentClassLogger(),
                eventAggregator
            );
        }

        [Test()]
        public void CreateTest()
        {
            Assert.IsInstanceOf<FifoTaskQueueAbstract.Fmaciasruano.Components.ITaskQueue>(FifoTaskQueue.Fmaciasruano.Components.FifoTaskQueue.Create(
                    TaskScheduler.Current,
                    TasksProvider.Create(LogManager.GetCurrentClassLogger()),
                    LogManager.GetCurrentClassLogger(),
                    eventAggregator
                ));
        }

        [Test()]
        public void EnqueueActionNoArgumentsTest()
        {
            bool done1 = false;
            bool done2 = false;
            IActionObserver<Action> observer1 = fifoTaskQueue.Enqueue<Action>(() =>
            {
                done1 = true;
                
            });

            IActionObserver<Action> observer2 = fifoTaskQueue.Enqueue<Action>(() =>
            {
                done2 = true;
            
            });
            Assert.IsFalse(object.ReferenceEquals(observer1.Runner, observer2.Runner));
            observer1.Runner.Run();
            observer2.Runner.Run();
            observer1.Unsubscribe();
            observer2.Unsubscribe();
            Assert.IsTrue(done1);
            Assert.IsTrue(done2);
        }

        [Test()]
        public void EnqueueActionArgumentsTest()
        {
            IActionObserver<Action<int>> observer1 = fifoTaskQueue.Enqueue<Action<int>,int>((args) =>
            {
                Assert.AreEqual(1,args);
            },1);

            Assert.IsInstanceOf<IJobRunner>(observer1.Runner);

            int[] arrayParams = {1, 2};

            IActionObserver<Action<int[]>> observer2 = fifoTaskQueue.Enqueue<Action<int[]>, int[]>((args) =>
            {
                Assert.AreEqual(1, args[0]);
                Assert.AreEqual(2, args[1]);
            }, arrayParams);
            Assert.IsFalse(object.ReferenceEquals(observer1.Runner, observer2.Runner));
            Assert.IsInstanceOf<IJobRunner>(observer1.Runner);
            Assert.IsInstanceOf<IJobRunner>(observer2.Runner);
            observer1.Runner.Run();
            observer1.Unsubscribe();
            observer2.Runner.Run();
            observer2.Unsubscribe();
            Assert.AreEqual(0, fifoTaskQueue.Provider.Subscriptions.Length);
        }
        [Test()]
        public async Task ComleteSyncActionAndCallbacksAreSequentiallyInvoked()
        {
            bool firstActionDone = false;
            bool firstCallback = false;
            bool secondActionDone = false;
            bool secondCallback = false;

            await fifoTaskQueue.Complete(fifoTaskQueue.Enqueue<Action<int[]>, int[]>((args) =>
                {
                    firstActionDone = true;
                    Assert.IsTrue(firstActionDone);
                    Assert.IsFalse(firstCallback);
                    Assert.IsFalse(secondActionDone);
                    Assert.IsFalse(secondCallback);
                }, new int[] { 1, 2 })
                .OnCompleteCallback((object sender) =>
                {
                    firstCallback = true;
                    Assert.IsTrue(firstActionDone);
                    Assert.IsTrue(firstCallback);
                    Assert.IsFalse(secondActionDone);
                    Assert.IsFalse(secondCallback);
                    Assert.AreEqual(ObserverStatus.Completed, ((IObserver)sender).Status);
                }), fifoTaskQueue.Enqueue<Action>(() =>
                {
                    secondActionDone = true;
                    Assert.IsTrue(firstActionDone);
                    Assert.IsTrue(firstCallback);
                    Assert.IsTrue(secondActionDone);
                    Assert.IsFalse(secondCallback);
                }).OnCompleteCallback((object sender) =>
                {
                    secondCallback = true;
                    Assert.IsTrue(firstActionDone);
                    Assert.IsTrue(firstCallback);
                    Assert.IsTrue(secondActionDone);
                    Assert.IsTrue(secondCallback);
                    Assert.AreEqual(ObserverStatus.Completed, ((IObserver)sender).Status);
                })
            );
            Assert.AreEqual(0, fifoTaskQueue.Provider.Subscriptions.Length);
            Assert.IsTrue(firstActionDone);
            Assert.IsTrue(firstCallback);
            Assert.IsTrue(secondActionDone);
            Assert.IsTrue(secondCallback);
            Assert.IsTrue(fifoTaskQueue.Provider.Subscriptions.Length == 0);
        }


        [Test()]
        public async Task ComleteAsyncActionAndCallbacksAreSequentiallyInvoked()
        {
            bool firstActionDone = false;
            bool firstCallback = false;
            bool secondActionDone = false;
            bool secondCallback = false;

            await fifoTaskQueue.Complete(fifoTaskQueue.Enqueue<Action<int[]>, int[]>(async (args) =>
            {
                await Task.Delay(100);
                firstActionDone = true;
                Assert.IsTrue(firstActionDone);
                Assert.IsFalse(firstCallback);
                Assert.IsFalse(secondActionDone);
                Assert.IsFalse(secondCallback);
            }, new int[] { 1, 2 })
                .OnCompleteCallback((object sender) =>
                {
                    firstCallback = true;
                    Assert.IsTrue(firstActionDone);
                    Assert.IsTrue(firstCallback);
                    Assert.IsFalse(secondActionDone);
                    Assert.IsFalse(secondCallback);
                    Assert.AreEqual(ObserverStatus.Completed, ((IObserver)sender).Status);
                }), fifoTaskQueue.Enqueue<Action>(async () =>
                {
                    await Task.Delay(1000);
                    secondActionDone = true;
                    Assert.IsTrue(firstActionDone);
                    Assert.IsTrue(firstCallback);
                    Assert.IsTrue(secondActionDone);
                    Assert.IsFalse(secondCallback);
                }).OnCompleteCallback((object sender) =>
                {
                    secondCallback = true;
                    Assert.IsTrue(firstActionDone);
                    Assert.IsTrue(firstCallback);
                    Assert.IsTrue(secondActionDone);
                    Assert.IsTrue(secondCallback);
                    Assert.AreEqual(ObserverStatus.Completed, ((IObserver)sender).Status);
                })
            );
            Assert.AreEqual(0, fifoTaskQueue.Provider.Subscriptions.Length);
            Assert.IsTrue(firstActionDone);
            Assert.IsTrue(firstCallback);
            Assert.IsTrue(secondActionDone);
            Assert.IsTrue(secondCallback);
            Assert.IsTrue(fifoTaskQueue.Provider.Subscriptions.Length == 0);
        }

        [Test()]
        public async Task CancelAfterTest()
        {
            bool firstActionDone = false;
            bool secondActionDone = false;
            CancellationToken token = fifoTaskQueue.CancellationToken;

            IActionObserver<Action<CancellationToken>> action1 = fifoTaskQueue
                .Enqueue<Action<CancellationToken>,CancellationToken>(obj =>
                {
                    var _ = Task.Run<Task>(async () =>
                    {
                        await Task.Delay(100,obj);
                        firstActionDone = true;
                        Assert.IsTrue(firstActionDone);
                        Assert.IsFalse(secondActionDone);
                    },obj).Unwrap();
                    

                }, token).OnCompleteCallback((object sender) => 
                {
                        Assert.AreEqual(ObserverStatus.Completed, ((IObserver)sender).Status);
                });

            IActionObserver<Action> action2 = fifoTaskQueue.Enqueue<Action>(() =>
            {
                Task.Run<Task>(async () =>
                    {
                        await Task.Delay(100);
                        firstActionDone = true;
                        Assert.IsTrue(firstActionDone);
                        Assert.IsFalse(secondActionDone);
                    }).Wait();

            }).OnCompleteCallback((object sender) =>
            {
                Assert.AreEqual(ObserverStatus.Completed, ((IObserver)sender).Status);
            });

            await fifoTaskQueue.Complete();
        }


        
        [Test()]
        public void CancelExecutionTest()
        {
            Assert.Fail();
        }
        [Test()]
        public void xxx()
        {
            int[] numbers1 = new int[] { 1, 2, 3, 4, 5 };
            int[] numbers2 = new int[] { 15,25,35 };
            int[] numbers3 = new int[] { 8,8 };
            Console.WriteLine(numbers1);
            Console.WriteLine(numbers2);
            Console.WriteLine(numbers3);
        }
        private static int find_total(int[] my_numbers)
        {
            //Insert your code here
            int score = 0;
            for (var x = 0; x <= my_numbers.Length; x++)
            {
                int currentNumber = my_numbers[x];
                if (my_numbers[x] == 8)
                {
                    score += 8;
                    continue;
                }
                if (currentNumber % 2 == 0)
                    score += 1;
                else
                    score += 3;
            }
            return score;
        }
        [Test()]
        public void time()
        {
            DateTime dt = DateTime.Now;
            DateTime dt_1hour = dt.AddHours(1);
            DateTime dt_1day = dt.AddDays(1); 
            DateTime dt_7days = dt.AddDays(7);

            this.Format(DateTime.Now, DateTime.Now);
            this.Format(DateTime.Now.AddMinutes(-1), DateTime.Now);
            this.Format(DateTime.Now.AddMinutes(-5), DateTime.Now);
            
            this.Format(DateTime.Now.AddHours(-1), DateTime.Now);

            this.Format(DateTime.Now.AddHours(-13), DateTime.Now);
            this.Format(DateTime.Now.AddDays(-1), DateTime.Now);
            this.Format(DateTime.Now.AddDays(-4), DateTime.Now);
            this.Format(DateTime.Now.AddDays(-7), DateTime.Now);




        }
        public void Format(DateTime date, DateTime current)
        {
            TimeSpan diff = current - date;

            if (diff.TotalDays >= 7)
                Console.WriteLine(string.Format(current.ToString()));

            else if (diff.TotalDays >= 1)
                Console.WriteLine(string.Format("{0} day(s) ago", diff.Days));

            else if (diff.TotalHours >= 1 && diff.TotalDays <= 1)
                Console.WriteLine(string.Format("{0} hour(s) ago", diff.Hours));

            else if (diff.totalSe.TotalMinutes < 1 && diff.TotalDays == 0)
                Console.WriteLine("now");

            else if (diff.TotalMinutes < 60)
                Console.WriteLine(string.Format("{0} minute(s) ago", diff.TotalMinutes));
            
            

        }
    }
}