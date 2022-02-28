using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UFXCase.FSM {
    public class FSM {
        private List<IState> m_list; //状态机维护的一组状态
        public IState CurrentState{get; private set; } //当前状态
        public FSM()
        {
            m_list = new List<IState>();
        }
        public void AddState(IState _state)// 添加指定的状态
        {
            IState _tmpState = GetState(_state.GetState());
            if (_tmpState == null)
            {
                m_list.Add(_state);
            }
            else
            {
                Debug.LogWarningFormat("FSMSystem(容错)：该状态【{0}】已经被添加！", _state.GetState().ToString());
            }
        }
        public void RemoveState(IState _state) //删除状态
        {
            IState _tmpState = GetState(_state.GetState());
            if (_tmpState != null)
            {
                m_list.Remove(_tmpState);
            }
            else
            {
                Debug.LogWarningFormat("FSMSystem(容错)：该状态【{0}】已经被移除！", _state.GetState().ToString());
            }
        }
        public IState GetState(string state)//获取相应状态
        {
            foreach (IState _state in m_list)    //遍历List里面所有状态取出相应的
            {
                if (_state.GetState() == state)
                {
                    return _state;
                }
            }
            return null;
        }
        /// <summary>
        /// 状态机状态翻转
        /// </summary>
        /// <param name="state">指定状态机</param>
        /// <returns>执行结果</returns>
        public void ChangeState(string state) 
        {
            IState _tmpState = GetState(state);       //要改变的状态不存在
            if (_tmpState == null)
            {
                Debug.LogWarningFormat("FSMSystem(容错)：该状态【{0}】不存在于状态机中！", state);
            }
            if (CurrentState != null) //当前状态不为空
            {
                CurrentState.OnExit();
            }
            CurrentState = _tmpState; //缓存为当前状态
            CurrentState.OnEnter();   //触发当前状态的OnEnter
        }
        public void Update()// 更新状态机状态
        {
            if (CurrentState != null)
            {
                CurrentState.OnUpdate();
            }
        }
        public void RemoveAllState() //移除所有状态
        {
            if (CurrentState != null)
            {
                CurrentState.OnExit();
                CurrentState = null;
            }
            m_list.Clear();
        }
    }
}
