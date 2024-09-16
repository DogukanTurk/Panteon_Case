using System.Collections.Generic;

namespace DOT.Utilities
{
    /// <summary>
    /// The purpose of this class is to manage IObserver classes which don't inherit from MonoBehaviour
    /// </summary>
    public class Observer : IObserver
    {
        /* ------------------------------------------ */

        /// <summary>
        /// Every subject needs to identify with their class name
        /// </summary>
        public string SenderName;

        /* ------------------------------------------ */

        /// <summary>
        /// The list responsible to hold all the current observers.
        /// </summary>
        private List<IObserver> _observers = new();

        /* ------------------------------------------ */

        /// <summary>
        /// Adding an observer to the current observers list
        /// </summary>
        /// <param name="observer"></param>
        public void AddObserver(IObserver observer)
        {
            _observers.Add(observer);
        }

        /// <summary>
        /// Removing an observer from the current observers list
        /// </summary>
        /// <param name="observer"></param>
        public void RemoveObserver(IObserver observer)
        {
            if (_observers.Contains(observer))
                _observers.Remove(observer);
        }

        /* ------------------------------------------ */

        /// <summary>
        /// Nofity the current observers with a simple message
        /// </summary>
        /// <param name="message"></param>
        public virtual void NotifyObservers(Msg message)
        {
            message.Sender = SenderName;

            for (int x = 0; x < _observers.Count; x++)
                _observers[x].OnNotify(message);
        }

        /// <summary>
        /// Notify the current observers with a dynamic message
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        public virtual void NotifyObservers<T>(Msg<T> message)
        {
            message.Sender = SenderName;

            for (int x = 0; x < _observers.Count; x++)
                _observers[x].OnNotify(message);
        }

        /// <summary>
        /// Get notice when a subject does something with a simple message
        /// </summary>
        /// <param name="message"></param>
        public virtual void OnNotify(Msg message)
        {
        }

        /// <summary>
        /// Get noticed when a subject does something with a dynamic message
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        public virtual void OnNotify<T>(Msg<T> message)
        {
        }

        /* ------------------------------------------ */

        /// <summary>
        /// A simple message used in broadcasting messages to observers
        /// </summary>
        public struct Msg
        {
            public string Sender;

            public int Type;
        }

        /// <summary>
        /// A dynamic complex message used in broadcasting messages to observers
        /// </summary>
        public struct Msg<T>
        {
            public string Sender;

            public int Type;

            /// <summary>
            /// Dynamic message responsible to hold complex data
            /// </summary>
            public T Message;
        }

        #region Data_Implementation

        // We are declaring these structs in order to make the dev able to manage their memory usage more carefully.
        // If s/he doesn't need any complex data, s/he can use simple messages with one parameter or
        // s/he can use Arrays to broadcast more data, if s/he needs.

        public struct Msg_Data_Array<_T1>
        {
            public _T1[] T1;
        }

        public struct Msg_Data_Array<_T1, _T2>
        {
            public _T1[] T1;
            public _T2[] T2;
        }

        public struct Msg_Data_Array<_T1, _T2, _T3>
        {
            public _T1[] T1;
            public _T2[] T2;
            public _T3[] T3;
        }

        public struct Msg_Data_Array<_T1, _T2, _T3, _T4>
        {
            public _T1[] T1;
            public _T2[] T2;
            public _T3[] T3;
            public _T4[] T4;
        }

        public struct Msg_Data<_T1>
        {
            public _T1 T1;
        }

        public struct Msg_Data<_T1, _T2>
        {
            public _T1 T1;
            public _T2 T2;
        }

        public struct Msg_Data<_T1, _T2, _T3>
        {
            public _T1 T1;
            public _T2 T2;
            public _T3 T3;
        }

        public struct Msg_Data<_T1, _T2, _T3, _T4>
        {
            public _T1 T1;
            public _T2 T2;
            public _T3 T3;
            public _T4 T4;
        }

        #endregion

        /* ------------------------------------------ */
    }

    public interface IObserver
    {
        /* ------------------------------------------ */

        /// <summary>
        /// Adding an observer to the current observers list
        /// </summary>
        /// <param name="observer"></param>
        public virtual void AddObserver(IObserver observer)
        {
        }

        /// <summary>
        /// Removing an observer from the current observers list
        /// </summary>
        /// <param name="observer"></param>
        public virtual void RemoveObserver(IObserver observer)
        {
        }

        /* ------------------------------------------ */

        /// <summary>
        /// Notify the current observers with a dynamic message
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        public virtual void NotifyObservers(Observer.Msg message)
        {
        }

        /// <summary>
        /// Nofity the current observers with a simple message
        /// </summary>
        /// <param name="message"></param>
        public virtual void NotifyObservers<T>(Observer.Msg<T> message)
        {
        }

        /// <summary>
        /// Get notice when a subject does something with a simple message
        /// </summary>
        /// <param name="message"></param>
        public virtual void OnNotify(Observer.Msg message)
        {
        }

        /// <summary>
        /// Get noticed when a subject does something with a dynamic message
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        public virtual void OnNotify<T>(Observer.Msg<T> message)
        {
        }

        /* ------------------------------------------ */
    }
}