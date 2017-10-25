using System;
using System.Collections.Generic;
using AI4E.EventResults;

namespace AI4E
{
    public static class EventResultExtension
    {
        public static bool IsTimeout(this IEventResult eventResult)
        {
            return eventResult is TimeoutEventResult;
        }

        public static bool IsTimeout(this IEventResult eventResult, out DateTime dueTime)
        {
            if (eventResult is TimeoutEventResult timoutEventResult)
            {
                dueTime = timoutEventResult.DueTime;
                return true;
            }

            dueTime = default;
            return false;
        }

        public static bool IsAggregateResult(this IEventResult eventResult)
        {
            return eventResult is IAggregateEventResult;
        }

        public static IAggregateEventResult Flatten(this IAggregateEventResult aggregateEventResult)
        {
            var result = new List<IEventResult>();
            AddEventResultsToList(aggregateEventResult, result);

            return new AggregateEventResult(result);
        }

        private static void AddEventResultsToList(IAggregateEventResult aggregateEventResult, List<IEventResult> list)
        {
            foreach (var eventResult in aggregateEventResult.EventResults)
            {
                if (eventResult is IAggregateEventResult innerAggregateEventResult)
                {
                    AddEventResultsToList(innerAggregateEventResult, list);
                }
                else
                {
                    list.Add(eventResult);
                }
            }
        }
    }
}
