using System;
using System.Timers;
using Stateless;
using Stateless.Graph;

namespace ResourceStateless
{
    internal class Processor
    {
        enum Trigger
        {
            Start,
            Done,
            PlannedWaitStart,
            PlannedWaitDone
        }

        enum State
        {
            Idle,
            Running,
            PlannedWait
        }

        State _state = State.Idle;
        
        StateMachine<State, Trigger> _machine;
        StateMachine<State, Trigger>.TriggerWithParameters<string, int> _startTrigger;

        string _resource;
        string _callee;

        private string _article;
        private int _timeout;
        Timer timeoutTimer = new Timer();
        
        public Processor(string resource)
        {
            _resource = resource;

            timeoutTimer.Elapsed += TimeoutTimer_Elapsed;
            timeoutTimer.Enabled = false;
            timeoutTimer.AutoReset = false;

            _machine = new StateMachine<State, Trigger>(() => _state, s => _state = s);

            _startTrigger = _machine.SetTriggerParameters<string, int>(Trigger.Start);


            _machine.Configure(State.Idle)
                    .Permit(Trigger.Start, State.Running)
                    .Permit(Trigger.PlannedWaitStart, State.PlannedWait);
                

            _machine.Configure(State.Running)
                .OnEntryFrom(_startTrigger, OnSetArticleAndTimeout)
                .OnEntry(t => StartCallTimer())
                .OnExit(t => StopCallTimer())                
                .Permit(Trigger.Done, State.Idle)
                .Permit(Trigger.PlannedWaitStart, State.PlannedWait);

            _machine.Configure(State.PlannedWait)
                .Permit(Trigger.Start, State.Running)
                .Permit(Trigger.Done, State.Idle)
                .Permit(Trigger.PlannedWaitDone, State.Idle);
        }

        void OnSetArticleAndTimeout(string article, int timeout)
        {
            _article = article;
            _timeout = timeout * 1000;
            timeoutTimer.Interval = _timeout;
            Console.WriteLine("{0:HH:mm:ss.fff} [Timer:] Timeout set to " + timeout + "!", DateTime.Now);
        }
        
        void TimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timeoutTimer.Stop();
            Console.WriteLine("{0:HH:mm:ss.fff} [Timer:] Timeout!", DateTime.Now);
        }

        void StartCallTimer()
        {
            timeoutTimer.Stop();
            timeoutTimer.Interval = _timeout;
            timeoutTimer.Start();
            //Console.WriteLine("[Timer:] Call started at {0}", DateTime.Now);
        }

        void StopCallTimer()
        {
            timeoutTimer.Stop();
            //Console.WriteLine("[Timer:] Call ended at {0}", DateTime.Now);
        }
        
        public void Print()
        {
            Console.WriteLine("{2:HH:mm:ss.fff} [{0}] placed call and [Status:] {1}", this._resource, this._state, DateTime.Now);
        }
        
        public void Start(string article, int timeout)
        {
            _machine.Fire(_startTrigger, article, timeout);
        }

        public void Done()
        {
            _machine.Fire(Trigger.Done);
        }

        public void PlannedWaitStart()
        {
            _machine.Fire(Trigger.PlannedWaitStart);
        }
        
        public void PlannedWaitDone()
        {
            _machine.Fire(Trigger.PlannedWaitDone);
        }

        public string ToDotGraph()
        {
            return UmlDotGraph.Format(_machine.GetInfo());
        }
    }
}