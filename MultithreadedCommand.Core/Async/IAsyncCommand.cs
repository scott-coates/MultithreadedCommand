﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultithreadedCommand.Core.Commands;

namespace MultithreadedCommand.Core.Async
{
    public interface IAsyncCommand : ICommand
    {
        ICommand DecoratedCommand { get; }
        event Action AfterSetActionInactive;
    }
}