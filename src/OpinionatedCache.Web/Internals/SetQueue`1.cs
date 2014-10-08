﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System;
using System.Collections.Generic;

namespace OpinionatedCache.Web
{
    public class SetQueue<T>
    {
        private readonly Dictionary<T, bool> duplicates;
        private readonly Queue<T> queue = new Queue<T>();

        public SetQueue()
        {
            duplicates = new Dictionary<T, bool>();
        }

        public SetQueue(IEqualityComparer<T> comparer)
        {
            duplicates = new Dictionary<T, bool>(comparer);
        }

        public int Count
        {
            get
            {
                return duplicates.Count;
            }
        }

        public bool Enqueue(T item)
        {
            if (!duplicates.ContainsKey(item))
            {
                duplicates[item] = true;
                queue.Enqueue(item);
                return true;
            }

            return false;
        }

        public T Dequeue()
        {
            if (queue.Count > 0)
            {
                var item = queue.Dequeue();

                if (!duplicates.ContainsKey(item))
                    throw new InvalidOperationException("The dictionary should contain an item");
                else
                    duplicates.Remove(item);

                return item;
            }

            throw new InvalidOperationException("Can't dequeue on an empty queue.");
        }
    }
}

