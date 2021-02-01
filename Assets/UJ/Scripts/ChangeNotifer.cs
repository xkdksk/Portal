using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace UJ.Data
{

    public class ChangeNotifier
    {
        Dictionary<string, List<Delegate>> subscriber = new Dictionary<string, List<Delegate>>();


        public enum ChangeKind
        {
            Change,
            Add,
            Remove,
            Clear,
            ReplaceAll
        }

        public struct Payload<T>
        {
            public T value;
            public int index;

            public ChangeKind changeKind;
        }

        public void Publish<T>(string str, T value)
        {


            if (subscriber.ContainsKey(str) == false)
                return;

            foreach (var d in subscriber[str].Select
                (item => item as Action<T>))
            {
                d.Invoke(value);
            }
        }





        public Action Subscribe<T>(string str, System.Action<T> action)
        {

            if (subscriber.ContainsKey(str) == false)
            {
                subscriber.Add(str, new List<Delegate>());
            }
            var delegates = subscriber[str];

            if (delegates.Contains(action) == false)
            {
                delegates.Add(action);
            }

            return () =>
            {
                Unsubscribe<T>(str, action);
            };

        }




        public void Unsubscribe<T>(string str, System.Action<T> action)
        {

            if (subscriber.ContainsKey(str) == false)
            {
                return;
            }

            var delegates = subscriber[str];

            if (delegates.Contains(action) == false)
            {
                return;
            }

            delegates.Remove(action);

        }




    }
}