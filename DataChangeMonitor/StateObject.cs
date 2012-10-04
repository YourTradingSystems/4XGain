using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataChangeMonitor
{
    public enum states{OT, I, U, D, IT1, IT2, UT1, UT2, DT1, DT2, IT1F, IT1P, IT2P, IT2F, DT1F, DT1P, DT2F, DT2P};
    public delegate void process(Object o);

    class StateObject
    {
        private HashSet<states> disabledStates = new HashSet<states>(){states.IT1P, states.IT2P, states.UT2, states.DT2P};
        private int _callLevel;
        private bool _isOperation;
        private DataHistoryRow _data;
        private states _currentState;
        private process commands;

        public int CallLevel { get { return _callLevel; } }
        public bool IsOperation{ get{ return _isOperation;} }
        public states CurrentState { get { return _currentState; } }
        public bool StateEnabled { get { return !disabledStates.Contains(_currentState); } }

        public process GetOperation(){
            return commands;
        }

        public DataHistoryRow GetAnalizedRow(){
            return _data;
        }

        public StateObject(DataHistoryRow passeddata)
        {
            _callLevel = 0;
            _isOperation = false;
            _data = passeddata;
            _currentState = states.OT;
        }

        public void Next(){
            String parsedData = "";
            switch (_currentState){
                case states.OT:
                    switch (_data.OperationType){
                        case 'I': _currentState = states.I; break;
                        case 'U': _currentState = states.U; break;
                        case 'D': _currentState = states.D; break;
                    }
                    break;
        //Level 1 -----------------------------------------------------------------------
                case states.I:
                    switch (_data.TableName.ToLower()){
                        case "accountidtosystemid" : _currentState = states.IT1; break;
                        case "userbrokers": _currentState = states.IT2; break;
                    }
                    break;
                case states.D:
                    switch (_data.TableName.ToLower()){
                        case "accountidtosystemid" : _currentState = states.DT1; break;
                        case "userbrokers": _currentState = states.DT2; break;
                    }
                    break;
                case states.U:
                    switch (_data.TableName.ToLower()){
                        case "accountidtosystemid" : _currentState = states.UT1; break;
                        case "userbrokers": _currentState = states.UT2; break;
                    }
                    break;
        //Level 2 -----------------------------------------------------------------------
                case states.IT1:
                    //Split row old version
                    parsedData = _data.RowOldVersion.Split(';')[0].Split('=')[1];
                    switch(IsFollower(parsedData)){
                        case true: _currentState = states.IT1F; break;
                        case false : _currentState = states.IT1P; break;
                    }
                    break;
                case states.IT2:
                    parsedData = _data.RowOldVersion.Split(';')[1].Split('=')[1];
                    switch(IsFollower(parsedData)){
                        case true: _currentState = states.IT2F; break;
                        case false : _currentState = states.IT2P; break;
                    }
                    break;
                case states.UT2:     // <------------ operation!!!
                    //Update fileds
                    _isOperation = true;
                    commands = new process((o) =>
                    {
                        Console.WriteLine("Update fileds");
                    }
                    );
                    break;
                case states.DT1 :
                    parsedData = _data.RowOldVersion.Split(';')[0].Split('=')[1];
                    switch (IsFollower(parsedData)){
                        case true: _currentState = states.DT1F; break;
                        case false: _currentState = states.DT1P; break;
                    }
                    break;
                case states.DT2:
                    parsedData = _data.RowOldVersion.Split(';')[1].Split('=')[1];
                    switch (IsFollower(parsedData)){
                        case true: _currentState = states.DT2F; break;
                        case false: _currentState = states.DT2P; break;
                    }
                    break;
        // Level 3 -----------------------------------------------------------------------------
                case states.IT1F :
                    _isOperation = true;
                    commands = new process((o) =>
                        {
                            Console.WriteLine("Sybscribe!!!");
                        }
                    );
                    break;
                case states.IT2F:
                    _isOperation = true;
                    commands = new process((o) =>
                        {
                            Console.WriteLine("Create follower object");
                        }
                    );
                    break;
                case states.DT1F :
                    _isOperation = true;
                    commands = new process((o) =>
                        {
                            Console.WriteLine("Unsubscribe follower");
                        }
                    );
                    break;
                case states.DT1P:
                    _isOperation = true;
                    commands = new process((o) =>
                        {
                            Console.WriteLine("Unsubscribe all");
                        }
                    );
                    break;
                case states.DT2F:
                    _isOperation = true;
                    commands = new process((o) =>
                        {
                            Console.WriteLine("Dispose follower obj");
                        }
                    );
                    break;    
            }
        }

        public Boolean IsFollower(String accID){
            return false;
        }
    }
}
