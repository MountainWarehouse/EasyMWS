﻿using System;
using System.IO;

namespace MountainWarehouse.EasyMWS.CallbackLogic
{
    internal interface ICallbackActivator
    {
	    Callback SerializeCallback(Action<Stream, object> callbackMethod, object callbackData);
	    void CallMethod(Callback callback, Stream stream);
    }
}
