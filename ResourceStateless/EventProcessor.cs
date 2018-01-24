using System;
using System.Timers;
using Stateless;
using Stateless.Graph;

namespace ResourceStateless
{
    public interface IEvent
    {
        int TypeEvent { get; }
    }

    public interface IStartEvent : IEvent
    {
        string Article { get; }
        int Timeout { get; }
    }

    public class StartEvent : IStartEvent
    {
        public int TypeEvent { get { return 1; } }
        private readonly string _article;
        public string Article { get { return _article; } }
        private readonly int _timeout;
        public int Timeout { get { return _timeout; } }

        public StartEvent() { }

        public StartEvent(string article, int timeout)
        {
            _article = article;
            _timeout = timeout;
        }
    }

    public interface IDoneEvent : IEvent
    {
        int Pieces { get; }
    }

    public class DoneEvent : IDoneEvent
    {
        public int TypeEvent { get { return 2; } }
        private readonly int _pieces;
        public int Pieces { get { return _pieces; } }

        public DoneEvent() { }

        public DoneEvent(int pieces)
        {
            _pieces = pieces;
        }
    }

    public interface IPlannedWaitStartEvent : IEvent { }

    public class PlannedWaitStartEvent : IPlannedWaitStartEvent
    {
        public int TypeEvent { get { return 3; } }

        public PlannedWaitStartEvent() { }
    }

    public interface IPlannedWaitDoneEvent : IEvent { }

    public class PlannedWaitDoneEvent : IPlannedWaitDoneEvent
    {
        public int TypeEvent { get { return 4; } }

        public PlannedWaitDoneEvent() { }
    }

    internal class EventProcessor
    {
        //enum Trigger
        //{
        //    Start,
        //    Done,
        //    PlannedWaitStart,
        //    PlannedWaitDone
        //}

        enum State
        {
            Idle,
            Running,
            PlannedWait
        }

        State _state = State.Idle;
        
        StateMachine<State, IEvent> _machine;
        StateMachine<State, IEvent>.TriggerWithParameters<string, int> _setArticleAndTimeoutTrigger;

        string _resource;
        string _callee;

        private string _article;
        private int _timeout;
        Timer timeoutTimer = new Timer();
        
        public EventProcessor(string resource)
        {
            _resource = resource;

            timeoutTimer.Elapsed += TimeoutTimer_Elapsed;
            timeoutTimer.Enabled = false;
            timeoutTimer.AutoReset = false;

            _machine = new StateMachine<State, IEvent>(() => _state, s => _state = s);

            _setArticleAndTimeoutTrigger = _machine.SetTriggerParameters<string, int>(new StartEvent());


            _machine.Configure(State.Idle)
                    .Permit(new StartEvent(), State.Running)
                    .Permit(new PlannedWaitStartEvent(), State.PlannedWait);
                

            _machine.Configure(State.Running)
                .OnEntryFrom(_setArticleAndTimeoutTrigger, OnSetArticleAndTimeout)
                .OnEntry(t => StartCallTimer())
                .OnExit(t => StopCallTimer())                
                .Permit(new DoneEvent(), State.Idle)
                .Permit(new PlannedWaitStartEvent(), State.PlannedWait);

            _machine.Configure(State.PlannedWait)
                .Permit(new StartEvent(), State.Running)
                .Permit(new DoneEvent(), State.Idle)
                .Permit(new PlannedWaitDoneEvent(), State.Idle);
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
            //_machine.Fire(_setArticleAndTimeoutTrigger, article, timeout);
            _machine.Fire(new StartEvent(article, timeout));
        }

        public void Done()
        {
            _machine.Fire(new DoneEvent());
        }

        public void PlannedWaitStart()
        {
            _machine.Fire(new PlannedWaitStartEvent());
        }
        
        public void PlannedWaitDone()
        {
            _machine.Fire(new PlannedWaitDoneEvent());
        }

        public string ToDotGraph()
        {
            return UmlDotGraph.Format(_machine.GetInfo());
        }
    }
}