using Common;
using Microsoft.UI.Xaml;
using ScottPlot.WinUI;

namespace Hinagiku;
public sealed partial class MainWindow : Window
{
    SerialConsole serialConsole = new();

    public static WinUIPlot winUIPlot;
    public static DataStreamer dataStreamer;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        Status.statusBar = StatusBar;
        Status.dispatcherQueue = DispatcherQueue;

        RefreshSerialPort();
        SetBaudRates();

        winUIPlot = WinUIPlot1;

        dataStreamer = new();
    }

    void RefreshSerialPort()
    {
        SelectSerialPort.Items.Clear();
        foreach(string portName in SerialConsole.GetSerialPort())
            SelectSerialPort.Items.Add(portName);
    }

    void SetBaudRates()
    {
        SelectBaudRate.Items.Clear();
        foreach (string baudRate in SerialConsole.GetBaudRates())
            SelectBaudRate.Items.Add(baudRate);
        SelectBaudRate.SelectedIndex = 6;
    }

    void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshSerialPort();
    }

    void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        string portName = SelectSerialPort.SelectedItem as string;
        serialConsole.portName = portName;
        serialConsole.baudRate = int.Parse(SelectBaudRate.SelectedItem as string);
        if(serialConsole.OpenSerialPort()){
            ConnectButton.Visibility = Visibility.Collapsed;
            DisconnectButton.Visibility = Visibility.Visible;
        }
    }

    void DisconnectButton_Click(object sender, RoutedEventArgs e)
    {
        if(serialConsole.CloseSerialPort()){
            ConnectButton.Visibility = Visibility.Visible;
            DisconnectButton.Visibility = Visibility.Collapsed;
        };
        winUIPlot.Plot.Axes.SetLimits(null, null, -1, 1);
    }
}
