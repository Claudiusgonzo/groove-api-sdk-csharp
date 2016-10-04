﻿// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------
namespace Microsoft.Groove.Api.Samples
{
    using System;
    using System.Diagnostics;
    using Windows.UI.ApplicationSettings;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using Client;
    using DataContract;
    using ViewModels;

    // TODO: Add a deeplink button to the Groove app and update documentation
    // TODO: Add in-app streaming

    public sealed partial class MainPage : Page
    {
        // Provide your own values here
        // See https://github.com/Microsoft/Groove-API-documentation/blob/master/Main/Using%20the%20Groove%20RESTful%20Services/Obtaining%20a%20Developer%20Access%20Token.md
        private const string AzureDataMarketClientId = "xmva-e2e-ieb-test-1";
        private const string AzureDataMarketClientSecret = "TaQvTe0a9t/jZfyLQbGXcEWsZ2tlK/ZXzu0CJUhenkc=";

        private readonly UserAccountManagerWithNotifications _userAccountManager;
        private readonly IGrooveClient _grooveClient;

        public string SearchQuery { get; set; }
        public MusicContentPaneViewModel MusicContentPaneViewModel { get; set; }

        public MainPage()
        {
            InitializeComponent();

            _userAccountManager = new UserAccountManagerWithNotifications();
            _grooveClient = GrooveClientFactory.CreateGrooveClient(AzureDataMarketClientId, AzureDataMarketClientSecret, _userAccountManager);

            MusicContentPaneViewModel = new MusicContentPaneViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += _userAccountManager.BuildAccountPaneAsync;
            await _userAccountManager.SignInUserAccountSilentlyAsync();
            if (_userAccountManager.UserIsSignedIn)
            {
                Debug.WriteLine("Successful silent sign-in");
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested -= _userAccountManager.BuildAccountPaneAsync;
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;

            if (_userAccountManager.UserIsSignedIn)
            {
                await _userAccountManager.SignOutAccountAsync();
            }
            else
            {
                AccountsSettingsPane.Show();
            }
            
            ((Button)sender).IsEnabled = true;
        }

        private async void PlaylistsButton_OnClick(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;

            if (_userAccountManager.UserIsSignedIn)
            {
                ContentResponse playlists = await _grooveClient.BrowseAsync(
                    MediaNamespace.music,
                    ContentSource.Collection,
                    ItemType.Playlists);

                HandleGrooveApiError(playlists.Error);
                MusicContentPaneViewModel.DisplayMusicContent(playlists);
            }
            else
            {
                AccountsSettingsPane.Show();
            }

            ((Button)sender).IsEnabled = true;
        }

        private async void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;

            ContentResponse searchResponse = await _grooveClient.SearchAsync(
                MediaNamespace.music,
                SearchQuery,
                ContentSource.Catalog,
                maxItems: 10);

            HandleGrooveApiError(searchResponse.Error);
            MusicContentPaneViewModel.DisplayMusicContent(searchResponse);

            ((Button)sender).IsEnabled = true;
        }

        private void HandleGrooveApiError(Error error)
        {
            if (error == null)
            {
                Debug.WriteLine("Successful Groove API call");
            }
            else
            {
                // TODO: Add a pop-up window detailing the error
                Debug.WriteLine($"Groove API error: {error.ErrorCode}");
            }
        }
    }
}
