using UnityEngine;
using System.Collections;


namespace TMPro
{

    public struct TMP_XmlTagStack<T>
    {
        public T[] itemStack;
        public int index;

        /// <summary>
        /// Constructor to create a new item stack.
        /// </summary>
        /// <param name="tagStack"></param>
        public TMP_XmlTagStack(T[] tagStack)
        {
            itemStack = tagStack;
            index = 0;
        }


        /// <summary>
        /// Function to clear and reset stack to first item.
        /// </summary>
        public void Clear()
        {
            index = 0;
        }


        /// <summary>
        /// Function to set the first item on the stack and reset index.
        /// </summary>
        /// <param name="item"></param>
        public void SetDefault(T item)
        {
            itemStack[0] = item;
            index = 1;
        }


        /// <summary>
        /// Function to add a new item to the stack.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (index < itemStack.Length)
            {
                itemStack[index] = item;
                index += 1;
            }
        }


        /// <summary>
        /// Function to retrieve an item from the stack.
        /// </summary>
        /// <returns></returns>
        public T Remove()
        {
            index -= 1;

            if (index <= 0)
            {
                index = 0;
                return itemStack[0];
            }

            return itemStack[index - 1];
        }

    }
}
