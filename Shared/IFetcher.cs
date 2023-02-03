﻿/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public interface IFetcher<T>
    {
        T Find(int id);

        int GetId(T obj);
    }
}
