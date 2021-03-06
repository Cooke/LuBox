﻿using System;
using System.Collections.Generic;
using LuBox.InternalTypes;

namespace LuBox.Library
{
    public static class LuMathLibrary
    {
        public static LuTable Create()
        {
            var random = new Random();
            var dictionary = new Dictionary<object, object>
            {
                {
                    "random", new DelegateWrapper(new Delegate[]
                    {
                        new Func<double>(random.NextDouble),
                        new Func<int, int>(random.Next),
                        new Func<int, int, int>((l, u) => random.Next(l, u + 1)),
                    })
                }
            };
            return new LuTable(dictionary);
        }
    }
}