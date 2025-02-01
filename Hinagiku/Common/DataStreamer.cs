using System;
using Common;
using Microsoft.UI.Xaml;
using ScottPlot;
using ScottPlot.Plottables;

namespace Hinagiku;

public partial class DataStreamer
{
    DispatcherTimer UpdatePlotTimer = new();
    ScottPlot.Plottables.DataStreamer Streamer;
    ScottPlot.WinUI.WinUIPlot plot = MainWindow.winUIPlot;

    public DataStreamer()
    {
        UpdatePlotTimer.Tick += UpdatePlotTimer_Tick;
        UpdatePlotTimer.Interval = TimeSpan.FromMilliseconds(5);
        UpdatePlotTimer.Start();

        Streamer = plot.Plot.Add.DataStreamer(500);
        Streamer.Color = Color.FromARGB(0xFFBF174F);
        plot.UserInputProcessor.Disable();
        Streamer.ViewScrollLeft();

        plot.Plot.Axes.SetLimits(0, 500, -1, 1);

        ScottPlot.TickGenerators.NumericAutomatic myTickGenerator = new()
        {
            LabelFormatter = CustomFormatter
        };
        plot.Plot.Axes.Bottom.TickGenerator = myTickGenerator;

        // plot.Plot.Axes.ContinuouslyAutoscale = true;
        // Streamer.ManageAxisLimits = true;
    }

    static string CustomFormatter(double position)
    {
        if (position == 0)
            return "0";
        else if (position > 0)
            return $"+{position / 100.0}";
        else
            return $"({-position/ 100.0})";
    }

    public void AddValue(double value)
    {
        Streamer.Add(value);
    }

    void UpdatePlotTimer_Tick(object sender, object e)
    {
        if (Streamer.HasNewData)
        {
            Status.AddMessage($"Processed {Streamer.Data.CountTotal:N0} points");
            plot.Refresh(); 
        }
    }
}