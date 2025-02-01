using System;
using System.IO.Ports;
using Hinagiku;
using System.Management;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common;

public class SerialConsole
{
    public string portName = "";
    public int baudRate = 0;
    SerialPort serialPort;
    bool connected = false;

    public static string ExtractCOM(string input)
    {
        // 正規表現パターン
        string pattern = @"^COM\d+";
        // 正規表現によるマッチを検索
        Match match = Regex.Match(input, pattern);
        if (match.Success)
            return match.Value; // マッチした値を返す
        return ""; // マッチしなかった場合
    }

    static string GetSerialPortDescription(string portName)
    {
        // WMIでシリアルポートの情報を取得
        string query = $"SELECT * FROM Win32_SerialPort WHERE DeviceID = '{portName}'";
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
        // ポートの説明を返す
        foreach (ManagementObject port in searcher.Get())
            return port["Description"]?.ToString();
        // 説明が見つからなかった場合はnullを返す
        return "";
    }

    public static string[] GetSerialPort()
    {
        // シリアルポートの一覧を取得
        string[] ports = SerialPort.GetPortNames();
        List<string> portDescriptions = new();

        foreach (string portName in ports)
        {
            try{
                string serialPortDescription = GetSerialPortDescription(portName);
                // ポート名と説明を組み合わせてリストに追加
                portDescriptions.Add($"{portName} ({serialPortDescription})");
            }catch(Exception ex){
                Status.AddMessage(ex.Message);
            }
        }

        return portDescriptions.ToArray(); // List<string> を配列に変換して返す
    }

    public static string[] GetBaudRates()
    {
        return new string[]{
            "300", "600", "750", "1200", "2400", "4800", "9600", "19200", "31250", "38400", "57600", "74880", "115200", "230400", "250000", "460800", "500000", "921600", "1000000", "2000000"
        };
    }

    static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    {
        SerialPort sp = (SerialPort)sender;
        try
        {
            // 受信データを読み込む
            string data = sp.ReadLine();

            // 改行コードを削除
            data = data.Replace("\r", "").Replace("\n", "");

            Status.AddMessage("Received: " + data);

            double y = ((int.Parse(data) / 1023.0) - 0.5) * 2;
            if(y < 1 && y >= -1)
                MainWindow.dataStreamer.AddValue(y);
        }
        catch (Exception ex)
        {
            // Status.AddMessage("Error reading from serial port: " + ex.Message);
            Task.Run(() => {_ = Dialog.ShowError(MainWindow.mainWindow.Content, ex.Message);});
        }
    }

    public SerialPort InitializeSerialPort()
    {
        serialPort = new()
        {
            PortName = ExtractCOM(portName),
            BaudRate = baudRate,
            Parity = Parity.None,
            DataBits = 8,
            StopBits = StopBits.One,
            Handshake = Handshake.None
        };

        // データ受信時のイベントハンドラを設定
        serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        return serialPort;
    }

    public async Task<bool> OpenSerialPortAsync()
    {
        if(portName == null){
            _ = await Dialog.ShowError(MainWindow.mainWindow.Content, MainWindow.mainWindow.NoSelectedSerialPort.Text);
            return false;
        }

        try{
            InitializeSerialPort();
        }catch (Exception ex){
            _ = await Dialog.ShowError(MainWindow.mainWindow.Content, ex.Message);
            Status.AddMessage("Error initializing serial port: " + ex.Message);
            return false;
        }

        try
        {
            serialPort.Open();
            Status.AddMessage("Listening to Arduino...");
        }
        catch (Exception ex)
        {
            Status.AddMessage("Error opening serial port: " + ex.Message);

            return false;
        }
        connected = true;
        return true;
    }

    public bool CloseSerialPort()
    {
        if(!connected)
        {
            Status.AddMessage("The serial port is not open.");
            return false;
        }

        if (serialPort.IsOpen)
        {
            serialPort.Close();
            Status.AddMessage("Serial port closed.");
        } else {
            return false;
        }
        return true;
    }
}