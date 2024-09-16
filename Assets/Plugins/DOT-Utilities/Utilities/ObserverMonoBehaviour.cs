using System.Collections.Generic;
using UnityEngine;

namespace DOT.Utilities
{
    /// <summary>
    /// The purpose of this class is to manage IObserver classes which inherit from MonoBehaviour
    /// </summary>
    public class ObserverMonoBehaviour : MonoBehaviour, IObserver
    {
        /* ------------------------------------------ */

        /// <summary>
        /// Every subject needs to identify with their class name
        /// </summary>
        [HideInInspector] 
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
        public virtual void AddObserver(IObserver observer)
        {
            _observers.Add(observer);
        }

        /// <summary>
        /// Removing an observer from the current observers list
        /// </summary>
        /// <param name="observer"></param>
        public virtual void RemoveObserver(IObserver observer)
        {
            if (_observers.Contains(observer))
                _observers.Remove(observer);
        }

        /* ------------------------------------------ */
        
        /// <summary>
        /// Notify the current observers with a dynamic message
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        public virtual void NotifyObservers<T>(Observer.Msg<T> message)
        {
            message.Sender = SenderName;

            for (int x = 0; x < _observers.Count; x++)
                _observers[x].OnNotify(message);
        }
        
        /// <summary>
        /// Nofity the current observers with a simple message
        /// </summary>
        /// <param name="message"></param>
        public virtual void NotifyObservers(Observer.Msg message)
        {
            message.Sender = SenderName;

            for (int x = 0; x < _observers.Count; x++)
                _observers[x].OnNotify(message);
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