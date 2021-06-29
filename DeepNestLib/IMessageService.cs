namespace DeepNestLib
{
  using System;

  public interface IMessageService
  {
    void DisplayMessage(string message);

    void DisplayMessage(Exception ex);
  }
}
