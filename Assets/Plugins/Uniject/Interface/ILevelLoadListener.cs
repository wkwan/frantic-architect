using System;

namespace Uniject
{
    public interface ILevelLoadListener
    {
        void registerListener(Action action);
    }
}
