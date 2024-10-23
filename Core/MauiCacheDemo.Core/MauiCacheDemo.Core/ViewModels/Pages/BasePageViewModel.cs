// Adapted from https://github.com/reactiveui/ReactiveUI.Samples/blob/main/Xamarin/Cinephile/Cinephile.ViewModels/ViewModelBase.cs
// Copyright (c) 2019 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using MauiCacheDemo.Core.Reporting;
using ReactiveUI;
using Splat;
using System.Reactive;
using System.Reactive.Concurrency;
using MauiCacheDemo.Core.Data;
using MauiCacheDemo.Core.ViewModels.Controls;

namespace MauiCacheDemo.Core.ViewModels.Pages;

/// <summary>
/// A base for all the different view models used throughout the application.
/// </summary>
public abstract class BasePageViewModel : ReactiveObject, IRoutableViewModel,
    IActivatableViewModel
{
    #region Constructors

    // TODO Inject App ReportingService, DataService, NavigationService for testing
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
    /// </summary>
    /// <param name="title">The title of the view model for routing purposes.</param>
    /// <param name="mainThreadScheduler">The scheduler to use to schedule operations on the main thread.</param>
    /// <param name="taskPoolScheduler">The scheduler to use to schedule operations on the task pool.</param>
    /// <param name="hostScreen">The screen used for routing purposes.</param>
    protected BasePageViewModel(string title,
        IScheduler? mainThreadScheduler = null,
        IScheduler? taskPoolScheduler = null,
        IScreen? hostScreen = null)
    {
        try
        {
            //
            // Generic RxUI VM Settings (from RxUI sample)
            //

            UrlPathSegment = title;
            // TODO Handle null
            HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>();

            // Set the schedulers like this so we can inject the test scheduler later on when doing VM unit tests
            MainThreadScheduler = mainThreadScheduler ?? RxApp.MainThreadScheduler;
            TaskPoolScheduler = taskPoolScheduler ?? RxApp.TaskpoolScheduler;

            ShowAlert = new Interaction<AlertViewModel, Unit>(MainThreadScheduler);
            OpenBrowser = new Interaction<string, Unit>(MainThreadScheduler);

            //
            // App Settings
            //

            ReportingService =
                Locator.Current.GetService<IReportingService>();

            //PrintPageViewModelHeader(GetType().Name, "CONSTRUCTOR");

            PetstoreDataService =
                PetstoreDataService
                ?? Locator.Current.GetService<IPetstoreDataService>();

            PlatformDataService =
                PlatformDataService
                ?? Locator.Current.GetService<IPlatformDataService>();

            //NavigationService =
            //    Locator.Current.GetService<NavigationService>();

            //// TODO Convert into a service for Splat locator
            //_resourceManager =
            //    new ResourceManager("MauiCacheDemo.Core.Resources.Text",
            //        Assembly.GetExecutingAssembly());
        }
        catch (Exception exception)
        {
            // Use static Splat logger in case ReportingService is not initialized
            if (ReportingService is null)
                LogHost.Default.Error(exception,
                    $"EXCEPTION:  {exception.Message}");
            else
                ReportingService.ReportException(exception);
        }
    }

    #endregion

    #region Properties - App Settings


    // TODO Inject PetstoreDataService instead of static
    protected static IPetstoreDataService
        PetstoreDataService { get; private set; }

    // TODO Inject PlatformDataService instead of static
    protected static IPlatformDataService
        PlatformDataService { get; private set; }

    //protected NavigationService NavigationService { get; }

    protected IReportingService ReportingService { get; }

    #endregion

    #region Properties - Generic RxUI VM Settings

    /// <summary>
    /// Gets the current page path.
    /// </summary>
    public string UrlPathSegment { get; }

    /// <summary>
    /// Gets the screen used for routing operations.
    /// </summary>
    public IScreen HostScreen { get; }

    /// <summary>
    /// Gets the activator which contains context information for use in activation of the view model.
    /// </summary>
    public ViewModelActivator Activator { get; } = new ViewModelActivator();

    /// <summary>
    /// Gets a interaction which will show an alert.
    /// </summary>
    public Interaction<AlertViewModel, Unit> ShowAlert { get; }

    /// <summary>
    /// Gets an interaction which will open a browser window.
    /// </summary>
    public Interaction<string, Unit> OpenBrowser { get; }

    /// <summary>
    /// Gets the scheduler for scheduling operations on the main thread.
    /// </summary>
    protected IScheduler MainThreadScheduler { get; }

    /// <summary>
    /// Gets the scheduler for scheduling operations on the task pool.
    /// </summary>
    protected IScheduler TaskPoolScheduler { get; }

    #endregion
}
