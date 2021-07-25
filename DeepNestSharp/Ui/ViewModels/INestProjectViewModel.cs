﻿namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib.NestProject;

  public interface INestProjectViewModel
  {
    IProjectInfo ProjectInfo { get; }

    IDetailLoadInfo SelectedDetailLoadInfo { get; set; }

    int SelectedDetailLoadInfoIndex { get; set; }
  }
}