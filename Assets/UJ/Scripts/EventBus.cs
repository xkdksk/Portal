using System;
using System.Collections.Generic;



namespace UJ.Data
{

    public class BaseEvent<FromT, ContextT>
    {
        public FromT from;
        public ContextT context;


        public void Set(FromT from, ContextT context)
        {
            this.from = from;
            this.context = context;
        }
    }

    public class SubscribeObj
    {
        List<System.Action> unsubscribes = new List<Action>();

        public void AddUnsub(System.Action a)
        {
            unsubscribes.Add(a);
        }

        public void UnsubAndClear()
        {
            foreach (var a in unsubscribes)
            {
                a();
            }
            unsubscribes.Clear();
        }


    }

    public class EventBus
    {



        static Dictionary<Type, List<Delegate>> handlerDic = new Dictionary<Type, List<Delegate>>();


        public static Action Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);

            if (handlerDic.ContainsKey(type) == false)
            {
                handlerDic.Add(type, new List<Delegate>());

            }

            var handlers = handlerDic[type];

            handlers.Add(handler);

            return () =>
            {
                Unsubscribe<T>(handler);
            };
        }

        const int MaxHandlerCount = 1000;
        const int MaxCallDepth = 10;
        static Delegate[,] tempDelegates = new Delegate[MaxHandlerCount, MaxCallDepth];

        static int callDepth = 0;

        public static void Publish<T>(T e)
        {
            var type = typeof(T);

            if (handlerDic.ContainsKey(type) == false)
                return;

            callDepth++;
            try
            {
                var handlers = handlerDic[type];

                int i = 0;

                foreach (var h in handlers)
                {
                    tempDelegates[i, callDepth - 1] = h;
                    i++;
                }


                while (i > 0)
                {
                    i--;
                    (tempDelegates[i, callDepth - 1] as Action<T>)(e);

                }
            }
            finally
            {
                callDepth--;
            }
        }

        internal static void Unsubscribe<T>(Action<T> addChatLines)
        {

            var type = typeof(T);

            if (handlerDic.ContainsKey(type) == false)
                return;

            var handlers = handlerDic[type];
            handlers.Remove(addChatLines);
        }


    }
}