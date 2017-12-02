using System;
using System.Collections.Generic;

namespace LudumDare40.FSM
{
    public abstract class State<T, E>
    {
        public FiniteStateMachine<T, E> fsm;
        public E entity;

        public abstract void begin();
        public abstract void handleInput();
        public abstract void update();
        public abstract void end();
    }

    public class FiniteStateMachine<T, E>
    {
        private Stack<State<T, E>> _stateStack;
        private E _entity;
        private State<T, E> _requestingState;
        private bool _requestingReset;

        public FiniteStateMachine(E entity, State<T, E> initialState)
        {
            _stateStack = new Stack<State<T, E>>();
            _entity = entity;

            setupState(initialState);
            _stateStack.Push(initialState);
        }

        public void update()
        {
            var currentState = _stateStack.Peek();
            currentState.handleInput();
            currentState.update();

            Console.WriteLine(currentState);

            if (_requestingState != null)
            {
                currentState.end();
                if (_requestingReset)
                {
                    _stateStack.Clear();
                }
                else
                {
                    _stateStack.Pop();
                }
                setupState(_requestingState);
                _stateStack.Push(_requestingState);
                _requestingState = null;
            }
        }

        public void setupState(State<T, E> state)
        {
            state.entity = _entity;
            state.fsm = this;
            state.begin();
        }

        public void pushState(State<T, E> state)
        {
            setupState(state);
            _stateStack.Push(state);
        }

        public void popState()
        {
            _stateStack.Pop();
        }

        public void changeState(State<T, E> state)
        {
            _requestingState = state;
        }

        public void resetStackTo(State<T, E> state)
        {
            _requestingReset = true;
            _requestingState = state;
        }
    }
}
