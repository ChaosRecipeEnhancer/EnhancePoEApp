﻿using ChaosRecipeEnhancer.UI.Models.UserSettings;
using ChaosRecipeEnhancer.UI.Services;
using ChaosRecipeEnhancer.UI.Services.FilterManipulation;
using ChaosRecipeEnhancer.UI.UserControls.SettingsForms.AccountForms;
using ChaosRecipeEnhancer.UI.UserControls.SettingsForms.GeneralForms;
using ChaosRecipeEnhancer.UI.UserControls.SettingsForms.OtherForms;
using ChaosRecipeEnhancer.UI.UserControls.SettingsForms.OverlayForms;
using ChaosRecipeEnhancer.UI.Windows;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Serilog;
using System;

namespace ChaosRecipeEnhancer.UI.Configuration;

public static class ServiceConfiguration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        ConfigureCoreServices(services);
        ConfigureHttpClients(services);
        ConfigureViewModels(services);

        services.AddSingleton<IAuthStateManager, AuthStateManager>();
        services.AddSingleton<IPoEApiService, PoEApiService>();
    }

    private static void ConfigureCoreServices(IServiceCollection services)
    {
        services.AddSingleton<IUserSettings, UserSettings>();
        services.AddSingleton<IReloadFilterService, ReloadFilterService>();
        services.AddSingleton<IFilterManipulationService, FilterManipulationService>();
        services.AddSingleton<INotificationSoundService, NotificationSoundService>();
    }

    private static void ConfigureHttpClients(IServiceCollection services)
    {
        services.AddHttpClient<IAuthStateManager, AuthStateManager>();
        services.AddHttpClient("PoEApiClient")
            // Standard retry policy for transient errors
            .AddTransientHttpErrorPolicy(builder =>
                builder.WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, retryCount, context) =>
                    {
                        // Log the retry attempts
                        Log.Information(
                            "Retrying request {RequestUri} - {ExceptionMessage} - {RetryCount}",
                            context,
                            exception.Exception.Message,
                            retryCount
                        );
                    }
                )
            );
    }

    private static void ConfigureViewModels(IServiceCollection services)
    {
        services.AddTransient<GeneralFormViewModel>();
        services.AddTransient<SetTrackerOverlayFormViewModel>();
        services.AddTransient<StashTabOverlayViewModel>();
        services.AddTransient<PathOfExileAccountOAuthFormViewModel>();
        services.AddTransient<RecipesFormViewModel>();
        services.AddTransient<AdvancedFormViewModel>();
        services.AddTransient<SystemFormViewModel>();
    }
}