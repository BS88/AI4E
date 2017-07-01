﻿using System;

namespace AI4E.Modularity.Integration
{
    public sealed class UnregisterQueryForwarding
    {
        public UnregisterQueryForwarding(Type queryType, Type resultType)
        {
            QueryType = queryType;
            ResultType = resultType;
        }

        public Type QueryType { get; }
        public Type ResultType { get; }
    }
}