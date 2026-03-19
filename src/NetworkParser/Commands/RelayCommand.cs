using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkParser.UI.Commands;

internal class RelayCommand : ICommand {
    public event EventHandler? CanExecuteChanged;

    private readonly Action execute;

    public RelayCommand (Action execute) {
        this.execute = execute;
    }

    public bool CanExecute (object parameter) => true;

    public void Execute (object parameter) => execute();
}
