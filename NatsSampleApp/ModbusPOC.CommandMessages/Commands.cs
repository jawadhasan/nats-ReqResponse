using System;

namespace ModbusPOC.CommandMessages
{
    public enum Operation
    {
        None,
        OperationStart,
        OperationStop,
        DeflectionVoltageControlOn,
        DeflectionVoltageControlOff,
        FaultClear,
        CommandOnline,
        CommandOffline
    }

    //Commands
    public interface ICommand
    {
    }
    public class PrinterCommand : ICommand
    {
        public string IpAddress { get; set; }
        public string Port { get; set; }
        public string PrinterName { get; set; }
        public string CommandType { get; set; }
        public string CommandId { get; set; } = Guid.NewGuid().ToString("N");
        public string Source { get; set; } = "API";
        public object Data { get; set; }

        public PrinterCommand()
        {

        }
        public PrinterCommand(string ipAddress, string port, string printerName)
        {
            this.IpAddress = ipAddress;
            this.Port = port;
            this.PrinterName = printerName;
        }
    }
    public class SetOnLineCommand : PrinterCommand
    {
        public SetOnLineCommand(string ipAddress, string port, string printerName)
            : base(ipAddress, port, printerName)
        {
            this.CommandType = Operation.CommandOnline.ToString();
        }
    }
    public class SetOffLineCommand : PrinterCommand
    {
        public SetOffLineCommand(string ipAddress, string port, string printerName)
            : base(ipAddress, port, printerName)
        {
            this.CommandType = Operation.CommandOffline.ToString();
        }
    }
    public class OperationStartCommand : PrinterCommand
    {
        public OperationStartCommand(string ipAddress, string port, string printerName)
              : base(ipAddress, port, printerName)
        {
            this.CommandType = Operation.OperationStart.ToString();
        }
    }

    public class EmptyCommand : PrinterCommand
    {
        public EmptyCommand(string ipAddress, string port, string printerName)
              : base(ipAddress, port, printerName)
        {
            this.CommandType = Operation.None.ToString();
        }
    }


    //Command Handler
    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        PrinterOperationResult Handle(TCommand command);
    }


    public class PrinterOperationResult
    {
        public PrinterOperationResult()
        {
            IsSuccess = true;
        }
        public string CommandType { get; set; }
        public object Data { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorDescription { get; set; }
        public long ExecutionTime { get; set; }
    }

    public static class CommandBuilder
    {
        public static ICommand BuildCommand(PrinterCommand command)
        {
            switch (command.CommandType)
            {
                case "CommandOnline":
                    return new SetOnLineCommand(command.IpAddress, command.Port, command.PrinterName);

                case "CommandOffline":
                    return new SetOffLineCommand(command.IpAddress, command.Port, command.PrinterName);

                default:
                    return new EmptyCommand(string.Empty, string.Empty, string.Empty);
            }
        }
    }
}
