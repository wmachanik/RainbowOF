
using Blazorise.Snackbar;
using Microsoft.AspNetCore.Components;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Components.Modals
{
    public partial class PopUpAndLogNotification : ComponentBase
    {
        #region Internal classes and vars
        public enum NotificationType
        {
            Info,
            Success,
            Warning,
            Error,
            Debug
        }
        #endregion
        #region Parameters
        [Parameter]
        [EditorRequired]
        public string NotificationTitle { get; set; } = "Title";
        [Parameter]
        public string NotificationMessage { get; set; } = "Message";
        [Parameter]
        public SnackbarStackLocation NotificationPosition { get; set; } = SnackbarStackLocation.End;
        [Parameter]
        public int NotificationTimeout { get; set; } = 10000;
        #endregion
        #region Injections
        [Inject]
        public ILoggerManager NotificationLogger { get; set; }
        //[Inject]
        //public IToastService NotificationToastService { get; set; }
        #endregion
        #region Public vars
        public SnackbarStack popupSnackBar { get; set; }
        public SnackbarColor popupColour { get; set; } = SnackbarColor.Primary;
        //public PopUpAndLogNotification PopUpRef;
        #endregion
        #region Exposed Routines
        public async Task ShowNotificationAsync(NotificationType pNotificationType)
        {
            switch (pNotificationType)
            {
                case NotificationType.Info:
                    await popupSnackBar.PushAsync(NotificationMessage, SnackbarColor.Info);
//                    NotificationToastService.ShowInfo(NotificationMessage, NotificationTitle);
                    NotificationLogger.LogInfo( $"Info: {NotificationMessage}");
                    break;
                case NotificationType.Success:
                    await popupSnackBar.PushAsync(NotificationMessage, SnackbarColor.Success);
                    NotificationLogger.LogInfo($"Success: {NotificationMessage}");
                    break;
                case NotificationType.Warning:
                    await popupSnackBar.PushAsync(NotificationMessage, SnackbarColor.Warning);
                    NotificationLogger.LogWarn($"Warning: {NotificationMessage}");
                    break;
                case NotificationType.Error:
                    await popupSnackBar.PushAsync(NotificationMessage, SnackbarColor.Danger);
                    NotificationLogger.LogError($"Error: {NotificationMessage}");
                    break;
                default:     // assume this is debug
                    await popupSnackBar.PushAsync(NotificationMessage, SnackbarColor.Primary);
                    NotificationLogger.LogDebug($"Debug: {NotificationMessage}");
                    break;
            }
        }
        public async Task ShowNotificationAsync(NotificationType pNotificationType, string pMessage)
        {
            NotificationMessage = pMessage;
            NotificationTitle = "";
            await ShowNotificationAsync(pNotificationType);
        }
        public async Task ShowNotificationAsync(NotificationType pNotificationType, string pMessage, string pTtitle)
        {
            NotificationMessage = pMessage;
            NotificationTitle = pTtitle;
            await ShowNotificationAsync(pNotificationType);
        }
        public async Task ShowQuickNotificationAsync(NotificationType pNotificationType, string pMessage)
        {
            NotificationTimeout = 5000;  // not sure were to send this
            NotificationMessage = pMessage;
            NotificationTitle = "";
            await ShowNotificationAsync(pNotificationType);
        }
        #endregion
    }
}
