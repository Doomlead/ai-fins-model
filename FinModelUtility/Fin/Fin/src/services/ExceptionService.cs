﻿using System;

using fin.io.web;

namespace fin.services;

public static class ExceptionService {
  public static void HandleException(Exception e, IExceptionContext? c)
    => OnException?.Invoke(e, c);

  public static event Action<Exception, IExceptionContext?> OnException;
}