namespace DeepNestLib
{
  using System;

  internal class DateService : IDateService
  {
    public DateTime Now => DateTime.Now;
  }
}