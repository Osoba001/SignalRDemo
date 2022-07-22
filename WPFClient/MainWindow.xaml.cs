using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using System.Windows;

namespace WPFClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HubConnection hubConnection;
        public MainWindow()
        {
            InitializeComponent();
            hubConnection = new HubConnectionBuilder()
                .WithUrl(url: "https://localhost:7018/chathub")
                .WithAutomaticReconnect()
                .Build();

            hubConnection.Reconnecting += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    string newMesg = "Attempting to reconnect...";
                    messages.Items.Add(newMesg);
                });
                return Task.CompletedTask;
            };

            hubConnection.Reconnected += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    string newMesg = "Reconnected to server";
                    messages.Items.Clear();
                    messages.Items.Add(newMesg);
                });
                return Task.CompletedTask;
            };
            hubConnection.Closed += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    string newMesg = "Connection Closed";
                    messages.Items.Add(newMesg);
                    oprnConnection.IsEnabled = true;
                    sendMessage.IsEnabled = false;
                });
                return Task.CompletedTask;
            };
        }

        private async void oprnConnection_Click(object sender, RoutedEventArgs e)
        {
            hubConnection.On<string, string>(methodName: "ReceiveMessage", (user, message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var formattedmessage = $"{user}: {message}";
                    messages.Items.Add(formattedmessage);
                });
            });

            try
            {
                await hubConnection.StartAsync();
                messages.Items.Add("Connection Started");
                oprnConnection.IsEnabled=false;
                sendMessage.IsEnabled=true;
            }
            catch (System.Exception ex)
            {

                messages.Items.Add(ex.Message);
            }
        }

        private async void sendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await hubConnection.InvokeAsync(methodName: "SendMessage", "WPF Client", messageInput.Text);
                messageInput.Clear();
            }
            catch (System.Exception ex)
            {

                messages.Items.Add(ex.Message);
            }
        }
    }
}
