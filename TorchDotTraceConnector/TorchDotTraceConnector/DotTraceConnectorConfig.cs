using System.Xml.Serialization;
using Torch;
using Torch.Views;
using TorchDotTraceConnector.Core;
using Utils.Torch;

namespace TorchDotTraceConnector
{
    public sealed class DotTraceConnectorConfig : ViewModel,
        DotTraceConnector.IConfig,
        FileLoggingConfigurator.IConfig
    {
        const string AutoTraceGroupName = "Auto Tracing";
        const string ConnectorGroupName = "Connector";
        const string LogGroupName = "Logs";

        // auto trace
        bool _autoTrace;
        int _initialWaitSecs = 120;
        double _simThreshold = 0.9d;

        // connector
        string _exePath = @"C:\Program Files\JetBrains.dotTrace.CommandLineTools\ConsoleProfiler.exe";
        string _saveFilePathFormat = "Logs/{timestamp}.dtp";
        int _traceSecs = 30;

        //logs
        bool _suppressWpfOutput;
        bool _enableLoggingTrace;
        bool _enableLoggingDebug;
        string _logFilePath = "Logs/DotTraceConnector-${shortdate}.log";
        ProfilingType _profilingType;

        // need restart
        [XmlElement(nameof(AutoTrace))]
        [Display(Name = "Enable auto tracing (NEED RESTART)", GroupName = AutoTraceGroupName, Order = 3)]
        public bool AutoTrace
        {
            get => _autoTrace;
            set => SetValue(ref _autoTrace, value);
        }

        [XmlElement(nameof(InitialWaitSecs))]
        [Display(Name = "Initial seconds to wait", GroupName = AutoTraceGroupName, Order = 4)]
        public int InitialWaitSecs
        {
            get => _initialWaitSecs;
            set => SetValue(ref _initialWaitSecs, value);
        }

        [XmlElement(nameof(SimThreshold))]
        [Display(Name = "Min simspeed to auto-trace", GroupName = AutoTraceGroupName, Order = 5)]
        public double SimThreshold
        {
            get => _simThreshold;
            set => SetValue(ref _simThreshold, value);
        }

        [XmlElement(nameof(ExePath))]
        [Display(Name = "Profiler exe path", GroupName = ConnectorGroupName, Order = 2)]
        public string ExePath
        {
            get => _exePath;
            set => SetValue(ref _exePath, value);
        }

        [XmlElement(nameof(SaveFilePathFormat))]
        [Display(Name = "Output file path format (--save-to)", GroupName = ConnectorGroupName, Order = 3)]
        public string SaveFilePathFormat
        {
            get => _saveFilePathFormat;
            set => SetValue(ref _saveFilePathFormat, value);
        }

        [XmlElement(nameof(TraceSecs))]
        [Display(Name = "Trace timeout seconds (--timeout)", GroupName = ConnectorGroupName, Order = 4)]
        public int TraceSecs
        {
            get => _traceSecs;
            set => SetValue(ref _traceSecs, value);
        }

        [XmlElement(nameof(ProfilingType))]
        [Display(Name = "Profiling type (--profiling-type)", GroupName = ConnectorGroupName, Order = 5)]
        public ProfilingType ProfilingType
        {
            get => _profilingType;
            set => SetValue(ref _profilingType, value);
        }

        [XmlElement(nameof(SuppressWpfOutput))]
        [Display(Name = "Suppress console output", Order = 2, GroupName = LogGroupName)]
        public bool SuppressWpfOutput
        {
            get => _suppressWpfOutput;
            set => SetValue(ref _suppressWpfOutput, value);
        }

        [XmlElement(nameof(EnableLoggingTrace))]
        [Display(Name = "Enable TRACE logs", Order = 3, GroupName = LogGroupName)]
        public bool EnableLoggingTrace
        {
            get => _enableLoggingTrace;
            set => SetValue(ref _enableLoggingTrace, value);
        }

        [XmlElement(nameof(EnableLoggingDebug))]
        [Display(Name = "Enable DEBUG logs", Order = 4, GroupName = LogGroupName)]
        public bool EnableLoggingDebug
        {
            get => _enableLoggingDebug;
            set => SetValue(ref _enableLoggingDebug, value);
        }

        [XmlElement(nameof(LogFilePath))]
        [Display(Name = "Log file path", Order = 5, GroupName = LogGroupName)]
        public string LogFilePath
        {
            get => _logFilePath;
            set => SetValue(ref _logFilePath, value);
        }
    }
}